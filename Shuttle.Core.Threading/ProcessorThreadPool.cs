using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Threading;

public class ProcessorThreadPool : IProcessorThreadPool
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly List<ProcessorThread> _processorThreads = [];
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private bool _disposed;
    private bool _started;
    private readonly ThreadingOptions _threadingOptions;

    public ProcessorThreadPool(string name, int threadCount, IServiceScopeFactory serviceScopeFactory, IProcessorFactory processorFactory, ThreadingOptions threadingOptions)
    {
        if (threadCount < 1)
        {
            throw new ThreadCountZeroException();
        }
        
        Name = name;
        _serviceScopeFactory = Guard.AgainstNull(serviceScopeFactory);
        ProcessorFactory = Guard.AgainstNull(processorFactory);
        _threadingOptions = Guard.AgainstNull(threadingOptions);
        ThreadCount = threadCount;
    }

    public string Name { get; }
    public IProcessorFactory ProcessorFactory { get; }
    public int ThreadCount { get; }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (!_started)
            {
                return;
            }

            foreach (var thread in _processorThreads)
            {
                await thread.StopAsync();
            }

            _started = false;
        }
        finally
        {
            _lock.Release();
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
                var processorThread = new ProcessorThread($"{Name} / {i}", this, await ProcessorFactory.CreateAsync(cancellationToken), _serviceScopeFactory, _threadingOptions);

                await _threadingOptions.ProcessorThreadCreated.InvokeAsync(new(this, processorThread), cancellationToken);

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

            await StopAsync(CancellationToken.None);

            await ProcessorFactory.TryDisposeAsync();

            _disposed = true;
        }
        finally
        {
            _lock.Release();
        }
    }
}