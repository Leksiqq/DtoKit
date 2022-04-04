using Microsoft.Extensions.DependencyInjection;
using Net.Leksi.Dto;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TestProject1.Dto1;

namespace TestProject1;

public class DtoBuilderUnitTest
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Trace.Flush();
    }

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        DtoServiceProvider dsp = new(null);
        dsp.AddTransient<IShipCall, ShipCall>();
        dsp.AddTransient<ILocation, Location>();
        dsp.AddTransient<IRoute, Route>();
        dsp.AddTransient<ILine, Line>();
        dsp.AddTransient<IVessel, Vessel>();
        dsp.Commit();

        TypesForest tf = new(dsp);

        DtoBuilder dtoBuilder = new(tf);

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
        DtoServiceProvider dsp = new(null);
        dsp.AddTransient<IShipCall, ShipCall>();
        dsp.AddTransient<ILocation, Location>();
        dsp.AddTransient<IRoute, Route>();
        dsp.AddTransient<ILine, Line>();
        dsp.AddTransient<IVessel, Vessel>();
        dsp.Commit();

        TypesForest tf = new(dsp);

        DtoBuilder dtoBuilder = new(tf);

        Trace.WriteLine(dtoBuilder.GenerateHandlerSkeleton<IShipCall>());
    }

    [Test]
    public void Test3()
    {
        DtoServiceProvider dsp = new(null);
        dsp.AddTransient<IShipCallForListing, ShipCall>();
        dsp.AddTransient<IShipCallAdditionalInfo, ShipCall>();
        dsp.AddTransient<ILocation, Location>();
        dsp.AddTransient<IRouteShort, Route>();
        dsp.AddTransient<ILine, Line>();
        dsp.AddTransient<IVesselShort, Vessel>();
        dsp.Commit();

        TypesForest tf = new(dsp);

        DtoBuilder dtoBuilder = new(tf);

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
                case "/Route/ID_LINE":
                    args.Value = "TRE";
                    args.Commit();
                    break;
                case "/Route":
                    break;
                case "/Route/ID_RHEAD":
                    args.Value = 1;
                    args.Commit();
                    break;
                case "/Route/Line":
                    break;
                case "/Route/Line/ID_LINE":
                    args.Value = "TRE";
                    args.Commit();
                    break;
                case "/Route/Line/Name":
                    args.Value = "TRE";
                    args.Commit();
                    break;
                case "/Route/Vessel":
                    break;
                case "/Route/Vessel/ID_VESSEL":
                    args.Value = "VARYAG";
                    args.Commit();
                    break;
                case "/Route/Vessel/Name":
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
        DtoServiceProvider dsp = new(null);
        dsp.AddTransient<ILocation, Location>();
        dsp.AddTransient<IVessel, Vessel>();
        dsp.Commit();

        TypesForest tf = new(dsp);

        DtoBuilder dtoBuilder = new(tf);

        Trace.WriteLine(dtoBuilder.GenerateHandlerSkeleton<IVessel>());
    }

    public class MyClass
    {
        // Should tell me I cannot assign a null
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public object Family { get; set; }
        // Should tell me I can assign a null
        public DateTime? DateOfDeath { get; set; }
    }

    [Test]
    public void Test5()
    {
        foreach (PropertyInfo pi in typeof(MyClass).GetProperties())
        {
            bool canBeNull = !pi.PropertyType.IsValueType && pi.GetCustomAttributes().Any(a => a.GetType().Name.Contains("NullableAttribute"))
                || (Nullable.GetUnderlyingType(pi.PropertyType) != null);
            Console.WriteLine($"{pi.Name}, {canBeNull}");
        }
    }

    [Test]
    public void Test6()
    {
        DtoServiceProvider dsp = new(null);
        dsp.AddTransient<IShipCallForListing, ShipCall>();
        dsp.AddTransient<ILocation, Location>();
        dsp.AddTransient<IRoute, Route>();
        dsp.AddTransient<ILine, Line>();
        dsp.AddTransient<IVessel, Vessel>();
        dsp.Commit();

        TypesForest tf = new(dsp);

        DtoBuilder dtoBuilder = new(tf);

        Trace.WriteLine(dtoBuilder.GenerateHandlerSkeleton<IShipCallForListing>());
    }

}
