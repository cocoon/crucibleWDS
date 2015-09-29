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
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public partial class unicast : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Utility utility = new Utility();
            if (utility.GetSettings("Default Host View") == "all")
                PopulateGrid();
        }
    }

    protected void btnDeploy_Click(object sender, EventArgs e)
    {
        Unicast unicast = new Unicast();
        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        string hostID = gvHosts.DataKeys[gvRow.RowIndex].Value.ToString();     
        Session["hostID"] = hostID;
        Session["direction"] = "push";
        lblTitle.Text = "Deploy The Selected Host?";
        gvConfirm.DataSource = unicast.Confirm(hostID);
        gvConfirm.DataBind();
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
   
    }

    protected void btnUpload_Click(object sender, EventArgs e)
    {
        Unicast unicast = new Unicast();
        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        string hostID = gvHosts.DataKeys[gvRow.RowIndex].Value.ToString();
        Session["hostID"] = hostID;
        Session["direction"] = "pull";
        lblTitle.Text = "Upload The Selected Host?";
        gvConfirm.DataSource = unicast.Confirm(hostID);
        gvConfirm.DataBind();
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });;", true);
        
    }

    protected void OkButton_Click(object sender, EventArgs e)
    {
        Unicast unicast = new Unicast();
        int hostID = Convert.ToInt32((string)(Session["hostID"]));
        Session.Remove("hostID");
        string direction = (string)(Session["direction"]);
        Session.Remove("direction");
        if(direction == "push")
        {
            Image image = new Image();
            unicast = unicast.Read(hostID,unicast);
            string imageID = image.GetImageID(unicast.ImageName);
            Session["imageID"] = imageID;
            if (image.Check_Checksum(imageID))
                unicast.CreateUnicast(direction, hostID);
            else
            {
                lblIncorrectChecksum.Text = "This Image Has Not Been Confirmed And Cannot Be Deployed.  <br>Confirm It Now?";
                ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('incorrectChecksum'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);

            }
            
        }
        else
            unicast.CreateUnicast(direction, hostID);
        Master.Msgbox(Utility.Message);
    }
    protected void OkButtonChecksum_Click(object sender, EventArgs e)
    {
        string imageID = (string)(Session["imageID"]);
        Response.Redirect("~/views/images/view.aspx?page=specs&imageid=" + imageID, false);
        Session.Remove("imageID");
    }
    public string GetSortDirection(string SortExpression)
    {
        if (ViewState[SortExpression] == null)
            ViewState[SortExpression] = "Desc";
        else
            ViewState[SortExpression] = ViewState[SortExpression].ToString() == "Desc" ? "Asc" : "Desc";

        return ViewState[SortExpression].ToString();
    }

    protected void search_Changed(object sender, EventArgs e)
    {
        PopulateGrid();
    }

    protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
    {
        PopulateGrid();

        DataTable dataTable = gvHosts.DataSource as DataTable;

        if (dataTable != null)
        {
            DataView dataView = new DataView(dataTable);
            dataView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
            gvHosts.DataSource = dataView;
            gvHosts.DataBind();
        }
    }

    protected void PopulateGrid()
    {
        Host host = new Host();
        if (Master.IsInMembership("User"))
            gvHosts.DataSource = host.TableForUser(txtSearch.Text);
        else
            gvHosts.DataSource = host.Search(txtSearch.Text);

        gvHosts.DataBind();

        lblTotal.Text = gvHosts.Rows.Count.ToString() + " Result(s) / " + host.GetTotalCount() + " Total Host(s)";
    } 
}
