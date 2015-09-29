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
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using Mono.Unix.Native;

public partial class modifygroups : System.Web.UI.Page
{
    public string requestedPage { get; set; }
    public Group group { get; set; }

    #region View Page

    protected void Page_Load(object sender, EventArgs e)
    {
        group = new Group();
        requestedPage = Request.QueryString["page"] as string;
        group.ID = Request.QueryString["groupid"] as string;
        group.Read(group);

        if (!IsPostBack)
        {

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
                case "custombootmenu":
                    lblSubNav.Text = "| boot menu";
                    custombootmenu.Visible = true;
                    custombootmenu_page();
                    break;
                default:
                    break;
            }
        }
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        lblTitle.Text = "Delete This Group?";
        Session["direction"] = "delete";
        gvConfirm.DataBind(); // clear gridview if deploy or upload was clicked first
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);

    }

    protected void btnMulticast_Click(object sender, EventArgs e)
    {
        Multicast multicast = new Multicast();
        Session["isGroupUnicast"] = 0;
        lblTitle.Text = "Multicast The Selected Group?";
        gvConfirm.DataSource = multicast.Confirm(group.ID);
        gvConfirm.DataBind();
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() { var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
    }

    protected void btnUnicast_Click(object sender, EventArgs e)
    {
        Multicast multicast = new Multicast();
        Session["isGroupUnicast"] = 1;
        lblTitle.Text = "Unicast All The Hosts In The Selected Group?";
        gvConfirm.DataSource = multicast.Confirm(group.ID);
        gvConfirm.DataBind();
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() { var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
    }

    protected void btnConfirm_Click(object sender, EventArgs e)
    {
        if ((string)Session["direction"] == "delete")
        {
            Session.Remove("direction");
            List<int> delList = new List<int>();
            delList.Add(Convert.ToInt32(group.ID));
            group.Delete(delList);
            if (Utility.Message.Contains("Successfully"))
                Response.Redirect("~/views/groups/search.aspx");
            else
                Master.Msgbox(Utility.Message);
        }
        else
        {
            Image imageInfo = new Image();
            imageInfo.ID = imageInfo.GetImageID(group.Image);
            Session["imageID"] = imageInfo.ID;
            if (imageInfo.Check_Checksum(imageInfo.ID))
            {
                int isUnicast = Convert.ToInt32(Session["isGroupUnicast"]);
                if (isUnicast == 1)
                {
                    Unicast unicast = new Unicast();
                    List<int> listHostID = new List<int>();
                    listHostID = unicast.UnicastFromGroup(Convert.ToInt32(group.ID));
                    for (int z = 0; z < listHostID.Count; z++)
                        unicast.CreateUnicast("push", listHostID[z]);
                    Utility.Message = "Started " + listHostID.Count + " Tasks";
                    History history = new History();
                    history.Event = "Unicast";
                    history.Type = "Group";
                    history.TypeID = group.ID;
                    history.CreateEvent(history);

                }
                else
                {
                    Multicast multicast = new Multicast();
                    multicast.CreateMulticast(Convert.ToInt32(group.ID));
                }
                Session.Remove("isGroupUnicast");
                Master.Msgbox(Utility.Message);
            }

            else
            {
                lblIncorrectChecksum.Text = "This Image Has Not Been Confirmed And Cannot Be Deployed.  <br>Confirm It Now?";
                ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('incorrectChecksum'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
            }
        }
    }

    protected void OkButtonChecksum_Click(object sender, EventArgs e)
    {
        string imageID = (string)(Session["imageID"]);
        Response.Redirect("~/views/images/view.aspx?page=specs&imageid=" + imageID, false);
        Session.Remove("imageID");
    }

    #endregion

    #region Edit Page

    protected void edit_page()
    {
        Master.Msgbox(Utility.Message);

        if (Master.IsInMembership("User"))
        {
            WDSUser user = new WDSUser();
            user.ID = user.GetID(HttpContext.Current.User.Identity.Name);
            user = user.Read(user);
            List<string> listManagementGroups = user.GroupManagement.Split(' ').ToList<string>();

            bool isAuthorized = false;
            foreach (string id in listManagementGroups)
            {
                if (group.ID == id)
                {
                    isAuthorized = true;
                    break;
                }
            }

            if (!isAuthorized)
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied");
        }

        ddlGroupImage.DataSource = Utility.PopulateImagesDdl();
        ddlGroupImage.DataBind();
        ddlGroupImage.Items.Insert(0, "Select Image");

        ddlGroupKernel.DataSource = Utility.GetKernels();
        ddlGroupKernel.DataBind();
        ListItem itemKernel = ddlGroupKernel.Items.FindByText("kernel");
        if (itemKernel != null)
            ddlGroupKernel.SelectedValue = "speed";
        else
            ddlGroupKernel.Items.Insert(0, "Select Kernel");

        ddlGroupBootImage.DataSource = Utility.GetBootImages();
        ddlGroupBootImage.DataBind();
        ListItem itemBootImage = ddlGroupBootImage.Items.FindByText("initrd.gz");
        if (itemBootImage != null)
            ddlGroupBootImage.SelectedValue = "initrd.gz";
        else
            ddlGroupBootImage.Items.Insert(0, "Select Boot Image");


        lbScripts.DataSource = Utility.GetScripts();
        lbScripts.DataBind();

        txtGroupName.Text = group.Name;
        txtGroupDesc.Text = group.Description;
        ddlGroupImage.Text = group.Image;
        ddlGroupKernel.Text = group.Kernel;
        ddlGroupBootImage.Text = group.BootImage;
        txtGroupArguments.Text = group.Args;
        txtGroupSenderArgs.Text = group.SenderArgs;
        if (!string.IsNullOrEmpty(group.Scripts))
        {
            List<string> listhostScripts = group.Scripts.Split(',').ToList<string>();
            foreach (ListItem item in lbScripts.Items)
                foreach (var script in listhostScripts)
                    if (item.Value == script)
                        item.Selected = true;
        }

        gvRemove.DataSource = group.CurrentMembers(group.Name);
        gvRemove.DataBind();

        Utility utility = new Utility();
        if (utility.GetSettings("Default Host View") == "all")
            PopulateGrid();

    }
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (Utility.NoSpaceNotEmpty(txtGroupName.Text))
        {
            if (Utility.NoSpaceNotEmpty(ddlGroupKernel.Text))
            {
                if (Utility.NoSpaceNotEmpty(ddlGroupBootImage.Text))
                {
                    List<int> currentMembers = new List<int>();
                    List<int> membersToAdd = new List<int>();
                    List<int> membersToRemove = new List<int>();

                    group.Name = txtGroupName.Text;
                    group.Description = txtGroupDesc.Text;
                    group.Image = ddlGroupImage.Text;
                    group.Kernel = ddlGroupKernel.Text;
                    group.BootImage = ddlGroupBootImage.Text;
                    group.Args = txtGroupArguments.Text;
                    group.SenderArgs = txtGroupSenderArgs.Text;
                    group.Scripts = null;
                    foreach (ListItem item in lbScripts.Items)
                        if (item.Selected == true)
                            group.Scripts += item.Value + ",";
                    if (group.Update(group))
                    {
                      
                        foreach (GridViewRow row in gvAdd.Rows)
                        {
                            CheckBox cb = (CheckBox)row.FindControl("chkSelector");
                            if (cb != null && cb.Checked)
                                membersToAdd.Add(Convert.ToInt32(gvAdd.DataKeys[row.RowIndex].Value));
                        }

                        foreach (GridViewRow row in gvRemove.Rows)
                        {
                            currentMembers.Add(Convert.ToInt32(gvRemove.DataKeys[row.RowIndex].Value));

                            CheckBox cb = (CheckBox)row.FindControl("chkSelector");
                            if (cb != null && cb.Checked)
                                membersToRemove.Add(Convert.ToInt32(gvRemove.DataKeys[row.RowIndex].Value));
                        }

                        if (currentMembers.Count > 0)
                            group.UpdateHosts(group, currentMembers, false);
                        if (membersToAdd.Count > 0)
                            group.UpdateHosts(group, membersToAdd, true);
                        if (membersToRemove.Count > 0)
                            group.RemoveGroupHosts(group, membersToRemove);
                    }

                    gvRemove.DataSource = group.CurrentMembers(group.Name);
                    gvRemove.DataBind();

                    Master.Msgbox(Utility.Message);
                }
                else
                    Master.Msgbox("Boot Image Cannot Be Empty");
            }
            else
                Master.Msgbox("Kernel Cannot Be Empty");
        }
        else
            Master.Msgbox("Name Cannot Be Empty Or Contain Spaces");
    }

    private void ToggleCheckState(bool checkState, bool isAdd)
    {
        if (isAdd)
        {
            foreach (GridViewRow row in gvAdd.Rows)
            {
                CheckBox cb = (CheckBox)row.FindControl("chkSelector");
                if (cb != null)
                    cb.Checked = checkState;
            }
        }
        else
        {
            foreach (GridViewRow row in gvRemove.Rows)
            {
                CheckBox cb = (CheckBox)row.FindControl("chkSelector");
                if (cb != null)
                    cb.Checked = checkState;
            }
        }
    }

    protected void SelectAll_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox hcb;

        hcb = (CheckBox)gvRemove.HeaderRow.FindControl("chkSelectAll");

        if (hcb.Checked == true)
            ToggleCheckState(true, false);
        else
            ToggleCheckState(false, false);
    }

    protected void SelectAllAdd_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox hcb;

        hcb = (CheckBox)gvAdd.HeaderRow.FindControl("chkSelectAll");

        if (hcb.Checked == true)
            ToggleCheckState(true, true);
        else
            ToggleCheckState(false, true);
    }

    protected void txtSearchHosts_TextChanged(object sender, EventArgs e)
    {
        PopulateGrid();
    }

    protected void PopulateGrid()
    {
            Host host = new Host();
        if (Master.IsInMembership("User"))
            gvAdd.DataSource = host.TableForUser(txtSearchHosts.Text);
        else
            gvAdd.DataSource = host.Search(txtSearchHosts.Text);

        gvAdd.DataBind();

        lblTotal.Text = gvAdd.Rows.Count.ToString() + " Result(s) / " + host.GetTotalCount() + " Total Host(s)";
    }
    #endregion

    #region History Page

    protected void history_page()
    {
        if (!IsPostBack)
            ddlLimit.SelectedValue = "10";
        History history = new History();
        history.Type = "Group";
        history.TypeID = group.ID;
        gvHistory.DataSource = history.Read(history, ddlLimit.Text);
        gvHistory.DataBind();
        Master.Msgbox(Utility.Message);

    }

    protected void ddlLimit_SelectedIndexChanged(object sender, EventArgs e)
    {
        History history = new History();
        history.Type = "Group";
        history.TypeID = group.ID;
        gvHistory.DataSource = history.Read(history, ddlLimit.Text);
        gvHistory.DataBind();
        Master.Msgbox(Utility.Message);
    }

    #endregion

    #region Boot Menu Page

    protected void custombootmenu_page()
    {
        ddlTemplate.DataSource = Utility.PopulateBootMenusDdl();
        ddlTemplate.DataBind();
        ddlTemplate.Items.Insert(0, "select template");
        ddlTemplate.Items.Insert(1, "default");
    }

    protected void btnSetBootMenu_Click(object sender, EventArgs e)
    {
        foreach (string hostID in group.GetMemberIDs(group.Name))
        {
            Host host = new Host();
            host.ID = hostID;
            host = host.Read(host);
            host.SetCustomBootMenu(host, txtCustomBootMenu.Text);
        }

        History historyg = new History();
        historyg.Event = "Set Boot Menu";
        historyg.Type = "Group";
        historyg.TypeID = group.ID;
        historyg.CreateEvent(historyg);

        Master.Msgbox(Utility.Message);
    }

    protected void btnRemoveBootMenu_Click(object sender, EventArgs e)
    {
        foreach (string hostID in group.GetMemberIDs(group.Name))
        {
            Host host = new Host();
            host.ID = hostID;
            host = host.Read(host);
            host.RemoveCustomBootMenu(host);
        }

        History historyg = new History();
        historyg.Event = "Remove Boot Menu";
        historyg.Type = "Group";
        historyg.TypeID = group.ID;
        historyg.CreateEvent(historyg);

        Master.Msgbox(Utility.Message);
    }

    protected void ddlTemplate_SelectedIndexChanged(object sender, EventArgs e)
    {
        Utility settings = new Utility();
        string path = null;
        string mode = settings.GetSettings("PXE Mode");

        if (ddlTemplate.Text == "default")
        {
            if (mode.Contains("ipxe"))
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
            else
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";

            try
            {
                txtCustomBootMenu.Text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Master.Msgbox("Could Not Read Default Boot Menu.  Check The Exception Log For More Info");
                Logger.Log(ex.Message);
            }
        }
        else if (ddlTemplate.Text == "select template")
        {
            txtCustomBootMenu.Text = "";

        }
        else
        {
            BootTemplate template = new BootTemplate().Read(ddlTemplate.Text);
            txtCustomBootMenu.Text = template.templateContent;
        }

    }

    #endregion
}

