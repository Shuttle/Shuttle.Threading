using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ProcessorThreadPoolFactory(IOptions<ThreadingOptions> threadingOptions, IServiceScopeFactory serviceScopeFactory) : IProcessorThreadPoolFactory
{
    private readonly ThreadingOptions _threadingOptions = Guard.AgainstNull(Guard.AgainstNull(threadingOptions).Value);
    private readonly IServiceScopeFactory _serviceScopeFactory = Guard.AgainstNull(serviceScopeFactory);

    public async Task<IProcessorThreadPool> CreateAsync(string name, int threadCount, IProcessorFactory processorFactory, CancellationToken cancellationToken = default)
    {
        var result = new ProcessorThreadPool(name, threadCount, _serviceScopeFactory, processorFactory, _threadingOptions);

        await _threadingOptions.ProcessorThreadPoolCreated.InvokeAsync(new(result), cancellationToken);

        return result;
    }
}