namespace DtoKitDemoEntities;

public class ShipCall
{
    public int ID_SHIPCALL { get; set; }
    public string ID_LINE { get; set; }

    public Route Route { get; set; }

    public string Voyage { get; set; }

    public string Port { get; set; }

    public DateTime? Arrival { get; set; }

    public DateTime? Departure { get; set; }

    public string AdditionalInfo { get; set; }
}
