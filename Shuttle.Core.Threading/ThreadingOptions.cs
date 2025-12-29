using Shuttle.Extensions.Options;

namespace Shuttle.Core.Threading;

public class ThreadingOptions
{
    public TimeSpan JoinTimeout { get; set; } = TimeSpan.FromSeconds(15);

    public AsyncEvent<ProcessorThreadCreatedEventArgs> ProcessorThreadCreated { get; set; } = new();
    public AsyncEvent<ProcessorExecutingEventArgs> ProcessorExecuting { get; set; } = new();
    public AsyncEvent<ProcessorExecutedEventArgs> ProcessorExecuted { get; set; } = new();
    public AsyncEvent<ProcessorThreadExceptionEventArgs> ProcessorException { get; set; } = new();
    public AsyncEvent<ProcessorThreadEventArgs> ProcessorThreadActive {get;set;} = new();
    public AsyncEvent<ProcessorThreadEventArgs> ProcessorThreadOperationCanceled {get;set;} = new();
    public AsyncEvent<ProcessorThreadEventArgs> ProcessorThreadStarting {get;set;} = new();
    public AsyncEvent<ProcessorThreadEventArgs> ProcessorThreadStopped {get;set;} = new();
    public AsyncEvent<ProcessorThreadEventArgs> ProcessorThreadStopping {get;set;} = new();
}