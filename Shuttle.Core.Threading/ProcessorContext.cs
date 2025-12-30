using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ProcessorContext(string serviceKey, int managedThreadId) : IProcessorContext
{
    public string ServiceKey { get;  } = Guard.AgainstEmpty(serviceKey);
    public int ManagedThreadId { get; } = managedThreadId;
}