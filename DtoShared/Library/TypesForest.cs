﻿using System.Reflection;

namespace Net.Leksi.Dto;

public class TypesForest
{
    private class KeyNameComparer : IComparer<PropertyNode>
    {
        public int Compare(PropertyNode? x, PropertyNode? y)
        {
            if (x == y)
            {
                return 0;
            }
            if (x is null)
            {
                return -1;
            }
            if (y is null)
            {
                return 1;
            }
            return string.Compare(x.PropertyName, y.PropertyName);
        }
    }

    private const string Slash = "/";
    private const string Dot = ".";
    private const string _nullableAttributeName = "NullableAttribute";

    private static readonly KeyNameComparer _keyNameComparer = new();

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

    public PropertyNode? GetPropertyNode(TypeNode typeNode, string propertyName)
    {
        if (typeNode?.ChildNodes is { } children)
        {
            return children.Find(propertyNode => propertyNode.PropertyName == propertyName);
        }
        return null;
    }

    public bool PlantTypeTree(Type type)
    {
        if (!ServiceProvider.IsRegistered(type))
        {
            throw new ArgumentException($"{nameof(type)} is not registered.");
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
                    FillChildren(TypeTrees[type].ChildNodes!, type, antiLoop);
                }
            }
        }
        return result;
    }

    public bool ConfirmTypeNode(TypeNode typeNode, Type actualType)
    {
        bool result = !typeNode.IsConfirmed;
        if (result)
        {
            lock (typeNode)
            {
                result = !typeNode.IsConfirmed;
                if (result)
                {
                    if(typeNode.ChildNodes is { } children)
                    {
                        foreach (PropertyInfo propertyInfo in actualType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (propertyInfo.GetCustomAttribute<KeyAttribute>() is { })
                            {
                                if (propertyInfo.CanWrite)
                                {
                                    var newPropertyNode = new PropertyNode
                                    {
                                        PropertyName = propertyInfo.Name,
                                        PropertyInfo = propertyInfo,
                                        TypeNode = new TypeNode { Type = propertyInfo.PropertyType }
                                    };
                                    if (typeNode.KeysCount == 0)
                                    {
                                        children.Insert(0, newPropertyNode);
                                    }
                                    else
                                    {
                                        int pos = -1 - typeNode.ChildNodes.GetRange(0, typeNode.KeysCount).BinarySearch(newPropertyNode, _keyNameComparer);
                                        children.Insert(pos, newPropertyNode);
                                    }
                                    typeNode.KeysCount++;
                                }
                            }
                            else if (propertyInfo.Name.Contains(Dot))
                            {
                                if (propertyInfo.Name.StartsWith(typeNode.Type.FullName!)
                                    && propertyInfo.Name.LastIndexOf(Dot) == typeNode.Type.FullName!.Length
                                )
                                {
                                    PropertyInfo? actualProperty = null;
                                    if (propertyInfo.GetCustomAttribute<ActualPropertyAttribute>() is { } actualPropertyAttr)
                                    {
                                        actualProperty = actualType.GetProperty(actualPropertyAttr.PropertyName);
                                    }
                                    else
                                    {
                                        actualProperty = actualType.GetProperty(propertyInfo.Name.Substring(propertyInfo.Name.LastIndexOf(Dot) + 1));
                                    }
                                    if (actualProperty is { } && actualProperty.CanWrite)
                                    {
                                        if (typeNode.ChildNodes.Where(ch => ch.PropertyName.Equals(propertyInfo.Name.Substring(propertyInfo.Name.LastIndexOf(Dot) + 1)))
                                            .FirstOrDefault() is PropertyNode node)
                                        {
                                            node.PropertyInfo = actualProperty;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (children.Where(ch => ch.PropertyName.Equals(propertyInfo.Name))
                                    .FirstOrDefault() is PropertyNode node)
                                {
                                    node.PropertyInfo = propertyInfo;
                                }
                            }
                        }
                        children.RemoveAll(ch => (ch.PropertyInfo is null || !ch.PropertyInfo.CanWrite));
                    }
                    typeNode.IsConfirmed = true;
                }
            }
        }
        return result;
    }

    private void FillChildren(List<PropertyNode> childNodes, Type type, List<Type> antiLoop)
    {
        foreach (PropertyInfo propertyInfo in type.GetProperties())
        {
            if (antiLoop.Contains(propertyInfo.PropertyType))
            {
                throw new Exception($"Loop detected: {string.Join(Slash, antiLoop.Select(t => t.Name))}/{propertyInfo.PropertyType.Name}");
            }
            TypeNode typeNode;
            bool foundTypeNode = TypeTrees.ContainsKey(propertyInfo.PropertyType);
            if(!foundTypeNode)
            {
                typeNode = new TypeNode { Type = propertyInfo.PropertyType };
                TypeTrees[propertyInfo.PropertyType] = typeNode;
            }
            else
            {
                typeNode = TypeTrees[propertyInfo.PropertyType];
            }
            PropertyNode node = new PropertyNode
            {
                PropertyName = propertyInfo.Name,
                TypeNode = typeNode,
                IsNullable = propertyInfo.GetCustomAttributes().Any(a => a.GetType().Name.Contains(_nullableAttributeName))
            };
            childNodes.Add(node);
            if (!foundTypeNode)
            {
                antiLoop.Add(propertyInfo.PropertyType);
                if (ServiceProvider.IsRegistered(propertyInfo.PropertyType))
                {
                    node.TypeNode.ChildNodes = new List<PropertyNode>();
                    FillChildren(node.TypeNode.ChildNodes, propertyInfo.PropertyType, antiLoop);
                }
                antiLoop.RemoveAt(antiLoop.Count - 1);
            }
        }
    }
}
