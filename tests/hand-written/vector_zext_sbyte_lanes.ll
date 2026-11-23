; exit-code: 0
; zext of a vector whose lanes are i8 must zero-extend: 0x80 is 128, not -128 (and not 0xFFFFFF80).
; The VectorUtility.ZExt helpers used to cast an sbyte lane straight to the wider type, which sign-extends.
; main returns the 1-based number of the first check that failed, or 0.

define i32 @main() {
  %v4 = bitcast <4 x i8> <i8 -128, i8 -1, i8 1, i8 127> to <4 x i8>

  ; <4 x i8> to <4 x i16>
  %a = zext <4 x i8> %v4 to <4 x i16>
  %a0 = extractelement <4 x i16> %a, i64 0
  %a1 = extractelement <4 x i16> %a, i64 1
  %a2 = extractelement <4 x i16> %a, i64 2
  %a3 = extractelement <4 x i16> %a, i64 3
  %ok_a0 = icmp eq i16 %a0, 128
  br i1 %ok_a0, label %c2, label %fail1
c2:
  %ok_a1 = icmp eq i16 %a1, 255
  br i1 %ok_a1, label %c3, label %fail2
c3:
  %ok_a2 = icmp eq i16 %a2, 1
  %ok_a3 = icmp eq i16 %a3, 127
  %ok_a23 = and i1 %ok_a2, %ok_a3
  br i1 %ok_a23, label %c4, label %fail3

  ; <4 x i8> to <4 x i32>
c4:
  %b = zext <4 x i8> %v4 to <4 x i32>
  %b0 = extractelement <4 x i32> %b, i64 0
  %b1 = extractelement <4 x i32> %b, i64 1
  %b2 = extractelement <4 x i32> %b, i64 2
  %ok_b0 = icmp eq i32 %b0, 128
  br i1 %ok_b0, label %c5, label %fail4
c5:
  %ok_b1 = icmp eq i32 %b1, 255
  %ok_b2 = icmp eq i32 %b2, 1
  %ok_b12 = and i1 %ok_b1, %ok_b2
  br i1 %ok_b12, label %c6, label %fail5

  ; <2 x i8> to <2 x i64>
c6:
  %v2 = bitcast <2 x i8> <i8 -128, i8 -1> to <2 x i8>
  %c = zext <2 x i8> %v2 to <2 x i64>
  %c0 = extractelement <2 x i64> %c, i64 0
  %c1 = extractelement <2 x i64> %c, i64 1
  %ok_c0 = icmp eq i64 %c0, 128
  br i1 %ok_c0, label %c7, label %fail6
c7:
  %ok_c1 = icmp eq i64 %c1, 255
  br i1 %ok_c1, label %pass, label %fail7

pass:
  ret i32 0
fail1:
  ret i32 1
fail2:
  ret i32 2
fail3:
  ret i32 3
fail4:
  ret i32 4
fail5:
  ret i32 5
fail6:
  ret i32 6
fail7:
  ret i32 7
}
