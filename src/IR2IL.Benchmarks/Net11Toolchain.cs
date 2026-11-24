using System.Collections.Generic;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;

namespace IR2IL.Benchmarks;

/// <summary>
/// The usual csproj toolchain for net11.0. BenchmarkDotNet 0.15.8 builds and runs net11.0 projects fine
/// when told the target framework, but its SDK validator throws on a runtime it does not know, so that
/// validation is skipped. Remove this when BenchmarkDotNet recognises net11.0.
/// </summary>
internal sealed class Net11Toolchain : IToolchain
{
    private readonly IToolchain _inner = CsProjCoreToolchain.From(new NetCoreAppSettings("net11.0", null, ".NET 11.0"));

    public string Name => _inner.Name;

    public IGenerator Generator => _inner.Generator;

    public IBuilder Builder => _inner.Builder;

    public IExecutor Executor => _inner.Executor;

    public bool IsInProcess => _inner.IsInProcess;

    public IEnumerable<ValidationError> Validate(BenchmarkCase benchmarkCase, IResolver resolver) => [];
}
