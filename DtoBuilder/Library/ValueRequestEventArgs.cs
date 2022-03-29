using System.Reflection;

namespace Net.Leksi.Dto;

public class ValueRequestEventArgs : EventArgs
{
    internal static readonly object NewValue = new();

    private PropertyInfo _propertyInfo;
    private string _path;
    private ValueRequestStatus _status = ValueRequestStatus.Pending;

    internal void SetPropertyInfo(PropertyInfo propertyInfo)
    {
        _propertyInfo = propertyInfo;
    }

    internal object Target { get; set; }

    internal void SetPath(string path)
    {
        _path = path;
    }

    internal void ResetStatus()
    {
        _status = ValueRequestStatus.Pending;
    }

    public object Value
    {
        get
        {
            return _propertyInfo.GetValue(Target);
        }
        set
        {
            if(Target is null)
            {
                Target = value;
            }
            else
            {
                _propertyInfo.SetValue(Target, value);
            }
        }
    }
    public string Path 
    {
        get
        {
            return _path;
        }
    }
    public ValueRequestStatus Status
    {
        get
        {
            return (ValueRequestStatus)_status;
        }
        set
        { 
            if(_status is not ValueRequestStatus.Pending || value is ValueRequestStatus.Pending)
            {
                throw new InvalidOperationException();
            }
            _status = value;
        }
    }
    public void CreateDefault()
    {
        if(Target is { })
        {
            throw new InvalidOperationException();
        }
        Value = NewValue;
        Status = ValueRequestStatus.Node;
    }
}
