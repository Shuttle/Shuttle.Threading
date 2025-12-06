using NUnit.Framework;

namespace Shuttle.Core.Threading.Tests;

[TestFixture]
public class SharedCancellationTokenSourceFixture
{
    private static readonly Random Random = new(DateTime.Now.Millisecond);

    [Test]
    public void Should_be_able_to_shared_cancellation_token_source_entries()
    {
        using (var cancellationTokenSource = new DefaultCancellationTokenSource())
        {
            Assert.That(cancellationTokenSource.Get(), Is.SameAs(cancellationTokenSource.Get()));

            var cancellationToken = cancellationTokenSource.Get().Token;

            var tasks = new List<Task>
            {
                Task.Run(() => Spin("A", cancellationToken), cancellationToken),
                Task.Run(() => Spin("B", cancellationToken), cancellationToken),
                Task.Run(() => Spin("C", cancellationToken), cancellationToken),
                Task.Run(() => Spin("D", cancellationToken), cancellationToken)
            };

            // wait for all the tasks to start
            while (tasks.Any(task => task.Status != TaskStatus.Running))
            {
                Thread.Sleep(100);
            }

            cancellationTokenSource.Get().Cancel();

            Task.WaitAll(tasks.ToArray(), CancellationToken.None);
        }
    }

    private void Spin(string name, CancellationToken cancellationToken)
    {
        Log($@"[starting] : {name}");

        while (!cancellationToken.IsCancellationRequested)
        {
            var timeout = Random.Next(5, 20) * 100;

            Log($@"[sleeping] : {name} / timeout = {timeout}");

            Thread.Sleep(timeout);
        }

        Log($@"[ending] : {name}");
    }

    private void Log(string message)
    {
        Console.WriteLine($@"{DateTime.Now:O} - {message}");
    }
}