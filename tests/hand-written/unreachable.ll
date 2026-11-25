; `unreachable` after a branch that is never taken, in the default of a `switch` and after calls that do not return.
; The inputs come through @id, so nothing is folded away. The program prints what it did, then ends with exit(7)
; from a function that is noreturn, so it checks the exit code too.

declare i32 @printf(ptr, ...)
declare void @exit(i32) noreturn

@fmt = private constant [4 x i8] c"%d\0A\00"

define i32 @id(i32 %x) {
  ret i32 %x
}

; Never-taken branch to unreachable.
define i32 @guarded(i32 %x) {
entry:
  %bad = icmp slt i32 %x, 0
  br i1 %bad, label %trap, label %ok
trap:
  unreachable
ok:
  %r = add i32 %x, 1
  ret i32 %r
}

; switch with an unreachable default: every value the switch sees has a case.
define i32 @classify(i32 %x) {
entry:
  switch i32 %x, label %impossible [
    i32 0, label %zero
    i32 1, label %one
    i32 2, label %two
  ]
zero:
  ret i32 10
one:
  ret i32 20
two:
  ret i32 30
impossible:
  unreachable
}

; A function whose body is a noreturn call followed by unreachable.
define void @finish(i32 %code) noreturn {
  call void @exit(i32 %code)
  unreachable
}

; A noreturn call on one side of a branch, with the other side carrying on.
define i32 @maybe_finish(i32 %x) {
entry:
  %stop = icmp eq i32 %x, 99
  br i1 %stop, label %die, label %go
die:
  call void @finish(i32 3)
  unreachable
go:
  %r = mul i32 %x, 2
  ret i32 %r
}

define i32 @main() {
entry:
  %a = call i32 @id(i32 5)
  %b = call i32 @guarded(i32 %a)
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %b)

  %c0 = call i32 @classify(i32 0)
  %c1 = call i32 @classify(i32 1)
  %c2 = call i32 @classify(i32 2)
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %c0)
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %c1)
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %c2)

  %m = call i32 @id(i32 21)
  %n = call i32 @maybe_finish(i32 %m)
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %n)

  ; Ends the program: the last line is the exit code.
  call void @finish(i32 7)
  unreachable
}
