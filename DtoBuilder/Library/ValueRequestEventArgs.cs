namespace Net.Leksi.Dto;

public class ValueRequestEventArgs : EventArgs
{
    private PropertyNode _propertyNode;
    private string _path;
    private object _target;
    private bool _isReset = false;

    internal PropertyNode PropertyNode => _propertyNode;

    internal object SuggestedValue { get; set; }

    internal bool IsReset => _isReset;

    public object Value
    {
        get
        {
            return (Kind is ValueRequestKind.Terminal) ? _propertyNode.PropertyInfo?.GetValue(_target) : _target;
        }
        set
        {
            if (IsCommited)
            {
                throw new InvalidOperationException("Request is already commited.");
            }
            if (Kind is ValueRequestKind.NotNullableNode)
            {
                throw new InvalidOperationException("At NotNullableNode request a value cannot be assigned.");
            }
            if (Kind is ValueRequestKind.NullableNode)
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
                _propertyNode.PropertyInfo.SetValue(_target, value);
            }
        }
    }
    public string Path => _path;

    public ValueRequestKind Kind
    {
        get
        {
            if (_propertyNode.PropertyInfo is null || (!_propertyNode.IsLeaf && !_propertyNode.IsNullable))
            {
                return ValueRequestKind.NotNullableNode;
            }
            if (!_propertyNode.IsLeaf)
            {
                return ValueRequestKind.NullableNode;
            }
            return ValueRequestKind.Terminal;
        }
    }

    public Type ExpectedType => _propertyNode?.PropertyInfo.PropertyType ?? _target.GetType();

    public bool IsCommited { get; private set; } = false;


    internal void Init(PropertyNode propertyNode, object target, string path)
    {
        _propertyNode = propertyNode;
        _path = path;
        _target = target;
        IsCommited = false;
        _isReset = false;
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
