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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Npgsql;
using System.Data;
using System.Configuration;

public partial class addgroups : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
       
        if (!IsPostBack)
        {
            if (Master.IsInMembership("User"))
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied");

            ddlGroupImage.DataSource = Utility.PopulateImagesDdl();
            ddlGroupImage.DataBind();
            ddlGroupImage.Items.Insert(0, "Select Image");

            ddlGroupKernel.DataSource = Utility.GetKernels();
            ddlGroupKernel.DataBind();
            ListItem itemKernel = ddlGroupKernel.Items.FindByText("3.18.1-WDS");
            if (itemKernel != null)
                 ddlGroupKernel.SelectedValue = "3.18.1-WDS";
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

            Utility utility = new Utility();
            if (utility.GetSettings("Default Host View") == "all")
                PopulateGrid();
        }          
    }

    protected void Submit_Click(object sender, EventArgs e)
    {
        if (Utility.NoSpaceNotEmpty(txtGroupName.Text))
        {
            if (Utility.NoSpaceNotEmpty(ddlGroupKernel.Text))
            {
                if (Utility.NoSpaceNotEmpty(ddlGroupBootImage.Text))
                {
                    List<int> members = new List<int>();
                    Group group = new Group();

                    foreach (GridViewRow row in gvHosts.Rows)
                    {
                        CheckBox cb = (CheckBox)row.FindControl("chkSelector");
                        if (cb != null && cb.Checked)
                            members.Add(Convert.ToInt32(gvHosts.DataKeys[row.RowIndex].Value));
                    }

                    group.Name = txtGroupName.Text;
                    group.Description = txtGroupDesc.Text;
                    group.Image = ddlGroupImage.Text;
                    group.Kernel = ddlGroupKernel.Text;
                    group.BootImage = ddlGroupBootImage.Text;
                    group.Args = txtGroupArguments.Text;
                    group.SenderArgs = txtGroupSenderArgs.Text;
                    foreach (ListItem item in lbScripts.Items)
                        if (item.Selected == true)
                            group.Scripts += item.Value + ",";
                    group.Create(group, members);

                    if (Utility.Message.Contains("Successfully"))
                        Response.Redirect("~/views/groups/view.aspx?page=edit&groupid=" + group.GetGroupID(group.Name));

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

    protected void SelectAll_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox hcb = (CheckBox)gvHosts.HeaderRow.FindControl("chkSelectAll");
        if (hcb.Checked == true)
            ToggleCheckState(true);
        else
            ToggleCheckState(false);
    }

    private void ToggleCheckState(bool checkState)
    {
        foreach (GridViewRow row in gvHosts.Rows)
            {
                CheckBox cb = (CheckBox)row.FindControl("chkSelector");
                if (cb != null)
                    cb.Checked = checkState;
            }
    }

    protected void txtSearchHosts_TextChanged(object sender, EventArgs e)
    {
        PopulateGrid();
       
        
    }

    protected void PopulateGrid()
    {
        Host host = new Host();
        gvHosts.DataSource = host.Search(txtSearchHosts.Text);
        gvHosts.DataBind();
        lblTotal.Text = gvHosts.Rows.Count.ToString() + " Result(s) / " + host.GetTotalCount() + " Total Host(s)"; 
    }
}







