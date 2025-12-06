using System;

namespace Shuttle.Core.Threading;

public interface IProcessorThreadPoolFactory
{
    Task<IProcessorThreadPool> CreateAsync(string name, int threadCount, IProcessorFactory processorFactory, CancellationToken cancellationToken = default);
}