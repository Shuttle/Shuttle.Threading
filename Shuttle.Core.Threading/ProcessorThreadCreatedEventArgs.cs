using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ProcessorThreadCreatedEventArgs(ProcessorThreadPool processorThreadPool, ProcessorThread processorThread) : EventArgs
{
    public ProcessorThreadPool ProcessorThreadPool { get; } = Guard.AgainstNull(processorThreadPool);
    public ProcessorThread ProcessorThread { get; } = Guard.AgainstNull(processorThread);
}