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
using System.Security.Cryptography;
using System.Web.Security;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using System.IO;

public class WDSUser
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string Salt { get; set; }
    public string Membership { get; set; }
    public string GroupManagement { get; set; }
    public string OndAccess { get; set; }
    public string DebugAccess { get; set; }
    public string DiagAccess { get; set; }

    public string CreateSalt(int byteSize)
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] buff = new byte[byteSize];
        rng.GetBytes(buff);
        return Convert.ToBase64String(buff);
    }

    public string CreatePasswordHash(string pwd, string salt)
    {
        string saltAndPwd = String.Concat(pwd, salt);
        string hashedPwd = FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, "sha1");
        return hashedPwd;
    }

    public void Create(WDSUser user)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("users_create", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@userName", user.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@userPwd", user.CreatePasswordHash(user.Password, user.Salt)));
                cmd.Parameters.Add(new NpgsqlParameter("@userSalt", user.Salt));
                cmd.Parameters.Add(new NpgsqlParameter("@userMembership", user.Membership));
                cmd.Parameters.Add(new NpgsqlParameter("@groupManagement", user.GroupManagement));
                cmd.Parameters.Add(new NpgsqlParameter("@ondAccess", user.OndAccess));
                cmd.Parameters.Add(new NpgsqlParameter("@debugAccess", user.DebugAccess));
                cmd.Parameters.Add(new NpgsqlParameter("@diagAccess", user.DiagAccess));
                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;

                if (Utility.Message.Contains("Successfully"))
                {
                    History history = new History();
                    history.Event = "Create";
                    history.Type = "User";
                    history.TypeID = user.GetID(user.Name);
                    history.CreateEvent(history);
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Create User.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());      
        }
    }

    public void Delete(List<int> listDelete)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();
                for (int i = 0; i < listDelete.Count; i++)
                {
                    WDSUser user = new WDSUser();
                    user.ID = listDelete[i].ToString();
                    user = user.Read(user);

                    NpgsqlCommand cmd = new NpgsqlCommand("users_delete", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new NpgsqlParameter("@userID", listDelete[i]));
                    cmd.ExecuteNonQuery();

                    History history = new History();
                    history.Event = "Delete";
                    history.Type = "User";
                    history.TypeID = user.ID;
                    history.CreateEvent(history);

                }
                Utility.Message = "Successfully Deleted User(s)";
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Delete User.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString()); 
        }
    }

    public int GetAdminCount()
    {
        int result = 1;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("users_getadmincount", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@result", NpgsqlDbType.Integer));
                cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                conn.Open();
                cmd.ExecuteNonQuery();
                result = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString()); 
        }
        return result;
    }

    public string GetTotalCount()
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();

                NpgsqlCommand cmd = new NpgsqlCommand("users_totalcount", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                return cmd.ExecuteScalar().ToString();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return "Error";
        }
    }

    public void Import()
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("users_import", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@inpath", HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "csvupload" + Path.DirectorySeparatorChar));
                cmd.Parameters.Add(new NpgsqlParameter("@result", NpgsqlDbType.Char, 100));
                cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                conn.Open();
                cmd.ExecuteNonQuery();
                Utility.Message = cmd.Parameters["@result"].Value.ToString() + " Users(s) Imported Successfully";
            }

        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Import Users.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString()); 
        }
    }

    public WDSUser Read(WDSUser user)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("users_read", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@userID", user.ID));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    user.Name = (string)rdr["username"];
                    user.Membership = (string)rdr["usermembership"];
                    user.GroupManagement = rdr["groupmanagement"].ToString();
                    user.OndAccess = rdr["allowond"].ToString();
                    user.DebugAccess = rdr["allowdebug"].ToString();
                    user.DiagAccess = rdr["allowdiag"].ToString();
                }
            }         
        }
        catch (Exception ex)
        {
            
            Logger.Log(ex.ToString()); 
        }
        return user;
    }

    public DataTable Search(string searchString)
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("users_search", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@searchString", searchString));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Search Users.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public void Update(WDSUser user, string userID)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("users_update", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@userID", userID));
                cmd.Parameters.Add(new NpgsqlParameter("@userName", user.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@userPwd", user.CreatePasswordHash(user.Password, user.Salt)));
                cmd.Parameters.Add(new NpgsqlParameter("@userSalt", user.Salt));
                cmd.Parameters.Add(new NpgsqlParameter("@userMembership", user.Membership));
                cmd.Parameters.Add(new NpgsqlParameter("@groupManagement", user.GroupManagement));
                cmd.Parameters.Add(new NpgsqlParameter("@ondAccess", user.OndAccess));
                cmd.Parameters.Add(new NpgsqlParameter("@debugAccess", user.DebugAccess));
                cmd.Parameters.Add(new NpgsqlParameter("@diagAccess", user.DiagAccess));
                
                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;

                if (Utility.Message.Contains("Successfully"))
                {
                    History history = new History();
                    history.Event = "Edit";
                    history.Type = "User";
                    history.TypeID = user.ID;
                    history.CreateEvent(history);
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update User.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString()); 
        }
    }

    public void UpdateNoPass(WDSUser user, string userID)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("users_update_nopass", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@userID", userID));
                cmd.Parameters.Add(new NpgsqlParameter("@userName", user.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@userMembership", user.Membership));
                cmd.Parameters.Add(new NpgsqlParameter("@groupManagement", user.GroupManagement));
                cmd.Parameters.Add(new NpgsqlParameter("@ondAccess", user.OndAccess));
                cmd.Parameters.Add(new NpgsqlParameter("@debugAccess", user.DebugAccess));
                cmd.Parameters.Add(new NpgsqlParameter("@diagAccess", user.DiagAccess));
                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update User.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public string GetID(string userName)
    {
        string id = null;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("users_getid", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@userName", userName));
                conn.Open();
                id = cmd.ExecuteScalar().ToString();
               
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read User Info.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return id;
    }
}