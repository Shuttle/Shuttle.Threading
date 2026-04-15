namespace Shuttle.Threading;

public interface IThreadActivity
{
    Task SignalAsync(bool workPerformed, CancellationToken cancellationToken = default);
}