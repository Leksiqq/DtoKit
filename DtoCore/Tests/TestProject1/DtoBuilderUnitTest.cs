using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Leksi.Dto;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
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
                    services.AddTransient<IArrivalShipCall, ShipCall>();
                    services.AddTransient<IDepartureShipCall, ShipCall>();
                    services.AddTransient<IRouteShort, Route>();
                    services.AddTransient<IVesselShort, Vessel>();
                    services.AddTransient<ITravelForListing, Travel>();
                });
            });
        host = hostBuilder.Build();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Trace.Flush();
    }

    [NUnit.Framework.SetUp]
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
            if (args.Kind is ValueRequestKind.Leaf)
            {
                args.Commit();
            }
        };

        IShipCall shipCall = dtoBuilder.Build<IShipCall>();

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


    internal class Helper1
    {

        [Path("/")]
        object Apache(string path, object value, bool isNullable, ref bool isCommited)
        {
            //isCommited = true;
            return new Line {ID_LINE = "NTL", Name = "NTL" };
        }
        object Nginx(string path, object value)
        {
            return "1";
        }
        [Path("/ID_LINE")]
        [Path("/Name")]
        object Netscape(string path, object value)
        {
            return "TRE";
        }

        [Net.Leksi.Dto.Setup]
        void Setup()
        {
            Trace.WriteLine("Setup");
        }
        [Shutdown]
        void Shutdown()
        {
            Trace.WriteLine("Shutdown");
        }
    }

    [Test]
    public void Test5()
    {
        DtoBuilder dtoBuilder = host.Services.GetRequiredService<DtoBuilder>();
        DtoJsonConverterFactory converter = host.Services.GetRequiredService<DtoJsonConverterFactory>();
        JsonSerializerOptions options = new JsonSerializerOptions { };
        options.Converters.Add(converter);

        ILine line = dtoBuilder.Build<ILine>(new Helper1());

        Trace.WriteLine(line.ShortName);

        Trace.WriteLine(JsonSerializer.Serialize(line, options));

    }

    [Test]
    public void Test6()
    {

        DtoBuilder dtoBuilder = host.Services.GetRequiredService<DtoBuilder>();

        Trace.WriteLine(dtoBuilder.GenerateHandlerSkeleton<IShipCallForListing>());
    }

    [Test]
    public void Test7()
    {

        DtoBuilder dtoBuilder = host.Services.GetRequiredService<DtoBuilder>();

        Trace.WriteLine(dtoBuilder.GenerateHelperSkeleton<ITravelForListing>());
    }

    class TravelHelper
    {
        [Path("/ArrivalShipCall")]
        public object Set_ArrivalShipCall(string path, object value, bool isNullable, ref bool isCommited)
        {
            return null;
        }

        [Path("/ArrivalShipCall/ID_LINE")]
        public object Set_ArrivalShipCall_ID_LINE(string path, object value)
        {
            throw new NotImplementedException();
        }

        [Path("/ArrivalShipCall/ID_ROUTE")]
        public object Set_ArrivalShipCall_ID_ROUTE(string path, object value)
        {
            throw new NotImplementedException();
        }

        [Path("/ArrivalShipCall/ActualArrival")]
        public object Set_ArrivalShipCall_ActualArrival(string path, object value)
        {
            throw new NotImplementedException();
        }

        [Path("/ArrivalShipCall/Location")]
        public object Set_ArrivalShipCall_Location(string path, object value, bool isNullable, ref bool isCommited)
        {
            throw new NotImplementedException();
        }

        [Path("/ArrivalShipCall/Location/ID_LOCATION")]
        public object Set_ArrivalShipCall_Location_ID_LOCATION(string path, object value)
        {
            throw new NotImplementedException();
        }

        [Path("/ArrivalShipCall/Location/Name")]
        public object Set_ArrivalShipCall_Location_Name(string path, object value)
        {
            throw new NotImplementedException();
        }

        [Path("/ArrivalShipCall/Location/Type")]
        public object Set_ArrivalShipCall_Location_Type(string path, object value)
        {
            throw new NotImplementedException();
        }

        [Path("/ArrivalShipCall/Location/Unlocode")]
        public object Set_ArrivalShipCall_Location_Unlocode(string path, object value)
        {
            throw new NotImplementedException();
        }

        [Path("/DepartureShipCall")]
        public object Set_DepartureShipCall(string path, object value, bool isNullable, ref bool isCommited)
        {
            throw new NotImplementedException();
        }

        [Path("/DepartureShipCall/ID_LINE")]
        public object Set_DepartureShipCall_ID_LINE(string path, object value)
        {
            throw new NotImplementedException();
        }

        [Path("/DepartureShipCall/ID_ROUTE")]
        public object Set_DepartureShipCall_ID_ROUTE(string path, object value)
        {
            throw new NotImplementedException();
        }

    }

    [Test]
    public void Test8()
    {

        DtoBuilder dtoBuilder = host.Services.GetRequiredService<DtoBuilder>();

        dtoBuilder.Build<ITravelForListing>(new TravelHelper());
    }

}
