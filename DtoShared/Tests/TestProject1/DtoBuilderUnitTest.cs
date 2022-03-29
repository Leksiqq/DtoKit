using Microsoft.Extensions.DependencyInjection;
using Net.Leksi.Dto;
using NUnit.Framework;
using System.Diagnostics;
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
        dsp.AddTransient<IVesselShort, Vessel>();

        DtoBuilder<IShipCall> dtoBuilder = new(dsp);

        dtoBuilder.IsAutoCreateDefault = true;

        dtoBuilder.ValueRequest += DtoBuilder_ValueRequest;

        dtoBuilder.Build();
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

        DtoBuilder<IShipCall> dtoBuilder = new(dsp);

        Trace.WriteLine(dtoBuilder.GenerateHandlerStub());
    }

    private void DtoBuilder_ValueRequest(ValueRequestEventArgs args)
    {
        Trace.WriteLine(args.Path);
        args.Status = ValueRequestStatus.Node;
    }
}
