using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Net.Leksi.Dto;

public class DtoBuilder
{

    public event ValueRequestEventHandler ValueRequest;

    private const string IDontKnowWhy = "Какая-то неведомая фигня";

    private readonly TypesForest _typesForest;
    private readonly ObjectCache _objectCache = new();
    private readonly Dictionary<Type, object> _probeObjects = new();

    public object? Target { get; set; } = null;

    public DtoBuilder(TypesForest typesForest)
    {
        _typesForest = typesForest ?? throw new ArgumentNullException(nameof(typesForest));

    }

    public T Build<T>() where T : class
    {
        T result = null;
        PropertyNode root = new PropertyNode { TypeNode = _typesForest.GetTypeNode(typeof(T)) };
        Stack<object?> targets = new();
        Stack<TypeNode?> typeNodes = new();
        ValueRequestEventArgs eventArgs = new();
        List<string> ignoredPaths = new();
        int skipTo = -1;
        int childPosition = 0;
        object[]? key = null;
        foreach (ValueRequest request in root.TypeNode.ValueRequests)
        {

            if(skipTo < 0 || targets.Count <= skipTo)
            {
                skipTo = -1;
                if (!request.PropertyNode.IsLeaf)
                {
                    childPosition = 0;
                    object target = null;
                    if (request.PropertyNode.TypeNode.Type == typeof(T))
                    {
                        target = Target;
                    }
                    if (target is null)
                    {
                        if (_probeObjects.TryGetValue(request.PropertyNode.TypeNode.Type, out object probe))
                        {
                            _probeObjects.Remove(request.PropertyNode.TypeNode.Type);
                            target = probe;
                        }
                        else
                        {
                            target = _typesForest.ServiceProvider.GetRequiredService(request.PropertyNode.TypeNode.Type);
                        }
                    }
                    if (targets.Count == 0)
                    {
                        result = (T)target;
                    }
                    else
                    {
                        request.PropertyNode.PropertyInfo.SetValue(targets.Peek(), target);
                    }
                    targets.Push(target);
                    typeNodes.Push(request.PropertyNode.TypeNode);
                    eventArgs.Init(request.PropertyNode, target, request.Path);
                    ValueRequest?.Invoke(eventArgs);
                    if (eventArgs.IsReset)
                    {
                        targets.Pop();
                        skipTo = targets.Count;
                        request.PropertyNode.PropertyInfo.SetValue(targets.Peek(), null);
                        targets.Push(null);
                        if (!_probeObjects.TryAdd(request.PropertyNode.TypeNode.Type, target))
                        {
                            throw new Exception(IDontKnowWhy);
                        }
                    }
                    else if (eventArgs.IsCommited)
                    {
                        skipTo = targets.Count - 1;
                    }
                }
                else
                {
                    childPosition++;
                    eventArgs.Init(request.PropertyNode, targets.Peek(), request.Path);
                    ValueRequest?.Invoke(eventArgs);
                    if (!eventArgs.IsCommited)
                    {
                        ignoredPaths.Add(eventArgs.Path);
                    }
                    if(childPosition == typeNodes.Peek().KeysCount)
                    {
                        key = request.PropertyNode.TypeNode.GetKey(targets.Peek());
                        if (_objectCache.TryGet(request.PropertyNode.TypeNode.Type, key, out object cachedObject))
                        {
                            if (!_probeObjects.TryAdd(request.PropertyNode.TypeNode.Type, targets.Peek()))
                            {
                                throw new Exception(IDontKnowWhy);
                            }
                            targets.Pop();
                            targets.Push(cachedObject);
                            key = null;
                            skipTo = targets.Count - 1;
                        }

                    }
                }
            }
            for (int i = request.PopsCount; i < 0; i++)
            {
                object? target = targets.Pop();
                if (key is { })
                {
                    _objectCache.Add(typeNodes.Peek().Type, key, target);
                    key = null;
                }
                typeNodes.Pop();
            }
        }
        if (ignoredPaths.Count > 0)
        {
            throw new InvalidOperationException($"path(s) ignored:\n{string.Join("\n", ignoredPaths)}");
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

}
