using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLILL.Tests
{
    [TestClass]
    public class CompilerTests
    {
        private static IEnumerable<object[]> TestData()
        {
            var testFiles = Directory.GetFiles("TestPrograms", "*.c", SearchOption.AllDirectories);

            var optimizationLevels = new string[]
            {
                "O0",
                "O3",
            };

            return from testFile in testFiles
                   from optimizationLevel in optimizationLevels
                   select new object[] { testFile, optimizationLevel };
        }

        [TestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void CanCompileLlvmIrToMsil(string testName, string optimizationLevel)
        {
            var sourceFilePath = Path.Combine(Environment.CurrentDirectory, testName);

            var fullTestName = $"{testName}_{optimizationLevel}";

            var irPath = fullTestName + ".ll";
            var binaryPath = fullTestName + "_native.exe";

            // Compile to LLVM IR.
            RunClang([sourceFilePath, "-o", irPath, "-emit-llvm", "-S", $"-{optimizationLevel}"]);

            // Compile to executable binary.
            RunClang([sourceFilePath, "-o", binaryPath, $"-{optimizationLevel}"]);

            var outputPath = $"{fullTestName}.exe";

            using (var compiler = new Compiler())
            using (var source = LLVMSourceCode.FromFile(irPath))
            {
                compiler.Compile(source, outputPath);
            }

            RunProgram(
                "dotnet",
                [outputPath],
                out var managedExitCode,
                out var managedStandardOutput,
                out var managedStandardError);

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
                @"..\..\..\..\..\lib\llvm\win-x64\clang.exe",
                arguments,
                out var exitCode,
                out _,
                out var standardError);

            if (exitCode != 0)
            {
                throw new InvalidOperationException(standardError);
            }
        }
    }
}
