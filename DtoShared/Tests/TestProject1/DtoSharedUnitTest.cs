using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Leksi.Dto;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using TestProject1.Dto1;

namespace TestProject1;

public class DtoSharedUnitTest
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

        IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(serviceCollection =>
            {
                DtoServiceProvider.Install(serviceCollection, services =>
                {
                    services.AddTransient<ILine, Line>();
                });
                DtoServiceProvider.Install(serviceCollection, services =>
                {
                    services.AddTransient<IRoute, Route>();
                });
            })
            ;
        IHost host = hostBuilder.Build();
        host.RunAsync();


        foreach (DtoServiceProvider dtoSp in host.Services.GetServices<DtoServiceProvider>())
        {
            Trace.WriteLine(dtoSp.IsRegistered<ILine>());
            Trace.WriteLine(dtoSp.IsRegistered<IRoute>());
            ILine line = dtoSp.GetService<ILine>();
            IRoute route = dtoSp.GetService<IRoute>();

            Trace.WriteLine(line);
            Trace.WriteLine(route);

            Trace.WriteLine("");
        }

    }
}
