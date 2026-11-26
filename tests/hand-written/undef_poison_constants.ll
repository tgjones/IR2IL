; undef and poison operands of double and i128 type (and of the other scalar types) that never reach the
; result: the arm of a select that isn't taken, an argument the callee ignores, a value stored then overwritten.

@fmt_d = private constant [8 x i8] c"d %.1f\0A\00"
@fmt_i = private constant [8 x i8] c"i %lld\0A\00"
@fmt_f = private constant [8 x i8] c"f %.1f\0A\00"

declare i32 @printf(ptr, ...)

define double @ignore_double(double %ignored, double %x) {
  ret double %x
}

define i128 @ignore_i128(i128 %ignored, i128 %x) {
  ret i128 %x
}

define double @pick_double(i1 %c, double %x) {
  %r = select i1 %c, double %x, double undef
  %s = select i1 %c, double %r, double poison
  ret double %s
}

define i128 @pick_i128(i1 %c, i128 %x) {
  %r = select i1 %c, i128 %x, i128 undef
  %s = select i1 %c, i128 %r, i128 poison
  ret i128 %s
}

define float @pick_float(i1 %c, float %x) {
  %r = select i1 %c, float %x, float poison
  ret float %r
}

define i64 @pick_i64(i1 %c, i64 %x) {
  %r = select i1 %c, i64 %x, i64 undef
  ret i64 %r
}

define i32 @main() {
  %t = call i1 @id(i1 true)
  %d1 = call double @pick_double(i1 %t, double 2.5)
  call i32 (ptr, ...) @printf(ptr @fmt_d, double %d1)
  %d2 = call double @ignore_double(double undef, double 3.5)
  call i32 (ptr, ...) @printf(ptr @fmt_d, double %d2)
  %d3 = call double @ignore_double(double poison, double 4.5)
  call i32 (ptr, ...) @printf(ptr @fmt_d, double %d3)

  %i1 = call i128 @pick_i128(i1 %t, i128 123456789012345678901234567890)
  %i1lo = trunc i128 %i1 to i64
  call i32 (ptr, ...) @printf(ptr @fmt_i, i64 %i1lo)
  %i2 = call i128 @ignore_i128(i128 undef, i128 77)
  %i2lo = trunc i128 %i2 to i64
  call i32 (ptr, ...) @printf(ptr @fmt_i, i64 %i2lo)
  %i3 = call i128 @ignore_i128(i128 poison, i128 -5)
  %i3lo = trunc i128 %i3 to i64
  call i32 (ptr, ...) @printf(ptr @fmt_i, i64 %i3lo)

  %f1 = call float @pick_float(i1 %t, float 1.5)
  %f1d = fpext float %f1 to double
  call i32 (ptr, ...) @printf(ptr @fmt_f, double %f1d)
  %l1 = call i64 @pick_i64(i1 %t, i64 99)
  call i32 (ptr, ...) @printf(ptr @fmt_i, i64 %l1)

  %slot = alloca double
  store double undef, ptr %slot
  store double 6.5, ptr %slot
  %v = load double, ptr %slot
  call i32 (ptr, ...) @printf(ptr @fmt_d, double %v)
  ret i32 0
}

define i1 @id(i1 %x) {
  ret i1 %x
}
