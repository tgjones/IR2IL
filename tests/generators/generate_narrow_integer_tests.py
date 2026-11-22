#!/usr/bin/env python3
"""Generates tests/hand-written/narrow_integers_i<N>.ll.

An integer narrower than 32 bits (i1, i8, i16, ...) lives on the IL stack in an int32, and an integer
that is not a power of two wide (i1, i24, i40, ...) lives in a bigger container. Whichever instruction
produced the value, every consumer has to read the *logical* iN value. A value can arrive:

  * zero-extended        (`trunc`)
  * sign-extended        (`load`, `call`, `fptosi`)
  * with stray high bits (`add`, `mul`, `sub`, `xor`: the result is only narrowed when it is stored)

Each generated function crosses one such producer with one consumer (zext, sext, sitofp, sdiv, icmp,
switch, select, call, phi, ...) and returns 1 when the result is what LLVM defines, 0 otherwise.
`main` returns the 1-based number of the first function that returned something else (0 if none); the
comment above each function names the producer and consumer.

Usage:
    generate_narrow_integer_tests.py            rewrite the files in tests/hand-written
    generate_narrow_integer_tests.py N --debug  print the .ll for iN, whose main prints every failing number
"""

import os
import re
import sys

WIDTHS = [1, 8, 16, 24, 40]  # i24 is held in an int32, i40 in an int64


def wrap(width, value):
    """The signed value of the low `width` bits of `value`."""
    value &= (1 << width) - 1
    return value - (1 << width) if value >> (width - 1) else value


def trunc_div(a, b):
    quotient = abs(a) // abs(b)
    return quotient if (a < 0) == (b < 0) else -quotient


class Suite:
    def __init__(self, width, wide_type):
        self.width = width
        self.wide = wide_type  # type of the surrounding values: i32, or i64 when iN is wider than 32 bits
        self.names = []
        self.functions = []

    def add(self, name, body):
        self.names.append(name)
        self.functions.append(f"; {len(self.names)}: {name}\ndefine i32 @t{len(self.names) - 1}() {{\n{body}}}\n")

    def check(self, value, expected):
        return (f"  %ok = icmp eq {self.wide} {value}, {expected}\n"
                f"  %res = zext i1 %ok to i32\n  ret i32 %res\n")

    def render(self, header, debug=False):
        n, w, x = len(self.names), self.width, self.wide
        out = [header, "",
               f"define {x} @id({x} %x) {{\n  ret {x} %x\n}}",
               f"define i{w} @idn(i{w} %v) {{\n  ret i{w} %v\n}}",
               f"define {x} @argzext(i{w} %v) {{\n  %r = zext i{w} %v to {x}\n  ret {x} %r\n}}",
               f"define {x} @argsext(i{w} %v) {{\n  %r = sext i{w} %v to {x}\n  ret {x} %r\n}}", ""]
        out.extend(self.functions)
        if debug:
            out.append('@fmt = private constant [4 x i8] c"%d\\0A\\00"\ndeclare i32 @printf(ptr, ...)')
        out.append("define i32 @main() {")
        for i in range(n):
            fail = (f"  call i32 (ptr, ...) @printf(ptr @fmt, i32 {i + 1})\n  br label %n{i}" if debug
                    else f"  ret i32 {i + 1}")
            out.append(f"  %r{i} = call i32 @t{i}()\n  %c{i} = icmp eq i32 %r{i}, 1\n"
                       f"  br i1 %c{i}, label %n{i}, label %fail{i}\nfail{i}:\n{fail}\nn{i}:")
        out.append("  ret i32 0\n}")
        return "\n".join(out) + "\n"


def rename_producer_locals(body):
    """Everything but %v gets a prefix, so it can't clash with the consumer's names."""
    return re.sub(r"%(?!v\b)(\w+)", r"%p_\1", body)


def make_wide(width):
    """i8, i16, i24, i40: the value under test is the signed minimum, 0b100...0."""
    x = "i32" if width <= 32 else "i64"
    t = f"i{width}"
    mask = (1 << width) - 1
    signed_min = -(1 << (width - 1))  # s
    unsigned = 1 << (width - 1)  # u: the same bits, read as unsigned
    signed_max = unsigned - 1
    half = unsigned // 2
    s, u = signed_min, unsigned
    suite = Suite(width, x)

    def trunc(source):
        return f"  %x = call {x} @id({x} {source})\n  %v = trunc {x} %x to {t}\n"

    producers = {
        "trunc": trunc(u),
        "trunc-high-bits": trunc(u + (1 << width)),
        "add": f"  %v = add {t} {signed_max}, 1\n",
        "mul": f"  %v = mul {t} {half}, 2\n",
        "sub": f"  %v = sub {t} 0, {s}\n",
        "xor": trunc(mask - u).replace("%v = trunc", "%t = trunc") + f"  %v = xor {t} %t, -1\n",
        "load": f"  %p = alloca {t}\n  store {t} {s}, ptr %p\n  %v = load {t}, ptr %p\n",
        "fptosi": f"  %y = call {x} @id({x} {s})\n  %d = sitofp {x} %y to double\n  %v = fptosi double %d to {t}\n",
        "select": trunc(u).replace("%v = trunc", "%t = trunc") + f"  %v = select i1 true, {t} %t, {t} 0\n",
        "call": f"  %v = call {t} @idn({t} {s})\n",
    }

    consumers = {}  # name -> (body, expected); expected None: the body returns by itself

    def consume(name, body, expected):
        consumers[name] = (body, expected)

    def compare(name, predicate, rhs, truth):
        consume(f"icmp-{name}", f"  %b = icmp {predicate} {t} %v, {rhs}\n  %r = zext i1 %b to {x}\n", int(truth))

    consume("zext", f"  %r = zext {t} %v to {x}\n", u)
    consume("sext", f"  %r = sext {t} %v to {x}\n", s)
    consume("sitofp-double", f"  %d = sitofp {t} %v to double\n  %r = fptosi double %d to {x}\n", s)
    consume("uitofp-float", f"  %d = uitofp {t} %v to float\n  %r = fptosi float %d to {x}\n", u)
    consume("sdiv", f"  %q = sdiv {t} %v, 2\n  %r = sext {t} %q to {x}\n", wrap(width, trunc_div(s, 2)))
    consume("udiv", f"  %q = udiv {t} %v, 2\n  %r = zext {t} %q to {x}\n", u // 2)
    consume("srem", f"  %q = srem {t} %v, 3\n  %r = sext {t} %q to {x}\n", s - 3 * trunc_div(s, 3))
    consume("urem", f"  %q = urem {t} %v, 3\n  %r = zext {t} %q to {x}\n", u % 3)
    consume("lshr", f"  %q = lshr {t} %v, 1\n  %r = zext {t} %q to {x}\n", u >> 1)
    consume("ashr", f"  %q = ashr {t} %v, 1\n  %r = sext {t} %q to {x}\n", s >> 1)
    consume("shl", f"  %q = shl {t} %v, 1\n  %r = zext {t} %q to {x}\n", 0)
    consume("shl-by-variable",
            f"  %a = call {x} @id({x} 1)\n  %am = trunc {x} %a to {t}\n  %q = shl {t} %v, %am\n  %r = zext {t} %q to {x}\n", 0)
    consume("mul", f"  %q = mul {t} %v, 3\n  %r = zext {t} %q to {x}\n", (u * 3) & mask)
    consume("xor-not", f"  %q = xor {t} %v, -1\n  %r = zext {t} %q to {x}\n", (~u) & mask)
    consume("select-value", f"  %q = select i1 true, {t} %v, {t} 0\n  %r = zext {t} %q to {x}\n", u)
    consume("store-load", f"  %p2 = alloca {t}\n  store {t} %v, ptr %p2\n  %q = load {t}, ptr %p2\n  %r = zext {t} %q to {x}\n", u)
    consume("call-arg-zext", f"  %r = call {x} @argzext({t} %v)\n", u)
    consume("call-arg-sext", f"  %r = call {x} @argsext({t} %v)\n", s)
    consume("call-arg-ret", f"  %q = call {t} @idn({t} %v)\n  %r = zext {t} %q to {x}\n", u)
    compare("eq", "eq", s, True)
    compare("ne", "ne", s, False)
    compare("slt-0", "slt", 0, True)
    compare("sgt-0", "sgt", 0, False)
    compare("sge", "sge", s, True)
    compare("sle", "sle", s, True)
    compare("ult-max", "ult", signed_max, False)
    compare("ugt-max", "ugt", signed_max, True)
    compare("ule", "ule", s, True)
    compare("uge", "uge", s, True)
    compare("self-slt", "slt", "%v", False)
    consume("br-icmp", f"  %b = icmp slt {t} %v, 0\n  br i1 %b, label %yes, label %no\nyes:\n  ret i32 1\nno:\n  ret i32 0\n", None)
    consume("select-cond-icmp", f"  %b = icmp ugt {t} %v, {signed_max}\n  %r = select i1 %b, {x} 7, {x} 8\n", 7)
    consume("phi", f"  br label %a\na:\n  br label %b\nb:\n  %q = phi {t} [ %v, %a ]\n  %r = zext {t} %q to {x}\n", u)
    if width <= 32:  # switch on wider conditions isn't implemented
        consume("switch-single", f"  switch {t} %v, label %def [\n    {t} {s}, label %yes\n  ]\nyes:\n  ret i32 1\ndef:\n  ret i32 0\n", None)
        consume("switch-cluster",
                f"  switch {t} %v, label %def [\n    {t} {s}, label %yes\n    {t} {s + 1}, label %yes\n  ]\nyes:\n  ret i32 1\ndef:\n  ret i32 0\n", None)
        consume("switch-two-clusters",
                f"  switch {t} %v, label %def [\n    {t} 0, label %no\n    {t} {s}, label %yes\n  ]\nyes:\n  ret i32 1\nno:\n  ret i32 0\ndef:\n  ret i32 0\n", None)

    for producer_name, producer in producers.items():
        producer = rename_producer_locals(producer)
        for consumer_name, (body, expected) in consumers.items():
            suite.add(f"{producer_name} -> {consumer_name}",
                      producer + body + (suite.check("%r", expected) if expected is not None else ""))

    header = (f"; exit-code: 0\n"
              f"; Generated by tests/generators/generate_narrow_integer_tests.py: every way of producing an i{width} holding {s}\n"
              f"; (0b1 followed by zeros) against every consumer of a narrow integer. See the generator for the details.")
    return suite, header


def make_i1():
    """An i1 is held in a byte that must be 0 or 1 whatever produced it, but `sext` makes true -1."""
    suite = Suite(1, "i32")

    def trunc(source):
        return f"  %x = call i32 @id(i32 {source})\n  %v = trunc i32 %x to i1\n"

    def via_temp(source, rest):
        return trunc(source).replace("%v = trunc", "%t = trunc") + rest

    # (name, truth, body producing %v)
    producers = [
        ("trunc-3", 1, trunc(3)), ("trunc-255", 1, trunc(255)), ("trunc-minus-1", 1, trunc(-1)),
        ("trunc-2", 0, trunc(2)), ("trunc-254", 0, trunc(254)), ("trunc-256", 0, trunc(256)),
        ("trunc-i64-3", 1, "  %x = zext i32 3 to i64\n  %v = trunc i64 %x to i1\n"),
        ("add-1-1", 0, "  %v = add i1 1, 1\n"), ("add-1-0", 1, "  %v = add i1 1, 0\n"),
        ("sub-0-1", 1, "  %v = sub i1 0, 1\n"), ("mul-1-1", 1, "  %v = mul i1 1, 1\n"),
        ("not-trunc-2", 1, via_temp(2, "  %v = xor i1 %t, true\n")),
        ("not-trunc-3", 0, via_temp(3, "  %v = xor i1 %t, true\n")),
        ("and-trunc-3", 1, via_temp(3, "  %v = and i1 %t, true\n")),
        ("or-trunc-2", 0, via_temp(2, "  %v = or i1 %t, false\n")),
        ("load-true", 1, "  %p = alloca i1\n  store i1 true, ptr %p\n  %v = load i1, ptr %p\n"),
        ("load-false", 0, "  %p = alloca i1\n  store i1 false, ptr %p\n  %v = load i1, ptr %p\n"),
        ("store-trunc-3-load", 1, via_temp(3, "  %p = alloca i1\n  store i1 %t, ptr %p\n  %v = load i1, ptr %p\n")),
        ("select-trunc-3", 1, via_temp(3, "  %v = select i1 true, i1 %t, i1 false\n")),
        ("select-trunc-2", 0, via_temp(2, "  %v = select i1 true, i1 %t, i1 true\n")),
        ("icmp", 1, "  %v = icmp eq i32 1, 1\n"),
        ("fcmp", 0, "  %v = fcmp olt double 2.0, 1.0\n"),
        ("call", 1, "  %v = call i1 @idn(i1 true)\n"),
    ]

    def returns(t_yes, t_no):
        return f"yes:\n  ret i32 {t_yes}\nno:\n  ret i32 {t_no}\n"

    consumers = [  # (name, truth -> (body, expected)); expected None: the body returns by itself
        ("zext", lambda t: ("  %r = zext i1 %v to i32\n", t)),
        ("sext", lambda t: ("  %r = sext i1 %v to i32\n", -t)),
        ("sext-i8", lambda t: ("  %q = sext i1 %v to i8\n  %r = zext i8 %q to i32\n", 255 * t)),
        ("sext-i64", lambda t: ("  %q = sext i1 %v to i64\n  %r = trunc i64 %q to i32\n", -t)),
        ("sitofp-double", lambda t: ("  %d = sitofp i1 %v to double\n  %r = fptosi double %d to i32\n", -t)),
        ("uitofp-float", lambda t: ("  %d = uitofp i1 %v to float\n  %r = fptosi float %d to i32\n", t)),
        ("select-cond", lambda t: ("  %r = select i1 %v, i32 7, i32 8\n", 7 if t else 8)),
        ("select-value", lambda t: ("  %q = select i1 true, i1 %v, i1 false\n  %r = zext i1 %q to i32\n", t)),
        ("br", lambda t: (f"  br i1 %v, label %yes, label %no\n{returns(t, 1 - t)}", None)),
        ("and-true", lambda t: ("  %q = and i1 %v, true\n  %r = zext i1 %q to i32\n", t)),
        ("xor-true", lambda t: ("  %q = xor i1 %v, true\n  %r = zext i1 %q to i32\n", 1 - t)),
        ("xor-self", lambda t: ("  %q = xor i1 %v, %v\n  %r = zext i1 %q to i32\n", 0)),
        ("not-then-br", lambda t: (f"  %q = xor i1 %v, true\n  br i1 %q, label %yes, label %no\n{returns(1 - t, t)}", None)),
        ("icmp-eq-true", lambda t: ("  %b = icmp eq i1 %v, true\n  %r = zext i1 %b to i32\n", t)),
        ("icmp-eq-false", lambda t: ("  %b = icmp eq i1 %v, false\n  %r = zext i1 %b to i32\n", 1 - t)),
        ("icmp-ult-true", lambda t: ("  %b = icmp ult i1 %v, true\n  %r = zext i1 %b to i32\n", 1 - t)),
        ("icmp-ugt-false", lambda t: ("  %b = icmp ugt i1 %v, false\n  %r = zext i1 %b to i32\n", t)),
        ("icmp-slt-false", lambda t: ("  %b = icmp slt i1 %v, false\n  %r = zext i1 %b to i32\n", t)),  # true is -1
        ("icmp-sgt-true", lambda t: ("  %b = icmp sgt i1 %v, true\n  %r = zext i1 %b to i32\n", 1 - t)),  # false (0) > true (-1)
        ("br-icmp-eq-true", lambda t: (f"  %b = icmp eq i1 %v, true\n  br i1 %b, label %yes, label %no\n{returns(t, 1 - t)}", None)),
        ("switch-single", lambda t: (f"  switch i1 %v, label %no [\n    i1 true, label %yes\n  ]\n{returns(t, 1 - t)}", None)),
        ("switch-both", lambda t: (f"  switch i1 %v, label %def [\n    i1 false, label %no\n    i1 true, label %yes\n  ]\n{returns(t, 1 - t)}def:\n  ret i32 2\n", None)),
        ("store-load", lambda t: ("  %p2 = alloca i1\n  store i1 %v, ptr %p2\n  %q = load i1, ptr %p2\n  %r = zext i1 %q to i32\n", t)),
        ("call-arg-zext", lambda t: ("  %r = call i32 @argzext(i1 %v)\n", t)),
        ("call-arg-sext", lambda t: ("  %r = call i32 @argsext(i1 %v)\n", -t)),
        ("call-arg-ret", lambda t: ("  %q = call i1 @idn(i1 %v)\n  %r = zext i1 %q to i32\n", t)),
        ("phi", lambda t: ("  br label %a\na:\n  br label %b\nb:\n  %q = phi i1 [ %v, %a ]\n  %r = zext i1 %q to i32\n", t)),
    ]

    for producer_name, truth, producer in producers:
        producer = rename_producer_locals(producer)
        for consumer_name, make in consumers:
            body, expected = make(truth)
            suite.add(f"{producer_name} (i1 {truth}) -> {consumer_name}",
                      producer + body + (suite.check("%r", expected) if expected is not None else ""))

    header = ("; exit-code: 0\n"
              "; Generated by tests/generators/generate_narrow_integer_tests.py: every way of producing an i1 against every\n"
              "; consumer of one. An i1 must read as 0 or 1 whatever produced it (`trunc i32 2 to i1` is false), and as -1 or 0\n"
              "; when it is read as signed (`sext i1 true to i32` is -1). See the generator for the details.")
    return suite, header


def build(width):
    return make_i1() if width == 1 else make_wide(width)


if __name__ == "__main__":
    if len(sys.argv) > 1:
        suite, header = build(int(sys.argv[1]))
        sys.stdout.write(suite.render(header, debug="--debug" in sys.argv))
        if "--debug" in sys.argv:
            with open(f"/tmp/narrow_names_i{sys.argv[1]}.txt", "w") as names:
                names.write("\n".join(f"{i + 1}: {name}" for i, name in enumerate(suite.names)))
    else:
        directory = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "hand-written")
        for width in WIDTHS:
            suite, header = build(width)
            with open(os.path.join(directory, f"narrow_integers_i{width}.ll"), "w", newline="\n") as out:
                out.write(suite.render(header))
