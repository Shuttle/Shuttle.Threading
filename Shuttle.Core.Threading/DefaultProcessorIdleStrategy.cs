using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class DefaultProcessorIdleStrategy(IOptionsMonitor<ThreadingOptions> threadingOptions) : IProcessorIdleStrategy
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IOptionsMonitor<ThreadingOptions> _processorIdleOptions = Guard.AgainstNull(threadingOptions);
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

                if (options == null || options.ProcessorIdleDurations.Count == 0)
                {
                    throw new ApplicationException(string.Format(Resources.ProcessorIdleOptionsMissingException, serviceKey));
                }

                _threadActivities.Add(serviceKey, new(options.ProcessorIdleDurations));
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