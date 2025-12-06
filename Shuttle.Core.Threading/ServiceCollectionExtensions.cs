using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

            services.AddOptions<ThreadingOptions>().Configure(options =>
            {
                options.IsBackground = threadingBuilder.Options.IsBackground;
                options.JoinTimeout = threadingBuilder.Options.JoinTimeout;
                options.Priority = threadingBuilder.Options.Priority;

                options.PipelineCompleted = threadingBuilder.Options.PipelineCompleted;
                options.PipelineCreated = threadingBuilder.Options.PipelineCreated;
                options.PipelineObtained = threadingBuilder.Options.PipelineObtained;
                options.PipelineRecursiveException = threadingBuilder.Options.PipelineRecursiveException;
                options.PipelineReleased = threadingBuilder.Options.PipelineReleased;
                options.PipelineStarting = threadingBuilder.Options.PipelineStarting;
                options.StageCompleted = threadingBuilder.Options.StageCompleted;
                options.StageStarting = threadingBuilder.Options.StageStarting;
            });

            services.TryAddSingleton<IPipelineFactory, PipelineFactory>();

            return services;
        }
    }
}