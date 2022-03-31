namespace Net.Leksi.Dto;

[AttributeUsage(AttributeTargets.Property)]
public class AliasForAttribute : Attribute
{
    private string _propertyName;
    public AliasForAttribute(string propertyName)
    {
        _propertyName = propertyName;
    }

    public string PropertyName => _propertyName;
}
