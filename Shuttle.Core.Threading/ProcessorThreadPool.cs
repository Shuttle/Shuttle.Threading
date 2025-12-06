using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Threading;

public class ProcessorThreadPool : IProcessorThreadPool
{
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

    public async Task StopAsync()
    {
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
        if (_started)
        {
            return;
        }

        var i = 0;

        while (i++ < ThreadCount)
        {
            var processorThread = new ProcessorThread($"{Name} / {i}", _serviceScopeFactory, await ProcessorFactory.CreateAsync(cancellationToken), _threadingOptions);

            await _threadingOptions.ProcessorThreadCreated.InvokeAsync(new(processorThread), cancellationToken);

            _processorThreads.Add(processorThread);

            await processorThread.StartAsync();
        }

        _started = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var thread in _processorThreads)
        {
            thread.Deactivate();
        }

        foreach (var thread in _processorThreads)
        {
            await thread.StopAsync();
        }

        await ProcessorFactory.TryDisposeAsync();

        _disposed = true;
    }
}