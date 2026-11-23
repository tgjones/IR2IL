; `inttoptr` zero-extends its operand: i32 -1 becomes 0xFFFFFFFF and i8 -1 becomes 0xFF, not all ones.
; 1 for the i32 case, 2 for the i8 case.

define i64 @id(i64 %x) {
  ret i64 %x
}

define i32 @main() {
  %x = call i64 @id(i64 4294967295)
  %i32 = trunc i64 %x to i32
  %p32 = inttoptr i32 %i32 to ptr
  %n32 = ptrtoint ptr %p32 to i64
  %ok32 = icmp eq i64 %n32, 4294967295
  %r32 = zext i1 %ok32 to i32

  %i8 = trunc i64 %x to i8
  %p8 = inttoptr i8 %i8 to ptr
  %n8 = ptrtoint ptr %p8 to i64
  %ok8 = icmp eq i64 %n8, 255
  %r8 = zext i1 %ok8 to i32
  %r8s = shl i32 %r8, 1

  %r = add i32 %r32, %r8s
  ret i32 %r
}
