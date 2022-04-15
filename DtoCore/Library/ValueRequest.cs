namespace Net.Leksi.Dto;

public class ValueRequest
{
    public int PopsCount { get; internal set; } = 0;

    public PropertyNode PropertyNode { get; internal set; } = null!;

    public string Path { get; internal set; } = string.Empty;

    public override string ToString()
    {
        return $"{Path}, {(PropertyNode.IsLeaf ? string.Empty : "+" + PropertyNode.TypeNode.Type)}, {PopsCount}";
    }
}
