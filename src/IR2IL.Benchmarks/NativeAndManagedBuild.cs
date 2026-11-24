using System;
using System.Diagnostics;
using System.IO;

namespace IR2IL.Benchmarks;

/// <summary>
/// Builds a C benchmark twice: as a shared library with clang (native), and as an IR2IL assembly
/// (clang to LLVM IR, then IR2IL). Output goes to <c>build/benchmarks</c> in the repository.
/// </summary>
internal static class NativeAndManagedBuild
{
    private static readonly string RepoRoot = FindRepoRoot();

    private static readonly string ClangPath =
        Path.Combine(RepoRoot, "build", "llvm", "bin", OperatingSystem.IsWindows() ? "clang.exe" : "clang");

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "IR2IL.sln")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find the repository root above " + AppContext.BaseDirectory);
    }

    public static string SharedLibraryExtension =>
        OperatingSystem.IsWindows() ? ".dll" : OperatingSystem.IsMacOS() ? ".dylib" : ".so";

    /// <summary>Compiles <c>tests/benchmarks/{name}.c</c> and returns the paths of both outputs.</summary>
    public static (string ManagedAssemblyPath, string NativeLibraryPath) Build(string name, string optimizationLevel)
    {
        var sourcePath = Path.Combine(RepoRoot, "tests", "benchmarks", name + ".c");
        var outputDirectory = Path.Combine(RepoRoot, "build", "benchmarks", $"{name}_{optimizationLevel}");
        Directory.CreateDirectory(outputDirectory);

        var irPath = Path.Combine(outputDirectory, name + ".ll");
        RunClang(sourcePath, "-g", "-emit-llvm", "-S", $"-{optimizationLevel}", "-o", irPath);

        var managedPath = Path.Combine(outputDirectory, name + ".exe");
        Compiler.Compile(irPath, managedPath);

        var nativePath = Path.Combine(outputDirectory, name + SharedLibraryExtension);
        RunClang(sourcePath, OperatingSystem.IsMacOS() ? "-dynamiclib" : "-shared", $"-{optimizationLevel}", "-o", nativePath);

        return (managedPath, nativePath);
    }

    private static void RunClang(string sourcePath, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo(ClangPath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add(sourcePath);
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (OperatingSystem.IsMacOS())
        {
            startInfo.ArgumentList.Add("-isysroot");
            startInfo.ArgumentList.Add(Run("xcrun", "--show-sdk-path").Trim());
        }

        Run(startInfo);
    }

    private static string Run(string fileName, string argument)
    {
        var startInfo = new ProcessStartInfo(fileName, argument)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        return Run(startInfo);
    }

    private static string Run(ProcessStartInfo startInfo)
    {
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start " + startInfo.FileName);
        var error = process.StandardError.ReadToEndAsync();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{startInfo.FileName} failed:\n{output}\n{error.Result}");
        }

        return output;
    }
}
