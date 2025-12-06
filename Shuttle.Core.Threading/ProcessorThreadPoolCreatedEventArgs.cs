using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class ProcessorThreadPoolCreatedEventArgs(IProcessorThreadPool processorThreadPool) : EventArgs
{
    public IProcessorThreadPool ProcessorThreadPool { get; } = Guard.AgainstNull(processorThreadPool);
}