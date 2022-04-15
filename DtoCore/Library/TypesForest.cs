using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

namespace Net.Leksi.Dto;

public class TypesForest
{
    
    private const string Slash = "/";
    private const string Dot = ".";
    private const string _nullableAttributeName = "NullableAttribute";

    private static readonly PropertyNodeComparer _propertyNodeComparer = new();

    public Dictionary<Type, TypeNode> TypeTrees { get; init; } = new();

    public DtoServiceProvider ServiceProvider { get; init; }

    public TypesForest(DtoServiceProvider _serviceProvider)
    {
        ServiceProvider = _serviceProvider;
    }

    public TypeNode GetTypeNode(Type type)
    {
        if (!TypeTrees.ContainsKey(type))
        {
            PlantTypeTree(type);
        }
        return TypeTrees[type];
    }

    public TypeNode GetTypeNode<T>() where T : class
    {
        return GetTypeNode(typeof(T));
    }

    public PropertyNode? GetPropertyNode(TypeNode typeNode, string propertyName)
    {
        if (typeNode?.ChildNodes is { } children)
        {
            return children.Find(propertyNode => propertyNode.Name == propertyName);
        }
        return null;
    }

    public bool PlantTypeTree(Type type)
    {
        if (!ServiceProvider.IsRegistered(type))
        {
            throw new ArgumentException($"{type} is not registered.");
        }
        bool result = !TypeTrees.ContainsKey(type);
        if (result)
        {
            lock (type)
            {
                result = !TypeTrees.ContainsKey(type);
                if (result)
                {
                    TypeTrees[type] = new TypeNode { Type = type, ChildNodes = new List<PropertyNode>() };
                    List<Type> antiLoop = new() { type };
                    ConfigureTypeNode(TypeTrees[type], antiLoop);

                    TypeTrees[type].ValueRequests = new List<ValueRequest>();
                    CollectValueRequests(TypeTrees[type], TypeTrees[type].ValueRequests);

                    //Console.WriteLine();
                    //Console.WriteLine(string.Join("\n", TypeTrees[type].ValueRequests.Select(v => $"{v.Path}, {(!v.PropertyNode.IsLeaf ? ("+" + v.PropertyNode.TypeNode.Type) : string.Empty) }, {(v.PopsCount != 0 ? v.PopsCount.ToString() : string.Empty)}")));
                }
            }
        }
        return result;
    }

    private void ConfigureTypeNode(TypeNode typeNode, List<Type> antiLoop)
    {
        List<PropertyInfo> properties = new();
        CollectProperties(typeNode, properties, antiLoop);

        typeNode.ActualType = ServiceProvider.GetRequiredService(typeNode.Type).GetType();
        List<PropertyInfo> actualProperties = new();
        CollectActualProperties(typeNode, actualProperties, antiLoop);

        CollectChildNodes(typeNode, properties, actualProperties, antiLoop);
    }

    private void CollectValueRequests(TypeNode typeNode, List<ValueRequest> requests)
    {
        StringBuilder path = new();
        if(path.Length == 0)
        {
            requests.Add(new ValueRequest
            {
                Path = Slash,
                PropertyNode = new PropertyNode { TypeNode = typeNode }
            });
        }
        foreach(PropertyNode propertyNode in typeNode.ChildNodes)
        {
            int pathLength = path.Length;
            path.Append(Slash).Append(propertyNode.PropertyInfo.Name);
            ValueRequest request = new ValueRequest
            {
                Path = path.ToString(),
                PropertyNode = propertyNode
            };
            requests.Add(request);
            if (!propertyNode.IsLeaf)
            {
                foreach(ValueRequest req in propertyNode.TypeNode.ValueRequests)
                {
                    if(req.Path != Slash)
                    {
                        int pathLength1 = path.Length;
                        path.Append(req.Path);
                        request = new ValueRequest
                        {
                            Path = path.ToString(),
                            PropertyNode = req.PropertyNode,
                            PopsCount = req.PopsCount
                        };
                        requests.Add(request);
                        path.Length = pathLength1;
                    }
                }
            }
            path.Length = pathLength;
        }
        requests.Last().PopsCount--;
    }

    private void CollectChildNodes(TypeNode typeNode, List<PropertyInfo> properties, 
        List<PropertyInfo> actualProperties, List<Type> antiLoop)
    {
        foreach (PropertyInfo propertyInfo in actualProperties)
        {
            PropertyInfo? actualProperty = null;
            if (propertyInfo.GetCustomAttribute<KeyAttribute>() is KeyAttribute)
            {
                if (propertyInfo.CanWrite)
                {
                    typeNode.KeysCount++;
                    actualProperty = propertyInfo;
                }
            }
            else if (propertyInfo.Name.Contains(Dot))
            {
                if (propertyInfo.Name.StartsWith(typeNode.Type.FullName!)
                    && propertyInfo.Name.LastIndexOf(Dot) == typeNode.Type.FullName!.Length
                )
                {
                    if (propertyInfo.GetCustomAttribute<ActualPropertyAttribute>() is { } actualPropertyAttr)
                    {
                        actualProperty = actualProperties.Where(
                            v => v.Name == actualPropertyAttr.PropertyName
                        ).FirstOrDefault();
                    }
                    else
                    {
                        actualProperty = actualProperties.Where(
                            v => v.Name == propertyInfo.Name.Substring(propertyInfo.Name.LastIndexOf(Dot) + 1)
                        ).FirstOrDefault();
                    }
                }
            }
            else
            {
                PropertyInfo sourceProperty = properties.Where(
                    v => v.Name == propertyInfo.Name && v.PropertyType == propertyInfo.PropertyType
                ).FirstOrDefault();
                if (sourceProperty is PropertyInfo)
                {
                    actualProperty = propertyInfo;
                }
            }
            if (actualProperty is PropertyInfo && actualProperty.CanWrite)
            {
                bool foundTypeNode = TypeTrees.ContainsKey(propertyInfo.PropertyType);
                var newPropertyNode = new PropertyNode
                {
                    Name = propertyInfo.Name.Contains(Dot) 
                        ? propertyInfo.Name.Substring(propertyInfo.Name.LastIndexOf(Dot) + 1) 
                        : propertyInfo.Name,
                    PropertyInfo = actualProperty,
                    TypeNode = ServiceProvider.IsRegistered(propertyInfo.PropertyType) 
                        ? GetTypeNode(propertyInfo.PropertyType) 
                        : new TypeNode { Type = propertyInfo.PropertyType, ActualType = propertyInfo.PropertyType },
                    IsNullable = propertyInfo.GetCustomAttributes().Any(a => a.GetType().Name.Contains(_nullableAttributeName))
                };
                typeNode.ChildNodes.Add(newPropertyNode);
            }
        }
        typeNode.ChildNodes.Sort(_propertyNodeComparer);
    }

    private void CollectActualProperties(TypeNode typeNode, List<PropertyInfo> actualProperties, List<Type> antiLoop)
    {
        Type currentType = typeNode.ActualType;
        List<Type> considered = new();
        while (currentType != typeof(object))
        {
            foreach (PropertyInfo propertyInfo in currentType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (antiLoop.Contains(propertyInfo.PropertyType))
                {
                    throw new Exception($"Loop detected: {string.Join(Slash, antiLoop.Select(t => t.Name))}/{propertyInfo.PropertyType.Name}");
                }
                actualProperties.Add(propertyInfo);
            }
            currentType = currentType.BaseType;
        }
    }

    private void CollectProperties(TypeNode typeNode, List<PropertyInfo> properties, List<Type> antiLoop)
    {
        Queue<Type> queue = new();
        List<Type> considered = new();
        queue.Enqueue(typeNode.Type);
        while (queue.Count > 0)
        {
            Type subType = queue.Dequeue();
            foreach (Type subInterface in subType.GetInterfaces())
            {
                if (!considered.Contains(subInterface))
                {
                    queue.Enqueue(subInterface);
                }
                considered.Add(subInterface);
            }
            foreach (PropertyInfo propertyInfo in subType.GetProperties())
            {
                if (antiLoop.Contains(propertyInfo.PropertyType))
                {
                    throw new Exception($"Loop detected: {string.Join(Slash, antiLoop.Select(t => t.Name))}/{propertyInfo.PropertyType.Name}");
                }
                properties.Add(propertyInfo);
            }
        }
    }

    public void Copy(Type sourceType, object source, object target)
    {
        TypeNode typeNode = GetTypeNode(sourceType);
        foreach(PropertyNode propertyNode in typeNode.ChildNodes)
        {
            object? sourceValue = propertyNode.PropertyInfo.GetValue(source);
            if (sourceValue is null) {
                propertyNode.PropertyInfo.SetValue(target, null);
            }
            else
            {
                if(propertyNode.TypeNode.ChildNodes is { } children)
                {
                    object? targetValue = propertyNode.PropertyInfo.GetValue(target);
                    if (targetValue is null)
                    {
                        targetValue = ServiceProvider.GetRequiredService(propertyNode.TypeNode.Type);
                        propertyNode.PropertyInfo.SetValue(target, targetValue);
                    }
                    Copy(propertyNode.TypeNode.Type, sourceValue, targetValue);
                }
                else
                {
                    propertyNode.PropertyInfo.SetValue(target, sourceValue);
                }
            }

        }
    }
}
