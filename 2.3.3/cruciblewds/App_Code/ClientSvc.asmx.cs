using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Npgsql;
using System.Data;
using NpgsqlTypes;
using System.Diagnostics;
using System.Threading;
using System.DirectoryServices.AccountManagement;
using Newtonsoft.Json;

namespace CrucibleWDS
{
   
    [WebService(Namespace = "http://localhost/cruciblewds/ClientSvc.asmx")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
 
    [System.Web.Script.Services.ScriptService]

    public class ClientSvc : System.Web.Services.WebService
    {
        string[] ImageExtensions = new string[3] { ".gz", ".lz4", ".none" }; // modified by cocoon
        string[] ImagePartTypes = new string[5] { ".ntfs", ".fat", ".extfs",".hfsp", ".imager" }; // modified by cocoon

        #region On Demand

         [WebMethod]
         public void listimages()
         {
              Utility utility = new Utility();
              HttpContext postedContext = HttpContext.Current;
              HttpFileCollection Files = postedContext.Request.Files;

              string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);

              if (utility.GetSettings("On Demand") == "Enabled")
              {
                   if (serverKey == utility.GetSettings("Server Key"))
                   {
                        var listImages = new List<string>();
                        string result = null;
                        try
                        {
                             using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                             {
                                  NpgsqlCommand cmd = new NpgsqlCommand("images_getimagescustom", conn);
                                  cmd.CommandType = CommandType.StoredProcedure;
                                  conn.Open();
                                  NpgsqlDataReader rdr = cmd.ExecuteReader();
                                  while (rdr.Read())
                                       listImages.Add(rdr["imageid"].ToString() + " " + (string)rdr["imagename"]);
                             }
                             foreach (string image in listImages)
                                  result += image + ",";
                        }
                        catch (Exception ex)
                        {
                             result = "Could Not Read Image List.  Check The Exception Log For More Info";
                             Logger.Log(ex.ToString());
                        }
                        HttpContext.Current.Response.Write(result);
                   }
                   else
                   {
                        Logger.Log("An Incorrect Key Was Provided While Trying To List Images " + serverKey );
                   }
              }
              else
              {
                   Logger.Log("On Demand Mode Is Globally Disabled.");
              }
         }

         [WebMethod]
         public void mcinfo()
         {
              Utility utility = new Utility();
              HttpContext postedContext = HttpContext.Current;
              HttpFileCollection Files = postedContext.Request.Files;

              string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
              string mcTaskName = utility.Decode((string)postedContext.Request.Form["mcTaskName"]);
              string mac = utility.Decode((string)postedContext.Request.Form["mac"]);

              if (utility.GetSettings("On Demand") == "Enabled")
              {
                   if (serverKey == utility.GetSettings("Server Key"))
                   {
                        string imageOS = null;
                        string imageName = null;
                        string hostName = null;
                        string portBase = null;
                        string result = null;
                        try
                        {
                             using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                             {
                                  NpgsqlCommand cmd = new NpgsqlCommand("client_custommcinfo", conn);
                                  cmd.CommandType = CommandType.StoredProcedure;
                                  cmd.Parameters.Add(new NpgsqlParameter("@mcTaskName", mcTaskName));
                                  cmd.Parameters.Add(new NpgsqlParameter("@hostMac", mac));
                                  cmd.Parameters.Add(new NpgsqlParameter("@imageOSResult", NpgsqlDbType.Varchar, 100));
                                  cmd.Parameters["@imageOSResult"].Direction = ParameterDirection.Output;
                                  cmd.Parameters.Add(new NpgsqlParameter("@imageNameResult", NpgsqlDbType.Varchar, 100));
                                  cmd.Parameters["@imageNameResult"].Direction = ParameterDirection.Output;
                                  cmd.Parameters.Add(new NpgsqlParameter("@hostNameResult", NpgsqlDbType.Varchar, 100));
                                  cmd.Parameters["@hostNameResult"].Direction = ParameterDirection.Output;
                                  cmd.Parameters.Add(new NpgsqlParameter("@portBaseResult", NpgsqlDbType.Varchar, 100));
                                  cmd.Parameters["@portBaseResult"].Direction = ParameterDirection.Output;
                                  conn.Open();
                                  cmd.ExecuteNonQuery();
                                  imageOS = (cmd.Parameters["@imageOSResult"].Value as string);
                                  hostName = (cmd.Parameters["@hostNameResult"].Value as string);
                                  portBase = (cmd.Parameters["@portBaseResult"].Value as string);
                                  imageName = (cmd.Parameters["@imageNameResult"].Value as string);
                                  result = "imgOS=" + imageOS + " portBase=" + portBase + " imgName=" + imageName;
                             }
                        }
                        catch (Exception ex)
                        {
                             result = "Could Not Read Multicast Info.  Check The Exception Log For More Info";
                             Logger.Log(ex.ToString());
                        }
                        HttpContext.Current.Response.Write(result);
                   }
                   else
                   {
                        Logger.Log("An Incorrect Key Was Provided While Trying To List Images");
                   }
              }
              else
              {
                   Logger.Log("On Demand Mode Is Globally Disabled.");
              }
         }

         [WebMethod(EnableSession = true)]
         public void mcsessions()
         {
              Utility utility = new Utility();
              HttpContext postedContext = HttpContext.Current;
              HttpFileCollection Files = postedContext.Request.Files;

              string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);

              if (utility.GetSettings("On Demand") == "Enabled")
              {
                   if (serverKey == utility.GetSettings("Server Key"))
                   {
                        string result = null;
                        var listSessions = new List<string>();
                        NpgsqlDataReader rdr = null;
                        try
                        {
                             using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                             {
                                  NpgsqlCommand cmd = new NpgsqlCommand("client_custommcsessions", conn);
                                  cmd.CommandType = CommandType.StoredProcedure;
                                  conn.Open();
                                  rdr = cmd.ExecuteReader();
                                  while (rdr.Read())
                                  {
                                       int n;
                                       bool isNumeric = int.TryParse((string)rdr["client_custommcsessions"], out n);
                                       if (isNumeric)
                                            listSessions.Add(n.ToString());
                                  }
                             }

                             if (listSessions.Count == 0)
                                  result = "There Are No Active Sessions";
                             else
                             {
                                  foreach (string session in listSessions)
                                       result += session + " ";
                             }
                        }
                        catch (Exception ex)
                        {
                             result = "Could Not Read Active Multicasts.  Check The Exception Log For More Info";
                             Logger.Log(ex.ToString());
                        }
                        HttpContext.Current.Session.Remove("Message");
                        HttpContext.Current.Response.Write(result);
                   }
                   else
                   {
                        Logger.Log("An Incorrect Key Was Provided While Trying To List Images");
                   }
              }
              else
              {
                   Logger.Log("On Demand Mode Is Globally Disabled.");
              }
         }

         [WebMethod]
         public void ucinfo()
         {
             Utility utility = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;

             string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
             string direction = utility.Decode((string)postedContext.Request.Form["direction"]);
             string mac = utility.Decode((string)postedContext.Request.Form["mac"]);
             string imageID = utility.Decode((string)postedContext.Request.Form["imageID"]);

             if (utility.GetSettings("On Demand") == "Enabled")
             {
                 if (serverKey == utility.GetSettings("Server Key"))
                 {
                     Image image = new Image();
                     if (direction == "push")
                     {
                         if (!image.Check_Checksum(imageID))
                         {
                             HttpContext.Current.Response.Write("Client Error: This Image Has Not Been Confirmed And Cannot Be Deployed.");
                             return;
                         }
                     }

                     string storage = null;
                     string imageOS = null;
                     string hostName = null;
                     string hostScripts = null;
                     string hostArgs = null;
                     string serverIP = utility.GetSettings("Server IP");
                     string xferMode = utility.GetSettings("Image Transfer Mode");
                     string compAlg = utility.GetSettings("Compression Algorithm");
                     string compLevel = utility.GetSettings("Compression Level");
                     string imageProtected = null;
                     string imageName = null;
                     string result = null;
                     if (xferMode == "smb" || xferMode == "smb+http")
                         storage = utility.GetSettings("SMB Path");
                     else
                     {
                         if (direction == "pull")
                             storage = utility.GetSettings("Nfs Upload Path");
                         else
                             storage = utility.GetSettings("Nfs Deploy Path");
                     }
                     try
                     {
                         using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                         {
                             NpgsqlCommand cmd = new NpgsqlCommand("client_customunicastinfo", conn);
                             cmd.CommandType = CommandType.StoredProcedure;
                             cmd.Parameters.Add(new NpgsqlParameter("@imageName", imageID));
                             cmd.Parameters.Add(new NpgsqlParameter("@hostMac", mac));
                             cmd.Parameters.Add(new NpgsqlParameter("@imageOSResult", NpgsqlDbType.Varchar, 100));
                             cmd.Parameters["@imageOSResult"].Direction = ParameterDirection.Output;
                             cmd.Parameters.Add(new NpgsqlParameter("@hostNameResult", NpgsqlDbType.Varchar, 100));
                             cmd.Parameters["@hostNameResult"].Direction = ParameterDirection.Output;
                             cmd.Parameters.Add(new NpgsqlParameter("@hostscripts", NpgsqlDbType.Varchar, 100));
                             cmd.Parameters["@hostscripts"].Direction = ParameterDirection.Output;
                             cmd.Parameters.Add(new NpgsqlParameter("@hostargs", NpgsqlDbType.Varchar, 100));
                             cmd.Parameters["@hostargs"].Direction = ParameterDirection.Output;
                             cmd.Parameters.Add(new NpgsqlParameter("@imageprotected", NpgsqlDbType.Varchar, 100));
                             cmd.Parameters["@imageprotected"].Direction = ParameterDirection.Output;
                             cmd.Parameters.Add(new NpgsqlParameter("@imagename", NpgsqlDbType.Varchar, 100));
                             cmd.Parameters["@imagename"].Direction = ParameterDirection.Output;

                             conn.Open();
                             cmd.ExecuteNonQuery();
                             imageOS = (cmd.Parameters["@imageOSResult"].Value as string);
                             hostName = (cmd.Parameters["@hostNameResult"].Value as string);
                             hostScripts = (cmd.Parameters["@hostscripts"].Value as string);
                             hostArgs = (cmd.Parameters["@hostargs"].Value as string);
                             imageProtected = (cmd.Parameters["@imageprotected"].Value as string);
                             imageName = (cmd.Parameters["@imagename"].Value as string);
                         }

                         result = "imgOS=" + imageOS + " imgName=" + imageName + " hostName=" + hostName + " hostScripts=" + "\"" + hostScripts + "\" " + hostArgs + " storage=" +
                             storage + " serverIP=" + serverIP + " xferMode=" + xferMode + " compAlg=" + compAlg + " compLevel=-" + compLevel + " imageProtected=" + imageProtected;

                         if (direction == "pull" && utility.GetSettings("Image Transfer Mode") == "udp+http")
                         {
                             Task task = new Task();
                             int portBase = task.GetPort();
                             result = result + " portBase=" + portBase;
                         }
                     }
                     catch (Exception ex)
                     {
                         result = "Client Error: Could Not Read Custom Unicast Info.  Check The Exception Log For More Info";
                         Logger.Log(ex.ToString());
                     }
                     HttpContext.Current.Response.Write(result);


                 }
                 else
                 {
                     Logger.Log("An Incorrect Key Was Provided While Trying To List Images");
                 }
             }
             else
             {
                 Logger.Log("On Demand Mode Is Globally Disabled.");
             }
         }
        #endregion

        #region Upload

        [WebMethod]
        public void imagesize()
        {
             Utility utility = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;
             string imagename = utility.Decode((string)postedContext.Request.Form["imgName"]);
             string imagesize = utility.Decode((string)postedContext.Request.Form["imageSize"]);
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("images_updatesize", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new NpgsqlParameter("@imagename", imagename));
                    cmd.Parameters.Add(new NpgsqlParameter("@imagesize", imagesize));
                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
                
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                HttpContext.Current.Response.Write("false");
                return;
            }

            //Validate image specifications were recorded successfully
            Image image = new Image();
            image.ID = image.GetImageID(imagename);
            image.Read(image);
            Image_Physical_Specs existingips = new Image_Physical_Specs();
            try
            {
                existingips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);
            }
            catch(Exception ex)
            {
                Logger.Log(ex.ToString());
                HttpContext.Current.Response.Write("false");
                return;
            }
            if (existingips.hd == null)
            {
                HttpContext.Current.Response.Write("false");
                return;
            }
            //Reset Custom Specs
            image.UpdateSpecs(image.Name, "");
            HttpContext.Current.Response.Write("true");

        }

        [WebMethod]
        public void startreceiver()
        {
            string result = null;
            Host host = new Host();
            Utility utility = new Utility();
            HttpContext postedContext = HttpContext.Current;
            HttpFileCollection Files = postedContext.Request.Files;

            string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
            string imagePath = utility.Decode((string)postedContext.Request.Form["imgPath"]);
            string port = utility.Decode((string)postedContext.Request.Form["portBase"]);

            if (serverKey == utility.GetSettings("Server Key"))
            {
                    string compAlgorithm = utility.GetSettings("Compression Algorithm");
                    imagePath = imagePath.Replace('/', Path.DirectorySeparatorChar);
                    string compExt = null;

                // modified by cocoon^M
                    /*
                    if (compAlgorithm == "lz4")
                        compExt = ".lz4";
                    else
                        compExt = ".gz";
                    */
                    switch (compAlgorithm)
                    {
                        case "lz4":
                            compExt = ".lz4";
                            break;

                        case "gzip":
                            compExt = ".gz";
                            break;

                        case "none":
                            compExt = ".none";
                            break;

                        default:
                            compExt = ".gz";
                            break;
                    }



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

                    Process receiver = null;
                    ProcessStartInfo receiverInfo = new ProcessStartInfo();
                    receiverInfo.FileName = (shell);

                    // modified by cocoon
                    if (Environment.OSVersion.ToString().Contains("Unix"))
                    {
                        if (compExt == ".none")
                        {
                            receiverInfo.Arguments = (" -c \"" + "udp-receiver" + " --portbase " + port + " " + 
                                utility.GetSettings("Receiver Args") + " --start-timeout 30 --file " + 
                                utility.GetSettings("Image Store Path") + imagePath + compExt + "\"");
                        }
                        else
                        {
                            receiverInfo.Arguments = (" -c \"" + "udp-receiver" + " --portbase " + port + " " + 
                                utility.GetSettings("Receiver Args") + " --start-timeout 30 | " + " " + compAlgorithm + " -" + 
                                utility.GetSettings("Compression Level") + " > " + 
                                utility.GetSettings("Image Store Path") + imagePath + compExt + "\"");
                        }

                    }
                    else
                    {
                        if (compExt == ".none")
                        {
                            receiverInfo.Arguments = (" /c " + appPath + "udp-receiver.exe" + " --portbase " + port + " " + utility.GetSettings("Receiver Args") + " --start-timeout 30 --file " + 
                                utility.GetSettings("Image Store Path") + imagePath + compExt);
                        }
                        else
                        {
                            receiverInfo.Arguments = (" /c " + appPath + "udp-receiver.exe" + " --portbase " + port + " " + utility.GetSettings("Receiver Args") + " --start-timeout 30 | " +
                                appPath + compAlgorithm + " -" + utility.GetSettings("Compression Level") + " > " + utility.GetSettings("Image Store Path") + imagePath + compExt);
                        }
                    }

                    string log = ("\r\n" + DateTime.Now.ToString("MM.dd.yy hh:mm") + " Starting Unicast Upload " + imagePath +
                                  " With The Following Command:\r\n\r\n" + receiverInfo.FileName + receiverInfo.Arguments + "\r\n\r\n");
                    System.IO.File.AppendAllText(logPath + "unicast.log", log);

                    try
                    {
                        receiver = Process.Start(receiverInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.ToString());
                        result = "Could Not Start Upload .  Check The Exception Log For More Info";
                        System.IO.File.AppendAllText(logPath + "unicast.log", "Could Not Start Session " + imagePath + "Try Pasting The Command Into A Command Prompt");
                    }

                    Thread.Sleep(2000);

                    if (receiver.HasExited)
                    {
                        result = "Could Not Start Upload .  Check The Exception Log For More Info";
                        System.IO.File.AppendAllText(logPath + "multicast.log", "Session " + imagePath + @" Started And Was Forced To Quit, Try Pasting The Command Into A Command Prompt");

                    }
                    result = "true";
            }
            else
                result = "Server Key Did Not Match";

            HttpContext.Current.Response.Write(result);
        }

        [WebMethod]
        public void createdirectory()
        {
             Host host = new Host();
             Image image = new Image();
             Utility utility = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;

             string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
             string imageName = utility.Decode((string)postedContext.Request.Form["imgName"]);
             string dirName = utility.Decode((string)postedContext.Request.Form["dirName"]);

             if (serverKey == utility.GetSettings("Server Key"))
             {
                  if (!image.ImageProtected(imageName))
                  {
                       try
                       {
                            if (utility.GetSettings("Image Transfer Mode") == "udp+http")
                                 Directory.CreateDirectory(utility.GetSettings("Image Store Path") + imageName + Path.DirectorySeparatorChar + "hd" + dirName);
                            else
                                 Directory.CreateDirectory(utility.GetSettings("Image Hold Path") + imageName + Path.DirectorySeparatorChar + "hd" + dirName);
                       }
                       catch (Exception ex)
                       {
                            Logger.Log(ex.Message);
                            HttpContext.Current.Response.Write("Could Not Create Directory");
                       }
                       HttpContext.Current.Response.Write("true");
                  }
                  else
                       HttpContext.Current.Response.Write("Image Is Protected");
             }
             else
                  HttpContext.Current.Response.Write("Server Key Did Not Match");
        }

        [WebMethod]
        public void deleteimage()
        {
             Host host = new Host();

             Image image = new Image();
             Utility utility = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;

             string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
             string imageName = utility.Decode((string)postedContext.Request.Form["imgName"]);

             if (serverKey == utility.GetSettings("Server Key"))
             {
                  if (!image.ImageProtected(imageName))
                  {
                       try
                       {
                            if (Directory.Exists(utility.GetSettings("Image Store Path") + imageName))
                                 utility.DeleteAllFiles(utility.GetSettings("Image Store Path") + imageName);

                            if (Directory.Exists(utility.GetSettings("Image Hold Path") + imageName))
                                 utility.DeleteAllFiles(utility.GetSettings("Image Hold Path") + imageName);
                       }
                       catch (Exception ex)
                       {
                            Logger.Log(ex.Message);
                            HttpContext.Current.Response.Write("Could Not Delete Existing Image");
                       }

                       try
                       {
                            if (utility.GetSettings("Image Transfer Mode") == "udp+http")
                                 Directory.CreateDirectory(utility.GetSettings("Image Store Path") + imageName);
                            else
                                 Directory.CreateDirectory(utility.GetSettings("Image Hold Path") + imageName);
                       }
                       catch (Exception ex)
                       {
                            Logger.Log(ex.Message);
                            HttpContext.Current.Response.Write("Could Not Delete Existing Image");
                       }
                       HttpContext.Current.Response.Write("true");
                  }
                  else
                       HttpContext.Current.Response.Write("Image Is Protected");
             }
             else
                  HttpContext.Current.Response.Write("Server Key Did Not Match");
        }

        [WebMethod]
        public void upload()
        {
            Host host = new Host();
            Utility utility = new Utility();
            try
            {
                HttpContext postedContext = HttpContext.Current;
                HttpFileCollection Files = postedContext.Request.Files;

                string fileName = utility.Decode((string)postedContext.Request.Form["fileName"]);
                string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
                string imagePath = utility.Decode((string)postedContext.Request.Form["imagePath"]);            
                string fileType = utility.Decode((string)postedContext.Request.Form["fileType"]);
                string fullPath = null;

                if (fileType == "mbr")
                {
                    imagePath = imagePath.Replace('/', Path.DirectorySeparatorChar);
                    if (utility.GetSettings("Image Transfer Mode") == "udp+http")
                        fullPath = utility.GetSettings("Image Store Path") + imagePath + Path.DirectorySeparatorChar + fileName;
                    else
                        fullPath = utility.GetSettings("Image Hold Path") + imagePath + Path.DirectorySeparatorChar + fileName;
                }
                else if (fileType == "log")
                    if (imagePath == "host")
                        fullPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar + "hosts" + Path.DirectorySeparatorChar + fileName;
                    else
                        fullPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar + fileName;
                else
                {
                    HttpContext.Current.Response.Write("File Type Not Specified");
                    return;
                }

                if (serverKey == utility.GetSettings("Server Key"))
                {
                        if (Files.Count == 1 && Files[0].ContentLength > 0 && fileName != null && fileName != "")
                        {
                            byte[] binaryWriteArray = new
                            byte[Files[0].InputStream.Length];
                            Files[0].InputStream.Read(binaryWriteArray, 0, (int)Files[0].InputStream.Length);
                            FileStream objfilestream = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite);
                            objfilestream.Write(binaryWriteArray, 0, binaryWriteArray.Length);
                            objfilestream.Close();
                            if (File.Exists(fullPath))
                            {
                                FileInfo file = new FileInfo(fullPath);
                                if (file.Length > 0)
                                    HttpContext.Current.Response.Write("true");
                                else
                                    HttpContext.Current.Response.Write("File Has No Size");
                            }
                            else
                                HttpContext.Current.Response.Write("File Could Not Be Created");
                        }
                        else
                           HttpContext.Current.Response.Write("File Was Not Posted Successfully");
                }
                else
                    HttpContext.Current.Response.Write("Incorrect Server Key");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                HttpContext.Current.Response.Write("Check The Exception Log For More Info");
            }
        }

        #endregion

        #region Download

        [WebMethod]
        public void getfilenames()
        {
            Utility utility = new Utility();
            HttpContext postedContext = HttpContext.Current;
            HttpFileCollection Files = postedContext.Request.Files;

            string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);

            if (serverKey == utility.GetSettings("Server Key"))
            {
                 string imageName = utility.Decode((string)postedContext.Request.Form["imgName"]);
                 List<string> dirs = new List<string>();
                 string result = null;
                 string path = utility.GetSettings("Image Store Path") + imageName;
                 dirs.Add(path);

                 for (int x = 2; true; x++)
                 {
                      string subdir = path + Path.DirectorySeparatorChar + "hd" + x;
                      if (Directory.Exists(subdir))
                           dirs.Add(subdir);
                      else
                           break;
                 }

                 foreach (string dirPath in dirs)
                 {
                      var files = Directory.GetFiles(dirPath, "*.*");
                      foreach (var file in files)
                           result += Path.GetFileName(file) + ";";

                      result += ",";
                 }

                 HttpContext.Current.Response.Write(result);
            }
            else
            {
                 HttpContext.Current.Response.Write("false");
                 Logger.Log("Incorrect Key Provided for Client Getfilenames");
            }
        }

        [WebMethod]
        public void downloadimage()
        {
             Utility utility = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;

             string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
             string partName = utility.Decode((string)postedContext.Request.Form["partName"]);
             string imageName = utility.Decode((string)postedContext.Request.Form["imgName"]);

             if (serverKey == utility.GetSettings("Server Key"))
             {
                 
                  HttpContext.Current.Response.ContentType = "application/octet-stream";
                  HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment; filename=" + partName);
                  HttpContext.Current.Response.TransmitFile(utility.GetSettings("Image Store Path") + imageName + Path.DirectorySeparatorChar + partName);
                  HttpContext.Current.Response.End();
             }
             else
             {
                  Logger.Log("Provided Server Key Was Incorrect When Trying To Download Partition" + partName + " Key: " + serverKey);
             }
        }

        #endregion

        #region Global

        [WebMethod]
        public void getHDParameter(string imgName, string paramName, string hdToGet, string partNumber)
        {
            Image image = new Image();
            image.ID = image.GetImageID(imgName);
            image.Read(image);
            Image_Physical_Specs ips = new Image_Physical_Specs();
            if (!string.IsNullOrEmpty(image.ClientSizeCustom))
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
            else
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);

            int activeCounter = Convert.ToInt32(hdToGet);
            int hdNumberToGet = Convert.ToInt32(hdToGet) - 1;

            //Look for first active hd
            if (ips.hd[hdNumberToGet].active != "1")
            {
                while (activeCounter <= ips.hd.Count())
                {
                    if (ips.hd[activeCounter - 1].active == "1")
                    {
                        hdNumberToGet = activeCounter - 1;
                    }
                    activeCounter++;
                }
            }

            if (paramName == "uuid")
            {
                foreach (var partition in ips.hd[hdNumberToGet].partition)
                    if (partition.number == partNumber)
                    {
                        HttpContext.Current.Response.Write(partition.uuid);
                        break;
                    }
            }
            else if (paramName == "lvmswap")
            {
                foreach (var partition in ips.hd[hdNumberToGet].partition)
                {
                    if (partition.vg != null)
                    {
                        if (partition.vg.lv != null)
                        {
                            foreach (var lv in partition.vg.lv)
                            {
                                if (lv.fstype.ToLower() == "swap"  && lv.active == "1")
                                {
                                    HttpContext.Current.Response.Write(lv.vg.Replace("-","--") + "-" + lv.name.Replace("-","--") + "," + lv.uuid);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (paramName == "guid")
            {
                foreach (var partition in ips.hd[hdNumberToGet].partition)
                    if (partition.number == partNumber)
                    {
                        HttpContext.Current.Response.Write(partition.guid);
                        break;
                    }
            }
            else if (paramName == "table")
            {
                HttpContext.Current.Response.Write(ips.hd[hdNumberToGet].table);
            }
            else if (paramName == "partCount")
            {
                int activePartsCounter = 0;
                foreach(var part in ips.hd[hdNumberToGet].partition)
                {
                    if (part.active == "1")
                        activePartsCounter++;
                }
                HttpContext.Current.Response.Write(activePartsCounter.ToString());
            }
            else if (paramName == "activeParts")
            {
                Utility settings = new Utility();
                string parts = null;
                string imageFiles = null;
                String[] partFiles = null;
                string compExt = null;
                foreach (var part in ips.hd[hdNumberToGet].partition)
                {
                    if (part.active == "1")
                    {
                        if (hdNumberToGet == 0)
                            imageFiles = settings.GetSettings("Image Store Path") + imgName;
                        else
                            imageFiles = settings.GetSettings("Image Store Path") + imgName + Path.DirectorySeparatorChar + "hd" + (hdNumberToGet + 1).ToString();

                        try
                        {
                            // modified by cocoon^M
                            foreach (var ext in ImageExtensions)
                            {
                                partFiles = Directory.GetFiles(imageFiles + Path.DirectorySeparatorChar, "*" + ext);
                                if (partFiles != null && partFiles.Length > 0)
                                {
                                    compExt = ext;
                                    break;
                                }
                            }
                            if (partFiles.Length == 0)
                            {
                                Logger.Log("Image Files Could Not Be Located");
                            }
                        }
                        catch
                        {
                            Logger.Log("Image Files Could Not Be Located");
                        }

                        // modified by cocoon^M
                        foreach (var pType in ImagePartTypes)
                        {
                            if (File.Exists(imageFiles + Path.DirectorySeparatorChar + "part" + part.number + pType + compExt))
                            {
                                parts += part.number + " ";
                                break;
                            }
                        }
                    }
                }
                HttpContext.Current.Response.Write(parts);
            }
            else if (paramName == "lvmactiveParts")
            {
                Utility settings = new Utility();
                string parts = null;
                string imageFiles = null;
                String[] partFiles = null;
                string compExt = null;
                foreach (var part in ips.hd[hdNumberToGet].partition)
                {
                    if (part.active != "1")
                        continue;
                    if (hdNumberToGet == 0)
                        imageFiles = settings.GetSettings("Image Store Path") + imgName;
                    else
                        imageFiles = settings.GetSettings("Image Store Path") + imgName + Path.DirectorySeparatorChar + "hd" + (hdNumberToGet + 1).ToString();

                    try
                    {
                            // modified by cocoon
                            foreach (var ext in ImageExtensions)
                            {
                                partFiles = Directory.GetFiles(imageFiles + Path.DirectorySeparatorChar, "*" + ext);
                                if (partFiles != null && partFiles.Length > 0)
                                {
                                    compExt = ext;
                                    break;
                                }
                            }
                            if (partFiles.Length == 0)
                            {
                                Logger.Log("Image Files Could Not Be Located");
                            }
                    }
                    catch
                    {
                        Logger.Log("Image Files Could Not Be Located");
                    }
                    if (part.vg != null)
                    {
                        if (part.vg.lv != null)
                        {
                            foreach (var lv in part.vg.lv)
                            {
                                if (lv.active == "1")
                                {

                                    // modified by cocoon
                                    foreach (var pType in ImagePartTypes)
                                    {
                                        if (File.Exists(imageFiles + Path.DirectorySeparatorChar + lv.vg + "-" + lv.name + pType + compExt))
                                        {
                                            parts += lv.vg + "-" + lv.name + " ";
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                HttpContext.Current.Response.Write(parts);
            }
            else if (paramName == "HDguid")
            {
                HttpContext.Current.Response.Write(ips.hd[hdNumberToGet].guid);
            }
            else if (paramName == "originalHDSize")
            {
                string originalSize = (Convert.ToInt64(ips.hd[hdNumberToGet].size) * Convert.ToInt32(ips.hd[hdNumberToGet].lbs)).ToString();
                HttpContext.Current.Response.Write(originalSize);
            }
            else if (paramName == "isKnownLayout")
            {
                if(ips.hd[hdNumberToGet].table == "mbr")
                {
                    if(ips.hd[hdNumberToGet].partition.Count() == 1)
                        if(ips.hd[hdNumberToGet].partition[0].start == "63")
                            HttpContext.Current.Response.Write("winxp");
                        else if (ips.hd[hdNumberToGet].partition[0].start == "2048")
                            HttpContext.Current.Response.Write("winvista");
                        else
                            HttpContext.Current.Response.Write("false");
                    else if(ips.hd[hdNumberToGet].partition.Count() == 2)
                    {
                        string part1Size = ((Convert.ToInt64(ips.hd[hdNumberToGet].partition[0].size) * Convert.ToInt32(ips.hd[hdNumberToGet].lbs) / 1024 /1024)).ToString();
                        if(ips.hd[hdNumberToGet].partition[0].start == "2048" && part1Size == "100")
                            HttpContext.Current.Response.Write("win7");
                        else if(ips.hd[hdNumberToGet].partition[0].start == "2048" && part1Size == "350")
                            HttpContext.Current.Response.Write("win8");
                        else
                            HttpContext.Current.Response.Write("false");
                    }
                    else
                        HttpContext.Current.Response.Write("false");
                }
                else if (ips.hd[hdNumberToGet].table == "gpt")
                {
                    if (ips.hd[hdNumberToGet].partition.Count() == 3)
                        if (ips.hd[hdNumberToGet].partition[0].start == "2048" && ips.hd[hdNumberToGet].partition[1].start == "206848" && ips.hd[hdNumberToGet].partition[2].start == "468992")
                            HttpContext.Current.Response.Write("win7gpt");
                        else
                            HttpContext.Current.Response.Write("false");
                    else if (ips.hd[hdNumberToGet].partition.Count() == 4)
                    {
                        if (ips.hd[hdNumberToGet].partition[0].start == "2048" && ips.hd[hdNumberToGet].partition[1].start == "616448" && ips.hd[hdNumberToGet].partition[2].start == "821248" && ips.hd[hdNumberToGet].partition[3].start == "1083392")
                            HttpContext.Current.Response.Write("win8gpt");
                        else if (ips.hd[hdNumberToGet].partition[0].start == "2048" && ips.hd[hdNumberToGet].partition[1].start == "616448" && ips.hd[hdNumberToGet].partition[2].start == "819200" && ips.hd[hdNumberToGet].partition[3].start == "1081344")
                            HttpContext.Current.Response.Write("win8gpt");
                        else
                            HttpContext.Current.Response.Write("false");
                    }
                    else
                        HttpContext.Current.Response.Write("false");
                }
                else
                    HttpContext.Current.Response.Write("Error: Could Not Determine Partition Table Type");
            }
        }

        [WebMethod]
        public void getOriginalLVM(string imgName, string hdToGet, string clienthd)
        {
            Image image = new Image();
            image.ID = image.GetImageID(imgName);
            image.Read(image);
            Image_Physical_Specs ips = new Image_Physical_Specs();
            if (!string.IsNullOrEmpty(image.ClientSizeCustom))
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
            else
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);
            int hdNumberToGet = Convert.ToInt32(hdToGet) - 1;

            foreach (var part in ips.hd[hdNumberToGet].partition)
            {
                if (part.active != "1")
                    continue;
                if (part.vg != null)
                {
                    if (part.vg.lv != null)
                    {
                        HttpContext.Current.Response.Write("pvcreate -u " + part.uuid + " --norestorefile -yf " + clienthd+part.vg.pv[part.vg.pv.Length - 1] + "\r\n");
                        HttpContext.Current.Response.Write("vgcreate " + part.vg.name + " " + clienthd + part.vg.pv[part.vg.pv.Length - 1] + " -yf" + "\r\n");
                        HttpContext.Current.Response.Write("echo \"" + part.vg.uuid + "\" >>/tmp/vg-" +part.vg.name + "\r\n");
                        foreach (var lv in part.vg.lv)
                        {
                            if (lv.active == "1")
                            {
                                HttpContext.Current.Response.Write("lvcreate -L " + lv.size + "s -n " + lv.name + " " + lv.vg + "\r\n");
                                HttpContext.Current.Response.Write("echo \"" + lv.uuid + "\" >>/tmp/" + lv.vg + "-" + lv.name + "\r\n");
                            }
                        }
                        HttpContext.Current.Response.Write("vgcfgbackup -f /tmp/lvm-" + part.vg.name + "\r\n");
                       
                    }
                }
            }
        }
        
        [WebMethod]
        public void getMinHDSize(string imgName, string hdToGet, string newHDSize)
        {
            Image image = new Image();
            image.ID = image.GetImageID(imgName);
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

            
            if(ips.hd == null)
            {
                HttpContext.Current.Response.Write("compatibility,0");
                return;
            }

            if (ips.hd.Count() < Convert.ToInt32(hdToGet))
            {
                HttpContext.Current.Response.Write("notexist,0");
                return;
            }

            int activeCounter = Convert.ToInt32(hdToGet);
            bool foundActive = false;
            int hdNumberToGet = Convert.ToInt32(hdToGet) - 1;

            //Look for first active hd
            if (ips.hd[hdNumberToGet].active == "1")
            {
                foundActive = true;
            }
            else
            {
                while(activeCounter <= ips.hd.Count())
                {
                    if(ips.hd[activeCounter -1 ].active == "1")
                    {
                        hdNumberToGet = activeCounter - 1;
                        foundActive = true;
                        break;
                    }
                    activeCounter++;
                }
            }

            if(!foundActive)
            {
                HttpContext.Current.Response.Write("notactive,0");
                return;
            }

            long minHDSizeRequired_BYTE = image.CalculateMinSizeHD(imgName, hdNumberToGet, newHDSize);
            long newHD_BYTES = Convert.ToInt64(newHDSize);

            if (minHDSizeRequired_BYTE > newHD_BYTES)
            {
                HttpContext.Current.Response.Write("false," + (hdNumberToGet + 1));
                Logger.Log("Error:  " + newHD_BYTES / 1024 / 1024 + " MB Is Less Than The Minimum Required HD Size For This Image(" + minHDSizeRequired_BYTE / 1024 / 1024 + " MB)");
            }
            else if(minHDSizeRequired_BYTE == newHD_BYTES)
            {
                HttpContext.Current.Response.Write("original,"+ (hdNumberToGet + 1));
            }
            else
                HttpContext.Current.Response.Write("true," + (hdNumberToGet + 1));
        }

        [WebMethod]
        public void modifyKnownLayout(string clientHD, string layout)
        {
            string partCommands = null;
            if (layout == "winxp")
            {
                partCommands = "fdisk -c=dos " + clientHD + " &>>/tmp/clientlog.log <<FDISK\r\n";
                partCommands += "d\r\n";
                partCommands += "n\r\n";
                partCommands += "p\r\n";
                partCommands += "1\r\n";
                partCommands += "63\r\n";
                partCommands += "\r\n";
                partCommands += "t\r\n";
                partCommands += "7\r\n";
                partCommands += "a\r\n";
                partCommands += "1\r\n";
                partCommands += "w\r\n";
                partCommands += "FDISK";
            }

            else if (layout == "winvista")
            {
                partCommands = "fdisk " + clientHD + " &>>/tmp/clientlog.log <<FDISK\r\n";
                partCommands += "d\r\n";
                partCommands += "n\r\n";
                partCommands += "p\r\n";
                partCommands += "1\r\n";
                partCommands += "2048\r\n";
                partCommands += "\r\n";
                partCommands += "t\r\n";
                partCommands += "7\r\n";
                partCommands += "a\r\n";
                partCommands += "1\r\n";
                partCommands += "w\r\n";
                partCommands += "FDISK";
            }

            else if (layout == "win7")
            {
                partCommands = "fdisk " + clientHD + " &>>/tmp/clientlog.log <<FDISK\r\n";
                partCommands += "d\r\n";
                partCommands += "2\r\n";
                partCommands += "n\r\n";
                partCommands += "p\r\n";
                partCommands += "2\r\n";
                partCommands += "206848\r\n";
                partCommands += "\r\n";
                partCommands += "t\r\n";
                partCommands += "2\r\n";
                partCommands += "7\r\n";
                partCommands += "w\r\n";
                partCommands += "FDISK";
            }

            else if (layout == "win8")
            {
                partCommands = "fdisk " + clientHD + " &>>/tmp/clientlog.log <<FDISK\r\n";
                partCommands += "d\r\n";
                partCommands += "2\r\n";
                partCommands += "n\r\n";
                partCommands += "p\r\n";
                partCommands += "2\r\n";
                partCommands += "718848\r\n";
                partCommands += "\r\n";
                partCommands += "t\r\n";
                partCommands += "2\r\n";
                partCommands += "7\r\n";
                partCommands += "w\r\n";
                partCommands += "FDISK";
            }

            else if (layout == "win7gpt")
            {
                partCommands = "gdisk " + clientHD + " &>>/tmp/clientlog.log <<GDISK\r\n";
                partCommands += "d\r\n";
                partCommands += "3\r\n";
                partCommands += "n\r\n";
                partCommands += "3\r\n";
                partCommands += "\r\n";
                partCommands += "\r\n";
                partCommands += "0700\r\n";
                partCommands += "w\r\n";
                partCommands += "Y\r\n";
                partCommands += "GDISK";
            }

            else if (layout == "win8gpt")
            {
                partCommands = "gdisk " + clientHD + " &>>/tmp/clientlog.log <<GDISK\r\n";
                partCommands += "d\r\n";
                partCommands += "4\r\n";
                partCommands += "n\r\n";
                partCommands += "4\r\n";
                partCommands += "\r\n";
                partCommands += "\r\n";
                partCommands += "0700\r\n";
                partCommands += "w\r\n";
                partCommands += "Y\r\n";
                partCommands += "GDISK";
            }
            else
                partCommands = "false";

            HttpContext.Current.Response.Write(partCommands);
        }

        [WebMethod]
        public void getPartLayout(string imgName, string hdToGet, string newHDSize, string clientHD, string taskType)
        {
            Image image = new Image();
            image.ID = image.GetImageID(imgName);
            image.Read(image);
            Image_Physical_Specs ips = new Image_Physical_Specs();
            if (!string.IsNullOrEmpty(image.ClientSizeCustom))
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
            else
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);

            int activeCounter = Convert.ToInt32(hdToGet);
            int hdNumberToGet = Convert.ToInt32(hdToGet) - 1;
            //Look for first active hd
            if (ips.hd[hdNumberToGet].active != "1")
            {
                while (activeCounter <= ips.hd.Count())
                {
                    if (ips.hd[activeCounter - 1].active == "1")
                    {
                        hdNumberToGet = activeCounter - 1;
                    }
                    activeCounter++;
                }
            }

            newHDSize = (Convert.ToInt64(newHDSize) * .99).ToString("#");

            int lbs_BYTE = Convert.ToInt32(ips.hd[hdNumberToGet].lbs);
            long newHD_BYTES = Convert.ToInt64(newHDSize);
            long newHD_BLK = Convert.ToInt64(newHDSize) / lbs_BYTE;
           

            
            List<Partition_Resized_Client> listPhysicalAndExtended = new List<Partition_Resized_Client>();
            List<Partition_Resized_Client> listLogical = new List<Partition_Resized_Client>();
            List<Partition_Resized_Client_LVM> listLVM = new List<Partition_Resized_Client_LVM>();


            string bootPart = null;
            if(ips.hd[hdNumberToGet].boot.Length > 0)
                bootPart = ips.hd[hdNumberToGet].boot.Substring(ips.hd[hdNumberToGet].boot.Length - 1, 1);

            ExtendedPartition EP = image.CalculateMinSizeExtended(imgName,hdNumberToGet);
            List<VolumeGroup> listVGS = new List<VolumeGroup>();

            long agreedExtendedSize_BLK = 0;
            double percentCounter = 0;
            bool partLayoutVerified = false;
            bool hasResizableVG = false;
            int startPart = 0;
            while (!partLayoutVerified)
            {
                double totalPercentage = 0;
                listPhysicalAndExtended.Clear();
                listLogical.Clear();
                listVGS.Clear();
                startPart = Convert.ToInt32(ips.hd[hdNumberToGet].partition[0].start);
                int partCounter = 0;
                hasResizableVG = false;
                foreach (var part in ips.hd[hdNumberToGet].partition)
                {
                    if (Convert.ToInt32(part.start) < startPart)
                        startPart = Convert.ToInt32(part.start);
                    if (part.active != "1")
                        continue;
                    if (part.type.ToLower() == "logical")
                        continue;

                    VolumeGroup VG = image.SinglePartMinSizeVG(imgName, hdNumberToGet, partCounter);
                    if (VG.pv != null)
                    {
                        part.resize = (VG.minSize_BLK * lbs_BYTE / 1024 / 1024).ToString();
                        hasResizableVG = true;
                    }

                    Partition_Resized_Client resizedPart = new Partition_Resized_Client();
                    if (bootPart == part.number)
                        resizedPart.isBoot = true;
                    else
                        resizedPart.isBoot = false;

                    resizedPart.number = part.number;
                    resizedPart.start = part.start;
                    resizedPart.type = part.type;
                    resizedPart.fsid = part.fsid;
                    resizedPart.uuid = part.uuid;
                    resizedPart.guid = part.guid;
                    resizedPart.fstype = part.fstype;


                    //If not resizable set aside a minimum size based off original partition length
                    long newPartSize_BLK = 0;
                    double tmpTotalPercentage = 0;

                    if (!string.IsNullOrEmpty(part.size_override))
                    {
                        newPartSize_BLK = Convert.ToInt64(part.size_override);
                        resizedPart.size = newPartSize_BLK.ToString();
                        tmpTotalPercentage = (double)newPartSize_BLK / newHD_BLK;
                        resizedPart.partResized = false;
                        if (resizedPart.type.ToLower() == "extended")
                        {
                            resizedPart.size = EP.minSize_BLK.ToString();
                            agreedExtendedSize_BLK = EP.minSize_BLK;
                        }
                       
                    }

                    else if ((string.IsNullOrEmpty(part.resize) && part.type.ToLower() != "extended") || (part.type.ToLower() == "extended" && EP.isOnlySwap) || Convert.ToInt64(part.size) * lbs_BYTE <= 2097152000)
                    {
                        newPartSize_BLK = Convert.ToInt64(part.size);
                        resizedPart.size = newPartSize_BLK.ToString();
                        tmpTotalPercentage = (double)newPartSize_BLK / newHD_BLK;
                        resizedPart.partResized = false;
                        if (resizedPart.type.ToLower() == "extended")
                        {
                            resizedPart.size = EP.minSize_BLK.ToString();
                            agreedExtendedSize_BLK = EP.minSize_BLK;
                        }
                      
                    }
                    //If resizable determine what percent of drive partition was originally and match that to the new drive
                    //while making sure the min size is still greater than the resized size.
                    else
                    {
                        resizedPart.partResized = true;
                        if (part.type.ToLower() == "extended")
                            newPartSize_BLK = EP.minSize_BLK;
                        else
                        {
                            if (VG.pv != null)
                            {
                                newPartSize_BLK = (Convert.ToInt64(part.resize) * 1024 * 1024) / lbs_BYTE;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(part.used_mb))
                                    part.used_mb = "0";
                                if (Convert.ToInt64(part.resize) > Convert.ToInt64(part.used_mb))
                                    newPartSize_BLK = (Convert.ToInt64(part.resize) * 1024 * 1024) / lbs_BYTE;
                                else
                                    newPartSize_BLK = (Convert.ToInt64(part.used_mb) * 1024 * 1024) / lbs_BYTE;
                            }
                        }


                        double percentOfOrigDrive = (double)(Convert.ToInt64(part.size) / (double)(Convert.ToInt64(ips.hd[hdNumberToGet].size)));
                        if (percentOfOrigDrive - (percentCounter / 100) <= 0)
                            resizedPart.size = (newHD_BLK * (percentOfOrigDrive)).ToString("#");
                        else
                            resizedPart.size = (newHD_BLK * (percentOfOrigDrive - (percentCounter / 100))).ToString("#");

                        if (resizedPart.type.ToLower() == "extended")
                            tmpTotalPercentage = ((double)(Convert.ToInt64(resizedPart.size)) + (((1048576 / lbs_BYTE) * EP.logicalCount) + (1048576 / lbs_BYTE))) / newHD_BLK;
                        else
                            tmpTotalPercentage = (double)(Convert.ToInt64(resizedPart.size)) / newHD_BLK;

                        if (Convert.ToInt64(resizedPart.size) < newPartSize_BLK)
                        {
                            if (resizedPart.type.ToLower() == "extended")
                            {
                                resizedPart.size = (newPartSize_BLK + (((1048576 / lbs_BYTE) * EP.logicalCount) + (1048576 / lbs_BYTE))).ToString();
                                tmpTotalPercentage = (double)(Convert.ToInt64(resizedPart.size)) / newHD_BLK;
                            }
                            else
                            {
                                resizedPart.size = newPartSize_BLK.ToString();
                                tmpTotalPercentage = (double)(Convert.ToInt64(resizedPart.size)) / newHD_BLK;
                            }
                        }

                        if (resizedPart.type.ToLower() == "extended")
                            agreedExtendedSize_BLK = Convert.ToInt64(resizedPart.size);
                       
                    }

                    if (hasResizableVG)
                    {
                        VG.agreedPVSize_BLK = Convert.ToInt64(resizedPart.size);
                        listVGS.Add(VG);
                    }

                    totalPercentage += tmpTotalPercentage;
                    listPhysicalAndExtended.Add(resizedPart);

                    partCounter++;
                }


                percentCounter++;
                if (totalPercentage <= 1)
                {
                    long totalAllocated_BLK = 0;
                    long totalUnallocated = 0;
                    int resizablePartsCount = 0;
                    //If totalPercentage is too far below 1 try to increase size of available resizable partitions
                    if (totalPercentage < .95)
                    {
                        foreach (var partition in listPhysicalAndExtended)
                        {
                            totalAllocated_BLK += Convert.ToInt64(partition.size);
                            if (partition.partResized)
                                resizablePartsCount++;
                        }
                        totalUnallocated = newHD_BLK - totalAllocated_BLK;
                        if (resizablePartsCount > 0)
                        {
                            foreach (var partition in listPhysicalAndExtended)
                            {
                                if (partition.partResized)
                                {
                                    partition.size = (Convert.ToInt64(partition.size) + (totalUnallocated / resizablePartsCount)).ToString();
                                    if (partition.type.ToLower() == "extended")
                                        agreedExtendedSize_BLK = Convert.ToInt64(partition.size);
                                    for (int i = 0; i < listVGS.Count(); i++)
                                        if (ips.hd[hdNumberToGet].name + partition.number == listVGS[i].pv)
                                            listVGS[i].agreedPVSize_BLK = Convert.ToInt64(partition.size);
                                }
                            }
                        }

                    }
                    partLayoutVerified = true;
                }

                //Theoretically should never hit this, but added to prevent infinite loop
                if (percentCounter == 99)
                {
                    HttpContext.Current.Response.Write("false");
                    Logger.Log("Error.  Could Not Determine A Partition Layout That Fits This HD");
                    return;

                }


            }

            //Try to resize lv to fit inside newly created lvm
            if (partLayoutVerified && hasResizableVG)
            {
                foreach (var volumeGroup in listVGS)
                {
                    volumeGroup.agreedPVSize_BLK = Convert.ToInt64(volumeGroup.agreedPVSize_BLK * .99);
                    foreach (var partition in ips.hd[hdNumberToGet].partition)
                    {
                        if (ips.hd[hdNumberToGet].name + partition.number == volumeGroup.pv)
                        {
                            bool singleLvVerified = false;
                            
                            percentCounter = 0;
                            
                            while (!singleLvVerified)
                            {
                                double totalPercentage = 0;
                                listLVM.Clear();
                                if (partition.active != "1")
                                    continue;

                                foreach (var lv in partition.vg.lv)
                                {
                                    if (lv.active != "1")
                                        continue;
                                    Partition_Resized_Client_LVM resizedLV = new Partition_Resized_Client_LVM();

                                    resizedLV.name = lv.name;
                                    resizedLV.vg = lv.vg;
                                    resizedLV.uuid = lv.uuid;
                                    resizedLV.fstype = lv.fstype;


                                    //If not resizable set aside a minimum size based off original partition length
                                    long newPartSize_BLK = 0;
                                    double tmpTotalPercentage = 0;

                                    if (!string.IsNullOrEmpty(lv.size_override))
                                    {
                                        newPartSize_BLK = Convert.ToInt64(lv.size_override);
                                        resizedLV.size = newPartSize_BLK.ToString();
                                        tmpTotalPercentage = (double)newPartSize_BLK / volumeGroup.agreedPVSize_BLK;
                                        resizedLV.partResized = false;
                                    }

                                    else if (string.IsNullOrEmpty(lv.resize))
                                    {
                                        newPartSize_BLK = (Convert.ToInt64(lv.size));
                                        resizedLV.size = newPartSize_BLK.ToString();
                                        tmpTotalPercentage = (double)newPartSize_BLK / volumeGroup.agreedPVSize_BLK;
                                        resizedLV.partResized = false;
                                    }
                                    //If resizable determine what percent of drive partition was originally and match that to the new drive
                                    //while making sure the min size is still greater than the resized size.
                                    else
                                    {
                                        resizedLV.partResized = true;

                                        if (Convert.ToInt64(lv.resize) > Convert.ToInt64(lv.used_mb))
                                            newPartSize_BLK = (Convert.ToInt64(lv.resize) * 1024 * 1024) / lbs_BYTE;
                                        else
                                            newPartSize_BLK = (Convert.ToInt64(lv.used_mb) * 1024 * 1024) / lbs_BYTE;


                                        double percentOfOrigDrive = (double)(Convert.ToInt64(lv.size) / (double)(Convert.ToInt64(ips.hd[hdNumberToGet].size)));
                                        if (percentOfOrigDrive - (percentCounter / 100) <= 0)
                                            resizedLV.size = (volumeGroup.agreedPVSize_BLK * (percentOfOrigDrive)).ToString("#");
                                        else
                                            resizedLV.size = (volumeGroup.agreedPVSize_BLK * (percentOfOrigDrive - (percentCounter / 100))).ToString("#");
                                        tmpTotalPercentage = (double)(Convert.ToInt64(resizedLV.size)) / volumeGroup.agreedPVSize_BLK;
                                        if (Convert.ToInt64(resizedLV.size) < newPartSize_BLK)
                                        {
                                            resizedLV.size = newPartSize_BLK.ToString();
                                            tmpTotalPercentage = (double)(Convert.ToInt64(resizedLV.size)) / volumeGroup.agreedPVSize_BLK;

                                        }

                                    }


                                    totalPercentage += tmpTotalPercentage;
                                    listLVM.Add(resizedLV);
                                  

                                }
                                percentCounter++;
                                if (totalPercentage <= 1)
                                {
                                    long totalAllocated_BLK = 0;
                                    long totalUnallocated = 0;
                                    int resizablePartsCount = 0;
                                    //If totalPercentage is too far below 1 try to increase size of available resizable partitions
                                    if (totalPercentage < .95)
                                    {
                                        foreach (var lv in listLVM)
                                        {
                                            totalAllocated_BLK += Convert.ToInt64(lv.size);
                                            if (lv.partResized)
                                                resizablePartsCount++;
                                        }
                                        totalUnallocated = volumeGroup.agreedPVSize_BLK - totalAllocated_BLK;
                                        if (resizablePartsCount > 0)
                                        {
                                            foreach (var lv in listLVM)
                                            {
                                                if (lv.partResized)
                                                    lv.size = (Convert.ToInt64(lv.size) + (totalUnallocated / resizablePartsCount)).ToString();
                                            }
                                        }

                                    }
                                    singleLvVerified = true;
                                }

                                //Theoretically should never hit this, but added to prevent infinite loop
                                if (percentCounter == 99)
                                {
                                    HttpContext.Current.Response.Write("Error");
                                    break;
                                }
                            }
                        }

                    }

                }
            }

            // Try to resize logical to fit inside newly created extended
            if (partLayoutVerified && EP.hasLogical)
            {
                percentCounter = 0;
                bool logicalPartLayoutVerified = false;

                while (!logicalPartLayoutVerified)
                {
                    listLogical.Clear();
                    double totalPercentage = 0;

                    foreach (var part in ips.hd[hdNumberToGet].partition)
                    {
                        if (part.type.ToLower() != "logical")
                            continue;
                        Partition_Resized_Client resizedPart = new Partition_Resized_Client();
                        if (bootPart == part.number)
                            resizedPart.isBoot = true;
                        else
                            resizedPart.isBoot = false;

                        resizedPart.number = part.number;
                        resizedPart.start = part.start;
                        resizedPart.type = part.type;
                        resizedPart.fsid = part.fsid;
                        resizedPart.uuid = part.uuid;
                        resizedPart.guid = part.guid;
                        resizedPart.fstype = part.fstype;


                        //If not resizable set aside a minimum size based off original partition length
                        long newPartSize_BLK = 0;
                        double tmpTotalPercentage = 0;

                        //If partition is logical and lvm
                        if (part.fsid.ToLower() == "8e")
                        {
                            //List<VolumeGroup> listVGS_lv = new List<VolumeGroup>();
                            listVGS = image.CalculateMinSizeVG(imgName, hdNumberToGet);
                            foreach (var VG in listVGS)
                            {
                                if (VG.pv == ips.hd[hdNumberToGet].name + part.number)
                                    newPartSize_BLK = VG.minSize_BLK;
                            }
                            if (newPartSize_BLK == 0)
                                newPartSize_BLK = Convert.ToInt64(part.size);
                            part.resize = ((newPartSize_BLK * lbs_BYTE) / 1024 / 1024).ToString();
                        }

                        if (!string.IsNullOrEmpty(part.size_override))
                        {
                            newPartSize_BLK = Convert.ToInt64(part.size_override);
                            resizedPart.size = newPartSize_BLK.ToString();
                            tmpTotalPercentage = (double)newPartSize_BLK / agreedExtendedSize_BLK;
                            resizedPart.partResized = false;
                        }

                        if (string.IsNullOrEmpty(part.resize))
                        {

                            newPartSize_BLK = (Convert.ToInt64(part.size));
                            resizedPart.size = newPartSize_BLK.ToString();
                            tmpTotalPercentage = (double)newPartSize_BLK / agreedExtendedSize_BLK;
                            resizedPart.partResized = false;

                        }
                        //If resizable determine what percent of drive partition was originally and match that to the new drive
                        //while making sure the min size is still greater than the resized size.
                        else
                        {
                            resizedPart.partResized = true;

                            if (part.fsid.ToLower() == "8e")
                            {
                                newPartSize_BLK = (Convert.ToInt64(part.resize) * 1024 * 1024) / lbs_BYTE;
                            }
                            else
                            {
                                if (Convert.ToInt64(part.resize) > Convert.ToInt64(part.used_mb))
                                    newPartSize_BLK = (Convert.ToInt64(part.resize) * 1024 * 1024) / lbs_BYTE;
                                else
                                    newPartSize_BLK = (Convert.ToInt64(part.used_mb) * 1024 * 1024) / lbs_BYTE;
                            }

                            if (taskType == "upload")
                                resizedPart.size = newPartSize_BLK.ToString();
                            else
                            {
                                double percentOfOrigDrive = (double)(Convert.ToInt64(part.size) / (double)(Convert.ToInt64(ips.hd[hdNumberToGet].size)));
                                if (percentOfOrigDrive - (percentCounter / 100) <= 0)
                                    resizedPart.size = (agreedExtendedSize_BLK * (percentOfOrigDrive)).ToString("#");
                                else
                                    resizedPart.size = (agreedExtendedSize_BLK * (percentOfOrigDrive - (percentCounter / 100))).ToString("#");
                                tmpTotalPercentage = (double)(Convert.ToInt64(resizedPart.size)) / agreedExtendedSize_BLK;
                                if (Convert.ToInt64(resizedPart.size) < newPartSize_BLK)
                                {
                                    resizedPart.size = newPartSize_BLK.ToString();
                                    tmpTotalPercentage = (double)(Convert.ToInt64(resizedPart.size)) / agreedExtendedSize_BLK;

                                }
                            }
                        }
 
                        totalPercentage += tmpTotalPercentage;
                        listLogical.Add(resizedPart);

                    }

                    percentCounter++;
                    if (totalPercentage <= 1)
                    {
                        long totalAllocated_BLK = 0;
                        long totalUnallocated = 0;
                        int resizablePartsCount = 0;
                        //If totalPercentage is too far below 1 try to increase size of available resizable partitions
                        if (totalPercentage < .95)
                        {
                            foreach (var partition in listLogical)
                            {
                                totalAllocated_BLK += Convert.ToInt64(partition.size);
                                if (partition.partResized)
                                    resizablePartsCount++;
                            }
                            totalUnallocated = agreedExtendedSize_BLK - totalAllocated_BLK;
                            if (resizablePartsCount > 0)
                            {
                                foreach (var partition in listLogical)
                                {
                                    if (partition.partResized)
                                        partition.size = (Convert.ToInt64(partition.size) + (totalUnallocated / resizablePartsCount)).ToString();
                                }
                            }

                        }
                        logicalPartLayoutVerified = true;

                        //testing if logical is also lvm
                        foreach (var part in listLogical)
                        {
                            if (part.fsid.ToLower() == "8e")
                            {
                                foreach (var VG in listVGS)
                                {
                                    if (VG.pv == ips.hd[hdNumberToGet].name + part.number)
                                    {
                                        VG.agreedPVSize_BLK = Convert.ToInt64(part.size);
                                    }
                                }

                                foreach (var volumeGroup in listVGS)
                                {
                                    volumeGroup.agreedPVSize_BLK = Convert.ToInt64(volumeGroup.agreedPVSize_BLK * .99);
                                    foreach (var partition in ips.hd[hdNumberToGet].partition)
                                    {
                                        if (ips.hd[hdNumberToGet].name + partition.number == volumeGroup.pv)
                                        {
                                            bool singleLvVerified = false;

                                            percentCounter = 0;

                                            while (!singleLvVerified)
                                            {
                                                totalPercentage = 0;
                                                listLVM.Clear();
                                                if (partition.active != "1")
                                                    continue;

                                                foreach (var lv in partition.vg.lv)
                                                {
                                                    if (lv.active != "1")
                                                        continue;
                                                    Partition_Resized_Client_LVM resizedLV = new Partition_Resized_Client_LVM();

                                                    resizedLV.name = lv.name;
                                                    resizedLV.vg = lv.vg;
                                                    resizedLV.uuid = lv.uuid;
                                                    resizedLV.fstype = lv.fstype;


                                                    //If not resizable set aside a minimum size based off original partition length
                                                    long newPartSize_BLK = 0;
                                                    double tmpTotalPercentage = 0;

                                                    if (!string.IsNullOrEmpty(lv.size_override))
                                                    {
                                                        newPartSize_BLK = Convert.ToInt64(lv.size_override);
                                                        resizedLV.size = newPartSize_BLK.ToString();
                                                        tmpTotalPercentage = (double)newPartSize_BLK / volumeGroup.agreedPVSize_BLK;
                                                        resizedLV.partResized = false;
                                                    }

                                                    else if (string.IsNullOrEmpty(lv.resize))
                                                    {
                                                        newPartSize_BLK = (Convert.ToInt64(lv.size));
                                                        resizedLV.size = newPartSize_BLK.ToString();
                                                        tmpTotalPercentage = (double)newPartSize_BLK / volumeGroup.agreedPVSize_BLK;
                                                        resizedLV.partResized = false;
                                                    }
                                                    //If resizable determine what percent of drive partition was originally and match that to the new drive
                                                    //while making sure the min size is still greater than the resized size.
                                                    else
                                                    {
                                                        resizedLV.partResized = true;

                                                        if (Convert.ToInt64(lv.resize) > Convert.ToInt64(lv.used_mb))
                                                            newPartSize_BLK = (Convert.ToInt64(lv.resize) * 1024 * 1024) / lbs_BYTE;
                                                        else
                                                            newPartSize_BLK = (Convert.ToInt64(lv.used_mb) * 1024 * 1024) / lbs_BYTE;


                                                        double percentOfOrigDrive = (double)(Convert.ToInt64(lv.size) / (double)(Convert.ToInt64(ips.hd[hdNumberToGet].size)));
                                                        if (percentOfOrigDrive - (percentCounter / 100) <= 0)
                                                            resizedLV.size = (volumeGroup.agreedPVSize_BLK * (percentOfOrigDrive)).ToString("#");
                                                        else
                                                            resizedLV.size = (volumeGroup.agreedPVSize_BLK * (percentOfOrigDrive - (percentCounter / 100))).ToString("#");
                                                        tmpTotalPercentage = (double)(Convert.ToInt64(resizedLV.size)) / volumeGroup.agreedPVSize_BLK;
                                                        if (Convert.ToInt64(resizedLV.size) < newPartSize_BLK)
                                                        {
                                                            resizedLV.size = newPartSize_BLK.ToString();
                                                            tmpTotalPercentage = (double)(Convert.ToInt64(resizedLV.size)) / volumeGroup.agreedPVSize_BLK;

                                                        }

                                                    }


                                                    totalPercentage += tmpTotalPercentage;
                                                    listLVM.Add(resizedLV);


                                                }
                                                percentCounter++;
                                                if (totalPercentage <= 1)
                                                {
                                                    totalAllocated_BLK = 0;
                                                    totalUnallocated = 0;
                                                    resizablePartsCount = 0;
                                                    //If totalPercentage is too far below 1 try to increase size of available resizable partitions
                                                    if (totalPercentage < .95)
                                                    {
                                                        foreach (var lv in listLVM)
                                                        {
                                                            totalAllocated_BLK += Convert.ToInt64(lv.size);
                                                            if (lv.partResized)
                                                                resizablePartsCount++;
                                                        }
                                                        totalUnallocated = volumeGroup.agreedPVSize_BLK - totalAllocated_BLK;
                                                        if (resizablePartsCount > 0)
                                                        {
                                                            foreach (var lv in listLVM)
                                                            {
                                                                if (lv.partResized)
                                                                    lv.size = (Convert.ToInt64(lv.size) + (totalUnallocated / resizablePartsCount)).ToString();
                                                            }
                                                        }

                                                    }
                                                    singleLvVerified = true;
                                                }

                                                //Theoretically should never hit this, but added to prevent infinite loop
                                                if (percentCounter == 99)
                                                {
                                                    HttpContext.Current.Response.Write("Error");
                                                    break;
                                                }
                                            }
                                        }

                                    }

                                }
                            }                    
                        }
                    }

                    //Theoretically should never hit this, but added to prevent infinite loop
                    if (percentCounter == 99)
                    {
                        HttpContext.Current.Response.Write("Error");
                        break;
                    }
                }
            }



            //Order partitions based of block start
            listPhysicalAndExtended = listPhysicalAndExtended.OrderBy(Part => Part.start, new Image.CustomComparer()).ToList();
            listLogical = listLogical.OrderBy(Part => Part.start, new Image.CustomComparer()).ToList();

            if(taskType == "debug")
            {
                try
                {
                    agreedExtendedSize_BLK = agreedExtendedSize_BLK * 512 / 1024 / 1024;
                }
                catch { }
                foreach(var p in listPhysicalAndExtended)
                    p.size = (Convert.ToInt64(p.size) * 512 / 1024 /1024).ToString();
                foreach (var p in listLogical)
                    p.size = (Convert.ToInt64(p.size) * 512 / 1024 / 1024).ToString();
                foreach (var p in listLVM)
                    p.size = (Convert.ToInt64(p.size) * 512 / 1024 / 1024).ToString();

            }

            //Create Menu
            if (ips.hd[hdNumberToGet].table.ToLower() == "mbr")
            {
                int counter = 0;
                int partCount = listPhysicalAndExtended.Count;

                string partitionCommands = null;
                if(Convert.ToInt32(listPhysicalAndExtended[0].start) < 2048)              
                    partitionCommands = "fdisk -c=dos " + clientHD + " &>>/tmp/clientlog.log <<FDISK\r\n";
                else
                    partitionCommands = "fdisk " + clientHD + " &>>/tmp/clientlog.log <<FDISK\r\n";

                foreach (var part in listPhysicalAndExtended)
                {
                    counter++;
                    partitionCommands += "n\r\n";
                    if (part.type == "primary")
                        partitionCommands += "p\r\n";
                    else if (part.type == "extended")
                    {
                        partitionCommands += "e\r\n";
                    }
                    partitionCommands += part.number + "\r\n";
                    if (counter == 1 || taskType == "upload")
                        partitionCommands += startPart.ToString() + "\r\n";
                    else
                        partitionCommands += "\r\n";
                    if(part.type == "extended")
                         partitionCommands += "+" + (Convert.ToInt64(agreedExtendedSize_BLK) - 1).ToString() + "\r\n";
                    else //FDISK seems to include the starting sector in size so we need to subtract 1
                        partitionCommands += "+" + (Convert.ToInt64(part.size) -1).ToString() + "\r\n";

                    partitionCommands += "t\r\n";
                    if (counter == 1)
                        partitionCommands += part.fsid + "\r\n";
                    else
                    {
                        partitionCommands += part.number + "\r\n";
                        partitionCommands += part.fsid + "\r\n";
                    }
                    if (counter == 1 && part.isBoot)
                        partitionCommands += "a\r\n";
                    if (counter != 1 && part.isBoot)
                    {
                        partitionCommands += "a\r\n";
                        partitionCommands += part.number + "\r\n";
                    }
                    if ((counter == partCount && listLogical.Count == 0))
                    {
                        partitionCommands += "w\r\n";
                        partitionCommands += "FDISK\r\n";
                    }


                }


                int logicalCounter = 0;
                foreach (var logicalPart in listLogical)
                {
                    logicalCounter++;
                    partitionCommands += "n\r\n";

                    if (listPhysicalAndExtended.Count < 4)
                        partitionCommands += "l\r\n";


                    partitionCommands += "\r\n";

                    if (taskType == "debug")
                        partitionCommands += "+" + (Convert.ToInt64(logicalPart.size) - (logicalCounter * 1)).ToString() + "\r\n";
                    else
                        partitionCommands += "+" + (Convert.ToInt64(logicalPart.size) - (logicalCounter * 2049)).ToString() + "\r\n";


                    partitionCommands += "t\r\n";

                    partitionCommands += logicalPart.number + "\r\n";
                    partitionCommands += logicalPart.fsid + "\r\n";

                    if (logicalPart.isBoot)
                    {
                        partitionCommands += "a\r\n";
                        partitionCommands += logicalPart.number + "\r\n";
                    }
                    if (logicalCounter == listLogical.Count)
                    {
                        partitionCommands += "w\r\n";
                        partitionCommands += "FDISK\r\n";
                    }
                }
                HttpContext.Current.Response.Write(partitionCommands);
            }
            else
            {
                int counter = 0;
                int partCount = listPhysicalAndExtended.Count;

                string partitionCommands = "gdisk " + clientHD + " &>>/tmp/clientlog.log <<GDISK\r\n";
                foreach (var part in listPhysicalAndExtended)
                {
                    counter++;
                    
                    partitionCommands += "n\r\n";
                   
                    partitionCommands += part.number + "\r\n";
                    if (counter == 1 || taskType == "upload")
                        partitionCommands += startPart.ToString() + "\r\n";
                    else
                        partitionCommands += "\r\n";
                    //GDISK seems to NOT include the starting sector in size so don't subtract 1 like in FDISK
                    partitionCommands += "+" + Convert.ToInt64(part.size).ToString() + "\r\n";


                    partitionCommands += part.fsid + "\r\n";
                
                  
                    if ((counter == partCount))
                    {
                        partitionCommands += "w\r\n";
                        partitionCommands += "y\r\n";
                        partitionCommands += "GDISK\r\n";
                    }


                }
                HttpContext.Current.Response.Write(partitionCommands);

                
            }

           
            foreach (var part in ips.hd[hdNumberToGet].partition)
            {
                if (part.active != "1")
                    continue;
                if (part.vg != null)
                {
                    if (part.vg.lv != null)
                    {
                         HttpContext.Current.Response.Write("echo \"pvcreate -u " + part.uuid + " --norestorefile -yf " + clientHD + part.vg.pv[part.vg.pv.Length - 1] + "\" >>/tmp/lvmcommands \r\n");
                         HttpContext.Current.Response.Write("echo \"vgcreate " + part.vg.name + " " + clientHD + part.vg.pv[part.vg.pv.Length - 1] + " -yf" + "\" >>/tmp/lvmcommands \r\n");
                        HttpContext.Current.Response.Write("echo \"" + part.vg.uuid + "\" >>/tmp/vg-" + part.vg.name + " \r\n");
                        foreach (var lv in part.vg.lv)
                        {
                            foreach(var rlv in listLVM)
                            {                               
                                if (lv.name == rlv.name && lv.vg == rlv.vg)
                                {
                                    HttpContext.Current.Response.Write("echo \"lvcreate -L " + ((Convert.ToInt64(rlv.size) - 8192).ToString()) + "s -n " + rlv.name + " " + rlv.vg + "\" >>/tmp/lvmcommands \r\n");
                                    HttpContext.Current.Response.Write("echo \"" + rlv.uuid + "\" >>/tmp/" + rlv.vg + "-" + rlv.name + "\r\n");
                                }
                            }
                        }
                        HttpContext.Current.Response.Write("echo \"vgcfgbackup -f /tmp/lvm-" + part.vg.name + "\" >>/tmp/lvmcommands\r\n");

                    }
                }
            }
        }

        [WebMethod]
        public void ipxelogin()
        {
            History history = new History();
            Utility settings = new Utility();
            HttpContext postedContext = HttpContext.Current;
            HttpFileCollection Files = postedContext.Request.Files;

            string username = (string)postedContext.Request.Form["uname"];
            string password = (string)postedContext.Request.Form["pwd"];
            string kernel = (string)postedContext.Request.Form["kernel"];
            string bootImage = (string)postedContext.Request.Form["bootImage"];
            string task = (string)postedContext.Request.Form["task"];
            string wds_key = null;
            if (settings.GetSettings("Server Key Mode") == "Automated")
                wds_key = settings.GetSettings("Server Key");
            else
                wds_key = "";

            string globalHostArgs = settings.GetSettings("Global Host Args");
            if (settings.UserLogin(username, password))
            {
                 
                 string lines;

                 lines = "#!ipxe\r\n";
                 lines += "kernel " + "http://" + settings.GetServerIP() + "/cruciblewds/data/boot/kernels/" + kernel + ".krn" + " initrd=" + bootImage + " root=/dev/ram0 rw ramdisk_size=127000 ip=dhcp " + " web=" + settings.GetSettings("Web Path") + " WDS_KEY=" + wds_key + " task=" + task + " consoleblank=0 " + globalHostArgs + "\r\n";
                 lines += "imgfetch " + "http://" + settings.GetServerIP() + "/cruciblewds/data/boot/images/" + bootImage + "\r\n";
                 lines += "boot";
               

                 HttpContext.Current.Response.Write(lines);

                 history.Event = "Successful Login";
                 history.Type = "iPXE";
                 history.EventUser = username;
                 history.Notes = " Task: " + task;
                 history.CreateEvent(history);
            }
            else
            {
                 HttpContext.Current.Response.End();

                 history.Event = "Failed Login";
                 history.Type = "iPXE";
                 history.EventUser = username;
                 history.Notes = "Password: " + password + " Task: " + task;
                 history.CreateEvent(history);
            }
        }

        [WebMethod]
        public void consolelogin()
        {
            History history = new History();
            Utility settings = new Utility();
            HttpContext postedContext = HttpContext.Current;
            HttpFileCollection Files = postedContext.Request.Files;

            string serverKey = settings.Decode((string)postedContext.Request.Form["serverKey"]);
            history.IP = settings.Decode((string)postedContext.Request.Form["clientIP"]);

            if (serverKey == settings.GetSettings("Server Key"))
            {
                 string username = settings.Decode((string)postedContext.Request.Form["username"]);
                 string password = settings.Decode((string)postedContext.Request.Form["password"]);
                 string task = settings.Decode((string)postedContext.Request.Form["task"]);


                 if (settings.UserLogin(username, password))
                 {
                      WDSUser wdsuser = new WDSUser();
                      string userID = wdsuser.GetID(username);
                      wdsuser.ID = userID;
                      wdsuser = wdsuser.Read(wdsuser);

                      if (task == "ond" && wdsuser.OndAccess == "1")
                      {
                           HttpContext.Current.Response.Write("true," + userID);
                           history.Event = "Successful Console Login";
                           history.Type = "User";
                           history.EventUser = username;
                           history.TypeID = userID;
                           history.Notes = "";
                           history.CreateEvent(history);
                      }
                      else if (task == "debug" && wdsuser.DebugAccess == "1")
                      {
                           HttpContext.Current.Response.Write("true," + userID);
                           history.Event = "Successful Console Login";
                           history.Type = "User";
                           history.EventUser = username;
                           history.TypeID = userID;
                           history.Notes = "";
                           history.CreateEvent(history);
                      }
                      else if (task == "diag" && wdsuser.DiagAccess == "1")
                      {
                           HttpContext.Current.Response.Write("true," + userID);
                           history.Event = "Successful Console Login";
                           history.Type = "User";
                           history.EventUser = username;
                           history.TypeID = userID;
                           history.Notes = "";
                           history.CreateEvent(history);
                      }
                      else
                      {
                           HttpContext.Current.Response.Write("false");
                           history.Event = "Failed Console Login";
                           history.Type = "User";
                           history.EventUser = username;
                           history.Notes = password;
                           history.CreateEvent(history);
                      }
                 }
                 else if (!string.IsNullOrEmpty(settings.GetSettings("AD Login Domain")))
                 {


                      try
                      {
                           PrincipalContext context = new PrincipalContext(ContextType.Domain, settings.GetSettings("AD Login Domain"), username, password);
                           UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
                           if (user != null)
                           {
                                WDSUser wdsuser = new WDSUser();
                                string userID = wdsuser.GetID(username);
                                wdsuser.ID = userID;
                                wdsuser = wdsuser.Read(wdsuser);

                                if (task == "ond" && wdsuser.OndAccess == "1")
                                {
                                     HttpContext.Current.Response.Write("true," + userID);
                                     history.Event = "Successful Console Login";
                                     history.Type = "User";
                                     history.EventUser = username;
                                     history.TypeID = userID;
                                     history.Notes = "";
                                     history.CreateEvent(history);
                                }
                                else if (task == "debug" && wdsuser.DebugAccess == "1")
                                {
                                     HttpContext.Current.Response.Write("true," + userID);
                                     history.Event = "Successful Console Login";
                                     history.Type = "User";
                                     history.EventUser = username;
                                     history.TypeID = userID;
                                     history.Notes = "";
                                     history.CreateEvent(history);
                                }
                                else if (task == "diag" && wdsuser.DiagAccess == "1")
                                {
                                     HttpContext.Current.Response.Write("true," + userID);
                                     history.Event = "Successful Console Login";
                                     history.Type = "User";
                                     history.EventUser = username;
                                     history.TypeID = userID;
                                     history.Notes = "";
                                     history.CreateEvent(history);
                                }
                                else
                                {
                                     HttpContext.Current.Response.Write("false");
                                     history.Event = "Failed Console Login";
                                     history.Type = "User";
                                     history.EventUser = username;
                                     history.Notes = password;
                                     history.CreateEvent(history);
                                }
                           }

                      }
                      catch
                      {
                           HttpContext.Current.Response.Write("false");
                           history.Event = "Failed Console Login";
                           history.Type = "User";
                           history.EventUser = username;
                           history.Notes = password;
                           history.CreateEvent(history);
                      }
                 }
                 else
                 {
                      HttpContext.Current.Response.Write("false");
                      history.Event = "Failed Console Login";
                      history.Type = "User";
                      history.EventUser = username;
                      history.Notes = password;
                      history.CreateEvent(history);
                 }
            }

            else
            {
                 Logger.Log("Incorrect Key For Client Login Was Provided");
            }
        }

        [WebMethod]
        public void getHostName(string mac)
        {
            Host host = new Host();
            host.ID = host.GetHostID(mac);
            if (host.ID == "error")
            {
                HttpContext.Current.Response.Write("");
                return;
            }
            host.Read(host);
            HttpContext.Current.Response.Write(host.Name);
        }

        [WebMethod]
        public void clienttest()
        {
            HttpContext.Current.Response.Write("true");
        }

        [WebMethod]
        public void getlocaldatetime()
        {
             HttpContext.Current.Response.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [WebMethod]
        public void getutcdatetime()
        {
             HttpContext.Current.Response.Write(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [WebMethod]
        public void inslot()
        {
            Utility settings = new Utility();
            HttpContext postedContext = HttpContext.Current;
            HttpFileCollection Files = postedContext.Request.Files;
            string serverKey = settings.Decode((string)postedContext.Request.Form["serverKey"]);

             if (serverKey == settings.GetSettings("Server Key"))
             {
                  string mac = settings.Decode((string)postedContext.Request.Form["mac"]);
                  string result = null;
                  try
                  {
                       using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                       {
                            NpgsqlCommand cmd = new NpgsqlCommand("client_inslot", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new NpgsqlParameter("@mac", mac));
                            conn.Open();
                            result = cmd.ExecuteScalar() as string;
                       }
                  }
                  catch (Exception ex)
                  {
                       result = "Could Not Place Host Into Active Slot.  Check The Exception Log For More Info";
                       Logger.Log(ex.ToString());
                  }

                  HttpContext.Current.Response.Write(result);
             }
             else
            {
                 Logger.Log("Incorrect Key For Client inslot Was Provided");
            }
        }

        [WebMethod]
        public void mccheckout()
        {
             Utility settings = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;
             string portBase = settings.Decode((string)postedContext.Request.Form["portBase"]);

            string result = null;
            string pid = null;
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("client_readmcpid", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new NpgsqlParameter("@portBase", portBase));
                    conn.Open();
                    pid = cmd.ExecuteScalar() as string;
                }
            }
            catch (Exception ex)
            {
                result = "Could Not Read Multicast PID.  Check The Exception Log For More Info";
                Logger.Log(ex.ToString());
            }

            if (pid != "0")
            {
                bool prsRunning = true;

                if (Environment.OSVersion.ToString().Contains("Unix"))
                {
                    try
                    {
                        Process prs = Process.GetProcessById(Convert.ToInt32(pid));
                        if (prs.HasExited)
                        {
                            prsRunning = false;
                        }
                    }
                    catch
                    {
                        prsRunning = false;
                    }

                }
                else
                {
                    try
                    {
                        Process prs = Process.GetProcessById(Convert.ToInt32(pid));
                    }
                    catch
                    {
                        prsRunning = false;
                    }
                }
                if (!prsRunning)
                {
                    try
                    {
                        using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                        {
                            NpgsqlCommand cmd = new NpgsqlCommand("client_closemc", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new NpgsqlParameter("@portBase", portBase));
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            result = "Success";
                        }
                    }
                    catch (Exception ex)
                    {
                        result = "An Error Has Occurred.  Check The Exception Log For More Info";
                        Logger.Log(ex.ToString());
                    }
                }
                else
                    result = "Cannot Close Session, It Is Still In Progress";
            }
            else
                result = "Session Is Already Closed";
            HttpContext.Current.Response.Write(result);
        }

        [WebMethod]
        public void queuepos()
        {
             Utility settings = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;
             string mac = settings.Decode((string)postedContext.Request.Form["mac"]);

            string result = null;
            int tmpResult = 0;
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("client_getqueuepos", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new NpgsqlParameter("@mac", mac));
                    conn.Open();
                    tmpResult = Convert.ToInt16(cmd.ExecuteScalar());
                    result = tmpResult.ToString();
                }
            }
            catch (Exception ex)
            {
                result = "Could Not Get Queue Position.  Check The Exception Log For More Info";
                Logger.Log(ex.ToString());
            }
            HttpContext.Current.Response.Write(result);
        }

        [WebMethod]
        public void currentpos()
        {
             Utility settings = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;
             string mac = settings.Decode((string)postedContext.Request.Form["mac"]);

             string result = null;
             int tmpResult = 0;
             try
             {
                  using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                  {
                       NpgsqlCommand cmd = new NpgsqlCommand("client_hostsbeforeme", conn);
                       cmd.CommandType = CommandType.StoredProcedure;
                       cmd.Parameters.Add(new NpgsqlParameter("@mac", mac));
                       conn.Open();
                       tmpResult = Convert.ToInt16(cmd.ExecuteScalar());
                       result = tmpResult.ToString();

                  }
             }
             catch (Exception ex)
             {
                  result = "Could Not Get Current Queue Position.  Check The Exception Log For More Info";
                  Logger.Log(ex.ToString());
             }
             HttpContext.Current.Response.Write(result);
        }

        [WebMethod]
        public void queuestatus()
        {
            string result = null;
            int tmpResult = 0;
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("client_queuecount", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    tmpResult = Convert.ToInt16(cmd.ExecuteScalar());
                }
                result = tmpResult.ToString();
                result += ",";
                string settingValue = null;
                using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                {

                    NpgsqlCommand cmd = new NpgsqlCommand("settings_getSettings", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@settingName", "Queue Size");
                    conn.Open();
                    settingValue = cmd.ExecuteScalar().ToString();
                    result += settingValue;
                }
            }
            catch (Exception ex)
            {
                result = "Could Not Determine Queue Size.  Check The Exception Log For More Info";
                Logger.Log(ex.ToString());
            }
            HttpContext.Current.Response.Write(result);
        }

        [WebMethod(EnableSession = true)]
        public void AddHost(Host host)
        {
            host.Create(host);
            HttpContext.Current.Response.Write(Utility.Message);
        }

        [WebMethod(EnableSession = true)]
        public void UpdateProgress(Task task)
        {
            task.UpdateProgress(task);
        }

        [WebMethod(EnableSession = true)]
        public void AddImage(Image image)
        {
            image.Create(image);
            string imageID = image.GetImageID(image.Name);
            HttpContext.Current.Response.Write(imageID + "," + Utility.Message);
        }

        [WebMethod]
        public void aminext()
        {
            string result = null;
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("client_aminext", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    result = Convert.ToString(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                result = "Could Not Determine If Host Is Next In Queue.  Check The Exception Log For More Info";
                Logger.Log(ex.ToString());
            }
            HttpContext.Current.Response.Write(result);
        }

        [WebMethod]
        public void checkin()
        {
            Utility settings = new Utility();
            HttpContext postedContext = HttpContext.Current;
            HttpFileCollection Files = postedContext.Request.Files;
            string serverKey = settings.Decode((string)postedContext.Request.Form["serverKey"]);

            if (serverKey == settings.GetSettings("Server Key"))
            {
                 string mac = settings.Decode((string)postedContext.Request.Form["mac"]);
                 string result = null;
                 try
                 {
                      using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                      {
                           NpgsqlCommand cmd = new NpgsqlCommand("client_checkin", conn);
                           cmd.CommandType = CommandType.StoredProcedure;
                           cmd.Parameters.Add(new NpgsqlParameter("@mac", mac));
                           conn.Open();
                           result = cmd.ExecuteScalar() as string;
                      }
                 }
                 catch (Exception ex)
                 {
                      result = "Could Not Check In.  Check The Exception Log For More Info";
                      Logger.Log(ex.ToString());
                 }
                 HttpContext.Current.Response.Write(result);
            }
            else
            {
                 Logger.Log("Incorrect Key For Client Checkin Was Provided");
            }
        }

        [WebMethod]
        public void updateprogresspartition(string hostName, string partition)
        {
            Task task = new Task();
            task.UpdateProgressPartition(hostName, partition);
        }

        [WebMethod]
        public void checkout()
        {
            Utility utility = new Utility();
            HttpContext postedContext = HttpContext.Current;
            HttpFileCollection Files = postedContext.Request.Files;
            string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);

            if (serverKey == utility.GetSettings("Server Key"))
            {
                 string imgName = utility.Decode((string)postedContext.Request.Form["imgName"]);
                 string direction = utility.Decode((string)postedContext.Request.Form["direction"]);
                 string mac = utility.Decode((string)postedContext.Request.Form["mac"]);

                 string result = null;
                 try
                 {
                      using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
                      {
                           NpgsqlCommand cmd = new NpgsqlCommand("client_checkout", conn);
                           cmd.CommandType = CommandType.StoredProcedure;
                           cmd.Parameters.Add(new NpgsqlParameter("@mac", mac));
                           conn.Open();
                           result = cmd.ExecuteScalar() as string;
                      }

                      Task task = new Task();
                      string pxeHostMac = "01-" + mac.ToLower().Replace(':', '-');
                      task.CleanPxeBoot(pxeHostMac);

                      if (direction == "pull")
                      {
                           string xferMode = utility.GetSettings("Image Transfer Mode");
                           if (xferMode != "udp+http" && xferMode != "smb" && xferMode != "smb+http")
                           {
                                Utility.MoveFolder(utility.GetSettings("Image Hold Path") + imgName, utility.GetSettings("Image Store Path") + imgName);
                                if ((Directory.Exists(utility.GetSettings("Image Store Path") + imgName)) && (!Directory.Exists(utility.GetSettings("Image Hold Path") + imgName)))
                                {
                                     try
                                     {
                                          Directory.CreateDirectory(utility.GetSettings("Image Hold Path") + imgName); // for next upload
                                          result = "Success";
                                     }
                                     catch (Exception ex)
                                     {
                                          Logger.Log(ex.Message);
                                          result = "Could Not Recreate Directory,  You Must Create It Before You Can Upload Again";
                                     }

                                }
                                else
                                     result = "Could Not Move Image From Hold To Store.  You Must Do It Manually.";
                           }
                      }

                 }
                 catch (Exception ex)
                 {
                      result = "Could Not Check Out.  Check The Exception Log For More Info";
                      Logger.Log(ex.ToString());
                 }

                 HttpContext.Current.Response.Write(result);
            }
            else
            {
                 Logger.Log("Incorrect Key For Client Checkout Was Provided");
            }
        }

        [WebMethod]
        public void downloadcorescripts()
        {
            Utility utility = new Utility();
            HttpContext postedContext = HttpContext.Current;
            HttpFileCollection Files = postedContext.Request.Files;

            string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
            string scriptName = (string)postedContext.Request.Form["scriptName"];

            if (serverKey == utility.GetSettings("Server Key"))
            {
                string scriptPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "clientscripts" + Path.DirectorySeparatorChar + "core" + Path.DirectorySeparatorChar;
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment; filename=" + scriptName);
                HttpContext.Current.Response.TransmitFile(scriptPath + scriptName);
                HttpContext.Current.Response.End();
            }
            Logger.Log("An Incorrect Key Was Provided While Trying To Download Core Scripts");
        }

        [WebMethod]
        public void smbcredentials()
        {
             Utility utility = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;

             string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
             string credential = utility.Decode((string)postedContext.Request.Form["credential"]);
             string xferMode = utility.GetSettings("Image Transfer Mode");
             if (xferMode != "smb" && xferMode != "smb+http")
             {
                  Logger.Log("An Attempt Was Made To Access SMB Credentials But Current Image Transfer Mode Is Not SMB");
                  HttpContext.Current.Response.Write("");
                  HttpContext.Current.Response.End();
             }
            
             if (serverKey == utility.GetSettings("Server Key"))
             {
                if(credential == "username")
                     HttpContext.Current.Response.Write(utility.GetSettings("SMB User Name"));
                else if(credential == "password")
                     HttpContext.Current.Response.Write(utility.GetSettings("SMB Password"));
             }
             else
               Logger.Log("An Incorrect Key Was Provided While Trying To Access SMB Credentials");
        }

        [WebMethod]
        public void downloadcustomscripts()
        {
             Utility utility = new Utility();
             HttpContext postedContext = HttpContext.Current;
             HttpFileCollection Files = postedContext.Request.Files;

             string serverKey = utility.Decode((string)postedContext.Request.Form["serverKey"]);
             string scriptName = (string)postedContext.Request.Form["scriptName"];

             if (serverKey == utility.GetSettings("Server Key"))
             {
                  string scriptPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "clientscripts" + Path.DirectorySeparatorChar;
                  HttpContext.Current.Response.ContentType = "application/octet-stream";
                  HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment; filename=" + scriptName);
                  HttpContext.Current.Response.TransmitFile(scriptPath + scriptName);
                  HttpContext.Current.Response.End();
             }
             Logger.Log("An Incorrect Key Was Provided While Trying To Download Custom Scripts");
        }
        #endregion
    }
}
