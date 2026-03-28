using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Extensions.Options;

namespace Shuttle.Core.Threading.Tests;

[TestFixture]
public class AddThreadingFixture
{
    [Test]
    public void Should_allow_last_writer_wins_for_threading_options()
    {
        var provider = new ServiceCollection()
            .AddThreading(builder =>
            {
                builder.Configure(options =>
                {
                    options.JoinTimeout = TimeSpan.FromSeconds(1);
                });
            })
            .AddThreading(builder =>
            {
                builder.Configure(options =>
                {
                    options.JoinTimeout = TimeSpan.FromSeconds(5);
                });
            })
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<ThreadingOptions>>().Value;

        Assert.That(options.JoinTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Should_keep_named_processor_idle_options_isolated()
    {
        var provider = new ServiceCollection()
            .AddThreading(builder =>
            {
                builder.Configure("thread", options => options.Durations.Add(TimeSpan.FromMilliseconds(100)));
            }).AddThreading(builder =>
            {
                builder.Configure("background", options => options.Durations.Add(TimeSpan.FromSeconds(1)));
            })
            .BuildServiceProvider();

        var monitor = provider.GetRequiredService<IOptionsMonitor<ProcessorIdleOptions>>();

        Assert.That(monitor.Get("thread").Durations.Single(), Is.EqualTo(TimeSpan.FromMilliseconds(100)));
        Assert.That(monitor.Get("background").Durations.Single(), Is.EqualTo(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public async Task Should_merge_async_event_handlers_from_multiple_AddThreading_calls()
    {
        var invokedA = 0;
        var invokedB = 0;

        AsyncEventHandler<ProcessorExecutingEventArgs> handlerA = (_, _) =>
        {
            Interlocked.Increment(ref invokedA);
            return Task.CompletedTask;
        };

        AsyncEventHandler<ProcessorExecutingEventArgs> handlerB = (_, _) =>
        {
            Interlocked.Increment(ref invokedB);
            return Task.CompletedTask;
        };

        var provider = new ServiceCollection()
            .AddThreading(builder =>
            {
                builder.Configure(options =>
                {
                    options.ProcessorExecuting += handlerA;
                });
            }).AddThreading(builder =>
            {
                builder.Configure(options =>
                {
                    options.ProcessorExecuting += handlerB;
                });
            })
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<ThreadingOptions>>().Value;

        Assert.That(options.ProcessorExecuting.Count, Is.EqualTo(2));

        await options.ProcessorExecuting.InvokeAsync(new("service-key", 0, new Mock<IProcessor>().Object), CancellationToken.None);

        Assert.That(invokedA, Is.EqualTo(1));
        Assert.That(invokedB, Is.EqualTo(1));
    }

    [Test]
    public void Should_merge_processor_idle_options_from_multiple_AddThreading_calls()
    {
        var provider = new ServiceCollection()
            .AddThreading(builder =>
            {
                builder.Configure("thread", options => options.Durations.Add(TimeSpan.FromMilliseconds(100)));
            }).AddThreading(builder =>
            {
                builder.Configure("thread", options => options.Durations.Add(TimeSpan.FromMilliseconds(500)));
            })
            .BuildServiceProvider();

        var monitor = provider.GetRequiredService<IOptionsMonitor<ProcessorIdleOptions>>();
        var options = monitor.Get("thread");

        Assert.That(options.Durations.Count, Is.EqualTo(2));
        Assert.That(options.Durations, Is.EquivalentTo([TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500)]));
    }

    [Test]
    public void Should_not_add_same_async_event_handler_twice()
    {
        var invoked = 0;

        AsyncEventHandler<ProcessorExecutingEventArgs> handler = (_, _) =>
        {
            Interlocked.Increment(ref invoked);
            return Task.CompletedTask;
        };

        var provider = new ServiceCollection()
            .AddThreading(builder =>
            {
                builder.Configure(options =>
                {
                    options.ProcessorExecuting += handler;
                });
            }).AddThreading(builder =>
            {
                builder.Configure(options =>
                {
                    options.ProcessorExecuting += handler;
                });
            })
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<ThreadingOptions>>().Value;

        Assert.That(options.ProcessorExecuting.Count, Is.EqualTo(1));
    }

    [Test]
    public void Should_preserve_async_event_instance_across_multiple_configure_calls()
    {
        AsyncEvent<ProcessorExecutingEventArgs>? instanceFromFirstConfigure = null;

        var provider = new ServiceCollection()
            .AddThreading(builder =>
            {
                builder.Configure(options =>
                {
                    instanceFromFirstConfigure = options.ProcessorExecuting;
                });
            }).AddThreading(builder =>
            {
                builder.Configure(options =>
                {
                    Assert.That(options.ProcessorExecuting, Is.SameAs(instanceFromFirstConfigure));
                });
            })
            .BuildServiceProvider();

        _ = provider.GetRequiredService<IOptions<ThreadingOptions>>().Value;
    }

    [Test]
    public void Should_throw_when_processor_idle_options_are_invalid()
    {
        var provider = new ServiceCollection()
            .AddThreading(builder =>
            {
                builder.Configure("thread", _ =>
                {
                });
            })
            .AddOptions<ProcessorIdleOptions>()
            .Services
            .BuildServiceProvider();

        var monitor = provider.GetRequiredService<IOptionsMonitor<ProcessorIdleOptions>>();

        Assert.Throws<OptionsValidationException>(() =>
        {
            _ = monitor.Get("thread");
        });
    }
}