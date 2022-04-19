using DtoKitDemoContract;
using Net.Leksi.Dto;

namespace DtoKitDemoEntities;

public class Route: IRoute
{
    [Key]
    public int ID_ROUTE { get; set; }
    [Key]
    public string ID_LINE { get; set; }
    public Line Line { get; set; }

    public Vessel Vessel { get; set; }

    ILine IRoute.Line => Line;

    IVessel IRoute.Vessel => Vessel;
}
