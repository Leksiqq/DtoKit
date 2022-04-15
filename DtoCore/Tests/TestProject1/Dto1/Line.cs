
using Net.Leksi.Dto;

namespace TestProject1.Dto1;

public class Line : ILine
{
    [Key]
    public string ID_LINE { get; set; }

    public string ShortName => ID_LINE;
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        return (obj is Line line) && ID_LINE == line.ID_LINE;
    }

    public override int GetHashCode()
    {
        return ID_LINE.GetHashCode();
    }
}

