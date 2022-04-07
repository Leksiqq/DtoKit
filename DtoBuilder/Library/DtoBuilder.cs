using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

namespace Net.Leksi.Dto;

public class DtoBuilder
{
    public event ValueRequestEventHandler ValueRequest;

    private const string IDontKnowWhy = "Какая-то неведомая фигня";
    private const string SetUp = nameof(SetUp);
    private const string ShutDown = nameof(ShutDown);

    private readonly TypesForest _typesForest;
    private readonly ObjectCache _objectCache = new();
    private readonly Dictionary<Type, object> _probeObjects = new();
    private readonly Dictionary<Type, Dictionary<string, MethodInfo>> _helperMethods = new();
    private readonly Dictionary<object, ValueRequestEventHandler> _helperHandlers = new();

    public object? Target { get; set; } = null;

    public DtoBuilder(TypesForest typesForest)
    {
        _typesForest = typesForest ?? throw new ArgumentNullException(nameof(typesForest));

    }

    public T Build<T>(object helper) where T : class
    {
        if(helper is null)
        {
            throw new ArgumentNullException();
        }
        if (!_helperHandlers.ContainsKey(helper))
        {
            SaveHelper(helper);
        }
        if(_helperMethods[helper.GetType()].TryGetValue(SetUp, out MethodInfo setup))
        {
            setup.Invoke(helper, null);
        }
        ValueRequest += _helperHandlers[helper];
        T result = Build<T>();
        ValueRequest -= _helperHandlers[helper];
        if (_helperMethods[helper.GetType()].TryGetValue(ShutDown, out MethodInfo shutdown))
        {
            shutdown.Invoke(helper, null);
        }
        return result;
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
                        if(targets.Count > 0)
                        {
                            request.PropertyNode.PropertyInfo.SetValue(targets.Peek(), eventArgs.Result);
                        }
                        else
                        {
                            result = (T)eventArgs.Result;
                        }
                        targets.Push(eventArgs.Result);
                        if (!_probeObjects.TryAdd(request.PropertyNode.TypeNode.Type, target))
                        {
                            throw new Exception(IDontKnowWhy);
                        }
                    }
                    if (eventArgs.IsCommited)
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

    private void SaveHelper(object helper)
    {
        if (!_helperMethods.ContainsKey(helper.GetType()))
        {
            SaveHelperMethods(helper.GetType());
        }
        Dictionary<string, MethodInfo> methods = _helperMethods[helper.GetType()];
        object?[] parameters3 = new object[3];
        _helperHandlers[helper] = args =>
        {
            if (methods.TryGetValue(args.Path, out MethodInfo method))
            {
                if (args.Kind is ValueRequestKind.Terminal)
                {
                    object?[] parameters = new object[1];
                    parameters[0] = args.Value;
                    args.Value = method.Invoke(helper, parameters);
                    args.Commit();
                }
                else {
                    object?[] parameters = new object[3];
                    parameters[0] = args.Value;
                    parameters[1] = args.Kind is ValueRequestKind.NullableNode;
                    parameters[2] = false;
                    args.Value = method.Invoke(helper, parameters);
                    if ((parameters[2] is bool isCommited && isCommited))
                    {
                        args.Commit();
                    }
                }
            }
        };
    }

    private void SaveHelperMethods(Type type)
    {
        Type currentType = type;
        _helperMethods[type] = new Dictionary<string, MethodInfo>();
        while (currentType != typeof(object))
        {
            foreach(MethodInfo methodInfo in currentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (methodInfo.GetCustomAttribute<SetupAttribute>() is SetupAttribute)
                {
                    if (methodInfo.ReturnType != typeof(void) || methodInfo.GetParameters().Length > 0)
                    {
                        throw new Exception($"{methodInfo} is not a setup void delegate()");
                    }
                    if (!_helperMethods[type].TryAdd(SetUp, methodInfo))
                    {
                        throw new Exception($"setup method is already declared");
                    }
                }
                else if (methodInfo.GetCustomAttribute<ShutdownAttribute>() is ShutdownAttribute)
                {
                    if (methodInfo.ReturnType != typeof(void) || methodInfo.GetParameters().Length > 0)
                    {
                        throw new Exception($"{methodInfo} is not a shutdown void delegate()");
                    }
                    if (!_helperMethods[type].TryAdd(ShutDown, methodInfo))
                    {
                        throw new Exception($"shutdown method is already declared");
                    }
                }
                else
                {
                    ParameterInfo[] parameters0 = methodInfo.GetParameters();
                    foreach (PathAttribute PathAttribute in methodInfo.GetCustomAttributes<PathAttribute>())
                    {
                        MethodInfo method = PathAttribute.DelegateType.GetMethod("Invoke");
                        ParameterInfo[] parameters = method.GetParameters();
                        if (
                            method.ReturnType != methodInfo.ReturnType
                            || parameters0.Length != parameters.Length
                            || !parameters0.Zip(parameters)
                                .All(v =>
                                    v.First.ParameterType == v.Second.ParameterType
                                    && v.First.IsIn == v.Second.IsIn
                                    && v.First.IsOut == v.Second.IsOut))
                        {
                            throw new Exception($"{methodInfo} is not a delegate {PathAttribute.DelegateType}");
                        }
                        if (!_helperMethods[type].TryAdd(PathAttribute.Path, methodInfo))
                        {
                            throw new Exception($"Path \"{PathAttribute.Path}\" method is already declared");
                        }
                    }
                }
            }
            currentType = currentType.BaseType;
        }
    }
}
