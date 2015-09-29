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
using System.Text;
using System.Data;
using Npgsql;
using Microsoft.Win32;
using Mono.Unix.Native;


public class Settings
{
    public string DebugPwd { get; set; }
    public string AddPwd { get; set; }
    public string OndPwd { get; set; }
    public string DiagPwd { get; set; }


    public List<string> Read()
    {
        List<string> settings = new List<string>();

        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("settings_read", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    settings.Add((string)rdr["settingname"]);
                    settings.Add((string)rdr["settingvalue"]);
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Settings.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }

        return settings;
    }

    public void PXEmode(string mode)
    {
        Utility settings = new Utility();
        string proxyDHCP = settings.GetSettings("Proxy Dhcp");
        string biosFile = null;
        string efi32File = null;
        string efi64File = null;
        string sourcePath = settings.GetSettings("Tftp Path") + "static"+ Path.DirectorySeparatorChar;
        string destinationPath = null;
        if (proxyDHCP == "Yes")
        {
            destinationPath = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar;
            biosFile = settings.GetSettings("Proxy Bios File");
            efi32File = settings.GetSettings("Proxy Efi32 File");
            efi64File = settings.GetSettings("Proxy Efi64 File");
        }
        else
            destinationPath = settings.GetSettings("Tftp Path");
        try
        {
            if (proxyDHCP == "Yes")
            {
                string type = null;
                string httpImagePath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "boot" + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar;
                string httpKernelPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "boot" + Path.DirectorySeparatorChar + "kernels" + Path.DirectorySeparatorChar;
                string kernelPath = settings.GetSettings("Tftp Path") + "kernels" + Path.DirectorySeparatorChar;
                string bootImagePath = settings.GetSettings("Tftp Path") + "images" + Path.DirectorySeparatorChar;

                if (biosFile == "ipxe")
                {
                    type = "bios";
                    File.Copy(sourcePath + "ipxe" +Path.DirectorySeparatorChar+ "proxy" + Path.DirectorySeparatorChar + "ipxe" + Path.DirectorySeparatorChar + "undionly.kpxe", destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    CopyKernelsForHttp();
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                else if (biosFile == "lpxelinux")
                {
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "lpxelinux.0", destinationPath + "bios" + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "ldlinux.c32", destinationPath + "bios" + Path.DirectorySeparatorChar + "ldlinux.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "libcom32.c32", destinationPath + "bios" + Path.DirectorySeparatorChar + "libcom32.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "libutil.c32", destinationPath + "bios" + Path.DirectorySeparatorChar + "libutil.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "vesamenu.c32", destinationPath + "bios" + Path.DirectorySeparatorChar + "vesamenu.c32", true);
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "ldlinux.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "libcom32.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "libutil.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "vesamenu.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                    CopyKernelsForHttp();
                }

                else if (biosFile == "pxelinux")
                {
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "pxelinux.0", destinationPath + "bios" + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "ldlinux.c32", destinationPath + "bios" + Path.DirectorySeparatorChar + "ldlinux.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "libcom32.c32", destinationPath + "bios" + Path.DirectorySeparatorChar + "libcom32.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "libutil.c32", destinationPath + "bios" + Path.DirectorySeparatorChar + "libutil.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "vesamenu.c32", destinationPath + "bios" + Path.DirectorySeparatorChar + "vesamenu.c32", true);
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "ldlinux.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "libcom32.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "libutil.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "bios" + Path.DirectorySeparatorChar + "vesamenu.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                if (efi32File == "ipxe_32_efi")
                {
                    type = "efi32";
                    File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "proxy" + Path.DirectorySeparatorChar + "ipxe_efi_32" + Path.DirectorySeparatorChar + "ipxe.efi", destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    CopyKernelsForHttp();
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                else if (efi32File == "ipxe_32_efi_snp")
                {
                    type = "efi32";
                    File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "proxy" + Path.DirectorySeparatorChar + "ipxe_efi_32" + Path.DirectorySeparatorChar + "snp.efi", destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    CopyKernelsForHttp();
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                else if (efi32File == "ipxe_32_efi_snponly")
                {
                    type = "efi32";
                    File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "proxy" + Path.DirectorySeparatorChar + "ipxe_efi_32" + Path.DirectorySeparatorChar + "snponly.efi", destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    CopyKernelsForHttp();
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                else if (efi32File == "syslinux_32_efi")
                {
                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "syslinux.efi", destinationPath + "efi32" + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "ldlinux.e32", destinationPath + "efi32" + Path.DirectorySeparatorChar + "ldlinux.e32", true);
                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "libcom32.c32", destinationPath + "efi32" + Path.DirectorySeparatorChar + "libcom32.c32", true);
                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "libutil.c32", destinationPath + "efi32" + Path.DirectorySeparatorChar + "libutil.c32", true);
                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "vesamenu.c32", destinationPath + "efi32" + Path.DirectorySeparatorChar + "vesamenu.c32", true);
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + "efi32" + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "efi32" + Path.DirectorySeparatorChar + "ldlinux.e32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "efi32" + Path.DirectorySeparatorChar + "libcom32.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "efi32" + Path.DirectorySeparatorChar + "libutil.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "efi32" + Path.DirectorySeparatorChar + "vesamenu.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                if (efi64File == "ipxe_64_efi")
                {
                    type = "efi64";
                    File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "proxy" + Path.DirectorySeparatorChar + "ipxe_efi_64" + Path.DirectorySeparatorChar + "ipxe.efi", destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    CopyKernelsForHttp();
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                else if (efi64File == "ipxe_64_efi_snp")
                {
                    type = "efi64";
                    File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "proxy" + Path.DirectorySeparatorChar + "ipxe_efi_64" + Path.DirectorySeparatorChar + "snp.efi", destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    CopyKernelsForHttp();
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                else if (efi64File == "ipxe_64_efi_snponly")
                {
                    type = "efi64";
                    File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "proxy" + Path.DirectorySeparatorChar + "ipxe_efi_64" + Path.DirectorySeparatorChar + "snponly.efi", destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    CopyKernelsForHttp();
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + type + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                else if (efi64File == "syslinux_64_efi")
                {
                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "syslinux.efi", destinationPath + "efi64" + Path.DirectorySeparatorChar + "pxeboot.0", true);
                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "ldlinux.e64", destinationPath + "efi64" + Path.DirectorySeparatorChar + "ldlinux.e64", true);
                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "libcom32.c32", destinationPath + "efi64" + Path.DirectorySeparatorChar + "libcom32.c32", true);
                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "libutil.c32", destinationPath + "efi64" + Path.DirectorySeparatorChar + "libutil.c32", true);
                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "vesamenu.c32", destinationPath + "efi64" + Path.DirectorySeparatorChar + "vesamenu.c32", true);
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + "efi64" + Path.DirectorySeparatorChar + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "efi64" + Path.DirectorySeparatorChar + "ldlinux.e64", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "efi64" + Path.DirectorySeparatorChar + "libcom32.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "efi64" + Path.DirectorySeparatorChar + "libutil.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "efi64" + Path.DirectorySeparatorChar + "vesamenu.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
            }
            else
            {
                if (mode.Contains("ipxe"))
                {
                    if (mode == "ipxe")
                        File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "normal" + Path.DirectorySeparatorChar + "ipxe" + Path.DirectorySeparatorChar + "undionly.kpxe", destinationPath + "pxeboot.0", true);
                    else if (mode == "ipxe_32_efi")
                        File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "normal" + Path.DirectorySeparatorChar + "ipxe_efi_32" + Path.DirectorySeparatorChar + "ipxe.efi", destinationPath + "pxeboot.0", true);
                    else if (mode == "ipxe_64_efi")
                        File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "normal" + Path.DirectorySeparatorChar + "ipxe_efi_64" + Path.DirectorySeparatorChar + "ipxe.efi", destinationPath + "pxeboot.0", true);
                    else if (mode == "ipxe_32_efi_snp")
                        File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "normal" + Path.DirectorySeparatorChar + "ipxe_efi_32" + Path.DirectorySeparatorChar + "snp.efi", destinationPath + "pxeboot.0", true);
                    else if (mode == "ipxe_64_efi_snp")
                        File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "normal" + Path.DirectorySeparatorChar + "ipxe_efi_64" + Path.DirectorySeparatorChar + "snp.efi", destinationPath + "pxeboot.0", true);
                    else if (mode == "ipxe_32_efi_snponly")
                        File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "normal" + Path.DirectorySeparatorChar + "ipxe_efi_32" + Path.DirectorySeparatorChar + "snponly.efi", destinationPath + "pxeboot.0", true);
                    else if (mode == "ipxe_64_efi_snponly")
                        File.Copy(sourcePath + "ipxe" + Path.DirectorySeparatorChar + "normal" + Path.DirectorySeparatorChar + "ipxe_efi_64" + Path.DirectorySeparatorChar + "snponly.efi", destinationPath + "pxeboot.0", true);

                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                    CopyKernelsForHttp();
                }
                else if (mode == "lpxelinux")
                {
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "lpxelinux.0", destinationPath + "pxeboot.0", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "ldlinux.c32", destinationPath + "ldlinux.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "libcom32.c32", destinationPath + "libcom32.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "libutil.c32", destinationPath + "libutil.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "vesamenu.c32", destinationPath + "vesamenu.c32", true);
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "ldlinux.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "libcom32.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "libutil.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "vesamenu.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                    CopyKernelsForHttp();
                }
                else if (mode == "pxelinux")
                {

                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "pxelinux.0", destinationPath + "pxeboot.0", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "ldlinux.c32", destinationPath + "ldlinux.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "libcom32.c32", destinationPath + "libcom32.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "libutil.c32", destinationPath + "libutil.c32", true);
                    File.Copy(sourcePath + "pxelinux" + Path.DirectorySeparatorChar + "vesamenu.c32", destinationPath + "vesamenu.c32", true);
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "ldlinux.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "libcom32.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "libutil.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "vesamenu.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }

                }
                else if (mode == "syslinux_32_efi")
                {

                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "syslinux.efi", destinationPath + "pxeboot.0", true);
                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "ldlinux.e32", destinationPath + "ldlinux.e32", true);
                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "libcom32.c32", destinationPath + "libcom32.c32", true);
                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "libutil.c32", destinationPath + "libutil.c32", true);
                    File.Copy(sourcePath + "syslinux_efi_32" + Path.DirectorySeparatorChar + "vesamenu.c32", destinationPath + "vesamenu.c32", true);
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "ldlinux.e32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "libcom32.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "libutil.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "vesamenu.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }
                else if (mode == "syslinux_64_efi")
                {

                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "syslinux.efi", destinationPath + "pxeboot.0", true);
                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "ldlinux.e64", destinationPath + "ldlinux.e64", true);
                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "libcom32.c32", destinationPath + "libcom32.c32", true);
                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "libutil.c32", destinationPath + "libutil.c32", true);
                    File.Copy(sourcePath + "syslinux_efi_64" + Path.DirectorySeparatorChar + "vesamenu.c32", destinationPath + "vesamenu.c32", true);
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        Syscall.chmod(destinationPath + "pxeboot.0", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "ldlinux.e64", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "libcom32.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "libutil.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                        Syscall.chmod(destinationPath + "vesamenu.c32", (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }
        
    }

    private void CopyKernelsForHttp()
    {
        Utility settings = new Utility();
        string httpImagePath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "boot" + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar;
        string httpKernelPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "boot" + Path.DirectorySeparatorChar + "kernels" + Path.DirectorySeparatorChar;
        string kernelPath = settings.GetSettings("Tftp Path") + "kernels" + Path.DirectorySeparatorChar;
        string bootImagePath = settings.GetSettings("Tftp Path") + "images" + Path.DirectorySeparatorChar;

        string[] kernels = Utility.GetKernels();
        foreach (string kernel in kernels)
        {
            File.Copy(kernelPath + kernel, httpKernelPath + kernel + ".krn", true);
        }

        string[] bootImages = Utility.GetBootImages();
        foreach (string bootimage in bootImages)
        {
            File.Copy(bootImagePath + bootimage, httpImagePath + bootimage, true);
        }
    }
    public void Update(List<string> names, List<string> values)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();
                for (int i = 0; i < names.Count; i++)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("settings_update", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new NpgsqlParameter("@settingName", names[i]));
                    cmd.Parameters.Add(new NpgsqlParameter("@settingValue", values[i]));
                    cmd.ExecuteNonQuery();
                }
                Utility.Message = "Successfully Updated Settings";
            }

        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update Settings.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }
    public void CreateBootMenu(Settings bootMenu, string kernel, string bootImage, string type)
    {
        Utility settings = new Utility();
        string mode = settings.GetSettings("PXE Mode");
       
        if (type == "noprox")
        {
            if (mode.Contains("ipxe"))
                CreateIpxeMenu(bootMenu, kernel, bootImage, type);
            else
                CreateSyslinuxMenu(bootMenu, kernel, bootImage, type);
        }
        else
        {
              CreateIpxeMenu(bootMenu, kernel, bootImage, type);
              CreateSyslinuxMenu(bootMenu, kernel, bootImage, type);
        }    
    }

    private void CreateSyslinuxMenu(Settings bootMenu, string kernel, string bootImage, string type)
    {
        Utility settings = new Utility();
        string ipAddress = settings.GetSettings("Server IP");
        string webPath = settings.GetSettings("Web Path");
        string globalHostArgs = settings.GetSettings("Global Host Args");
        string wds_key = null;
        if (settings.GetSettings("Server Key Mode") == "Automated")
            wds_key = settings.GetSettings("Server Key");
        else
            wds_key = "";
        string lines = null;
        string path = null;

        lines = "DEFAULT vesamenu.c32\r\n";
        lines += "MENU TITLE Boot Menu\r\n";
        lines += "MENU BACKGROUND bg.png\r\n";
        lines += "menu tabmsgrow 22\r\n";
        lines += "menu cmdlinerow 22\r\n";
        lines += "menu endrow 24\r\n";
        lines += "menu color title                1;34;49    #eea0a0ff #cc333355 std\r\n";
        lines += "menu color sel                  7;37;40    #ff000000 #bb9999aa all\r\n";
        lines += "menu color border               30;44      #ffffffff #00000000 std\r\n";
        lines += "menu color pwdheader            31;47      #eeff1010 #20ffffff std\r\n";
        lines += "menu color hotkey               35;40      #90ffff00 #00000000 std\r\n";
        lines += "menu color hotsel               35;40      #90000000 #bb9999aa all\r\n";
        lines += "menu color timeout_msg          35;40      #90ffffff #00000000 none\r\n";
        lines += "menu color timeout              31;47      #eeff1010 #00000000 none\r\n";
        lines += "NOESCAPE 0\r\n";
        lines += "ALLOWOPTIONS 0\r\n";
        lines += "\r\n";
        lines += "LABEL local\r\n";
        lines += "localboot 0\r\n";
        lines += "MENU DEFAULT\r\n";
        lines += "MENU LABEL Boot To Local Machine\r\n";
        lines += "\r\n";

        lines += "LABEL Client Console\r\n";
        if (bootMenu.DebugPwd != "" && bootMenu.DebugPwd != null && bootMenu.DebugPwd != "Error: Empty password")
            lines += "MENU PASSWD " + bootMenu.DebugPwd + "\r\n";
        if (type != "noprox")
        {
            lines += "kernel kernels" + Path.DirectorySeparatorChar + kernel + "\r\n";
            lines += "append initrd=images" + Path.DirectorySeparatorChar + bootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp " + " web=" + webPath + " WDS_KEY=" + wds_key + " task=debug consoleblank=0 " + globalHostArgs + "\r\n";
        }
        else
        {
            lines += "kernel kernels" + Path.DirectorySeparatorChar + kernel + "\r\n";
            lines += "append initrd=images" + Path.DirectorySeparatorChar + bootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp " + " web=" + webPath + " WDS_KEY=" + wds_key + " task=debug consoleblank=0 " + globalHostArgs + "\r\n";
        }

        lines += "MENU LABEL Client Console\r\n";
        lines += "\r\n";

        lines += "LABEL Add Host\r\n";
        if (bootMenu.AddPwd != "" && bootMenu.AddPwd != null && bootMenu.AddPwd != "Error: Empty password")
            lines += "MENU PASSWD " + bootMenu.AddPwd + "\r\n";
        if (type != "noprox")
        {
            lines += "kernel kernels" + Path.DirectorySeparatorChar + kernel + "\r\n";
            lines += "append images" + Path.DirectorySeparatorChar + bootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp " + " web=" + webPath + " WDS_KEY=" + wds_key + " task=register consoleblank=0 " + globalHostArgs + "\r\n";
        }
        else
        {
            lines += "kernel kernels" + Path.DirectorySeparatorChar + kernel + "\r\n";
            lines += "append initrd=images" + Path.DirectorySeparatorChar + bootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp " + " web=" + webPath + " WDS_KEY=" + wds_key + " task=register consoleblank=0 " + globalHostArgs + "\r\n";
        }
        lines += "MENU LABEL Add Host\r\n";
        lines += "\r\n";

        lines += "LABEL On Demand\r\n";
        if (bootMenu.OndPwd != "" && bootMenu.OndPwd != null && bootMenu.OndPwd != "Error: Empty password")
            lines += "MENU PASSWD " + bootMenu.OndPwd + "\r\n";
        if (type != "noprox")
        {
            lines += "kernel kernels" + Path.DirectorySeparatorChar + kernel + "\r\n";
            lines += "append initrd=images" + Path.DirectorySeparatorChar + bootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp " + " web=" + webPath + " WDS_KEY=" + wds_key + " task=ond consoleblank=0 " + globalHostArgs + "\r\n";
        }
        else
        {
            lines += "kernel kernels" + Path.DirectorySeparatorChar + kernel + "\r\n";
            lines += "append initrd=images" + Path.DirectorySeparatorChar + bootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp " + " web=" + webPath + " WDS_KEY=" + wds_key + " task=ond consoleblank=0 " + globalHostArgs + "\r\n";
        }
        lines += "MENU LABEL On Demand\r\n";
        lines += "\r\n";

        lines += "LABEL Diagnostics\r\n";
        if (bootMenu.DiagPwd != "" && bootMenu.DiagPwd != null && bootMenu.DiagPwd != "Error: Empty password")
            lines += "MENU PASSWD " + bootMenu.DiagPwd + "\r\n";
        if (type != "noprox")
        {
            lines += "kernel kernels" + Path.DirectorySeparatorChar + kernel + "\r\n";
            lines += "append initrd=images" + Path.DirectorySeparatorChar + bootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp " + " web=" + webPath + " WDS_KEY=" + wds_key + " task=diag consoleblank=0 " + globalHostArgs + "\r\n";
        }
        else
        {
            lines += "kernel kernels" + Path.DirectorySeparatorChar + kernel + "\r\n";
            lines += "append initrd=images" + Path.DirectorySeparatorChar + bootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp " + " web=" + webPath + " WDS_KEY=" + wds_key + " task=diag consoleblank=0 " + globalHostArgs + "\r\n";
        }

        lines += "MENU LABEL Diagnostics\r\n";
        lines += "\r\n";

        lines += "PROMPT 0\r\n";
        lines += "TIMEOUT 50";


        if (type == "noprox")
        {
            path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
        }
        else
        {
            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
        }
        try
        {
            using (StreamWriter file = new StreamWriter(path))
            {
                file.WriteLine(lines);
                file.Close();
                Utility.Message = "Successfully Created Default Boot Menu";
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            Utility.Message = "Could Not Create Boot Menu.  Check The Exception Log For More Info.";
            return;
        }
        if (Environment.OSVersion.ToString().Contains("Unix"))
        {
            Syscall.chmod(path, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
        }  
    }

    private void CreateIpxeMenu(Settings bootMenu, string kernel, string bootImage, string type)
    {
        Utility settings = new Utility();
        string ipAddress = settings.GetSettings("Server IP");
        string webPath = settings.GetSettings("Web Path");
        string globalHostArgs = settings.GetSettings("Global Host Args");
        string wds_key = null;
        if (settings.GetSettings("Server Key Mode") == "Automated")
            wds_key = settings.GetSettings("Server Key");
        else
            wds_key = "";
        string path = null;
        string lines = null;
        lines = "#!ipxe\r\n";
        lines += "chain 01-${net0/mac:hexhyp}.ipxe || goto Menu\r\n";
        lines += "\r\n";
        lines += ":Menu\r\n";
        lines += "menu Boot Menu\r\n";
        lines += "item boot Boot To Local Machine\r\n";
        lines += "item console Client Console\r\n";
        lines += "item addhost Add Host\r\n";
        lines += "item ond On Demand\r\n";
        lines += "item diag Diagnostics\r\n";
        lines += "choose --default boot --timeout 5000 target && goto ${target}\r\n";
        lines += "\r\n";

        lines += ":boot\r\n";
        lines += "exit\r\n";
        lines += "\r\n";

        lines += ":console\r\n";
        lines += "set task debug\r\n";
        lines += "goto login\r\n";
        lines += "\r\n";

        lines += ":addhost\r\n";
        lines += "set task register\r\n";
        lines += "goto login\r\n";
        lines += "\r\n";

        lines += ":ond\r\n";
        lines += "set task ond\r\n";
        lines += "goto login\r\n";

        lines += "\r\n";
        lines += ":diag\r\n";
        lines += "set task diag\r\n";
        lines += "goto login\r\n";
        lines += "\r\n";

        lines += ":login\r\n";
        lines += "login\r\n";
        lines += "params\r\n";
        lines += "param uname ${username:uristring}\r\n";
        lines += "param pwd ${password:uristring}\r\n";
        lines += "param kernel " + kernel + "\r\n";
        lines += "param bootImage " + bootImage + "\r\n";
        lines += "param task " + "${task}" + "\r\n";
        lines += "echo Authenticating\r\n";
        lines += "chain --timeout 15000 http://" + settings.GetServerIP() + "/cruciblewds/service/client.asmx/ipxelogin##params || goto Menu\r\n";

        if (type == "noprox")
        {
            path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
        }
        else
        {
            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
        }

        try
        {
            using (StreamWriter file = new StreamWriter(path))
            {
                file.WriteLine(lines);
                file.Close();
                Utility.Message = "Successfully Created Default Boot Menu";
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            Utility.Message = "Could Not Create Boot Menu.  Check The Exception Log For More Info.";
            return;
        }
        if (Environment.OSVersion.ToString().Contains("Unix"))
        {
            Syscall.chmod(path, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
        }
    }

    public void Export()
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("settings_exportdb", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@outpath", HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "dbbackup" + Path.DirectorySeparatorChar));
                conn.Open();
                cmd.ExecuteNonQuery();

                Utility.Message = "Backup Complete.  Located At: " + (HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "dbbackup").Replace(@"\", @"\\");
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Backup Failed.  Check The Exception Log For More Info.";
        }
    }

    public void UpdateLastPort(int port)
    {
        
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("settings_updatelastport", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@port", port));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
        }
    }
}
