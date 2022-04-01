using System.Text;

namespace Net.Leksi.Dto;

public class DtoBuilder { 
    
    public event ValueRequestEventHandler ValueRequest;

    private const string Slash = "/";
    private const string IDontKnowWhy = "Какая-то неведомая фигня";

    private readonly TypesForest _typesForest;
    private readonly List<string> _paths = new();
    private readonly List<string> _ignoredPaths = new();
    private readonly ObjectCache _objectCache = new();
    private readonly Dictionary<Type, object> _probeObjects = new();

    public object? Target { get; set; } = null;

    public DtoBuilder(TypesForest typesForest)
    {
        _typesForest = typesForest ?? throw new ArgumentNullException(nameof(typesForest));

    }

    public T Build<T>() where T : class
    {
        if (!_typesForest.ServiceProvider.IsRegistered(typeof(T)))
        {
            throw new ArgumentException($"{typeof(T)} is not registered.");
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

    public string GenerateHandlerSkeleton<T>() where T : class
    {
        StringBuilder sb = new();
        sb.Append(@"
(ValueRequestEventArgs args) => {
    switch(args.Path) {");
        ValueRequestEventHandler eh = args =>
        {
            sb.Append(@$"
        case ""{args.Path}"":");
            switch (args.Kind)
            {
                case ValueRequestKind.Terminal:
                    sb.Append(@"
            //args.Value = ...;
            //args.Commit();");
                    args.Commit();
                    break;
                case ValueRequestKind.NullableNode:
                    sb.Append(@"
            //args.Value = null;
            //args.Commit();");
                    break;
                case ValueRequestKind.NotNullableNode:
                    sb.Append(@"
            //args.Commit();");
                    break;
            }
            sb.Append(@"
            break;");
        };
        ValueRequest += eh;
        Build<T>();
        ValueRequest -= eh;
        sb.AppendLine(@"
    }
}");
        return sb.ToString();
    }

    private object Build(PropertyNode propertyNode, object target)
    {
        ValueRequestEventArgs eventArgs = new();
        if (target is null)
        {
            if(_probeObjects.TryGetValue(propertyNode.TypeNode.Type, out object probe))
            {
                _probeObjects.Remove(propertyNode.TypeNode.Type);
                target = probe;
            }
            else
            {
                target = _typesForest.ServiceProvider.GetService(propertyNode.TypeNode.Type);
            }
        }
        ValueRequestKind valueRequestKind = 
            propertyNode.IsNullable 
                ? ValueRequestKind.NullableNode 
                : ValueRequestKind.NotNullableNode
            ;
        eventArgs.Init(null, target, $"{Slash}{string.Join(Slash, _paths)}", valueRequestKind);
        ValueRequest!.Invoke(eventArgs);
        if (eventArgs.IsReset)
        {
            if(!_probeObjects.TryAdd(propertyNode.TypeNode.Type, target))
            {
                throw new Exception(IDontKnowWhy);
            }
            return null;
        }
        if (eventArgs.IsCommited)
        {
            return target;
        }
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
                    eventArgs.Init(child.PropertyInfo, target, $"{Slash}{string.Join(Slash, _paths)}", ValueRequestKind.Terminal);
                    ValueRequest!.Invoke(eventArgs);
                    if (!eventArgs.IsCommited)
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
                    if(_objectCache.TryGet(propertyNode.TypeNode.Type, key, out object cachedObject))
                    {
                        if (!_probeObjects.TryAdd(propertyNode.TypeNode.Type, target))
                        {
                            throw new Exception(IDontKnowWhy);
                        }
                        target = cachedObject;
                        key = null;
                        break;
                    }
                }
            }
            if(key is { })
            {
                _objectCache.Add(propertyNode.TypeNode.Type, key, target);
            }
        }
        return target;

    }
}
