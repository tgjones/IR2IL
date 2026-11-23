; Vectors of 16 and 32 bits (<2 x i8>, <4 x i8>, <2 x i16>) are held in a Vector64 with padding lanes, but have
; only their own bytes in memory and as struct fields and array elements. main returns the 1-based number of the
; first check that failed, or 0.

%S = type { <2 x i8>, i8, <2 x i16>, <4 x i8> }

@global_s = global %S { <2 x i8> <i8 1, i8 -2>, i8 7, <2 x i16> <i16 300, i16 -400>, <4 x i8> <i8 1, i8 2, i8 3, i8 4> }

define i32 @id(i32 %x) {
  ret i32 %x
}

; Loads read exactly the bytes of the vector: the buffer holds 0x7f after the vector, which would show up in
; the padding lanes (and is not allowed to be read past the end of an allocation anyway).
define i32 @check_load_store() {
  %buf = alloca [16 x i8], align 8
  call void @llvm.memset.p0.i64(ptr %buf, i8 127, i64 16, i1 false)

  %x = call i32 @id(i32 5)
  %x8 = trunc i32 %x to i8
  %v0 = insertelement <2 x i8> zeroinitializer, i8 %x8, i64 0
  %v = insertelement <2 x i8> %v0, i8 -9, i64 1
  store <2 x i8> %v, ptr %buf, align 2

  ; The two bytes after the vector are untouched.
  %p2 = getelementptr i8, ptr %buf, i64 2
  %b2 = load i8, ptr %p2
  %ok1 = icmp eq i8 %b2, 127

  ; The lanes made it to memory.
  %b0 = load i8, ptr %buf
  %ok2 = icmp eq i8 %b0, 5
  %p1 = getelementptr i8, ptr %buf, i64 1
  %b1 = load i8, ptr %p1
  %ok3 = icmp eq i8 %b1, -9

  ; Loading it back, the vector as an integer holds just its two bytes.
  %w = load <2 x i8>, ptr %buf, align 2
  %wi = bitcast <2 x i8> %w to i16
  %ok4 = icmp eq i16 %wi, -2299 ; 0xf705

  %a = and i1 %ok1, %ok2
  %b = and i1 %ok3, %ok4
  %c = and i1 %a, %b
  %r = select i1 %c, i32 0, i32 1
  ret i32 %r
}

define i32 @check_bitcast() {
  %x = call i32 @id(i32 66051) ; 0x00010203
  %v4 = bitcast i32 %x to <4 x i8>
  %e0 = extractelement <4 x i8> %v4, i64 0
  %e3 = extractelement <4 x i8> %v4, i64 3
  %ok1 = icmp eq i8 %e0, 3
  %ok2 = icmp eq i8 %e3, 0

  %back = bitcast <4 x i8> %v4 to i32
  %ok3 = icmp eq i32 %back, 66051

  %y = call i32 @id(i32 -65536) ; 0xffff0000
  %v2 = bitcast i32 %y to <2 x i16>
  %f0 = extractelement <2 x i16> %v2, i64 0
  %f1 = extractelement <2 x i16> %v2, i64 1
  %ok4 = icmp eq i16 %f0, 0
  %ok5 = icmp eq i16 %f1, -1

  ; <2 x i16> as <4 x i8>.
  %v4b = bitcast <2 x i16> %v2 to <4 x i8>
  %g3 = extractelement <4 x i8> %v4b, i64 3
  %ok6 = icmp eq i8 %g3, -1

  ; <2 x i8> from and to i16.
  %z = call i32 @id(i32 258)
  %z16 = trunc i32 %z to i16
  %v2b = bitcast i16 %z16 to <2 x i8>
  %h1 = extractelement <2 x i8> %v2b, i64 1
  %ok7 = icmp eq i8 %h1, 1

  ; A float as <2 x i16>.
  %fl = bitcast i32 %y to float
  %fv = bitcast float %fl to <2 x i16>
  %fi = bitcast <2 x i16> %fv to i32
  %ok8 = icmp eq i32 %fi, -65536

  %a = and i1 %ok1, %ok2
  %b = and i1 %ok3, %ok4
  %c = and i1 %ok5, %ok6
  %d = and i1 %ok7, %ok8
  %ab = and i1 %a, %b
  %cd = and i1 %c, %d
  %all = and i1 %ab, %cd
  %r = select i1 %all, i32 0, i32 1
  ret i32 %r
}

; Struct fields and array elements take their own size, so the layout is the one of LLVM.
define i32 @check_struct_and_array() {
  %s = alloca %S, align 8
  call void @llvm.memset.p0.i64(ptr %s, i8 85, i64 16, i1 false)
  %c = load %S, ptr @global_s
  store %S %c, ptr %s, align 8

  %f0 = getelementptr %S, ptr %s, i32 0, i32 0
  %v0 = load <2 x i8>, ptr %f0, align 2
  %e = extractelement <2 x i8> %v0, i64 1
  %ok1 = icmp eq i8 %e, -2

  %f1 = getelementptr %S, ptr %s, i32 0, i32 1
  %b1 = load i8, ptr %f1
  %ok2 = icmp eq i8 %b1, 7

  %f2 = getelementptr %S, ptr %s, i32 0, i32 2
  %v2 = load <2 x i16>, ptr %f2, align 4
  %e2 = extractelement <2 x i16> %v2, i64 1
  %ok3 = icmp eq i16 %e2, -400

  %f3 = getelementptr %S, ptr %s, i32 0, i32 3
  %v3 = load <4 x i8>, ptr %f3, align 4
  %e3 = extractelement <4 x i8> %v3, i64 3
  %ok4 = icmp eq i8 %e3, 4

  ; extractvalue of a field.
  %ev = extractvalue %S %c, 2
  %e4 = extractelement <2 x i16> %ev, i64 0
  %ok5 = icmp eq i16 %e4, 300

  ; An array of <2 x i16> has a stride of 4 bytes.
  %arr = alloca [4 x <2 x i16>], align 4
  call void @llvm.memset.p0.i64(ptr %arr, i8 0, i64 16, i1 false)
  %x = call i32 @id(i32 9)
  %x16 = trunc i32 %x to i16
  %w = insertelement <2 x i16> zeroinitializer, i16 %x16, i64 1
  %slot = getelementptr [4 x <2 x i16>], ptr %arr, i64 0, i64 2
  store <2 x i16> %w, ptr %slot, align 4
  %raw = getelementptr i8, ptr %arr, i64 10
  %rawv = load i16, ptr %raw
  %ok6 = icmp eq i16 %rawv, 9
  %back = load <2 x i16>, ptr %slot, align 4
  %be = extractelement <2 x i16> %back, i64 1
  %ok7 = icmp eq i16 %be, 9

  ; extractvalue and insertvalue on an array of <2 x i8>.
  %agg = load [3 x <2 x i8>], ptr @global_arr
  %el = extractvalue [3 x <2 x i8>] %agg, 2
  %ee = extractelement <2 x i8> %el, i64 1
  %ok8 = icmp eq i8 %ee, 6
  %agg2 = insertvalue [3 x <2 x i8>] %agg, <2 x i8> <i8 9, i8 8>, 1
  %el2 = extractvalue [3 x <2 x i8>] %agg2, 1
  %ee2 = extractelement <2 x i8> %el2, i64 0
  %ok9 = icmp eq i8 %ee2, 9
  %el3 = extractvalue [3 x <2 x i8>] %agg2, 2
  %ee3 = extractelement <2 x i8> %el3, i64 0
  %ok10 = icmp eq i8 %ee3, 5

  %a1 = and i1 %ok1, %ok2
  %a2 = and i1 %ok3, %ok4
  %a3 = and i1 %ok5, %ok6
  %a4 = and i1 %ok7, %ok8
  %a5 = and i1 %ok9, %ok10
  %b1x = and i1 %a1, %a2
  %b2x = and i1 %a3, %a4
  %b3x = and i1 %b1x, %b2x
  %all = and i1 %b3x, %a5
  %r = select i1 %all, i32 0, i32 1
  ret i32 %r
}

@global_arr = global [3 x <2 x i8>] [<2 x i8> <i8 1, i8 2>, <2 x i8> <i8 3, i8 4>, <2 x i8> <i8 5, i8 6>]

; Shuffles that combine small vectors, and operations that fill all lanes.
define i32 @check_lanes() {
  %x = call i32 @id(i32 1)
  %x8 = trunc i32 %x to i8
  %a = insertelement <2 x i8> <i8 0, i8 2>, i8 %x8, i64 0
  %b = insertelement <2 x i8> <i8 3, i8 4>, i8 %x8, i64 1

  ; <2 x i8> + <2 x i8> -> <4 x i8>: lanes 0 and 1 of the result come from a, 2 and 3 from b.
  %c = shufflevector <2 x i8> %a, <2 x i8> %b, <4 x i32> <i32 0, i32 1, i32 2, i32 3>
  %c2 = extractelement <4 x i8> %c, i64 2
  %c3 = extractelement <4 x i8> %c, i64 3
  %ok1 = icmp eq i8 %c2, 3
  %ok2 = icmp eq i8 %c3, 1

  ; Reversal.
  %d = shufflevector <4 x i8> %c, <4 x i8> poison, <4 x i32> <i32 3, i32 2, i32 1, i32 0>
  %d0 = extractelement <4 x i8> %d, i64 0
  %ok3 = icmp eq i8 %d0, 1

  ; <2 x i16> + <2 x i16> -> <4 x i16>.
  %x16 = sext i8 %x8 to i16
  %p = insertelement <2 x i16> <i16 10, i16 20>, i16 %x16, i64 0
  %q = insertelement <2 x i16> <i16 30, i16 40>, i16 %x16, i64 0
  %pq = shufflevector <2 x i16> %p, <2 x i16> %q, <4 x i32> <i32 0, i32 1, i32 2, i32 3>
  %pq3 = extractelement <4 x i16> %pq, i64 3
  %pq2 = extractelement <4 x i16> %pq, i64 2
  %ok4 = icmp eq i16 %pq3, 40
  %ok5 = icmp eq i16 %pq2, 1

  ; Arithmetic: a is <1, 2> and b is <3, 1>.
  %sum = add <2 x i8> %a, %b
  %s0 = extractelement <2 x i8> %sum, i64 0
  %s1 = extractelement <2 x i8> %sum, i64 1
  %ok6 = icmp eq i8 %s0, 4
  %ok7 = icmp eq i8 %s1, 3

  ; Comparison, select and extension.
  %cmp = icmp slt <4 x i8> %c, <i8 2, i8 2, i8 2, i8 2>
  %sel = select <4 x i1> %cmp, <4 x i8> <i8 100, i8 100, i8 100, i8 100>, <4 x i8> %c
  %sel2 = extractelement <4 x i8> %sel, i64 2
  %sel0 = extractelement <4 x i8> %sel, i64 0
  %ok8 = icmp eq i8 %sel2, 3
  %ok9 = icmp eq i8 %sel0, 100
  %ext = sext <4 x i8> %c to <4 x i32>
  %ext3 = extractelement <4 x i32> %ext, i64 3
  %ok10 = icmp eq i32 %ext3, 1
  %zext = zext <2 x i16> %p to <2 x i32>
  %zext1 = extractelement <2 x i32> %zext, i64 1
  %ok11 = icmp eq i32 %zext1, 20

  ; A splat.
  %ins = insertelement <2 x i16> poison, i16 %x16, i64 0
  %splat = shufflevector <2 x i16> %ins, <2 x i16> poison, <2 x i32> zeroinitializer
  %sp1 = extractelement <2 x i16> %splat, i64 1
  %ok12 = icmp eq i16 %sp1, 1

  %t1 = and i1 %ok1, %ok2
  %t2 = and i1 %ok3, %ok4
  %t3 = and i1 %ok5, %ok6
  %t4 = and i1 %ok7, %ok8
  %t5 = and i1 %ok9, %ok10
  %t6 = and i1 %ok11, %ok12
  %u1 = and i1 %t1, %t2
  %u2 = and i1 %t3, %t4
  %u3 = and i1 %t5, %t6
  %v1 = and i1 %u1, %u2
  %all = and i1 %v1, %u3
  %r = select i1 %all, i32 0, i32 1
  ret i32 %r
}

define i32 @main() {
  %r1 = call i32 @check_load_store()
  %f1 = icmp ne i32 %r1, 0
  br i1 %f1, label %fail1, label %next1
fail1:
  ret i32 1
next1:
  %r2 = call i32 @check_bitcast()
  %f2 = icmp ne i32 %r2, 0
  br i1 %f2, label %fail2, label %next2
fail2:
  ret i32 2
next2:
  %r3 = call i32 @check_struct_and_array()
  %f3 = icmp ne i32 %r3, 0
  br i1 %f3, label %fail3, label %next3
fail3:
  ret i32 3
next3:
  %r4 = call i32 @check_lanes()
  %f4 = icmp ne i32 %r4, 0
  br i1 %f4, label %fail4, label %done
fail4:
  ret i32 4
done:
  ret i32 0
}

declare void @llvm.memset.p0.i64(ptr, i8, i64, i1)
