namespace Shuttle.Threading.Tests;

public class MockProcessor(TimeSpan executionDuration) : IProcessor
{
    public int ExecutionCount { get; private set; }

    public async ValueTask<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(executionDuration, cancellationToken).ConfigureAwait(false);
        
        ExecutionCount++;

        return true;
    }
}