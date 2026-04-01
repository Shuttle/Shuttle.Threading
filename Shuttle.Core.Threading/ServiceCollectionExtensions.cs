using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Shuttle.Core.Threading;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public ThreadingBuilder AddThreading(Action<ThreadingOptions>? configureOptions = null)
        {
            services.AddOptions();
            services.AddOptions<ThreadingOptions>().Configure(options =>
            {
                configureOptions?.Invoke(options);
            });

            services
                .AddSingleton<IValidateOptions<ProcessorIdleOptions>, ProcessorIdleOptionsValidator>()
                .AddSingleton<IProcessorIdleStrategy, DefaultProcessorIdleStrategy>()
                .AddScoped<ProcessorContextAccessor>()
                .AddScoped<IProcessorContext>(sp => sp.GetRequiredService<ProcessorContextAccessor>().Context ?? throw new InvalidOperationException(Resources.ProcessorContextException)); 

            return new(services);
        }
    }
}