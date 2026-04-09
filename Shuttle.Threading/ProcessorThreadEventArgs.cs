using Shuttle.Contract;

namespace Shuttle.Threading;

public class ProcessorThreadEventArgs(ProcessorThread processorThread, int managedThreadId)
{
    public ProcessorThread ProcessorThread { get; } = Guard.AgainstNull(processorThread);
    public int ManagedThreadId { get; } = managedThreadId;
}