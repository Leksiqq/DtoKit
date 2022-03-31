using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace Net.Leksi.Dto;

public class DtoBuilder { 
    private class KeyComparer : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y)
        {
            if (x == y)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return ((object[])x).Length == ((object[])y).Length && ((object[])x).Zip((object[])y).All(v => v.First.Equals(v.Second));
        }

        public int GetHashCode([DisallowNull] object obj)
        {
            int result = ((object[])obj).Select(v => v.GetHashCode()).Aggregate(0, (v, res) => unchecked (v + res));
            return result;
        }
    }

    public event ValueRequestEventHandler ValueRequest;

    private const string Slash = "/";
    private const string Dot = ".";

    private static readonly KeyComparer _keyComparer = new();

    private TypesForest _typesForest;
    private List<string> _paths = new();
    private Dictionary<Type, Dictionary<object[], object>> _objectsCache = new();
    private List<string> _ignoredPaths = new();

    public object? Target { get; set; } = null;

    public bool ShouldAutoCreateNotNullable { get; set; } = false;

    public DtoBuilder(TypesForest typesForest)
    {
        _typesForest = typesForest ?? throw new ArgumentNullException(nameof(typesForest));

    }

    public T Build<T>() where T : class
    {
        if (!_typesForest.ServiceProvider.IsRegistered(typeof(T)))
        {
            throw new ArgumentException(nameof(T));
        }
        _typesForest.PlantTypeTree(typeof(T));
        _ignoredPaths.Clear();
        T result = (T)Build(new PropertyNode { TypeNode = _typesForest.GetTypeNode(typeof(T)) }, Target);
        if(_ignoredPaths.Count > 0)
        {
            throw new InvalidOperationException($"path(s) ignored:\n{string.Join("\n", _ignoredPaths)}");

        }
        return result;
    }

    public void ClearObjectCache()
    {
        _objectsCache.Clear();
    }

    public string GenerateHandlerSkeleton<T>(bool WithIsAutoCreateDefault) where T : class
    {
        bool IsAutoCreateDefaultSave = ShouldAutoCreateNotNullable;
        ShouldAutoCreateNotNullable = WithIsAutoCreateDefault;
        StringBuilder sb = new();
        sb.Append(@"
(ValueRequestEventArgs args) => {
    switch(args.Path) {");
        ValueRequestEventHandler eh = args =>
        {
            sb.Append(@$"
        case ""{args.Path}"":
            //args.CreateDefault();
            //args.Value = ...;
            //args.Status = ValueRequestStatus.Node;
            //args.Status = ValueRequestStatus.Terminal;
            break;");
            if(args.Target is { })
            {
                args.Status = ValueRequestStatus.Node;
            }
            else
            {
                args.CreateDefault();
            }
        };
        ValueRequest += eh;
        Build<T>();
        ValueRequest -= eh;
        ShouldAutoCreateNotNullable = IsAutoCreateDefaultSave;
        sb.AppendLine(@"
    }
}");
        return sb.ToString();
    }

    private object Build(PropertyNode propertyNode, object target)
    {
        ValueRequestEventArgs eventArgs = new();
        eventArgs.Target = target;
        if (
            eventArgs.Target is null 
            && ShouldAutoCreateNotNullable 
            && (
                (
                    propertyNode.PropertyInfo is { } pi 
                    && Nullable.GetUnderlyingType(pi.PropertyType) is null
                )
                || propertyNode.PropertyInfo is null
            )
        )
        {
            eventArgs.Target = _typesForest.ServiceProvider.GetService(propertyNode.TypeNode.Type);
        }
        if (eventArgs.Target is null)
        {
            eventArgs.SetPropertyInfo(propertyNode.PropertyInfo);
            eventArgs.SetPath($"{Slash}{string.Join(Slash, _paths)}");
            ValueRequest!.Invoke(eventArgs);
            if (eventArgs.Status is ValueRequestStatus.Pending)
            {
                _ignoredPaths.Add(eventArgs.Path);
            }
            if((eventArgs.Target is null && _ignoredPaths.Count > 0) || (eventArgs.Target == ValueRequestEventArgs.NewValue))
            {
                eventArgs.Target = _typesForest.ServiceProvider.GetService(propertyNode.TypeNode.Type);
            }
            if (eventArgs.Status is ValueRequestStatus.Terminal)
            {
                return eventArgs.Target;
            }
        }
        if(eventArgs.Target is null)
        {
            return null;
        }
        target = eventArgs.Target;
        if(propertyNode.TypeNode.ChildNodes is { } children)
        {
            _typesForest.ConfirmTypeNode(propertyNode.TypeNode, target.GetType());
            int childPosition = 0;
            object[]? key = null;
            foreach (PropertyNode child in children)
            {
                _paths.Add(child.PropertyName);
                if (child.TypeNode.ChildNodes is null)
                {
                    eventArgs.SetPath($"{Slash}{string.Join(Slash, _paths)}");
                    eventArgs.ResetStatus();
                    eventArgs.SetPropertyInfo(child.PropertyInfo);
                    ValueRequest!.Invoke(eventArgs);
                    if (eventArgs.Status is ValueRequestStatus.Pending)
                    {
                        _ignoredPaths.Add(eventArgs.Path);
                    }
                }
                else
                {
                    object result = Build(child, child.PropertyInfo.GetValue(target));
                    child.PropertyInfo.SetValue(target, result);
                }
                _paths.RemoveAt(_paths.Count - 1);
                childPosition++;
                if(childPosition == propertyNode.TypeNode.KeysCount)
                {
                    key = children.Take(propertyNode.TypeNode.KeysCount).Select(v => v.PropertyInfo.GetValue(target)).ToArray();
                    if(key.Any(v => v is null))
                    {
                        key = null;
                    } 
                    else
                    {
                        if (_objectsCache.ContainsKey(propertyNode.TypeNode.Type))
                        {
                            if (_objectsCache[propertyNode.TypeNode.Type].TryGetValue(key, out object cachedObject))
                            {
                                target = cachedObject;
                                break;
                            }
                        }
                    }

                }
            }
            if(key is { })
            {
                if (!_objectsCache.ContainsKey(propertyNode.TypeNode.Type))
                {
                    _objectsCache.Add(propertyNode.TypeNode.Type, new Dictionary<object[], object>(_keyComparer));
                }
                _objectsCache[propertyNode.TypeNode.Type][key] = target;
            }
        }
        return target;

    }
}
