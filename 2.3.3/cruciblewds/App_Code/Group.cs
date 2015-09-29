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
using System.Text;
using Npgsql;
using NpgsqlTypes;
using System.IO;

public class Group
{
    public string ID { get; set; }
    public string Name;
    public string Image;
    public string Description;
    public string Kernel;
    public string BootImage;
    public string Args;
    public string SenderArgs;
    public string Scripts;

    public void Create(Group group, List<int> members)
    {
        bool result = false;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("groups_create", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupName", group.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@groupDesc", group.Description));
                cmd.Parameters.Add(new NpgsqlParameter("@groupImage", group.Image));
                cmd.Parameters.Add(new NpgsqlParameter("@groupKernel", group.Kernel));
                cmd.Parameters.Add(new NpgsqlParameter("@groupBootImage", group.BootImage));
                cmd.Parameters.Add(new NpgsqlParameter("@groupArguments", group.Args));
                cmd.Parameters.Add(new NpgsqlParameter("@groupSenderArgs", group.SenderArgs));
                cmd.Parameters.Add(new NpgsqlParameter("@groupScripts", group.Scripts));
                conn.Open();
                result = Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Create Group.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }

        if (result)
        {
            History history = new History();
            history.Event = "Create";
            history.Type = "Group";
            history.TypeID = group.GetGroupID(group.Name);
            history.CreateEvent(history);

            if (UpdateHosts(group, members, true))
                Utility.Message = "Successfully Created Group " + group.Name + " With " + members.Count + " Hosts";
        }
        else
            Utility.Message = "The Group " + group.Name + " Already Exists";
    }

    public DataTable CurrentMembers(string groupName)
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("groups_currentmembers", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupName", groupName));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }      
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Current Group Members.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
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
                    Group group = new Group();
                    group.ID = listDelete[i].ToString();
                    group = group.Read(group);

                    NpgsqlCommand cmd = new NpgsqlCommand("groups_delete", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new NpgsqlParameter("@groupID", listDelete[i]));
                    cmd.ExecuteNonQuery();

                    History history = new History();
                    history.Event = "Delete";
                    history.Type = "Group";
                    history.TypeID = group.ID;
                    history.CreateEvent(history);
                }

                Utility.Message = "Successfully Deleted Group(s)";
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Delete Group(s).  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public string GetGroupID(string groupName)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand("groups_readgroupid", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupName", groupName));
                return cmd.ExecuteScalar().ToString();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return "error";
        }
    }

    public string GetMemberCount(string groupName)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();

                NpgsqlCommand cmd = new NpgsqlCommand("groups_membercount", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupName", groupName));
                return cmd.ExecuteScalar().ToString();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return "Error";
        }
    }

    public List<string> GetMemberIDs(string groupName)
    {
        List<string> hostIDs = new List<string>();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("groups_getmemberids", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupName", groupName));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    hostIDs.Add(rdr["groups_getmemberids"].ToString());
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Current Group Members.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return hostIDs;
    }

    public string GetTotalCount()
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();

                NpgsqlCommand cmd = new NpgsqlCommand("groups_totalcount", conn);
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
                NpgsqlCommand cmd = new NpgsqlCommand("groups_import", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@inpath", HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "csvupload" + Path.DirectorySeparatorChar));
                cmd.Parameters.Add(new NpgsqlParameter("@result", NpgsqlDbType.Char, 100));
                cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                conn.Open();
                cmd.ExecuteNonQuery();
                Utility.Message = cmd.Parameters["@result"].Value.ToString() + " Group(s) Imported Successfully";
            }
            
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Import Groups.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }
    

    public Group Read(Group group)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("groups_read", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupID", group.ID));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    group.Name = (string)rdr["groupName"];
                    group.Image = (string)rdr["groupImage"];
                    group.Description = (string)rdr["groupDesc"];
                    group.Kernel = (string)rdr["groupKernel"];
                    group.BootImage = (string)rdr["groupBootImage"];
                    group.Args = (string)rdr["groupArguments"];
                    group.SenderArgs = (string)rdr["groupSenderArgs"];
                    group.Scripts = rdr["groupScripts"].ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Group Info.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return group;
    }

    public void RemoveGroupHosts(Group group, List<int> members)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("groups_deletehosts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@hostID", NpgsqlDbType.Integer));

                conn.Open();
                for (int i = 0; i < members.Count; i++)
                {
                    cmd.Parameters["@hostID"].Value = members[i];
                    cmd.ExecuteNonQuery();
                }
                Utility.Message += "Successfully Removed " + members.Count + " Host(s) From " + group.Name + "";
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Remove Hosts From Group.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public DataTable Search(string searchString)
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("groups_search", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@searchString", searchString));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Search Groups.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public bool Update(Group group)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("groups_update", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupID", group.ID));
                cmd.Parameters.Add(new NpgsqlParameter("@groupName", group.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@groupDesc", group.Description));
                cmd.Parameters.Add(new NpgsqlParameter("@groupImage", group.Image));
                cmd.Parameters.Add(new NpgsqlParameter("@groupKernel", group.Kernel));
                cmd.Parameters.Add(new NpgsqlParameter("@groupBootImage", group.BootImage));
                cmd.Parameters.Add(new NpgsqlParameter("@groupArguments", group.Args));
                cmd.Parameters.Add(new NpgsqlParameter("@groupSenderArgs", group.SenderArgs));             
                cmd.Parameters.Add(new NpgsqlParameter("@groupScripts", group.Scripts));                        
                conn.Open();
                cmd.ExecuteNonQuery();
                Utility.Message = "Successfully Updated Group Information <br>";

                History history = new History();
                history.Event = "Edit";
                history.Type = "Group";
                history.TypeID = group.ID;
                history.CreateEvent(history);
                return true;
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update Group.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
            return false;
        }
    }

    public bool UpdateHosts(Group group, List<int> members, bool isAdd)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("groups_updatehosts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupName", group.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@groupImage", group.Image));
                cmd.Parameters.Add(new NpgsqlParameter("@groupKernel", group.Kernel));
                cmd.Parameters.Add(new NpgsqlParameter("@groupBootImage", group.BootImage));
                cmd.Parameters.Add(new NpgsqlParameter("@groupArguments", group.Args));
                cmd.Parameters.Add(new NpgsqlParameter("@groupScripts", group.Scripts));
                cmd.Parameters.Add(new NpgsqlParameter("@hostID", NpgsqlDbType.Integer));
                conn.Open();
                for (int i = 0; i < members.Count; i++)
                {
                    cmd.Parameters["@hostID"].Value = members[i];
                    cmd.ExecuteNonQuery();

                    History history = new History();
                    history.Event = "Edit";
                    history.Type = "Host";
                    history.Notes = "Via Group Update " + group.Name;
                    history.EventUser = HttpContext.Current.User.Identity.Name;
                    history.TypeID = members[i].ToString();
                    history.CreateEvent(history);
                }

                
            }
            if (isAdd)
                Utility.Message += "Successfully Added " + members.Count + " Host(s) To " + group.Name + "<br>";
                
            return true;
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update Group Hosts.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
            return false;
        }
    }

    public DataTable TableForUser(string search)
    {
        Group group = new Group();
        WDSUser user = new WDSUser();
        user.ID = user.GetID(HttpContext.Current.User.Identity.Name);
        user = user.Read(user);
        DataTable table = group.Search(search);


        if (!string.IsNullOrEmpty(user.GroupManagement))
        {
             List<string> listManagementGroups = user.GroupManagement.Split(' ').ToList<string>();



             List<string> groupNames = new List<string>();
             foreach (string id in listManagementGroups)
             {
                  group.ID = id;
                  group = group.Read(group);
                  groupNames.Add(group.Name);
             }
             foreach (DataRow row in table.Rows)
             {
                  if (!groupNames.Contains(row["groupname"].ToString()))
                       row.Delete();
             }
        }
        else
        {
             foreach (DataRow row in table.Rows)
             {
               row.Delete();
             }
        }

        return table;
    }
}