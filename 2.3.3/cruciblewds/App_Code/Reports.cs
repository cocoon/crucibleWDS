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
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Npgsql;

public class Reports
{

    public DataTable LastUsers()
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("reports_lastusers", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }

        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Report Data.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
        
    }
    
    public DataTable UserStats()
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("reports_userstats", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Report Data.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public DataTable LastUnicasts()
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("reports_lastunicasts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Report Data.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public DataTable LastMulticasts()
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("reports_lastmulticasts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Report Data.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public DataTable TopFiveUnicast()
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("reports_topunicast", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Report Data.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public DataTable TopFiveMulticast()
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("reports_topmulticast", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Report Data.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }
}