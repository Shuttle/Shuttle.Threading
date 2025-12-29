using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddThreading(Action<ThreadingBuilder>? builder = null)
        {
            Guard.AgainstNull(services);

            services.AddOptions<ThreadingOptions>();
            services.AddOptions<ProcessorIdleOptions>();

            var threadingBuilder = new ThreadingBuilder(services);

            builder?.Invoke(threadingBuilder);

            services
                .AddSingleton<IValidateOptions<ProcessorIdleOptions>, ProcessorIdleOptionsValidator>()
                .AddSingleton<IProcessorIdleStrategy, DefaultProcessorIdleStrategy>();

            return services;
        }
    }
}