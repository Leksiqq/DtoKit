using System;

namespace DtoTestProject.Dto1;

public interface IArrivalShipCall
{
    ILocation Location { get; }
    DateTime ActualArrival { get; }
}
