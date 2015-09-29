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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data;

public partial class modifyimages : System.Web.UI.Page
{
    public string requestedPage { get; set; }
    public Image image { get; set; }

    #region View Page

    protected void Page_Load(object sender, EventArgs e)
    {
        image = new Image();
        requestedPage = Request.QueryString["page"] as string;
        image.ID = Request.QueryString["imageid"] as string;
        image.Read(image);

        if (!IsPostBack)
        {
            if (Master.IsInMembership("User"))
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied",true);

            Master.Msgbox(Utility.Message);

            switch (requestedPage)
            {
                case "edit":
                    lblSubNav.Text = "| edit";
                    edit.Visible = true;
                    edit_page();
                    break;
                case "history":
                    lblSubNav.Text = "| history";
                    historypage.Visible = true;
                    history_page();
                    break;
                case "specs":
                    lblSubNav.Text = "| Specs";
                    specspage.Visible = true;
                    specs_page();
                    break;
                default:
                    break;
            }
        }

    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        lblTitle.Text = "Delete This Image?";
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
    }

    protected void OkButton_Click(object sender, EventArgs e)
    {
        List<int> delList = new List<int>();
        delList.Add(Convert.ToInt32(image.ID));
        image.Delete(delList);
        if (Utility.Message.Contains("Successfully"))
            Response.Redirect("~/views/images/search.aspx");
        else
            Master.Msgbox(Utility.Message);
    }
    #endregion

    #region Edit Page

    protected void edit_page()
    {
        Utility utility = new Utility();
        ViewState["currentName"] = image.Name;
        string currentName = (string)(ViewState["currentName"]);

        txtImageName.Text = image.Name;
        txtImageDesc.Text = image.Description;
        if (image.Protected == 1)
            chkProtected.Checked = true;
        if (image.IsVisible == 1)
            chkVisible.Checked = true;
        
        try
        {
             if (Directory.Exists(utility.GetSettings("Image Hold Path") + currentName))
             {
                  lblImageHold.Text = currentName + " Exists In Image Hold Path : Pass";
                  ViewState["holdCheck"] = "true";
             }
             else
             {
                  lblImageHold.Text = currentName + " Exists In Image Hold Path : Fail";
                  ViewState["holdCheck"] = "false";
             }
        }
        catch(Exception ex)
        {
             lblImageHold.Text = currentName + " Exists In Image Hold Path : Error " + ex.Message;
             ViewState["holdCheck"] = "false";
        }

        try
        {
             if (Directory.Exists(utility.GetSettings("Image Store Path") + currentName))
             {
                  lblImage.Text = currentName + " Exists In Image Store Path : Pass";
                  ViewState["storeCheck"] = "true";
             }
             else
             {
                  lblImage.Text = currentName + " Exists In Image Store Path : Fail";
                  ViewState["storeCheck"] = "false";
             }
        }
        catch (Exception ex)
        {
             lblImage.Text = currentName + " Exists In Image Store Path : Error " + ex.Message;
             ViewState["storeCheck"] = "false";
        }

        try
        {
             if (Directory.GetFiles(utility.GetSettings("Image Hold Path") + currentName).Length > 0)
             {
                  lblImageHoldStatus.Text = currentName + " Image Hold Is Not Empty." + "This Is Normal If You Are Currently Uploading This Image.";
                  ViewState["holdEmpty"] = "false";
             }
             else
             {
                  lblImageHoldStatus.Text = currentName + " Image Hold Is Empty";
                  ViewState["holdEmpty"] = "true";
             }
        }
        catch (Exception ex)
        {
             lblImageHoldStatus.Text = "Could Not Determine If " + currentName + " Image Hold Is Empty " + ex.Message;
             ViewState["holdEmpty"] = "true";
        }

        try
        {
             if (Directory.GetFiles(utility.GetSettings("Image Store Path") + currentName).Length > 0)
             {
                  lblImageStatus.Text = currentName + " Image Store Is Not Empty.";
                  ViewState["storeEmpty"] = "false";
             }
             else
             {
                  lblImageStatus.Text = currentName + " Image Store Is Empty";
                  ViewState["storeEmpty"] = "true";
             }
        }
        catch (Exception ex)
        {
             lblImageStatus.Text = "Could Not Determine If " + currentName + " Image Hold Is Empty " + ex.Message;
             ViewState["storeEmpty"] = "false";
        }

            
    }

    protected void btnUpdateImage_Click(object sender, EventArgs e)
    {
        if (Utility.NoSpaceNotEmpty(txtImageName.Text))
        {
            string currentName = (string)(ViewState["currentName"]);
            image.Name = txtImageName.Text;
            image.OS = "";
            image.Description = txtImageDesc.Text;
            if (chkProtected.Checked)
                image.Protected = 1;
            else
                image.Protected = 0;
            if (chkVisible.Checked)
                image.IsVisible = 1;
            else
                image.IsVisible = 0;

            if (image.Update(image))
            {
                if (currentName != image.Name)
                    image.RenameFolder(currentName, image.Name);
            }
            Master.Msgbox(Utility.Message);
        }
        else
            Master.Msgbox("Name Cannot Be Empty Or Contain Spaces");
    }

    protected void btnFixImage_Click(object sender, EventArgs e)
    {
         bool needsFixed = false;
         Utility utility = new Utility();
         string currentName = (string)(ViewState["currentName"]);

         if ((string)(ViewState["storeCheck"]) == "false")
         {
              needsFixed = true;
              try
              {
                   Directory.CreateDirectory(utility.GetSettings("Image Store Path") + currentName);
                   Utility.Message = "Successfully Created Directory In Image Store Path. ";
              }
              catch(Exception ex)
              {
                   Logger.Log("Could Not Create Directory In Image Store Path. " + ex.Message);
                   Utility.Message = "Could Not Create Directory In Image Store Path.  Check The Exception Log For More Info. ";
              }
         }
         if ((string)(ViewState["holdCheck"]) == "false")
         {
              needsFixed = true;
              try
              {
                   Directory.CreateDirectory(utility.GetSettings("Image Hold Path") + currentName);
                   Utility.Message += "Successfully Created Directory In Image Hold Path. ";
              }
              catch (Exception ex)
              {
                   Logger.Log("Could Not Create Directory In Image Hold Path. " + ex.Message);
                   Utility.Message += "Could Not Create Directory In Image Hold Path.  Check The Exception Log For More Info. ";
              }
         }

         if ((string)(ViewState["holdCheck"]) == "true" && (string)(ViewState["storeCheck"]) == "true")
         {
              if ((string)(ViewState["storeEmpty"]) == "true" && (string)(ViewState["holdEmpty"]) == "false")
              {
                   needsFixed = true;
                   try
                   {
                        Utility.MoveFolder(utility.GetSettings("Image Hold Path") + currentName, utility.GetSettings("Image Store Path") + currentName);

                        try
                        {
                             Directory.CreateDirectory(utility.GetSettings("Image Hold Path") + currentName); // for next upload
                             Utility.Message = "Successfully Moved Image From Hold To Store";
                        }
                        catch (Exception ex)
                        {
                             Logger.Log("Could Not Recreate Directory " + ex.Message);
                             Utility.Message = "Could Not Recreate Directory,  You Must Create It Before You Can Upload Again";
                        }


                   }
                   catch (Exception ex)
                   {
                        Logger.Log("Could Not Move Image From Hold Path To Store Path " + ex.Message);
                        Utility.Message = "Could Not Move Image From Hold Path To Store Path.  Check The Exception Log For More Info.";
                   }
              }
         }

         if (!needsFixed)
              Utility.Message = "No Fixes Are Needed For This Image";
         Master.Msgbox(Utility.Message);
    }

    #endregion

    #region History Page

    protected void history_page()
    {
        if (!IsPostBack)
            ddlLimit.SelectedValue = "10";
        History history = new History();
        history.Type = "Image";
        history.TypeID = image.ID;
        gvHistory.DataSource = history.Read(history, ddlLimit.Text);
        gvHistory.DataBind();
        Master.Msgbox(Utility.Message);

    }

    protected void ddlLimit_SelectedIndexChanged(object sender, EventArgs e)
    {
        History history = new History();
        history.Type = "Image";
        history.TypeID = image.ID;
        gvHistory.DataSource = history.Read(history, ddlLimit.Text);
        gvHistory.DataBind();
        Master.Msgbox(Utility.Message);
    }
    #endregion

    #region Specifications Page

    protected void btnRestoreImageSpecs_Click(object sender, EventArgs e)
    {
        if (image.UpdateSpecs(image.Name, ""))
            Master.Msgbox("Successfully Restored Image Specs.  Reload This Page To View Changes.");
        else
            Master.Msgbox("Could Not Restore Image Specs");
    }

    protected void btnUpdateImageSpecs_Click(object sender, EventArgs e)
    {
        Image_Physical_Specs ips = new Image_Physical_Specs();
        try
        {
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);
        }
        catch
        { return; }

        int rowCounter = 0;
        foreach (GridViewRow row in gvHDs.Rows)
        {
            CheckBox box = row.FindControl("chkHDActive") as CheckBox;
            if (box.Checked)
                ips.hd[rowCounter].active = "1";
            else
                ips.hd[rowCounter].active = "0";

            GridView gvParts = (GridView)row.FindControl("gvParts");

            int partCounter = 0;
            foreach (GridViewRow partRow in gvParts.Rows)
            {
                CheckBox boxPart = partRow.FindControl("chkPartActive") as CheckBox;
                if (boxPart.Checked)
                    ips.hd[rowCounter].partition[partCounter].active = "1";
                else
                    ips.hd[rowCounter].partition[partCounter].active = "0";

                TextBox txtCustomSize = partRow.FindControl("txtCustomSize") as TextBox;
                if (!string.IsNullOrEmpty(txtCustomSize.Text))
                {
                    string customSize_BLK = (Convert.ToInt64(txtCustomSize.Text) * 1024 * 1024 / Convert.ToInt32(ips.hd[rowCounter].lbs)).ToString();
                    ips.hd[rowCounter].partition[partCounter].size_override = customSize_BLK;
                }
               

                GridView gvVG = (GridView)partRow.FindControl("gvVG");
                foreach(GridViewRow vg in gvVG.Rows)
                {
                    GridView gvLVS = (GridView)vg.FindControl("gvLVS");
                    int lvCounter = 0;
                    foreach (GridViewRow LV in gvLVS.Rows)
                    {
                        CheckBox boxLv = LV.FindControl("chkPartActive") as CheckBox;
                        if (boxLv.Checked)
                            ips.hd[rowCounter].partition[partCounter].vg.lv[lvCounter].active = "1";
                        else
                            ips.hd[rowCounter].partition[partCounter].vg.lv[lvCounter].active = "0";

                        TextBox txtCustomSizeLv = LV.FindControl("txtCustomSize") as TextBox;
                        if (!string.IsNullOrEmpty(txtCustomSizeLv.Text))
                        {
                            string customSize_BLK = (Convert.ToInt64(txtCustomSizeLv.Text) * 1024 * 1024 / Convert.ToInt32(ips.hd[rowCounter].lbs)).ToString();
                            ips.hd[rowCounter].partition[partCounter].vg.lv[lvCounter].size_override = customSize_BLK;
                        }
                        lvCounter++;
                    }
                }
                partCounter++;
            }
            rowCounter++;
        }

        if (image.UpdateSpecs(image.Name, JsonConvert.SerializeObject(ips)))
            Master.Msgbox("Successfully Updated Image Specs");
        else
            Master.Msgbox("Could Not Update Image Specs");
    }

    protected void specs_page()
    {
        Utility utility = new Utility();

        try
        {
            Image_Physical_Specs ips = new Image_Physical_Specs();
            if (!string.IsNullOrEmpty(image.ClientSizeCustom))
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
            else
                ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);


            List<HD_Physical_Specs> specslist = new List<HD_Physical_Specs>();

            foreach (var hd in ips.hd)
            {
                long logical_block_size = Convert.ToInt64(hd.lbs);
                hd.size = (Convert.ToInt64(hd.size) * logical_block_size / 1000f / 1000f / 1000f).ToString("#.##") + " GB" + " / " + (Convert.ToInt64(hd.size) * logical_block_size / 1024f / 1024f / 1024f).ToString("#.##") + " GB";
                specslist.Add(hd);
            }

            gvHDs.DataSource = specslist;
            gvHDs.DataBind();

            foreach (GridViewRow row in gvHDs.Rows)
            {
                string isActive = ((HiddenField)row.FindControl("HiddenActive")).Value;
                if (isActive == "1")
                {
                    CheckBox box = row.FindControl("chkHDActive") as CheckBox;
                    box.Checked = true;
                }
            }


        }
        catch
        {
            lblSpecsUnavailable.Text = "Image Specifications Will Be Available After The Image Is Uploaded";
            lblSpecsUnavailable.Visible = true;
        }

        if(utility.GetSettings("Image Checksum") == "On" && lblSpecsUnavailable.Visible != true)
        {
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

                    incorrectChecksum.Visible = true;
                    ViewState["checkSum"] = physicalImageJson;
                }
            }
            catch(Exception ex)
            {
                Logger.Log(ex.Message + " This can be safely ignored if the image has not been uploaded yet");
                incorrectChecksum.Visible = false;
            }
        }


        
    }
    protected void btnConfirmChecksum_Click(object sender, EventArgs e)
    {   
        image.Checksum = (string)(ViewState["checkSum"]);
        image.UpdateChecksum(image);
        Master.Msgbox(Utility.Message);
        Response.Redirect("~/views/images/view.aspx?page=specs&imageid=" + image.ID, true);
    }

    protected void btnPart_Click(object sender, EventArgs e)
    {
        Utility utility = new Utility();
        string imagePath = utility.GetSettings("Image Store Path") + image.Name;

        string selectedHD = (string)(ViewState["selectedHD"]);
        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        GridView gv = (GridView)gvRow.FindControl("gvFiles");
        string selectedPartition = gvRow.Cells[3].Text;

        String[] partFiles = null;
        try
        {
            if (selectedHD == "0")
                partFiles = Directory.GetFiles(imagePath + Path.DirectorySeparatorChar, "part" + selectedPartition + ".*");
            else
            {
                selectedHD = (Convert.ToInt32(selectedHD) + 1).ToString();
                partFiles = Directory.GetFiles(imagePath + Path.DirectorySeparatorChar + "hd" + selectedHD +Path.DirectorySeparatorChar, "part" + selectedPartition + ".*");
            }
        }
        catch
        {
            return;
        }
        if (partFiles.Length == 0)
        {
            return;
        }

        List<string> partList = new List<string>();


        DataTable dt = new DataTable();
        DataRow dr = null;
        dt.Columns.Add(new DataColumn("fileName", typeof(string)));
        dt.Columns.Add(new DataColumn("serverSize", typeof(string)));

        int z = 0;
        foreach (var file in partFiles)
        {
            try
            {
                FileInfo fi = new FileInfo(file);
                dr = dt.NewRow();
                dr["fileName"] = fi.Name;
                dr["serverSize"] = (fi.Length / 1024f / 1024f).ToString("#.##") + " MB";
                dt.Rows.Add(dr);
            }
            catch
            {
                FileInfo fi = new FileInfo(file);
                dr = dt.NewRow();
                dr["fileName"] = fi.Name;
                dt.Rows.Add(dr);
            }
            z++;
        }

        LinkButton btn = (LinkButton)gvRow.FindControl("partClick");

        if (gv.Visible == false)
        {
            gv.Visible = true;
            var td = gvRow.FindControl("tdFile");
            td.Visible = true;
            gv.DataSource = dt;
            gv.DataBind();
            btn.Text = "-";
        }
        else
        {
            gv.Visible = false;
            var td = gvRow.FindControl("tdFile");
            td.Visible = false;
            btn.Text = "+";
        }
    }

    protected void btnParts_Click(object sender, EventArgs e)
    {

        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        GridView gv = (GridView)gvRow.FindControl("gvParts");
       
        string selectedHD = gvRow.Cells[3].Text;
        ViewState["selectedHD"] = gvRow.RowIndex.ToString();
        ViewState["selectedHDName"] = selectedHD;
        Image_Physical_Specs ips = new Image_Physical_Specs();
        if (!string.IsNullOrEmpty(image.ClientSizeCustom))
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
        else
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);

        List<Partition_Physical_Specs> specslist = new List<Partition_Physical_Specs>();
        List<VG_Physical_Specs> vgList = new List<VG_Physical_Specs>();
        foreach (var hd in ips.hd)
        {
            if (hd.name == selectedHD)
            {
                foreach (var part in hd.partition)
                {
                    long logical_block_size = Convert.ToInt64(hd.lbs);
                    if ((Convert.ToInt64(part.size) * logical_block_size) < 1048576000)
                        part.size = (Convert.ToInt64(part.size) * logical_block_size / 1024f / 1024f ).ToString("#.##") + " MB";
                    else
                        part.size = (Convert.ToInt64(part.size) * logical_block_size / 1024f / 1024f / 1024f).ToString("#.##") + " GB";
                    part.used_mb = part.used_mb + " MB";
                    if (!string.IsNullOrEmpty(part.size_override))
                        part.size_override = (Convert.ToInt64(part.size_override) * logical_block_size / 1024f / 1024f).ToString();
                    if(!string.IsNullOrEmpty(part.resize))
                        part.resize = part.resize + " MB";
                    specslist.Add(part);

                    if(part.vg != null)
                        if(part.vg.name != null)

                        vgList.Add(part.vg);



                }
            }     
        }

      
        
        LinkButton btn = (LinkButton)gvRow.FindControl("btnParts");
        if (gv.Visible == false)
        {
            gv.Visible = true;

            var td = gvRow.FindControl("tdParts");
            td.Visible = true;
            gv.DataSource = specslist;
            gv.DataBind();

            btn.Text = "-";
        }
        else
        {
            gv.Visible = false;

            var td = gvRow.FindControl("tdParts");
            td.Visible = false;
            btn.Text = "+";
        }

        foreach (GridViewRow row in gv.Rows)
        {
            GridView gvVG = (GridView)row.FindControl("gvVG");
            foreach(var vg in vgList)
            {
                if(vg.pv == selectedHD + row.Cells[3].Text)
                {
                    List<VG_Physical_Specs> vgListBind = new List<VG_Physical_Specs>();
                    vgListBind.Add(vg);
                    gvVG.DataSource = vgListBind;
                    gvVG.DataBind();
                    gvVG.Visible = true;
                    var td = row.FindControl("tdVG");
                    td.Visible = true;
                }

            }
            string isActive = ((HiddenField)row.FindControl("HiddenActivePart")).Value;
            if (isActive == "1")
            {
                CheckBox box = row.FindControl("chkPartActive") as CheckBox;
                box.Checked = true;
            }
        }

    }

    protected void btnVG_Click(object sender, EventArgs e)
    {
        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        GridView gv = (GridView)gvRow.FindControl("gvLVS");

        string selectedHD = (string)(ViewState["selectedHD"]);

        Image_Physical_Specs ips = new Image_Physical_Specs();
        if (!string.IsNullOrEmpty(image.ClientSizeCustom))
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSizeCustom);
        else
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);


        List<LV_Physical_Specs> lvList = new List<LV_Physical_Specs>();

        foreach(var partition in ips.hd[Convert.ToInt32(selectedHD)].partition)
        {
            if (partition.vg.lv != null)
            {
                if (partition.vg.name != null)
                {
                    foreach (var lv in partition.vg.lv)
                    {
                        if (gvRow.Cells[1].Text == lv.vg)
                        {
                            long logical_block_size = Convert.ToInt64(ips.hd[Convert.ToInt32(selectedHD)].lbs);
                            if ((Convert.ToInt64(lv.size) * logical_block_size) < 1048576000)
                                lv.size = (Convert.ToInt64(lv.size) * logical_block_size / 1024f / 1024f).ToString("#.##") + " MB";
                            else
                                lv.size = (Convert.ToInt64(lv.size) * logical_block_size / 1024f / 1024f / 1024f).ToString("#.##") + " GB";
                            lv.used_mb = lv.used_mb + " MB";
                            if (!string.IsNullOrEmpty(lv.size_override))
                                lv.size_override = (Convert.ToInt64(lv.size_override) * logical_block_size / 1024f / 1024f).ToString();
                            if (!string.IsNullOrEmpty(lv.resize))
                                lv.resize = lv.resize + " MB";


                            lvList.Add(lv);
                        }
                    }
                }
            }

        }

        LinkButton btn = (LinkButton)gvRow.FindControl("vgClick");
        if (gv.Visible == false)
        {
            gv.Visible = true;

            var td = gvRow.FindControl("tdLVS");
            td.Visible = true;
            gv.DataSource = lvList;
            gv.DataBind();
            btn.Text = "-";
        }

        else
        {
            gv.Visible = false;
            var td = gvRow.FindControl("tdLVS");
            td.Visible = false;
            btn.Text = "+";
        }

        foreach (GridViewRow row in gv.Rows)
        {

            string isActive = ((HiddenField)row.FindControl("HiddenActivePart")).Value;
            if (isActive == "1")
            {
                CheckBox box = row.FindControl("chkPartActive") as CheckBox;
                box.Checked = true;
            }
        }
    }
    #endregion
}


