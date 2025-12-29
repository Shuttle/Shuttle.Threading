namespace Shuttle.Core.Threading;

public class NullProcessorIdleStrategy : IProcessorIdleStrategy
{
    public Task SignalAsync(string serviceKey, bool workPerformed, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}