using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ProcessorThreadCreatedEventArgs(ProcessorThread processorThread) : EventArgs
{
    public ProcessorThread ProcessorThread { get; } = Guard.AgainstNull(processorThread);
}