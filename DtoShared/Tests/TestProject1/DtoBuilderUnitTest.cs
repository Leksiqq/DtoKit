﻿using Microsoft.Extensions.DependencyInjection;
using Net.Leksi.Dto;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using TestProject1.Dto1;
using System.Diagnostics.CodeAnalysis;

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
        dsp.AddTransient<IVesselShort, Vessel>();
        dsp.Commit();

        TypesForest tf = new(dsp);

        DtoBuilder dtoBuilder = new(tf);

        dtoBuilder.ShouldAutoCreateNotNullable = true;

        dtoBuilder.ValueRequest += DtoBuilder_ValueRequest;

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
        dsp.AddTransient<IVesselShort, Vessel>();
        dsp.Commit();

        TypesForest tf = new(dsp);

        DtoBuilder dtoBuilder = new(tf);

        Trace.WriteLine(dtoBuilder.GenerateHandlerSkeleton<IShipCall>(true));
    }

    private void DtoBuilder_ValueRequest(ValueRequestEventArgs args)
    {
        Trace.WriteLine(args.Path);
        args.Status = ValueRequestStatus.Node;
    }

    [Test]
    public void Test3()
    {
        DtoServiceProvider dsp = new(null);
        dsp.AddTransient<IShipCall, ShipCall>();
        dsp.AddTransient<ILocation, Location>();
        dsp.AddTransient<IRoute, Route>();
        dsp.AddTransient<ILine, Line>();
        dsp.AddTransient<IVesselShort, Vessel>();
        dsp.Commit();

        TypesForest tf = new(dsp);

        DtoBuilder dtoBuilder = new(tf);

        dtoBuilder.ShouldAutoCreateNotNullable = true;

        int i = 1;

        dtoBuilder.ValueRequest += args =>
        {
            switch (args.Path)
            {
                case "/ID_LINE":
                    //args.CreateDefault();
                    args.Value = "TRE";
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/ID_ROUTE":
                    //args.CreateDefault();
                    args.Value = i;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Route/ID_LINE":
                    //args.CreateDefault();
                    args.Value = "TRE";
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Route/ID_RHEAD":
                    //args.CreateDefault();
                    args.Value = 1;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Route/Line/ID_LINE":
                    //args.CreateDefault();
                    args.Value = "TRE";
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Route/Line/Name":
                    //args.CreateDefault();
                    args.Value = "TRE";
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Route/Vessel/ID_VESSEL":
                    //args.CreateDefault();
                    args.Value = "VARYAG";
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Route/Vessel/Name":
                    //args.CreateDefault();
                    args.Value = "VARYAG";
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Voyage":
                    //args.CreateDefault();
                    args.Value = "VAR22001";
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/VoyageAlt":
                    //args.CreateDefault();
                    //args.Value = ...;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Location/ID_LOCATION":
                    //args.CreateDefault();
                    args.Value = i.ToString();
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Location/Type":
                    //args.CreateDefault();
                    args.Value = LocationType.Port;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Location/Unlocode":
                    //args.CreateDefault();
                    //args.Value = ...;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Location/Name":
                    //args.CreateDefault();
                    //args.Value = ...;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/ScheduledArrival":
                    //args.CreateDefault();
                    //args.Value = ...;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/ActualArrival":
                    //args.CreateDefault();
                    //args.Value = ...;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/ScheduledDeparture":
                    //args.CreateDefault();
                    //args.Value = ...;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/ActualDeparture":
                    //args.CreateDefault();
                    //args.Value = ...;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
                case "/Condition":
                    //args.CreateDefault();
                    //args.Value = ...;
                    args.Status = ValueRequestStatus.Node;
                    //args.Status = ValueRequestStatus.Terminal;
                    break;
            }
        };

        List<IShipCall> shipCalls = new();

        for(; i <= 2; i++)
        {
            shipCalls.Add(dtoBuilder.Build<IShipCall>());
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

        Trace.WriteLine(dtoBuilder.GenerateHandlerSkeleton<IVessel>(true));
    }

}
