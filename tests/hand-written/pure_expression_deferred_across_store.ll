; exit-code: 5
; A store between a pure expression and its user doesn't change the result.

define i32 @id(i32 %x) {
  ret i32 %x
}

define i32 @main() {
  %p = alloca i32
  %a = add i32 2, 3
  store i32 0, ptr %p
  %r = call i32 @id(i32 %a)
  ret i32 %r
}
