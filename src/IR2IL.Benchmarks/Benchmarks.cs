using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace IR2IL.Benchmarks;

/// <summary>
/// Calls the <c>main</c> of each program in <c>tests/benchmarks</c> through a function pointer, native
/// (clang) against managed (IR2IL), at the optimisation level in <see cref="OptimizationLevel"/>. Each
/// <c>main</c> prints its result, so the cost of printing is part of the measurement on both sides.
/// To add a benchmark, add its file name (without <c>.c</c>) to <see cref="Names"/>.
/// The single job keeps the default tiered compilation: BenchmarkDotNet's warm-up gets the code to Tier-1
/// with PGO data, which is the steady state a .NET application reaches.
/// </summary>
[Config(typeof(Config))]
public unsafe class Benchmarks
{
    private sealed class Config : ManualConfig
    {
        public Config()
        {
            // BenchmarkDotNet does not know net11.0 yet: the toolchain builds the net11.0 project, and the
            // runtime moniker only has to be one BenchmarkDotNet recognises (it is just a label here).
            var runtime = CoreRuntime.Core10_0;
            var toolchain = new Net11Toolchain();
            AddJob(Job.Default.WithRuntime(runtime).WithToolchain(toolchain));
        }
    }

    public static IEnumerable<string> Names { get; } =
    [
        "benchmark_arcfour",
        "benchmark_fibonacci",
        "benchmark_fireflies_flocking",
        "benchmark_mandelbrot",
        "benchmark_nbody",
        "benchmark_particle_kinematics",
        "benchmark_pixar_raytracer",
        "benchmark_polynomials",
        "benchmark_radix",
        "benchmark_seahash",
        "benchmark_sieve_of_eratosthenes",
    ];

    [ParamsSource(nameof(Names))]
    public string Name = "";

    [Params("O3")]
    public string OptimizationLevel = "";

    private delegate*<int> _managed;
    private delegate* unmanaged[Cdecl, SuppressGCTransition]<int> _native;
    private nint _nativeLibrary;

    [GlobalSetup]
    public void Setup()
    {
        var (managedPath, nativePath) = NativeAndManagedBuild.Build(Name, OptimizationLevel);

        var program = Assembly.LoadFrom(managedPath).GetType("Program", throwOnError: true)!;
        RuntimeHelpers.RunClassConstructor(program.TypeHandle);
        _managed = (delegate*<int>)program
            .GetMethod("main", BindingFlags.Static | BindingFlags.Public)!
            .MethodHandle.GetFunctionPointer();

        _nativeLibrary = NativeLibrary.Load(nativePath);
        _native = (delegate* unmanaged[Cdecl, SuppressGCTransition]<int>)NativeLibrary.GetExport(_nativeLibrary, "main");
    }

    [GlobalCleanup]
    public void Cleanup() => NativeLibrary.Free(_nativeLibrary);

    [Benchmark(Baseline = true)]
    public int Native() => _native();

    [Benchmark]
    public int Managed() => _managed();
}
