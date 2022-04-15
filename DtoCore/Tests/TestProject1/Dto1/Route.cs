using Net.Leksi.Dto;

namespace TestProject1.Dto1;

public class Route : IRoute, IRouteShort
{
    [Key]
    public int ID_RHEAD { get; set; }
    [Key]
    public string ID_LINE { get; set; }
    public Line Line { get; set; }

    public Vessel Vessel { get; set; }

    ILine IRoute.Line => Line;

    ILine IRouteShort.Line => Line;

    IVessel IRoute.Vessel => Vessel;

    IVesselShort IRouteShort.Vessel => Vessel;
}
