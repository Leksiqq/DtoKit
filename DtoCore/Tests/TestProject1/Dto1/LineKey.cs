using Net.Leksi.Dto;

namespace DtoTestProject.Dto1;

public class LineKey: ILineKey
{
    [Key]
    public string ID_LINE { get; set; }

    public override string ToString()
    {
        return ID_LINE;
    }
}
