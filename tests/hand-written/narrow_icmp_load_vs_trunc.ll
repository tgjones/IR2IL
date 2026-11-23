; A sign-extended `load i8`/`load i16` and a zero-extended `trunc` of the same bit pattern (0x80 / 0x8000)
; must compare equal, whichever way the comparison is emitted. Both operands are left on the IL stack
; (the load is deferred, the trunc is used by the next instruction), so nothing normalises them.
; 1 + 2 + 4 for the i8 comparisons, 8 + 16 + 32 for the i16 ones.

define i32 @id(i32 %x) {
  ret i32 %x
}

define i32 @cmp8() {
  %p = alloca i8
  store i8 -128, ptr %p
  %l = load i8, ptr %p
  %x = call i32 @id(i32 128)
  %t = trunc i32 %x to i8
  %eq = icmp eq i8 %l, %t
  %uge = icmp uge i8 %l, %t
  %sge = icmp sge i8 %l, %t
  %a = zext i1 %eq to i32
  %b = zext i1 %uge to i32
  %b2 = shl i32 %b, 1
  %c = zext i1 %sge to i32
  %c2 = shl i32 %c, 2
  %r1 = add i32 %a, %b2
  %r = add i32 %r1, %c2
  ret i32 %r
}

define i32 @branch16() {
  %p = alloca i16
  store i16 -32768, ptr %p
  %l = load i16, ptr %p
  %x = call i32 @id(i32 32768)
  %t = trunc i32 %x to i16
  %eq = icmp eq i16 %l, %t
  br i1 %eq, label %next1, label %fail

next1:
  %l2 = load i16, ptr %p
  %t2 = trunc i32 %x to i16
  %uge = icmp uge i16 %l2, %t2
  br i1 %uge, label %next2, label %fail

next2:
  %l3 = load i16, ptr %p
  %t3 = trunc i32 %x to i16
  %sge = icmp sge i16 %l3, %t3
  br i1 %sge, label %ok, label %fail

ok:
  ret i32 56

fail:
  ret i32 0
}

define i32 @main() {
  %a = call i32 @cmp8()
  %b = call i32 @branch16()
  %r = add i32 %a, %b
  ret i32 %r
}
