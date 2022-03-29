namespace Net.Leksi.Dto;

[AttributeUsage(AttributeTargets.Property)]
public class AliasAttribute : Attribute
{
    private string _propertyName;
    public AliasAttribute(string propertyName)
    {
        _propertyName = propertyName;
    }

    public string PropertyName => _propertyName;
}
