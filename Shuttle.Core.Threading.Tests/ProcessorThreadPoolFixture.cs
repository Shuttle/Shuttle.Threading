using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Shuttle.Core.Threading.Tests;

public class ProcessorThreadPoolFixture
{
    [Test]
    public async Task Should_be_able_to_execute_processor_thread_pool_async()
    {
        const int minimumExecutionCount = 5;
        var executionDuration = TimeSpan.FromMilliseconds(200);
        var semaphore = new SemaphoreSlim(1, 1);

        var serviceProvider = new ServiceCollection()
            .AddKeyedTransient<IProcessor>("thread-pool", (_, _) => new MockProcessor(executionDuration))
            .BuildServiceProvider();

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var executionCounts = new Dictionary<int, int>();

        var threadingOptions = new ThreadingOptions();

        threadingOptions.ProcessorException += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorException] : name = '{args.ProcessorThread.ServiceKey}' / managed thread id =  {args.ManagedThreadId} / exception = '{args.Exception}'");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorExecuting += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorExecuting] : name = '{args.ServiceKey}' / execution count = {((MockProcessor)args.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorExecuted += async (args, _) =>
        {
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                if (!executionCounts.TryAdd(args.ManagedThreadId, 1))
                {
                    executionCounts[args.ManagedThreadId] += 1;
                }
            }
            finally
            {
                semaphore.Release();
            }

            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorExecuted] : name = '{args.ServiceKey}' / execution count = {executionCounts[args.ManagedThreadId]} / managed thread id = {args.ManagedThreadId}");
        };

        threadingOptions.ProcessorThreadActive += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadActive] : name = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadStarting += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadStarting] : name = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadStopped += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadStopped] : name = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadStopping += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadStopping] : name = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadOperationCanceled += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadOperationCanceled] : name = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        var processorThreadPool = new ProcessorThreadPool("thread-pool", 5, serviceProvider.GetRequiredService<IServiceScopeFactory>(), threadingOptions, new NullProcessorIdleStrategy());

        await processorThreadPool.StartAsync(cancellationToken);

        var timeout = DateTime.Now.AddSeconds(5);
        var timedOut = false;

        while (executionCounts.Count < 5 || executionCounts.Any(pair => pair.Value <= minimumExecutionCount) && !timedOut)
        {
            await Task.Delay(25, cancellationToken).ConfigureAwait(false);

            timedOut = DateTime.Now >= timeout;
        }

        await cancellationTokenSource.CancelAsync();

        await processorThreadPool.StopAsync(CancellationToken.None);

        Assert.That(timedOut, Is.False, $"[TIMEOUT] : Did not complete {minimumExecutionCount} executions before {timeout:O}");
    }
}