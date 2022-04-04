using System.Diagnostics.CodeAnalysis;

namespace Net.Leksi.Dto;

public class KeyComparer : IEqualityComparer<object>
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