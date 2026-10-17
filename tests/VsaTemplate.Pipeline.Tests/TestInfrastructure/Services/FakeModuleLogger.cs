using Microsoft.Extensions.Logging.Testing;
using ModularPipelines.Logging;

namespace VsaTemplate.Pipeline.Tests.TestInfrastructure.Services;

public sealed class FakeModuleLogger : FakeLogger, IModuleLogger
{
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}
