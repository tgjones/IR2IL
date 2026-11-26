using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace IR2IL.Tests;

[TestClass]
public partial class CompilerTests
{
    private static readonly string RepoRoot = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "..");
    private static readonly string TestProgramsPath = Path.Combine(RepoRoot, "tests");
    private static readonly string ClangPath = Path.Combine(RepoRoot, "build", "llvm", "bin", OperatingSystem.IsWindows() ? "clang.exe" : "clang");
    private static readonly string[] ClangExtraArgs = GetClangExtraArgs();

    /// <summary>
    /// Set <c>IR2IL_DUMP=1</c> to always write the disassembled IL (.il) and decompiled C# (.cs) next to
    /// each compiled test. Without it they are only written when a test fails, because decompiling the
    /// whole module is expensive.
    /// </summary>
    private static readonly bool AlwaysDump = Environment.GetEnvironmentVariable("IR2IL_DUMP") is "1" or "true";

    // Path of the managed executable compiled by the current test, if any.
    private string? _compiledOutputPath;

    public TestContext TestContext { get; set; } = null!;

    [TestCleanup]
    public void WriteDebugDumpsIfNeeded()
    {
        if (_compiledOutputPath == null || !File.Exists(_compiledOutputPath))
        {
            return;
        }

        if (!AlwaysDump && TestContext.CurrentTestOutcome == UnitTestOutcome.Passed)
        {
            return;
        }

        WriteDebugDumps(_compiledOutputPath);
    }

    private static string[] GetClangExtraArgs()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return [];
        }

        RunProgram("xcrun", ["--show-sdk-path"], out var exitCode, out var stdout, out _);
        if (exitCode != 0)
        {
            throw new InvalidOperationException("xcrun --show-sdk-path failed.");
        }

        return ["-isysroot", stdout.Trim()];
    }

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
    [DynamicData(nameof(TestDataArbitrary), DynamicDataDisplayName = nameof(TestDataDisplayName))]
    public void Arbitrary(string testName, string optimizationLevel) => AssertSameAsNative(testName, optimizationLevel);

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
    [DynamicData(nameof(TestDataCTestSuite), DynamicDataDisplayName = nameof(TestDataDisplayName))]
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

        Assert.AreEqual("", NormalizeLineEndings(managedStandardError));
        Assert.AreEqual(NormalizeLineEndings(nativeStandardOutput), NormalizeLineEndings(managedStandardOutput));
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

            // These tests don't run on macOS because OpenMP isn't installed by default.
            "0002_0185" when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => false,

            // These tests don't run correctly on MSVC because of _Generic differences.
            "0005_0005" => false,

            // These tests don't compile correctly on MSVC because of fpclassify type differences.
            "0005_0006" => false,

            // These tests require GLIBC defines.
            "0005_0017" or "0005_0018" or "0005_0019" or "0005_0020" or "0005_0021" or "0005_0022" => false,
            "0005_0028" or "0005_0029" or "0005_0030" or "0005_0045" or "0005_0063" => false,

            // These tests don't compile correctly on MSVC because of align library differences.
            "0005_0032" or "0005_0033" => false,

            // These tests don't execute correctly on MSVC because of OpenMP differences.
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
        // This test's reference output was made where `char` is unsigned (`b[i] = i` for i >= 128 is 128), but it
        // is signed on macOS ARM64 and on Windows, so clang gives -128 there as well.
        .Where(x => Path.GetFileNameWithoutExtension(x) != "0022_0010")
        // These tests cover bitfields which have different alignment on MSVC / Windows.
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0014_"))
        // These tests don't compile correctly on MSVC because of _Complex type differences.
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0016_"))
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0018_"))
        // These tests don't execute correctly because of restrict differences.
        .Where(x => !Path.GetFileNameWithoutExtension(x).StartsWith("0020_"))

        // TODO: Support more tests.
        .Where(x => int.Parse(Path.GetFileNameWithoutExtension(x).Substring(0, 4)) <= 0022))
        // The 0022 tests cover `_Float16`. At -O3 clang vectorises them into vectors of half, which are not
        // supported (.NET has no SIMD type for Half); at -O0 they use scalar half.
        .Where(x => !(Path.GetFileNameWithoutExtension((string)x[0]).StartsWith("0022_") && (string)x[1] == "O3"));

    [GeneratedRegex(@"exit (\d+)")]
    private static partial Regex FujitsuCompilerTestSuiteExitCodeRegex();

    [TestMethod]
    [DynamicData(nameof(TestDataFujitsuCompilerTestSuite), DynamicDataDisplayName = nameof(TestDataDisplayName))]
    public void FujitsuCompilerTestSuite(string testName, string optimizationLevel)
    {
        CompileAndExecuteManaged(
            testName,
            optimizationLevel,
            out var managedExitCode,
            out var managedStandardOutput,
            out var managedStandardError);

        Assert.AreEqual("", NormalizeLineEndings(managedStandardError));

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
            Assert.AreEqual(NormalizeLineEndings(nativeStandardOutput), NormalizeLineEndings(managedStandardOutput));
            Assert.AreEqual(nativeExitCode, managedExitCode);
        }

        Console.WriteLine($"Stdout: {managedStandardOutput}");
    }

    private static IEnumerable<object[]> TestDataBenchmarks() => TestFiles(
        Directory
        .GetFiles(Path.Combine(TestProgramsPath, "benchmarks"), "*.c", SearchOption.AllDirectories));

    [TestMethod]
    [DynamicData(nameof(TestDataBenchmarks), DynamicDataDisplayName = nameof(TestDataDisplayName))]
    public void Benchmark(string testName, string optimizationLevel) => AssertSameAsNative(testName, optimizationLevel);

    private void AssertSameAsNative(string testName, string optimizationLevel)
    {
        CompileAndExecuteManaged(
            testName,
            optimizationLevel,
            out var managedExitCode,
            out var managedStandardOutput,
            out var managedStandardError);

        // Compile to executable binary.
        var binaryPath = GetOutputPath(testName, optimizationLevel) + "_native.exe";
        RunClangCached(GetSourceFilePath(testName), binaryPath, $"-{optimizationLevel}");

        RunProgram(
            binaryPath,
            [],
            out var llvmExitCode,
            out var llvmStandardOutput,
            out var llvmStandardError);

        Assert.AreEqual(NormalizeLineEndings(llvmStandardError), NormalizeLineEndings(managedStandardError));
        Assert.AreEqual(NormalizeLineEndings(llvmStandardOutput), NormalizeLineEndings(managedStandardOutput));
        Assert.AreEqual(llvmExitCode, managedExitCode);

        Console.WriteLine($"ExitCode: {managedExitCode}");
        Console.WriteLine($"Stdout: {managedStandardOutput}");
    }

    [TestMethod]
    public void RunProgramKillsProcessOnTimeout()
    {
        var directory = Directory.CreateTempSubdirectory("ir2il_timeout_test_").FullName;
        try
        {
            var irPath = Path.Combine(directory, "infinite_loop.ll");
            File.WriteAllText(
                irPath,
                """
                define i32 @main() {
                entry:
                  br label %loop
                loop:
                  br label %loop
                }
                """);

            var exePath = Path.Combine(directory, "infinite_loop.exe");
            Compiler.Compile(irPath, exePath);

            var stopwatch = Stopwatch.StartNew();

            Assert.Throws<TimeoutException>(() => ExecuteManaged(
                exePath,
                out _,
                out _,
                out _,
                timeout: TimeSpan.FromSeconds(2)));

            Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(30), $"Took {stopwatch.Elapsed}");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
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

    private void CompileAndExecuteManaged(
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

    private string CompileManaged(string testName, string optimizationLevel)
    {
        var fullTestName = GetOutputPath(testName, optimizationLevel);

        var irPath = fullTestName + ".ll";

        // Compile to LLVM IR.
        RunClangCached(GetSourceFilePath(testName), irPath, "-g", "-emit-llvm", "-S", $"-{optimizationLevel}");

        var outputPath = $"{fullTestName}.exe";
        Compiler.Compile(irPath, outputPath);

        _compiledOutputPath = outputPath;

        return outputPath;
    }

    private static void WriteDebugDumps(string outputPath)
    {
        var fullTestName = Path.ChangeExtension(outputPath, null);

        // Write IL to file for debugging purposes.
        {
            using var peFile = new PEFile(outputPath);
            using var ilWriter = new StringWriter();
            var disassembler = new ReflectionDisassembler(new PlainTextOutput(ilWriter), CancellationToken.None);
            disassembler.WriteModuleContents(peFile);
            File.WriteAllText($"{fullTestName}.il", ilWriter.ToString());
        }

        // ... and for an easier birds-eye-view, write decompiled C# to file.
        var decompiler = new CSharpDecompiler(
            outputPath,
            new DecompilerSettings());
        File.WriteAllText(
            $"{fullTestName}.cs",
            decompiler.DecompileWholeModuleAsString());
    }

    private static void ExecuteManaged(
        string managedExePath,
        out int managedExitCode,
        out string managedStandardOutput,
        out string managedStandardError,
        TimeSpan? timeout = null)
    {
        var tempDir = Directory.CreateTempSubdirectory("ir2il_test_").FullName;
        try
        {
            RunProgram(
                "dotnet",
                [managedExePath],
                out managedExitCode,
                out managedStandardOutput,
                out managedStandardError,
                workingDirectory: tempDir,
                timeout: timeout);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    // Generous, because tests run in parallel and CI machines are slower than a developer machine.
    // This only has to catch hangs, such as a miscompiled infinite loop.
    // A property rather than a static field: `ClangExtraArgs` calls `RunProgram` during static
    // initialisation, before any later field initialiser would have run.
    private static TimeSpan DefaultProcessTimeout =>
        TimeSpan.FromSeconds(
            int.TryParse(Environment.GetEnvironmentVariable("IR2IL_TEST_TIMEOUT_SECONDS"), out var seconds) && seconds > 0
                ? seconds
                : 120);

    /// <exception cref="TimeoutException">
    /// The process did not exit within <paramref name="timeout"/> (default 120 s, or
    /// <c>IR2IL_TEST_TIMEOUT_SECONDS</c>). The process tree is killed first.
    /// </exception>
    private static void RunProgram(
        string executablePath,
        string[] arguments,
        out int exitCode,
        out string standardOutput,
        out string standardError,
        string? workingDirectory = null,
        TimeSpan? timeout = null)
    {
        timeout ??= DefaultProcessTimeout;

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.ASCII,
            WorkingDirectory = workingDirectory ?? string.Empty,
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

        if (!process.WaitForExit(timeout.Value))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();

            // Killing the tree closes the pipes, so the reads complete; don't wait forever if they don't.
            Task.WaitAll([standardOutputTask, standardErrorTask], TimeSpan.FromSeconds(5));

            throw new TimeoutException(
                $"'{executablePath} {string.Join(' ', arguments)}' did not exit within {timeout.Value.TotalSeconds:0.#} s and was killed." +
                $"{Environment.NewLine}Stdout so far: {(standardOutputTask.IsCompletedSuccessfully ? standardOutputTask.Result : "<unavailable>")}" +
                $"{Environment.NewLine}Stderr so far: {(standardErrorTask.IsCompletedSuccessfully ? standardErrorTask.Result : "<unavailable>")}");
        }

        // The overload with a timeout doesn't guarantee that async output handling has finished.
        process.WaitForExit();

        exitCode = process.ExitCode;

        standardOutput = standardOutputTask.Result;
        standardError = standardErrorTask.Result;
    }

    private static readonly string ClangCachePath = Path.Combine(RepoRoot, "build", "cache", "clang");

    // Identifies the exact clang build. Hashing the binary (rather than trusting `--version` or a path)
    // means swapping or upgrading the toolchain automatically invalidates every cached output.
    private static readonly Lazy<string> ClangIdentity = new(() =>
    {
        using var stream = File.OpenRead(ClangPath);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream));
    });

    /// <summary>
    /// Runs clang on <paramref name="sourcePath"/> to produce <paramref name="outputPath"/>, reusing a
    /// previous result from <c>build/cache/clang</c> when nothing that influences the output has changed.
    /// The cache key covers the clang binary, the flags (including the macOS SDK), the source path
    /// (debug info embeds it), the source contents and any header files next to the source.
    /// </summary>
    private static void RunClangCached(string sourcePath, string outputPath, params string[] flags)
    {
        var fullSourcePath = Path.GetFullPath(sourcePath);

        var key = new StringBuilder();
        key.AppendLine(ClangIdentity.Value);
        key.AppendLine(fullSourcePath);
        key.AppendLine(string.Join('\0', flags));
        key.AppendLine(string.Join('\0', ClangExtraArgs));
        key.AppendLine(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(fullSourcePath))));

        var sourceDirectory = Path.GetDirectoryName(fullSourcePath)!;
        foreach (var header in Directory.EnumerateFiles(sourceDirectory)
            .Where(x => Path.GetExtension(x) is ".h" or ".hpp" or ".inc")
            .Order(StringComparer.Ordinal))
        {
            key.AppendLine(header);
            key.AppendLine(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(header))));
        }

        var keyHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(key.ToString())));
        var cachedPath = Path.Combine(ClangCachePath, keyHash + Path.GetExtension(outputPath));

        if (File.Exists(cachedPath))
        {
            File.Copy(cachedPath, outputPath, overwrite: true);
            return;
        }

        RunClang([fullSourcePath, ..flags, "-o", outputPath]);

        // Publish atomically, because tests run in parallel.
        Directory.CreateDirectory(ClangCachePath);
        var temporaryPath = Path.Combine(ClangCachePath, $"{keyHash}.{Guid.NewGuid():N}.tmp");
        File.Copy(outputPath, temporaryPath, overwrite: true);
        File.Move(temporaryPath, cachedPath, overwrite: true);
    }

    private static void RunClang(string[] arguments)
    {
        RunProgram(
            ClangPath,
            [..arguments, ..ClangExtraArgs],
            out var exitCode,
            out var standardOutput,
            out var standardError);

        if (exitCode != 0)
        {
            throw new InvalidOperationException(standardOutput + Environment.NewLine + standardError);
        }
    }

    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n");
    }
}