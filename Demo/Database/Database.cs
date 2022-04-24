using System.Data.Common;
using System.Xml.Linq;

namespace DtoKit.Demo;

public class Database
{
    private const string _lines = "lines.xml";
    private const string _ports = "ports.xml";
    private const string _vessels = "vessels.xml";
    private const string _routes = "routes.xml";
    private const string _shipcalls = "shipcalls.xml";

    private object _lock = new object();

    public Task<DbDataReader> GetLinesAsync(string? id)
    {
        return Task.Run(() =>
        {
            XDocument xlines;
            lock (_lock)
            {
                xlines = XDocument.Load(_lines);
            }
            var lines = from line in xlines.Root.Elements()
                        select new { ID_LINE = line.Attribute("ID_LINE").Value, Name = line.Attribute("Name").Value };
            return new DataReader(from line in lines where (id is null || line.ID_LINE == id) select line) as DbDataReader;
        });
    }

    public Task<DbDataReader> GetPortsAsync(string? id)
    {

        return Task.Run(() =>
        {
            XDocument xport;
            lock (_lock)
            {
                xport = XDocument.Load(_ports);
            }
            var ports = from port in xport.Root.Elements()
                        select new { ID_PORT = port.Attribute("ID_PORT").Value, Name = port.Attribute("Name").Value };
            return new DataReader(from port in ports where (id is null || port.ID_PORT == id) select port) as DbDataReader;
        });
    }

    public Task<DbDataReader> GetVesselsAsync(string? id)
    {
        return Task.Run(() =>
        {
            XDocument xvessels;
            XDocument xports;
            lock (_lock)
            {
                xvessels = XDocument.Load(_vessels);
                xports = XDocument.Load(_ports);
            }
            var vessels = from vessel in xvessels.Root.Elements()
                          from port in xports.Root.Elements()
                          where port.Attribute("ID_PORT").Value == vessel.Attribute("ID_PORT").Value
                          select new
                          {
                              ID_VESSEL = vessel.Attribute("ID_VESSEL").Value,
                              Name = vessel.Attribute("Name").Value,
                              CallSign = vessel.Attribute("CallSign").Value,
                              Brutto = vessel.Attribute("Brutto").Value,
                              Height = vessel.Attribute("Height").Value,
                              Length = vessel.Attribute("Length").Value,
                              Netto = vessel.Attribute("Netto").Value,
                              Width = vessel.Attribute("Width").Value,
                              ID_PORT = vessel.Attribute("ID_PORT").Value,
                              PortName = port.Attribute("Name").Value
                          };
            return new DataReader(from vessel in vessels where (id is null || vessel.ID_VESSEL == id) select vessel) as DbDataReader;
        });
    }

    public Task<DbDataReader> GetRoutesAsync(string? id_line, int? id_route)
    {
        return Task.Run(() =>
        {
            XDocument xroutes;
            XDocument xvessels;
            XDocument xlines;
            XDocument xports;
            lock (_lock)
            {
                xvessels = XDocument.Load(_vessels);
                xlines = XDocument.Load(_lines);
                xroutes = XDocument.Load(_routes);
                xports = XDocument.Load(_ports);
            }
            var routes = from route in xroutes.Root.Elements()
                         from vessel in xvessels.Root.Elements()
                         from line in xlines.Root.Elements()
                         from port in xports.Root.Elements()
                         where line.Attribute("ID_LINE").Value == route.Attribute("ID_LINE").Value
                           && vessel.Attribute("ID_VESSEL").Value == route.Attribute("ID_VESSEL").Value
                           && port.Attribute("ID_PORT").Value == vessel.Attribute("ID_PORT").Value
                         select new
                         {
                             ID_LINE = route.Attribute("ID_LINE").Value,
                             ID_ROUTE = route.Attribute("ID_ROUTE").Value,
                             ID_VESSEL = route.Attribute("ID_VESSEL").Value,
                             LineName = line.Attribute("Name").Value,
                             VesselName = vessel.Attribute("Name").Value,
                             VesselCallSign = vessel.Attribute("CallSign").Value,
                             VesselBrutto = vessel.Attribute("Brutto").Value,
                             VesselHeight = vessel.Attribute("Height").Value,
                             VesselLength = vessel.Attribute("Length").Value,
                             VesselNetto = vessel.Attribute("Netto").Value,
                             VesselWidth = vessel.Attribute("Width").Value,
                             VesselID_PORT = vessel.Attribute("ID_PORT").Value,
                             VesselPortName = port.Attribute("Name").Value
                         };
            return new DataReader(from route in routes
                                  where (id_line is null || route.ID_LINE == id_line)
                                    && (id_route is null || route.ID_ROUTE == id_route.ToString())
                                  select route) as DbDataReader;
        });
    }

    public Task<DbDataReader> GetShipCallsAsync(string? id_line, string? id_vessel, 
        string? id_port, DateTime? _from, DateTime? to)
    {
        return Task.Run(() =>
        {
            XDocument xshipcalls;
            XDocument xroutes;
            XDocument xvessels;
            XDocument xlines;
            XDocument xports;
            lock (_lock)
            {
                xshipcalls = XDocument.Load(_shipcalls);
                xvessels = XDocument.Load(_vessels);
                xlines = XDocument.Load(_lines);
                xroutes = XDocument.Load(_routes);
                xports = XDocument.Load(_ports);
            }
            var shipcalls = from shipcall in xshipcalls.Root.Elements()
                         from route in xroutes.Root.Elements()
                         from vessel in xvessels.Root.Elements()
                         from line in xlines.Root.Elements()
                         from port in xports.Root.Elements()
                         where route.Attribute("ID_ROUTE").Value == shipcall.Attribute("ID_ROUTE").Value
                           && route.Attribute("ID_LINE").Value == shipcall.Attribute("ID_LINE").Value
                            && line.Attribute("ID_LINE").Value == route.Attribute("ID_LINE").Value
                           && vessel.Attribute("ID_VESSEL").Value == route.Attribute("ID_VESSEL").Value
                           && port.Attribute("ID_PORT").Value == vessel.Attribute("ID_PORT").Value
                         select new
                         {
                             ID_LINE = shipcall.Attribute("ID_LINE").Value,
                             ID_SHIPCALL = shipcall.Attribute("ID_SHIPCALL").Value,
                             AdditionalInfo = string.Empty,
                             Arrival = shipcall.Attribute("Arrival").Value,
                             Departure = shipcall.Attribute("Departure").Value,
                             Voyage = shipcall.Attribute("Voyage").Value,
                             ID_PORT = vessel.Attribute("ID_PORT").Value,
                             PortName = port.Attribute("Name").Value,
                             PrevID_SHIPCALL = shipcall.Attribute("PrevCall").Value,
                             ID_ROUTE = route.Attribute("ID_ROUTE").Value,
                             ID_VESSEL = route.Attribute("ID_VESSEL").Value,
                             LineName = line.Attribute("Name").Value,
                             VesselCallSign = vessel.Attribute("CallSign").Value,
                             VesselBrutto = vessel.Attribute("Brutto").Value,
                             VesselHeight = vessel.Attribute("Height").Value,
                             VesselLength = vessel.Attribute("Length").Value,
                             VesselNetto = vessel.Attribute("Netto").Value,
                             VesselWidth = vessel.Attribute("Width").Value,
                             VesselID_PORT = vessel.Attribute("ID_PORT").Value,
                             VesselPortName = port.Attribute("Name").Value
                         };
            return new DataReader(from shipcall in shipcalls
                                  where (id_line is null || shipcall.ID_LINE == id_line)
                                    && (id_vessel is null || shipcall.ID_VESSEL == id_vessel)
                                    && (id_port is null || shipcall.ID_PORT == id_port)
                                    && (_from is null || DateTime.Parse(shipcall.Departure) >= _from)
                                    && (to is null || DateTime.Parse(shipcall.Departure) < to)
                                  select shipcall) as DbDataReader;
        });
    }
}
}
