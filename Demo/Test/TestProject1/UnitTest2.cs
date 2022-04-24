using DtoKit.Demo;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public async void Test1()
        {
            CultureInfo myCIclone = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            myCIclone.NumberFormat.NumberDecimalSeparator = ".";

            Thread.CurrentThread.CurrentCulture = myCIclone;

            Database db = new();
            DbDataReader dr = await db.GetRoutesAsync(null, null);
            while (dr.Read())
            {
                Console.WriteLine(dr);
            }
        }

        [Test]
        public async Task Test2()
        {
            Random random = new Random();
            XDocument xdoc = new();
            
            xdoc.Add(new XElement("Data"));
            int travelDurationDays = 3;
            int stayHours = 10;
            Database db = new();
            List<string> allPorts = new();
            DbDataReader dr = await db.GetPortsAsync(null);
            while (dr.Read())
            {
                allPorts.Add(dr["ID_PORT"].ToString());
            }
            
            dr = await db.GetRoutesAsync(null, null);
            while (dr.Read())
            {
                string idVessel = dr["ID_VESSEL"].ToString();
                DateTime departure = DateTime.ParseExact("2018-01-01T19:00:00", "s", null) 
                    + TimeSpan.FromDays(random.Next(180) - 90);
                int n_ports = (int)random.Next(3, 10);
                List<string> ports = new();
                int voyageNum = 0;
                int year = 0;
                for(int i = 0; i < n_ports; i++)
                {
                    while (true)
                    {
                        string port = allPorts[random.Next(allPorts.Count)];
                        if (!ports.Contains(port))
                        {
                            ports.Add(port);
                            break;
                        }
                    }
                }
                int idShipCall = 0;
                bool running = true;
                for(int direction = 0; running; ++direction)
                {
                    if(departure.Year != year)
                    {
                        voyageNum = 0;
                        year = departure.Year;
                    }
                    ++voyageNum;
                    string voyage = idVessel.Substring(0, Math.Min(3, idVessel.Length)) + departure.ToString("yy") 
                        + string.Format("{0:000}", voyageNum);
                    for(int i = 0; i < n_ports - 1; i++)
                    {
                        string port = direction % 2 == 0 ? ports[i] : ports[n_ports - i - 1];
                        DateTime arrival = departure + TimeSpan.FromHours(-stayHours);
                        if(arrival > DateTime.Now)
                        {
                            running = false;
                            break;
                        }
                        xdoc.Root.Add(
                            new XElement("table",
                                new XAttribute("PrevCall", idShipCall == 0 ? string.Empty : idShipCall),
                                new XAttribute("ID_SHIPCALL", ++idShipCall),
                                new XAttribute("ID_ROUTE", dr["ID_ROUTE"]),
                                new XAttribute("ID_LINE", dr["ID_LINE"]),
                                new XAttribute("Arrival", arrival),
                                new XAttribute("Departure", departure),
                                new XAttribute("ID_PORT", port),
                                new XAttribute("Voyage", voyage)
                                )
                            );
                        departure += TimeSpan.FromDays(travelDurationDays);
                    }
                }
            }
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.Encoding = Encoding.UTF8;
            using FileStream output = new FileStream("shipcalls.xml", FileMode.Create);
            using XmlWriter xw = XmlWriter.Create(output, xws);
            xdoc.WriteTo(xw);
            
        }

   }
}