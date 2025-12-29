
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ThreadingBuilder(IServiceCollection services)
{
    public ThreadingBuilder ConfigureThreading(Action<ThreadingOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    public ThreadingBuilder ConfigureProcessorIdle(string name, Action<ProcessorIdleOptions> configure)
    {
        Services.Configure(name, configure);
        return this;
    }

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
}