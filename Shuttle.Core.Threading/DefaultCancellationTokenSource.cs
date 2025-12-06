namespace Shuttle.Core.Threading;

public class DefaultCancellationTokenSource : ICancellationTokenSource, IDisposable
{
    private CancellationTokenSource _cancellationTokenSource = new();

    public CancellationTokenSource Get()
    {
        return _cancellationTokenSource;
    }

    public void Renew()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new();
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
    }
}