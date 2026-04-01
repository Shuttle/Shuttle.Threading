using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Shuttle.Core.Threading.Tests;

public class ProcessorThreadFixture
{
    [Test]
    public async Task Should_be_able_to_execute_processor_thread_async()
    {
        const int minimumExecutionCount = 5;
        var executionDuration = TimeSpan.FromMilliseconds(200);
        var mockProcessor = new MockProcessor(executionDuration);

        var serviceProvider = new ServiceCollection()
            .AddThreading()
            .Services
            .AddKeyedTransient<IProcessor>("thread", (_, _) => mockProcessor)
            .BuildServiceProvider();

        var threadingOptions = new ThreadingOptions();

        var processorThread = new ProcessorThread("thread", serviceProvider.GetRequiredService<IServiceScopeFactory>(), threadingOptions, new NullProcessorIdleStrategy());
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        threadingOptions.ProcessorException += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorException] : service key = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId} / exception = '{args.Exception}'");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorExecuting += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorExecuting] : service key = '{args.ServiceKey}' / execution count = {((MockProcessor)args.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorExecuted += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorExecuted] : service key = '{args.ServiceKey}' / execution count = {((MockProcessor)args.Processor).ExecutionCount} / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadActive += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadActive] : service key = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadStarting += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadStarting] : service key = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadStopped += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadStopped] : service key = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadStopping += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadStopping] : service key = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        threadingOptions.ProcessorThreadOperationCanceled += (args, _) =>
        {
            Console.WriteLine($@"{DateTime.Now:O} - [ProcessorThreadOperationCanceled] : service key = '{args.ProcessorThread.ServiceKey}' / managed thread id = {args.ManagedThreadId}");

            return Task.CompletedTask;
        };

        await processorThread.StartAsync(cancellationToken);

        var timeout = DateTime.Now.AddSeconds(5);
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