using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ProcessorThreadEventArgs(IProcessorThreadPool processorThreadPool, ProcessorThread processorThread, int managedThreadId)
{
    public IProcessorThreadPool ProcessorThreadPool { get; } = Guard.AgainstNull(processorThreadPool);
    public ProcessorThread ProcessorThread { get; } = Guard.AgainstNull(processorThread);
    public int ManagedThreadId { get; } = managedThreadId;
}