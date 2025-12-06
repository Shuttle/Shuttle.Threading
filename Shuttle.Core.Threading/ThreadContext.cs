using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ProcessorThreadContext(IState state, IServiceScope serviceScope) : IDisposable, IProcessorThreadContext
{
    public IState State { get; } = Guard.AgainstNull(state);
    public IServiceScope ServiceScope { get; } = Guard.AgainstNull(serviceScope);

    public void Dispose()
    {
        ServiceScope.Dispose();
    }
}