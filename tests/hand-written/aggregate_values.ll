; extractvalue and insertvalue on arrays and structs (as SSA values, not in memory): one index and
; several, nested aggregates, and a value that comes back from a call.

%Inner = type { i8, i64 }
%Outer = type { i32, %Inner, [3 x i16], double }

@fmt = private constant [8 x i8] c"%s: %ld\00"
@fmt_ld = private constant [9 x i8] c"%s: %ld\0A\00"
@fmt_lf = private constant [9 x i8] c"%s: %lf\0A\00"
@l_x = private constant [4 x i8] c"ext\00"
@l_i = private constant [4 x i8] c"ins\00"
@l_c = private constant [4 x i8] c"cal\00"

declare i32 @printf(ptr, ...)

define void @show(ptr %label, i64 %v) {
  %1 = call i32 (ptr, ...) @printf(ptr @fmt_ld, ptr %label, i64 %v)
  ret void
}

define %Outer @make(i32 %a, i64 %b) {
  %o0 = insertvalue %Outer undef, i32 %a, 0
  %o1 = insertvalue %Outer %o0, i64 %b, 1, 1
  %o2 = insertvalue %Outer %o1, i8 -5, 1, 0
  %o3 = insertvalue %Outer %o2, i16 300, 2, 1
  %o4 = insertvalue %Outer %o3, i16 -7, 2, 0
  %o5 = insertvalue %Outer %o4, i16 9, 2, 2
  %o6 = insertvalue %Outer %o5, double 2.5, 3
  ret %Outer %o6
}

define i32 @main() {
  %o = call %Outer @make(i32 -3, i64 1234567890123)
  %a = extractvalue %Outer %o, 0
  %ea = sext i32 %a to i64
  call void @show(ptr @l_c, i64 %ea)
  %b = extractvalue %Outer %o, 1, 1
  call void @show(ptr @l_c, i64 %b)
  %c = extractvalue %Outer %o, 1, 0
  %ec = sext i8 %c to i64
  call void @show(ptr @l_c, i64 %ec)
  %inner = extractvalue %Outer %o, 1
  %d = extractvalue %Inner %inner, 0
  %ed = sext i8 %d to i64
  call void @show(ptr @l_x, i64 %ed)
  %arr = extractvalue %Outer %o, 2
  %e0 = extractvalue [3 x i16] %arr, 0
  %e1 = extractvalue [3 x i16] %arr, 1
  %e2 = extractvalue [3 x i16] %arr, 2
  %s0 = sext i16 %e0 to i64
  %s1 = sext i16 %e1 to i64
  %s2 = sext i16 %e2 to i64
  call void @show(ptr @l_x, i64 %s0)
  call void @show(ptr @l_x, i64 %s1)
  call void @show(ptr @l_x, i64 %s2)
  %f = extractvalue %Outer %o, 3
  call i32 (ptr, ...) @printf(ptr @fmt_lf, ptr @l_x, double %f)

  ; replace members of a value that is already complete, and extract what was replaced and what was not
  %o2 = insertvalue %Outer %o, i32 77, 0
  %o3 = insertvalue %Outer %o2, i64 -1, 1, 1
  %o4 = insertvalue %Outer %o3, i16 1000, 2, 2
  %o5 = insertvalue %Outer %o4, double -0.25, 3
  %r0 = extractvalue %Outer %o5, 0
  %r1 = extractvalue %Outer %o5, 1, 1
  %r2 = extractvalue %Outer %o5, 1, 0
  %r3 = extractvalue %Outer %o5, 2, 2
  %r4 = extractvalue %Outer %o5, 2, 0
  %r5 = extractvalue %Outer %o5, 3
  %q0 = sext i32 %r0 to i64
  %q2 = sext i8 %r2 to i64
  %q3 = sext i16 %r3 to i64
  %q4 = sext i16 %r4 to i64
  call void @show(ptr @l_i, i64 %q0)
  call void @show(ptr @l_i, i64 %r1)
  call void @show(ptr @l_i, i64 %q2)
  call void @show(ptr @l_i, i64 %q3)
  call void @show(ptr @l_i, i64 %q4)
  call i32 (ptr, ...) @printf(ptr @fmt_lf, ptr @l_i, double %r5)

  ; the original is unchanged
  %orig = extractvalue %Outer %o, 0
  %eo = sext i32 %orig to i64
  call void @show(ptr @l_x, i64 %eo)

  ; array of arrays
  %m0 = insertvalue [2 x [2 x i32]] zeroinitializer, i32 5, 0, 1
  %m1 = insertvalue [2 x [2 x i32]] %m0, i32 6, 1, 0
  %m2 = insertvalue [2 x [2 x i32]] %m1, i32 7, 1, 1
  %x01 = extractvalue [2 x [2 x i32]] %m2, 0, 1
  %x10 = extractvalue [2 x [2 x i32]] %m2, 1, 0
  %x11 = extractvalue [2 x [2 x i32]] %m2, 1, 1
  %x00 = extractvalue [2 x [2 x i32]] %m2, 0, 0
  %t = add i32 %x01, %x10
  %t2 = mul i32 %t, %x11
  %t3 = add i32 %t2, %x00
  %et = sext i32 %t3 to i64
  call void @show(ptr @l_i, i64 %et)
  ret i32 0
}
