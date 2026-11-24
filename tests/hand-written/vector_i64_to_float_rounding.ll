declare i32 @printf(ptr, ...)
@fmt = private constant [6 x i8] c"%08x\0A\00"
@vals = private constant [2 x i64] [i64 1152921573326323713, i64 -1152921573326323713]

define i32 @main() {
  %a = load <2 x i64>, ptr @vals, align 16
  %s = sitofp <2 x i64> %a to <2 x float>
  %u = uitofp <2 x i64> %a to <2 x float>
  %s0 = extractelement <2 x float> %s, i32 0
  %s1 = extractelement <2 x float> %s, i32 1
  %u0 = extractelement <2 x float> %u, i32 0
  %u1 = extractelement <2 x float> %u, i32 1
  %b0 = bitcast float %s0 to i32
  %b1 = bitcast float %s1 to i32
  %b2 = bitcast float %u0 to i32
  %b3 = bitcast float %u1 to i32
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %b0)
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %b1)
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %b2)
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %b3)
  ret i32 0
}
