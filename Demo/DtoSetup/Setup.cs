using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Dto = Net.Leksi.Dto;

namespace DtoKit.Demo;

public class Setup
{
    private static ConditionalWeakTable<object, object> _refs = new();
    private static object _obj = new();

    public static void Configure(IServiceCollection services)
    {
        Dto.DtoKit.Install(services, provider => {
            provider.AddTransient<ILine>(op => Setup.Create<Line>());
            provider.AddTransient<IPort>(op => Setup.Create<Port>());
            provider.AddTransient<IVessel>(op => Create<Vessel>());
            provider.AddTransient<IRoute>(op => Create<Route>());
            provider.AddTransient<IShipCall>(op => Create<ShipCall>());
            provider.AddTransient<IVesselForShipCallList>(op => Create<Vessel>());
            provider.AddTransient<IRouteForShipCallList>(op => Create<Route>());
            provider.AddTransient<IShipCallForList>(op => Create<ShipCall>());
            provider.AddTransient<IShipCallAdditionInfo>(op => Create<ShipCall>());
        });
    }

    private static T Create<T>() where T : new()
    {
        T result = new();
        _refs.AddOrUpdate(result, _obj);
        return result;
    }
}
