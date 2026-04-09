using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Threading;

public class DefaultProcessorIdleStrategy(IOptionsMonitor<ProcessorIdleOptions> processorIdleOptions) : IProcessorIdleStrategy
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IOptionsMonitor<ProcessorIdleOptions> _processorIdleOptions = Guard.AgainstNull(processorIdleOptions);
    private readonly Dictionary<string, ThreadActivity> _threadActivities = new();

    public async Task SignalAsync(string serviceKey, bool workPerformed, CancellationToken cancellationToken = default)
    {
        ThreadActivity threadActivity;

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (!_threadActivities.ContainsKey(serviceKey))
            {
                var options = _processorIdleOptions.Get(Guard.AgainstEmpty(serviceKey));

                if (options == null || options.Durations.Count == 0)
                {
                    throw new ApplicationException(string.Format(Resources.ProcessorIdleOptionsMissingException, serviceKey));
                }

                _threadActivities.Add(serviceKey, new(options.Durations));
            }

            threadActivity = _threadActivities[serviceKey];
        }
        finally
        {
            _lock.Release();
        }

        await threadActivity.SignalAsync(workPerformed, cancellationToken);
    }
}