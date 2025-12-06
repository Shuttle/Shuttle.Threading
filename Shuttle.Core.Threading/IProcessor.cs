namespace Shuttle.Core.Threading;

public interface IProcessor
{
    Task ExecuteAsync(IProcessorThreadContext processorThread, CancellationToken cancellationToken = default);
}