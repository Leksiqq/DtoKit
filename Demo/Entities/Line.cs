using Net.Leksi.Dto;

namespace DtoKit.Demo;

public class Line: ILine
{
    [Key]
    public string ID_LINE { get; set; }

    public string ShortName => ID_LINE;
    public string Name { get; set; }

}

