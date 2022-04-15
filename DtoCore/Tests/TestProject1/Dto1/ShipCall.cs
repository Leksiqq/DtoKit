using Net.Leksi.Dto;
using System;

namespace TestProject1.Dto1;

public class ShipCall : IShipCall, IShipCallForListing, IDepartureShipCall, IArrivalShipCall
{
    [Key]
    public string ID_LINE { get; set; }

    [Key]
    public int ID_ROUTE { get; set; }

    public Route RouteImpl { get; set; }

    public string Voyage { get; set; }

    public string VoyageAlt { get; set; }

    public Location Location { get; set; }

    public DateTime ScheduledArrival { get; set; }

    public DateTime ActualArrival { get; set; }

    public DateTime ScheduledDeparture { get; set; }

    public DateTime ActualDeparture { get; set; }

    public ShipCallCondition Condition { get; set; }

    public string AdditionalInfo { get; set; } = "N/A";

    [ActualProperty("RouteImpl")]
    IRoute IShipCall.Route => RouteImpl;

    [ActualProperty("RouteImpl")]
    IRouteShort IShipCallForListing.Route => RouteImpl;

    [ActualProperty("RouteImpl")]
    IRouteShort IDepartureShipCall.Route => RouteImpl;

    ILocation IShipCall.Location => Location;

    ILocation IShipCallForListing.Location => Location;

    ILocation IDepartureShipCall.Location => Location;

    ILocation IArrivalShipCall.Location => Location;
}
