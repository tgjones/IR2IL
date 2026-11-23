; Like load_not_deferred_past_store, but the write is a memcpy intrinsic.

declare void @llvm.memcpy.p0.p0.i64(ptr, ptr, i64, i1)

define i32 @id(i32 %x) {
  ret i32 %x
}

define i32 @main() {
  %src = alloca i32
  %dst = alloca i32
  store i32 7, ptr %src
  store i32 3, ptr %dst
  %v = load i32, ptr %dst
  %a = add i32 %v, 1
  call void @llvm.memcpy.p0.p0.i64(ptr %dst, ptr %src, i64 4, i1 false)
  %r = call i32 @id(i32 %a)
  ret i32 %r
}
