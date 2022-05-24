
using Net.Leksi.Dto;

namespace DtoTestProject.Dto1;

public class Line : ILine
{
    [Key]
    public ILineKey ID { get; set; }

    public string ShortName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public override string ToString()
    {
        return ID.ToString() + ", " + Name;
    }

}

