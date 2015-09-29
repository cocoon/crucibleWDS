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
using Mono.Unix.Native;

public class Host
{
    public string ID {get; set;}
    public string Name { get; set; }
    public string Mac { get; set; }
    public string Image { get; set; }
    public string Group { get; set; }
    public string Description { get; set; }
    public string Kernel { get; set; }
    public string BootImage { get; set; }
    public string Args { get; set; }
    public string Scripts { get; set; }

    public void Create(Host host)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_create", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@hostName", host.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@hostMac", host.Mac));
                cmd.Parameters.Add(new NpgsqlParameter("@hostImage", host.Image));
                cmd.Parameters.Add(new NpgsqlParameter("@hostGroup", host.Group));
                cmd.Parameters.Add(new NpgsqlParameter("@hostDesc", host.Description));
                cmd.Parameters.Add(new NpgsqlParameter("@hostKernel", host.Kernel));
                cmd.Parameters.Add(new NpgsqlParameter("@hostBootImage", host.BootImage));
                cmd.Parameters.Add(new NpgsqlParameter("@hostArguments", host.Args));
                cmd.Parameters.Add(new NpgsqlParameter("@hostScripts", host.Scripts));
                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;

                if (Utility.Message.Contains("Successfully"))
                {
                    History history = new History();
                    history.Event = "Create";
                    history.Type = "Host";
                    history.Notes = host.Mac;
                    history.TypeID = host.GetHostID(host.Mac);
                    history.CreateEvent(history);
                }
            }
        }

        catch (Exception ex)
        {
            Utility.Message = "Could Not Create Host.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public string CheckActive(string mac)
    {
        string result = null;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_checkactive", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@mac", mac));
                conn.Open();
                result = cmd.ExecuteScalar() as string;
            }
        }
        catch (Exception ex)
        {
            result = "Could Not Check Active Status.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return result;
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
                    Host host = new Host();
                    host.ID = listDelete[i].ToString();
                    host = host.Read(host);

                    NpgsqlCommand cmd = new NpgsqlCommand("hosts_delete", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new NpgsqlParameter("@hostID", listDelete[i]));
                    cmd.ExecuteNonQuery();

                    History history = new History();
                    history.Event = "Delete";
                    history.Type = "Host";
                    history.Notes = host.Name;
                    history.TypeID = host.ID;
                    history.CreateEvent(history);
                }
                Utility.Message =  "Successfully Deleted Host(s)";
                
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Delete Host.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public string GetHostID(string mac)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_readhostid", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@hostMac", mac));
                return cmd.ExecuteScalar().ToString();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return "error";
        }
    }

    public string GetTotalCount()
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_totalcount", conn);
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
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_import", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@inpath", HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "csvupload" + Path.DirectorySeparatorChar));
                cmd.Parameters.Add(new NpgsqlParameter("@result", NpgsqlDbType.Char, 100));
                cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                conn.Open();
                cmd.ExecuteNonQuery();
                Utility.Message = cmd.Parameters["@result"].Value.ToString() + " Host(s) Imported Successfully";
            }
            
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Import Hosts.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public Host Read(Host host)
    {  
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_read", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@hostID", host.ID));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    host.Name = (string)rdr["hostname"];
                    host.Mac = (string)rdr["hostmac"];
                    host.Image = (string)rdr["hostimage"];
                    host.Group = (string)rdr["hostgroup"];
                    host.Description = (string)rdr["hostdesc"];
                    host.Kernel = (string)rdr["hostkernel"];
                    host.BootImage = (string)rdr["hostbootimage"];
                    host.Args = (string)rdr["hostarguments"];
                    host.Scripts = rdr["hostscripts"].ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Host Info.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return host;
      
    }

    public DataTable Search(string searchString)
    {
        DataTable table = new DataTable();

        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_search", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@searchString", searchString));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Search Hosts.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public DataTable SearchLimited(string searchString, string group)
    {
        DataTable table = new DataTable();

        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_search_limited", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@searchString", searchString));
                cmd.Parameters.Add(new NpgsqlParameter("@groupString", group));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Search Hosts.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public void Update(Host host)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_update", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@hostID", host.ID));
                cmd.Parameters.Add(new NpgsqlParameter("@hostName", host.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@hostMac", host.Mac));
                cmd.Parameters.Add(new NpgsqlParameter("@hostImage", host.Image));
                cmd.Parameters.Add(new NpgsqlParameter("@hostGroup", host.Group));
                cmd.Parameters.Add(new NpgsqlParameter("@hostDesc", host.Description));
                cmd.Parameters.Add(new NpgsqlParameter("@hostKernel", host.Kernel));
                cmd.Parameters.Add(new NpgsqlParameter("@hostBootImage", host.BootImage));
                cmd.Parameters.Add(new NpgsqlParameter("@hostArguments", host.Args));
                cmd.Parameters.Add(new NpgsqlParameter("@hostScripts", host.Scripts));
                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;

                if (Utility.Message.Contains("Successfully"))
                {
                    History history = new History();
                    history.Event = "Edit";
                    history.Type = "Host";
                    history.Notes = host.Mac;
                    history.TypeID = host.ID;
                    history.CreateEvent(history);
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update Host.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public void SetCustomBootMenu(Host host, string filename)
    {
        Utility settings = new Utility();
        Task task = new Task();
        string mode = settings.GetSettings("PXE Mode");
        string pxeHostMac = task.MacToPXE(host.Mac);
        string isActive = host.CheckActive(host.Mac);
        string path = null;

        string proxyDHCP = settings.GetSettings("Proxy Dhcp");

        if (isActive == "Inactive")
        {
            if (proxyDHCP == "Yes")
            {
                string biosFile = settings.GetSettings("Proxy Bios File");
                string efi32File = settings.GetSettings("Proxy Efi32 File");
                string efi64File = settings.GetSettings("Proxy Efi64 File");



                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                WritePath(path, filename);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;

                WritePath(path, filename);



                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                WritePath(path, filename);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;

                WritePath(path, filename);


                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                WritePath(path, filename);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;

                WritePath(path, filename);
            }
            else
            {
                if (mode.Contains("ipxe"))
                    path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                else
                    path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;

                WritePath(path, filename);
            }
        }
        else
        {
            if (proxyDHCP == "Yes")
            {
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom";
                WritePath(path, filename);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom";
                WritePath(path, filename);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom";
                WritePath(path, filename);

                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom";
                WritePath(path, filename);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom";
                WritePath(path, filename);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom";
                WritePath(path, filename);
            }
            else
            {
                if(mode.Contains("ipxe"))
                    path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom";
                else
                    path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom";
                WritePath(path, filename);
            }
        }

        try
        {
           
            host.CustomBoot(host.Mac, true);
            History history = new History();
            history.Event = "Set Boot Menu";
            history.Type = "Host";
            history.Notes = host.Mac;
            history.TypeID = host.ID;
            history.CreateEvent(history);
            Utility.Message ="Successfully Set Custom Boot Menu For This Host";
        }

        catch (Exception ex)
        {
            Utility.Message = "Could Not Set Custom Boot Menu.  Check The Exception Log For More Info.";
            Logger.Log(ex.Message);
        }
    }

    public bool WritePath(string path, string contents)
    {
        try
        {
            using (StreamWriter file = new StreamWriter(path))
            {
                file.WriteLine(contents);
                file.Close();
            }
            if (Environment.OSVersion.ToString().Contains("Unix"))
            {
                Syscall.chmod(path, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
            }
            return true;
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Set Custom Boot Menu.  Check The Exception Log For More Info.";
            Logger.Log(ex.Message);
            return false;
        }
    }

    public bool DeletePath(string path)
    {
        try
        {
            File.Delete(path);
            return true;
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Remove Custom Boot Menu.  Check The Exception Log For More Info.";
            Logger.Log(ex.Message);
            return false;
        }
    }
    public void RemoveCustomBootMenu(Host host)
    {
        Utility settings = new Utility();
        Task task = new Task();
        string mode = settings.GetSettings("PXE Mode");
        string pxeHostMac = task.MacToPXE(host.Mac);
        string isActive = host.CheckActive(host.Mac);
        string path = null;

        string proxyDHCP = settings.GetSettings("Proxy Dhcp");

        if (isActive == "Inactive")
        {
            if (proxyDHCP == "Yes")
            {
                string biosFile = settings.GetSettings("Proxy Bios File");
                string efi32File = settings.GetSettings("Proxy Efi32 File");
                string efi64File = settings.GetSettings("Proxy Efi64 File");



                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                DeletePath(path);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;

                DeletePath(path);



                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                DeletePath(path);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;

                DeletePath(path);


                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                DeletePath(path);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;

                DeletePath(path);
            }
            else
            {
                if (mode.Contains("ipxe"))
                    path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                else
                    path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
                DeletePath(path);
            }
        }
        else
        {
            if (proxyDHCP == "Yes")
            {
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom";
                DeletePath(path);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom";
                DeletePath(path);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom";
                DeletePath(path);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom";
                DeletePath(path);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom";
                DeletePath(path);
                path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom";
                DeletePath(path);
            }
            else
            {
                if(mode.Contains("ipxe"))
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom";
                else
                    path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom";
                DeletePath(path);
            }
        }

        try
        {
            host.CustomBoot(host.Mac, false);
            History history = new History();
            history.Event = "Remove Boot Menu";
            history.Type = "Host";
            history.Notes = host.Mac;
            history.TypeID = host.ID;

            history.CreateEvent(history);
            Utility.Message = "Successfully Removed Custom Boot Menu For This Host";
        }

        catch (Exception ex)
        {
            Utility.Message = "Could Not Remove Custom Boot Menu.  Check The Exception Log For More Info.";
            Logger.Log(ex.Message);
        }
    }
    public void CustomBoot(string hostMac, bool enable)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd;
                if(enable)
                    cmd = new NpgsqlCommand("UPDATE hosts SET custombootenabled = '1' WHERE hostmac=@hostMac;", conn);
                else
                    cmd = new NpgsqlCommand("UPDATE hosts SET custombootenabled = '0' WHERE hostmac=@hostMac;", conn);
                cmd.Parameters.Add(new NpgsqlParameter("@hostMac", hostMac));
                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update Host Boot Template Status.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public bool IsCustomBootEnabled(string hostMac)
    {
        string status = null;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("SELECT custombootenabled FROM hosts WHERE hostmac=@hostMac;", conn);
                cmd.Parameters.Add(new NpgsqlParameter("@hostMac", hostMac));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    status = (string)rdr["custombootenabled"];
                }
            }
            if (status == "1")
                return true;
            else
                return false;
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Boot Template Status.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
            return false;
        }
    }

    public DataTable TableForUser(string search)
    {
        Host host = new Host();
        WDSUser user = new WDSUser();
        user.ID = user.GetID(HttpContext.Current.User.Identity.Name);
        user = user.Read(user);
        DataTable table = new DataTable();

        if (!string.IsNullOrEmpty(user.GroupManagement))
        {
             List<string> listManagementGroups = user.GroupManagement.Split(' ').ToList<string>();

             

             foreach (string id in listManagementGroups)
             {
                  Group mgmtgroup = new Group();
                  mgmtgroup.ID = id;
                  mgmtgroup = mgmtgroup.Read(mgmtgroup);

                  table.Merge(host.SearchLimited(search, mgmtgroup.Name));
             }

             if (table.Rows.Count > 0)
             {
                  DataView dtview = new DataView(table);
                  dtview.Sort = "hostName asc";
                  table = dtview.ToTable();
             }
        }

        return table;
    }
}