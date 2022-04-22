using DtoKit.Demo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Leksi.Dto;
using NUnit.Framework;
using System;
using System.Data.Common;
using System.Globalization;
using System.Threading;

namespace TestProject1
{
    public class Tests
    {
        private IHost _host;

        [OneTimeSetUp]
        public void Setup()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(serviceCollection =>
                {
                    DtoKit.Demo.Setup.Configure(serviceCollection);
                }).Build();
        }

        [Test]
        [TestCase(typeof(ILine))]
        [TestCase(typeof(IPort))]
        [TestCase(typeof(IVessel))]
        [TestCase(typeof(IRoute))]
        [TestCase(typeof(IShipCall))]
        public void Test2(Type type)
        {
            DtoBuilder dtoBuilder = _host.Services.GetRequiredService<DtoBuilder>();

            dtoBuilder.ValueRequest += arg =>
            {
                Console.WriteLine(arg.Path);

                arg.IsCommited = arg.IsLeaf;
            };

            dtoBuilder.BuildOfType(type);
        }
    }
}