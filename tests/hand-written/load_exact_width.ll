; exit-code: 0
; Loading an i24 / i40 from the last bytes of a global gives the right value, zero- and sign-extended.
; Consumers normalise, so an over-read would not change these results: this checks the value, not the
; number of bytes read (which only shows up as a fault past the end of a mapping).

@a = global [3 x i8] c"\FF\FF\FF", align 1
@b = global [5 x i8] c"\01\02\03\04\05", align 1
@guard = global [8 x i8] c"\55\55\55\55\55\55\55\55", align 1

define i32 @main() {
  %x = load i24, ptr @a, align 1
  %xz = zext i24 %x to i32
  %ok1 = icmp eq i32 %xz, 16777215

  %y = load i40, ptr @b, align 1
  %yz = zext i40 %y to i64
  %ok2 = icmp eq i64 %yz, 21542142465

  ; sext of a loaded i24 with the sign bit set
  %xs = sext i24 %x to i32
  %ok3 = icmp eq i32 %xs, -1

  %and1 = and i1 %ok1, %ok2
  %all = and i1 %and1, %ok3
  %res = select i1 %all, i32 0, i32 1
  ret i32 %res
}
