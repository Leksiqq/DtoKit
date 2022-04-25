namespace DtoKit.Demo;

public class ShipCallsFilter
{
    public string Voyage { get; set; } = null;
    public string VesselName { get; set; } = null;
    public DateTime? From { get; set; } = null;
    public DateTime? To { get; set; } = null;
    public string PortName { get; set; } = null;
    public ILine Line { get; set; } = null;
}
