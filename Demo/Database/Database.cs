using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Xml.Linq;

namespace DtoKit.Demo;

public class Database
{
    private const string _lines = "lines.xml";
    private const string _ports = "ports.xml";
    private const string _vessels = "vessels.xml";

    private object _lock = new object();

    public DbDataReader GetLines()
    {
        return GetLine(null);
    }

    public DbDataReader GetLine(string id)
    {
        XDocument xlines;
        lock (_lock)
        {
            xlines = XDocument.Load(_lines);
        }
        var lines =  from line in xlines.Root.Elements().Elements()
                     select new { ID_LINE = line.Attribute("ID_LINE").Value, Name = line.Attribute("Name").Value };
        return new DataReader(from line in lines where (id is null || line.ID_LINE == id) select line);
    }

    public DbDataReader GetPorts()
    {
        return GetPort(null);
    }

    public DbDataReader GetPort(string id)
    {
        XDocument xport;
        lock (_lock)
        {
            xport = XDocument.Load(_ports);
        }
        var ports = from port in xport.Root.Elements().Elements()
                    select new { ID_PORT = port.Attribute("ID_PORT").Value, Name = port.Attribute("Name").Value };
        return new DataReader(from port in ports where (id is null || port.ID_PORT == id) select port);
    }

    public DbDataReader GetVessels()
    {
        return GetVessel(null);
    }

    public DbDataReader GetVessel(string id)
    {
        XDocument xvessels;
        XDocument xports;
        lock (_lock)
        {
            xvessels = XDocument.Load(_vessels);
            xports = XDocument.Load(_ports);
        }
        var vessels = from vessel in xvessels.Root.Elements().Elements()
                    from port in xports.Root.Elements().Elements()
                    where port.Attribute("ID_PORT").Value == vessel.Attribute("ID_PORT").Value
                    select new { 
                        ID_VESSEL = vessel.Attribute("ID_VESSEL").Value, 
                        Name = vessel.Attribute("Name").Value, 
                        PortName = port.Attribute("Name").Value };
        return new DataReader(from vessel in vessels where (id is null || vessel.ID_VESSEL == id) select vessel);
    }

}
