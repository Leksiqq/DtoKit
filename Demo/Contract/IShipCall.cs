namespace DtoKit.Demo;

public interface IShipCall
{
    IRoute Route { get; }

    string Voyage { get; }

    IPort Port { get; }

    public DateTime? Arrival { get; }

    public DateTime? Departure { get; }

    public string AdditionalInfo { get; }
}
