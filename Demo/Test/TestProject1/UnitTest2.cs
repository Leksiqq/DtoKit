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
            int travelDurationDays = 3;
            int stayHours = 10;
            while (dr.Read())
            {
                Console.WriteLine(dr["ID_LINE"]);
                continue;
                DateTime departure = DateTime.ParseExact("2018-01-01T19:00:00", "s", null) 
                    + TimeSpan.FromDays(random.Next(180) - 90);
                List<string> allPorts = new();
                DbDataReader dr1 = db.GetPorts(null);
                while (dr1.Read())
                {
                    allPorts.Add(dr1["ID_PORT"].ToString());
                }
                int n_ports = (int)random.Next(3, 10);
                List<string> ports = new();
                for(int i = 0; i < n_ports; i++)
                {
                    string port = allPorts[random.Next(allPorts.Count)];
                    if (!ports.Contains(port))
                    {
                        ports.Add(port);
                    }
                }
                int idShipCall = 0;
                for(int direction = 0; ; ++direction)
                {
                    for(int i = 0; i < n_ports - 1; i++)
                    {
                        string port = direction % 2 == 0 ? ports[i] : ports[n_ports - i - 1];
                        DateTime arrival = departure.AddHours(-stayHours);
                        departure = departure.AddDays(travelDurationDays);
                        xdoc.Root.Add(
                            new XElement("table",
                                new XAttribute("ID_SHIPCALL", ++idShipCall),
                                new XAttribute("ID_LINE", dr["ID_LINE"])
                                )
                            );
                    }
                }
            }
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            using XmlWriter xw = XmlWriter.Create(Console.Out, xws);
            xdoc.WriteTo(xw);
            
        }

   }
}