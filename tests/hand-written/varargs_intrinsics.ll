; llvm.va_start, llvm.va_copy, llvm.va_end and the va_arg instruction.
;
; The functions that read their arguments receive them through the variadic convention of
; docs/varargs-and-stdio.md and read them back with the va_arg instruction, which LLVM only lowers meaningfully
; when the va_list is a pointer to the next argument slot (Darwin ARM64, Windows x64: the platforms of this
; test suite). Cases:
; - reading i32, i64, double and ptr arguments in order, and an `i1`, `i8` and `i16` promoted to i32 by the caller;
; - no extra argument at all, and 24 arguments (far more than any register file holds), integers and doubles mixed;
; - va_copy: the copy and the original advance independently, and the copy survives va_end of the original;
; - a va_list passed by address to a function that reads some arguments, after which the caller reads the rest;
; - a va_list forwarded to vprintf, and a variadic function that forwards its extra arguments to another one;
; - several fixed parameters before the extra arguments.

declare i32 @printf(ptr, ...)
declare i32 @vprintf(ptr, ptr)
declare void @llvm.va_start.p0(ptr)
declare void @llvm.va_end.p0(ptr)
declare void @llvm.va_copy.p0(ptr, ptr)

@fmt_i = private constant [6 x i8] c"%lld\0A\00"
@fmt_d = private constant [6 x i8] c"%.3f\0A\00"
@fmt_s = private constant [4 x i8] c"%s\0A\00"
@fmt_label = private constant [10 x i8] c"%s: %lld\0A\00"
@str_a = private constant [6 x i8] c"alpha\00"
@str_b = private constant [5 x i8] c"beta\00"
@vfmt = private constant [29 x i8] c"vprintf: %d %.2f %s %lld %d\0A\00"
@label_sum = private constant [4 x i8] c"sum\00"
@label_copy = private constant [5 x i8] c"copy\00"
@label_orig = private constant [5 x i8] c"orig\00"
@label_rest = private constant [5 x i8] c"rest\00"
@label_fixed = private constant [6 x i8] c"fixed\00"

define void @emit(i64 %v) {
  call i32 (ptr, ...) @printf(ptr @fmt_i, i64 %v)
  ret void
}

define void @emit_double(double %v) {
  call i32 (ptr, ...) @printf(ptr @fmt_d, double %v)
  ret void
}

define void @emit_labelled(ptr %label, i64 %v) {
  call i32 (ptr, ...) @printf(ptr @fmt_label, ptr %label, i64 %v)
  ret void
}

; Sums n i32 arguments.
define i64 @sum_i32(i32 %n, ...) {
entry:
  %ap = alloca ptr
  call void @llvm.va_start.p0(ptr %ap)
  br label %loop
loop:
  %i = phi i32 [ 0, %entry ], [ %i1, %body ]
  %acc = phi i64 [ 0, %entry ], [ %acc1, %body ]
  %more = icmp slt i32 %i, %n
  br i1 %more, label %body, label %done
body:
  %v = va_arg ptr %ap, i32
  %w = sext i32 %v to i64
  %acc1 = add i64 %acc, %w
  %i1 = add i32 %i, 1
  br label %loop
done:
  call void @llvm.va_end.p0(ptr %ap)
  ret i64 %acc
}

; Reads i32, i64, double, ptr, i32 in this order and prints them.
define void @mixed(i32 %unused, ...) {
  %ap = alloca ptr
  call void @llvm.va_start.p0(ptr %ap)
  %a = va_arg ptr %ap, i32
  %b = va_arg ptr %ap, i64
  %c = va_arg ptr %ap, double
  %d = va_arg ptr %ap, ptr
  %e = va_arg ptr %ap, i32
  call void @llvm.va_end.p0(ptr %ap)
  %a64 = sext i32 %a to i64
  call void @emit(i64 %a64)
  call void @emit(i64 %b)
  call void @emit_double(double %c)
  call i32 (ptr, ...) @printf(ptr @fmt_s, ptr %d)
  %e64 = sext i32 %e to i64
  call void @emit(i64 %e64)
  ret void
}

; Reads an i32 and an i64 per pair, with doubles in between: 8 i32, 8 i64 and 8 double.
define void @many(i32 %n, ...) {
entry:
  %ap = alloca ptr
  call void @llvm.va_start.p0(ptr %ap)
  br label %loop
loop:
  %i = phi i32 [ 0, %entry ], [ %i1, %body ]
  %isum = phi i64 [ 0, %entry ], [ %isum2, %body ]
  %dsum = phi double [ 0.0, %entry ], [ %dsum1, %body ]
  %more = icmp slt i32 %i, %n
  br i1 %more, label %body, label %done
body:
  %a = va_arg ptr %ap, i32
  %b = va_arg ptr %ap, i64
  %c = va_arg ptr %ap, double
  %a64 = sext i32 %a to i64
  %isum1 = add i64 %isum, %a64
  %isum2 = add i64 %isum1, %b
  %dsum1 = fadd double %dsum, %c
  %i1 = add i32 %i, 1
  br label %loop
done:
  call void @llvm.va_end.p0(ptr %ap)
  call void @emit(i64 %isum)
  call void @emit_double(double %dsum)
  ret void
}

; Sums the first n arguments from the copy, then the first n from the original.
define void @copied(i32 %n, ...) {
entry:
  %ap = alloca ptr
  %cp = alloca ptr
  call void @llvm.va_start.p0(ptr %ap)
  call void @llvm.va_copy.p0(ptr %cp, ptr %ap)
  br label %loop1
loop1:
  %i = phi i32 [ 0, %entry ], [ %i1, %body1 ]
  %acc = phi i64 [ 0, %entry ], [ %acc1, %body1 ]
  %more = icmp slt i32 %i, %n
  br i1 %more, label %body1, label %mid
body1:
  %v = va_arg ptr %cp, i32
  %w = sext i32 %v to i64
  %acc1 = add i64 %acc, %w
  %i1 = add i32 %i, 1
  br label %loop1
mid:
  call void @emit_labelled(ptr @label_copy, i64 %acc)
  ; The original has not moved: a second copy made now starts at the first argument too.
  %cp2 = alloca ptr
  call void @llvm.va_copy.p0(ptr %cp2, ptr %ap)
  call void @llvm.va_end.p0(ptr %cp)
  br label %loop2
loop2:
  %j = phi i32 [ 0, %mid ], [ %j1, %body2 ]
  %acc2 = phi i64 [ 0, %mid ], [ %acc3, %body2 ]
  %more2 = icmp slt i32 %j, %n
  br i1 %more2, label %body2, label %done
body2:
  %x = va_arg ptr %ap, i32
  %y = sext i32 %x to i64
  %acc3 = add i64 %acc2, %y
  %j1 = add i32 %j, 1
  br label %loop2
done:
  call void @llvm.va_end.p0(ptr %ap)
  call void @emit_labelled(ptr @label_orig, i64 %acc2)
  ; The second copy, made before the original was read, still yields the first argument and then the second.
  %f = va_arg ptr %cp2, i32
  %g = va_arg ptr %cp2, i32
  %f64 = sext i32 %f to i64
  %g64 = sext i32 %g to i64
  %fg = mul i64 %f64, 1000
  %fgs = add i64 %fg, %g64
  call void @emit(i64 %fgs)
  call void @llvm.va_end.p0(ptr %cp2)
  ret void
}

; Reads one i32 through a va_list that belongs to the caller.
define i32 @take_one(ptr %list) {
  %v = va_arg ptr %list, i32
  ret i32 %v
}

; Reads two arguments through take_one, then the rest itself (n arguments in all).
define void @shared(i32 %n, ...) {
entry:
  %ap = alloca ptr
  call void @llvm.va_start.p0(ptr %ap)
  %a = call i32 @take_one(ptr %ap)
  %b = call i32 @take_one(ptr %ap)
  %a64 = sext i32 %a to i64
  %b64 = sext i32 %b to i64
  %ab = add i64 %a64, %b64
  call void @emit_labelled(ptr @label_fixed, i64 %ab)
  %left = sub i32 %n, 2
  br label %loop
loop:
  %i = phi i32 [ 0, %entry ], [ %i1, %body ]
  %acc = phi i64 [ 0, %entry ], [ %acc1, %body ]
  %more = icmp slt i32 %i, %left
  br i1 %more, label %body, label %done
body:
  %v = va_arg ptr %ap, i32
  %w = sext i32 %v to i64
  %acc1 = add i64 %acc, %w
  %i1 = add i32 %i, 1
  br label %loop
done:
  call void @llvm.va_end.p0(ptr %ap)
  call void @emit_labelled(ptr @label_rest, i64 %acc)
  ret void
}

; Passes its extra arguments (i32, double, ptr, i64, i32) to vprintf.
define void @forward_to_vprintf(ptr %unused, ...) {
  %ap = alloca ptr
  call void @llvm.va_start.p0(ptr %ap)
  %list = load ptr, ptr %ap
  call i32 @vprintf(ptr @vfmt, ptr %list)
  call void @llvm.va_end.p0(ptr %ap)
  ret void
}

; A variadic function that calls another with its own extra arguments, on a copy of its list.
define i64 @inner_sum(i32 %n, ptr %list) {
entry:
  br label %loop
loop:
  %i = phi i32 [ 0, %entry ], [ %i1, %body ]
  %acc = phi i64 [ 0, %entry ], [ %acc1, %body ]
  %more = icmp slt i32 %i, %n
  br i1 %more, label %body, label %done
body:
  %v = va_arg ptr %list, i64
  %acc1 = add i64 %acc, %v
  %i1 = add i32 %i, 1
  br label %loop
done:
  ret i64 %acc
}

define i64 @outer_sum(i32 %n, ...) {
  %ap = alloca ptr
  call void @llvm.va_start.p0(ptr %ap)
  %r = call i64 @inner_sum(i32 %n, ptr %ap)
  call void @llvm.va_end.p0(ptr %ap)
  ret i64 %r
}

; Nine fixed parameters (more than the registers of either ABI), then the extra arguments.
define i64 @fixed_many(i32 %a, i32 %b, i32 %c, i32 %d, i32 %e, i32 %f, i32 %g, i32 %h, i32 %i, ...) {
  %ap = alloca ptr
  call void @llvm.va_start.p0(ptr %ap)
  %x = va_arg ptr %ap, i64
  %y = va_arg ptr %ap, i64
  call void @llvm.va_end.p0(ptr %ap)
  %s1 = add i32 %a, %b
  %s2 = add i32 %s1, %c
  %s3 = add i32 %s2, %d
  %s4 = add i32 %s3, %e
  %s5 = add i32 %s4, %f
  %s6 = add i32 %s5, %g
  %s7 = add i32 %s6, %h
  %s8 = add i32 %s7, %i
  %s64 = sext i32 %s8 to i64
  %xy = add i64 %x, %y
  %r = mul i64 %s64, %xy
  ret i64 %r
}

; The extra arguments are i1, i8 and i16 values the caller extended to i32.
define void @promoted(i32 %n, ...) {
  %ap = alloca ptr
  call void @llvm.va_start.p0(ptr %ap)
  %a = va_arg ptr %ap, i32
  %b = va_arg ptr %ap, i32
  %c = va_arg ptr %ap, i32
  call void @llvm.va_end.p0(ptr %ap)
  %a64 = sext i32 %a to i64
  %b64 = sext i32 %b to i64
  %c64 = sext i32 %c to i64
  call void @emit(i64 %a64)
  call void @emit(i64 %b64)
  call void @emit(i64 %c64)
  ret void
}

define i32 @main() {
  %none = call i64 (i32, ...) @sum_i32(i32 0)
  call void @emit_labelled(ptr @label_sum, i64 %none)
  %s5 = call i64 (i32, ...) @sum_i32(i32 5, i32 10, i32 -20, i32 30, i32 2147483647, i32 -2147483648)
  call void @emit_labelled(ptr @label_sum, i64 %s5)

  call void (i32, ...) @mixed(i32 0, i32 -7, i64 -4611686018427387904, double 3.14159, ptr @str_a, i32 99)

  call void (i32, ...) @many(i32 8,
    i32 1, i64 10, double 0.5,
    i32 2, i64 20, double 1.5,
    i32 3, i64 30, double 2.5,
    i32 4, i64 40, double 3.5,
    i32 5, i64 50, double 4.5,
    i32 6, i64 60, double 5.5,
    i32 7, i64 70, double 6.5,
    i32 8, i64 80, double 7.5)

  call void (i32, ...) @copied(i32 3, i32 100, i32 200, i32 300, i32 400)
  call void (i32, ...) @shared(i32 6, i32 1, i32 2, i32 3, i32 4, i32 5, i32 6)
  call void (ptr, ...) @forward_to_vprintf(ptr null, i32 -5, double 2.5, ptr @str_b, i64 1234567890123, i32 7)

  %o = call i64 (i32, ...) @outer_sum(i32 4, i64 1, i64 20, i64 300, i64 4000)
  call void @emit(i64 %o)

  %f = call i64 (i32, i32, i32, i32, i32, i32, i32, i32, i32, ...) @fixed_many(i32 1, i32 2, i32 3, i32 4, i32 5, i32 6, i32 7, i32 8, i32 9, i64 3, i64 4)
  call void @emit(i64 %f)

  call void (i32, ...) @promoted(i32 0, i32 1, i32 -128, i32 65535)
  ret i32 0
}
