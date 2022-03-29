using Net.Leksi.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Dto1;

public class ShipCall : IShipCall
{
    [Key]
    public int ID_ROUTE { get; set; }
    [Key]
    public string ID_LINE { get; set; }
    public Route Route { get; set; }

    public string Voyage { get; set; }

    public string VoyageAlt { get; set; }

    public Location Location { get; set; }

    public DateTime ScheduledArrival { get; set; }

    public DateTime ActualArrival { get; set; }

    public DateTime ScheduledDeparture { get; set; }

    public DateTime ActualDeparture { get; set; }

    public ShipCallCondition Condition { get; set; }

    [Alias(nameof(Route))]
    IRoute IShipCall.Route => Route;
    [Alias(nameof(Location))]
    ILocation IShipCall.Location => Location;
}
