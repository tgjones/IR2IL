; Dynamic alloca and llvm.stacksave / llvm.stackrestore.
;
; - dynamic alloca: the element count comes from an argument, a load and a computation, for several element types,
;   and the memory is written and read back;
; - a VLA in a loop with stacksave/stackrestore around it: 200000 iterations of 4 KiB would overflow a real stack
;   (800 MB) if stackrestore did nothing;
; - nested saves, restoring the outer one, and allocation after a restore that must not see the earlier contents
;   corrupted (a live outer VLA survives restores of inner scopes);
; - a function with a VLA that is called deeply and repeatedly: its memory has to be released when it returns;
; - alloca alignment of 16 and 64 bytes.

declare i32 @printf(ptr, ...)
declare ptr @llvm.stacksave.p0()
declare void @llvm.stackrestore.p0(ptr)

@fmt = private constant [6 x i8] c"%llu\0A\00"
@count = private global i64 37

define void @emit(i64 %v) {
  call i32 (ptr, ...) @printf(ptr @fmt, i64 %v)
  ret void
}

; Fills n i32 elements with i*3+seed and returns their sum.
define i64 @fill_sum_i32(i64 %n, i32 %seed) {
entry:
  %a = alloca i32, i64 %n
  br label %fill
fill:
  %i = phi i64 [ 0, %entry ], [ %i1, %fill ]
  %t = trunc i64 %i to i32
  %m = mul i32 %t, 3
  %v = add i32 %m, %seed
  %p = getelementptr i32, ptr %a, i64 %i
  store i32 %v, ptr %p
  %i1 = add i64 %i, 1
  %c = icmp ult i64 %i1, %n
  br i1 %c, label %fill, label %sum
sum:
  %j = phi i64 [ 0, %fill ], [ %j1, %sum2 ]
  %s = phi i64 [ 0, %fill ], [ %s1, %sum2 ]
  br label %sum2
sum2:
  %q = getelementptr i32, ptr %a, i64 %j
  %x = load i32, ptr %q
  %xe = zext i32 %x to i64
  %s1 = add i64 %s, %xe
  %j1 = add i64 %j, 1
  %d = icmp ult i64 %j1, %n
  br i1 %d, label %sum, label %done
done:
  ret i64 %s1
}

; Same, with i8, i64 and double elements and a count computed in the function.
define i64 @fill_sum_mixed(i64 %base) {
  %n = add i64 %base, 5
  %a8 = alloca i8, i64 %n
  %a64 = alloca i64, i64 %n
  %ad = alloca double, i64 %n
  %as = alloca { i8, i32 }, i64 %n
  call void @llvm.memset.p0.i64(ptr %a8, i8 7, i64 %n, i1 false)
  %last = sub i64 %n, 1
  %p8 = getelementptr i8, ptr %a8, i64 %last
  store i8 99, ptr %p8
  %p64 = getelementptr i64, ptr %a64, i64 %last
  store i64 123456789012, ptr %p64
  %pd = getelementptr double, ptr %ad, i64 %last
  store double 2.5, ptr %pd
  %ps = getelementptr { i8, i32 }, ptr %as, i64 %last, i32 1
  store i32 77, ptr %ps
  %v8 = load i8, ptr %p8
  %w8 = zext i8 %v8 to i64
  %v64 = load i64, ptr %p64
  %vd = load double, ptr %pd
  %wd = fptoui double %vd to i64
  %vs = load i32, ptr %ps
  %ws = zext i32 %vs to i64
  %r1 = add i64 %w8, %v64
  %r2 = add i64 %r1, %wd
  %r3 = add i64 %r2, %ws
  ret i64 %r3
}

declare void @llvm.memset.p0.i64(ptr, i8, i64, i1)

; The count is loaded from a global.
define i64 @from_load() {
  %n = load i64, ptr @count
  %r = call i64 @fill_sum_i32(i64 %n, i32 1000)
  ret i64 %r
}

; A VLA in a loop, released at the end of each iteration.
define i64 @vla_loop(i64 %iterations) {
entry:
  br label %loop
loop:
  %i = phi i64 [ 0, %entry ], [ %i1, %loop ]
  %acc = phi i64 [ 0, %entry ], [ %acc1, %loop ]
  %sp = call ptr @llvm.stacksave.p0()
  %a = alloca i8, i64 4096
  %b = trunc i64 %i to i8
  call void @llvm.memset.p0.i64(ptr %a, i8 %b, i64 4096, i1 false)
  %p = getelementptr i8, ptr %a, i64 4095
  %x = load i8, ptr %p
  %xe = zext i8 %x to i64
  %acc1 = add i64 %acc, %xe
  call void @llvm.stackrestore.p0(ptr %sp)
  %i1 = add i64 %i, 1
  %c = icmp ult i64 %i1, %iterations
  br i1 %c, label %loop, label %done
done:
  ret i64 %acc1
}

; An outer VLA that must survive the restores of inner scopes, with nested saves.
define i64 @nested() {
  %outer = alloca i32, i64 64
  br label %fill
fill:
  %i = phi i64 [ 0, %0 ], [ %i1, %fill ]
  %t = trunc i64 %i to i32
  %p = getelementptr i32, ptr %outer, i64 %i
  store i32 %t, ptr %p
  %i1 = add i64 %i, 1
  %c = icmp ult i64 %i1, 64
  br i1 %c, label %fill, label %inner
inner:
  %sp1 = call ptr @llvm.stacksave.p0()
  %a = alloca i32, i64 100
  call void @llvm.memset.p0.i64(ptr %a, i8 1, i64 400, i1 false)
  %sp2 = call ptr @llvm.stacksave.p0()
  %b = alloca i32, i64 200
  call void @llvm.memset.p0.i64(ptr %b, i8 2, i64 800, i1 false)
  %xb = load i32, ptr %b
  call void @llvm.stackrestore.p0(ptr %sp2)
  %c2 = alloca i32, i64 300
  call void @llvm.memset.p0.i64(ptr %c2, i8 3, i64 1200, i1 false)
  %xa = load i32, ptr %a
  %xc = load i32, ptr %c2
  call void @llvm.stackrestore.p0(ptr %sp1)
  %d = alloca i32, i64 50
  call void @llvm.memset.p0.i64(ptr %d, i8 4, i64 200, i1 false)
  %xd = load i32, ptr %d
  %q = getelementptr i32, ptr %outer, i64 63
  %xo = load i32, ptr %q
  %q0 = getelementptr i32, ptr %outer, i64 17
  %xo2 = load i32, ptr %q0
  %e1 = zext i32 %xa to i64
  %e2 = zext i32 %xb to i64
  %e3 = zext i32 %xc to i64
  %e4 = zext i32 %xd to i64
  %e5 = zext i32 %xo to i64
  %e6 = zext i32 %xo2 to i64
  call void @emit(i64 %e1)
  call void @emit(i64 %e2)
  call void @emit(i64 %e3)
  call void @emit(i64 %e4)
  call void @emit(i64 %e5)
  call void @emit(i64 %e6)
  ret i64 0
}

; Recursion with a VLA in every frame: 1000 frames of 8 KiB, and 200 such calls in a row.
define i64 @recurse(i64 %depth) {
entry:
  %a = alloca i8, i64 8192
  %b = trunc i64 %depth to i8
  store i8 %b, ptr %a
  %z = icmp eq i64 %depth, 0
  br i1 %z, label %base, label %rec
base:
  ret i64 0
rec:
  %d1 = sub i64 %depth, 1
  %r = call i64 @recurse(i64 %d1)
  %x = load i8, ptr %a
  %xe = zext i8 %x to i64
  %s = add i64 %r, %xe
  ret i64 %s
}

define i64 @many_recursions() {
entry:
  br label %loop
loop:
  %i = phi i64 [ 0, %entry ], [ %i1, %loop ]
  %acc = phi i64 [ 0, %entry ], [ %acc1, %loop ]
  %r = call i64 @recurse(i64 1000)
  %acc1 = add i64 %acc, %r
  %i1 = add i64 %i, 1
  %c = icmp ult i64 %i1, 200
  br i1 %c, label %loop, label %done
done:
  ret i64 %acc1
}

define void @alignment(i64 %n) {
  %a = alloca i8, i64 %n, align 16
  %b = alloca i8, i64 %n, align 64
  %c = alloca i32, i64 %n, align 32
  %ia = ptrtoint ptr %a to i64
  %ib = ptrtoint ptr %b to i64
  %ic = ptrtoint ptr %c to i64
  %ma = and i64 %ia, 15
  %mb = and i64 %ib, 63
  %mc = and i64 %ic, 31
  call void @emit(i64 %ma)
  call void @emit(i64 %mb)
  call void @emit(i64 %mc)
  ret void
}

define i32 @main() {
  %r1 = call i64 @fill_sum_i32(i64 10, i32 5)
  call void @emit(i64 %r1)
  %r2 = call i64 @fill_sum_i32(i64 1, i32 5)
  call void @emit(i64 %r2)
  %r3 = call i64 @fill_sum_i32(i64 100000, i32 9)
  call void @emit(i64 %r3)
  %r4 = call i64 @fill_sum_mixed(i64 0)
  call void @emit(i64 %r4)
  %r5 = call i64 @fill_sum_mixed(i64 300)
  call void @emit(i64 %r5)
  %r6 = call i64 @from_load()
  call void @emit(i64 %r6)
  %r7 = call i64 @vla_loop(i64 200000)
  call void @emit(i64 %r7)
  %r8 = call i64 @nested()
  call void @emit(i64 %r8)
  %r9 = call i64 @many_recursions()
  call void @emit(i64 %r9)
  call void @alignment(i64 1)
  call void @alignment(i64 17)
  call void @alignment(i64 1000)
  ret i32 0
}
