using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Shuttle.Core.Threading.Tests;

public class ProcessorThreadFixture
{
    [Test]
    public async Task Should_be_able_to_execute_processor_thread_async()
    {
        const int minimumExecutionCount = 5;

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();

        serviceScopeFactory.Setup(m => m.CreateScope()).Returns(new Mock<IServiceScope>().Object);

        var threadingOptions = new ThreadingOptions();

        var executionDuration = TimeSpan.FromMilliseconds(200);
        var mockProcessor = new MockProcessor(executionDuration);
        var processorThread = new ProcessorThread("thread", new Mock<IProcessorThreadPool>().Object, mockProcessor, serviceScopeFactory.Object, threadingOptions);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

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

        await processorThread.StartAsync(cancellationToken);

        var timeout = DateTime.Now.AddSeconds(500);
        var timedOut = false;

        while (mockProcessor.ExecutionCount <= minimumExecutionCount && !timedOut)
        {
            await Task.Delay(25, cancellationToken).ConfigureAwait(false);

            timedOut = DateTime.Now >= timeout;
        }

        await cancellationTokenSource.CancelAsync();

        await processorThread.StopAsync();

        Assert.That(timedOut, Is.False, $"[TIMEOUT] : Did not complete {minimumExecutionCount} executions before {timeout:O}");
    }
}