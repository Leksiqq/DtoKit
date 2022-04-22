using DtoKit.Demo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dto = Net.Leksi.Dto;

IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(serviceCollection =>
    {
        Dto.DtoKit.Install(
            serviceCollection,
            provider =>
            {
                provider.AddTransient<ILine, Line>();
                provider.AddTransient<IPort, Port>();
                provider.AddTransient<IVessel, Vessel>();
                provider.AddTransient<IRoute, Route>();
                provider.AddTransient<IShipCall, ShipCall>();
                provider.AddTransient<IVesselForShipCallList, Vessel>();
                provider.AddTransient<IRouteForShipCallList, Route>();
                provider.AddTransient<IShipCallForList, ShipCall>();
                provider.AddTransient<IShipCallAdditionInfo, ShipCall>();
            }
        );
    }).Build();

Dto.DtoBuilder dtoBuilder = host.Services.GetRequiredService<Dto.DtoBuilder>();

dtoBuilder.ValueRequest += arg =>
{
    Console.WriteLine(arg.Path);

    arg.IsCommited = arg.IsLeaf;
};

dtoBuilder.Build<IShipCall>();