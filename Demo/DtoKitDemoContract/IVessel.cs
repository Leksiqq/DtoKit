namespace DtoKitDemoContract;

public interface IVessel
{
    IPort Port { get; }
    double Length { get; }

    double Width { get; }

    double Height { get; }

    double Brutto { get; }

    double Netto { get; }

    string CallSign { get; }

    string Name { get; }
}
