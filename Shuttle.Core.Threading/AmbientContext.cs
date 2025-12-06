using System.Collections.Concurrent;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

// Daniel Cazzulino: http://www.cazzulino.com/callcontext-netstandard-netcore.html
public static class AmbientContext
{
    private static readonly ConcurrentDictionary<string, AsyncLocal<object>> State = new();

    public static object? GetData(string name)
    {
        return State.TryGetValue(Guard.AgainstEmpty(name), out var data) ? data.Value : null;
    }

    public static void SetData(string name, object data)
    {
        State.GetOrAdd(Guard.AgainstEmpty(name), _ => new()).Value = data;
    }
}