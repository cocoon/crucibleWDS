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
using Newtonsoft.Json;

public class Multicast
{
    public List<string> HostNames { get; set; }
    public List<string> HostMacs { get; set; }
    public List<string> HostImages { get; set; }
    public List<string> HostKernels { get; set; }
    public List<string> HostBootImages { get; set; }
    public List<string> HostArguments { get; set; }
    public List<string> HostIDs { get; set; }
    public List<string> TaskIDs { get; set; }
    public List<bool> PXEBoot { get; set; }

    public string ImageOS { get; set; }
    public string GroupName { get; set; }
    public string GroupImage { get; set; }
    public string GroupKernel { get; set; }
    public string GroupBootImage { get; set; }
    public string GroupArguments { get; set; }
    public string GroupSenderArgs { get; set; }
    public string GroupScripts { get; set; }
    public string PXEHostMac { get; set; }
    public bool ActiveTaskCreated { get; set; }

    

	public Multicast()
	{
        HostNames = new List<string>();
        HostMacs = new List<string>();
        HostImages = new List<string>();
        HostKernels = new List<string>();
        HostBootImages = new List<string>();
        HostArguments = new List<string>();
        HostIDs = new List<string>();
        TaskIDs = new List<string>();
        PXEBoot = new List<bool>();
	}

    protected bool CheckAllHostsEqual(Multicast multicast)
    {
        bool allEqual = true;

        for (int i = 0; i < multicast.HostNames.Count; i++)
        {
            allEqual = multicast.HostImages.TrueForAll(x => x == multicast.HostImages[i]);
            if (!allEqual)
                break;
            allEqual = multicast.HostKernels.TrueForAll(x => x == multicast.HostKernels[i]);
            if (!allEqual)
                break;
            allEqual = multicast.HostBootImages.TrueForAll(x => x == multicast.HostBootImages[i]);
            if (!allEqual)
                break;
            allEqual = multicast.HostArguments.TrueForAll(x => x == multicast.HostArguments[i]);
            if (!allEqual)
                break;
        }
        if (!allEqual)
            Utility.Message = "All Hosts Are Not Equal";
        return allEqual;
    }

    public void CreateMulticast(int groupID)
    {
        Multicast multicast = new Multicast();
        Utility settings = new Utility();
        Task task = new Task();
        int portBase = task.GetPort();

        if (portBase != 0)
        {
            multicast = multicast.Read(groupID);
            if (multicast != null)
            {
                if (String.IsNullOrEmpty(multicast.GroupSenderArgs))
                    multicast.GroupSenderArgs = settings.GetSettings("Sender Args");

                if (multicast.HostNames.Count > 0)
                {
                    if (CheckAllHostsEqual(multicast))
                    {
                        if (CreateMulticastTask(multicast, "push", "true", portBase))
                        {
                            if (StartMulticastSender(multicast, portBase))
                            {
                                History history = new History();
                                history.Event = "Multicast";
                                history.Type = "Group";
                                history.TypeID = groupID.ToString();
                                history.CreateEvent(history);

                                Host host = new Host();
                                foreach (string mac in HostMacs)
                                {
                                    history.Event = "Deploy";
                                    history.Type = "Host";
                                    history.Notes = "Via Group Multicast: " + multicast.GroupName;
                                    history.TypeID = host.GetHostID(mac);
                                    history.CreateEvent(history);
                                }

                                foreach (string name in HostNames)
                                {
                                    Image image = new Image();
                                    history.Event = "Deploy";
                                    history.Type = "Image";
                                    history.Notes = name;
                                    history.TypeID = image.GetImageID(multicast.GroupImage);
                                    history.CreateEvent(history);
                                }
                            }
                            else
                                RollBack(multicast, true, true, true);

                        }
                    }   
                }
                else
                    Utility.Message = "The Group Does Not Have Any Hosts";
            }         
        }      
    }

    protected bool CreateMulticastTask(Multicast multicast, string direction, string isMulticast, int portBase)
    {
        bool mcTaskCreated = false;
        bool pxeBootCreated = false;
        bool activeTaskCheck = true;

        if (multicast.GroupImage != null)
        {
            mcTaskCreated = multicast.Create(multicast);

            if (mcTaskCreated)
                activeTaskCheck = multicast.IsHostAlreadyActive(multicast);

            if (activeTaskCheck && mcTaskCreated)
                multicast = multicast.CreateHostTask(multicast);

            if (mcTaskCreated && multicast.ActiveTaskCreated)
            {
                multicast = GetHostIDs(multicast);
                pxeBootCreated = multicast.CreatePXEFiles(multicast, portBase);
            }
        }
        else
        {
            Utility.Message = "The Groups Current Image No Longer Exists";
            return false;
        }

        if (mcTaskCreated && multicast.ActiveTaskCreated && pxeBootCreated)
            return true;
        else
            RollBack(multicast, mcTaskCreated, multicast.ActiveTaskCreated, pxeBootCreated);
        return false;
    }

    public bool CreatePxeBootMC(Multicast multicast, string direction, string isMulticast, int portBase, int i)
    {
        Utility settings = new Utility();
        string path = null;
        string[] lines = null;
        string wds_key = null;
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
        string mode = settings.GetSettings("PXE Mode");

        if (settings.GetSettings("Server Key Mode") == "Automated")
             wds_key = settings.GetSettings("Server Key");
        else
             wds_key = "";

        if (proxyDHCP == "Yes")
        {

            biosPathipxe = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe";
            string[] biosLinesipxe = {@"#!ipxe",
                           @"kernel " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/kernels/" + multicast.GroupKernel + ".krn" + " initrd=" + multicast.GroupBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + multicast.HostArguments,
                           @"imgfetch " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/images/" + multicast.GroupBootImage ,
                           @"boot"};

            biosPath = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac;
            string[] biosLines = { @"DEFAULT cruciblewds",
                           @"LABEL cruciblewds", @"KERNEL kernels" + Path.DirectorySeparatorChar + multicast.GroupKernel , 
                           @"APPEND initrd=images" + Path.DirectorySeparatorChar + multicast.GroupBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + multicast.HostArguments};


            efi32Pathipxe = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe";
            string[] efi32Linesipxe = {@"#!ipxe",
                           @"kernel " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/kernels/" + multicast.GroupKernel + ".krn" + " initrd=" + multicast.GroupBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + multicast.HostArguments,
                           @"imgfetch " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/images/" + multicast.GroupBootImage ,
                           @"boot"};

            efi32Path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac;
            string[] efi32Lines = { @"DEFAULT cruciblewds",
                           @"LABEL cruciblewds", @"KERNEL kernels" + Path.DirectorySeparatorChar + multicast.GroupKernel , 
                           @"APPEND initrd=images" + Path.DirectorySeparatorChar + multicast.GroupBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + multicast.HostArguments};


            efi64Pathipxe = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe";
            string[] efi64Linesipxe = {@"#!ipxe",
                           @"kernel " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/kernels/" + multicast.GroupKernel + ".krn" + " initrd=" + multicast.GroupBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + multicast.HostArguments,
                           @"imgfetch " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/images/" + multicast.GroupBootImage ,
                           @"boot"};

            efi64Path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac;
            string[] efi64Lines = { @"DEFAULT cruciblewds",
                           @"LABEL cruciblewds", @"KERNEL kernels" + Path.DirectorySeparatorChar + multicast.GroupKernel , 
                           @"APPEND initrd=images" + Path.DirectorySeparatorChar + multicast.GroupBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + multicast.HostArguments};

            if (File.Exists(biosPath))
            {
                Host hostFunction = new Host();
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(multicast.PXEHostMac)))
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
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(multicast.PXEHostMac)))
                {
                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe.custom");
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
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(multicast.PXEHostMac)))
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
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(multicast.PXEHostMac)))
                {

                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe.custom");
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
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(multicast.PXEHostMac)))
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
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(multicast.PXEHostMac)))
                {

                    Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe.custom");
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
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac;
                string[] tmplines = { @"DEFAULT cruciblewds",
                           @"LABEL cruciblewds", @"KERNEL " + "kernels" + Path.DirectorySeparatorChar + multicast.GroupKernel , 
                           @"APPEND initrd=" + "images" + Path.DirectorySeparatorChar + multicast.GroupBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + multicast.GroupArguments};

                lines = tmplines;
            }
            else if (mode.Contains("ipxe"))
            {
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + multicast.PXEHostMac + ".ipxe";
                string[] tmplines = {@"#!ipxe",
                           @"kernel " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/kernels/" + multicast.GroupKernel + ".krn" + " initrd=" + multicast.GroupBootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp imgDirection=" + direction + " consoleblank=0" + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " " + settings.GetSettings("Global Host Args") + " " + multicast.HostArguments,
                           @"imgfetch " + "http://"+ settings.GetServerIP() +"/cruciblewds/data/boot/images/" + multicast.GroupBootImage ,
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
                if (hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(PXEHostMac)))
                {
                    if (mode.Contains("ipxe"))
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + PXEHostMac + ".ipxe", settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + PXEHostMac + ".ipxe.custom");
                    else
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + PXEHostMac, settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + PXEHostMac + ".custom");
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
                Utility.Message = "Could Not Create PXE File";
                Logger.Log(ex.ToString());
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
            string hostArgs = "imgName=" + multicast.GroupImage + " storage=" + settings.GetSettings(storagePath) + " hostID=" + multicast.HostIDs[i] +
                              " imgOS=" + multicast.ImageOS + " multicast=true " + " hostScripts=" + multicast.GroupScripts +  " xferMode=" + xferMode + " serverIP=" + settings.GetSettings("Server IP") +
                              " hostName=" + multicast.HostNames[i] + " portBase=" + portBase + " " + "clientReceiverArgs=" + settings.GetSettings("Client Receiver Args");

            Task task = new Task();
            if (task.CreateTaskArgs(hostArgs,multicast.TaskIDs[i]))
                return true;
            else
                return false;
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Create PXE File";
            Logger.Log(ex.ToString());
            return false;
        }

    }

    public bool CreatePXEFiles(Multicast multicast, int portBase)
    {
        Task task = new Task();

        for (int i = 0; i < multicast.HostMacs.Count; i++)
        {
            multicast.PXEHostMac = task.MacToPXE(multicast.HostMacs[i]);
            multicast.PXEBoot.Add(CreatePxeBootMC(multicast, "push", "true", portBase, i));
        }
        if (multicast.PXEBoot.Contains(false))
            return false;
        else
            return true;
    }

    public bool StartCustomMC(string imageName)
    {
        string appPath = null;
        string logPath = null;
        string shell = null;

        if (Environment.OSVersion.ToString().Contains("Unix"))
        {
             string dist = null;
             ProcessStartInfo distInfo = new ProcessStartInfo();
             distInfo.UseShellExecute = false;
             distInfo.FileName = "uname";
             distInfo.RedirectStandardOutput = true;
             distInfo.RedirectStandardError = true;
             
             using (Process process = Process.Start(distInfo))
             {
                  dist = process.StandardOutput.ReadToEnd();
             }
             Logger.Log("Distro is " + dist);

             if(dist.ToLower().Contains("bsd"))
               shell = "/bin/csh";
             else
               shell = "/bin/bash";

             logPath = HttpContext.Current.Server.MapPath("~") + @"/data/logs" + @"/";

        }
        else
        {
             appPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "apps" + Path.DirectorySeparatorChar;
             logPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar;
             shell = "cmd.exe";
        }

        Multicast multicast = new Multicast();
        Utility settings = new Utility();
        string fullImagePath = null;

        Task task = new Task();
        int portBase = task.GetPort();
        string senderArgs = settings.GetSettings("Sender Args");
        Process multiSender = null;
        ProcessStartInfo senderInfo = new ProcessStartInfo();

        senderInfo.FileName = (shell);
        string udpFile = null;
        String[] partFiles = null;
        string compExt = null;
        string compAlg = null;
        string stdout = null;

        //Multicasting currently only supports the first active hd
        //Find First Active HD
        Image image = new Image();
        image.ID = image.GetImageID(imageName);
        image.Read(image);
        Image_Physical_Specs ips = new Image_Physical_Specs();
        if (!string.IsNullOrEmpty(image.ClientSizeCustom))
        {
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
            try
            {
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
            }
            catch { }
        }
        else
        {
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);
            try
            {
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);
            }
            catch { }
        }
        string activeHD = null;
        int activeCounter = 0;
        foreach (var hd in ips.hd)
        {
            if (hd.active == "1")
            {
                activeHD = activeCounter.ToString();
                break;
            }
            activeCounter++;
        }

        if (activeCounter == 0)
        {
            fullImagePath = settings.GetSettings("Image Store Path") + imageName;
        }
        else
        {
            fullImagePath = settings.GetSettings("Image Store Path") + imageName + Path.DirectorySeparatorChar + "hd" + (activeCounter + 1).ToString();
        }

        try
        {
            partFiles = Directory.GetFiles(fullImagePath + Path.DirectorySeparatorChar, "*.gz*");
            if (partFiles.Length == 0)
            {
                partFiles = Directory.GetFiles(fullImagePath + Path.DirectorySeparatorChar, "*.lz4*");
                if (partFiles.Length == 0)
                {
                    Utility.Message = "Image Files Could Not Be Located";
                    return false;
                }
                else
                {
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                        compAlg = "lz4 -d ";
                    else
                        compAlg = "lz4.exe -d ";
                    compExt = ".lz4";
                    stdout = " - ";
                }
            }
            else
            {
                if (Environment.OSVersion.ToString().Contains("Unix"))
                    compAlg = "gzip -c -d ";
                else
                    compAlg = "gzip.exe -c -d ";
                
                compExt = ".gz";
                stdout = "";
            }
        }       
        catch
        {
            Utility.Message = "Image Files Could Not Be Located";
            return false;
        }


        int x = 0;
        foreach (var part in ips.hd[activeCounter].partition)
        {
            udpFile = null;
            if (part.active == "1")
            {
                if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".ntfs" + compExt))
                    udpFile = fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".ntfs" + compExt;
                else if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".fat" + compExt))
                    udpFile = fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".fat" + compExt;
                else if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".extfs" + compExt))
                    udpFile = fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".extfs" + compExt;
                else if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".hfsp" + compExt))
                    udpFile = fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".hfsp" + compExt;
                else if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".imager" + compExt))
                    udpFile = fullImagePath + Path.DirectorySeparatorChar + "part" + part.number + ".imager" + compExt;
                else
                {
                    //Look for lvm
                    if (part.vg != null)
                    {
                        if (part.vg.lv != null)
                        {
                            foreach (var lv in part.vg.lv)
                            {
                                if (lv.active == "1")
                                {

                                    if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".ntfs" + compExt))
                                        udpFile = fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".ntfs" + compExt;
                                    else if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".fat" + compExt))
                                        udpFile = fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".fat" + compExt;
                                    else if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".extfs" + compExt))
                                        udpFile = fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".extfs" + compExt;
                                    else if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".hfsp" + compExt))
                                        udpFile = fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".hfsp" + compExt;
                                    else if (File.Exists(fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".imager" + compExt))
                                        udpFile = fullImagePath + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".imager" + compExt;
                                }
                            }
                        }
                    }
                }

                if (udpFile == null)
                    continue;
                else
                    x++;

                if (Environment.OSVersion.ToString().Contains("Unix"))
                {
                    if (x == 1)
                        senderInfo.Arguments = (" -c \"" + compAlg + udpFile + stdout + " | udp-sender" +
                                           " --portbase " + portBase + " " + senderArgs + " --ttl 32");
                    else
                        senderInfo.Arguments += (" ; " + compAlg + udpFile + stdout + " | udp-sender" +
                                           " --portbase " + portBase + " " + senderArgs + " --ttl 32");
                }
                else
                {
                    if (x == 1)
                        senderInfo.Arguments = (" /c " + appPath + compAlg + udpFile + stdout + " | " + appPath + "udp-sender.exe" +
                                           " --portbase " + portBase + " " + senderArgs + " --ttl 32");
                    else
                        senderInfo.Arguments += (" & " + appPath + compAlg + udpFile + stdout + " | " + appPath + "udp-sender.exe" +
                                           " --portbase " + portBase + " " + senderArgs + " --ttl 32");
                }
            }
        }

        string log = ("\r\n" + DateTime.Now.ToString("MM.dd.yy hh:mm") + " Starting Multicast Session " + portBase +
                      " With The Following Command:\r\n\r\n" + senderInfo.FileName + senderInfo.Arguments + "\r\n\r\n");
        System.IO.File.AppendAllText(logPath + "multicast.log", log);

        if (Environment.OSVersion.ToString().Contains("Unix"))
        {
             senderInfo.Arguments += "\"";
        }

        try
        {
            multiSender = Process.Start(senderInfo);

        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Start Multicast Sender.  Check The Exception Log For More Info";
            System.IO.File.AppendAllText(logPath + "multicast.log", "Could Not Start Session " + portBase + " Try Pasting The Command Into A Command Prompt");
            return false;
        }

        Thread.Sleep(2000);

        if (multiSender.HasExited)
        {
            Utility.Message = "Could Not Start Multicast Sender";
            System.IO.File.AppendAllText(logPath + "multicast.log", "Session " + portBase + @" Started And Was Forced To Quit, Try Pasting The Command Into A Command Prompt");
            return false;
        }
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_createcustom", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@mcTaskName", portBase.ToString()));
                cmd.Parameters.Add(new NpgsqlParameter("@mcPID", multiSender.Id));
                cmd.Parameters.Add(new NpgsqlParameter("@mcPortBase", portBase));
                cmd.Parameters.Add(new NpgsqlParameter("@mcImage", imageName));

                conn.Open();
                cmd.ExecuteNonQuery();
                Utility.Message = "Successfully Started Task " + portBase;
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Create Custom Multicast.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
            return false;
        }
        return true;
    }

    

    protected bool StartMulticastSender(Multicast multicast, int portBase)
    {
        string shell = null;

        string appPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "apps" + Path.DirectorySeparatorChar;
        string logPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar;
        if (Environment.OSVersion.ToString().Contains("Unix"))
        {
             string dist = null;
             ProcessStartInfo distInfo = new ProcessStartInfo();
             distInfo.UseShellExecute = false;
             distInfo.FileName = "uname";
             distInfo.RedirectStandardOutput = true;
             distInfo.RedirectStandardError = true;

             using (Process process = Process.Start(distInfo))
             {
                  dist = process.StandardOutput.ReadToEnd();
             }

             if (dist.ToLower().Contains("bsd"))
                  shell = "/bin/csh";
             else
                  shell = "/bin/bash";
        }
        else
        {       
            shell = "cmd.exe";
        }

        Task task = new Task();
        Utility settings = new Utility();
        string mcImage = multicast.GroupImage;
        int receivers = multicast.HostNames.Count;
        string imageName = multicast.GroupImage;
       


        Process sender = null;
        ProcessStartInfo senderInfo = new ProcessStartInfo();

        senderInfo.FileName = (shell);
        string udpFile = null;
        String[] partFiles = null;
        string compExt = null;
        string compAlg = null;
        string stdout = null;

        //Multicasting currently only supports the first active hd
        //Find First Active HD
        Image image = new Image();
        image.ID = image.GetImageID(imageName);
        image.Read(image);
        Image_Physical_Specs ips = new Image_Physical_Specs();
        if (!string.IsNullOrEmpty(image.ClientSizeCustom))
        {
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
            try
            {
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
            }
            catch { }
        }
        else
        {
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);
            try
            {
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);
            }
            catch { }
        }
        string activeHD = null;
        int activeCounter = 0;
        foreach(var hd in ips.hd)
        {
            if (hd.active == "1")
            {
                activeHD = activeCounter.ToString();
                break;
            }
            activeCounter++;
        }

        if (activeCounter == 0)
        {
            multicast.GroupImage = settings.GetSettings("Image Store Path") + multicast.GroupImage;
        }
        else
        {
            multicast.GroupImage = settings.GetSettings("Image Store Path") + multicast.GroupImage + Path.DirectorySeparatorChar + "hd" + (activeCounter + 1).ToString();
        }

        try
        {
            partFiles = Directory.GetFiles(multicast.GroupImage + Path.DirectorySeparatorChar, "*.gz*");
            if (partFiles.Length == 0)
            {
                partFiles = Directory.GetFiles(multicast.GroupImage + Path.DirectorySeparatorChar, "*.lz4*");
                if (partFiles.Length == 0)
                {
                    Utility.Message = "Image Files Could Not Be Located";
                    return false;
                }
                else
                {
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                        compAlg = "lz4 -d ";
                    else
                        compAlg = "lz4.exe -d ";
                    compExt = ".lz4";
                    stdout = " - ";
                }
            }
            else
            {
                if (Environment.OSVersion.ToString().Contains("Unix"))
                    compAlg = "gzip -c -d ";
                else
                    compAlg = "gzip.exe -c -d ";

                compExt = ".gz";
                stdout = "";
            }
        }       
        catch
        {
            Utility.Message = "Image Files Could Not Be Located";
            return false;
        }

        int x = 0;
        foreach (var part in ips.hd[activeCounter].partition)
        {
            udpFile = null;
            if (part.active == "1")
            {
                if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".ntfs" + compExt))
                    udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".ntfs" + compExt;
                else if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".fat" + compExt))
                    udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".fat" + compExt;
                else if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".extfs" + compExt))
                    udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".extfs" + compExt;
                else if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".hfsp" + compExt))
                    udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".hfsp" + compExt;
                else if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".imager" + compExt))
                    udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + "part" + part.number + ".imager" + compExt;
                else
                {
                    //Look for lvm
                    if (part.vg != null)
                    {
                        if (part.vg.lv != null)
                        {
                            foreach (var lv in part.vg.lv)
                            {
                                if (lv.active == "1")
                                {

                                    if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".ntfs" + compExt))
                                        udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".ntfs" + compExt;
                                    else if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".fat" + compExt))
                                        udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".fat" + compExt;
                                    else if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".extfs" + compExt))
                                        udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".extfs" + compExt;
                                    else if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".hfsp" + compExt))
                                        udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".hfsp" + compExt;
                                    else if (File.Exists(multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".imager" + compExt))
                                        udpFile = multicast.GroupImage + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + ".imager" + compExt;
                                }
                            }
                        }
                    }
                }

                if (udpFile == null)
                    continue;
                else
                    x++;

                if (Environment.OSVersion.ToString().Contains("Unix"))
                {
                    if (x == 1)
                        senderInfo.Arguments = (" -c \"" + compAlg + udpFile + stdout + " | udp-sender" +
                                               " --portbase " + portBase + " --min-receivers " + receivers + " " + multicast.GroupSenderArgs + " --ttl 32");

                    else
                        senderInfo.Arguments += (" ; " + compAlg + udpFile + stdout + " | udp-sender" +
                                           " --portbase " + portBase + " --min-receivers " + receivers + " " + multicast.GroupSenderArgs + " --ttl 32");
                }
                else
                {
                    if (x == 1)
                        senderInfo.Arguments = (" /c " + appPath + compAlg + udpFile + stdout + " | " + appPath + "udp-sender.exe" +
                                               " --portbase " + portBase + " --min-receivers " + receivers + " " + multicast.GroupSenderArgs + " --ttl 32");
                    else
                        senderInfo.Arguments += (" & " + appPath + compAlg + udpFile + stdout + " | " + appPath + "udp-sender.exe" +
                                           " --portbase " + portBase + " --min-receivers " + receivers + " " + multicast.GroupSenderArgs + " --ttl 32");
                }
            }
        }

        if (Environment.OSVersion.ToString().Contains("Unix"))
        {
             senderInfo.Arguments += "\"";
        }

        string log = ("\r\n" + DateTime.Now.ToString("MM.dd.yy hh:mm") + " Starting Multicast Session " + multicast.GroupName +
                      " With The Following Command:\r\n\r\n" + senderInfo.FileName + senderInfo.Arguments + "\r\n\r\n");
        System.IO.File.AppendAllText(logPath + "multicast.log", log);

        try
        {
            sender = Process.Start(senderInfo);

        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Start Multicast Sender.  Check The Exception Log For More Info";
            System.IO.File.AppendAllText(logPath + "multicast.log", "Could Not Start Session " + multicast.GroupName + " Try Pasting The Command Into A Command Prompt");
            return false;
        }

        Thread.Sleep(2000);

        if (sender.HasExited)
        {
            Utility.Message = "Could Not Start Multicast Sender";
            System.IO.File.AppendAllText(logPath + "multicast.log", "Session " + multicast.GroupName + @" Started And Was Forced To Quit, Try Pasting The Command Into A Command Prompt");
            return false;
        }

        multicast.UpdatePID(sender.Id, multicast, portBase, mcImage);

        for (int i = 0; i < multicast.HostMacs.Count; i++)
        {
            task.WakeUp(multicast.HostMacs[i]);
        }

        Utility.Message = "Successfully Started Multicast " + multicast.GroupName;
        return true;
    }

    public void RollBack(Multicast multicast, bool mcTaskCreated, bool activeTaskCreated, bool pxeBootCreated)
    {
        Task task = new Task();
        if (pxeBootCreated)
        {
            string pxeHostMac;
            for (int i = 0; i < multicast.HostNames.Count; i++)
            {
                pxeHostMac = task.MacToPXE(multicast.HostMacs[i]);
                task.CleanPxeBoot(pxeHostMac);
            }
        }

        if (activeTaskCreated)
            multicast.RollBackActiveTasks(multicast);

        if (mcTaskCreated)
            multicast.RollBackActiveMCTask(multicast);
    }

    public DataTable Confirm(string groupID)
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_confirm", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupID", groupID));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Confirm Data.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public Multicast Read(int groupID)
    {
        Multicast multicast = new Multicast();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_read", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@groupID", groupID));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    multicast.HostNames.Add((string)rdr["_hostname"]);
                    multicast.HostMacs.Add((string)rdr["_hostmac"]);
                    multicast.HostImages.Add((string)rdr["_hostimage"]);
                    multicast.HostKernels.Add((string)rdr["_hostkernel"]);
                    multicast.HostBootImages.Add((string)rdr["_hostbootimage"]);
                    multicast.HostArguments.Add((string)rdr["_hostarguments"]);
                    multicast.ImageOS = (string)rdr["_imageos"];
                    multicast.GroupImage = (string)rdr["_groupimage"];
                    multicast.GroupKernel = (string)rdr["_groupkernel"];
                    multicast.GroupBootImage = (string)rdr["_groupbootimage"];
                    multicast.GroupArguments = (string)rdr["_grouparguments"];
                    multicast.GroupSenderArgs = (string)rdr["_groupsenderargs"];
                    multicast.GroupName = (string)rdr["_groupname"];
                    multicast.GroupScripts = rdr["_groupscripts"].ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Multicast Info.  Check The Exception Log For More Info.";
            Logger.Log(ex.ToString());
            multicast = null;
        }
        
        return multicast;
    }

    public bool Create(Multicast multicast)
    {
        bool mcTaskCreated = false;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_create", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@multicastName", multicast.GroupName));
                conn.Open();
                string result = cmd.ExecuteScalar() as string;
                if (result != null)
                    Utility.Message = result;
                else
                    mcTaskCreated = true;
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Create Multicast.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return mcTaskCreated;
    }

    public bool IsHostAlreadyActive(Multicast multicast)
    {
        bool activeTaskCheck = true;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_readactivehosts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@hostName", NpgsqlDbType.Varchar));
                conn.Open();
                for (int i = 0; i < multicast.HostNames.Count; i++)
                {
                    cmd.Parameters["@hostName"].Value = multicast.HostNames[i];
                    string result = cmd.ExecuteScalar() as string;
                    if (result != null)
                    {
                        Utility.Message = result;
                        activeTaskCheck = false;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Determine If Any Hosts Are Already Active.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
            activeTaskCheck = false;
        }
        return activeTaskCheck;
    }

    public Multicast CreateHostTask(Multicast multicast)
    {
        multicast.ActiveTaskCreated = false;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_createhosts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@hostName", NpgsqlDbType.Varchar));
                cmd.Parameters.Add(new NpgsqlParameter("@multicastName", multicast.GroupName));

                conn.Open();
                for (int i = 0; i < multicast.HostNames.Count; i++)
                {
                    cmd.Parameters["@hostName"].Value = multicast.HostNames[i];
                    multicast.TaskIDs.Add(cmd.ExecuteScalar() as string);
                }

                multicast.ActiveTaskCreated = true;
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Create Host Tasks.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return multicast;
    }

    public Multicast GetHostIDs(Multicast multicast)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("hosts_readhostid", conn);
                cmd.CommandType = CommandType.StoredProcedure;  
                cmd.Parameters.Add(new NpgsqlParameter("@hostMac", NpgsqlDbType.Varchar));

                conn.Open();
                for (int i = 0; i < multicast.HostNames.Count; i++)
                {
                    cmd.Parameters["@hostMac"].Value = multicast.HostMacs[i];
                    multicast.HostIDs.Add(cmd.ExecuteScalar().ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Host IDs.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return multicast;
    }

    public void RollBackActiveTasks(Multicast multicast)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_rollbackhosts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@taskname", multicast.GroupName));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }
    }

    public void RollBackActiveMCTask(Multicast multicast)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_rollback", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@taskname", multicast.GroupName));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }
        
    }

    public void UpdatePID(int pid, Multicast multicast, int portBase, string mcImage)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_updatepid", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@mcPID", pid));
                cmd.Parameters.Add(new NpgsqlParameter("@mcPortBase", portBase));
                cmd.Parameters.Add(new NpgsqlParameter("@mcTaskName", multicast.GroupName));
                cmd.Parameters.Add(new NpgsqlParameter("@mcImage", mcImage));
                conn.Open();
                bool result = Convert.ToBoolean(cmd.ExecuteScalar());
                if (!result)
                    Utility.Message = result.ToString();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }

    }
}