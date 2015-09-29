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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;
using System.Data;
using System.Configuration;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Web.Security;
using Npgsql;
using NpgsqlTypes;
using System.Security.Cryptography;

public class Utility
{
    public static string DBString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
    public static string Message
    {
        get { return (string)HttpContext.Current.Session["Message"]; }
        set { HttpContext.Current.Session["Message"] = value; }
    }

    public static string[] GetKernels()
    {
        Utility settings = new Utility();
        string kernelPath = null;
        kernelPath = settings.GetSettings("Tftp Path") + "kernels" + Path.DirectorySeparatorChar;
        String[] kernelFiles = null;
        try
        {
            kernelFiles = Directory.GetFiles(kernelPath, "*.*");

            for (int x = 0; x < kernelFiles.Length; x++)
                kernelFiles[x] = Path.GetFileName(kernelFiles[x]);
        }

        catch(Exception ex)
        {
            Logger.Log(ex.Message);
        }
        return kernelFiles;
    }

    public static string[] GetScripts()
    {
        Utility settings = new Utility();
        string scriptPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "clientscripts" + Path.DirectorySeparatorChar;
        String[] scriptFiles = null;
        try
        {
            scriptFiles = Directory.GetFiles(scriptPath, "*.*");
            for (int x = 0; x < scriptFiles.Length; x++)
                scriptFiles[x] = Path.GetFileName(scriptFiles[x]);
        }

        catch (Exception ex)
        {
            Logger.Log(ex.Message);
        }
        return scriptFiles;
    }

    public static string[] GetBootImages()
    {     
        Utility settings = new Utility();
        string bootImagePath = null;
        bootImagePath = settings.GetSettings("Tftp Path") + "images" + Path.DirectorySeparatorChar;

        String[] bootImageFiles = null;
        try
        {
            bootImageFiles = Directory.GetFiles(bootImagePath, "*.*");

            for (int x = 0; x < bootImageFiles.Length; x++)
                bootImageFiles[x] = Path.GetFileName(bootImageFiles[x]);
        }
        catch(Exception ex)
        {
            Logger.Log(ex.Message);
        }
        return bootImageFiles;
    }

    public static string GenerateKey()
    {
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string key = null;
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(timestamp));
            key = new Guid(hash).ToString();
        }

        return key.Substring(0,18);
    }

    public static string[] GetLogs()
    {
        string logPath = null;

        logPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar;

        String[] logFiles = Directory.GetFiles(logPath, "*.*");

        for (int x = 0; x < logFiles.Length; x++)
            logFiles[x] = Path.GetFileName(logFiles[x]);

        return logFiles;
    }

    public void DeleteAllFiles(string directoryPath)
    {
         string[] directories = Directory.GetDirectories(directoryPath);
         foreach (string dirPath in directories)
         {
              if (Directory.Exists(dirPath))
                   Directory.Delete(dirPath, true);
         }

         string[] files = Directory.GetFiles(directoryPath);
         foreach (string filePath in files)
         {
              if(File.Exists(filePath))
                    File.Delete(filePath);
         }
    }

    public string GetServerIP()
    {
        string ipAddress = GetSettings("Server IP");
        string port = GetSettings("Web Server Port");
        if(port != "80" && port != "443" && !string.IsNullOrEmpty(port))
        {
            ipAddress += ":" + port;
        }

        return ipAddress;
    }
    public string GetSettings(string settingName)
    {
        string settingValue = null;

        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
        {        
            NpgsqlCommand cmd = new NpgsqlCommand("settings_getsettings", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@settingName", settingName);
            conn.Open();
            settingValue = cmd.ExecuteScalar().ToString();
        }

        //Handle replacement of [server-ip] placeholder as well as ip addresses on different ports
        if (settingName == "Web Path" || settingName == "Nfs Upload Path" || settingName == "Nfs Deploy Path" || settingName == "SMB Path")
        {
            if (settingValue.Contains("[server-ip]"))
            {
                settingValue = settingValue.Replace("[server-ip]", GetSettings("Server IP"));      
            }           
        }           
        return settingValue;
    }

    //Overloaded to handle viewing settings correctly on the admin settings page without needing to redo all other function calls
    public string GetSettings(string settingName, bool isAdminSettings)
    {
        string settingValue = null;

        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
        {
            NpgsqlCommand cmd = new NpgsqlCommand("settings_getsettings", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@settingName", settingName);
            conn.Open();
            settingValue = cmd.ExecuteScalar().ToString();
        }

        return settingValue;
    }

    public static List<string> PopulateImagesDdl()
    {
        var listImages = new List<string>();

        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
        {
            NpgsqlCommand cmd = new NpgsqlCommand("images_getimages", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
                listImages.Add((string)rdr["images_getimages"]);
        }
        return listImages;
    }

    public static List<string> PopulateGroupsDdl()
    {
        var listGroups = new List<string>();

        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
        {
            NpgsqlCommand cmd = new NpgsqlCommand("groups_getgroups", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
                listGroups.Add((string)rdr["groups_getgroups"]);
        }

        return listGroups;
    }

    public static List<string> PopulateBootMenusDdl()
    {
        var listMenus = new List<string>();

        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
        {
            NpgsqlCommand cmd = new NpgsqlCommand("boottemplates_get", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
                listMenus.Add((string)rdr["boottemplates_get"]);
        }
        return listMenus;
    }

    public string CreatePasswordHash(string pwd, string salt)
    {
        string saltAndPwd = String.Concat(pwd, salt);
        string hashedPwd = FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, "sha1");
        return hashedPwd;
    }

    public string GetSalt(string userName)
    {
        string salt = null;
        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
        {

            NpgsqlCommand cmd = new NpgsqlCommand("users_getsalt", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new NpgsqlParameter("@userName", userName));
            conn.Open();
            salt = cmd.ExecuteScalar() as string;
        }

        return salt;
    }

    public bool UserLogin(string userName, string password)
    {
        string result = null;

        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
        {
            NpgsqlCommand cmd = new NpgsqlCommand("users_validatelogin", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new NpgsqlParameter("@userName", userName));
            cmd.Parameters.Add(new NpgsqlParameter("@userPwd", CreatePasswordHash(password, GetSalt(userName))));
            conn.Open();
            result = cmd.ExecuteScalar() as string;
        }
        if (string.IsNullOrEmpty(result))
            return false;
        else
            return true;
    }

    public static bool NoSpaceNotEmpty(string field)
    {
        if (string.IsNullOrEmpty(field))
            return false;
        else
        {
            if (field.Contains(" ") || (field.Contains("Select Kernel")) || (field.Contains("Select Boot Image")) )
                return false;
            else
                return true;
        }
    }

    public string Decode(string encoded)
    {
        string decoded = null;
        try
        {
            byte[] dBytes = Convert.FromBase64String(encoded);
            decoded = Encoding.UTF8.GetString(dBytes);
        }
        catch (Exception ex)
        {
            Logger.Log("Decoding Failed. " + ex.Message);
        }

        return decoded;
    }

    public static void MoveFile(string sourceFileName, string destFileName)
    {
        if (File.Exists(destFileName))
        {
            File.Delete(destFileName);
        }

        File.Move(sourceFileName, destFileName);
    }

    public static void MoveFolder(string sourceFolderName, string destFolderName)
    {
        if (Directory.Exists(destFolderName))
        {
            Directory.Delete(destFolderName,true);
        }

        Directory.Move(sourceFolderName, destFolderName);
    }

    public static string FixMac(string mac)
    {
        if (mac.Length == 12)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mac.Length; i++)
            {
                if (i % 2 == 0 && i != 0)
                    sb.Append(':');
                sb.Append(mac[i]);
            }
            mac = sb.ToString();
        }
        else
            mac = mac.Replace('-', ':');

        return mac.ToUpper();
    }
}

