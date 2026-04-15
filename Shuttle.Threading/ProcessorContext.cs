using Shuttle.Contract;

namespace Shuttle.Threading;

public class ProcessorContext(string serviceKey, int managedThreadId) : IProcessorContext
{
    public string ServiceKey { get;  } = Guard.AgainstEmpty(serviceKey);
    public int ManagedThreadId { get; } = managedThreadId;
}