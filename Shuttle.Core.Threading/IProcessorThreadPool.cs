namespace Shuttle.Core.Threading;

public interface IProcessorThreadPool : IDisposable, IAsyncDisposable
{
    string ServiceKey { get; }
    IEnumerable<ProcessorThread> ProcessorThreads { get; }
    int ThreadCount { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}