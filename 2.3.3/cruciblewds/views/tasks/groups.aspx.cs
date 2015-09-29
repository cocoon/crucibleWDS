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

public partial class multicast : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        PopulateGrid();
    }

    protected void btnMulticast_Click(object sender, EventArgs e)
    {
        Multicast multicast = new Multicast();
        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        string groupID = gvGroups.DataKeys[gvRow.RowIndex].Value.ToString();
        Session["groupID"] = groupID;
        Session["isGroupUnicast"] = 0;
        lblTitle.Text = "Multicast The Selected Group?";
        gvConfirm.DataSource = multicast.Confirm(groupID);
        gvConfirm.DataBind();
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() { var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
    }

    protected void btnUnicast_Click(object sender, EventArgs e)
    {
        Multicast multicast = new Multicast();
        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        string groupID = gvGroups.DataKeys[gvRow.RowIndex].Value.ToString();
        Session["groupID"] = groupID;
        Session["isGroupUnicast"] = 1;
        lblTitle.Text = "Unicast All The Hosts In The Selected Group?";
        gvConfirm.DataSource = multicast.Confirm(groupID);
        gvConfirm.DataBind();
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() { var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
    }

    protected void btnConfirm_Click(object sender, EventArgs e)
    {
        int groupID = Convert.ToInt32((string)(Session["groupID"]));
        int isUnicast = Convert.ToInt32(Session["isGroupUnicast"]);
        Group groupInfo = new Group();
        groupInfo.ID = groupID.ToString();
        groupInfo.Read(groupInfo);
        Image imageInfo = new Image();
        imageInfo.ID = imageInfo.GetImageID(groupInfo.Image);
        Session["imageID"] = imageInfo.ID;
        if (imageInfo.Check_Checksum(imageInfo.ID))
        {
            if (isUnicast == 1)
            {
                Unicast unicast = new Unicast();
                List<int> listHostID = new List<int>();
                listHostID = unicast.UnicastFromGroup(groupID);

                for (int z = 0; z < listHostID.Count; z++)
                    unicast.CreateUnicast("push", listHostID[z]);
                Utility.Message = "Started " + listHostID.Count + " Tasks";
            }
            else
            {
                Multicast multicast = new Multicast();
                multicast.CreateMulticast(groupID);
            }
            Session.Remove("groupID");
            Session.Remove("isGroupUnicast");
            Master.Msgbox(Utility.Message);
        }
        else
        {

            lblIncorrectChecksum.Text = "This Image Has Not Been Confirmed And Cannot Be Deployed.  <br>Confirm It Now?";
            ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('incorrectChecksum'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);

        }
    }

    protected void OkButtonChecksum_Click(object sender, EventArgs e)
    {
        string imageID = (string)(Session["imageID"]);
        Response.Redirect("~/views/images/view.aspx?page=specs&imageid=" + imageID, false);
        Session.Remove("imageID");
    }

    protected void PopulateGrid()
    {
        Group group = new Group();

        if (Master.IsInMembership("User"))
            gvGroups.DataSource = group.TableForUser(txtSearch.Text);
        else
            gvGroups.DataSource = group.Search(txtSearch.Text);

        gvGroups.DataBind();

        lblTotal.Text = gvGroups.Rows.Count.ToString() + " Result(s) / " + group.GetTotalCount() + " Total Group(s)";
    }
}
