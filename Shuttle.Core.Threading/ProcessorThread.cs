using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Threading;

public class ProcessorThread(string name, IProcessor processor, IServiceScopeFactory serviceScopeFactory, ThreadingOptions threadingOptions)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = Guard.AgainstNull(serviceScopeFactory);
    private readonly ThreadingOptions _threadingOptions = Guard.AgainstNull(threadingOptions);
    private CancellationToken _cancellationToken;
    private bool _started;
    private Task? _executionTask;

    public string Name { get; } = Guard.AgainstNull(name);
    public IProcessor Processor { get; } = Guard.AgainstNull(processor);
    public int ManagedThreadId { get; private set; }

    public IState State { get; } = new State();

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        State.Add("Name", Name);

        if (_started)
        {
            return;
        }

        _cancellationToken = cancellationToken;
        _executionTask = Task.Factory.StartNew(WorkAsync, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

        if (!cancellationToken.IsCancellationRequested)
        {
            await _threadingOptions.ProcessorThreadActive.InvokeAsync(new(this, Environment.CurrentManagedThreadId), cancellationToken);
        }

        _started = true;
    }

    public async Task StopAsync()
    {
        if (!_started)
        {
            throw new InvalidOperationException(Resources.ProcessorThreadNotStartedException);
        }

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

        await _threadingOptions.ProcessorThreadStopped.InvokeAsync(new(this, Environment.CurrentManagedThreadId), CancellationToken.None);
    }

    private async Task WorkAsync()
    {
        ManagedThreadId = Environment.CurrentManagedThreadId;
        State.Add("ManagedThreadId", ManagedThreadId);

        var eventArgs = new ProcessorThreadEventArgs(this, ManagedThreadId);

        await _threadingOptions.ProcessorThreadStarting.InvokeAsync(eventArgs, _cancellationToken);

        while (!_cancellationToken.IsCancellationRequested)
        {
            await _threadingOptions.ProcessorExecuting.InvokeAsync(eventArgs, _cancellationToken);

            try
            {
                using var context = new ProcessorThreadContext(State, _serviceScopeFactory.CreateScope());

                await Processor.ExecuteAsync(context, _cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await _threadingOptions.ProcessorThreadOperationCanceled.InvokeAsync(eventArgs, _cancellationToken);
                break;
            }
            catch (Exception ex)
            {
                await _threadingOptions.ProcessorException.InvokeAsync(new(this, ManagedThreadId, ex), _cancellationToken);
            }
        }

        await _threadingOptions.ProcessorThreadStopping.InvokeAsync(eventArgs, _cancellationToken);
    }
}