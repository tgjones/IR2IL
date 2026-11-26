; llvm.experimental.cttz.elts on masks of several widths: the first true lane, none, and padding lanes that are set.

declare i32 @printf(ptr, ...)
declare i64 @llvm.experimental.cttz.elts.i64.v2i1(<2 x i1>, i1)
declare i32 @llvm.experimental.cttz.elts.i32.v4i1(<4 x i1>, i1)
declare i64 @llvm.experimental.cttz.elts.i64.v4i1(<4 x i1>, i1)
declare i64 @llvm.experimental.cttz.elts.i64.v8i1(<8 x i1>, i1)
declare i64 @llvm.experimental.cttz.elts.i64.v16i1(<16 x i1>, i1)
declare i64 @llvm.experimental.cttz.elts.i64.v32i1(<32 x i1>, i1)

@fmt = private constant [6 x i8] c"%llu\0A\00"

define void @emit(i64 %v) {
  call i32 (ptr, ...) @printf(ptr @fmt, i64 %v)
  ret void
}

define void @run(i32 %seed) {
  ; vXi1 masks from comparisons of a lane index vector with the seed
  %s2 = insertelement <2 x i32> poison, i32 %seed, i32 0
  %s2b = shufflevector <2 x i32> %s2, <2 x i32> poison, <2 x i32> zeroinitializer
  %m2 = icmp eq <2 x i32> <i32 0, i32 1>, %s2b
  %r2 = call i64 @llvm.experimental.cttz.elts.i64.v2i1(<2 x i1> %m2, i1 false)
  call void @emit(i64 %r2)

  %s4 = insertelement <4 x i32> poison, i32 %seed, i32 0
  %s4b = shufflevector <4 x i32> %s4, <4 x i32> poison, <4 x i32> zeroinitializer
  %m4 = icmp ule <4 x i32> <i32 0, i32 1, i32 2, i32 3>, %s4b
  %m4n = xor <4 x i1> %m4, <i1 true, i1 true, i1 true, i1 true>
  %r4 = call i64 @llvm.experimental.cttz.elts.i64.v4i1(<4 x i1> %m4n, i1 false)
  call void @emit(i64 %r4)
  %r4i = call i32 @llvm.experimental.cttz.elts.i32.v4i1(<4 x i1> %m4, i1 false)
  %r4e = zext i32 %r4i to i64
  call void @emit(i64 %r4e)

  %s8 = insertelement <8 x i32> poison, i32 %seed, i32 0
  %s8b = shufflevector <8 x i32> %s8, <8 x i32> poison, <8 x i32> zeroinitializer
  %m8 = icmp uge <8 x i32> <i32 0, i32 1, i32 2, i32 3, i32 4, i32 5, i32 6, i32 7>, %s8b
  %r8 = call i64 @llvm.experimental.cttz.elts.i64.v8i1(<8 x i1> %m8, i1 false)
  call void @emit(i64 %r8)

  %s16 = insertelement <16 x i32> poison, i32 %seed, i32 0
  %s16b = shufflevector <16 x i32> %s16, <16 x i32> poison, <16 x i32> zeroinitializer
  %m16 = icmp eq <16 x i32> <i32 0, i32 1, i32 2, i32 3, i32 4, i32 5, i32 6, i32 7, i32 8, i32 9, i32 10, i32 11, i32 12, i32 13, i32 14, i32 15>, %s16b
  %r16 = call i64 @llvm.experimental.cttz.elts.i64.v16i1(<16 x i1> %m16, i1 false)
  call void @emit(i64 %r16)

  %t8 = trunc i32 %seed to i8
  %s32 = insertelement <32 x i8> poison, i8 %t8, i32 0
  %s32b = shufflevector <32 x i8> %s32, <32 x i8> poison, <32 x i32> zeroinitializer
  %m32 = icmp sgt <32 x i8> <i8 0, i8 1, i8 2, i8 3, i8 4, i8 5, i8 6, i8 7, i8 8, i8 9, i8 10, i8 11, i8 12, i8 13, i8 14, i8 15, i8 16, i8 17, i8 18, i8 19, i8 20, i8 21, i8 22, i8 23, i8 24, i8 25, i8 26, i8 27, i8 28, i8 29, i8 30, i8 31>, %s32b
  %r32 = call i64 @llvm.experimental.cttz.elts.i64.v32i1(<32 x i1> %m32, i1 false)
  call void @emit(i64 %r32)
  ret void
}

define i32 @main() {
  call void @run(i32 0)
  call void @run(i32 1)
  call void @run(i32 3)
  call void @run(i32 9)
  call void @run(i32 40)
  ret i32 0
}
