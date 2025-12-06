namespace Shuttle.Core.Threading;

public interface IProcessorFactory
{
    Task<IProcessor> CreateAsync(CancellationToken cancellationToken = default);
}