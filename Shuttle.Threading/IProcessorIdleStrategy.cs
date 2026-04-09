namespace Shuttle.Threading;

public interface IProcessorIdleStrategy
{
    Task SignalAsync(string serviceKey, bool workPerformed, CancellationToken cancellationToken = default);
}