using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Leksi.Dto;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace TestProject1
{
    public class Tests
    {
        private static IHost _host;

        static Tests()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(serviceCollection =>
                {
                    DtoKit.Demo.Setup.Configure(serviceCollection);
                }).Build();
        }

        static IEnumerable<Type> DtoTypes()
        {
            DtoServiceProvider sp = _host.Services.GetService<DtoServiceProvider>();

            foreach(var item in sp)
            {
                yield return item.ServiceType;
            }
        }

        [Test]
        [TestCaseSource(nameof(DtoTypes))]
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

        [Test]
        public void Test3()
        {

        }
    }
}