using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Threading;

public class ProcessorThread
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ThreadingOptions _threadingOptions;
    private readonly ProcessorThreadEventArgs _eventArgs;
    private bool _started;
    private Task? _executionTask;

    public ProcessorThread(string name, IServiceScopeFactory serviceScopeFactory, IProcessor processor, ThreadingOptions threadingOptions)
    {
        _serviceScopeFactory = Guard.AgainstNull(serviceScopeFactory);
        Name = Guard.AgainstNull(name);
        Processor = Guard.AgainstNull(processor);
        _threadingOptions = Guard.AgainstNull(threadingOptions);
        CancellationToken = _cancellationTokenSource.Token;

        _eventArgs = new(this, Environment.CurrentManagedThreadId);

        State.Add("Name", Name);
    }

    public CancellationToken CancellationToken { get; }
    public string Name { get; }
    public IProcessor Processor { get; }
    public int ManagedThreadId { get; private set; }

    internal void Deactivate()
    {
        _cancellationTokenSource.Cancel();
    }

    public IState State { get; } = new State();

    public async Task StartAsync()
    {
        if (_started)
        {
            return;
        }

        _executionTask = Task.Factory.StartNew(WorkAsync, CancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

        if (!CancellationToken.IsCancellationRequested)
        {
            await _threadingOptions.ProcessorThreadActive.InvokeAsync(_eventArgs, CancellationToken);
        }

        _started = true;
    }

    public async Task StopAsync()
    {
        if (!_started)
        {
            throw new InvalidOperationException(Resources.ProcessorThreadNotStartedException);
        }

        await _cancellationTokenSource.CancelAsync();
        await Processor.TryDisposeAsync();

        if (_executionTask != null)
        {
            var joinTimeout = _threadingOptions.JoinTimeout;

            if (joinTimeout.TotalSeconds < 1)
            {
                joinTimeout = TimeSpan.FromSeconds(1);
            }

            try
            {
                await _executionTask.WaitAsync(joinTimeout, CancellationToken.None);
            }
            catch (TimeoutException)
            {
                throw new ApplicationException(Resources.ProcessorThreadJoinTimeoutException);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation occurs
            }
        }

        await _threadingOptions.ProcessorThreadStopped.InvokeAsync(_eventArgs, CancellationToken);
    }

    private async Task WorkAsync()
    {
        ManagedThreadId = Environment.CurrentManagedThreadId;
        State.Add("ManagedThreadId", ManagedThreadId);

        var eventArgs = new ProcessorThreadEventArgs(this, ManagedThreadId);

        await _threadingOptions.ProcessorThreadStarting.InvokeAsync(eventArgs, CancellationToken);

        while (!CancellationToken.IsCancellationRequested)
        {
            await _threadingOptions.ProcessorExecuting.InvokeAsync(eventArgs, CancellationToken);

            try
            {
                using var context = new ProcessorThreadContext(State, _serviceScopeFactory.CreateScope());

                await Processor.ExecuteAsync(context, CancellationToken);
            }
            catch (OperationCanceledException)
            {
                await _threadingOptions.ProcessorThreadOperationCanceled.InvokeAsync(eventArgs, CancellationToken);
                break;
            }
            catch (Exception ex)
            {
                await _threadingOptions.ProcessorException.InvokeAsync(new(this, ManagedThreadId, ex), CancellationToken);
            }
        }

        await _threadingOptions.ProcessorThreadStopping.InvokeAsync(eventArgs, CancellationToken);
    }
}