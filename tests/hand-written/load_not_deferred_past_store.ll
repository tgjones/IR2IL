; `%a` has a single use, further down. It must still observe the value loaded before the store.

define i32 @id(i32 %x) {
  ret i32 %x
}

define i32 @main() {
  %p = alloca i32
  store i32 5, ptr %p
  %v = load i32, ptr %p
  %a = add i32 %v, 1
  store i32 0, ptr %p
  %r = call i32 @id(i32 %a)
  ret i32 %r
}
