namespace Shuttle.Core.Threading;

public interface IProcessorThreadPool : IDisposable, IAsyncDisposable
{
    string Name { get; }
    IProcessorFactory ProcessorFactory { get; }
    IEnumerable<ProcessorThread> ProcessorThreads { get; }
    int ThreadCount { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
}