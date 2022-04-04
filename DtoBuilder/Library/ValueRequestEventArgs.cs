using System.Reflection;

namespace Net.Leksi.Dto;

public class ValueRequestEventArgs : EventArgs
{
    private PropertyInfo? _propertyInfo;
    private string _path;
    private object _target;
    private ValueRequestKind _kind;
    private bool _isReset = false;

    internal object SuggestedValue { get; set; }

    internal bool IsReset => _isReset;

    public object Value
    {
        get
        {
            return (_kind is ValueRequestKind.Terminal) ? _propertyInfo?.GetValue(_target) : _target;
        }
        set
        {
            if (IsCommited)
            {
                throw new InvalidOperationException("Request is already terminated.");
            }
            if (_kind is ValueRequestKind.NotNullableNode)
            {
                throw new InvalidOperationException("At NotNullableNode request a value cannot be assigned.");
            }
            if (_kind is ValueRequestKind.NullableNode)
            {
                if(value is { })
                {
                    throw new InvalidOperationException("At NullableNode request only null can be assigned.");
                }
                _isReset = true;
                IsCommited = true;
            }
            else
            {
                _propertyInfo.SetValue(_target, value);
            }
        }
    }
    public string Path => _path;
    public PropertyInfo PropertyInfo => _propertyInfo;

    public ValueRequestKind Kind => _kind;

    public Type ExpectedType => _propertyInfo?.PropertyType ?? _target.GetType();

    public bool IsCommited { get; private set; } = false;

    internal void Init(PropertyInfo propertyInfo, object target, string path, ValueRequestKind kind)
    {
        _propertyInfo = propertyInfo;
        _target = target;
        _path = path;
        _kind = kind;
        IsCommited = false;
    }

    public void Commit()
    {
        if (IsCommited)
        {
            throw new InvalidOperationException("Request is already confirmed.");
        }
        IsCommited = true;
    }

}
