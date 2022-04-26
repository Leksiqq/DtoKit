using Net.Leksi.RestContract;
using System.Collections.ObjectModel;

namespace DtoKit.Demo;

public interface IConnector
{
    [RoutePath("/shipCalls/{filter}/{count:int}")]
    [HttpMethodGet]
    [Authorization]
    Task GetShipCalls(int count, ShipCallsFilter filter, ObservableCollection<IShipCallForList> list);
}
