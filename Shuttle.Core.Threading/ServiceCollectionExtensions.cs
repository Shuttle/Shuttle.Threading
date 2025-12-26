using Microsoft.Extensions.DependencyInjection;
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

                options.ProcessorThreadPoolCreated = threadingBuilder.Options.ProcessorThreadPoolCreated;
                options.ProcessorThreadCreated = threadingBuilder.Options.ProcessorThreadCreated;
                options.ProcessorException = threadingBuilder.Options.ProcessorException;
                options.ProcessorExecuting = threadingBuilder.Options.ProcessorExecuting;
                options.ProcessorThreadActive = threadingBuilder.Options.ProcessorThreadActive;
                options.ProcessorThreadOperationCanceled = threadingBuilder.Options.ProcessorThreadOperationCanceled;
                options.ProcessorThreadStarting = threadingBuilder.Options.ProcessorThreadStarting;
                options.ProcessorThreadStopped = threadingBuilder.Options.ProcessorThreadStopped;
                options.ProcessorThreadStopping = threadingBuilder.Options.ProcessorThreadStopping;
            });

            return services;
        }
    }
}