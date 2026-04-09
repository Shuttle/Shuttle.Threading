namespace Shuttle.Threading;

public interface IProcessor
{
    ValueTask<bool> ExecuteAsync(CancellationToken cancellationToken = default);
}