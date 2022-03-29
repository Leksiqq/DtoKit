using Net.Leksi.Dto;

namespace TestProject1.Dto1;

public class Location : ILocation
{
    [Key]
    public string ID_LOCATION { get; set; }

    public LocationType Type {get; set;}

    public string ShortName { get; set; }

    public string Name { get; set; }

    public string Unlocode { get; set; }
}
