using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Npgsql;
using System.Data;

public class History
{
    public string ID { get; set; }
    public string EventDate { get; set; }
    public string Type { get; set; }
    public string Event { get; set; }
    public string IP { get; set; }
    public string EventUser { get; set; }
    public string TypeID { get; set; }
    public string Notes { get; set; }

    public void CreateEvent(History history)
    {
        try
        {
        history.EventDate = DateTime.Now.ToString("MM-dd-yy h:mm:ss tt");

        if(string.IsNullOrEmpty(history.IP))
            history.IP = (string)HttpContext.Current.Session["ip_address"];

        if(string.IsNullOrEmpty(history.EventUser))
            history.EventUser = HttpContext.Current.User.Identity.Name;

            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("history_createevent", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@eventDate", history.EventDate));
                cmd.Parameters.Add(new NpgsqlParameter("@type", history.Type));
                cmd.Parameters.Add(new NpgsqlParameter("@event", history.Event));
                cmd.Parameters.Add(new NpgsqlParameter("@ip", history.IP));
                cmd.Parameters.Add(new NpgsqlParameter("@user", history.EventUser));
                cmd.Parameters.Add(new NpgsqlParameter("@notes", history.Notes));
                cmd.Parameters.Add(new NpgsqlParameter("@typeid", history.TypeID));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }
    }

    public DataTable Read(History history, string limit)
    {
        DataTable table = new DataTable();

        if (limit == "All")
            limit = "9999";
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("history_read", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@historytypeid", history.TypeID));
                cmd.Parameters.Add(new NpgsqlParameter("@historytype", history.Type));
                cmd.Parameters.Add(new NpgsqlParameter("@limit", limit));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read History.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public DataTable ReadUser(History history, string limit, string username)
    {
        DataTable table = new DataTable();

        if (limit == "All")
            limit = "9999";
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("history_readuser", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@historytypeid", history.TypeID));
                cmd.Parameters.Add(new NpgsqlParameter("@historytype", history.Type));
                cmd.Parameters.Add(new NpgsqlParameter("@limit", limit));
                cmd.Parameters.Add(new NpgsqlParameter("@username", username));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read History.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }
}