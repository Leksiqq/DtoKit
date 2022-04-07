using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Leksi.Dto;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TestProject1.Dto1;

namespace TestProject1;

public class DtoBuilderUnitTest
{
    private IHost host;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(serviceCollection =>
            {
                DtoKit.Install(serviceCollection, services =>
                {
                    services.AddTransient<IShipCall, ShipCall>();
                    services.AddTransient<ILocation, Location>();
                    services.AddTransient<IRoute, Route>();
                    services.AddTransient<ILine, Line>();
                    services.AddTransient<IVessel, Vessel>();
                    services.AddTransient<IShipCallForListing, ShipCall>();
                    services.AddTransient<IShipCallAdditionalInfo, ShipCall>();
                    services.AddTransient<IRouteShort, Route>();
                    services.AddTransient<IVesselShort, Vessel>();
                });
            });
        host = hostBuilder.Build();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Trace.Flush();
    }

    [SetUp]
    public void Setup()
    {
        host.RunAsync();
    }

    [Test]
    public void Test1()
    {
        DtoBuilder dtoBuilder = host.Services.GetRequiredService<DtoBuilder>();

        int tabPos = 35;

        dtoBuilder.ValueRequest += args =>
        {
            Trace.WriteLine($"{args.Path}{string.Join("", Enumerable.Range(0, tabPos - args.Path.Length).Select(v => " "))}({args.Kind})");
            if (args.Kind is ValueRequestKind.Terminal)
            {
                args.Commit();
            }
        };

        dtoBuilder.Build<IShipCall>();
    }

    [Test]
    public void Test2()
    {
        DtoBuilder dtoBuilder = host.Services.GetRequiredService<DtoBuilder>();

        Trace.WriteLine(dtoBuilder.GenerateHandlerSkeleton<IShipCall>());
    }

    [Test]
    public void Test3()
    {
        DtoBuilder dtoBuilder = host.Services.GetRequiredService<DtoBuilder>();

        int i = 1;

        dtoBuilder.ValueRequest += args =>
        {
            switch (args.Path)
            {
                case "/ID_LINE":
                    args.Value = "TRE";
                    args.Commit();
                    break;
                case "/ID_ROUTE":
                    args.Value = i;
                    args.Commit();
                    break;
                case "/RouteImpl/ID_LINE":
                    args.Value = "TRE";
                    args.Commit();
                    break;
                case "/RouteImpl":
                    break;
                case "/RouteImpl/ID_RHEAD":
                    args.Value = 1;
                    args.Commit();
                    break;
                case "/RouteImpl/Line":
                    break;
                case "/RouteImpl/Line/ID_LINE":
                    args.Value = "TRE";
                    args.Commit();
                    break;
                case "/RouteImpl/Line/Name":
                    args.Value = "TRE";
                    args.Commit();
                    break;
                case "/RouteImpl/Vessel":
                    break;
                case "/RouteImpl/Vessel/ID_VESSEL":
                    args.Value = "VARYAG";
                    args.Commit();
                    break;
                case "/RouteImpl/Vessel/Name":
                    args.Value = "VARYAG";
                    args.Commit();
                    break;
                case "/Voyage":
                    args.Value = "VAR22001";
                    args.Commit();
                    break;
                case "/VoyageAlt":
                    args.Commit();
                    break;
                case "/Location":
                    break;
                case "/Location/ID_LOCATION":
                    args.Value = i.ToString();
                    args.Commit();
                    break;
                case "/Location/Type":
                    args.Value = LocationType.Port;
                    args.Commit();
                    break;
                case "/Location/Unlocode":
                    args.Commit();
                    break;
                case "/Location/Name":
                    args.Commit();
                    break;
                case "/ScheduledArrival":
                    args.Commit();
                    break;
                case "/ActualArrival":
                    args.Commit();
                    break;
                case "/ScheduledDeparture":
                    args.Commit();
                    break;
                case "/ActualDeparture":
                    args.Commit();
                    break;
                case "/Condition":
                    args.Commit();
                    break;
                case "/AdditionalInfo":
                    args.Commit();
                    break;

            }
        };

        List<IShipCallForListing> shipCalls = new();

        for(; i <= 3; i++)
        {
            shipCalls.Add(dtoBuilder.Build<IShipCallForListing>());
        }
    }

    [Test]
    public void Test4()
    {
        DtoBuilder dtoBuilder = host.Services.GetRequiredService<DtoBuilder>();

        Trace.WriteLine(dtoBuilder.GenerateHandlerSkeleton<IVessel>());
    }

    [Test]
    public void Test6()
    {

        DtoBuilder dtoBuilder = host.Services.GetRequiredService<DtoBuilder>();

        Trace.WriteLine(dtoBuilder.GenerateHandlerSkeleton<IShipCallForListing>());
    }

}
