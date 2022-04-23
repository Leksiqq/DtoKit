using DtoKit.Demo;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace TestProject2
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
            CultureInfo myCIclone = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            myCIclone.NumberFormat.NumberDecimalSeparator = ".";

            Thread.CurrentThread.CurrentCulture = myCIclone;

            Database db = new();
            DbDataReader dr = db.GetRoutes(null, null);
            while (dr.Read())
            {
                Console.WriteLine(dr);
            }
        }

        [Test]
        public void Test2()
        {
            Random random = new Random();
            XDocument xdoc = new();
            xdoc.Add(new XElement("Data"));
            Database db = new();
            DbDataReader dr = db.GetRoutes(null, null);
            while (dr.Read())
            {
                DateTime departure = DateTime.ParseExact("2018-01-01T19:00:00", "s", null) 
                    + TimeSpan.FromDays(random.NextInt64(180) - 90);
                int? prevCallId = null;
                List<string> ports = new();
                DbDataReader dr1 = db.GetPorts(null);
                while (dr1.Read())
                {
                    ports.Add(dr1["ID_PORT"].ToString());
                }
                int n_ports = (int)random.NextInt64(3, 10);
                for(int i = 0; i < n_ports; i++)
                {

                }
                xdoc.Root.Add(new XElement("table"));
            }
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            using XmlWriter xw = XmlWriter.Create(Console.Out, xws);
            xdoc.WriteTo(xw);
            
        }

   }
}