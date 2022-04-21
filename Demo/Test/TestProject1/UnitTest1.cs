using DtoKit.Demo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Leksi.Dto;
using NUnit.Framework;
using System;
using System.Data.Common;

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
        public void Test1()
        {
            Database db = new();
            DbDataReader dr = db.GetLines();

            while (dr.Read())
            {
                Console.WriteLine(dr["ID_LINE"] + ", " + dr["Name"]);
            }


            dr = db.GetLine("SSK");

            if (dr.Read())
            {
                Console.WriteLine(dr["ID_LINE"] + ", " + dr["Name"]);
            }

            dr = db.GetLine("JJP");

            if (dr.Read())
            {
                Console.WriteLine(dr["ID_LINE"] + ", " + dr["Name"]);
            }
            else
            {
                Console.WriteLine("not found");
            }

            dr = db.GetPorts();
            while (dr.Read())
            {
                Console.WriteLine(dr["ID_PORT"] + ", " + dr["Name"]);
            }

            dr = db.GetPort("OSLO");

            if (dr.Read())
            {
                Console.WriteLine(dr["ID_PORT"] + ", " + dr["Name"]);
            }
            else
            {
                Console.WriteLine("not found");
            }
            dr = db.GetPort("TOKYO");

            if (dr.Read())
            {
                Console.WriteLine(dr["ID_PORT"] + ", " + dr["Name"]);
            }
            else
            {
                Console.WriteLine("not found");
            }

            dr = db.GetVessels();
            while (dr.Read())
            {
                Console.WriteLine(dr);
            }
        }

        [Test]
        public void Test2()
        {
            DtoBuilder dtoBuilder = _host.Services.GetRequiredService<DtoBuilder>();

            dtoBuilder.ValueRequest += arg =>
            {
                Console.WriteLine(arg.Path);

                arg.IsCommited = arg.IsLeaf;
            };

            dtoBuilder.Build<IRoute>();
        }
    }
}