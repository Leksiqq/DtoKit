namespace Net.Leksi.Dto;

public class TypeNode
{
    public Type Type { get; set; }
    public List<PropertyNode>? ChildNodes { get; set; }
    public bool IsConfirmed { get; set; } = false;
    public int KeysCount { get; set; } = 0;

}
