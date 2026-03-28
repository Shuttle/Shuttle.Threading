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

            var threadingBuilder = new ThreadingBuilder(services);

            builder?.Invoke(threadingBuilder);

            services
                .AddSingleton<IValidateOptions<ProcessorIdleOptions>, ProcessorIdleOptionsValidator>()
                .AddSingleton<IProcessorIdleStrategy, DefaultProcessorIdleStrategy>()
                .AddScoped<ProcessorContextAccessor>()
                .AddScoped<IProcessorContext>(sp => sp.GetRequiredService<ProcessorContextAccessor>().Context ?? throw new InvalidOperationException(Resources.ProcessorContextException)); 

            return services;
        }
    }
}