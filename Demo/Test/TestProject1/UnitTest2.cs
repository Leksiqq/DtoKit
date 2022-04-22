using DtoKit.Demo;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
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
            XDocument xdoc = new();
            xdoc.Add(new XElement("Data"));
            Database db = new();
            DbDataReader dr = db.GetRoutes(null, null);
            while (dr.Read())
            {
                xdoc.Root.Add(new XElement("table"));
            }
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            using XmlWriter xw = XmlWriter.Create(Console.Out, xws);
            xdoc.WriteTo(xw);
            
        }

   }
}