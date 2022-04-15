using System.Diagnostics.CodeAnalysis;

namespace Net.Leksi.Dto;

public class ObjectCache
{

    private static readonly KeyComparer _keyComparer = new();

    private Dictionary<Type, Dictionary<object[], object>> _objectsCache = new();

    public int Count => _objectsCache.Count;
    
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

    public void Clear()
    {
        _objectsCache.Clear();
    }

}
