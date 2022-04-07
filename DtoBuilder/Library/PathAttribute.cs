namespace Net.Leksi.Dto;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PathAttribute: Attribute
{
    public string Path { get; init; }
    public Type DelegateType { get; init; }
    public PathAttribute(string path, Type delegateType)
    {
        Path = path;
        DelegateType = delegateType;
    }
}
