using NUnit.Framework;

namespace Shuttle.Core.Threading.Tests;

[TestFixture]
public class ThreadActivityFixture
{
    [Test]
    public async Task Should_be_able_to_have_the_thread_wait_async()
    {
        var activity = new ThreadActivity(
        [
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(500)
        ]);

        var start = DateTime.Now;
        var token = new CancellationToken(false);

        await activity.SignalAsync(false, token);

        Assert.That((DateTime.Now - start).TotalMilliseconds >= 250, Is.True);

        await activity.SignalAsync(false, token);

        Assert.That((DateTime.Now - start).TotalMilliseconds >= 750, Is.True);
    }
}