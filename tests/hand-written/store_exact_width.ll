; exit-code: 0
; Storing an iN writes ceil(N/8) bytes: the bytes after it must not change.
; The i8 slots hold 0x55 around an i24, an i40 and an i72, written through a stray-high-bits value.

define i32 @id(i32 %x) {
  ret i32 %x
}

define i32 @main() {
  %buf = alloca [32 x i8], align 8
  call void @llvm.memset.p0.i64(ptr %buf, i8 85, i64 32, i1 false)

  ; i24 at offset 0: 3 bytes, byte 3 must stay 0x55.
  %a = call i32 @id(i32 -1)
  %a24 = trunc i32 %a to i24
  store i24 %a24, ptr %buf, align 1
  %p3 = getelementptr i8, ptr %buf, i64 3
  %b3 = load i8, ptr %p3
  %ok1 = icmp eq i8 %b3, 85

  ; i40 at offset 8: 5 bytes, byte 13 must stay 0x55.
  %c = call i32 @id(i32 -1)
  %c64 = sext i32 %c to i64
  %c40 = trunc i64 %c64 to i40
  %p8 = getelementptr i8, ptr %buf, i64 8
  store i40 %c40, ptr %p8, align 1
  %p13 = getelementptr i8, ptr %buf, i64 13
  %b13 = load i8, ptr %p13
  %ok2 = icmp eq i8 %b13, 85

  ; i72 at offset 16: 9 bytes, byte 25 must stay 0x55.
  %d = call i32 @id(i32 -1)
  %d128 = sext i32 %d to i128
  %d72 = trunc i128 %d128 to i72
  %p16 = getelementptr i8, ptr %buf, i64 16
  store i72 %d72, ptr %p16, align 1
  %p25 = getelementptr i8, ptr %buf, i64 25
  %b25 = load i8, ptr %p25
  %ok3 = icmp eq i8 %b25, 85

  ; the bytes written are all ones.
  %p2 = getelementptr i8, ptr %buf, i64 2
  %b2 = load i8, ptr %p2
  %ok4 = icmp eq i8 %b2, -1

  %and1 = and i1 %ok1, %ok2
  %and2 = and i1 %ok3, %ok4
  %all = and i1 %and1, %and2
  %res = select i1 %all, i32 0, i32 1
  ret i32 %res
}

declare void @llvm.memset.p0.i64(ptr, i8, i64, i1)
