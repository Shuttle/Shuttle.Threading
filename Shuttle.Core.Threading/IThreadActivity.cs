namespace Shuttle.Core.Threading;

public interface IThreadActivity
{
    Task WaitingAsync(CancellationToken cancellationToken = default);
    void Working();
}