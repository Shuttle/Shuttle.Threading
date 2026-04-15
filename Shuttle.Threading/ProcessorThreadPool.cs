using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Shuttle.Threading;

public class ProcessorThreadPool(string serviceKey, int threadCount, IServiceScopeFactory serviceScopeFactory, ThreadingOptions threadingOptions, IProcessorIdleStrategy processorIdleStrategy, ILoggerFactory? loggerFactory = null)
    : IProcessorThreadPool
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly List<ProcessorThread> _processorThreads = [];
    private bool _disposed;
    private bool _started;

    public string ServiceKey { get; } = serviceKey;
    public int ThreadCount { get; } = threadCount > 0 ? threadCount : throw new ThreadCountZeroException();

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            await StopThreadsAsync();

            _started = false;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task StopThreadsAsync()
    {
        if (!_started)
        {
            return;
        }

        foreach (var thread in _processorThreads)
        {
            await thread.StopAsync();
        }
    }

    public IEnumerable<ProcessorThread> ProcessorThreads => _processorThreads.AsReadOnly();

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_started)
            {
                return;
            }

            var i = 0;

            while (i++ < ThreadCount)
            {
                var processorThread = new ProcessorThread(ServiceKey, serviceScopeFactory, threadingOptions, processorIdleStrategy, _loggerFactory.CreateLogger<ProcessorThread>());

                await threadingOptions.ProcessorThreadCreated.InvokeAsync(new(this, processorThread), cancellationToken);

                _processorThreads.Add(processorThread);

                await processorThread.StartAsync(cancellationToken);
            }

            _started = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _lock.WaitAsync(CancellationToken.None);

        try
        {
            if (_disposed)
            {
                return;
            }

            await StopThreadsAsync();

            _disposed = true;
        }
        finally
        {
            _lock.Release();
        }
    }
}