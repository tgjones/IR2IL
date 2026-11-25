; getelementptr over structs and arrays: nested indices, variable indices, constant-expression GEPs
; (including a negative offset, which has to be sign-extended) and `ptrtoint`/`inttoptr` round trips.
; Every address stays inside its object, except for the pointer arithmetic of the negative offset,
; which is brought back into the object before it is dereferenced.

%Inner = type { i8, double }
%S = type { i8, i32, i64, [3 x i16], %Inner, [2 x %Inner] }

@s = global %S { i8 1, i32 2, i64 3, [3 x i16] [i16 4, i16 5, i16 6], %Inner { i8 7, double 8.5 },
                 [2 x %Inner] [%Inner { i8 9, double 10.5 }, %Inner { i8 11, double 12.5 }] }
@a = global [8 x i32] [i32 10, i32 11, i32 12, i32 13, i32 14, i32 15, i32 16, i32 17]
@m = global [3 x [4 x i32]] [[4 x i32] [i32 0, i32 1, i32 2, i32 3], [4 x i32] [i32 100, i32 101, i32 102, i32 103], [4 x i32] [i32 200, i32 201, i32 202, i32 203]]


declare i32 @printf(ptr, ...)


define void @show(ptr %label, i64 %v) {
  %1 = call i32 (ptr, ...) @printf(ptr @fmt_ld, ptr %label, i64 %v)
  ret void
}
@fmt_ld = private constant [9 x i8] c"%s: %ld\0A\00"
@fmt_lf = private constant [9 x i8] c"%s: %lf\0A\00"

@l_a = private constant [4 x i8] c"i8\00\00"
@l_b = private constant [4 x i8] c"i32\00"
@l_c = private constant [4 x i8] c"i64\00"
@l_d = private constant [4 x i8] c"arr\00"
@l_e = private constant [4 x i8] c"dbl\00"
@l_f = private constant [4 x i8] c"mat\00"
@l_g = private constant [4 x i8] c"neg\00"
@l_h = private constant [4 x i8] c"cst\00"
@l_i = private constant [4 x i8] c"p2i\00"
@l_j = private constant [4 x i8] c"dif\00"

define i32 @main() {
  ; struct fields by constant index
  %p0 = getelementptr %S, ptr @s, i32 0, i32 0
  %v0 = load i8, ptr %p0
  %e0 = sext i8 %v0 to i64
  call void @show(ptr @l_a, i64 %e0)
  %p1 = getelementptr %S, ptr @s, i32 0, i32 1
  %v1 = load i32, ptr %p1
  %e1 = sext i32 %v1 to i64
  call void @show(ptr @l_b, i64 %e1)
  %p2 = getelementptr inbounds %S, ptr @s, i32 0, i32 2
  %v2 = load i64, ptr %p2
  call void @show(ptr @l_c, i64 %v2)

  ; array inside a struct, then a nested struct with a double, with variable indices
  br label %loop
loop:
  %i = phi i64 [ 0, %0 ], [ %i.next, %loop ]
  %pa = getelementptr %S, ptr @s, i64 0, i32 3, i64 %i
  %va = load i16, ptr %pa
  %ea = sext i16 %va to i64
  call void @show(ptr @l_d, i64 %ea)
  %pe = getelementptr %S, ptr @s, i64 0, i32 5, i64 %i, i32 1
  %ve = load double, ptr %pe
  call i32 (ptr, ...) @printf(ptr @fmt_lf, ptr @l_e, double %ve)
  %i.next = add i64 %i, 1
  %c = icmp ult i64 %i.next, 2
  br i1 %c, label %loop, label %after
after:
  %pd = getelementptr %S, ptr @s, i32 0, i32 4, i32 1
  %vd = load double, ptr %pd
  call i32 (ptr, ...) @printf(ptr @fmt_lf, ptr @l_e, double %vd)

  ; multidimensional array, variable and constant
  %pm = getelementptr [3 x [4 x i32]], ptr @m, i64 0, i64 2, i64 1
  %vm = load i32, ptr %pm
  %em = sext i32 %vm to i64
  call void @show(ptr @l_f, i64 %em)
  %pm2 = getelementptr [4 x i32], ptr @m, i64 1, i64 3
  %vm2 = load i32, ptr %pm2
  %em2 = sext i32 %vm2 to i64
  call void @show(ptr @l_f, i64 %em2)

  ; negative offsets: a constant-expression GEP before the start of the array, brought back by a GEP
  %pn = getelementptr i32, ptr getelementptr (i32, ptr @a, i64 -2), i64 5
  %vn = load i32, ptr %pn
  %en = sext i32 %vn to i64
  call void @show(ptr @l_g, i64 %en)
  %pn2 = getelementptr i8, ptr getelementptr (i8, ptr @a, i64 -8), i64 12
  %vn2 = load i32, ptr %pn2
  %en2 = sext i32 %vn2 to i64
  call void @show(ptr @l_g, i64 %en2)
  ; negative variable index
  %end = getelementptr [8 x i32], ptr @a, i64 0, i64 8
  %pv = getelementptr i32, ptr %end, i64 -3
  %vv = load i32, ptr %pv
  %ev = sext i32 %vv to i64
  call void @show(ptr @l_g, i64 %ev)

  ; constant-expression GEPs loaded directly
  %vc = load i32, ptr getelementptr inbounds ([8 x i32], ptr @a, i64 0, i64 6)
  %ec = sext i32 %vc to i64
  call void @show(ptr @l_h, i64 %ec)
  %vc2 = load i16, ptr getelementptr inbounds (%S, ptr @s, i64 0, i32 3, i64 2)
  %ec2 = sext i16 %vc2 to i64
  call void @show(ptr @l_h, i64 %ec2)

  ; ptrtoint/inttoptr round trips, and the distance between two fields
  %ia = ptrtoint ptr @s to i64
  %ib = ptrtoint ptr %p2 to i64
  %dist = sub i64 %ib, %ia
  call void @show(ptr @l_j, i64 %dist)
  %back = inttoptr i64 %ib to ptr
  %vback = load i64, ptr %back
  call void @show(ptr @l_i, i64 %vback)
  %ic = ptrtoint ptr %pa to i32
  %id = ptrtoint ptr %p0 to i32
  %dist32 = sub i32 %ic, %id
  %edist = sext i32 %dist32 to i64
  call void @show(ptr @l_j, i64 %edist)
  ret i32 0
}
