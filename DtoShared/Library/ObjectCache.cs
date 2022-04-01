using System.Diagnostics.CodeAnalysis;

namespace Net.Leksi.Dto;

public class ObjectCache
{
    private class KeyComparer : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y)
        {
            if (x == y)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return ((object[])x).Length == ((object[])y).Length && ((object[])x).Zip((object[])y)
                .All(v => v.First is null && v.Second is null || v.First is { } && v.Second is { } && v.First.Equals(v.Second));
        }

        public int GetHashCode([DisallowNull] object obj)
        {
            int result = ((object[])obj).Select(v => v is null ? 0 : v.GetHashCode()).Aggregate(0, (v, res) => unchecked(v + res));
            return result;
        }
    }

    private static readonly KeyComparer _keyComparer = new();

    private Dictionary<Type, Dictionary<object[], object>> _objectsCache = new();

    public bool TryGet(Type type, object[] key, out object result)
    {
        if (_objectsCache.ContainsKey(type))
        {
            return _objectsCache[type].TryGetValue(key, out result);
        }
        result = default;
        return false;
    }

    public void Add(Type type, object[] key, object value)
    {
        if (!_objectsCache.ContainsKey(type))
        {
            _objectsCache.Add(type, new Dictionary<object[], object>(_keyComparer));
        }
        _objectsCache[type][key] = value;

    }

}
