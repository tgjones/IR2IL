#!/usr/bin/env python3
"""Generates tests/hand-written/vector_mask_flow.ll.

A `<N x i1>` mask is made by one instruction and read by another, possibly in a different block, possibly after
passing through a phi, a call or memory. This crosses every way of making a mask (PRODUCERS) with every way of
reading one (CONSUMERS), for N = 2, 4 and 8 lanes and two input patterns each, so that any change to how masks are
represented in IL (their lane width, whether a comparison is fused into its consumer, ...) is checked against
every combination, not only the ones that clang happens to produce.

A producer returns the lines that define `%m` and the lane values it must hold. A consumer returns the lines that
read `%m` and define `%ok` (an i1: did it see what LLVM defines?). Each combination is its own function; `main`
prints the name of every failing one and returns 1 if there was any, 0 if none.

A combination that doesn't work yet goes in KNOWN_GAPS (currently none) with the reason, and is left out. Run
with `--include-gaps` to put those back, or `--probe DIR` to write one small file per combination (including the
gaps) into DIR, to find out which ones fail on their own.

Usage:
    generate_vector_mask_flow_tests.py                    rewrite tests/hand-written/vector_mask_flow.ll
    generate_vector_mask_flow_tests.py --include-gaps     the same, with the known gaps
    generate_vector_mask_flow_tests.py --probe DIR        one file per producer x consumer x N in DIR
"""

import os
import sys

LANE_COUNTS = [2, 4, 8]
PATTERNS = [0, 1]


def signed(value, bits):
    value &= (1 << bits) - 1
    return value - (1 << bits) if value >= 1 << (bits - 1) else value


# ---------------------------------------------------------------------------------------------------------------
# Producers. Each is a function (lanes, pattern) -> (lines, bits) or None when the combination doesn't exist;
# the mask is %m, and `bits` holds the lane values it must have.
# ---------------------------------------------------------------------------------------------------------------

def boundary_values(bits):
    return [0, 1, 2, (1 << (bits - 1)) - 1, 1 << (bits - 1), (1 << bits) - 2, (1 << bits) - 1]


def operands(kind, lanes, pattern, salt=0):
    """Two lists of lane values for a comparison of `kind` lanes, mixing true and false lanes."""
    if kind in ("float", "double"):
        values = [-3, -1, 0, 1, 2, 5, 7]
    else:
        values = boundary_values(int(kind[1:]))
    k = len(values)
    a = [values[(i * 3 + pattern + salt) % k] for i in range(lanes)]
    b = [values[(i * 5 + 2 * pattern + salt + 1) % k] for i in range(lanes)]
    return a, b


def vector_constant(kind, values):
    if kind in ("float", "double"):
        return f"<{len(values)} x {kind}> <" + ", ".join(f"{kind} {float(v)!r}" for v in values) + ">"
    bits = int(kind[1:])
    return f"<{len(values)} x {kind}> <" + ", ".join(f"{kind} {signed(v, bits)}" for v in values) + ">"


def mask_constant(bits):
    """A constant mask as a value, for use after the type: `xor <4 x i1> %m, <i1 true, ...>`."""
    return "<" + ", ".join("i1 true" if b else "i1 false" for b in bits) + ">"


PREDICATES = {
    "eq": lambda a, b, w: a == b,
    "slt": lambda a, b, w: signed(a, w) < signed(b, w),
    "sle": lambda a, b, w: signed(a, w) <= signed(b, w),
    "ugt": lambda a, b, w: a > b,
    "ult": lambda a, b, w: a < b,
    "olt": lambda a, b, w: a < b,
    "ogt": lambda a, b, w: a > b,
}


def comparison(name, kind, predicate, lanes, pattern, salt=0):
    """Lines that define `%name` as a comparison, and its lane values."""
    a, b = operands(kind, lanes, pattern, salt)
    width = 0 if kind in ("float", "double") else int(kind[1:])
    bits = [PREDICATES[predicate](x, y, width) for x, y in zip(a, b)]
    opcode = "fcmp" if kind in ("float", "double") else "icmp"
    line = f"  %{name} = {opcode} {predicate} {vector_constant(kind, a)}, {vector_constant(kind, b)[len(f'<{lanes} x {kind}> '):]}"
    return [line], bits


def comparison_producer(kind, predicate):
    def produce(lanes, pattern):
        lines, bits = comparison("m", kind, predicate, lanes, pattern)
        return lines, bits
    return produce


def binary_producer(opcode):
    operation = {"and": lambda x, y: x and y, "or": lambda x, y: x or y, "xor": lambda x, y: x != y}[opcode]

    def produce(lanes, pattern):
        lines1, bits1 = comparison("m1", "i32", "slt", lanes, pattern)
        lines2, bits2 = comparison("m2", "i64", "ult", lanes, pattern, salt=2)
        return lines1 + lines2 + [f"  %m = {opcode} <{lanes} x i1> %m1, %m2"], [operation(x, y) for x, y in zip(bits1, bits2)]
    return produce


def select_of_masks(lanes, pattern):
    lines1, bits1 = comparison("m1", "i32", "slt", lanes, pattern)
    lines2, bits2 = comparison("m2", "i16", "sle", lanes, pattern, salt=1)
    lines3, bits3 = comparison("c", "float", "olt", lanes, pattern, salt=3)
    bits = [x if c else y for c, x, y in zip(bits3, bits1, bits2)]
    return lines1 + lines2 + lines3 + [f"  %m = select <{lanes} x i1> %c, <{lanes} x i1> %m1, <{lanes} x i1> %m2"], bits


def freeze_mask(lanes, pattern):
    lines, bits = comparison("m1", "i64", "slt", lanes, pattern)
    return lines + [f"  %m = freeze <{lanes} x i1> %m1"], bits


def phi_mask(taken_left):
    def produce(lanes, pattern):
        lines1, bits1 = comparison("m1", "i32", "slt", lanes, pattern)
        lines2, bits2 = comparison("m2", "i64", "ult", lanes, pattern, salt=2)
        lines = lines1 + lines2 + [
            f"  br i1 {'true' if taken_left else 'false'}, label %left, label %right",
            "left:",
            "  br label %merge",
            "right:",
            "  br label %merge",
            "merge:",
            f"  %m = phi <{lanes} x i1> [ %m1, %left ], [ %m2, %right ]",
        ]
        return lines, bits1 if taken_left else bits2
    return produce


def constant_mask(lanes, pattern):
    bits = [((i + pattern) % 3) != 0 for i in range(lanes)]
    return [f"  %m = freeze <{lanes} x i1> {mask_constant(bits)}"], bits


def call_mask(lanes, pattern):
    lines, bits = comparison("m1", "i32", "slt", lanes, pattern)
    return lines + [f"  %m = call <{lanes} x i1> @pass_mask{lanes}(<{lanes} x i1> %m1)"], bits


def memory_mask(lanes, pattern):
    lines, bits = comparison("m1", "i32", "slt", lanes, pattern)
    return lines + [
        f"  %slot = alloca <{lanes} x i1>",
        f"  store <{lanes} x i1> %m1, ptr %slot",
        f"  %m = load <{lanes} x i1>, ptr %slot",
    ], bits


def shuffled_mask(lanes, pattern):
    lines, bits = comparison("m1", "i32", "slt", lanes, pattern)
    mask = ", ".join(f"i32 {lanes - 1 - i}" for i in range(lanes))
    return lines + [f"  %m = shufflevector <{lanes} x i1> %m1, <{lanes} x i1> poison, <{lanes} x i32> <{mask}>"], bits[::-1]


def inserted_mask(value):
    def produce(lanes, pattern):
        lines, bits = comparison("m1", "i32", "slt", lanes, pattern)
        bits = list(bits)
        bits[1] = value
        return lines + [f"  %m = insertelement <{lanes} x i1> %m1, i1 {'true' if value else 'false'}, i64 1"], bits
    return produce


def truncated_mask(lanes, pattern):
    values = [(i * 3 + pattern) % 4 for i in range(lanes)]
    return [f"  %m = trunc {vector_constant('i32', values)} to <{lanes} x i1>"], [bool(v & 1) for v in values]


def bitcast_mask(lanes, pattern):
    bits = [((i * 2 + pattern) % 3) == 0 for i in range(lanes)]
    value = sum(1 << i for i, b in enumerate(bits) if b)
    return [f"  %m = bitcast i{lanes} {signed(value, lanes)} to <{lanes} x i1>"], bits


PRODUCERS = {
    "icmp_i8_ugt": comparison_producer("i8", "ugt"),
    "icmp_i16_sle": comparison_producer("i16", "sle"),
    "icmp_i32_slt": comparison_producer("i32", "slt"),
    "icmp_i64_ult": comparison_producer("i64", "ult"),
    "fcmp_float_olt": comparison_producer("float", "olt"),
    "fcmp_double_ogt": comparison_producer("double", "ogt"),
    "and_of_i32_and_i64": binary_producer("and"),
    "or_of_i32_and_i64": binary_producer("or"),
    "xor_of_i32_and_i64": binary_producer("xor"),
    "select_of_masks": select_of_masks,
    "freeze": freeze_mask,
    "phi_left": phi_mask(True),
    "phi_right": phi_mask(False),
    "constant": constant_mask,
    "call_result": call_mask,
    "load": memory_mask,
    "shufflevector": shuffled_mask,
    "insertelement_true": inserted_mask(True),
    "insertelement_false": inserted_mask(False),
    "trunc": truncated_mask,
    "bitcast_from_integer": bitcast_mask,
}


# ---------------------------------------------------------------------------------------------------------------
# Consumers. Each is a function (lanes, bits) -> lines that read %m and define %ok, or None.
# ---------------------------------------------------------------------------------------------------------------

def mask_integer(bits):
    return sum(1 << i for i, b in enumerate(bits) if b)


def check_vector_equals(kind, lanes, expected):
    """Lines that define %ok as: the vector %r equals `expected` in every lane."""
    compare = "fcmp oeq" if kind in ("float", "double") else "icmp eq"
    return [
        f"  %eq = {compare} <{lanes} x {kind}> %r, {vector_constant(kind, expected)[len(f'<{lanes} x {kind}> '):]}",
        f"  %eqbits = bitcast <{lanes} x i1> %eq to i{lanes}",
        f"  %ok = icmp eq i{lanes} %eqbits, -1",
    ]


def bitcast_consumer(lanes, bits):
    return [
        f"  %b = bitcast <{lanes} x i1> %m to i{lanes}",
        f"  %ok = icmp eq i{lanes} %b, {signed(mask_integer(bits), lanes)}",
    ]


def extract_consumer(lanes, bits):
    lines = []
    for i in range(lanes):
        lines += [
            f"  %e{i} = extractelement <{lanes} x i1> %m, i64 {i}",
            f"  %z{i} = zext i1 %e{i} to i32",
            f"  %s{i} = shl i32 %z{i}, {i}",
        ]
    lines.append("  %d0 = or i32 %s0, %s1")
    for i in range(2, lanes):
        lines.append(f"  %d{i - 1} = or i32 %d{i - 2}, %s{i}")
    lines.append(f"  %ok = icmp eq i32 %d{lanes - 2}, {mask_integer(bits)}")
    return lines


def branch_consumer(lanes, bits):
    # One branch per lane on an extracted element: the mask is used in several blocks, as when clang
    # scalarizes a masked load or store.
    lines = ["  br label %test0"]
    for i in range(lanes):
        lines += [
            f"test{i}:",
            "  %acc0 = add i32 0, 0" if i == 0 else f"  %acc{i} = add i32 %out{i - 1}, 0",
            f"  %e{i} = extractelement <{lanes} x i1> %m, i64 {i}",
            f"  br i1 %e{i}, label %on{i}, label %off{i}",
            f"on{i}:",
            f"  %set{i} = or i32 %acc{i}, {1 << i}",
            f"  br label %join{i}",
            f"off{i}:",
            f"  br label %join{i}",
            f"join{i}:",
            f"  %out{i} = phi i32 [ %set{i}, %on{i} ], [ %acc{i}, %off{i} ]",
        ]
        if i + 1 < lanes:
            lines.append(f"  br label %test{i + 1}")
    lines.append(f"  %ok = icmp eq i32 %out{lanes - 1}, {mask_integer(bits)}")
    return lines


def select_consumer(kind):
    def consume(lanes, bits):
        if kind == "i8" and lanes < 8:
            return None
        ones = list(range(1, lanes + 1))
        expected = [v if bit else -v for v, bit in zip(ones, bits)]
        return [
            f"  %r = select <{lanes} x i1> %m, {vector_constant(kind, ones)}, {vector_constant(kind, [-v for v in ones])}",
        ] + check_vector_equals(kind, lanes, expected)
    return consume


def extend_consumer(opcode, kind):
    def consume(lanes, bits):
        if kind == "i8" and lanes < 8:
            return None
        expected = [(1 if opcode == "zext" else -1) if bit else 0 for bit in bits]
        return [f"  %r = {opcode} <{lanes} x i1> %m to <{lanes} x {kind}>"] + check_vector_equals(kind, lanes, expected)
    return consume


def binary_consumer(opcode):
    operation = {"and": lambda x, y: x and y, "or": lambda x, y: x or y, "xor": lambda x, y: x != y}[opcode]

    def consume(lanes, bits):
        other = [(i % 2) == 0 for i in range(lanes)]
        return [
            f"  %r = {opcode} <{lanes} x i1> %m, {mask_constant(other)}",
            f"  %b = bitcast <{lanes} x i1> %r to i{lanes}",
            f"  %ok = icmp eq i{lanes} %b, {signed(mask_integer([operation(x, y) for x, y in zip(bits, other)]), lanes)}",
        ]
    return consume


def multi_use_consumer(lanes, bits):
    ones = list(range(1, lanes + 1))
    expected = [v if bit else -v for v, bit in zip(ones, bits)]
    return [
        f"  %b = bitcast <{lanes} x i1> %m to i{lanes}",
        f"  %ok1 = icmp eq i{lanes} %b, {signed(mask_integer(bits), lanes)}",
        f"  %r = select <{lanes} x i1> %m, {vector_constant('i32', ones)}, {vector_constant('i32', [-v for v in ones])}",
        f"  %eq = icmp eq <{lanes} x i32> %r, {vector_constant('i32', expected)[len(f'<{lanes} x i32> '):]}",
        f"  %eqbits = bitcast <{lanes} x i1> %eq to i{lanes}",
        f"  %ok2 = icmp eq i{lanes} %eqbits, -1",
        f"  %e0 = extractelement <{lanes} x i1> %m, i64 0",
        f"  %ok3 = icmp eq i1 %e0, {'true' if bits[0] else 'false'}",
        "  %ok12 = and i1 %ok1, %ok2",
        "  %ok = and i1 %ok12, %ok3",
    ]


def other_block_consumer(lanes, bits):
    return [
        "  br label %next",
        "next:",
        f"  %b = bitcast <{lanes} x i1> %m to i{lanes}",
        f"  %ok = icmp eq i{lanes} %b, {signed(mask_integer(bits), lanes)}",
    ]


def phi_consumer(lanes, bits):
    other = [(i % 2) == 0 for i in range(lanes)]
    return [
        "  br i1 true, label %cleft, label %cright",
        "cleft:",
        "  br label %cmerge",
        "cright:",
        f"  %flipped = xor <{lanes} x i1> %m, {mask_constant(other)}",
        "  br label %cmerge",
        "cmerge:",
        f"  %p = phi <{lanes} x i1> [ %m, %cleft ], [ %flipped, %cright ]",
        f"  %b = bitcast <{lanes} x i1> %p to i{lanes}",
        f"  %ok = icmp eq i{lanes} %b, {signed(mask_integer(bits), lanes)}",
    ]


def call_consumer(lanes, bits):
    return [
        f"  %v = call i32 @digest_mask{lanes}(<{lanes} x i1> %m)",
        f"  %ok = icmp eq i32 %v, {mask_integer(bits)}",
    ]


def store_consumer(lanes, bits):
    return [
        f"  %cslot = alloca <{lanes} x i1>",
        f"  store <{lanes} x i1> %m, ptr %cslot",
        f"  %l = load <{lanes} x i1>, ptr %cslot",
        f"  %b = bitcast <{lanes} x i1> %l to i{lanes}",
        f"  %ok = icmp eq i{lanes} %b, {signed(mask_integer(bits), lanes)}",
    ]


CONSUMERS = {
    "bitcast_to_integer": bitcast_consumer,
    "extractelement": extract_consumer,
    "extractelement_branches": branch_consumer,
    "select_i8": select_consumer("i8"),
    "select_i16": select_consumer("i16"),
    "select_i32": select_consumer("i32"),
    "select_i64": select_consumer("i64"),
    "select_float": select_consumer("float"),
    "select_double": select_consumer("double"),
    "zext_i8": extend_consumer("zext", "i8"),
    "zext_i32": extend_consumer("zext", "i32"),
    "zext_i64": extend_consumer("zext", "i64"),
    "sext_i8": extend_consumer("sext", "i8"),
    "sext_i32": extend_consumer("sext", "i32"),
    "sext_i64": extend_consumer("sext", "i64"),
    "and_with_constant": binary_consumer("and"),
    "or_with_constant": binary_consumer("or"),
    "xor_with_constant": binary_consumer("xor"),
    "several_uses": multi_use_consumer,
    "used_in_another_block": other_block_consumer,
    "phi": phi_consumer,
    "call_argument": call_consumer,
    "store_then_load": store_consumer,
}


# Combinations that don't work yet, as (producer, consumer, lanes, reason); None matches anything. Found with
# `--probe`; each one is a problem to fix in the compiler, not a property of the test. When one is fixed, delete
# its entry so that the test starts covering it. (Empty: everything here works.)
KNOWN_GAPS = []


def gap_reason(producer, consumer, lanes):
    for gap_producer, gap_consumer, gap_lanes, reason in KNOWN_GAPS:
        if gap_producer in (None, producer) and gap_consumer in (None, consumer) and gap_lanes in (None, lanes):
            return reason
    return None


# ---------------------------------------------------------------------------------------------------------------

def helpers():
    lines = []
    for lanes in LANE_COUNTS:
        lines += [
            f"define <{lanes} x i1> @pass_mask{lanes}(<{lanes} x i1> %v) {{",
            f"  ret <{lanes} x i1> %v",
            "}",
            f"define i32 @digest_mask{lanes}(<{lanes} x i1> %v) {{",
            f"  %b = bitcast <{lanes} x i1> %v to i{lanes}",
            f"  %z = zext i{lanes} %b to i32",
            "  ret i32 %z",
            "}",
            "",
        ]
    return lines


def cases(include_gaps):
    for producer in PRODUCERS:
        for consumer in CONSUMERS:
            for lanes in LANE_COUNTS:
                if gap_reason(producer, consumer, lanes) and not include_gaps:
                    continue
                for pattern in PATTERNS:
                    made = PRODUCERS[producer](lanes, pattern)
                    if made is None:
                        continue
                    lines, bits = made
                    read = CONSUMERS[consumer](lanes, bits)
                    if read is None:
                        continue
                    yield f"{producer}__{consumer}__{lanes}_{pattern}", lines, read


def function(name, produced, consumed):
    return (
        [f"define i32 @{name}() {{"] + produced + consumed
        + ["  %res = zext i1 %ok to i32", "  ret i32 %res", "}", ""]
    )


def main():
    args = sys.argv[1:]
    here = os.path.dirname(os.path.abspath(__file__))

    if args[:1] == ["--probe"]:
        directory = args[1]
        os.makedirs(directory, exist_ok=True)
        count = 0
        for name, produced, consumed in cases(include_gaps=True):
            body = helpers() + function(name, produced, consumed) + [
                "define i32 @main() {",
                f"  %r = call i32 @{name}()",
                "  %bad = icmp ne i32 %r, 1",
                "  %z = zext i1 %bad to i32",
                "  ret i32 %z",
                "}",
            ]
            with open(os.path.join(directory, name + ".ll"), "w") as f:
                f.write("\n".join(body) + "\n")
            count += 1
        print(f"wrote {count} files to {directory}")
        return

    include_gaps = "--include-gaps" in args
    out = [
        "; Generated by tests/generators/generate_vector_mask_flow_tests.py: every way of making a <N x i1> mask",
        "; (comparison, and/or/xor, select, freeze, phi, constant, call, load, shufflevector, insertelement, trunc,",
        "; bitcast) against every way of reading one (bitcast to an integer, extractelement, select, zext/sext, and/or/xor,",
        "; several uses, another block, phi, call argument, store). `main` prints the name of each failing",
        "; combination and returns 1 if there was one.",
        "",
    ]
    out += helpers()

    names = []
    for name, produced, consumed in cases(include_gaps):
        out += function(name, produced, consumed)
        names.append(name)

    strings = []
    for i, name in enumerate(names):
        text = f"FAIL {name}\\0A\\00"
        length = len(f"FAIL {name}") + 2
        strings.append(f'@name{i} = private unnamed_addr constant [{length} x i8] c"{text}"')
    out += strings
    out += ["", "declare i32 @printf(ptr, ...)", "", "define i32 @main() {", "  %failed = alloca i32", "  store i32 0, ptr %failed"]
    for i, name in enumerate(names):
        out += [
            f"  %r{i} = call i32 @{name}()",
            f"  %ok{i} = icmp eq i32 %r{i}, 1",
            f"  br i1 %ok{i}, label %pass{i}, label %fail{i}",
            f"fail{i}:",
            f"  %p{i} = call i32 (ptr, ...) @printf(ptr @name{i})",
            "  store i32 1, ptr %failed",
            f"  br label %pass{i}",
            f"pass{i}:",
        ]
    out += ["  %result = load i32, ptr %failed", "  ret i32 %result", "}"]

    path = os.path.normpath(os.path.join(here, "..", "hand-written", "vector_mask_flow.ll"))
    with open(path, "w") as f:
        f.write("\n".join(out) + "\n")
    print(f"wrote {path}: {len(names)} functions, {len(KNOWN_GAPS)} known gap entries left out")


main()
