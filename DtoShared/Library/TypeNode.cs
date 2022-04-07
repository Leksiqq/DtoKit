namespace Net.Leksi.Dto;

public class TypeNode
{
    public Type Type { get; internal set; }
    public Type ActualType { get; internal set; }
    public List<PropertyNode>? ChildNodes { get; internal set; }
    public int KeysCount { get; internal set; } = 0;

    public List<ValueRequest>? ValueRequests { get; internal set; } = null;

    public object[] GetKey(object item)
    {
        return ChildNodes?.Take(KeysCount).Select(v => v.PropertyInfo.GetValue(item)).ToArray();
    }

}
