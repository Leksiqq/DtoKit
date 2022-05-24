namespace DtoTestProject.Dto1;

public class Travel : ITravelForListing
{
    public ShipCall DepartureShipCall { get; set; }

    public ShipCall? ArrivalShipCall { get; set; }

    IDepartureShipCall ITravelForListing.DepartureShipCall => DepartureShipCall;

    IArrivalShipCall? ITravelForListing.ArrivalShipCall => ArrivalShipCall;
}
