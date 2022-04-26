using DtoKit.Demo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Leksi.Dto;
using Net.Leksi.RestContract;
using NUnit.Framework;
using System;
using System.Diagnostics;

public class RestContract
{
    private static IHost _host;

    static RestContract()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(serviceCollection =>
            {
                Setup.Configure(serviceCollection);
            }).Build();
        Trace.Listeners.Add(new ConsoleTraceListener());
        Trace.AutoFlush = true;
    }

    [Test]
    public void GenerateRestSources()
    {
        SourceGenerator sg = new(_host.Services.GetRequiredService<DtoServiceProvider>());

        string result = sg.GenerateMvcControllerInterfaceAndProxyClass<IConnector>("DtoKit.Demo.IDemoController", "DtoKit.Demo.DemoControllerProxy");
        Console.WriteLine(result);
    }
}
