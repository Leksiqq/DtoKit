namespace DtoTestProject.Dto1;

public interface IVessel
{
    ILocation? Port { get; set; }
    public double Length { get; }

    public double Width { get; }

    public double Height { get; }

    public double Brutto { get; }

    public double Netto { get; }

    public double LineMeters { get; }

    public string Description { get; }

    public int RiffCount { get; }

    public bool IsOcean { get; }

    public string CallSign { get; }

    public string Name { get; }
}
