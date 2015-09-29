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
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using Mono.Unix.Native;
public class Unicast
{
    public string HostKernel { get; set; }
    public string HostBootImage { get; set; }
    public string HostArguments { get; set; }
    public string HostMac { get; set; }
    public string HostName { get; set; }
    public string HostScripts { get; set; }
    public string ImageName { get; set; }
    public string ImageOS { get; set; }

    public void CreateUnicast(string direction, int hostID)
    {
        Task task = new Task();
        Unicast unicast = new Unicast();

        unicast = unicast.Read(hostID, unicast);
        if (unicast.HostName != null)
        {
            if (unicast.ImageName != null)
            {
                string taskID = unicast.Create(unicast);
                if (taskID != "0")
                {
                    if (CreatePxeBoot(unicast, direction, "false", taskID, hostID))
                    {
                        History history = new History();
                        history.Type = "Host";
                        history.Notes = unicast.HostMac;
                        history.TypeID = hostID.ToString();
                        if (direction == "push")
                            history.Event = "Deploy";
                        else                    
                            history.Event = "Upload";
                        history.CreateEvent(history);

                        Image image = new Image();
                        history.Type = "Image";
                        history.Notes = unicast.HostName;
                        history.TypeID = image.GetImageID(unicast.ImageName);
                        if (direction == "push")
                            history.Event = "Deploy";
                        else
                            history.Event = "Upload";

                        history.CreateEvent(history);

                        task.WakeUp(unicast.HostMac);

                    }
                    else
                        unicast.Rollback(unicast);
                }
            }     
        }
    }

    public bool CreatePxeBoot(Unicast unicast, string direction, string isMulticast, string taskID, int hostID)
    {
        Utility settings = new Utility();
        Task task = new Task();
        string pxeHostMac = task.MacToPXE(unicast.HostMac);
        string proxyDHCP = settings.GetSettings("Proxy Dhcp");
        string biosFile = settings.GetSettings("Proxy Bios File");
        string efi32File = settings.GetSettings("Proxy Efi32 File");
        string efi64File = settings.GetSettings("Proxy Efi64 File");
        string biosPath = null;
        string efi32Path = null;
        string efi64Path = null;
        string biosPathipxe = null;
        string efi32Pathipxe = null;
        string efi64Pathipxe = null;
        string path = null;
        string[] lines = null;
        string wds_key = null;
        if (settings.GetSettings("Server Key Mode") == "Automated")
            wds_key = settings.GetSettings("Server Key");
        else
            wds_key = "";

        string mode = settings.GetSettings("PXE Mode");

        if (proxyDHCP == "Yes")
        {
            biosPathipxe = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
            string[] biosLinesipxe = {@"#!ipxe",
                           @"kernel " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/kernels/" + unicast.HostKernel + ".krn" + " initrd=" + unicast.HostBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + unicast.HostArguments,
                           @"imgfetch " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/images/" + unicast.HostBootImage ,
                           @"boot"};

            biosPath = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
            string[] biosLines = { @"DEFAULT cruciblewds",
                           @"LABEL cruciblewds", @"KERNEL kernels" + Path.DirectorySeparatorChar + unicast.HostKernel , 
                           @"APPEND initrd=images" + Path.DirectorySeparatorChar + unicast.HostBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + unicast.HostArguments};


            efi32Pathipxe = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
            string[] efi32Linesipxe = {@"#!ipxe",
                           @"kernel " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/kernels/" + unicast.HostKernel + ".krn" + " initrd=" + unicast.HostBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + unicast.HostArguments,
                           @"imgfetch " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/images/" + unicast.HostBootImage ,
                           @"boot"};

            efi32Path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
            string[] efi32Lines = { @"DEFAULT cruciblewds",
                           @"LABEL cruciblewds", @"KERNEL kernels" + Path.DirectorySeparatorChar + unicast.HostKernel , 
                           @"APPEND initrd=images" + Path.DirectorySeparatorChar + unicast.HostBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + unicast.HostArguments};


            efi64Pathipxe = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
            string[] efi64Linesipxe = {@"#!ipxe",
                           @"kernel " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/kernels/" + unicast.HostKernel + ".krn" + " initrd=" + unicast.HostBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + unicast.HostArguments,
                           @"imgfetch " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/images/" + unicast.HostBootImage ,
                           @"boot"};

            efi64Path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
            string[] efi64Lines = { @"DEFAULT cruciblewds",
                           @"LABEL cruciblewds", @"KERNEL kernels" + Path.DirectorySeparatorChar + unicast.HostKernel , 
                           @"APPEND initrd=images" + Path.DirectorySeparatorChar + unicast.HostBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + unicast.HostArguments};


            if (File.Exists(biosPath))
            {
                Host hostFunction = new Host();
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(pxeHostMac)))
                {
                     Utility.MoveFile(biosPath, biosPath + ".custom");
                }
                else
                {
                    Utility.Message = "The PXE File Already Exists";
                    return false;
                }
            }

            if (File.Exists(biosPathipxe))
            {
                Host hostFunction = new Host();
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(pxeHostMac)))
                {
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom");
                }
                else
                {
                    Utility.Message = "The PXE File Already Exists";
                    return false;
                }
            }

            if (File.Exists(efi32Path))
            {
                Host hostFunction = new Host();
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(pxeHostMac)))
                {
                        Utility.MoveFile(efi32Path, efi32Path + ".custom");
                }
                else
                {
                    Utility.Message = "The PXE File Already Exists";
                    return false;
                }
            }

            if (File.Exists(efi32Pathipxe))
            {
                Host hostFunction = new Host();
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(pxeHostMac)))
                {
                
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom");
                }
                else
                {
                    Utility.Message = "The PXE File Already Exists";
                    return false;
                }
            }

            if (File.Exists(efi64Path))
            {
                Host hostFunction = new Host();
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(pxeHostMac)))
                {
                        Utility.MoveFile(efi64Path, efi64Path + ".custom");     
                }
                else
                {
                    Utility.Message = "The PXE File Already Exists";
                    return false;
                }
            }

            if (File.Exists(efi64Pathipxe))
            {
                Host hostFunction = new Host();
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(pxeHostMac)))
                {

                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom");
                }
                else
                {
                    Utility.Message = "The PXE File Already Exists";
                    return false;
                }
            }

            try
            {
                System.IO.File.WriteAllLines(biosPath, biosLines);
                if (Environment.OSVersion.ToString().Contains("Unix"))
                    Syscall.chmod(biosPath, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));

                System.IO.File.WriteAllLines(efi32Path, efi32Lines);
                if (Environment.OSVersion.ToString().Contains("Unix"))
                    Syscall.chmod(efi32Path, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));

                System.IO.File.WriteAllLines(efi64Path, efi64Lines);
                if (Environment.OSVersion.ToString().Contains("Unix"))
                    Syscall.chmod(efi64Path, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                
                System.IO.File.WriteAllLines(biosPathipxe, biosLinesipxe);
                if (Environment.OSVersion.ToString().Contains("Unix"))
                    Syscall.chmod(biosPathipxe, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));

                System.IO.File.WriteAllLines(efi32Pathipxe, efi32Linesipxe);
                if (Environment.OSVersion.ToString().Contains("Unix"))
                    Syscall.chmod(efi32Pathipxe, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));

                System.IO.File.WriteAllLines(efi64Pathipxe, efi64Linesipxe);
                if (Environment.OSVersion.ToString().Contains("Unix"))
                    Syscall.chmod(efi64Pathipxe, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));

            }

            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Utility.Message = "Could Not Create PXE File";
                return false;
            }

        }
        else
        {
            if (mode == "pxelinux" || mode == "syslinux_32_efi" || mode == "syslinux_64_efi")
            {
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
                string[] tmplines = { @"DEFAULT cruciblewds",
                           @"LABEL cruciblewds", @"KERNEL " + "kernels" + Path.DirectorySeparatorChar + unicast.HostKernel , 
                           @"APPEND initrd=" + "images" + Path.DirectorySeparatorChar + unicast.HostBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + unicast.HostArguments};
                lines = tmplines;
            }

            else if (mode.Contains("ipxe"))
            {
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";

                string[] tmplines = {@"#!ipxe",
                           @"kernel " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/kernels/" + unicast.HostKernel + ".krn" + " initrd=" + unicast.HostBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + unicast.HostArguments,
                           @"imgfetch " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/images/" + unicast.HostBootImage ,
                           @"boot"};
                lines = tmplines;
            }
            else
            {
                Utility.Message = "PXE Mode Is Not Set Correctly";
                return false;
            }


            if (File.Exists(path))
            {
                Host hostFunction = new Host();
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(pxeHostMac)))
                {
                    if (mode.Contains("ipxe"))
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe", settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom");
                    else
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac, settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom");
                }
                else
                {
                    Utility.Message = "The PXE File Already Exists";
                    return false;
                }
            }

            try
            {
                System.IO.File.WriteAllLines(path, lines);
                if (Environment.OSVersion.ToString().Contains("Unix"))
                {
                    Syscall.chmod(path, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Utility.Message = "Could Not Create PXE File";
                return false;
            }
        }

        try
        {
            string storagePath = null;
            string xferMode = settings.GetSettings("Image Transfer Mode");
            if (xferMode == "smb" || xferMode == "smb+http")
                 storagePath = "SMB Path";
            else
            {
                 if (direction == "pull")
                      storagePath = "Nfs Upload Path";
                 else
                      storagePath = "Nfs Deploy Path";
            }
            string hostArgs = "imgName=" + unicast.ImageName + " storage=" + settings.GetSettings(storagePath) + " hostID=" + hostID +
                              " imgOS=" + unicast.ImageOS + " multicast=" + isMulticast + " hostScripts=" + unicast.HostScripts + " xferMode=" + xferMode + " serverIP=" + settings.GetSettings("Server IP") +
                              " hostName=" + unicast.HostName + " compAlg=" + settings.GetSettings("Compression Algorithm") + " compLevel=-" + settings.GetSettings("Compression Level");

            if(direction == "pull" && xferMode == "udp+http")
            {
                int portBase = task.GetPort();
                hostArgs = hostArgs + " portBase=" + portBase;
            }

            if (task.CreateTaskArgs(hostArgs,taskID))
                return true;
            else
                return false;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Create PXE File";
            return false;
        }

    }

    public DataTable Confirm(string hostID)
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("unicast_confirm", conn);
                cmd.Parameters.Add(new NpgsqlParameter("@hostID", hostID));
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Confirmation Data.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString()); 
        }
        return table;
    }

    public Unicast Read(int hostID, Unicast unicast)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlDataReader rdr = null;
                NpgsqlCommand cmd = new NpgsqlCommand("unicast_read", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@hostID", hostID));

                conn.Open();
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    unicast.ImageName = (string)rdr["_imagename"];
                    unicast.ImageOS = (string)rdr["_imageos"];
                    unicast.HostName = (string)rdr["_hostname"];
                    unicast.HostKernel = (string)rdr["_hostkernel"];
                    unicast.HostBootImage = (string)rdr["_hostbootimage"];
                    unicast.HostArguments = (string)rdr["_hostarguments"];
                    unicast.HostMac = (string)rdr["_hostmac"];
                    unicast.HostScripts = rdr["_hostscripts"].ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Unicast Info.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString()); 
        }
        if(unicast.ImageName == null)
            Utility.Message = "The Image No Longer Exists";
       return unicast;
    }

    public string Create(Unicast unicast)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("unicast_create", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@taskName", unicast.HostName));
                conn.Open();
                string result = cmd.ExecuteScalar() as string;
                if (result != "0")
                    Utility.Message += "Successfully Started Task " + unicast.HostName + "<br>";
                else
                    Utility.Message += "This Host Is Already Part Of An Active Task <br>";
                return result;
            }
        }
        catch (Exception ex)
        {
            Utility.Message += "Could Not Create Unicast.  Check The Exception Log For More Info <br>";
            Logger.Log(ex.ToString()); 
            return "0";
        }
    }

    public void Rollback(Unicast unicast)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("unicast_rollback", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@taskName", unicast.HostName));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }

        Utility settings = new Utility();
        Task task = new Task();
        string pxeHostMac = task.MacToPXE(unicast.HostMac);

        Host hostFunction = new Host();

        if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(pxeHostMac)))
        {
            string mode = settings.GetSettings("PXE Mode");
            string proxyDHCP = settings.GetSettings("Proxy Dhcp");
            if (proxyDHCP == "Yes")
            {
                try
                {
                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");
                }
                catch { }
                try
                {
                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                }
                catch { }
                try
                {
                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");
                }
                catch { }
                try
                {
                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                }
                catch { }
                try
                {
                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");
                }
                catch { }
                try
                {
                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                }
                catch { }
            }
            else
            {
                try
                {
                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom", settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");
                }
                catch { }
                try
                {
                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom", settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                }
                catch { }
            }
        }
    }

    public List<int> UnicastFromGroup(int groupID)
    {
        var listHostID = new List<int>();

        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
        {
            NpgsqlCommand cmd = new NpgsqlCommand("unicast_createfromgroup", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new NpgsqlParameter("@groupID", groupID));
            conn.Open();
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
                listHostID.Add((int)rdr["unicast_createfromgroup"]);
        }
        
        return listHostID;
    }
}