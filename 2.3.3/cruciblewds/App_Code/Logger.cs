/*  
    CrucibleWDS A Windows Deployment Solution
    Copyright (C) 2011  Jon Dolny

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Npgsql;

public class Logger
{

    public static void Log(string message)
    {
        string logPath = null;
        try
        {
            logPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar + "exceptions.log";
            File.AppendAllText(logPath, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ":\t" + message + Environment.NewLine);
        }
        catch { }
    }

    public void LogActivity(string userName, string activity, string details)
    {
           
        string date = DateTime.Now.ToString("MM-dd-yy h:mm tt");
 
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("logs_useractivity", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@userName", userName));
                cmd.Parameters.Add(new NpgsqlParameter("@logDate", date));
                cmd.Parameters.Add(new NpgsqlParameter("@activity", activity));
                cmd.Parameters.Add(new NpgsqlParameter("@details", details));
                conn.Open();
                cmd.ExecuteNonQuery();            
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }
    }

    public static List<string> ViewLog(string log, string limit)
    {
        if (limit == "All")
            limit = "9999";
        List<string> text = new List<string>();
        string logPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar;

        try
        {
            text = File.ReadLines(logPath + log).Reverse().Take(Convert.ToInt16(limit)).Reverse().ToList();
        }
        catch (Exception ex)
        {
            Utility.Message = ex.Message.Replace("\\","\\\\");
        }

        return text;
    }
}
