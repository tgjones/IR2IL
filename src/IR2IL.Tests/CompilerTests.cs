using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace IR2IL.Tests;

[TestClass]
public partial class CompilerTests
{
    private static readonly string RepoRoot = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "..");
    private static readonly string TestProgramsPath = Path.Combine(RepoRoot, "tests");
    private static readonly string ClangPath = Path.Combine(RepoRoot, "build", "llvm", "bin", "clang.exe");

    private static IEnumerable<object[]> TestFiles(IEnumerable<string> testFiles)
    {
        var optimizationLevels = new string[]
        {
            "O0",
            "O3",
        };

        return from testFile in testFiles
               from optimizationLevel in optimizationLevels
               select new object[] { testFile, optimizationLevel };
    }

    public static string TestDataDisplayName(MethodInfo methodInfo, object[] values)
    {
        return $"{methodInfo.Name}({Path.GetFileName((string)values[0])}, {values[1]})";
    }

    private static IEnumerable<object[]> TestDataArbitrary() => TestFiles(
        Directory.GetFiles(Path.Combine(TestProgramsPath, "arbitrary"), "*.c", SearchOption.AllDirectories)
        .Concat(Directory.GetFiles(Path.Combine(TestProgramsPath, "arbitrary"), "*.cpp", SearchOption.AllDirectories)));

    [TestMethod]
    [DynamicData(nameof(TestDataArbitrary), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestDataDisplayName))]
    public void Arbitrary(string testName, string optimizationLevel)
    {
        CompileAndExecuteManaged(
            testName,
            optimizationLevel,
            out var managedExitCode,
            out var managedStandardOutput,
            out var managedStandardError);

        // Compile to executable binary.
        var binaryPath = GetOutputPath(testName, optimizationLevel) + "_native.exe";
        RunClang([GetSourceFilePath(testName), "-o", binaryPath, $"-{optimizationLevel}"]);

        RunProgram(
            binaryPath,
            [],
            out var llvmExitCode,
            out var llvmStandardOutput,
            out var llvmStandardError);

        Assert.AreEqual(llvmStandardError, managedStandardError);
        Assert.AreEqual(llvmStandardOutput, managedStandardOutput);
        Assert.AreEqual(llvmExitCode, managedExitCode);

        Console.WriteLine($"ExitCode: {managedExitCode}");
        Console.WriteLine($"Stdout: {managedStandardOutput}");
    }

    private static IEnumerable<object[]> TestDataCTestSuite() => TestFiles(
        Directory
        .GetFiles(Path.Combine(TestProgramsPath, "c-testsuite"), "*.c", SearchOption.AllDirectories)
        .Where(x => Path.GetFileNameWithoutExtension(x) switch
        {
            // These tests are not supported on Windows because they use `extern int printf(...)`
            // which isn't compatible with Microsoft's C runtime.
            "00210" or "00211" or "00213" or "00214" or "00215" or "00217" or "00218" => false,

            // These tests are not supported on Windows because they produce different
            // output than the expected output.
            "00212" or "00216" => false,

            _ => true,
        }));

    [TestMethod]
    [DynamicData(nameof(TestDataCTestSuite), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestDataDisplayName))]
    public void CTestSuite(string testName, string optimizationLevel)
    {
        CompileAndExecuteManaged(
            testName,
            optimizationLevel,
            out var managedExitCode,
            out var managedStandardOutput,
            out var managedStandardError);

        var nativeStandardOutput = File
            .ReadAllText(GetSourceFilePath(testName) + ".expected")
            .ReplaceLineEndings();

        Assert.AreEqual("", managedStandardError);
        Assert.AreEqual(nativeStandardOutput, managedStandardOutput);
        Assert.AreEqual(0, managedExitCode);

        Console.WriteLine($"Stdout: {managedStandardOutput}");
    }

    private static IEnumerable<object[]> TestDataFujitsuCompilerTestSuite() => TestFiles(
        Directory.GetFiles(Path.Combine(TestProgramsPath, "fujitsu-compiler-test-suite"), "*.c", SearchOption.AllDirectories)
        .Where(x => Path.GetFileNameWithoutExtension(x) switch
        {
            // These tests cover undefined behavior for bitshifting beyond the size of the operand.
            "0000_0025" or "0000_0026" or "0000_0031" or "0000_0032" or "0000_0034" or "0000_0035" => false,
            "0000_0048" or "0000_0049" or "0000_0050" or "0000_0051" or "0000_0060" or "0000_0061" => false,
            "0000_0062" or "0000_0063" or "0000_0066" or "0000_0067" or "0000_0068" or "0000_0069" => false,

            // These tests don't compile correctly on MSVC because of _Complex type differences.
            "0000_0078" or "0000_0079" or "0000_0080" or "0000_0089" or "0000_0090" or "0000_0091" => false,
            "0000_0192" or "0005_0058" or "0006_0000" or "0006_0009" or "0006_0010" or "0006_0011" => false,
            "0006_0024" or "0006_0025" or "0006_0026" => false,

            // These tests don't run correctly on MSVC because of _Generic differences.
            "0005_0005" => false,

            // These tests don't compile correctly on MSVC because of fpclassify type differences.
            "0005_0006" => false,

            // These tests require GLIBC defines.
            "0005_0017" or "0005_0018" or "0005_0019" or "0005_0020" or "0005_0021" or "0005_0022" => false,
            "0005_0028" or "0005_0029" or "0005_0030" or "0005_0045" or "0005_0063" => false,

            // These tests don't compile correctly on MSVC beacuse of align library differences.
            "0005_0032" or "0005_0033" => false,

            // These tests don't execute correctly on MSVC beacuse of OpenMP differences.
            "0005_0036" => false,

            // These tests don't execute correctly because quick_exit behaves differently when called from CoreCLR.
            "0005_0040" or "0005_0041" or "0005_0042" or "0005_0043" => false,

            // These tests don't execute correctly on MSVC because of stdnoreturn differences.
            "0005_0044" => false,

            // These tests don't execute correctly on MSVC because of x64 preprocessor define differences.
            "0005_0055" => false,

            // These tests don't execute correctly on MSVC because of float define differences.
            "0005_0051" or "0005_0053" or "0005_0056" => false,

            // These tests don't execute correctly on MSVC because of thread_local differences.
            "0005_0062" => false,

            // These tests don't execute correctly on Windows because of the stack being too large.
            "0008_0014" or "0008_0015" or "0008_0016" or "0008_0017" or "0008_0018" or "0008_0019" => false,
            "0008_0020" or "0008_0021" or "0008_0022" or "0008_0023" or "0008_0024" or "0008_0025" => false,
            "0008_0026" or "0008_0027" or "0008_0028" or "0008_0029" or "0008_0030" or "0008_0031" => false,
            "0008_0032" or "0008_0033" or "0008_0034" or "0008_0035" or "0008_0036" or "0008_0037" => false,
            "0008_0038" => false,

            // These tests cover undefined behavior for negative-floating-point-to-unsigned-int conversion.
            "0009_0015" or "0009_0016" or "0009_0020" or "0009_0021" => false,

            // These tests use llvm.frameaddress, which we can't (easily) support.
            "0011_0226" => false,

            // These tests don't execute correctly on MSVC because of "long" data type differences.
            "0011_0240" or "0011_0259" => false,

            // These tests don't execute correctly on MSVC because of __intptr_t / intptr_t differences.
            "0011_0247" => false,

            // These tests don't execute correctly on MSVC because of __STDC_VERSION__ differences.
            "0017_0031" or "0017_0032" or "0017_0033" or "0017_0034" or "0017_0035" => false,

            // These tests don't execute correctly because of restrict differences.
            "0019_0000" or "0019_0001" or "0019_0006" => false,

            _ => true,
        })
        // These tests are not supported on Windows because they use `extern int printf(...)`
        // which isn't compatible with Microsoft's C runtime.
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0021_"))
        // These tests cover Half, which .NET does have, but not yet for Vector128 etc.
        // We could implement the O0 versions, but not the O3 versions.
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0022_"))
        // These tests cover bitfields which have different alignment on MSVC / Windows.
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0014_"))
        // These tests don't compile correctly on MSVC because of _Complex type differences.
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0016_"))
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0018_"))
        // These tests don't execute correctly because of restrict differences.
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0020_"))

        // TODO: Support more tests.
        .Where(x => int.Parse(Path.GetFileNameWithoutExtension(x).Substring(0, 4)) <= 0020));

    [GeneratedRegex(@"exit (\d+)")]
    private static partial Regex FujitsuCompilerTestSuiteExitCodeRegex();

    [TestMethod]
    [DynamicData(nameof(TestDataFujitsuCompilerTestSuite), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestDataDisplayName))]
    public void FujitsuCompilerTestSuite(string testName, string optimizationLevel)
    {
        CompileAndExecuteManaged(
            testName,
            optimizationLevel,
            out var managedExitCode,
            out var managedStandardOutput,
            out var managedStandardError);

        Assert.AreEqual("", managedStandardError);

        var referenceOutputFilePath = Path.ChangeExtension(GetSourceFilePath(testName), ".reference_output");
        if (File.Exists(referenceOutputFilePath))
        {
            var referenceOutputLines = File.ReadLines(referenceOutputFilePath, Encoding.ASCII);

            var nativeStandardOutputBuilder = new StringBuilder();
            int? nativeExitCode = null;

            foreach (var referenceOutputLine in referenceOutputLines)
            {
                var nativeExitCodeMatch = FujitsuCompilerTestSuiteExitCodeRegex().Match(referenceOutputLine);
                if (nativeExitCodeMatch.Success)
                {
                    nativeExitCode = int.Parse(nativeExitCodeMatch.Groups[1].Value);
                    break;
                }

                nativeStandardOutputBuilder.AppendLine(referenceOutputLine);
            }

            if (nativeExitCode == null)
            {
                throw new InvalidOperationException("Missing exit code");
            }

            var nativeStandardOutput = nativeStandardOutputBuilder.ToString();
            Assert.AreEqual(nativeStandardOutput, managedStandardOutput);
            Assert.AreEqual(nativeExitCode, managedExitCode);
        }

        Console.WriteLine($"Stdout: {managedStandardOutput}");
    }

    private static IEnumerable<object[]> TestDataBenchmarks() => TestFiles(
        Directory
        .GetFiles(Path.Combine(TestProgramsPath, "benchmarks"), "*.c", SearchOption.AllDirectories));

    [TestMethod]
    [DynamicData(nameof(TestDataBenchmarks), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestDataDisplayName))]
    public void Benchmark(string testName, string optimizationLevel)
    {
        var managedExePath = CompileManaged(testName, optimizationLevel);

        // Compile to executable binary.
        var binaryPath = GetOutputPath(testName, optimizationLevel) + "_native.exe";
        RunClang([GetSourceFilePath(testName), "-o", binaryPath, $"-{optimizationLevel}"]);

        var stopwatch = Stopwatch.StartNew();

        RunProgram(
            "dotnet",
            [managedExePath],
            out var managedExitCode,
            out var managedStandardOutput,
            out var managedStandardError);

        Console.WriteLine($"Managed: {stopwatch.Elapsed}");

        stopwatch.Restart();

        RunProgram(
            binaryPath,
            [],
            out var llvmExitCode,
            out var llvmStandardOutput,
            out _);

        Console.WriteLine($"Native:  {stopwatch.Elapsed}");

        Assert.AreEqual(llvmExitCode, managedExitCode, managedStandardError);
        Assert.AreEqual(llvmStandardOutput, managedStandardOutput);

        Console.WriteLine($"Stdout: {managedStandardOutput}");
    }

    private static string GetOutputPath(string testName, string optimizationLevel)
    {
        var outputFilePath = $"{Path.Combine(Environment.CurrentDirectory, "output", Path.GetRelativePath(TestProgramsPath, testName))}_{optimizationLevel}";
        outputFilePath = Path.Combine(outputFilePath, Path.GetFileName(outputFilePath));
        var outputDirectory = Path.GetDirectoryName(outputFilePath) ?? throw new InvalidOperationException("No directory");
        Directory.CreateDirectory(outputDirectory);
        return outputFilePath;
    }

    private static string GetSourceFilePath(string testName) => Path.Combine(TestProgramsPath, testName);

    private static void CompileAndExecuteManaged(
        string testName,
        string optimizationLevel,
        out int managedExitCode,
        out string managedStandardOutput,
        out string managedStandardError)
    {
        var outputPath = CompileManaged(
            testName,
            optimizationLevel);

        ExecuteManaged(
            outputPath,
            out managedExitCode,
            out managedStandardOutput,
            out managedStandardError);

        File.WriteAllText(
            Path.ChangeExtension(outputPath, ".output"),
            managedStandardOutput);
    }

    private static string CompileManaged(string testName, string optimizationLevel)
    {
        var fullTestName = GetOutputPath(testName, optimizationLevel);

        var irPath = fullTestName + ".ll";

        // Compile to LLVM IR.
        RunClang([GetSourceFilePath(testName), "-g", "-o", irPath, "-emit-llvm", "-S", $"-{optimizationLevel}"]);

        var outputPath = $"{fullTestName}.exe";
        Compiler.Compile(irPath, outputPath);

        return outputPath;
    }

    private static void ExecuteManaged(
        string managedExePath,
        out int managedExitCode,
        out string managedStandardOutput,
        out string managedStandardError)
    {
        RunProgram(
            "dotnet",
            [managedExePath],
            out managedExitCode,
            out managedStandardOutput,
            out managedStandardError);
    }

    private static void RunProgram(
        string executablePath,
        string[] arguments,
        out int exitCode,
        out string standardOutput,
        out string standardError)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.ASCII,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process
        {
            StartInfo = startInfo,
        };

        if (!process.Start())
        {
            throw new InvalidOperationException();
        }

        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();

        process.WaitForExit();

        exitCode = process.ExitCode;

        standardOutput = standardOutputTask.Result;
        standardError = standardErrorTask.Result;
    }

    private static void RunClang(string[] arguments)
    {
        RunProgram(
            ClangPath,
            arguments,
            out var exitCode,
            out var standardOutput,
            out var standardError);

        if (exitCode != 0)
        {
            throw new InvalidOperationException(standardOutput + Environment.NewLine + standardError);
        }
    }
}