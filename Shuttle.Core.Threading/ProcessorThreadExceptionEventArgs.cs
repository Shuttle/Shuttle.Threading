using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ProcessorThreadExceptionEventArgs(ProcessorThread processorThread, int managedThreadId, Exception? exception = null)
{
    public int ManagedThreadId { get; } = managedThreadId;
    public Exception? Exception { get; } = exception;
    public ProcessorThread ProcessorThread { get; } = Guard.AgainstNull(processorThread);
}