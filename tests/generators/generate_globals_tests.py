#!/usr/bin/env python3
"""Generates tests/hand-written/globals.ll and constant_expressions.ll.

globals.ll: global variables of every kind, each read, written and read again, printing every scalar in it.
  - scalars (i8, i16, i32, i64, float, double) that are initialised, zero-initialised or `constant`, with every
    linkage (default, internal, private, weak, weak_odr, linkonce_odr, hidden, and common for a zero value);
  - aggregates (arrays, strings, structs, packed and nested structs, arrays of structs), initialised, zero-initialised
    and `constant`, with several linkages;
  - pointers: to a global, into the middle of another, to a pointer, to itself, in a cycle of two, null, and inside
    structs and arrays. A pointer is never printed (its value differs between runs): the test prints whether it is
    equal to the expected address, and what is read through it.

constant_expressions.ll: constant expressions (getelementptr, ptrtoint, inttoptr, and what LLVM keeps of
add/sub/trunc/...) used as operands of instructions and in the initialisers of globals. Only values that do not
depend on where a global is placed are printed: differences within one global, and what is read through a pointer.

There is no expected value here: the tests compare IR2IL's output with that of the same IR built by clang.

Usage:
    generate_globals_tests.py            rewrite the files in tests/hand-written
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from generate_float_tests import PRELUDE, hex_double, main_function, signed_value  # noqa: E402

# A type is ("int", bits), ("float",), ("double",), ("ptr", target) where target is the address the pointer is
# initialised with (an IR constant, or "null"), ("arr", element, length) or ("struct", [fields], packed).
I8, I16, I32, I64 = ("int", 8), ("int", 16), ("int", 32), ("int", 64)
FLOAT, DOUBLE = ("float",), ("double",)
SCALARS = [I8, I16, I32, I64, FLOAT, DOUBLE]

LINKAGES = ["", "internal", "private", "weak", "weak_odr", "linkonce_odr", "hidden"]


def ll(t):
    kind = t[0]
    if kind == "int":
        return f"i{t[1]}"
    if kind in ("float", "double"):
        return kind
    if kind == "ptr":
        return "ptr"
    if kind == "arr":
        return f"[{t[2]} x {ll(t[1])}]"
    inner = ", ".join(ll(f) for f in t[1])
    return f"<{{ {inner} }}>" if t[2] else f"{{ {inner} }}"


def scalar_const(t, k):
    """A deterministic constant of a scalar type, different for each k."""
    if t[0] == "int":
        return str(signed_value(k * 0x9E37 + 0x55, t[1]))
    return hex_double((k % 11 - 5) * 1.25)


def init(t, k, zero=False):
    """The initialiser of type t; the leaves are numbered from k."""
    if zero:
        return "zeroinitializer" if t[0] in ("arr", "struct") else ("null" if t[0] == "ptr" else scalar_zero(t))
    if t[0] == "ptr":
        return t[1]
    if t[0] in ("int", "float", "double"):
        return scalar_const(t, k)
    parts = []
    for e in (([t[1]] * t[2]) if t[0] == "arr" else t[1]):
        parts.append(f"{ll(e)} {init(e, k)}")
        k += count_leaves(e)
    inner = ", ".join(parts)
    if t[0] == "arr":
        return f"[{inner}]"
    return f"<{{ {inner} }}>" if t[2] else f"{{ {inner} }}"


def scalar_zero(t):
    return "0" if t[0] == "int" else "0.0"


def count_leaves(t):
    if t[0] == "arr":
        return t[2] * count_leaves(t[1])
    if t[0] == "struct":
        return sum(count_leaves(f) for f in t[1])
    return 1


def leaves(t, path=()):
    """(indices, scalar type) of every scalar in t, in order."""
    if t[0] == "arr":
        for i in range(t[2]):
            yield from leaves(t[1], path + (i,))
    elif t[0] == "struct":
        for i, f in enumerate(t[1]):
            yield from leaves(f, path + (i,))
    else:
        yield path, t


class Body:
    def __init__(self):
        self.lines = []
        self.count = 0

    def add(self, text):
        self.lines.append("  " + text)

    def let(self, text):
        self.count += 1
        name = f"%t{self.count}"
        self.add(f"{name} = {text}")
        return name


def address(b, container, whole_type, path):
    """The address of the leaf at path inside a global."""
    if not path:
        return container
    indices = ", ".join(f"i32 {i}" for i in path)
    return b.let(f"getelementptr inbounds {ll(whole_type)}, ptr {container}, i32 0, {indices}")


def print_leaf(b, t, ptr, expected_pointer=None):
    kind = t[0]
    if kind == "int":
        v = b.let(f"load i{t[1]}, ptr {ptr}")
        w = v if t[1] == 64 else b.let(f"sext i{t[1]} {v} to i64")
        b.add(f"call void @emit_i64(i64 {w})")
    elif kind in ("float", "double"):
        v = b.let(f"load {kind}, ptr {ptr}")
        b.add(f"call void @emit_{kind}({kind} {v})")
    else:
        # A pointer: print whether it is the expected address.
        v = b.let(f"load ptr, ptr {ptr}")
        same = b.let(f"icmp eq ptr {v}, {expected_pointer}")
        w = b.let(f"zext i1 {same} to i64")
        b.add(f"call void @emit_i64(i64 {w})")


def store_leaf(b, t, ptr, k):
    if t[0] == "ptr":
        b.add(f"store ptr null, ptr {ptr}")
        return "null"
    b.add(f"store {ll(t)} {scalar_const(t, k)}, ptr {ptr}")
    return None


def exercise(name, t, ptr_name, writable, seed):
    """A function that prints every leaf of the global, overwrites them (when writable) and prints them again."""
    b = Body()
    for path, leaf in leaves(t):
        print_leaf(b, leaf, address(b, ptr_name, t, path), leaf[1] if leaf[0] == "ptr" else None)
    if writable:
        for n, (path, leaf) in enumerate(leaves(t)):
            store_leaf(b, leaf, address(b, ptr_name, t, path), seed + 100 + n)
        for n, (path, leaf) in enumerate(leaves(t)):
            print_leaf(b, leaf, address(b, ptr_name, t, path), "null" if leaf[0] == "ptr" else None)
    return [f"define void @{name}() {{"] + b.lines + ["  ret void", "}", ""]


def global_line(name, linkage, kind, t, value, align=None):
    parts = [f"@{name} ="]
    if linkage:
        parts.append(linkage)
    parts.append("constant" if kind == "constant" else "global")
    parts.append(f"{ll(t)} {value}")
    line = " ".join(parts)
    return line + (f", align {align}" if align else "")


def globals_file():
    out = [
        "; Generated by tests/generators/generate_globals_tests.py: global variables. See the generator for the details.",
        "",
        PRELUDE,
    ]
    globals_, functions, names = [], [], []
    seed = 0

    def add(name, linkage, kind, t, zero=False, align=None):
        nonlocal seed
        seed += 7
        value = init(t, seed, zero)
        globals_.append(global_line(name, linkage, kind, t, value, align))
        functions.extend(exercise(f"run_{name}", t, f"@{name}", kind != "constant", seed))
        names.append(f"run_{name}")

    # Scalars: every type, kind and linkage.
    for t in SCALARS:
        for kind in ("initialised", "zero", "constant"):
            for linkage in LINKAGES:
                add(f"s_{ll(t)}_{kind}_{linkage or 'default'}", linkage, "constant" if kind == "constant" else "global",
                    t, zero=(kind == "zero"))
        if t[0] == "int":
            add(f"s_{ll(t)}_common", "common", "global", t, zero=True)

    # Aggregates.
    plain_struct = ("struct", [I8, I32, I64, DOUBLE], False)
    aggregates = {
        "arr_i32": ("arr", I32, 5),
        "arr_i8": ("arr", I8, 7),
        "arr_i16": ("arr", I16, 3),
        "arr_i64": ("arr", I64, 4),
        "arr_float": ("arr", FLOAT, 3),
        "arr_double": ("arr", DOUBLE, 3),
        "struct_plain": plain_struct,
        "struct_small": ("struct", [I8, I8, I16], False),
        "struct_packed": ("struct", [I8, I32, I16], True),
        "struct_nested": ("struct", [I16, ("arr", I32, 3), ("struct", [I8, DOUBLE], False)], False),
        "arr_of_structs": ("arr", ("struct", [I32, I8], False), 3),
        "arr_of_arrays": ("arr", ("arr", I16, 3), 2),
        "struct_float_pair": ("struct", [FLOAT, FLOAT], False),
    }
    for aname, t in aggregates.items():
        for kind in ("initialised", "zero", "constant"):
            for linkage in ("", "internal", "private", "weak", "linkonce_odr"):
                add(f"a_{aname}_{kind}_{linkage or 'default'}", linkage, "constant" if kind == "constant" else "global",
                    t, zero=(kind == "zero"))
    add("a_struct_packed_align1", "", "global", aggregates["struct_packed"], align=1)
    add("a_arr_i32_align16", "", "global", aggregates["arr_i32"], align=16)

    # A string, read byte by byte.
    globals_.append('@str = private unnamed_addr constant [6 x i8] c"hello\\00", align 1')
    globals_.append('@str_escapes = internal constant [8 x i8] c"a\\00\\FF\\0A\\22\\5C\\80z", align 1')
    for sname, n in (("str", 6), ("str_escapes", 8)):
        b = Body()
        for i in range(n):
            p = b.let(f"getelementptr inbounds [{n} x i8], ptr @{sname}, i64 0, i64 {i}")
            v = b.let(f"load i8, ptr {p}")
            w = b.let(f"zext i8 {v} to i64")
            b.add(f"call void @emit_i64(i64 {w})")
        functions += [f"define void @run_{sname}() {{"] + b.lines + ["  ret void", "}", ""]
        names.append(f"run_{sname}")

    # Pointers. The targets come first; every pointer is checked against its target and read through.
    globals_ += [
        "@target_a = global i32 1111",
        "@target_arr = global [4 x i32] [i32 10, i32 20, i32 30, i32 40]",
        "@target_s = global { i32, i64 } { i32 -7, i64 77 }",
        "@target_c = internal constant i32 9999",
        "@p_to_a = global ptr @target_a",
        "@p_to_arr = global ptr @target_arr",
        "@p_into_arr = global ptr getelementptr inbounds ([4 x i32], ptr @target_arr, i64 0, i64 2)",
        "@p_past_arr = global ptr getelementptr ([4 x i32], ptr @target_arr, i64 0, i64 4)",
        "@p_into_s = global ptr getelementptr inbounds ({ i32, i64 }, ptr @target_s, i32 0, i32 1)",
        "@p_to_const = internal global ptr @target_c",
        "@p_to_p = global ptr @p_to_a",
        "@p_to_p_to_p = private global ptr @p_to_p",
        "@p_null = global ptr null",
        "@p_self = global ptr @p_self",
        "@p_cycle_a = global ptr @p_cycle_b",
        "@p_cycle_b = global ptr @p_cycle_a",
        "@p_forward = global ptr @defined_later",
        "@defined_later = global i32 4242",
        "@p_const_p = internal constant ptr @target_a",
        "@p_struct = global { ptr, ptr, i32 } { ptr @target_arr, ptr getelementptr inbounds ([4 x i32], ptr @target_arr, i64 0, i64 3), i32 7 }",
        "@p_array = global [4 x ptr] [ptr @target_a, ptr @target_arr, ptr getelementptr inbounds ([4 x i32], ptr @target_arr, i64 0, i64 1), ptr null]",
        "@p_array_const = private constant [3 x ptr] [ptr @target_c, ptr @p_to_a, ptr @p_array_const]",
        "",
    ]
    b = Body()

    def read_i32(p):
        v = b.let(f"load i32, ptr {p}")
        w = b.let(f"zext i32 {v} to i64")
        b.add(f"call void @emit_i64(i64 {w})")

    def check(p, expected):
        v = b.let(f"load ptr, ptr {p}")
        same = b.let(f"icmp eq ptr {v}, {expected}")
        w = b.let(f"zext i1 {same} to i64")
        b.add(f"call void @emit_i64(i64 {w})")
        return v

    for p, expected in (("@p_to_a", "@target_a"), ("@p_to_arr", "@target_arr"),
                        ("@p_into_arr", "getelementptr inbounds ([4 x i32], ptr @target_arr, i64 0, i64 2)"),
                        ("@p_past_arr", "getelementptr ([4 x i32], ptr @target_arr, i64 0, i64 4)"),
                        ("@p_into_s", "getelementptr inbounds ({ i32, i64 }, ptr @target_s, i32 0, i32 1)"),
                        ("@p_to_const", "@target_c"), ("@p_to_p", "@p_to_a"), ("@p_to_p_to_p", "@p_to_p"),
                        ("@p_null", "null"), ("@p_self", "@p_self"), ("@p_cycle_a", "@p_cycle_b"),
                        ("@p_cycle_b", "@p_cycle_a"), ("@p_forward", "@defined_later"),
                        ("@p_const_p", "@target_a")):
        check(p, expected)
    # Read through them.
    read_i32(check("@p_to_a", "@target_a"))
    v = check("@p_into_arr", "getelementptr inbounds ([4 x i32], ptr @target_arr, i64 0, i64 2)")
    read_i32(v)
    read_i32(check("@p_to_const", "@target_c"))
    read_i32(check("@p_forward", "@defined_later"))
    # Two and three levels, and around the cycle.
    pp = check("@p_to_p", "@p_to_a")
    p1 = b.let(f"load ptr, ptr {pp}")
    read_i32(p1)
    ppp = check("@p_to_p_to_p", "@p_to_p")
    pp2 = b.let(f"load ptr, ptr {ppp}")
    p2 = b.let(f"load ptr, ptr {pp2}")
    read_i32(p2)
    c1 = check("@p_cycle_a", "@p_cycle_b")
    c2 = b.let(f"load ptr, ptr {c1}")
    same = b.let("icmp eq ptr %s, @p_cycle_a".replace("%s", c2))
    w = b.let(f"zext i1 {same} to i64")
    b.add(f"call void @emit_i64(i64 {w})")
    # Inside a struct and an array.
    sp = b.let("getelementptr inbounds { ptr, ptr, i32 }, ptr @p_struct, i32 0, i32 1")
    third = check(sp, "getelementptr inbounds ([4 x i32], ptr @target_arr, i64 0, i64 3)")
    read_i32(third)
    sp0 = b.let("getelementptr inbounds { ptr, ptr, i32 }, ptr @p_struct, i32 0, i32 0")
    read_i32(check(sp0, "@target_arr"))
    sp2 = b.let("getelementptr inbounds { ptr, ptr, i32 }, ptr @p_struct, i32 0, i32 2")
    read_i32(sp2)
    for i, expected in enumerate(("@target_a", "@target_arr", "getelementptr inbounds ([4 x i32], ptr @target_arr, i64 0, i64 1)", "null")):
        ap = b.let(f"getelementptr inbounds [4 x ptr], ptr @p_array, i64 0, i64 {i}")
        v = check(ap, expected)
        if i < 3:
            read_i32(v)
    for i, expected in enumerate(("@target_c", "@p_to_a", "@p_array_const")):
        ap = b.let(f"getelementptr inbounds [3 x ptr], ptr @p_array_const, i64 0, i64 {i}")
        check(ap, expected)
    # Write through a pointer, then read the target directly; repoint a global and read through it again.
    pa = b.let("load ptr, ptr @p_to_a")
    b.add(f"store i32 -5, ptr {pa}")
    read_i32("@target_a")
    b.add("store ptr @target_s, ptr @p_to_a")
    check("@p_to_a", "@target_s")
    pa = b.let("load ptr, ptr @p_to_a")
    v = b.let(f"load i32, ptr {pa}")
    w = b.let(f"zext i32 {v} to i64")
    b.add(f"call void @emit_i64(i64 {w})")
    # The global that p_to_p pointed through is changed: p_to_p still points at p_to_a.
    check("@p_to_p", "@p_to_a")
    functions += ["define void @run_pointers() {"] + b.lines + ["  ret void", "}", ""]
    names.append("run_pointers")

    out += globals_ + [""] + functions + [main_function(names)]
    return "\n".join(out) + "\n"


def constant_expressions_file():
    # What the uses below have in common: a table of 8 i32 and a nested struct, whose layout is known.
    out = [
        "; Generated by tests/generators/generate_globals_tests.py: constant expressions. See the generator for the details.",
        "",
        PRELUDE,
        "%Nest = type { i16, [3 x i32], { i8, double } }",
        "",
        "@carr = global [8 x i32] [i32 100, i32 101, i32 102, i32 103, i32 104, i32 105, i32 106, i32 107]",
        "@nest = global %Nest { i16 -3, [3 x i32] [i32 7, i32 -8, i32 9], { i8, double } { i8 5, double 2.5 } }",
        "@barr = global [16 x i8] [i8 0, i8 1, i8 2, i8 3, i8 4, i8 5, i8 6, i8 7, i8 8, i8 9, i8 10, i8 11, i8 12, i8 13, i8 14, i8 -15]",
        "",
    ]
    ELEM = "getelementptr ([8 x i32], ptr @carr, i64 0, i64 {})"
    OFFSET = "i64 sub (i64 ptrtoint (ptr getelementptr ([8 x i32], ptr @carr, i64 0, i64 {}) to i64), i64 ptrtoint (ptr @carr to i64))"
    globals_ = [
        # Offsets from the start of the array, as tables of them do.
        f"@offsets = global [4 x i64] [{', '.join(OFFSET.format(i) for i in (0, 2, 5, 7))}]",
        f"@offsets32 = global [4 x i32] [{', '.join('i32 trunc (' + OFFSET.format(i) + ' to i32)' for i in (1, 3, 4, 6))}]",
        f"@offsets8 = global [3 x i8] [{', '.join('i8 trunc (' + OFFSET.format(i) + ' to i8)' for i in (1, 2, 7))}]",
        f"@offsets_i16 = global [2 x i16] [{', '.join('i16 trunc (' + OFFSET.format(i) + ' to i16)' for i in (3, 5))}]",
        # Pointers into other globals.
        f"@ptr_table = global [4 x ptr] [ptr {ELEM.format(0)}, ptr {ELEM.format(3)}, ptr {ELEM.format(7)}, ptr getelementptr inbounds (%Nest, ptr @nest, i32 0, i32 1, i32 2)]",
        "@ptr_struct = global { ptr, i32, ptr } { ptr getelementptr inbounds (%Nest, ptr @nest, i32 0, i32 2, i32 1), i32 -1, ptr getelementptr inbounds ([16 x i8], ptr @barr, i64 0, i64 9) }",
        # Integers made from addresses, and back.
        "@addr_as_int = global i64 ptrtoint (ptr getelementptr ([8 x i32], ptr @carr, i64 0, i64 4) to i64)",
        "@addr_plus = global i64 add (i64 ptrtoint (ptr @carr to i64), i64 12)",
        "@addr_back = global ptr inttoptr (i64 add (i64 ptrtoint (ptr @carr to i64), i64 20) to ptr)",
        "@addr_roundtrip = global ptr inttoptr (i64 ptrtoint (ptr getelementptr ([8 x i32], ptr @carr, i64 0, i64 6) to i64) to ptr)",
        "@int_as_ptr = global ptr inttoptr (i64 4096 to ptr)",
        "@int_as_ptr_neg = global ptr inttoptr (i64 -16 to ptr)",
        "@null_plus = global ptr getelementptr (i8, ptr null, i64 40)",
        "@null_int = global i64 ptrtoint (ptr getelementptr (i8, ptr null, i64 -24) to i64)",
        "",
    ]
    b = Body()

    def emit(value, bits=64, signed=True):
        v = value
        if bits < 64:
            v = b.let(f"{'sext' if signed else 'zext'} i{bits} {value} to i64")
        b.add(f"call void @emit_i64(i64 {v})")

    def emit_same(ptr_value, expected):
        same = b.let(f"icmp eq ptr {ptr_value}, {expected}")
        emit(b.let(f"zext i1 {same} to i64"))

    # Instruction operands.
    for i in (0, 3, 7):
        emit(b.let(f"load i32, ptr {ELEM.format(i)}"), 32)
    emit(b.let("load i32, ptr getelementptr (i32, ptr getelementptr ([8 x i32], ptr @carr, i64 0, i64 6), i64 -4)"), 32)
    emit(b.let("load i32, ptr getelementptr (i32, ptr getelementptr ([8 x i32], ptr @carr, i64 1), i64 -8)"), 32)
    emit(b.let("load i32, ptr getelementptr inbounds (%Nest, ptr @nest, i32 0, i32 1, i32 1)"), 32)
    emit(b.let("load i16, ptr @nest"), 16)
    d = b.let("load double, ptr getelementptr inbounds (%Nest, ptr @nest, i32 0, i32 2, i32 1)")
    b.add(f"call void @emit_double(double {d})")
    emit(b.let("load i8, ptr getelementptr inbounds (%Nest, ptr @nest, i32 0, i32 2, i32 0)"), 8)
    emit(b.let("load i8, ptr getelementptr inbounds ([16 x i8], ptr @barr, i64 0, i64 15)"), 8)
    emit(b.let("load i8, ptr getelementptr (i8, ptr getelementptr inbounds ([16 x i8], ptr @barr, i64 0, i64 15), i64 -5)"), 8)
    # A store through a constant GEP, and the same address read another way.
    b.add("store i32 -77, ptr getelementptr ([8 x i32], ptr @carr, i64 0, i64 5)")
    emit(b.let("load i32, ptr getelementptr ([8 x i32], ptr @carr, i64 0, i64 5)"), 32)
    # Differences of addresses inside one global.
    emit(b.let(f"add {OFFSET.format(5)}, 0"))
    emit(b.let(f"add {OFFSET.format(7)}, 1"))
    emit(b.let("add i64 sub (i64 ptrtoint (ptr getelementptr inbounds ([16 x i8], ptr @barr, i64 0, i64 11) to i64), i64 ptrtoint (ptr @barr to i64)), 1000"))
    emit(b.let(f"trunc {OFFSET.format(6)} to i32"), 32)
    emit(b.let(f"trunc {OFFSET.format(6)} to i16"), 16)
    emit(b.let(f"trunc {OFFSET.format(6)} to i8"), 8)
    emit(b.let(f"mul {OFFSET.format(4)}, 3"))
    emit(b.let(f"udiv {OFFSET.format(7)}, 4"))
    # ptrtoint to narrow types and back.
    emit(b.let("add i32 ptrtoint (ptr getelementptr (i8, ptr null, i64 40) to i32), 1"), 32)
    emit(b.let("add i16 ptrtoint (ptr getelementptr (i8, ptr null, i64 -40) to i16), 1"), 16)
    emit(b.let("add i8 ptrtoint (ptr getelementptr (i8, ptr null, i64 -3) to i8), 1"), 8)
    emit(b.let("add i64 ptrtoint (ptr null to i64), 5"))
    emit(b.let("add i64 ptrtoint (ptr inttoptr (i64 -9 to ptr) to i64), 0"))
    emit(b.let("add i64 ptrtoint (ptr inttoptr (i32 -9 to ptr) to i64), 0"))
    emit(b.let("add i64 ptrtoint (ptr inttoptr (i8 -9 to ptr) to i64), 0"))
    wide = b.let("add i128 ptrtoint (ptr getelementptr (i8, ptr null, i64 -40) to i128), 1")
    emit(b.let(f"trunc i128 {wide} to i64"))
    emit(b.let("add i64 ptrtoint (ptr bitcast (ptr getelementptr (i8, ptr null, i64 24) to ptr) to i64), 0"))
    emit_same("inttoptr (i64 add (i64 ptrtoint (ptr @carr to i64), i64 8) to ptr)", ELEM.format(2))
    emit(b.let("load i32, ptr inttoptr (i64 add (i64 ptrtoint (ptr @carr to i64), i64 8) to ptr)"), 32)
    emit(b.let("load i32, ptr inttoptr (i64 sub (i64 ptrtoint (ptr getelementptr ([8 x i32], ptr @carr, i64 0, i64 4) to i64), i64 8) to ptr)"), 32)
    # Comparisons of addresses as operands.
    emit_same("@carr", "getelementptr ([8 x i32], ptr @carr, i64 0, i64 0)")
    emit_same("getelementptr ([8 x i32], ptr @carr, i64 0, i64 1)", "getelementptr (i32, ptr @carr, i64 1)")
    for pred in ("eq", "ne", "ult", "ule", "ugt", "uge", "slt", "sle", "sgt", "sge"):
        same = b.let(f"icmp {pred} ptr {ELEM.format(2)}, {ELEM.format(5)}")
        emit(b.let(f"zext i1 {same} to i64"))
        same = b.let(f"icmp {pred} ptr {ELEM.format(5)}, {ELEM.format(2)}")
        emit(b.let(f"zext i1 {same} to i64"))
    sel = b.let(f"select i1 true, ptr {ELEM.format(1)}, ptr {ELEM.format(2)}")
    emit(b.let(f"load i32, ptr {sel}"), 32)
    # Initialisers.
    for i in range(4):
        p = b.let(f"getelementptr [4 x i64], ptr @offsets, i64 0, i64 {i}")
        emit(b.let(f"load i64, ptr {p}"))
    for i in range(4):
        p = b.let(f"getelementptr [4 x i32], ptr @offsets32, i64 0, i64 {i}")
        emit(b.let(f"load i32, ptr {p}"), 32)
    for i in range(3):
        p = b.let(f"getelementptr [3 x i8], ptr @offsets8, i64 0, i64 {i}")
        emit(b.let(f"load i8, ptr {p}"), 8)
    for i in range(2):
        p = b.let(f"getelementptr [2 x i16], ptr @offsets_i16, i64 0, i64 {i}")
        emit(b.let(f"load i16, ptr {p}"), 16)
    for i in range(4):
        p = b.let(f"getelementptr [4 x ptr], ptr @ptr_table, i64 0, i64 {i}")
        q = b.let(f"load ptr, ptr {p}")
        emit(b.let(f"load i32, ptr {q}"), 32)
    q = b.let("load ptr, ptr @ptr_struct")
    d = b.let(f"load double, ptr {q}")
    b.add(f"call void @emit_double(double {d})")
    q = b.let("load ptr, ptr getelementptr inbounds ({ ptr, i32, ptr }, ptr @ptr_struct, i32 0, i32 2)")
    emit(b.let(f"load i8, ptr {q}"), 8)
    emit(b.let("load i32, ptr getelementptr inbounds ({ ptr, i32, ptr }, ptr @ptr_struct, i32 0, i32 1)"), 32)
    # The difference between the two pointers of the table is independent of the layout of memory.
    first = b.let("load ptr, ptr @ptr_table")
    last = b.let(f"load ptr, ptr getelementptr ([4 x ptr], ptr @ptr_table, i64 0, i64 2)")
    a = b.let(f"ptrtoint ptr {first} to i64")
    c = b.let(f"ptrtoint ptr {last} to i64")
    emit(b.let(f"sub i64 {c}, {a}"))
    v = b.let("load i64, ptr @addr_as_int")
    base = b.let("ptrtoint ptr @carr to i64")
    emit(b.let(f"sub i64 {v}, {base}"))
    v = b.let("load i64, ptr @addr_plus")
    emit(b.let(f"sub i64 {v}, {base}"))
    q = b.let("load ptr, ptr @addr_back")
    emit(b.let(f"load i32, ptr {q}"), 32)
    q = b.let("load ptr, ptr @addr_roundtrip")
    emit(b.let(f"load i32, ptr {q}"), 32)
    q = b.let("load ptr, ptr @int_as_ptr")
    emit(b.let(f"ptrtoint ptr {q} to i64"))
    q = b.let("load ptr, ptr @int_as_ptr_neg")
    emit(b.let(f"ptrtoint ptr {q} to i64"))
    q = b.let("load ptr, ptr @null_plus")
    emit(b.let(f"ptrtoint ptr {q} to i64"))
    emit(b.let("load i64, ptr @null_int"))
    functions = ["define void @run_constant_expressions() {"] + b.lines + ["  ret void", "}", ""]
    out += globals_ + functions + [main_function(["run_constant_expressions"])]
    return "\n".join(out) + "\n"


def main():
    folder = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "hand-written")
    for name, text in (("globals.ll", globals_file()), ("constant_expressions.ll", constant_expressions_file())):
        path = os.path.join(folder, name)
        with open(path, "w") as f:
            f.write(text)
        print(f"wrote {os.path.normpath(path)}: {text.count(chr(10))} lines")


if __name__ == "__main__":
    main()
