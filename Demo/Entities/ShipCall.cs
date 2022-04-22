using Net.Leksi.Dto;

namespace DtoKit.Demo;

public class ShipCall: IShipCall, IShipCallForList, IShipCallAdditionInfo
{
    [Key]
    public int ID_SHIPCALL { get; set; }
    [Key]
    public string ID_LINE { get; set; }

    public Route Route { get; set; }

    public string Voyage { get; set; }

    public Port Port { get; set; }

    public DateTime? Arrival { get; set; }

    public DateTime? Departure { get; set; }

    public string AdditionalInfo { get; set; }

    public ShipCall? PrevCall { get; set; }

    IRoute IShipCall.Route => Route;

    IRouteForShipCallList IShipCallForList.Route => Route;

    IPort IShipCall.Port => Port;

    IPort IShipCallForList.Port => Port;

    IShipCall? IShipCall.PrevCall => PrevCall;
}
