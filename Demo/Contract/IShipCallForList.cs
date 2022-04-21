namespace DtoKit.Demo;

public interface IShipCallForList
{
    IRouteForShipCallList Route { get; }

    string Voyage { get; }

    IPort Port { get; }

    public DateTime? Arrival { get; }

    public DateTime? Departure { get; }

    public string AdditionalInfo { get; }
}
