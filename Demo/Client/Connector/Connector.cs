using System.Collections.ObjectModel;

namespace DtoKit.Demo;

public class Connector : IConnector
{

    //private HttpClient _client = new HttpClient();

    //public Task GetShipCalls(ShipCallsFilter filter, [NotParameter] ObservableCollection<IShipCallForList> list)
    //{
    //    MethodBase? methodBase = MethodBase.GetCurrentMethod();

    //    Queue<Type> queue = new();
    //    List<Type> seen = new();
    //    queue.Enqueue(methodBase.DeclaringType);
    //    while (queue.Count > 0)
    //    {
    //        Type type = queue.Dequeue();
    //        if (!seen.Contains(type))
    //        {
    //            seen.Add(type);
    //            foreach (Type intrf in type.GetInterfaces())
    //            {
    //                queue.Enqueue(intrf);
    //            }
    //            if (type.BaseType is { })
    //            {
    //                queue.Enqueue(type.BaseType);
    //            }
    //            if (type.IsInterface)
    //            {
    //                MethodInfo? mi = type.GetMethod(methodBase.Name, methodBase.GetParameters().Select(p => p.ParameterType).ToArray());
    //                if(mi != null)
    //                {

    //                    Console.WriteLine(mi);
    //                    Console.WriteLine(string.Join(", ", mi.GetCustomAttributes()));
    //                }
    //            }
    //        }
    //    }


    //    return Task.CompletedTask;
    //}


    //private async Task<HttpResponseMessage> Send(MethodBase methodBase, object?[] args)
    //{
    //    HttpRequestMessage request = new HttpRequestMessage();
    //    return await _client.SendAsync(request);
    //}
    public Task GetShipCalls(ShipCallsFilter filter, ObservableCollection<IShipCallForList> list)
    {
        throw new NotImplementedException();
    }
}
