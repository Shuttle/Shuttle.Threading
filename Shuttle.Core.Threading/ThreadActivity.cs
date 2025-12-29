using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ThreadActivity(IEnumerable<TimeSpan> durations) : IThreadActivity
{
    private readonly TimeSpan[] _durations = Guard.AgainstEmpty(durations).ToArray();

    private int _durationIndex;

    public async Task SignalAsync(bool workPerformed, CancellationToken cancellationToken)
    {
        if (workPerformed)
        {
            _durationIndex = 0;
        }
        else
        {
            await Task.Delay(GetSleepTimeSpan(), cancellationToken).ConfigureAwait(false);
        }
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