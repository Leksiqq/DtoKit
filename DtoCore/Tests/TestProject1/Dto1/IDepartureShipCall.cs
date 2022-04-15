using System;

namespace TestProject1.Dto1;

public interface IDepartureShipCall: IShipCallAdditionalInfo
{
    IRouteShort Route { get; }
    string Voyage { get;}
    string VoyageAlt { get; }
    ILocation Location { get; }
    DateTime ActualDeparture { get; }
    ShipCallCondition Condition { get; }
}
