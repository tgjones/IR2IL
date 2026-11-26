; Debug metadata as debug records (#dbg_declare, #dbg_value, #dbg_label; see debug_intrinsic_calls.ll for the old
; intrinsic calls), instruction locations in several scopes and files (inlined
; scopes included), and a debug-info-carrying function that is variadic. None of it may change the result.

declare i32 @printf(ptr, ...)

@fmt = private constant [4 x i8] c"%d\0A\00"

define i32 @compute(i32 %x) !dbg !10 {
entry:
  %slot = alloca i32, align 4
    #dbg_declare(ptr %slot, !16, !DIExpression(), !20)
  store i32 %x, ptr %slot, !dbg !20
  %v = load i32, ptr %slot, !dbg !21
    #dbg_value(i32 %v, !17, !DIExpression(), !21)
  %w = mul i32 %v, 3, !dbg !22
    #dbg_value(i32 %w, !17, !DIExpression(DW_OP_plus_uconst, 1, DW_OP_stack_value), !22)
  %c = icmp sgt i32 %w, 10, !dbg !22
  br i1 %c, label %big, label %small, !dbg !22

big:
    #dbg_label(!18, !23)
  %b = sub i32 %w, 10, !dbg !23
  br label %done, !dbg !23

small:
  %s = add i32 %w, 100, !dbg !24
  br label %done, !dbg !24

done:
  %r = phi i32 [ %b, %big ], [ %s, %small ], !dbg !25
  ret i32 %r, !dbg !25
}

define i32 @old_style(i32 %x) !dbg !12 {
entry:
  %slot = alloca i32, align 4
    #dbg_declare(ptr %slot, !26, !DIExpression(), !27)
  store i32 %x, ptr %slot, !dbg !27
  %v = load i32, ptr %slot, !dbg !28
    #dbg_value(i32 %v, !26, !DIExpression(), !28)
  %r = add i32 %v, 7, !dbg !28
  ret i32 %r, !dbg !28
}

define i32 @sum(i32 %n, ...) !dbg !14 {
  ret i32 %n, !dbg !29
}

define i32 @main() !dbg !30 {
  %a = call i32 @compute(i32 2), !dbg !31
  %b = call i32 @compute(i32 9), !dbg !32
  %c = call i32 @old_style(i32 35), !dbg !33
  %d = call i32 (i32, ...) @sum(i32 5, i32 1), !dbg !33
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %a), !dbg !33
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %b), !dbg !33
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %c), !dbg !33
  call i32 (ptr, ...) @printf(ptr @fmt, i32 %d), !dbg !33
  ret i32 0, !dbg !33
}

!llvm.dbg.cu = !{!0}
!llvm.module.flags = !{!5, !6}

!0 = distinct !DICompileUnit(language: DW_LANG_C11, file: !1, producer: "hand written", isOptimized: false, runtimeVersion: 0, emissionKind: FullDebug)
!1 = !DIFile(filename: "debug_metadata.c", directory: "/tmp")
!2 = !DIFile(filename: "other.h", directory: "/tmp")
!5 = !{i32 2, !"Debug Info Version", i32 3}
!6 = !{i32 7, !"Dwarf Version", i32 5}
!7 = !DIBasicType(name: "int", size: 32, encoding: DW_ATE_signed)
!8 = !DISubroutineType(types: !9)
!9 = !{!7, !7}
!10 = distinct !DISubprogram(name: "compute", scope: !1, file: !1, line: 3, type: !8, scopeLine: 3, spFlags: DISPFlagDefinition, unit: !0)
!12 = distinct !DISubprogram(name: "old_style", scope: !1, file: !1, line: 20, type: !8, scopeLine: 20, spFlags: DISPFlagDefinition, unit: !0)
!14 = distinct !DISubprogram(name: "sum", scope: !2, file: !2, line: 1, type: !8, scopeLine: 1, spFlags: DISPFlagDefinition, unit: !0)
!16 = !DILocalVariable(name: "slot", scope: !10, file: !1, line: 4, type: !7)
!17 = !DILocalVariable(name: "tmp", scope: !10, file: !1, line: 5, type: !7)
!18 = !DILabel(scope: !10, name: "big", file: !1, line: 9)
!20 = !DILocation(line: 4, column: 3, scope: !10)
!21 = !DILocation(line: 5, column: 3, scope: !10)
!22 = !DILocation(line: 6, column: 3, scope: !10)
!23 = !DILocation(line: 9, column: 3, scope: !10)
!24 = !DILocation(line: 12, column: 3, scope: !10)
!25 = !DILocation(line: 14, column: 3, scope: !10)
!26 = !DILocalVariable(name: "slot", scope: !12, file: !1, line: 21, type: !7)
!27 = !DILocation(line: 21, column: 3, scope: !12)
!28 = !DILocation(line: 22, column: 3, scope: !12)
!29 = !DILocation(line: 2, column: 3, scope: !14)
!30 = distinct !DISubprogram(name: "main", scope: !1, file: !1, line: 30, type: !8, scopeLine: 30, spFlags: DISPFlagDefinition, unit: !0)
!31 = !DILocation(line: 31, column: 3, scope: !30)
!32 = !DILocation(line: 32, column: 3, scope: !30)
!33 = !DILocation(line: 33, column: 3, scope: !30)
