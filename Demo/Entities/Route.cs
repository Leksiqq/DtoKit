using Net.Leksi.Dto;

namespace DtoKit.Demo;

public class Route: IRoute, IRouteForShipCallList
{
    [Key]
    public int ID_ROUTE { get; set; }
    [Key]
    public string ID_LINE { get; set; }
    public Line Line { get; set; }

    public Vessel Vessel { get; set; }

    ILine IRoute.Line => Line;

    ILine IRouteForShipCallList.Line => Line;

    IVessel IRoute.Vessel => Vessel;

    IVesselForShipCallList IRouteForShipCallList.Vessel => Vessel;
}
