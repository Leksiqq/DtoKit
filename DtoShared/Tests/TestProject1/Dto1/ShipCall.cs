﻿using Net.Leksi.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Dto1;

public class ShipCall : IShipCall
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

    [AliasFor(nameof(RouteImpl))]
    IRoute IShipCall.Route => RouteImpl;
    ILocation IShipCall.Location => Location;
}
