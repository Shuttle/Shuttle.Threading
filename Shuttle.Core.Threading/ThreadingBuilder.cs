
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ThreadingBuilder(IServiceCollection services)
{
    public ThreadingBuilder Configure(Action<ThreadingOptions> configureOptions)
    {
        Services.Configure(configureOptions);
        return this;
    }

    public ThreadingBuilder Configure(string name, Action<ProcessorIdleOptions> configureOptions)
    {
        Services.Configure(name, configureOptions);
        return this;
    }

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
}