using Shuttle.Contract;

namespace Shuttle.Threading;

public class ProcessorExecutingEventArgs(string serviceKey, int managedThreadId, IProcessor processor)
{
    public int ManagedThreadId { get; } = managedThreadId;
    public string ServiceKey { get; } = Guard.AgainstEmpty(serviceKey);
    public IProcessor Processor { get; } = Guard.AgainstNull(processor);
}