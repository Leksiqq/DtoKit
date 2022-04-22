using DtoKit.Demo;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Data.Common;
using System.Globalization;
using System.Threading;

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
            DbDataReader dr = db.GetRoutes("SSK", 2);
            while (dr.Read())
            {
                Console.WriteLine(dr);
            }
        }

   }
}