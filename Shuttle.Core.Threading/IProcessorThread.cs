namespace Shuttle.Core.Threading;

public interface IProcessorThread
{
    int ManagedThreadId { get; }
    string ServiceKey { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
}