namespace Shuttle.Core.Threading;

public interface IProcessorContext
{
    string ServiceKey { get; }
    int ManagedThreadId { get; }
}