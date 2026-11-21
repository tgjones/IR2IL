using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IR2IL.Tests;

/// <summary>
/// Tests that compile the hand-written LLVM IR files in <c>tests/hand-written</c> (no clang involved).
/// Each file declares the exit code expected from <c>main</c> in a <c>; exit-code: N</c> comment.
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

    [GeneratedRegex(@"^;\s*exit-code:\s*(-?\d+)\s*$", RegexOptions.Multiline)]
    private static partial Regex HandWrittenExitCodeRegex();

    [TestMethod]
    [DynamicData(nameof(TestDataHandWritten), DynamicDataDisplayName = nameof(HandWrittenDisplayName))]
    public void HandWritten(string irPath)
    {
        var match = HandWrittenExitCodeRegex().Match(File.ReadAllText(irPath));
        Assert.IsTrue(match.Success, $"{irPath} has no '; exit-code: N' comment");
        var expectedExitCode = int.Parse(match.Groups[1].Value);

        var outputPath = Path.Combine(
            Environment.CurrentDirectory,
            "output",
            "hand-written",
            Path.GetFileNameWithoutExtension(irPath) + ".exe");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        Compiler.Compile(irPath, outputPath);
        _compiledOutputPath = outputPath;

        ExecuteManaged(outputPath, out var exitCode, out _, out var standardError);

        Assert.AreEqual("", NormalizeLineEndings(standardError));
        Assert.AreEqual(expectedExitCode, exitCode);
    }
}
