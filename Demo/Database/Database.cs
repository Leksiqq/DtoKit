using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Text;

namespace DtoKit.Demo;

public class Database
{
    private readonly string _connectionString;

    public Database(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async IAsyncEnumerable<DbDataReader> GetLinesAsync(string? id, string? name)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);

        Task t = connection.OpenAsync();

        StringBuilder sb = new("select ID_LINE, NAME from Lines");
        List<string> where = new();

        SqlCommand cmd = connection.CreateCommand();
        if (id is { })
        {
            where.Add("ID_LINE=@ID_LINE");
            cmd.Parameters.AddWithValue("@ID_LINE", id);
        }
        if (name is { })
        {
            where.Add("Name like @Name");
            cmd.Parameters.AddWithValue("@Name", name);
        }

        if(where.Count > 0)
        {
            sb.Append(" where ").Append(string.Join(" and ", where));
        }

        cmd.CommandText = sb.ToString();

        await t.ConfigureAwait(false);

        using SqlDataReader dr = await cmd.ExecuteReaderAsync();

        while(await dr.ReadAsync())
        {
            yield return (DbDataReader)dr;
        }
    }

    public async IAsyncEnumerable<DbDataReader> GetPortsAsync(string? id, string? name)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);

        Task t = connection.OpenAsync();

        StringBuilder sb = new("select ID_PORT, Name from Ports");
        List<string> where = new();

        SqlCommand cmd = connection.CreateCommand();
        if (id is { })
        {
            where.Add("ID_PORT=@ID_PORT");
            cmd.Parameters.AddWithValue("@ID_PORT", id);
        }
        if (name is { })
        {
            where.Add("Name like @Name");
            cmd.Parameters.AddWithValue("@Name", name);
        }

        if (where.Count > 0)
        {
            sb.Append(" where ").Append(string.Join(" and ", where));
        }

        cmd.CommandText = sb.ToString();

        await t.ConfigureAwait(false);

        using SqlDataReader dr = await cmd.ExecuteReaderAsync();

        while (await dr.ReadAsync())
        {
            yield return (DbDataReader)dr;
        }
    }

    public async IAsyncEnumerable<DbDataReader> GetVesselsAsync(string? id, string? name)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);

        Task t = connection.OpenAsync();

        StringBuilder sb = new("select ID_VESSEL, Name, ID_PORT PortID_PORT, Length, Width, Height, Brutto, Netto, CallSign from Vessels");
        List<string> where = new();

        SqlCommand cmd = connection.CreateCommand();
        if (id is { })
        {
            where.Add("ID_VESSEL=@ID_VESSEL");
            cmd.Parameters.AddWithValue("@ID_VESSEL", id);
        }
        if (name is { })
        {
            where.Add("Name like @Name");
            cmd.Parameters.AddWithValue("@Name", name);
        }

        if (where.Count > 0)
        {
            sb.Append(" where ").Append(string.Join(" and ", where));
        }

        cmd.CommandText = sb.ToString();

        await t.ConfigureAwait(false);

        using SqlDataReader dr = await cmd.ExecuteReaderAsync();

        while (await dr.ReadAsync())
        {
            yield return (DbDataReader)dr;
        }
    }

    public async IAsyncEnumerable<DbDataReader> GetRoutesAsync(string? id_line, int? id_route, string? lineName, string? vesselName)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);

        Task t = connection.OpenAsync();

        StringBuilder sb = new(@"
select ID_ROUTE, r.ID_LINE, l.Name LineName, r.ID_VESSEL, v.Brutto VesselBrutto, v.Netto VesselNetto, 
v.Length VesselLength, v.Width VesselWidth, v.Height VesselHeight, v.CallSign VesselCallSign, 
v.ID_PORT VesselID_PORT, p.Name VesselPportName
from Routes r, Lines l, Vessels v, Ports p
where 
l.ID_LINE=r.ID_LINE
and v.ID_VESSEL=r.ID_VESSEL
and p.ID_PORT=v.ID_PORT
");
        List<string> where = new();

        SqlCommand cmd = connection.CreateCommand();
        if (id_line is { })
        {
            where.Add("ID_LINE=@ID_LINE");
            cmd.Parameters.AddWithValue("@ID_LINE", id_line);
        }
        if (id_route is { })
        {
            where.Add("@ID_ROUTE=ID_ROUTE");
            cmd.Parameters.AddWithValue("@ID_ROUTE", id_route);
        }
        if (lineName is { })
        {
            where.Add("LineName like @LineName");
            cmd.Parameters.AddWithValue("@LineName", lineName);
        }
        if (vesselName is { })
        {
            where.Add("VesselName like @VesselName");
            cmd.Parameters.AddWithValue("@VesselName", vesselName);
        }

        if (where.Count > 0)
        {
            sb.Append(" and ").Append(string.Join(" and ", where));
        }

        cmd.CommandText = sb.ToString();

        await t.ConfigureAwait(false);

        using SqlDataReader dr = await cmd.ExecuteReaderAsync();

        while (await dr.ReadAsync())
        {
            yield return (DbDataReader)dr;
        }
    }

    public async IAsyncEnumerable<DbDataReader> GetShipCallsAsync(string? id_line, int? id_shipcall, string? lineName, string? voyage, string? vesselName,
        string? portName, DateTime? @from, DateTime? to)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);

        Task t = connection.OpenAsync();

        StringBuilder sb = new(@"
select
    s.ID_LINE,
    s.ID_SHIPCALL,
    s.Arrival,
    s.Departure,
    s.Voyage,
    s.ID_PORT,
    p1.Name PortName,
    s.PrevID_SHIPCALL,
    s.ID_ROUTE,
    l.Name LineName,
    r.ID_VESSEL,
    v.Brutto VesselBrutto,
    v.CallSign VesselCallSign,
    v.Height VesselHeight,
    v.Length VesselLength,
    v.Name VesselName,
    v.Netto VesselNetto,
    v.Width VesselWidth,
    v.ID_PORT VesselID_PORT,
    p2.Name VesselPortName
from 
    ShipCalls s,
    Ports p1,
    Lines l,
    Routes r,
    Vessels v,
    Ports p2
where
    p1.ID_PORT=s.ID_PORT
    and l.ID_LINE=s.ID_LINE
    and r.ID_ROUTE=s.ID_ROUTE
    and r.ID_LINE=s.ID_LINE
    and v.ID_VESSEL=r.ID_VESSEL
    and p2.ID_PORT=v.ID_PORT
");
        List<string> where = new();

        SqlCommand cmd = connection.CreateCommand();
        if (id_line is { })
        {
            where.Add("ID_LINE=@ID_LINE");
            cmd.Parameters.AddWithValue("@ID_LINE", id_line);
        }
        if (id_shipcall is { })
        {
            where.Add("@ID_SHIPCALL=ID_SHIPCALL");
            cmd.Parameters.AddWithValue("@ID_SHIPCALL", id_shipcall);
        }
        if (lineName is { })
        {
            where.Add("LineName like @LineName");
            cmd.Parameters.AddWithValue("@LineName", lineName);
        }
        if (vesselName is { })
        {
            where.Add("VesselName like @VesselName");
            cmd.Parameters.AddWithValue("@VesselName", vesselName);
        }
        if (portName is { })
        {
            where.Add("PortName like @PortName");
            cmd.Parameters.AddWithValue("@PortName", portName);
        }
        if (voyage is { })
        {
            where.Add("Voyage like @Voyage");
            cmd.Parameters.AddWithValue("@Voyage", voyage);
        }
        if (@from is { })
        {
            where.Add("Departure >= @DepartureFrom");
            cmd.Parameters.AddWithValue("@DepartureFrom", @from);
        }
        if (to is { })
        {
            where.Add("Departure < @DepartureTo");
            cmd.Parameters.AddWithValue("@DepartureTo", to);
        }

        if (where.Count > 0)
        {
            sb.Append(" and ").Append(string.Join(" and ", where));
        }

        cmd.CommandText = sb.ToString();

        await t.ConfigureAwait(false);

        using SqlDataReader dr = await cmd.ExecuteReaderAsync();

        while (await dr.ReadAsync())
        {
            yield return (DbDataReader)dr;
        }
    }
}

