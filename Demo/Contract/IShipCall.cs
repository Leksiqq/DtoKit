namespace DtoKit.Demo;

public interface IShipCall
{
    IRoute Route { get; }

    string Voyage { get; }

    IPort Port { get; }

    DateTime? Arrival { get; }

    DateTime? Departure { get; }

    string AdditionalInfo { get; }

    IShipCall? PrevCall { get; }
}
