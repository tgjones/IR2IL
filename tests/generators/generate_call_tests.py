#!/usr/bin/env python3
"""Generates tests/hand-written/calls_mixed_arguments.ll.

Calls whose parameters and return values mix types: i8, i16 (with and without `signext`/`zeroext`), i32, i64, float,
double, ptr, vectors (<4 x i32>, <2 x i64>, <8 x i16>, <16 x i8>, <4 x float>, <2 x double>), structs (flat and nested),
arrays, and every one of them as the only parameter. The signatures are chosen from a fixed seed, from 1 to 14
parameters (so some are passed on the stack), and a few are all of one type (10 floats, 10 i64, ...).

Each callee folds all the lanes of its arguments into one i64 (`acc * 1000003 ^ lane`), so a wrong value, order
or width in any of them changes the result. Each signature is called three ways, with the arguments as
  - constants,
  - values loaded from a global, and
  - values returned by a call (`@make_<type>`, which builds the value at run time, so each type is also a return type),
and the three hashes are printed. Also: results that are ignored, a `sret` return and a `byval` argument (the callee
changes its copy, which the caller must not see).

There is no expected value here: the tests compare IR2IL's output with that of the same IR built by clang.

Usage:
    generate_call_tests.py            rewrite tests/hand-written/calls_mixed_arguments.ll
"""
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from generate_float_tests import PRELUDE, hex_double, main_function, signed_value  # noqa: E402

# A type is ("int", bits), ("float",), ("double",), ("ptr",), ("vec", element, lanes), ("struct", [fields]) or
# ("array", element, length).
I8, I16, I32, I64 = ("int", 8), ("int", 16), ("int", 32), ("int", 64)
FLOAT, DOUBLE, PTR = ("float",), ("double",), ("ptr",)
TYPES = [
    I8, I16, I32, I64, FLOAT, DOUBLE, PTR,
    ("vec", I32, 4), ("vec", I64, 2), ("vec", I16, 8), ("vec", I8, 16), ("vec", FLOAT, 4), ("vec", DOUBLE, 2),
    ("struct", [I32, FLOAT]),
    ("struct", [I8, I64, DOUBLE]),
    ("struct", [I16, I16, I32]),
    ("struct", [I32, ("struct", [I8, DOUBLE]), PTR]),
    ("struct", [FLOAT, FLOAT, FLOAT, FLOAT]),
    ("struct", [DOUBLE, DOUBLE]),
    ("struct", [I64, I64, I64]),
    ("array", I16, 3),
    ("array", DOUBLE, 2),
]
EXTENSIONS = ["", "signext", "zeroext"]


def ll(t):
    kind = t[0]
    if kind == "int":
        return f"i{t[1]}"
    if kind in ("float", "double", "ptr"):
        return kind
    if kind == "vec":
        return f"<{t[2]} x {ll(t[1])}>"
    if kind == "struct":
        return "{ " + ", ".join(ll(f) for f in t[1]) + " }"
    return f"[{t[2]} x {ll(t[1])}]"


def parts_of(t):
    """The element types of a vector, struct or array, in order."""
    if t[0] == "vec" or t[0] == "array":
        return [t[1]] * t[2]
    return t[1]


def const(t, k):
    kind = t[0]
    if kind == "int":
        return str(signed_value(k * 0x9E37 + 0x55, t[1]))
    if kind in ("float", "double"):
        return hex_double((k % 13 - 6) * 0.75)
    if kind == "ptr":
        return f"getelementptr (i32, ptr @ptrtab, i64 {k % 4})"
    elements = [f"{ll(e)} {const(e, k + i + 1)}" for i, e in enumerate(parts_of(t))]
    if kind == "vec":
        return "<" + ", ".join(elements) + ">"
    return ("{ " if kind == "struct" else "[") + ", ".join(elements) + (" }" if kind == "struct" else "]")


class Body:
    """Instruction lines with fresh value names."""

    def __init__(self):
        self.lines = []
        self.count = 0

    def new(self, prefix="t"):
        self.count += 1
        return f"%{prefix}{self.count}"

    def add(self, text):
        self.lines.append("  " + text)

    def let(self, text, prefix="t"):
        name = self.new(prefix)
        self.add(f"{name} = {text}")
        return name


def build(b, t, k, offset):
    """Builds a value of type t at run time from the i32 %k; returns its name."""
    kind = t[0]
    x = b.let(f"add i32 {k}, {offset}")
    if kind == "int":
        bits = t[1]
        v = x if bits == 32 else b.let(f"{'trunc' if bits < 32 else 'sext'} i32 {x} to i{bits}")
        return b.let(f"mul i{bits} {v}, 37")
    if kind in ("float", "double"):
        f = b.let(f"sitofp i32 {x} to {kind}")
        return b.let(f"fmul {kind} {f}, 0.75")
    if kind == "ptr":
        index = b.let(f"and i32 {x}, 3")
        return b.let(f"getelementptr i32, ptr @ptrtab, i32 {index}")
    value = "poison" if kind == "vec" else "undef"
    for i, e in enumerate(parts_of(t)):
        element = build(b, e, k, offset + i + 1)
        if kind == "vec":
            value = b.let(f"insertelement {ll(t)} {value}, {ll(e)} {element}, i32 {i}", "v")
        else:
            value = b.let(f"insertvalue {ll(t)} {value}, {ll(e)} {element}, {i}", "v")
    return value


def lanes(b, t, value):
    """The i64 values that carry every lane of `value`."""
    kind = t[0]
    if kind == "int":
        return [value] if t[1] == 64 else [b.let(f"sext i{t[1]} {value} to i64")]
    if kind == "float":
        return [b.let(f"zext i32 {b.let(f'bitcast float {value} to i32')} to i64")]
    if kind == "double":
        return [b.let(f"bitcast double {value} to i64")]
    if kind == "ptr":
        return [b.let(f"sext i32 {b.let(f'load i32, ptr {value}')} to i64")]
    result = []
    for i, e in enumerate(parts_of(t)):
        if kind == "vec":
            element = b.let(f"extractelement {ll(t)} {value}, i32 {i}")
        else:
            element = b.let(f"extractvalue {ll(t)} {value}, {i}")
        result += lanes(b, e, element)
    return result


def fold(b, acc, values):
    for v in values:
        m = b.let(f"mul i64 {acc}, 1000003", "h")
        acc = b.let(f"xor i64 {m}, {v}", "h")
    return acc


def mangle(t):
    return {"int": lambda: f"i{t[1]}", "float": lambda: "float", "double": lambda: "double", "ptr": lambda: "ptr",
            "vec": lambda: f"v{t[2]}{mangle(t[1])}", "struct": lambda: "s_" + "_".join(mangle(f) for f in t[1]) + "_e",
            "array": lambda: f"a{t[2]}{mangle(t[1])}"}[t[0]]()


def param(t, extension):
    return f"{ll(t)} {extension}".strip()


def main():
    rng = random.Random(20261125)
    signatures = [[t] for t in TYPES]  # every type alone
    for _ in range(70):
        signatures.append([rng.choice(TYPES) for _ in range(rng.randint(2, 14))])
    for t in (FLOAT, DOUBLE, I64, I8, I16, PTR, ("vec", FLOAT, 4)):
        signatures.append([t] * 10)  # past the registers
    signatures.append([I8, FLOAT, I64, DOUBLE, I16, PTR, I32, FLOAT, I8, DOUBLE, I64, I16])  # alternating
    # An extension attribute on every i8 and i16 parameter, chosen per signature position.
    typed = []
    for s in signatures:
        typed.append([(t, rng.choice(EXTENSIONS) if t in (I8, I16) else "") for t in s])

    out = [
        "; Generated by tests/generators/generate_call_tests.py: calls with mixed argument and return types. See the",
        "; generator for the details.",
        "",
        PRELUDE,
        "@ptrtab = private global [4 x i32] [i32 11, i32 -22, i32 33, i32 -44]",
        "",
    ]
    # make_<type>: builds a value of the type from a seed (the return value of every type).
    for t in TYPES:
        b = Body()
        v = build(b, t, "%k", 0)
        out += [f"define {ll(t)} @make_{mangle(t)}(i32 %k) {{"] + b.lines + [f"  ret {ll(t)} {v}", "}", ""]
    for bits in (8, 16):
        for ext in ("signext", "zeroext"):
            out += [f"define {ext} i{bits} @make_i{bits}_{ext}(i32 %k) {{", f"  %t = trunc i32 %k to i{bits}",
                    f"  ret i{bits} %t", "}", ""]

    globals_, functions, names = [], [], []
    for index, signature in enumerate(typed):
        # The callee.
        b = Body()
        names_of = [f"%p{i}" for i in range(len(signature))]
        acc = "0"
        for (t, _), n in zip(signature, names_of):
            acc = fold(b, acc if acc != "0" else "1469598103934665603", lanes(b, t, n))
        params = ", ".join(f"{param(t, e)} {n}" for (t, e), n in zip(signature, names_of))
        functions += [f"define i64 @callee_{index}({params}) {{"] + b.lines + [f"  ret i64 {acc}", "}", ""]

        # The caller: constants, loaded values, returned values.
        b = Body()
        for variant in ("const", "loaded", "made"):
            args = []
            for position, (t, e) in enumerate(signature):
                k = (index * 7 + position) % 5
                if variant == "const":
                    value = const(t, k)
                elif variant == "loaded":
                    g = f"@g_{index}_{position}"
                    globals_.append(f"{g} = private global {ll(t)} {const(t, k + 2)}")
                    value = b.let(f"load {ll(t)}, ptr {g}")
                else:
                    value = b.let(f"call {ll(t)} @make_{mangle(t)}(i32 {k * 3 + 1})")
                args.append(f"{param(t, e)} {value}")
            r = b.let(f"call i64 @callee_{index}({', '.join(args)})", "r")
            b.add(f"call void @emit_i64(i64 {r})")
        functions += [f"define void @run_call_{index}() {{"] + b.lines + ["  ret void", "}", ""]
        names.append(f"run_call_{index}")

    # Results that are ignored, and the extension attributes on returns.
    b = Body()
    for t in TYPES:
        b.add(f"call {ll(t)} @make_{mangle(t)}(i32 5)")
    for bits in (8, 16):
        for ext in ("signext", "zeroext"):
            r = b.let(f"call {ext} i{bits} @make_i{bits}_{ext}(i32 -3)")
            w = b.let(f"{'sext' if ext == 'signext' else 'zext'} i{bits} {r} to i64")
            b.add(f"call void @emit_i64(i64 {w})")
    functions += ["define void @run_ignored_results() {"] + b.lines + ["  ret void", "}", ""]
    names.append("run_ignored_results")

    # sret and byval.
    functions += [
        "%S = type { i8, i32, double, i16 }",
        "",
        "define void @fill_s(ptr sret(%S) align 8 %out, i32 %k) {",
        "  %a = trunc i32 %k to i8",
        "  store i8 %a, ptr %out, align 8",
        "  %p1 = getelementptr %S, ptr %out, i32 0, i32 1",
        "  %b = mul i32 %k, 3",
        "  store i32 %b, ptr %p1, align 4",
        "  %p2 = getelementptr %S, ptr %out, i32 0, i32 2",
        "  %c = sitofp i32 %k to double",
        "  store double %c, ptr %p2, align 8",
        "  %p3 = getelementptr %S, ptr %out, i32 0, i32 3",
        "  %d = trunc i32 %k to i16",
        "  store i16 %d, ptr %p3, align 2",
        "  ret void",
        "}",
        "",
        "; Reads the fields, then changes its own copy.",
        "define i64 @sum_s(ptr byval(%S) align 8 %s) {",
        "  %a = load i8, ptr %s, align 8",
        "  %p1 = getelementptr %S, ptr %s, i32 0, i32 1",
        "  %b = load i32, ptr %p1, align 4",
        "  %p2 = getelementptr %S, ptr %s, i32 0, i32 2",
        "  %c = load double, ptr %p2, align 8",
        "  %p3 = getelementptr %S, ptr %s, i32 0, i32 3",
        "  %d = load i16, ptr %p3, align 2",
        "  %a64 = sext i8 %a to i64",
        "  %b64 = sext i32 %b to i64",
        "  %c64 = fptosi double %c to i64",
        "  %d64 = sext i16 %d to i64",
        "  %s1 = add i64 %a64, %b64",
        "  %s2 = add i64 %s1, %c64",
        "  %s3 = add i64 %s2, %d64",
        "  store i32 12345, ptr %p1, align 4",
        "  store i8 99, ptr %s, align 8",
        "  ret i64 %s3",
        "}",
        "",
        "define void @run_sret_byval() {",
        "  %buf = alloca %S, align 8",
        "  call void @fill_s(ptr sret(%S) align 8 %buf, i32 7)",
        "  %first = call i64 @sum_s(ptr byval(%S) align 8 %buf)",
        "  call void @emit_i64(i64 %first)",
        "  ; The callee changed its copy: the same call gives the same sum.",
        "  %second = call i64 @sum_s(ptr byval(%S) align 8 %buf)",
        "  call void @emit_i64(i64 %second)",
        "  %p1 = getelementptr %S, ptr %buf, i32 0, i32 1",
        "  %b = load i32, ptr %p1, align 4",
        "  %b64 = sext i32 %b to i64",
        "  call void @emit_i64(i64 %b64)",
        "  %a = load i8, ptr %buf, align 8",
        "  %a64 = sext i8 %a to i64",
        "  call void @emit_i64(i64 %a64)",
        "  ret void",
        "}",
        "",
    ]
    names.append("run_sret_byval")

    out += globals_ + [""] + functions + [main_function(names)]
    path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "hand-written", "calls_mixed_arguments.ll")
    with open(path, "w") as f:
        f.write("\n".join(out) + "\n")
    print(f"wrote {os.path.normpath(path)}: {len(signatures)} signatures")


if __name__ == "__main__":
    main()
