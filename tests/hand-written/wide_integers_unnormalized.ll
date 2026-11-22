; exit-code: 0
; An i72 is held in an Int128 whose bits above bit 71 are not defined: add, sub and sext leave stray high bits
; (and a negative constant is sign-extended), so every consumer has to read the logical i72 value.
; Each cN returns 1 when it is right; main returns the 1-based number of the first check that failed (0 if none).

define i72 @id72(i72 %v) {
  ret i72 %v
}
define i128 @id128(i128 %v) {
  ret i128 %v
}

; 0 - 1 = i72 -1, with all 128 bits set
define i32 @c1() {
  %a = call i72 @id72(i72 0)
  %v = sub i72 %a, 1
  %m = lshr i72 %v, 64
  %t = trunc i72 %m to i32
  %ok = icmp eq i32 %t, 255
  %res = zext i1 %ok to i32
  ret i32 %res
}
; ashr of the i72 -2^71 (bit 71 is its sign bit)
define i32 @c2() {
  %a = call i72 @id72(i72 1)
  %v = shl i72 %a, 71
  %s = ashr i72 %v, 70
  %ok = icmp eq i72 %s, -2
  %res = zext i1 %ok to i32
  ret i32 %res
}
; signed compare of an i72 whose bit 71 is set
define i32 @c3() {
  %a = call i72 @id72(i72 1)
  %v = shl i72 %a, 71
  %ok = icmp slt i72 %v, 0
  %res = zext i1 %ok to i32
  ret i32 %res
}
; unsigned compare, stray high bits
define i32 @c4() {
  %a = call i72 @id72(i72 0)
  %v = sub i72 %a, 1
  %ok = icmp ugt i72 %v, 5
  %res = zext i1 %ok to i32
  ret i32 %res
}
; eq against a constant, stray high bits
define i32 @c5() {
  %a = call i72 @id72(i72 0)
  %v = sub i72 %a, 1
  %ok = icmp eq i72 %v, 4722366482869645213695
  %res = zext i1 %ok to i32
  ret i32 %res
}
; zext i72 -> i128
define i32 @c6() {
  %a = call i72 @id72(i72 0)
  %v = sub i72 %a, 1
  %z = zext i72 %v to i128
  %m = lshr i128 %z, 64
  %t = trunc i128 %m to i32
  %ok = icmp eq i32 %t, 255
  %res = zext i1 %ok to i32
  ret i32 %res
}
; sext i72 -> i128
define i32 @c7() {
  %a = call i72 @id72(i72 1)
  %v = shl i72 %a, 71
  %z = sext i72 %v to i128
  %m = lshr i128 %z, 100
  %t = trunc i128 %m to i32
  %ok = icmp eq i32 %t, 268435455
  %res = zext i1 %ok to i32
  ret i32 %res
}
; trunc i128 -> i72, then zext back
define i32 @c8() {
  %a = call i128 @id128(i128 -1)
  %v = trunc i128 %a to i72
  %z = zext i72 %v to i128
  %m = lshr i128 %z, 64
  %t = trunc i128 %m to i32
  %ok = icmp eq i32 %t, 255
  %res = zext i1 %ok to i32
  ret i32 %res
}
; a negative i72 constant, lshr
define i32 @c9() {
  %a = call i72 @id72(i72 -1)
  %m = lshr i72 %a, 64
  %t = trunc i72 %m to i32
  %ok = icmp eq i32 %t, 255
  %res = zext i1 %ok to i32
  ret i32 %res
}
; branch on a wide compare, stray high bits
define i32 @c10() {
  %a = call i72 @id72(i72 0)
  %v = sub i72 %a, 1
  %c = icmp sgt i72 %v, -2
  br i1 %c, label %yes, label %no
yes:
  ret i32 1
no:
  ret i32 0
}
; shift by an amount with stray high bits: 0 - (2^72 - 8) is 8
define i32 @c11() {
  %a = call i72 @id72(i72 255)
  %k = call i72 @id72(i72 0)
  %amt = sub i72 %k, 4722366482869645213688
  %z = shl i72 %a, %amt
  %ok = icmp eq i72 %z, 65280
  %res = zext i1 %ok to i32
  ret i32 %res
}

define i32 @main() {
  %r1 = call i32 @c1()
  %r2 = call i32 @c2()
  %r3 = call i32 @c3()
  %r4 = call i32 @c4()
  %r5 = call i32 @c5()
  %r6 = call i32 @c6()
  %r7 = call i32 @c7()
  %r8 = call i32 @c8()
  %r9 = call i32 @c9()
  %r10 = call i32 @c10()
  %r11 = call i32 @c11()
  %f11 = icmp eq i32 %r11, 1
  %n11 = select i1 %f11, i32 0, i32 11
  %f10 = icmp eq i32 %r10, 1
  %n10 = select i1 %f10, i32 %n11, i32 10
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
