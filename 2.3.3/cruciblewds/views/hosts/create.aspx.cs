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

public partial class addhosts : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Master.IsInMembership("User"))
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied");

            ddlHostImage.DataSource = Utility.PopulateImagesDdl();
            ddlHostImage.DataBind();
            ddlHostImage.Items.Insert(0, "Select Image");

            ddlHostGroup.DataSource = Utility.PopulateGroupsDdl();
            ddlHostGroup.DataBind();
            ddlHostGroup.Items.Insert(0, "");

            ddlHostKernel.DataSource = Utility.GetKernels();
            ddlHostKernel.DataBind();
            ListItem itemHostKernel = ddlHostKernel.Items.FindByText("3.18.1-WDS");
            if (itemHostKernel != null)
                 ddlHostKernel.SelectedValue = "3.18.1-WDS";
            else
                 ddlHostKernel.Items.Insert(0, "Select Kernel");

            ddlHostBootImage.DataSource = Utility.GetBootImages();
            ddlHostBootImage.DataBind();
            ListItem itemBootImage = ddlHostBootImage.Items.FindByText("initrd.gz");
            if (itemBootImage != null)
                ddlHostBootImage.SelectedValue = "initrd.gz";
            else
                ddlHostBootImage.Items.Insert(0, "Select Boot Image");

            lbScripts.DataSource = Utility.GetScripts();
            lbScripts.DataBind();
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (Utility.NoSpaceNotEmpty(txtHostName.Text))
        {
            if (Utility.NoSpaceNotEmpty(txtHostMac.Text))
            {
                if (Utility.NoSpaceNotEmpty(ddlHostKernel.Text))
                {
                    if (Utility.NoSpaceNotEmpty(ddlHostBootImage.Text))
                    {
                        Host host = new Host();
                        host.Name = txtHostName.Text;
                        host.Mac = Utility.FixMac(txtHostMac.Text);
                        host.Image = ddlHostImage.Text;
                        host.Group = ddlHostGroup.Text;
                        host.Description = txtHostDesc.Text;
                        host.Kernel = ddlHostKernel.Text;
                        host.BootImage = ddlHostBootImage.Text;
                        host.Args = txtHostArguments.Text;
                        foreach (ListItem item in lbScripts.Items)
                            if (item.Selected == true)
                                host.Scripts += item.Value + ",";
                        host.Create(host);
                        if (Utility.Message.Contains("Successfully") && !createAnother.Checked)
                            Response.Redirect("~/views/hosts/view.aspx?page=edit&hostid=" + host.GetHostID(host.Mac));
                        else
                            Master.Msgbox(Utility.Message);
                    }
                    else
                        Master.Msgbox("Boot Image Cannot Be Empty");
                }
                else
                    Master.Msgbox("Kernel Cannot Be Empty");
            }
            else
                Master.Msgbox("MAC Address Cannot Be Empty Or Contain Spaces");
        }
        else
            Master.Msgbox("Name Cannot Be Empty Or Contain Spaces");
    }
}