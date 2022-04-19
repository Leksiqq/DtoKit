using DtoKitDemoContract;
using Net.Leksi.Dto;

namespace DtoKitDemoEntities;

public class Port : IPort
{
    [Key]
    public string ID_PORT { get; set; }

    public string Name { get; set; }
}
