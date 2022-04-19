
using Net.Leksi.Dto;

namespace DtoKitDemoEntities;

public class Line
{
    [Key]
    public string ID_LINE { get; set; }

    public string ShortName => ID_LINE;
    public string Name { get; set; }

}

