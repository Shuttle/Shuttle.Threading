using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Shuttle.Core.Threading.Tests;

public class ProcessorThreadPoolFixture
{
    [Test]
    public async Task Should_be_able_to_execute_processor_thread_pool_async()
    {
        const int minimumExecutionCount = 5;

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();

        serviceScopeFactory.Setup(m => m.CreateScope()).Returns(new Mock<IServiceScope>().Object);

        var executionDuration = TimeSpan.FromMilliseconds(500);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var processorFactory = new Mock<IProcessorFactory>();

        processorFactory.Setup(m => m.CreateAsync(cancellationToken)).Returns(() => Task.FromResult<IProcessor>(new MockProcessor(executionDuration)));

        var threadingOptions = new ThreadingOptions();

        threadingOptions.ProcessorException += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorException] : name = '{args.ProcessorThread.Name}' / execution count = {((MockProcessor)args.ProcessorThread.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId} / exception = '{args.Exception}'");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorExecuting += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorExecuting] : name = '{args.ProcessorThread.Name}' / execution count = {((MockProcessor)args.ProcessorThread.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadActive += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadActive] : name = '{args.ProcessorThread.Name}' / execution count = {((MockProcessor)args.ProcessorThread.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadStarting += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadStarting] : name = '{args.ProcessorThread.Name}' / execution count = {((MockProcessor)args.ProcessorThread.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadStopped += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadStopped] : name = '{args.ProcessorThread.Name}' / execution count = {((MockProcessor)args.ProcessorThread.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadStopping += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadStopping] : name = '{args.ProcessorThread.Name}' / execution count = {((MockProcessor)args.ProcessorThread.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadOperationCanceled += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadOperationCanceled] : name = '{args.ProcessorThread.Name}' / execution count = {((MockProcessor)args.ProcessorThread.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        var processorThreadPool = new ProcessorThreadPool("thread-pool", 5, serviceScopeFactory.Object, processorFactory.Object, threadingOptions);

        await processorThreadPool.StartAsync(cancellationToken);

        var timeout = DateTime.Now.AddSeconds(5);
        var timedOut = false;

        while (processorThreadPool.ProcessorThreads.Any(item => ((MockProcessor)item.Processor).ExecutionCount <= minimumExecutionCount && !timedOut))
        {
            await Task.Delay(25, cancellationToken).ConfigureAwait(false);

            timedOut = DateTime.Now >= timeout;
        }

        await cancellationTokenSource.CancelAsync();

        await processorThreadPool.StopAsync();

        Assert.That(timedOut, Is.False, $"[TIMEOUT] : Did not complete {minimumExecutionCount} executions before {timeout:O}");
    }
}