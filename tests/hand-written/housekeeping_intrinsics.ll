; The intrinsics that don't compute anything the program can see: llvm.lifetime.start/end, llvm.assume,
; llvm.objectsize (unknown at -O0: -1, or 0 when `min` is set), llvm.threadlocal.address and llvm.expect.
; Each is used in the middle of ordinary code, which has to behave as if it weren't there.

declare i32 @printf(ptr, ...)
declare void @llvm.lifetime.start.p0(i64 immarg, ptr)
declare void @llvm.lifetime.end.p0(i64 immarg, ptr)
declare void @llvm.assume(i1)
declare i64 @llvm.objectsize.i64.p0(ptr, i1 immarg, i1 immarg, i1 immarg)
declare i32 @llvm.objectsize.i32.p0(ptr, i1 immarg, i1 immarg, i1 immarg)
declare ptr @llvm.threadlocal.address.p0(ptr)
declare i64 @llvm.expect.i64(i64, i64)
declare i32 @llvm.expect.i32(i32, i32)
declare i1 @llvm.expect.i1(i1, i1)

@fmt = private constant [6 x i8] c"%lld\0A\00"
@tls = thread_local global i32 41
@tls_array = thread_local global [4 x i64] [i64 1, i64 2, i64 3, i64 4]
@buffer = private global [64 x i8] zeroinitializer
@weak_buffer = weak global [32 x i8] zeroinitializer
@odr_buffer = linkonce_odr global [24 x i8] zeroinitializer
@pair = global { i8, i64, [3 x i32] } zeroinitializer

define void @emit(i64 %v) {
  call i32 (ptr, ...) @printf(ptr @fmt, i64 %v)
  ret void
}

define void @lifetimes(i32 %x) {
  %a = alloca i32
  %b = alloca [8 x i64]
  call void @llvm.lifetime.start.p0(i64 4, ptr %a)
  store i32 %x, ptr %a
  call void @llvm.lifetime.start.p0(i64 64, ptr %b)
  %p = getelementptr [8 x i64], ptr %b, i64 0, i64 3
  %xe = zext i32 %x to i64
  store i64 %xe, ptr %p
  %v = load i32, ptr %a
  %w = load i64, ptr %p
  %ve = zext i32 %v to i64
  %s = add i64 %ve, %w
  call void @llvm.lifetime.end.p0(i64 64, ptr %b)
  call void @llvm.lifetime.end.p0(i64 4, ptr %a)
  call void @emit(i64 %s)
  ret void
}

define void @assumes(i32 %x) {
  %c = icmp ne i32 %x, 0
  call void @llvm.assume(i1 %c)
  %d = udiv i32 100, %x
  %de = zext i32 %d to i64
  call void @emit(i64 %de)
  %e = icmp ult i32 %x, 1000
  call void @llvm.assume(i1 %e)
  call void @llvm.assume(i1 true)
  ret void
}

define void @objectsizes() {
  %a = alloca [16 x i8]
  %p = getelementptr [16 x i8], ptr %a, i64 0, i64 4
  %r1 = call i64 @llvm.objectsize.i64.p0(ptr %a, i1 false, i1 false, i1 false)
  %r2 = call i64 @llvm.objectsize.i64.p0(ptr %a, i1 true, i1 false, i1 false)
  %r3 = call i64 @llvm.objectsize.i64.p0(ptr %p, i1 false, i1 true, i1 false)
  %r4 = call i64 @llvm.objectsize.i64.p0(ptr @buffer, i1 true, i1 true, i1 true)
  %r5 = call i32 @llvm.objectsize.i32.p0(ptr %a, i1 false, i1 false, i1 false)
  %r6 = call i32 @llvm.objectsize.i32.p0(ptr %a, i1 true, i1 false, i1 false)
  %r5e = sext i32 %r5 to i64
  %r6e = sext i32 %r6 to i64
  call void @emit(i64 %r1)
  call void @emit(i64 %r2)
  call void @emit(i64 %r3)
  call void @emit(i64 %r4)
  call void @emit(i64 %r5e)
  call void @emit(i64 %r6e)
  ret void
}

define void @objectsizes_more(ptr %arg, i64 %n) {
  %a = alloca [16 x i8]
  %past = getelementptr [16 x i8], ptr %a, i64 0, i64 20
  %end = getelementptr [16 x i8], ptr %a, i64 0, i64 16
  %before = getelementptr [16 x i8], ptr %a, i64 0, i64 -4
  %field = getelementptr { i8, i64, [3 x i32] }, ptr @pair, i32 0, i32 2, i32 1
  %dyn = alloca i8, i64 %n
  %r1 = call i64 @llvm.objectsize.i64.p0(ptr %past, i1 false, i1 false, i1 false)
  %r2 = call i64 @llvm.objectsize.i64.p0(ptr %end, i1 true, i1 false, i1 false)
  %r3 = call i64 @llvm.objectsize.i64.p0(ptr %before, i1 false, i1 false, i1 false)
  %r4 = call i64 @llvm.objectsize.i64.p0(ptr @weak_buffer, i1 false, i1 false, i1 false)
  %r5 = call i64 @llvm.objectsize.i64.p0(ptr @weak_buffer, i1 true, i1 false, i1 false)
  %r6 = call i64 @llvm.objectsize.i64.p0(ptr @odr_buffer, i1 false, i1 false, i1 false)
  %r7 = call i64 @llvm.objectsize.i64.p0(ptr %field, i1 false, i1 false, i1 false)
  %r8 = call i64 @llvm.objectsize.i64.p0(ptr %arg, i1 false, i1 false, i1 false)
  %r9 = call i64 @llvm.objectsize.i64.p0(ptr %arg, i1 true, i1 false, i1 false)
  %r10 = call i64 @llvm.objectsize.i64.p0(ptr %dyn, i1 false, i1 false, i1 false)
  call void @emit(i64 %r1)
  call void @emit(i64 %r2)
  call void @emit(i64 %r3)
  call void @emit(i64 %r4)
  call void @emit(i64 %r5)
  call void @emit(i64 %r6)
  call void @emit(i64 %r7)
  call void @emit(i64 %r8)
  call void @emit(i64 %r9)
  call void @emit(i64 %r10)
  ret void
}

define void @threadlocals() {
  %p = call ptr @llvm.threadlocal.address.p0(ptr @tls)
  %v = load i32, ptr %p
  %w = add i32 %v, 1
  store i32 %w, ptr %p
  %q = call ptr @llvm.threadlocal.address.p0(ptr @tls)
  %y = load i32, ptr %q
  %ye = zext i32 %y to i64
  call void @emit(i64 %ye)
  %g = call ptr @llvm.threadlocal.address.p0(ptr @tls_array)
  %g2 = getelementptr [4 x i64], ptr %g, i64 0, i64 2
  %x = load i64, ptr %g2
  call void @emit(i64 %x)
  ret void
}

define void @expects(i64 %x) {
  %a = call i64 @llvm.expect.i64(i64 %x, i64 3)
  %b = trunc i64 %x to i32
  %c = call i32 @llvm.expect.i32(i32 %b, i32 1)
  %d = icmp eq i64 %x, 5
  %e = call i1 @llvm.expect.i1(i1 %d, i1 false)
  %ce = zext i32 %c to i64
  %ee = zext i1 %e to i64
  call void @emit(i64 %a)
  call void @emit(i64 %ce)
  call void @emit(i64 %ee)
  br i1 %e, label %yes, label %no
yes:
  call void @emit(i64 111)
  ret void
no:
  call void @emit(i64 222)
  ret void
}

define i32 @main() {
  call void @lifetimes(i32 5)
  call void @lifetimes(i32 4000000000)
  call void @assumes(i32 7)
  call void @objectsizes()
  call void @objectsizes_more(ptr @buffer, i64 12)
  call void @threadlocals()
  call void @expects(i64 3)
  call void @expects(i64 5)
  ret i32 0
}
