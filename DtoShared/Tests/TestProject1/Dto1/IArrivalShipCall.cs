using System;

namespace TestProject1.Dto1;

public interface IArrivalShipCall
{
    ILocation Location { get; }
    DateTime ActualArrival { get; }
}
