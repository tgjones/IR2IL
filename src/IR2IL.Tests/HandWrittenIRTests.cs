using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IR2IL.Tests;

/// <summary>
/// Tests that compile the hand-written LLVM IR files in <c>tests/hand-written</c>. Clang is the oracle:
/// it builds each file into a native executable, and the IR2IL build must produce the same exit code,
/// standard output and standard error. The files must therefore avoid undefined behaviour and anything
/// else that may differ between targets. Self-checking files return a non-zero code (or print) to say
/// what failed, so a wrong compiler shows up even when both sides were to agree on a bad result.
/// </summary>
public partial class CompilerTests
{
    private static IEnumerable<object[]> TestDataHandWritten() =>
        Directory
        .GetFiles(Path.Combine(TestProgramsPath, "hand-written"), "*.ll")
        .Order(StringComparer.Ordinal)
        .Select(x => new object[] { x });

    public static string HandWrittenDisplayName(MethodInfo methodInfo, object[] values) =>
        $"{methodInfo.Name}({Path.GetFileName((string)values[0])})";

    [TestMethod]
    [DynamicData(nameof(TestDataHandWritten), DynamicDataDisplayName = nameof(HandWrittenDisplayName))]
    public void HandWritten(string irPath)
    {
        // A directory per test: the compiler copies IR2IL.Runtime.dll next to the output, which fails
        // if another test running in parallel has already loaded the copy in the shared directory.
        var testName = Path.GetFileNameWithoutExtension(irPath);
        var outputPath = Path.Combine(Environment.CurrentDirectory, "output", "hand-written", testName, testName + ".exe");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        Compiler.Compile(irPath, outputPath);
        _compiledOutputPath = outputPath;

        ExecuteManaged(outputPath, out var exitCode, out var standardOutput, out var standardError);

        var nativePath = Path.Combine(Path.GetDirectoryName(outputPath)!, testName + "_native.exe");
        RunClangCached(irPath, nativePath, "-Wno-override-module");
        RunProgram(nativePath, [], out var nativeExitCode, out var nativeStandardOutput, out var nativeStandardError);

        Assert.AreEqual(NormalizeLineEndings(nativeStandardError), NormalizeLineEndings(standardError));
        Assert.AreEqual(NormalizeLineEndings(nativeStandardOutput), NormalizeLineEndings(standardOutput));
        Assert.AreEqual(nativeExitCode, exitCode, standardOutput);
    }
}
