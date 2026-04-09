using Shuttle.Contract;

namespace Shuttle.Threading;

public class ProcessorThreadPoolCreatedEventArgs(IProcessorThreadPool processorThreadPool) : EventArgs
{
    public IProcessorThreadPool ProcessorThreadPool { get; } = Guard.AgainstNull(processorThreadPool);
}