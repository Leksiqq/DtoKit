namespace Net.Leksi.Dto;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ActualPropertyAttribute : Attribute
{
    private string _propertyName;
    public ActualPropertyAttribute(string propertyName)
    {
        _propertyName = propertyName;
    }

    public string PropertyName => _propertyName;
}
