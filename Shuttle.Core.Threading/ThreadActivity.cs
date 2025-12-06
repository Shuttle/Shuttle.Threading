using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ThreadActivity : IThreadActivity
{
    private readonly TimeSpan[] _durations;

    private int _durationIndex;

    public ThreadActivity(IEnumerable<TimeSpan> waitDurations)
    {
        Guard.AgainstEmpty(waitDurations);

        _durations = waitDurations.ToArray();
        _durationIndex = 0;
    }

    public async Task WaitingAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(GetSleepTimeSpan(), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Working()
    {
        _durationIndex = 0;
    }

    private TimeSpan GetSleepTimeSpan()
    {
        if (_durationIndex >= _durations.Length)
        {
            _durationIndex = _durations.Length - 1;
        }

        return _durations[_durationIndex++];
    }
}