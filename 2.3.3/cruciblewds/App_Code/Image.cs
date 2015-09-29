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
using System.Data;
using System.Configuration;
using System.IO;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using Newtonsoft.Json;
using System.Security.Cryptography;

public class Image
{
    public class CustomComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return long.Parse(x).CompareTo(long.Parse(y));
        }
    }

    public string ID { get; set; }
    public string Name { get; set; }
    public string OS { get; set; }
    public string Description { get; set; }
    public int Protected { get; set; }
    public int IsVisible { get; set; }
    public string ClientSize { get; set; }
    public string ClientSizeCustom { get; set; }
    public string Checksum { get; set; }


    public VolumeGroup SinglePartMinSizeVG(string imgName, int hdNumberToGet, int partToGet)
    {
        Image image = new Image();
        image.ID = image.GetImageID(imgName);
        image.Read(image);
        Image_Physical_Specs ips = new Image_Physical_Specs();
        if (!string.IsNullOrEmpty(image.ClientSizeCustom))
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
        else
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);

        int lbs_BYTE = Convert.ToInt32(ips.hd[hdNumberToGet].lbs);

        //Determine if any Volume Groups Are present.  Needed ahead of time correctly calculate sizes.
        //And calculate minimum needed lvm partition size

        VolumeGroup VG = new VolumeGroup();
        VG.minSize_BLK = 0;
        VG.hasLv = false;

        if (ips.hd[hdNumberToGet].partition[partToGet].fsid.ToLower() == "8e" || ips.hd[hdNumberToGet].partition[partToGet].fsid.ToLower() == "8e00")
        {
            
            if (ips.hd[hdNumberToGet].partition[partToGet].active != "1")
                return VG;
            
            //if part.vg is null, most likely version 2.3.0 beta1 before lvm was added.
            if (ips.hd[hdNumberToGet].partition[partToGet].vg != null)
            {
                //if vg.name is null partition was uploaded at physical partion level, not using the resize flag
                if (ips.hd[hdNumberToGet].partition[partToGet].vg.name != null)
                {
                    VG.name = ips.hd[hdNumberToGet].partition[partToGet].vg.name;
                    foreach (var lv in ips.hd[hdNumberToGet].partition[partToGet].vg.lv)
                    {
                        if (lv.active != "1")
                            continue;
                        VG.hasLv = true;
                        VG.pv = ips.hd[hdNumberToGet].partition[partToGet].vg.pv;
                        if (!string.IsNullOrEmpty(lv.size_override))
                            VG.minSize_BLK += Convert.ToInt64(lv.size_override);
                        else
                        {
                            if (string.IsNullOrEmpty(lv.resize))
                                VG.minSize_BLK += Convert.ToInt64(lv.size);
                            else
                            {
                                if (Convert.ToInt64(lv.resize) > Convert.ToInt64(lv.used_mb))
                                    VG.minSize_BLK += (Convert.ToInt64(lv.resize) * 1024 * 1024) / lbs_BYTE;
                                else
                                    VG.minSize_BLK += (Convert.ToInt64(lv.used_mb) * 1024 * 1024) / lbs_BYTE;
                            }
                        }
                    }
                    //Could Have VG Without LVs
                    //Set arbitary minimum size to 100mb
                    if (!VG.hasLv)
                    {
                        VG.pv = ips.hd[hdNumberToGet].partition[partToGet].vg.pv;
                        VG.minSize_BLK = (Convert.ToInt64("100") * 1024 * 1024) / lbs_BYTE;
                    }
                }
            }
        }
        return VG;
    }

    public List<VolumeGroup> CalculateMinSizeVG(string imgName, int hdNumberToGet)
    {
        Image image = new Image();
        image.ID = image.GetImageID(imgName);
        image.Read(image);
        Image_Physical_Specs ips = new Image_Physical_Specs();
        if (!string.IsNullOrEmpty(image.ClientSizeCustom))
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
        else
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);

        int lbs_BYTE = Convert.ToInt32(ips.hd[hdNumberToGet].lbs);
        List<VolumeGroup> listVGS = new List<VolumeGroup>();
       
        //Determine if any Volume Groups Are present.  Needed ahead of time correctly calculate sizes.
        //And calculate minimum needed lvm partition size

        foreach (var part in ips.hd[hdNumberToGet].partition)
        {
            if (part.fsid.ToLower() == "8e" || part.fsid.ToLower() == "8e00")
            {
                if (part.active != "1")
                    continue;
                VolumeGroup VG = new VolumeGroup();
                VG.minSize_BLK = 0;
                VG.hasLv = false;
                //if part.vg is null, most likely version 2.3.0 beta1 before lvm was added.
                if (part.vg != null)
                {
                    //if vg.name is null partition was uploaded at physical partion level, not using the resize flag
                    if (part.vg.name != null)
                    {
                        foreach (var lv in part.vg.lv)
                        {
                            if (lv.active != "1")
                                continue;
                            VG.hasLv = true;
                            VG.pv = part.vg.pv;
                            if (!string.IsNullOrEmpty(lv.size_override))
                                VG.minSize_BLK += Convert.ToInt64(lv.size_override);
                            else
                            {
                                if (string.IsNullOrEmpty(lv.resize))
                                    VG.minSize_BLK += Convert.ToInt64(lv.size);
                                else
                                {
                                    if (Convert.ToInt64(lv.resize) > Convert.ToInt64(lv.used_mb))
                                        VG.minSize_BLK += (Convert.ToInt64(lv.resize) * 1024 * 1024) / lbs_BYTE;
                                    else
                                        VG.minSize_BLK += (Convert.ToInt64(lv.used_mb) * 1024 * 1024) / lbs_BYTE;
                                }
                            }
                        }
                        //Could Have VG Without LVs
                        //Set arbitary minimum size to 100mb
                        if (!VG.hasLv)
                            VG.minSize_BLK = (Convert.ToInt64("100") * 1024 * 1024) / lbs_BYTE;

                        listVGS.Add(VG);
                    }                  
                }
            }
        }     
        return listVGS;
    }

    public ExtendedPartition CalculateMinSizeExtended(string imgName, int hdNumberToGet)
    {
        Image image = new Image();
        image.ID = image.GetImageID(imgName);
        image.Read(image);
        Image_Physical_Specs ips = new Image_Physical_Specs();
        if (!string.IsNullOrEmpty(image.ClientSizeCustom))
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
        else
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);

        int lbs_BYTE = Convert.ToInt32(ips.hd[hdNumberToGet].lbs);
        ExtendedPartition EP = new ExtendedPartition();
        EP.minSize_BLK = 0;
        EP.isOnlySwap = false;

        bool hasExtendedPartition = false;
        bool hasLogicalPartition = false;

        //Determine if any Extended or Logical Partitions are present.  Needed ahead of time correctly calculate sizes.
        //And calculate minimum needed extended partition size

        string logicalFSType = null;
        foreach (var part in ips.hd[hdNumberToGet].partition)
        {
            if (part.active != "1")
                continue;
            if (part.type.ToLower() == "extended")
                hasExtendedPartition = true;
            if (part.type.ToLower() == "logical")
            {
                EP.logicalCount++;
                logicalFSType = part.fstype;
                hasLogicalPartition = true;
                EP.hasLogical = true;
            }
        }

        if (EP.logicalCount == 1 && logicalFSType.ToLower() == "swap")
            EP.isOnlySwap = true;

        if (hasExtendedPartition)
        {
            foreach (var partition in ips.hd[hdNumberToGet].partition)
            {
                if (partition.active != "1")
                    continue;
                if (hasExtendedPartition && hasLogicalPartition)
                {
                    if (partition.type.ToLower() == "logical")
                    {
                        if (partition.fsid.ToLower() == "8e")
                        {
                            List<VolumeGroup> listVGS = image.CalculateMinSizeVG(imgName, hdNumberToGet);
                            foreach (var VG in listVGS)
                            {
                                if (VG.pv == ips.hd[hdNumberToGet].name + partition.number)
                                    EP.minSize_BLK = VG.minSize_BLK;
                            }
                            if (EP.minSize_BLK == 0)
                                EP.minSize_BLK = Convert.ToInt64(partition.size);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(partition.size_override))
                                EP.minSize_BLK += Convert.ToInt64(partition.size_override);
                            else
                            {
                                if (string.IsNullOrEmpty(partition.resize))
                                    EP.minSize_BLK += Convert.ToInt64(partition.size);
                                else
                                {
                                    if (Convert.ToInt64(partition.resize) > Convert.ToInt64(partition.used_mb))
                                        EP.minSize_BLK += (Convert.ToInt64(partition.resize) * 1024 * 1024) / lbs_BYTE;
                                    else
                                        EP.minSize_BLK += (Convert.ToInt64(partition.used_mb) * 1024 * 1024) / lbs_BYTE;
                                }
                            }
                        }

                    }
                }
                //If Hd has extended but no logical, use the extended to calc size
                else if (hasExtendedPartition && !hasLogicalPartition)
                {
                    //In this case someone has defined an extended partition but has not created any logical
                    //This could just be for preperation of leaving room for more logical partition later
                    //This should be highly unlikely but should account for it anyway.  There is no way of knowing a minimum size required
                    //while still having the partition be resizable.  So will set minimum sized required to unless user overrides
                    if (partition.type.ToLower() == "extended")
                        if(!string.IsNullOrEmpty(partition.size_override))
                            EP.minSize_BLK = Convert.ToInt64(partition.size_override);
                        else
                            //set arbitary minimum to 100MB
                            EP.minSize_BLK = (Convert.ToInt64("100") * 1024 * 1024) / lbs_BYTE;
                }
            }
        }
        //Logical paritions default to 1MB more than the previous block using fdisk. This needs to be added to extended size so logical parts will fit inside
        long epPadding = (((1048576 / lbs_BYTE) * EP.logicalCount) + (1048576 / lbs_BYTE));
        EP.minSize_BLK += epPadding;
        return EP;
    }

    public bool Check_Checksum(string imageID)
    {
        Utility utility = new Utility();
        if (utility.GetSettings("Image Checksum") == "On")
        {

            Image image = new Image();
            image.ID = imageID;
            image = image.Read(image);

            try
            {
                List<HD_Checksum> listPhysicalImageChecksums = new List<HD_Checksum>();
                string path = utility.GetSettings("Image Store Path") + image.Name;
                HD_Checksum imageChecksum = new HD_Checksum();
                imageChecksum.hdNumber = "hd1";
                imageChecksum.path = path;
                listPhysicalImageChecksums.Add(imageChecksum);
                for (int x = 2; true; x++)
                {
                    imageChecksum = new HD_Checksum();
                    string subdir = path + Path.DirectorySeparatorChar + "hd" + x;
                    if (Directory.Exists(subdir))
                    {
                        imageChecksum.hdNumber = "hd" + x;
                        imageChecksum.path = subdir;
                        listPhysicalImageChecksums.Add(imageChecksum);
                    }
                    else
                        break;
                }

                foreach (HD_Checksum hd in listPhysicalImageChecksums)
                {
                    List<File_Checksum> listChecksums = new List<File_Checksum>();

                    var files = Directory.GetFiles(hd.path, "*.*");
                    foreach (var file in files)
                    {
                        File_Checksum fc = new File_Checksum();
                        fc.fileName = Path.GetFileName(file);
                        fc.checksum = image.Calculate_Hash(file);
                        listChecksums.Add(fc);

                    }
                    hd.path = string.Empty;
                    hd.fc = listChecksums.ToArray();
                }


                string physicalImageJson = JsonConvert.SerializeObject(listPhysicalImageChecksums);
                if (physicalImageJson != image.Checksum)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return false;
            }
        }
        return true;
    }

    public string Calculate_Hash(string fileName)
    {
        Utility utility = new Utility();

        long read = 0;
        int r = -1;
        const long bytesToRead = 100 * 1024 * 1024;
        const int bufferSize = 4096;
        byte[] buffer = new byte[bufferSize];
        SHA256Managed sha = new SHA256Managed();

        using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            while (read <= bytesToRead && r != 0)
            {
                read += (r = stream.Read(buffer, 0, bufferSize));
                sha.TransformBlock(buffer, 0, r, null, 0);
            }
        }
        sha.TransformFinalBlock(buffer, 0, 0);
        return String.Join("", sha.Hash.Select(x => x.ToString("x2")));
    }

    public long CalculateMinSizeHD(string imgName, int hdNumberToGet, string newHDSize)
    {
        Image image = new Image();
        image.ID = image.GetImageID(imgName);
        image.Read(image);

        Image_Physical_Specs ips = new Image_Physical_Specs();
        if (!string.IsNullOrEmpty(image.ClientSizeCustom))
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
        else
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);
        ExtendedPartition EP = image.CalculateMinSizeExtended(imgName,hdNumberToGet);
        List<VolumeGroup> listVGS = image.CalculateMinSizeVG(imgName, hdNumberToGet);

        long minHDSizeRequired_BLK = 0;
        int lbs_BYTE = Convert.ToInt32(ips.hd[hdNumberToGet].lbs);
        if (Convert.ToInt64(ips.hd[hdNumberToGet].size) * lbs_BYTE == Convert.ToInt64(newHDSize))
            return Convert.ToInt64(newHDSize);
        foreach (var part in ips.hd[hdNumberToGet].partition)
        {
            if (part.active != "1")
                continue;
            long minPartSize_BLK = 0;

            if (!string.IsNullOrEmpty(part.size_override))
                minPartSize_BLK = Convert.ToInt64(part.size_override);
            else if (part.fsid.ToLower() == "8e" || part.fsid.ToLower() == "8e00")
            {
                foreach (var VG in listVGS)
                {
                    if (VG.pv == ips.hd[hdNumberToGet].name + part.number)
                        minPartSize_BLK = VG.minSize_BLK;
                }
                if(minPartSize_BLK == 0)
                    minPartSize_BLK = Convert.ToInt64(part.size);

            }
            else if ((string.IsNullOrEmpty(part.resize) && part.type.ToLower() != "extended") || (part.type.ToLower() == "extended" && EP.isOnlySwap))
                minPartSize_BLK = Convert.ToInt64(part.size);
            else
            {
                if (part.type.ToLower() == "extended")
                    minPartSize_BLK = EP.minSize_BLK;
                
                else
                {
                    if (string.IsNullOrEmpty(part.used_mb))
                        part.used_mb = "0";
                    if (Convert.ToInt64(part.resize) > Convert.ToInt64(part.used_mb))
                        minPartSize_BLK = (Convert.ToInt64(part.resize) * 1024 * 1024) / lbs_BYTE;
                    else
                        minPartSize_BLK = (Convert.ToInt64(part.used_mb) * 1024 * 1024) / lbs_BYTE;
                }
            }

            if (part.type.ToLower() != "logical")
                minHDSizeRequired_BLK += minPartSize_BLK;
        }
        return minHDSizeRequired_BLK * lbs_BYTE;
    }

    public void RenameFolder(string oldName, string newName)
    {
        Utility settings = new Utility();
        try
        {
            string imagePath = settings.GetSettings("Image Store Path");
            if(Directory.Exists(imagePath + oldName))
                Directory.Move(imagePath + oldName, imagePath + newName);
            imagePath = settings.GetSettings("Image Hold Path");
            if (Directory.Exists(imagePath + oldName))
                Directory.Move(imagePath + oldName, imagePath + newName);
            Utility.Message += "<br> Successfully Renamed Image Folder";
        }
        catch (Exception ex)
        {
            Utility.Message = "<br>" + ex.Message;
        }
    }

    public void Create(Image image)
    {
        Utility settings = new Utility();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("images_create", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@imageName", image.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@imageOS", image.OS));
                cmd.Parameters.Add(new NpgsqlParameter("@imageDesc", image.Description));
                cmd.Parameters.Add(new NpgsqlParameter("@imageProtected", image.Protected));
                cmd.Parameters.Add(new NpgsqlParameter("@imageVisible", image.IsVisible));
                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;
                if (Utility.Message.Contains("Successfully"))
                {
                    Directory.CreateDirectory(settings.GetSettings("Image Store Path") + image.Name);
                    Directory.CreateDirectory(settings.GetSettings("Image Hold Path") + image.Name);
                    History history = new History();
                    history.Event = "Create";
                    history.Type = "Image";
                    history.TypeID = image.GetImageID(image.Name);
                    history.CreateEvent(history);
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Create Image.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public void Delete(List<int> dbDelete)
    {
        Utility settings = new Utility();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();
                foreach(var imageToDelete in dbDelete)
                {
                    Image image = new Image();
                    image.ID = imageToDelete.ToString();
                    image = image.Read(image);
                    if (image.Protected == 0)
                    {
                        NpgsqlCommand cmd = new NpgsqlCommand("images_delete", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new NpgsqlParameter("@imageID", imageToDelete));
                        string result = cmd.ExecuteScalar().ToString();
                        if (string.IsNullOrEmpty(result))
                        {
                            Utility.Message = "Could Not Delete Image.  Check The Exception Log For More Info";
                            Logger.Log("Could Not Delete Image.  Image Name Cannot Be Empty.  Given ID: " + imageToDelete);
                            return;
                        }
                        if (Directory.Exists(settings.GetSettings("Image Store Path") + result))
                            Directory.Delete(settings.GetSettings("Image Store Path") + result, true);

                        if (Directory.Exists(settings.GetSettings("Image Hold Path") + result))
                            Directory.Delete(settings.GetSettings("Image Hold Path") + result, true);

                        History history = new History();
                        history.Event = "Delete";
                        history.Type = "Image";
                        history.TypeID = image.ID;
                        history.CreateEvent(history);
                        Utility.Message += "Successfully Deleted " + image.Name + "<br>";
                    }
                    else
                    {
                        Utility.Message += "Could Not Delete " + image.Name + " - It Is Protected" + "<br>";
                    }
                }
                
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Delete Image.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public string GetImageID(string imagename)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand("images_readimageid", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@imageName", imagename));
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

                NpgsqlCommand cmd = new NpgsqlCommand("images_totalcount", conn);
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
                NpgsqlCommand cmd = new NpgsqlCommand("images_import", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@inpath", HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "csvupload" + Path.DirectorySeparatorChar));
                cmd.Parameters.Add(new NpgsqlParameter("@result", NpgsqlDbType.Char, 100));
                cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                conn.Open();
                cmd.ExecuteNonQuery();
                Utility.Message = cmd.Parameters["@result"].Value.ToString() + " Image(s) Imported Successfully";
            }

        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Import Images.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
    }

    public Image Read(Image image)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("images_read", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@imageID", image.ID));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    image.Name = (string)rdr["imagename"];
                    image.OS = (string)rdr["imageos"];
                    image.Description = (string)rdr["imagedesc"];
                    image.Protected = (int)rdr["imageprotected"];
                    image.IsVisible = (int)rdr["imageviewcust"];
                    image.ClientSize = rdr["imageclientsize"].ToString();
                    image.ClientSizeCustom = rdr["imageclientsizecustom"].ToString();
                    image.Checksum = rdr["checksum"].ToString();
                    
                }
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Read Image Info.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return image;
    }

    public DataTable Search(string searchString)
    {
        DataTable table = new DataTable();
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("images_search", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@searchString", searchString));
                conn.Open();
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                table.Load(rdr);
            }
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Search Images.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
        }
        return table;
    }

    public bool Update(Image image)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("images_update", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@imageID", image.ID));
                cmd.Parameters.Add(new NpgsqlParameter("@imageName", image.Name));
                cmd.Parameters.Add(new NpgsqlParameter("@imageOS", image.OS));
                cmd.Parameters.Add(new NpgsqlParameter("@imageDesc", image.Description));
                cmd.Parameters.Add(new NpgsqlParameter("@imageProtected", image.Protected));
                cmd.Parameters.Add(new NpgsqlParameter("@imageVisible", image.IsVisible));

                conn.Open();
                Utility.Message = cmd.ExecuteScalar() as string;

                if (Utility.Message.Contains("Successfully"))
                {
                    History history = new History();
                    history.Event = "Edit";
                    history.Type = "Image";
                    history.TypeID = image.ID;
                    history.CreateEvent(history);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update Image.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
            return false;
        }
    }

    public bool UpdateChecksum(Image image)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("UPDATE images SET checksum=@imageChecksum WHERE imageid=@imageID;", conn);
                cmd.Parameters.Add(new NpgsqlParameter("@imageID", image.ID));
                cmd.Parameters.Add(new NpgsqlParameter("@imageChecksum", image.Checksum));
                conn.Open();
                cmd.ExecuteScalar();
                History history = new History();
                history.Event = "Approve Checksum";
                history.Type = "Image";
                history.TypeID = image.ID;
                history.CreateEvent(history);
                Utility.Message = "This Image Was Successfully Approved";
            }
            return true;
        }
        catch (Exception ex)
        {
            Utility.Message = "Could Not Update Image.  Check The Exception Log For More Info";
            Logger.Log(ex.ToString());
            return false;
        }
    }
    public bool UpdateSpecs(string imagename, string imagesize)
    {
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("images_updatesizecustom", conn);
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
            return false;
        }
        return true;
    }

    public bool ImageProtected(string imagename)
    {
        string result = null;
        try
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Utility.DBString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("images_imageprotected", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("@imagename", imagename));
                conn.Open();
                result = Convert.ToString(cmd.ExecuteScalar());
                if (result == "0")
                    return false;
                else
                    return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return true;
        }
    }
}