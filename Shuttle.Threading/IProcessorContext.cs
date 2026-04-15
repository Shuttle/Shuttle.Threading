namespace Shuttle.Threading;

public interface IProcessorContext
{
    string ServiceKey { get; }
    int ManagedThreadId { get; }
}