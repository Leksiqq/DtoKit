﻿using System;

namespace TestProject1.Dto1;

public interface IShipCall
{
    IRoute Route { get; }
    string Voyage { get;}
    string VoyageAlt { get; }
    ILocation Location { get; }
    DateTime ScheduledArrival { get; }
    DateTime ActualArrival { get; }
    DateTime ScheduledDeparture { get; }
    DateTime ActualDeparture { get; }
    ShipCallCondition Condition { get; }
}