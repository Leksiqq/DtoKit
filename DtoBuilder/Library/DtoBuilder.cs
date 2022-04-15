using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

namespace Net.Leksi.Dto;

public class DtoBuilder
{
    public event ValueRequestEventHandler? ValueRequest;

    private const string IDontKnowWhy = "Какая-то неведомая фигня";
    private const string SetUp = nameof(SetUp);
    private const string ShutDown = nameof(ShutDown);
    private const string Before = nameof(Before);
    private const string After = nameof(After);
    private const string Slash = "/";
    private const string Under = "_";

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

    public T? Build<T>(object helper) where T : class
    {
        if (helper is null)
        {
            throw new ArgumentNullException(nameof(helper));
        }
        if (!_helperHandlers.ContainsKey(helper))
        {
            SaveHelper(helper);
        }
        if (_helperMethods[helper.GetType()].TryGetValue(SetUp, out MethodInfo? setup))
        {
            setup?.Invoke(helper, null);
        }
        ValueRequest += _helperHandlers[helper];
        T? result = Build<T>();
        ValueRequest -= _helperHandlers[helper];
        if (_helperMethods[helper.GetType()].TryGetValue(ShutDown, out MethodInfo? shutdown))
        {
            shutdown?.Invoke(helper, null);
        }
        return result;
    }

    public T? Build<T>() where T : class
    {
        T? result = null;
        PropertyNode root = new() { TypeNode = _typesForest.GetTypeNode(typeof(T)) };
        Stack<object?> targets = new();
        Stack<TypeNode?> typeNodes = new();
        ValueRequestEventArgs eventArgs = new();
        List<string> ignoredPaths = new();
        int skipTo = -1;
        int childPosition = 0;
        object[]? key = null;
        foreach (ValueRequest request in root.TypeNode.ValueRequests!)
        {

            if (skipTo < 0 || targets.Count <= skipTo)
            {
                skipTo = -1;
                if (!request.PropertyNode.IsLeaf)
                {
                    childPosition = 0;
                    object? target = null;
                    if (request.PropertyNode.TypeNode.Type == typeof(T))
                    {
                        target = Target;
                    }
                    if (target is null)
                    {
                        if (_probeObjects.TryGetValue(request.PropertyNode.TypeNode.Type, out object? probe))
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
                        request.PropertyNode.PropertyInfo!.SetValue(targets.Peek(), target);
                    }
                    targets.Push(target);
                    typeNodes.Push(request.PropertyNode.TypeNode);
                    eventArgs.Init(request.PropertyNode, target, request.Path);
                    ValueRequest?.Invoke(eventArgs);
                    if (eventArgs.IsReset)
                    {
                        targets.Pop();
                        if (targets.Count > 0)
                        {
                            request.PropertyNode.PropertyInfo!.SetValue(targets.Peek(), eventArgs.Result);
                        }
                        else
                        {
                            result = (T?)eventArgs.Result;
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
                    eventArgs.Init(request.PropertyNode, targets.Peek()!, request.Path);
                    ValueRequest?.Invoke(eventArgs);
                    if (!eventArgs.IsCommited)
                    {
                        ignoredPaths.Add($"{eventArgs.NominalType} {eventArgs.Path}");
                    }
                    if (childPosition == typeNodes!.Peek()!.KeysCount)
                    {
                        key = request.PropertyNode.TypeNode.GetKey(targets.Peek()!);
                        if (_objectCache.TryGet(request.PropertyNode.TypeNode.Type, key, out object cachedObject))
                        {
                            if (!_probeObjects.TryAdd(request.PropertyNode.TypeNode.Type, targets.Peek()!))
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
            else
            {
                if (!request.PropertyNode.IsLeaf)
                {
                    targets.Push(null);
                    typeNodes.Push(request.PropertyNode.TypeNode);
                }
            }
            for (int i = request.PopsCount; i < 0; i++)
            {
                object? target = targets.Pop();
                if (key is { })
                {
                    _objectCache.Add(typeNodes.Peek()!.Type, key, target!);
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
        void eh(ValueRequestEventArgs args)
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
            //args.Value = ...;
            //args.Value = null;
            //args.Commit();");
                    break;
                case ValueRequestKind.NotNullableNode:
                    sb.Append(@"
            //args.Value = ...;
            //args.Commit();");
                    break;
            }
            sb.Append(@"
            break;");
        }
        ValueRequest += eh;
        Build<T>();
        ValueRequest -= eh;
        sb.AppendLine(@"
    }
}");
        return sb.ToString();
    }

    public string GenerateHelperSkeleton<T>() where T : class
    {
        StringBuilder sb = new();
        sb.Append(@"
using Net.Leksi.Dto;

public class HelperSkeleton
{
    [Setup]
    public void Setup()
    {
        throw new NotImplementedException();
    }

    [Shutdown]
    public void Shutdown()
    {
        throw new NotImplementedException();
    }

    [Before]
    public void Before(string path, object value)
    {
        throw new NotImplementedException();
    }

    [After]
    public void After(string path, object value)
    {
        throw new NotImplementedException();
    }
");
        void eh(ValueRequestEventArgs args)
        {
            switch (args.Kind)
            {
                case ValueRequestKind.Terminal:
                    sb.Append($@"
    [Path(""{args.Path}"", typeof(TerminalSetter))]
    public object Set_{args.Path.Substring(1).Replace(Slash, Under)}(object value)
    {{
        throw new NotImplementedException();
    }}
");
                    args.Commit();
                    break;
                case ValueRequestKind.NullableNode or ValueRequestKind.NotNullableNode:
                    sb.Append($@"
    [Path(""{args.Path}"", typeof(NodeSetter))]
    public object Set_{args.Path.Substring(1).Replace(Slash, Under)}(object value, bool isNullable, ref bool isCommited)
    {{
        throw new NotImplementedException();
    }}
");
                    break;
            }
        }
        ValueRequest += eh;
        Build<T>();
        ValueRequest -= eh;
        sb.Append(@"
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
        _helperHandlers[helper] = args =>
        {
            if (methods.TryGetValue(args.Path, out MethodInfo? method))
            {
                try
                {
                    if (_helperMethods[helper.GetType()].TryGetValue(Before, out MethodInfo? before))
                    {
                        object?[] parameters = new object[] { args.Path, args.Value };
                        before?.Invoke(helper, parameters);
                    }
                    if (args.Kind is ValueRequestKind.Terminal)
                    {
                        object?[] parameters = new object[1];
                        parameters[0] = args.Value;
                        args.Value = method.Invoke(helper, parameters);
                        args.Commit();
                    }
                    else
                    {
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
                    if (_helperMethods[helper.GetType()].TryGetValue(ShutDown, out MethodInfo? after))
                    {
                        object?[] parameters = new object[] { args.Path, args.Value };
                        after?.Invoke(helper, parameters);
                    }
                }
                catch (TargetParameterCountException tgpex)
                {
                    throw new AggregateException(new Exception[] { tgpex, new Exception(@$"{method} parameters count mismatch
possibly reasons:
1) {args.NominalType} is not registered
2) wrong delegate type used") });
                }
                catch (TargetInvocationException tiex)
                {
                    if (tiex.InnerException is NotImplementedException) { }
                    else
                    {
                        throw;
                    }
                }
            }
        };
    }

    private void SaveHelperMethods(Type type)
    {
        Type currentType = type;
        _helperMethods[type] = new Dictionary<string, MethodInfo>();
        Dictionary<string, bool> repeatedMethods = new();
        List<string> delegateMismatches = new();

        while (currentType != typeof(object))
        {
            foreach (MethodInfo methodInfo in currentType!.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (methodInfo.GetCustomAttribute<SetupAttribute>() is { })
                {
                    if (methodInfo.ReturnType != typeof(void) || methodInfo.GetParameters().Length > 0)
                    {
                        delegateMismatches.Add($"{methodInfo} is not a setup void delegate()");
                    }
                    if (!_helperMethods[type].TryAdd(SetUp, methodInfo))
                    {
                        repeatedMethods[SetUp] = true;
                    }
                }
                else if (methodInfo.GetCustomAttribute<ShutdownAttribute>() is { })
                {
                    if (methodInfo.ReturnType != typeof(void) || methodInfo.GetParameters().Length > 0)
                    {
                        delegateMismatches.Add($"{methodInfo} is not a shutdown void delegate()");
                    }
                    if (!_helperMethods[type].TryAdd(ShutDown, methodInfo))
                    {
                        repeatedMethods[ShutDown] = true;
                    }
                }
                else if (methodInfo.GetCustomAttribute<BeforeAttribute>() is { })
                {
                    if (!IsMethodDelegate(methodInfo, typeof(BeforeOrAfterProcessor)))
                    {
                        delegateMismatches.Add($"{methodInfo} is not a delegate {typeof(BeforeOrAfterProcessor)}");
                    }
                    if (!_helperMethods[type].TryAdd(Before, methodInfo))
                    {
                        repeatedMethods[Before] = true;
                    }
                }
                else if (methodInfo.GetCustomAttribute<AfterAttribute>() is { })
                {
                    if (!IsMethodDelegate(methodInfo, typeof(BeforeOrAfterProcessor)))
                    {
                        delegateMismatches.Add($"{methodInfo} is not a delegate {typeof(BeforeOrAfterProcessor)}");
                    }
                    if (!_helperMethods[type].TryAdd(After, methodInfo))
                    {
                        repeatedMethods[After] = true;
                    }
                }
                else
                {
                    ParameterInfo[] parameters0 = methodInfo.GetParameters();
                    foreach (PathAttribute PathAttribute in methodInfo.GetCustomAttributes<PathAttribute>())
                    {
                        if (!IsMethodDelegate(methodInfo, PathAttribute.DelegateType))
                        {
                            delegateMismatches.Add($"{methodInfo} is not a delegate {PathAttribute.DelegateType}");
                        }
                        if (!_helperMethods[type].TryAdd(PathAttribute.Path, methodInfo))
                        {
                            repeatedMethods[PathAttribute.Path] = true;
                        }
                    }
                }
            }
            currentType = currentType.BaseType!;
        }
        if (delegateMismatches.Count > 0 || repeatedMethods.Count > 0)
        {
            _helperMethods[type] = null;
            List<Exception> exceptions = new();
            if (delegateMismatches.Count > 0)
            {
                exceptions.Add(new Exception(string.Join("\n", delegateMismatches)));
            }
            if (repeatedMethods.Count > 0)
            {
                exceptions.Add(new Exception(string.Join("\n", repeatedMethods.Select(v => $"{v} method is duplicated"))));
            }
            throw new AggregateException();
        }
    }

    private bool IsMethodDelegate(MethodInfo methodInfo, Type delegateType)
    {
        ParameterInfo[] parameters0 = methodInfo.GetParameters();
        MethodInfo method = delegateType.GetMethod("Invoke")!;
        ParameterInfo[] parameters = method.GetParameters();
        return (
            method.ReturnType == methodInfo.ReturnType
            && parameters0.Length == parameters.Length
            && parameters0.Zip(parameters)
                .All(v =>
                    v.First.ParameterType == v.Second.ParameterType
                    && v.First.IsIn == v.Second.IsIn
                    && v.First.IsOut == v.Second.IsOut));
    }
}
