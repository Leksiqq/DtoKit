using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

namespace Net.Leksi.Dto;

public class DtoBuilder<T> where T : class
{
    public event ValueRequestEventHandler ValueRequest;

    private const string Slash = "/";
    private const string Dot = ".";

    private PropertyNode _root = new();
    private DtoServiceProvider _serviceProvider;
    private List<Type> _antiLoop = new();
    private List<string> _paths = new();
    private int _propertyNodeIdGen = 0;
    private Dictionary<int, string> _pathsCache = new();

    public T? Target { get; set; } = null;

    public bool IsAutoCreateDefault { get; set; } = false;

    public DtoBuilder(IServiceProvider serviceProvider)
    {
        if(serviceProvider is null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }
        if(serviceProvider is DtoServiceProvider dsp)
        {
            _serviceProvider = dsp;
        }
        else
        {
            _serviceProvider = serviceProvider.GetRequiredService<DtoServiceProvider>();
        }
        if(!_serviceProvider.IsRegistered(typeof(T)))
        {
            throw new ArgumentException(nameof(T));
        }
        _root.ChildNodes = new List<PropertyNode>();

        _antiLoop.Clear();
        _antiLoop.Add(typeof(T));
        FillChildren(_root.ChildNodes, typeof(T));

    }

    public T Build()
    {
        _paths.Clear();
        return (T)Build(_root, Target);
    }

    public string GenerateHandlerStub()
    {
        bool IsAutoCreateDefaultSave = IsAutoCreateDefault;
        IsAutoCreateDefault = true;
        StringBuilder sb = new();
        sb.Append(@"
(ValueRequestEventArgs args) => {
    switch(args.Path) {");
        ValueRequestEventHandler eh = args =>
        {
            sb.Append(@$"
        case ""{args.Path}"":
            //args.CreateDefault();
            //args.Status = ValueRequestStatus.Node;
            //args.Status = ValueRequestStatus.Terminal;
            break;");
            args.Status = ValueRequestStatus.Node;
        };
        ValueRequest += eh;
        Build();
        ValueRequest -= eh;
        IsAutoCreateDefault = IsAutoCreateDefaultSave;
        sb.AppendLine(@"
    }
}");
        return sb.ToString();
    }

    private object Build(PropertyNode propertyNode, object target)
    {
        ValueRequestEventArgs eventArgs = new();
        eventArgs.Target = target;
        Type targetType = propertyNode.PropertyInfo is { } pi ? pi.PropertyType : typeof(T);
        if(eventArgs.Target is null && IsAutoCreateDefault)
        {
            eventArgs.Target = _serviceProvider.GetService(targetType);
        }
        if (eventArgs.Target is null)
        {
            eventArgs.SetPropertyInfo(propertyNode.PropertyInfo);
            string path = _pathsCache.GetValueOrDefault(propertyNode.Id);
            if (path is null)
            {
                path = $"{Slash}{string.Join(Slash, _paths)}";
                _pathsCache[propertyNode.Id] = path;
            }
            eventArgs.SetPath(path);
            ValueRequest!.Invoke(eventArgs);
            if (eventArgs.Status is ValueRequestStatus.Pending || eventArgs.Target is null)
            {
                throw new InvalidOperationException(eventArgs.Path);
            }
            if (eventArgs.Status is ValueRequestStatus.Terminal)
            {
                return eventArgs.Target;
            }
            if(eventArgs.Target == ValueRequestEventArgs.NewValue)
            {
                eventArgs.Target = _serviceProvider.GetService(targetType);
            }
        }
        target = eventArgs.Target;
        if(propertyNode.ChildNodes is { } children)
        {
            Type type = target.GetType();
            if (!propertyNode.IsConfigured)
            {
                foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    if (propertyInfo.GetCustomAttribute<KeyAttribute>() is { })
                    {
                        if (propertyInfo.CanWrite)
                        {
                            propertyNode.ChildNodes.Insert(0, new PropertyNode { PropertyInfo = propertyInfo, Id = ++_propertyNodeIdGen });
                        }
                    }
                    else if (propertyInfo.Name.Contains(Dot))
                    {
                        if (propertyInfo.Name.StartsWith(targetType.FullName) 
                            && propertyInfo.Name.LastIndexOf(Dot) == targetType.FullName.Length 
                            && propertyInfo.GetCustomAttribute<AliasAttribute>() is { } alias)
                        {
                            PropertyInfo aliasProperty = type.GetProperty(alias.PropertyName);
                            if (aliasProperty.CanWrite)
                            {
                                if(children.Where(ch => ch.PropertyInfo.Name.Equals(propertyInfo.Name.Substring(propertyInfo.Name.LastIndexOf(Dot) + 1)))
                                    .FirstOrDefault() is PropertyNode node)
                                {
                                    node.AliasPropertyInfo = aliasProperty;
                                }
                            }
                        }
                    }
                    else
                    {
                        if(children.Where(ch => ch.PropertyInfo.Name.Equals(propertyInfo.Name))
                            .FirstOrDefault() is PropertyNode node)
                        {
                            node.AliasPropertyInfo = propertyInfo;
                        }
                    }
                }
                propertyNode.ChildNodes.RemoveAll(ch => (ch.AliasPropertyInfo is null && !ch.PropertyInfo.CanWrite) 
                    || (ch.AliasPropertyInfo is { } alias && !alias.CanWrite));
                propertyNode.IsConfigured = true;
            }
            foreach(PropertyNode child in children)
            {
                _paths.Add(child.PropertyInfo.Name);
                if (child.ChildNodes is null)
                {
                    string path = _pathsCache.GetValueOrDefault(child.Id);
                    if (path is null)
                    {
                        path = $"{Slash}{string.Join(Slash, _paths)}";
                        _pathsCache[child.Id] = path;
                    }
                    eventArgs.SetPath(path);
                    eventArgs.ResetStatus();
                    ValueRequest!.Invoke(eventArgs);
                    if (eventArgs.Status is ValueRequestStatus.Pending)
                    {
                        throw new InvalidOperationException(eventArgs.Path);
                    }
                }
                else
                {
                    object result = Build(child, (child.AliasPropertyInfo ?? child.PropertyInfo).GetValue(target));
                    (child.AliasPropertyInfo ?? child.PropertyInfo).SetValue(target, result);
                }
                _paths.RemoveAt(_paths.Count - 1);
            }
        }
        return target;

    }

    private void FillChildren(List<PropertyNode> childNodes, Type type)
    {
        foreach (PropertyInfo propertyInfo in type.GetProperties())
        {
            if (_antiLoop.Contains(propertyInfo.PropertyType))
            {
                throw new Exception($"Loop detected: {string.Join(Slash, _antiLoop.Select(t => t.Name))}/{propertyInfo.PropertyType.Name}");
            }
            _antiLoop.Add(propertyInfo.PropertyType);
            PropertyNode node = new PropertyNode { PropertyInfo = propertyInfo, Id = ++_propertyNodeIdGen };
            childNodes.Add(node);
            if (_serviceProvider.IsRegistered(propertyInfo.PropertyType))
            {
                node.ChildNodes = new List<PropertyNode>();
                FillChildren(node.ChildNodes, propertyInfo.PropertyType);
            }
            _antiLoop.RemoveAt(_antiLoop.Count - 1);
        }
    }
}
