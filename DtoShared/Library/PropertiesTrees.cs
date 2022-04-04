using System.Reflection;

namespace Net.Leksi.Dto;

public class PropertiesTrees
{
    //private const string Slash = "/";

    //private int _propertyNodeIdGen = 0;
    //private List<Type> _antiLoop = new();
    //public Dictionary<Type, PropertyNode> SubTrees { get; init; } = new();
    //public DtoServiceProvider ServiceProvider { get; init; }

    //public PropertiesTrees(DtoServiceProvider serviceProvider)
    //{
    //    ServiceProvider = serviceProvider;
    //}

    //public void AddTypeIfAbsent(Type type)
    //{
    //    if (!ServiceProvider.IsRegistered(type))
    //    {
    //        throw new ArgumentException($"{nameof(type)} is not registered");
    //    }
    //    if (!SubTrees.ContainsKey(type))
    //    {
    //        SubTrees[type] = new PropertyNode { ChildNodes = new List<PropertyNode>() };
    //        _antiLoop.Clear();
    //        _antiLoop.Add(type);
    //        FillChildren(SubTrees[type].ChildNodes, type);
    //    }
    //}

    //private void FillChildren(List<PropertyNode> childNodes, Type type)
    //{
    //    foreach (PropertyInfo propertyInfo in type.GetProperties())
    //    {
    //        if (!SubTrees.ContainsKey(propertyInfo.PropertyType))
    //        {
    //            if (_antiLoop.Contains(propertyInfo.PropertyType))
    //            {
    //                throw new Exception($"Loop detected: {string.Join(Slash, _antiLoop.Select(t => t.Name))}/{propertyInfo.PropertyType.Name}");
    //            }
    //            _antiLoop.Add(propertyInfo.PropertyType);
    //            PropertyNode node = new PropertyNode { PropertyInfo = propertyInfo, Id = ++_propertyNodeIdGen };
    //            childNodes.Add(node);
    //            if (ServiceProvider.IsRegistered(propertyInfo.PropertyType))
    //            {
    //                if (!SubTrees.ContainsKey(propertyInfo.PropertyType))
    //                {
    //                    SubTrees.Add(propertyInfo.PropertyType, node);
    //                }
    //                node.ChildNodes = new List<PropertyNode>();
    //                FillChildren(node.ChildNodes, propertyInfo.PropertyType);
    //            }
    //            _antiLoop.RemoveAt(_antiLoop.Count - 1);
    //        } else
    //        {
    //            if()
    //            childNodes.Add(SubTrees[propertyInfo.PropertyType]);
    //        }
    //    }
    //}
}
