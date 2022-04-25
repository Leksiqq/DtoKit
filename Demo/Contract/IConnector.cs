using Net.Leksi.Server.Contract;
using System.Collections.ObjectModel;

namespace DtoKit.Demo;

public interface IConnector
{
    [RoutePath("/shipCalls")]
    [HttpMethodGet]
    Task GetShipCalls(ShipCallsFilter filter, [NotParameter] ObservableCollection<IShipCallForList> list);
}
