namespace TestProject1.Dto1;

public interface ITravelForListing
{
    IDepartureShipCall DepartureShipCall { get; }
    IArrivalShipCall? ArrivalShipCall { get; }
}
