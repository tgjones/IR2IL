; A GEP index may be any integer type and is signed. Each index is a negative number produced by a
; `trunc`, which leaves it zero-extended on the IL stack (253 for i8 -3), so it has to be sign-extended
; before it is added to the pointer. a[4 - 3] + a[4 - 2] + a[4 - 1] = 1 + 2 + 3.

@a = global [8 x i32] [i32 0, i32 1, i32 2, i32 3, i32 4, i32 5, i32 6, i32 7]

define i64 @id(i64 %x) {
  ret i64 %x
}

define i32 @main() {
  %mid = getelementptr [8 x i32], ptr @a, i64 0, i64 4

  %x8 = call i64 @id(i64 253)
  %i8 = trunc i64 %x8 to i8
  %p8 = getelementptr i32, ptr %mid, i8 %i8
  %v8 = load i32, ptr %p8

  %x16 = call i64 @id(i64 65534)
  %i16 = trunc i64 %x16 to i16
  %p16 = getelementptr i32, ptr %mid, i16 %i16
  %v16 = load i32, ptr %p16

  %x32 = call i64 @id(i64 4294967295)
  %i32 = trunc i64 %x32 to i32
  %p32 = getelementptr i32, ptr %mid, i32 %i32
  %v32 = load i32, ptr %p32

  %s1 = add i32 %v8, %v16
  %s2 = add i32 %s1, %v32
  ret i32 %s2
}
