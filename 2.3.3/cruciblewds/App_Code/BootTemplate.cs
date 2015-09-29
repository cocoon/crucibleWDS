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
using System.Diagnostics;
using System.Threading;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using Mono.Unix.Native;
public class BootTemplate
{
    public int templateID { get; set; }
    public string templateName { get; set; }
    public string templateContent { get; set; }

    public void Create(BootTemplate template)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("boottemplates_create", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@templateName", template.templateName));
                cmd.Parameters.Add(new NpgsqlParameter("@templatecontent", template.templateContent));
                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;
            }
        }

        catch (Exception ex)
        {
            Utility.Message = "Could Not Create Template.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public BootTemplate Read(string templateName)
    {
        BootTemplate template = new BootTemplate();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("boottemplates_read", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@templateName", templateName));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    template.templateName = (string)rdr["templatename"];
                    template.templateContent = (string)rdr["templatecontent"];

                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Boot Menu.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return template;

    }

    public void Update(BootTemplate template)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("boottemplates_update", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@templateName", template.templateName));
                cmd.Parameters.Add(new NpgsqlParameter("@templateContent", template.templateContent));
                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update Template.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public void Delete(string templateName)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();

                NpgsqlCommand cmd = new NpgsqlCommand("boottemplates_delete", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@templateName", templateName));
                cmd.ExecuteNonQuery();
                Utility.Message = "Successfully Deleted Template";
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Delete Template.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }
}