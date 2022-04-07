﻿using Microsoft.Extensions.DependencyInjection;
using Net.Leksi.Dto;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using TestProject1.Dto1;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DeepEqual.Syntax;
using Microsoft.Extensions.Hosting;

namespace TestProject1;

public class DtoJsonConverterUnitTest
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

        dtoBuilder.ValueRequest += args =>
        {
            //Trace.WriteLine(args.Path);
            if(args.Kind is ValueRequestKind.Terminal)
            {
                args.Commit();
            }
        };

        IShipCall shipCall = dtoBuilder.Build<IShipCall>();

        DtoJsonConverterFactory converter = host.Services.GetRequiredService<DtoJsonConverterFactory>();
        converter.WithMagic = true;
        converter.WithKeyOnlyForRepeated = false;

        JsonSerializerOptions options = new() {WriteIndented = true };
        options.Converters.Add(converter);

        string json = JsonSerializer.Serialize(shipCall, options);

        Console.WriteLine(json);
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Test3(bool withMagic, bool withKeyOnly)
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
                case "/RouteImpl/ID_RHEAD":
                    args.Value = 1;
                    args.Commit();
                    break;
                case "/RouteImpl/Line/ID_LINE":
                    args.Value = "TRE";
                    args.Commit();
                    break;
                case "/RouteImpl/Line/Name":
                    args.Value = "TRE";
                    args.Commit();
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

        DtoJsonConverterFactory converter = host.Services.GetRequiredService<DtoJsonConverterFactory>();
        converter.WithMagic = withMagic;
        converter.WithKeyOnlyForRepeated = withKeyOnly;
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(converter);

        string json = JsonSerializer.Serialize(shipCalls, options);

        Console.WriteLine(json);

        converter = converter = host.Services.GetRequiredService<DtoJsonConverterFactory>();
        converter.WithMagic = withMagic;
        converter.WithKeyOnlyForRepeated = false;

        options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(converter);

        var res = new List<IShipCallForListing>();
        converter.Target = res;
        JsonSerializer.Deserialize<RewritableListStub<IShipCallForListing>>(json, options);

        shipCalls.ShouldDeepEqual(res);

        converter = converter = host.Services.GetRequiredService<DtoJsonConverterFactory>();
        converter.WithMagic = withMagic;
        converter.WithKeyOnlyForRepeated = withKeyOnly;

        options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(converter);

        Assert.That(JsonSerializer.Serialize<object>(res, options), Is.EqualTo(json));

        foreach(object shipCall in shipCalls)
        {
            ((ShipCall)shipCall).AdditionalInfo = "Additional info";
        }

        converter = converter = host.Services.GetRequiredService<DtoJsonConverterFactory>();
        converter.WithMagic = withMagic;
        converter.WithKeyOnlyForRepeated = withKeyOnly;

        options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(converter);

        json = JsonSerializer.Serialize(shipCalls.Select(v => (IShipCallAdditionalInfo)v), options);
        Console.WriteLine(json);

        converter = converter = host.Services.GetRequiredService<DtoJsonConverterFactory>();
        converter.WithMagic = withMagic;
        converter.WithKeyOnlyForRepeated = withKeyOnly;

        options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(converter);

        converter.Target = res;
        JsonSerializer.Deserialize<UpdateableListStub<IShipCallAdditionalInfo>>(json, options);


        converter = converter = host.Services.GetRequiredService<DtoJsonConverterFactory>();
        converter.WithMagic = withMagic;
        converter.WithKeyOnlyForRepeated = withKeyOnly;

        options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(converter);

        json = JsonSerializer.Serialize(res, options);

        Console.WriteLine(json);

    }

}
