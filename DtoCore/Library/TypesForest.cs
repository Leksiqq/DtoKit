﻿using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// "Лес" деревьев "свойств и типов" - служит для построения и хранения таких деревьев
/// </para>
/// <para xml:lang="en">
/// "Forest" of trees of "properties and types" - serves to build and store such trees
/// </para>
/// </summary>
public class TypesForest
{
    
    private const string Slash = "/";
    private const string Dot = ".";
    private const string _nullableAttributeName = "NullableAttribute";

    private static readonly PropertyNodeComparer _propertyNodeComparer = new();

    private Dictionary<Type, TypeNode> _typeTrees { get; init; } = new();

    /// <summary>
    /// <para xml:lang="ru">
    /// <see cref="DtoServiceProvider"/>, которым инициализирован данный лес
    /// </para>
    /// <para xml:lang="en">
    /// <see cref="DtoServiceProvider"/> with which this forest is initialized
    /// </para>
    /// </summary>
    public DtoServiceProvider ServiceProvider { get; init; }

    /// <summary>
    /// <para xml:lang="ru">
    /// Инициализирует <see cref="DtoServiceProvider"/>
    /// </para>
    /// <para xml:lang="en">
    /// Initializes <see cref="DtoServiceProvider"/>
    /// </para>
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <exception cref="ArgumentNullException">
    /// <para xml:lang="ru">
    /// Плохо, если <see cref="DtoServiceProvider"/> является <code>null</code>,
    /// так как ничего не получится сделать
    /// </para>
    /// <para xml:lang="en">
    /// Bad if <see cref="DtoServiceProvider"/> is <code>null</code>,
    /// since nothing can be done
    /// </para>
    /// </exception>
    public TypesForest(DtoServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    /// <summary>
    /// <para xml:lang="ru">
    /// Возвращает корневой узел дерева, соответствующего указанному типу.
    /// Если такого дерева в лесу нет, сажает его, выращивает и возвращает
    /// </para>
    /// <para xml:lang="en">
    /// Returns the root node of the tree corresponding to the specified type.
    /// If there is no such tree in the forest, plant it, grow it and return it
    /// </para>
    /// </summary>
    /// <param name="type">
    /// <para xml:lang="ru">
    /// Запрашиваемый тип
    /// </para>
    /// <para xml:lang="en">
    /// Requested type
    /// </para>
    /// </param>
    /// <returns></returns>
    public TypeNode GetTypeNode(Type type)
    {
        if (!_typeTrees.ContainsKey(type))
        {
            PlantTypeTree(type);
        }
        return _typeTrees[type];
    }

    /// <summary>
    /// <para xml:lang="ru">
    /// Ищет в лесу <see cref="PropertyNode"/>
    /// </para>
    /// <para xml:lang="en">
    /// Searches the forest for <see cref="PropertyNode"/>
    /// </para>
    /// </summary>
    /// <param name="typeNode">
    /// <para xml:lang="ru">
    /// <see cref="TypeNode"/>, содержащий искомое свойство
    /// </para>
    /// <para xml:lang="en">
    /// <see cref="TypeNode"/> containing the desired property
    /// </para>
    /// </param>
    /// <param name="propertyName">
    /// <para xml:lang="ru">
    /// Имя искомого свойства
    /// </para>
    /// <para xml:lang="en">
    /// Name of the searched property
    /// </para>
    /// </param>
    /// <returns></returns>
    public PropertyNode? FindPropertyNode(TypeNode typeNode, string propertyName)
    {
        return typeNode.ChildNodes?.Find(propertyNode => propertyNode.Name == propertyName);
    }

    /// <summary>
    /// <para xml:lang="ru">
    /// Копирует полное дерево объекта в целевой объект по шаблону применяемого интерфейса
    /// </para>
    /// <para xml:lang="en">
    /// Copies the complete object tree to the target object according to the template of the applied interface
    /// </para>
    /// </summary>
    /// <param name="sourceType">
    /// <para xml:lang="ru">
    /// Применяемый интерфейс
    /// </para>
    /// <para xml:lang="en">
    /// Applied interface
    /// </para>
    /// </param>
    /// <param name="source">
    /// <para xml:lang="ru">
    /// Исходный объект
    /// </para>
    /// <para xml:lang="en">
    /// Source object
    /// </para>
    /// </param>
    /// <param name="target">
    /// <para xml:lang="ru">
    /// Целевой объект
    /// </para>
    /// <para xml:lang="en">
    /// Target object
    /// </para>
    /// </param>
    public void Copy(Type sourceType, object source, object target)
    {
        TypeNode typeNode = GetTypeNode(sourceType);
        if(typeNode.ChildNodes is { })
        {
            foreach (PropertyNode propertyNode in typeNode.ChildNodes)
            {
                object? sourceValue = propertyNode.PropertyInfo?.GetValue(source);
                if (sourceValue is null)
                {
                    propertyNode.PropertyInfo?.SetValue(target, null);
                }
                else
                {
                    if (propertyNode.TypeNode.ChildNodes is { } children)
                    {
                        object? targetValue = propertyNode.PropertyInfo?.GetValue(target);
                        if (targetValue is null)
                        {
                            targetValue = ServiceProvider.GetRequiredService(propertyNode.TypeNode.Type);
                            propertyNode.PropertyInfo?.SetValue(target, targetValue);
                        }
                        Copy(propertyNode.TypeNode.Type, sourceValue, targetValue);
                    }
                    else
                    {
                        propertyNode.PropertyInfo?.SetValue(target, sourceValue);
                    }
                }

            }
        }
    }

    private bool PlantTypeTree(Type type)
    {
        if (!ServiceProvider.IsRegistered(type))
        {
            throw new ArgumentException($"{type} is not registered.");
        }
        bool result = !_typeTrees.ContainsKey(type);
        if (result)
        {
            lock (type)
            {
                result = !_typeTrees.ContainsKey(type);
                if (result)
                {
                    _typeTrees[type] = new TypeNode { Type = type, ChildNodes = new List<PropertyNode>() };
                    List<Type> antiLoop = new() { type };
                    ConfigureTypeNode(_typeTrees[type], antiLoop);

                    _typeTrees[type].ValueRequests = new List<ValueRequest>();
                    CollectValueRequests(_typeTrees[type], _typeTrees[type].ValueRequests!);
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
        foreach(PropertyNode propertyNode in typeNode.ChildNodes!)
        {
            int pathLength = path.Length;
            path.Append(Slash).Append(propertyNode.PropertyInfo!.Name);
            ValueRequest request = new ValueRequest
            {
                Path = path.ToString(),
                PropertyNode = propertyNode
            };
            requests.Add(request);
            if (!propertyNode.IsLeaf)
            {
                foreach(ValueRequest req in propertyNode.TypeNode.ValueRequests!)
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
                PropertyInfo? sourceProperty = properties.Where(
                    v => v.Name == propertyInfo.Name && v.PropertyType == propertyInfo.PropertyType
                ).FirstOrDefault();
                if (sourceProperty is PropertyInfo)
                {
                    actualProperty = propertyInfo;
                }
            }
            if (actualProperty is PropertyInfo && actualProperty.CanWrite)
            {
                bool foundTypeNode = _typeTrees.ContainsKey(propertyInfo.PropertyType);
                var newPropertyNode = new PropertyNode
                {
                    Name = propertyInfo.Name.Contains(Dot) 
                        ? propertyInfo.Name.Substring(propertyInfo.Name.LastIndexOf(Dot) + 1) 
                        : propertyInfo.Name,
                    PropertyInfo = actualProperty,
                    TypeNode = ServiceProvider.IsRegistered(propertyInfo.PropertyType) 
                        ? GetTypeNode(propertyInfo.PropertyType) 
                        : new TypeNode { Type = propertyInfo.PropertyType, ActualType = propertyInfo.PropertyType },
                    IsNullable = (propertyInfo.PropertyType.IsValueType && Nullable.GetUnderlyingType(propertyInfo.PropertyType) is Type)
                        || propertyInfo.GetCustomAttributes().Any(a => a.GetType().Name.Contains(_nullableAttributeName))
                };
                typeNode.ChildNodes!.Add(newPropertyNode);
            }
        }
        typeNode.ChildNodes!.Sort(_propertyNodeComparer);
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
            currentType = currentType.BaseType!;
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

}
