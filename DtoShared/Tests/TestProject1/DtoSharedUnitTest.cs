using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Leksi.Dto;
using NUnit.Framework;
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
                serviceCollection.AddTransient<DtoServiceProvider>();
                serviceCollection.AddTransient<ILine, Line>();
            })
            ;
        IHost host = hostBuilder.Build();
        host.RunAsync();

        DtoServiceProvider dtoSp = host.Services.GetRequiredService<DtoServiceProvider>();
        dtoSp.AddTransient<ILine>();
        DtoServiceProvider dtoSp1 = host.Services.GetRequiredService<DtoServiceProvider>();
        dtoSp1.AddTransient<ILine, Line>();

        ILine line = dtoSp.GetRequiredService<ILine>();
    }
}
