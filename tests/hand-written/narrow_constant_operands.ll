; LLVM folds an instruction whose operands are all constants, so the constants of c1, c2, c3, c7, c8 and c10
; go through a call; the others combine one with an argument.
; A negative constant of a narrow integer is not masked to its width: the bits above N are sign
; copies, so every consumer has to read the logical iN value. each cN returns 1 when it is right, and main
; returns the 1-based number of the first check that failed (0 if none).

define i24 @idn(i24 %v) {
  ret i24 %v
}
define i1 @id1(i1 %v) {
  ret i1 %v
}
define i40 @id40(i40 %v) {
  ret i40 %v
}
define i72 @id72(i72 %v) {
  ret i72 %v
}

define i32 @c1() {
  %v = call i24 @idn(i24 -1)
  %r = zext i24 %v to i32
  %ok = icmp eq i32 %r, 16777215
  %res = zext i1 %ok to i32
  ret i32 %res
}
define i32 @c2() {
  %v = call i24 @idn(i24 -8388608)
  %r = sext i24 %v to i32
  %ok = icmp eq i32 %r, -8388608
  %res = zext i1 %ok to i32
  ret i32 %res
}
define i32 @c3() {
  %v = call i24 @idn(i24 -1)
  %r = uitofp i24 %v to double
  %ok = fcmp oeq double %r, 16777215.0
  %res = zext i1 %ok to i32
  ret i32 %res
}
define i32 @c4(i24 %x) {
  %ok = icmp ult i24 %x, -1
  %res = zext i1 %ok to i32
  ret i32 %res
}
define i32 @c5(i24 %x) {
  %q = udiv i24 -1, %x
  %r = zext i24 %q to i32
  %ok = icmp eq i32 %r, 8388607
  %res = zext i1 %ok to i32
  ret i32 %res
}
define i32 @c6(i24 %x) {
  %q = lshr i24 -1, %x
  %r = zext i24 %q to i32
  %ok = icmp eq i32 %r, 16777215
  %res = zext i1 %ok to i32
  ret i32 %res
}
define i32 @c7() {
  %v = call i40 @id40(i40 -1)
  %r = zext i40 %v to i64
  %ok = icmp eq i64 %r, 1099511627775
  %res = zext i1 %ok to i32
  ret i32 %res
}
define i32 @c8() {
  %v = call i1 @id1(i1 true)
  %r = zext i1 %v to i32
  %ok = icmp eq i32 %r, 1
  %res = zext i1 %ok to i32
  ret i32 %res
}
define i32 @c9(i1 %c) {
  %s = select i1 %c, i24 -1, i24 5
  %r = zext i24 %s to i32
  %ok = icmp eq i32 %r, 16777215
  %res = zext i1 %ok to i32
  ret i32 %res
}
define i32 @c10() {
  %v = call i72 @id72(i72 -1)
  %m = lshr i72 %v, 64
  %t = trunc i72 %m to i32
  %ok = icmp eq i32 %t, 255
  %res = zext i1 %ok to i32
  ret i32 %res
}

define i32 @main() {
  %r1 = call i32 @c1()
  %r2 = call i32 @c2()
  %r3 = call i32 @c3()
  %r4 = call i32 @c4(i24 1)
  %r5 = call i32 @c5(i24 2)
  %r6 = call i32 @c6(i24 0)
  %r7 = call i32 @c7()
  %r8 = call i32 @c8()
  %r9 = call i32 @c9(i1 true)
  %r10 = call i32 @c10()
  %f10 = icmp eq i32 %r10, 1
  %n10 = select i1 %f10, i32 0, i32 10
  %f9 = icmp eq i32 %r9, 1
  %n9 = select i1 %f9, i32 %n10, i32 9
  %f8 = icmp eq i32 %r8, 1
  %n8 = select i1 %f8, i32 %n9, i32 8
  %f7 = icmp eq i32 %r7, 1
  %n7 = select i1 %f7, i32 %n8, i32 7
  %f6 = icmp eq i32 %r6, 1
  %n6 = select i1 %f6, i32 %n7, i32 6
  %f5 = icmp eq i32 %r5, 1
  %n5 = select i1 %f5, i32 %n6, i32 5
  %f4 = icmp eq i32 %r4, 1
  %n4 = select i1 %f4, i32 %n5, i32 4
  %f3 = icmp eq i32 %r3, 1
  %n3 = select i1 %f3, i32 %n4, i32 3
  %f2 = icmp eq i32 %r2, 1
  %n2 = select i1 %f2, i32 %n3, i32 2
  %f1 = icmp eq i32 %r1, 1
  %n1 = select i1 %f1, i32 %n2, i32 1
  ret i32 %n1
}
