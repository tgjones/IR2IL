; exit-code: 7
; The switch condition is an inlined expression and the cases form several clusters
; ({1,2}, {10}, {20,21}), so it is tested more than once. 19 + 1 = 20 is in the last cluster.

define i32 @id(i32 %x) {
  ret i32 %x
}

define i32 @main() {
  %x = call i32 @id(i32 19)
  %c = add i32 %x, 1
  switch i32 %c, label %other [
    i32 1, label %a
    i32 2, label %a
    i32 10, label %b
    i32 20, label %c20
    i32 21, label %c20
  ]

a:
  ret i32 1

b:
  ret i32 2

c20:
  ret i32 7

other:
  ret i32 99
}
