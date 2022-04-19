namespace DtoKitDemoContract;

public interface IRouteForShipCallList
{
    public ILine Line { get; }
    public IVesselForShipCallList Vessel { get; }
}
