using NUnit.Framework;

namespace Shuttle.Core.Threading.Tests;

[TestFixture]
public class AmbientContextFixture
{
    [Test]
    public void Should_be_able_to_flow_data()
    {
        var d1 = new object();
        var t1 = default(object);
        var t10 = default(object);
        var t11 = default(object);
        var t12 = default(object);
        var t13 = default(object);
        var d2 = new object();
        var t2 = default(object);
        var t20 = default(object);
        var t21 = default(object);
        var t22 = default(object);
        var t23 = default(object);

        Task.WaitAll(
            Task.Run(() =>
            {
                AmbientContext.SetData("d1", d1);
                new Thread(() => t10 = AmbientContext.GetData("d1")).Start();
                Task.WaitAll(
                    Task.Run(() => t1 = AmbientContext.GetData("d1"))
                        .ContinueWith(t => Task.Run(() => t11 = AmbientContext.GetData("d1"))),
                    Task.Run(() => t12 = AmbientContext.GetData("d1")),
                    Task.Run(() => t13 = AmbientContext.GetData("d1"))
                );
            }),
            Task.Run(() =>
            {
                AmbientContext.SetData("d2", d2);
                new Thread(() => t20 = AmbientContext.GetData("d2")).Start();
                Task.WaitAll(
                    Task.Run(() => t2 = AmbientContext.GetData("d2"))
                        .ContinueWith(t => Task.Run(() => t21 = AmbientContext.GetData("d2"))),
                    Task.Run(() => t22 = AmbientContext.GetData("d2")),
                    Task.Run(() => t23 = AmbientContext.GetData("d2"))
                );
            })
        );

        Assert.That(d1, Is.SameAs(t1));
        Assert.That(d1, Is.SameAs(t10));
        Assert.That(d1, Is.SameAs(t11));
        Assert.That(d1, Is.SameAs(t12));
        Assert.That(d1, Is.SameAs(t13));

        Assert.That(d2, Is.SameAs(t2));
        Assert.That(d2, Is.SameAs(t20));
        Assert.That(d2, Is.SameAs(t21));
        Assert.That(d2, Is.SameAs(t22));
        Assert.That(d2, Is.SameAs(t23));

        Assert.That(AmbientContext.GetData("d1"), Is.Null);
        Assert.That(AmbientContext.GetData("d2"), Is.Null);
    }
}