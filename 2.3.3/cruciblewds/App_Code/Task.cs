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
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Management;
using Npgsql;
using System.Threading;

public class Task
{
    public string HostName { get; set; }
    public string Progress { get; set; }

    public bool CleanPxeBoot(string pxeHostMac)
    {
        Utility settings = new Utility();
        Host hostFunction = new Host();
        string mode = settings.GetSettings("PXE Mode");
        bool isCustomBootTemplate = hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(pxeHostMac));
        string proxyDHCP = settings.GetSettings("Proxy Dhcp");
        string biosFile = settings.GetSettings("Proxy Bios File");
        string efi32File = settings.GetSettings("Proxy Efi32 File");
        string efi64File = settings.GetSettings("Proxy Efi64 File");

        if (proxyDHCP == "Yes")
        {
            try
            {

                    File.Delete(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                    if (isCustomBootTemplate)
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);


                    File.Delete(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");
                    if (isCustomBootTemplate)
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");



                    File.Delete(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                    if (isCustomBootTemplate)
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);



                    File.Delete(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");
                    if (isCustomBootTemplate)
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");




                    File.Delete(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                    if (isCustomBootTemplate)
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                

 
                    File.Delete(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");
                    if (isCustomBootTemplate)
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom", settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");



                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Utility.Message = "Could Not Delete PXE File";
                return false;
            }
        }
        else
        {
            try
            {
                if (mode.Contains("ipxe"))
                {
                    File.Delete(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");
                    if (isCustomBootTemplate)
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe.custom", settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe");
                }
                   
                else
                {
                   File.Delete(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                    if (isCustomBootTemplate)
                        Utility.MoveFile(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".custom", settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Utility.Message = "Could Not Delete PXE File";
                return false;
            }
        }
    }

    public void DeleteActiveMcTask(int mcTaskID, string groupName, string PID)
    {
        Task task = new Task();
        List<string> hostMacs = new List<string>();
        hostMacs = task.DeleteActiveMc(mcTaskID, groupName);

            for (int i = 0; i < hostMacs.Count; i++)
            {
                string pxeHostMac = task.MacToPXE(hostMacs[i]);
                task.CleanPxeBoot(pxeHostMac);
            }

            try
            {
                Process prs = Process.GetProcessById(Convert.ToInt32(PID));
                string processName = prs.ProcessName;
                if (Environment.OSVersion.ToString().Contains("Unix"))
                {
                    while (!prs.HasExited)
                    {
                        task.KillProcessLinux(Convert.ToInt32(PID));
                        Thread.Sleep(1000);
                        
                    }
                }
                else
                {
                    if (processName == "cmd")
                        task.KillProcess(Convert.ToInt32(PID));
                }
                Utility.Message = "Successfully Deleted Task";
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Utility.Message = "Could Not Kill Process.  Check The Exception Log For More Info";
            }
    }

    public void DeleteActiveTask(int taskID)
    {
        Task task = new Task();
        string hostMac = task.DeleteActive(taskID);
        if (hostMac != "error")
        {
            string pxeHostMac = task.MacToPXE(hostMac);
            if (task.CleanPxeBoot(pxeHostMac))
                Utility.Message = "Successfully Deleted Task ";
        }
        else
            Utility.Message = "Could Not Delete Active Task.  Check Exception Log For More Info";
    }

    private void KillProcess(int pid)
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
        ManagementObjectCollection moc = searcher.Get();
        foreach (ManagementObject mo in moc)
        {
            KillProcess(Convert.ToInt32(mo["ProcessID"]));
        }
        try
        {
            Process proc = Process.GetProcessById(Convert.ToInt32(pid));
            proc.Kill();
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Kill Process.  Check The Exception Log For More Info";
        }
    }

    private void KillProcessLinux(int pid)
    {
        try
        {
             string shell = null;
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

            Process killProc = null;
            ProcessStartInfo killProcInfo = new ProcessStartInfo();
            killProcInfo.FileName = (shell);
            killProcInfo.Arguments = (" -c \"pkill -TERM -P " + pid + "\"");                                  
            killProc = Process.Start(killProcInfo);  
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Kill Process.  Check The Exception Log For More Info";
        }
    }

    public string MacToPXE(string mac)
    {
        string pxeMac = "01-" + mac.ToLower().Replace(':', '-');
        return pxeMac;
    }

    public static string PXEMacToMac(string PXEmac)
    {
        string mac = PXEmac.Remove(0, 3);
        mac = mac.ToUpper().Replace('-', ':');
        return mac;
    }

    public void WakeUp(string mac)
    {

        Regex pattern = new Regex("[:]");
        string wolHostMac = pattern.Replace(mac, "");

        try
        {
            long value = long.Parse(wolHostMac, NumberStyles.HexNumber, CultureInfo.CurrentCulture.NumberFormat);
            byte[] macBytes = BitConverter.GetBytes(value);

            Array.Reverse(macBytes);
            byte[] macAddress = new byte[6];

            for (int j = 0; j < 6; j++)
                macAddress[j] = macBytes[j + 2];


            byte[] packet = new byte[17 * 6];

            for (int i = 0; i < 6; i++)
                packet[i] = 0xff;

            for (int i = 1; i <= 16; i++)
            {
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = macAddress[j];
            }

            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Broadcast, 9);
            client.Send(packet, packet.Length);
        }
        catch { }
    }

    public string DeleteActive(int taskID)
    {
        string hostMac;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_deleteactive", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@taskID", taskID));
                conn.Open();
                hostMac = cmd.ExecuteScalar() as string;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            hostMac = "error";
        }

        return hostMac;       
    }

    public List<string> DeleteActiveMc(int mcTaskID, string groupName)
    {
        List<string> hostMacs = new List<string>();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_deleteactivemc", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@taskid", mcTaskID));
                cmd.Parameters.Add(new NpgsqlParameter("@taskname", groupName));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    hostMacs.Add((string)rdr["result"]);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Delete Task.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString()); 
        }

        return hostMacs;
    }

    public DataTable ReadMcMembers(string groupName)
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_readmcmembers", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@tasknamemc", groupName));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Read Multicast Members.  Check The Exception Log For More Info";
        }
        return table;
    }

    public DataTable ReadActive()
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_readactive", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Read Active Tasks.  Check The Exception Log For More Info";
        }
        return table;
    }

    public DataTable ReadActiveUC()
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_readactiveuc", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Read Active Tasks.  Check The Exception Log For More Info";
        }
        return table;
    }

    public DataTable ReadMCProgress(string groupName)
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_readmcprogress", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@tasknamemc", groupName));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Read Active Tasks.  Check The Exception Log For More Info";
        }
        return table;
    }

    public void UpdateProgress(Task task)
    {
        List<string> values = task.Progress.Split('*').ToList();


            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_updateprogress", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@elapsed", values[1]));
                cmd.Parameters.Add(new NpgsqlParameter("@remaining", values[2]));
                cmd.Parameters.Add(new NpgsqlParameter("@completed", values[3]));
                cmd.Parameters.Add(new NpgsqlParameter("@rate", values[4]));
                cmd.Parameters.Add(new NpgsqlParameter("@hostName", task.HostName));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        

    }

    public int GetPort()
    {
        int port = 0;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("multicast_readport", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                port = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Determine Port Base.  Check The Exception Log For More Info.";
            Logger.Log(ex.ToString());
        }
        return port;
    }

    public void UpdateProgressPartition(string hostName, string partition)
    {
        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
        {
            NpgsqlCommand cmd = new NpgsqlCommand("tasks_updateprogresspartition", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new NpgsqlParameter("@hostName", hostName));
            cmd.Parameters.Add(new NpgsqlParameter("@partition", partition));
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public bool CreateTaskArgs(string taskArgs, string taskID)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_updateargs", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@taskargs", taskArgs));
                cmd.Parameters.Add(new NpgsqlParameter("@taskID", taskID));
                conn.Open();
                cmd.ExecuteNonQuery();
                return true;
            }
        }
        catch(Exception ex)
        {
            Logger.Log(ex.Message);
            return false;
        }
    }

    public DataTable ReadActiveMC()
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_readactivemc", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message = "Could Not Read Active Multicast Tasks.  Check The Exception Log For More Info";
        }
        return table;
    }

    public void CancelAll()
    {
        Utility settings = new Utility();
        List<String> pxePaths = new List<string>();
        pxePaths.Add(settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar);
        pxePaths.Add(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "bios" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar);
        pxePaths.Add(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi32" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar);
        pxePaths.Add(settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + "efi64" + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar);

       
        List<string> doNotRemove = new List<string>();

        int counter = 0;
        foreach (var pxePath in pxePaths)
        {
            string[] pxeFiles = Directory.GetFiles(pxePath, "01*");
            try
            {
                foreach (string pxeFile in pxeFiles)
                {
                    string ext = Path.GetExtension(pxeFile);

                    if (ext != ".custom")
                    {
                        Host hostFunction = new Host();
                        string fileName = Path.GetFileNameWithoutExtension(pxeFile);
                        bool isCustomBootTemplate = hostFunction.IsCustomBootEnabled(Task.PXEMacToMac(fileName));
                        if (isCustomBootTemplate)
                        {
                            if (File.Exists((pxePath + fileName + ".custom")))
                            {
                                Utility.MoveFile(pxePath + fileName + ".custom", pxeFile);
                                doNotRemove.Add(pxeFile);
                            }
                            if (File.Exists((pxePath + fileName + ".ipxe.custom")))
                            {
                                Utility.MoveFile(pxePath + fileName + ".ipxe.custom", pxeFile);
                                doNotRemove.Add(pxeFile);
                            }
                        }
                        else
                            if (!doNotRemove.Contains(pxeFile))
                                File.Delete(pxeFile);
                    }


                }

                Utility.Message = "Deleted All PXE Files<br>";
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                Utility.Message += "Could Not Delete PXE Files<br>";
            }
            counter++;
        }

        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("tasks_cancelall", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            Utility.Message += "Deleted All Tasks From Database<br>";
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            Utility.Message += "Could Not Delete All Tasks From Database<br>";
        }
        if (Environment.OSVersion.ToString().Contains("Unix"))
        {
             for (int x = 1; x < 10; x++)
             {
                  try
                  {
                       Process killProc = null;
                       ProcessStartInfo killProcInfo = new ProcessStartInfo();
                       killProcInfo.FileName = ("killall");
                       killProcInfo.Arguments = (" udp-sender udp-receiver");
                       killProc = Process.Start(killProcInfo);

                  }
                  catch
                  {
                  }

                  Thread.Sleep(200);
             }
        }

        else
        {
             for (int x = 1; x < 10; x++)
             {
                  foreach (Process p in System.Diagnostics.Process.GetProcessesByName("udp-sender"))
                  {
                       try
                       {
                            p.Kill();
                            p.WaitForExit();
                            //Utility.Message += "Deleted An Instance Of Udp-Sender<br>";
                       }
                       catch (Exception ex)
                       {
                            Logger.Log(ex.ToString());
                            //Utility.Message += "Could Not Delete Instance Of Udp-Sender<br>";

                       }
                  }
                  Thread.Sleep(200);
             }

             for (int x = 1; x < 10; x++)
             {
                  foreach (Process p in System.Diagnostics.Process.GetProcessesByName("udp-receiver"))
                  {
                       try
                       {
                            p.Kill();
                            p.WaitForExit();
                            //Utility.Message += "Deleted An Instance Of Udp-Sender<br>";
                       }
                       catch (Exception ex)
                       {
                            Logger.Log(ex.ToString());
                            //Utility.Message += "Could Not Delete Instance Of Udp-Sender<br>";

                       }
                  }
                  Thread.Sleep(200);
             }
        }

    }

}
