namespace Shuttle.Threading;

public class ProcessorExecutedEventArgs(string serviceKey, int managedThreadId, IProcessor processor, bool workPerformed) 
    : ProcessorExecutingEventArgs(serviceKey, managedThreadId, processor)
{
    public bool WorkPerformed { get; } = workPerformed;
}