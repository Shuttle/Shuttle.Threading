using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.Threading;

public class ThreadingBuilder(IServiceCollection services)
{
    public ThreadingBuilder ConfigureProcessor(string name, Action<ProcessorIdleOptions> configureOptions)
    {
        Services.Configure(name, configureOptions);
        return this;
    }

    public ThreadingBuilder ConfigureProcessor(string name, Action<ProcessorIdleOptions, IServiceProvider> configureOptions)
    {
        Services.AddOptions<ProcessorIdleOptions>(name).Configure<IServiceProvider>((options, serviceProvider) => configureOptions(options, serviceProvider));
        return this;
    }

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
}