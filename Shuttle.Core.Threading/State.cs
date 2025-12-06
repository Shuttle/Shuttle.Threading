using Shuttle.Core.Contract;

namespace Shuttle.Core.Threading;

public class State : IState
{
    private readonly List<string> _immutableKeys = ["Name", "ManagedThreadId"];
    private readonly Dictionary<string, object?> _state = new();

    public void Clear()
    {
        _state.Clear();
    }

    public void Add(string key, object? value)
    {
        _state.Add(Guard.AgainstEmpty(key), value);
    }

    public void Replace(string key, object? value)
    {
        if (_immutableKeys.Contains(Guard.AgainstEmpty(key)))
        {
            throw new InvalidOperationException(string.Format(Resources.ImmutableKeyException, key));
        }

        _state.Remove(key);
        _state.Add(key, value);
    }

    public object? Get(string key)
    {
        return _state.TryGetValue(Guard.AgainstEmpty(key), out var result) ? result : default;
    }

    public bool Contains(string key)
    {
        return _state.ContainsKey(Guard.AgainstEmpty(key));
    }

    public bool Remove(string key)
    {
        if (_immutableKeys.Contains(Guard.AgainstEmpty(key)))
        {
            throw new InvalidOperationException(string.Format(Resources.ImmutableKeyException, key));
        }

        return _state.Remove(key);
    }
}