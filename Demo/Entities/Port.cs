using Net.Leksi.Dto;

namespace DtoKit.Demo;

public class Port : IPort
{
    [Key]
    public string ID_PORT { get; set; }

    public string Name { get; set; }
}
