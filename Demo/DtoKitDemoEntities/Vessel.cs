using DtoKitDemoContract;
using Net.Leksi.Dto;

namespace DtoKitDemoEntities;

public class Vessel: IVessel, IVesselForShipCallList
{
    [Key]
    public string ID_VESSEL { get; set; }

    public Port Port { get; set; }
    public double Length { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double Brutto { get; set; }

    public double Netto { get; set; }

    public string CallSign { get; set; }

    public string Name { get; set; }

    IPort IVessel.Port => Port;
}
