namespace Shuttle.Core.Threading.Tests;

public class MockProcessor(TimeSpan executionDuration) : IProcessor
{
    public int ExecutionCount { get; private set; }

    public async Task ExecuteAsync(IProcessorThreadContext context, CancellationToken cancellationToken)
    {
        await Task.Delay(executionDuration, cancellationToken).ConfigureAwait(false);
        ExecutionCount++;
    }
}