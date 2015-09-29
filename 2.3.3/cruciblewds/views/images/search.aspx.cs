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
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;

public partial class searchimages : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
     
        if (!IsPostBack)
        {
            if (Master.IsInMembership("User"))
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied");

            Master.Msgbox(Utility.Message); //For Redirects
            PopulateGrid(true);
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        List<int> dbDelete = new List<int>();
        Image image = new Image();

        foreach (GridViewRow row in gvImages.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkSelector");
            if (cb != null && cb.Checked)
            {
                dbDelete.Add(Convert.ToInt32(gvImages.DataKeys[row.RowIndex].Value));
            }
        }
        
        if (dbDelete.Count > 0)
        {
            image.Delete(dbDelete);
            Master.Msgbox(Utility.Message);
            PopulateGrid(true);
            
        }
    }


    protected void search_Changed(object sender, EventArgs e)
    {
        PopulateGrid(true);
       
    }

    protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox hcb = (CheckBox)gvImages.HeaderRow.FindControl("chkSelectAll");
        if (hcb.Checked == true)        
            ToggleCheckState(true);       
        else        
            ToggleCheckState(false);       
    }

    private void ToggleCheckState(bool checkState)
    {
        foreach (GridViewRow row in gvImages.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkSelector");
            if (cb != null)
                cb.Checked = checkState;
        }
    }

    public string GetSortDirection(string SortExpression)
    {
        if (ViewState[SortExpression] == null)
            ViewState[SortExpression] = "Desc";
        else
            ViewState[SortExpression] = ViewState[SortExpression].ToString() == "Desc" ? "Asc" : "Desc";

        return ViewState[SortExpression].ToString();
    }

    protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
    {
        PopulateGrid(true);
        DataTable dataTable = gvImages.DataSource as DataTable;

        if (dataTable != null)
        {
            DataView dataView = new DataView(dataTable);
            dataView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
            gvImages.DataSource = dataView;
            gvImages.DataBind();

            PopulateGrid(false);
        }
    }

    public static long GetDirectorySize(DirectoryInfo d)
    {
        long Size = 0;
        FileInfo[] fis = d.GetFiles();
        foreach (FileInfo fi in fis)
        {
            Size += fi.Length;
        }

        //Don't recurse as of version 2.3.0
        /*DirectoryInfo[] dis = d.GetDirectories();
        foreach (DirectoryInfo di in dis)
        {
            Size += GetDirectorySize(di);
        }*/
        return (Size);
    }

    protected void btnHds_Click(object sender, EventArgs e)
    {
        Utility utility = new Utility();
        GridViewRow row = (GridViewRow)(sender as Control).Parent.Parent;
        Image image = new Image();
        image.ID = image.GetImageID(row.Cells[4].Text);
        image.Read(image);

        GridView gvHDs = (GridView)row.FindControl("gvHDs");
        List<HD_Physical_Specs> specslist = new List<HD_Physical_Specs>();
        Image_Physical_Specs ips = new Image_Physical_Specs();
        try
        {
            ips = JsonConvert.DeserializeObject<Image_Physical_Specs>(image.ClientSize);
        }
        catch
        { return; }
       if(ips == null)
            return;
        foreach (var hd in ips.hd)
        {
            specslist.Add(hd);
        }

      
        LinkButton btn = (LinkButton)row.FindControl("btnHDs");
        if (gvHDs.Visible == false)
        {
            var td = row.FindControl("tdHds");
            td.Visible = true;
            gvHDs.Visible = true;
            
            gvHDs.DataSource = specslist;
            gvHDs.DataBind();
            btn.Text = "-";
        }
        else
        {
            var td = row.FindControl("tdHds");
            td.Visible = false;
            gvHDs.Visible = false;
          
            btn.Text = "+";
        }

        foreach (GridViewRow hdrow in gvHDs.Rows)
        {
            string imagePath = null;

            try
            {
                Label lbl = hdrow.FindControl("lblHDSize") as Label;
                try
                {
                    if (hdrow.RowIndex.ToString() == "0")
                    {
                        imagePath = utility.GetSettings("Image Store Path") + row.Cells[4].Text;

                    }
                    else
                    {
                        string selectedHD = (hdrow.RowIndex + 1).ToString();
                        imagePath = utility.GetSettings("Image Store Path") + row.Cells[4].Text + Path.DirectorySeparatorChar + "hd" + selectedHD;

                    }
                }
                catch
                {
                    return;
                }

                
                float size = GetDirectorySize(new DirectoryInfo(imagePath)) / 1024f / 1024f / 1024f;
                if (size == 0.0f)
                    lbl.Text = "N/A";
                else
                {
                    lbl.Text = size.ToString("#.##") + " GB";
                    if (lbl.Text == " GB")
                        lbl.Text = "< .01 GB";
                }
            }
            catch
            {
                Label lbl = hdrow.FindControl("lblHDSize") as Label;
                lbl.Text = "N/A";
                Utility.Message = "";
            }

            try
            {

                Label lblClient = hdrow.FindControl("lblHDSizeClient") as Label;


                float fltClientSize = image.CalculateMinSizeHD(image.Name, hdrow.RowIndex, "1") / 1024f / 1024f / 1024f;

                if (fltClientSize == 0.0f)
                    lblClient.Text = "N/A";
                else
                {
                    lblClient.Text = fltClientSize.ToString("#.##") + " GB";
                    if (lblClient.Text == " GB")
                        lblClient.Text = "< .01 GB";
                }
            }
            catch (Exception)
            {
                Label lblClient = hdrow.FindControl("lblHDSizeClient") as Label;
                lblClient.Text = "N/A";
                Utility.Message = "";
            }
        }

    }

    protected void PopulateGrid(bool bind)
    {
        if (bind)
        {
            Image image = new Image();
            gvImages.DataSource = image.Search(txtSearch.Text);
            gvImages.DataBind();
            lblTotal.Text = gvImages.Rows.Count.ToString() + " Result(s) / " + image.GetTotalCount() + " Total Image(s)";
        }

        Utility utility = new Utility();
        foreach (GridViewRow row in gvImages.Rows)
        {
            try
            {
                Label lbl = row.FindControl("lblSize") as Label;
                string imagePath = utility.GetSettings("Image Store Path") + row.Cells[4].Text;
              
                float size = GetDirectorySize(new DirectoryInfo(imagePath)) / 1024f / 1024f / 1024f;
                if (size == 0.0f)
                    lbl.Text = "N/A";
                else
                    lbl.Text = size.ToString("#.##") + " GB";
            }
            catch 
            {
                Label lbl = row.FindControl("lblSize") as Label;
                lbl.Text = "N/A";
                Utility.Message = "";
            }

            try
            {

                Label lblClient = row.FindControl("lblSizeClient") as Label;
                Image img = new Image();
                string tmp = ((HiddenField)row.FindControl("HiddenID")).Value;
                img.ID = ((HiddenField)row.FindControl("HiddenID")).Value;
                img = img.Read(img);

                
                float fltClientSize = img.CalculateMinSizeHD(img.Name, 0, "1") / 1024f /1024f /1024f;
                if (fltClientSize == 0.0f)
                    lblClient.Text = "N/A";
                else
                    lblClient.Text = fltClientSize.ToString("#.##") + " GB";
            }
            catch (Exception ex)
            {
                Label lblClient = row.FindControl("lblSizeClient") as Label;
                lblClient.Text = "N/A";
                Utility.Message = "";
            }
        }

    }
}