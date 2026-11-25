; A module with no "target datalayout": struct offsets and llvm.objectsize use the host target's layout
; (i64 aligned to 8), as clang does, not LLVM's generic default.

declare i32 @printf(ptr, ...)
declare i64 @llvm.objectsize.i64.p0(ptr, i1 immarg, i1 immarg, i1 immarg)

@fmt = private constant [6 x i8] c"%lld\0A\00"
@pair = global { i8, i64, [3 x i32] } zeroinitializer

define i32 @main() {
  %f1 = getelementptr { i8, i64, [3 x i32] }, ptr @pair, i32 0, i32 1
  %f2 = getelementptr { i8, i64, [3 x i32] }, ptr @pair, i32 0, i32 2
  %e = getelementptr { i8, i64, [3 x i32] }, ptr @pair, i32 0, i32 2, i32 1
  %o1 = ptrtoint ptr %f1 to i64
  %o2 = ptrtoint ptr %f2 to i64
  %o3 = ptrtoint ptr %e to i64
  %b = ptrtoint ptr @pair to i64
  %d1 = sub i64 %o1, %b
  %d2 = sub i64 %o2, %b
  %d3 = sub i64 %o3, %b
  %s1 = call i64 @llvm.objectsize.i64.p0(ptr @pair, i1 false, i1 false, i1 false)
  %s2 = call i64 @llvm.objectsize.i64.p0(ptr %f2, i1 false, i1 false, i1 false)
  %s3 = call i64 @llvm.objectsize.i64.p0(ptr %e, i1 false, i1 false, i1 false)
  call i32 (ptr, ...) @printf(ptr @fmt, i64 %d1)
  call i32 (ptr, ...) @printf(ptr @fmt, i64 %d2)
  call i32 (ptr, ...) @printf(ptr @fmt, i64 %d3)
  call i32 (ptr, ...) @printf(ptr @fmt, i64 %s1)
  call i32 (ptr, ...) @printf(ptr @fmt, i64 %s2)
  call i32 (ptr, ...) @printf(ptr @fmt, i64 %s3)
  ret i32 0
}
