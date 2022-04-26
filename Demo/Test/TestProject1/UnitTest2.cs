using DtoKit.Demo;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Leksi.RestContract;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

public class Generators
{
    private IHost _host;
    private string _connectionString = @"Server=leksi\sqlexpress;Database=marina;Integrated Security=true;Encrypt=no";

    [OneTimeSetUp]
    public void Setup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        Trace.AutoFlush = true;

        string[] parts = Assembly.GetCallingAssembly().Location.Split('\\');
        int pos = parts.Length - 1;
        while(parts[pos] != "Demo")
        {
            --pos;
        }
        string configFile = string.Join(@"\", parts.Take(pos + 1).ToArray()) + @"\Server\Server\config.json";

        var builder = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(config => config.AddJsonFile(configFile))
            .ConfigureServices(serviceCollection =>
            {
                DtoKit.Demo.Setup.Configure(serviceCollection);
                serviceCollection.AddTransient<Database>(options => 
                    new Database(options.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")));
            });
        
        _host = builder.Build();

        _host.RunAsync();

    }

    [Test]
    public async Task Test1()
    {

        CultureInfo myCIclone = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        myCIclone.NumberFormat.NumberDecimalSeparator = ".";

        Thread.CurrentThread.CurrentCulture = myCIclone;

        Database db = _host.Services.GetRequiredService<Database>();
        int count = 0;
        await foreach (DbDataReader dr in db.GetShipCallsAsync("SSK", null, null, null, null, "%IRA%", DateTime.Parse("2022-01-01"), null))
        {
            ++count;
            Console.WriteLine(String.Join("; ", Enumerable.Range(0, dr.FieldCount).Select(i => dr[i].ToString())));
        }
        Console.WriteLine(count);
    }

    [Test]
    public async Task LoadLines()
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        SqlCommand cmd = connection.CreateCommand();
        cmd.CommandText = "insert into Lines (ID_LINE, Name) values (@ID_LINE, @Name)";
        cmd.Parameters.Add("@ID_LINE", System.Data.SqlDbType.NChar, 10);
        cmd.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 50);
        try
        {
            await connection.OpenAsync();


            foreach (var item in XDocument.Load("lines.xml").Root.Elements())
            {
                cmd.Parameters["@ID_LINE"].Value = item.Attribute("ID_LINE").Value;
                cmd.Parameters["@Name"].Value = item.Attribute("Name").Value;
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [Test]
    public async Task LoadPorts()
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        SqlCommand cmd = connection.CreateCommand();
        cmd.CommandText = "insert into Ports (ID_PORT, Name) values (@ID_PORT, @Name)";
        cmd.Parameters.Add("@ID_PORT", System.Data.SqlDbType.NChar, 10);
        cmd.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 50);
        try
        {
            await connection.OpenAsync();

            foreach (var item in XDocument.Load("ports.xml").Root.Elements())
            {
                cmd.Parameters["@ID_PORT"].Value = item.Attribute("ID_PORT").Value;
                cmd.Parameters["@Name"].Value = item.Attribute("Name").Value;
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [Test]
    public async Task LoadVessels()
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        
        SqlCommand select = connection.CreateCommand();

        SqlCommand cmd = connection.CreateCommand();


        cmd.CommandText = @"insert into Vessels (ID_VESSEL, Name, ID_PORT, Length, Width, Height, Brutto, Netto, CallSign) 
values (@ID_VESSEL, @Name, @ID_PORT, @Length, @Width, @Height, @Brutto, @Netto, @CallSign)";
        cmd.Parameters.Add("@ID_VESSEL", System.Data.SqlDbType.NChar, 10);
        cmd.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 50);
        cmd.Parameters.Add("@ID_PORT", System.Data.SqlDbType.NChar, 10);
        cmd.Parameters.Add("@Length", System.Data.SqlDbType.Float);
        cmd.Parameters.Add("@Width", System.Data.SqlDbType.Float);
        cmd.Parameters.Add("@Height", System.Data.SqlDbType.Float);
        cmd.Parameters.Add("@Brutto", System.Data.SqlDbType.Float);
        cmd.Parameters.Add("@Netto", System.Data.SqlDbType.Float);
        cmd.Parameters.Add("@CallSign", System.Data.SqlDbType.NChar, 10);
        try
        {
            await connection.OpenAsync();

            foreach (var item in XDocument.Load("vessels.xml").Root.Elements())
            {
                CultureInfo myCIclone = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                myCIclone.NumberFormat.NumberDecimalSeparator = ".";
                Thread.CurrentThread.CurrentCulture = myCIclone;


                cmd.Parameters["@ID_VESSEL"].Value = item.Attribute("ID_VESSEL").Value;
                cmd.Parameters["@Name"].Value = item.Attribute("Name").Value;
                cmd.Parameters["@ID_PORT"].Value = item.Attribute("ID_PORT").Value;
                cmd.Parameters["@Length"].Value = double.TryParse(item.Attribute("Length").Value, out double length) ? length : DBNull.Value;
                cmd.Parameters["@Width"].Value = double.TryParse(item.Attribute("Width").Value, out double width) ? width : DBNull.Value;
                cmd.Parameters["@Height"].Value = double.TryParse(item.Attribute("Height").Value, out double height) ? height : DBNull.Value;
                cmd.Parameters["@Brutto"].Value = double.TryParse(item.Attribute("Brutto").Value, out double brutto) ? brutto : DBNull.Value;
                cmd.Parameters["@Netto"].Value = double.TryParse(item.Attribute("Netto").Value, out double netto) ? netto : DBNull.Value;
                cmd.Parameters["@CallSign"].Value = item.Attribute("CallSign").Value;
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [Test]
    public async Task LoadRoutes()
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        SqlCommand cmd = connection.CreateCommand();
        cmd.CommandText = "insert into Routes (ID_ROUTE, ID_LINE, ID_VESSEL) values (@ID_ROUTE, @ID_LINE, @ID_VESSEL)";
        cmd.Parameters.Add("@ID_ROUTE", System.Data.SqlDbType.Int);
        cmd.Parameters.Add("@ID_LINE", System.Data.SqlDbType.NChar, 10);
        cmd.Parameters.Add("@ID_VESSEL", System.Data.SqlDbType.NChar, 10);
        try
        {
            await connection.OpenAsync();

            foreach (var item in XDocument.Load("routes.xml").Root.Elements())
            {
                cmd.Parameters["@ID_ROUTE"].Value = int.Parse(item.Attribute("ID_ROUTE").Value);
                cmd.Parameters["@ID_LINE"].Value = item.Attribute("ID_LINE").Value;
                cmd.Parameters["@ID_VESSEL"].Value = item.Attribute("ID_VESSEL").Value;
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [Test]
    public async Task LoadShipCalls()
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        SqlCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"insert into ShipCalls (ID_SHIPCALL, ID_LINE, ID_ROUTE, ID_PORT, Voyage, Arrival, Departure, PrevID_SHIPCALL) 
values (@ID_SHIPCALL, @ID_LINE, @ID_ROUTE, @ID_PORT, @Voyage, @Arrival, @Departure, @PrevID_SHIPCALL)";
        cmd.Parameters.Add("@ID_SHIPCALL", System.Data.SqlDbType.Int);
        cmd.Parameters.Add("@ID_LINE", System.Data.SqlDbType.NChar, 10);
        cmd.Parameters.Add("@ID_ROUTE", System.Data.SqlDbType.Int);
        cmd.Parameters.Add("@ID_PORT", System.Data.SqlDbType.NChar, 10);
        cmd.Parameters.Add("@Voyage", System.Data.SqlDbType.NVarChar, 50);
        cmd.Parameters.Add("@Arrival", System.Data.SqlDbType.DateTime);
        cmd.Parameters.Add("@Departure", System.Data.SqlDbType.DateTime);
        cmd.Parameters.Add("@PrevID_SHIPCALL", System.Data.SqlDbType.Int);
        try
        {
            await connection.OpenAsync();

            foreach (var item in XDocument.Load("shipcalls.xml").Root.Elements())
            {
                cmd.Parameters["@ID_SHIPCALL"].Value = int.Parse(item.Attribute("ID_SHIPCALL").Value);
                cmd.Parameters["@ID_LINE"].Value = item.Attribute("ID_LINE").Value;
                cmd.Parameters["@ID_ROUTE"].Value = int.Parse(item.Attribute("ID_ROUTE").Value);
                cmd.Parameters["@ID_PORT"].Value = item.Attribute("ID_PORT").Value;
                cmd.Parameters["@Voyage"].Value = item.Attribute("Voyage").Value;
                cmd.Parameters["@Arrival"].Value = DateTime.Parse(item.Attribute("Arrival").Value);
                cmd.Parameters["@Departure"].Value = DateTime.Parse(item.Attribute("Departure").Value);
                cmd.Parameters["@PrevID_SHIPCALL"].Value = int.TryParse(item.Attribute("PrevCall").Value, out int id) ? id : DBNull.Value;
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    [Test]
    public async Task GenerateShipCalls()
    {
        Random random = new Random();
        XDocument xdoc = new();

        xdoc.Add(new XElement("Data"));
        int travelDurationDays = 3;
        int stayHours = 10;
        Database db = _host.Services.GetRequiredService<Database>();
        List<string> allPorts = new();
        Dictionary<string, int> shipcallsIds = new();
        await foreach (var dr in db.GetPortsAsync(null, null))
        {
            allPorts.Add(dr["ID_PORT"].ToString());
        }

        await foreach (var dr in db.GetRoutesAsync(null, null, null, null))
        {
            string idVessel = dr["ID_VESSEL"].ToString();
            DateTime departure = DateTime.ParseExact("2018-01-01T19:00:00", "s", null)
                + TimeSpan.FromDays(random.Next(180) - 90);
            int n_ports = (int)random.Next(3, 10);
            List<string> ports = new();
            int voyageNum = 0;
            int year = 0;
            for (int i = 0; i < n_ports; i++)
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
            if (!shipcallsIds.ContainsKey(dr["ID_LINE"].ToString()))
            {
                shipcallsIds[dr["ID_LINE"].ToString()] = 0;
            }
            bool running = true;
            for (int direction = 0; running; ++direction)
            {
                if (departure.Year != year)
                {
                    voyageNum = 0;
                    year = departure.Year;
                }
                ++voyageNum;
                string voyage = idVessel.Substring(0, Math.Min(3, idVessel.Length)) + departure.ToString("yy")
                    + string.Format("{0:000}", voyageNum);
                for (int i = 0; i < n_ports - 1; i++)
                {
                    string port = direction % 2 == 0 ? ports[i] : ports[n_ports - i - 1];
                    DateTime arrival = departure + TimeSpan.FromHours(-stayHours);
                    if (arrival > DateTime.Now)
                    {
                        running = false;
                        break;
                    }
                    xdoc.Root.Add(
                        new XElement("table",
                            new XAttribute("PrevCall", shipcallsIds[dr["ID_LINE"].ToString()] == 0 ? string.Empty : shipcallsIds[dr["ID_LINE"].ToString()]),
                            new XAttribute("ID_SHIPCALL", ++shipcallsIds[dr["ID_LINE"].ToString()]),
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

    [Test]
    public void Test3()
    {
        var ports = from port in XDocument.Load("ports.xml").Root.Elements() select port.Attribute("ID_PORT").Value;
        var ports1 = from vessel in XDocument.Load("vessels.xml").Root.Elements()
            where !ports.Contains(vessel.Attribute("ID_PORT").Value)
            select vessel.Attribute("ID_PORT").Value;
        foreach (var idPort in ports1)
        {
            Console.WriteLine(idPort);
        }
    }

    [Test]
    public void GenerateControllers()
    {
        Console.Write(
            new SourceGenerator(_host.Services)
            .GenerateMvcControllerInterfaceAndProxyClass<IConnector>("DtoKit.Demo.IDemoController", "DtoKit.Demo.DemoControllerProxy")
            );
    }

}
