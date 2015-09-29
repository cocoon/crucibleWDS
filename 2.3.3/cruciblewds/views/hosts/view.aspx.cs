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
using System.IO;
using Mono.Unix.Native;
using System.Text;

public partial class modifyhosts : System.Web.UI.Page
{
    public string requestedPage { get; set; }
    public Host host { get; set; }

    #region View Page

    protected void Page_Load(object sender, EventArgs e)
    {
       

        host = new Host();
        requestedPage = Request.QueryString["page"] as string;
        host.ID = Request["hostid"] as string;
        host.Read(host);

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
                    history.Visible = true;
                    history_page();
                    break;
                case "activebootmenu":
                    lblSubNav.Text = "| boot menu";
                    activebootmenu.Visible = true;
                    activebootmenu_page();
                    break;
                case "custombootmenu":
                    lblSubNav.Text = "| boot menu";
                    custombootmenu.Visible = true;
                    custombootmenu_page();
                    break;
                case "log":
                    lblSubNav.Text = "| log";
                    logs.Visible = true;
                    logs_page();
                    break;
                default:
                    break;
            }
        }
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        lblTitle.Text = "Delete This Host?";
        Session["direction"] = "delete";
        gvConfirm.DataBind(); // clear gridview if deploy or upload was clicked first
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
    }
    protected void btnDeploy_Click(object sender, EventArgs e)
    {
        Unicast unicast = new Unicast();
        Session["direction"] = "push";
        lblTitle.Text = "Deploy The Selected Host?";
        gvConfirm.DataSource = unicast.Confirm(host.ID);
        gvConfirm.DataBind();
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
    }

    protected void btnUpload_Click(object sender, EventArgs e)
    {
        Unicast unicast = new Unicast();
        Session["direction"] = "pull";
        lblTitle.Text = "Upload The Selected Host?";
        gvConfirm.DataSource = unicast.Confirm(host.ID);
        gvConfirm.DataBind();
        ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });;", true);

    }

    protected void OkButton_Click(object sender, EventArgs e)
    {
        string direction = (string)(Session["direction"]);
        Session.Remove("direction");
        if (direction == "delete")
        {
 
            List<int> delList = new List<int>();
            delList.Add(Convert.ToInt32(host.ID));
            host.Delete(delList);
            if (Utility.Message.Contains("Successfully"))
                Response.Redirect("~/views/hosts/search.aspx");
            else
                Master.Msgbox(Utility.Message);
        }
        else if (direction == "push")
        {
            Unicast unicast = new Unicast();
            Image image = new Image();
            unicast = unicast.Read(Convert.ToInt32(host.ID), unicast);
            string imageID = image.GetImageID(unicast.ImageName);
            Session["imageID"] = imageID;
            if (image.Check_Checksum(imageID))
            {
                unicast.CreateUnicast(direction, Convert.ToInt32(host.ID));
                Master.Msgbox(Utility.Message);
            }
            else
            {
                lblIncorrectChecksum.Text = "This Image Has Not Been Confirmed And Cannot Be Deployed.  <br>Confirm It Now?";
                ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('incorrectChecksum'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);

            }


        }
        else
        {
            Unicast unicast = new Unicast();
            unicast.CreateUnicast(direction, Convert.ToInt32(host.ID));
            Master.Msgbox(Utility.Message);
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
        ddlHostImage.DataSource = Utility.PopulateImagesDdl();
        ddlHostImage.DataBind();
        ddlHostImage.Items.Insert(0, "Select Image");

        ddlHostGroup.DataSource = Utility.PopulateGroupsDdl();
        ddlHostGroup.DataBind();
        ddlHostGroup.Items.Insert(0, "");

        lbScripts.DataSource = Utility.GetScripts();
        lbScripts.DataBind();

       

        if (Master.IsInMembership("User"))
        {
            WDSUser user = new WDSUser();
            user.ID = user.GetID(HttpContext.Current.User.Identity.Name);
            user = user.Read(user);
            List<string> listManagementGroups = user.GroupManagement.Split(' ').ToList<string>();

            List<string> allowedGroups = new List<string>();

            foreach (string id in listManagementGroups)
            {
                Group mgmtgroup = new Group();
                mgmtgroup.ID = id;
                mgmtgroup = mgmtgroup.Read(mgmtgroup);

                foreach (ListItem item in ddlHostGroup.Items)
                {
                    if (item.Value == mgmtgroup.Name)
                        allowedGroups.Add(mgmtgroup.Name);
                }
            }

            bool isAuthorized = false;
            foreach (string aGroup in allowedGroups)
            {
                if (host.Group == aGroup)
                {
                    isAuthorized = true;
                    break;
                }
            }

            if (!isAuthorized)
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied");

            ddlHostGroup.DataSource = allowedGroups;
            ddlHostGroup.DataBind();
            ddlHostGroup.Items.Insert(0, "");
        }

        ddlHostKernel.DataSource = Utility.GetKernels();
        ddlHostKernel.DataBind();
        ddlHostKernel.Items.Insert(0, "Select Kernel");

        ddlHostBootImage.DataSource = Utility.GetBootImages();
        ddlHostBootImage.DataBind();
        ddlHostBootImage.Items.Insert(0, "Select Boot Image");

        txtHostName.Text = host.Name;
        txtHostMac.Text = host.Mac;
        ddlHostImage.Text = host.Image;
        ddlHostGroup.Text = host.Group;
        txtHostDesc.Text = host.Description;
        ddlHostKernel.Text = host.Kernel;
        ddlHostBootImage.Text = host.BootImage;
        txtHostArguments.Text = host.Args;

        if (!string.IsNullOrEmpty(host.Scripts))
        {
            List<string> listhostScripts = host.Scripts.Split(',').ToList<string>();
            foreach (ListItem item in lbScripts.Items)
                foreach (var script in listhostScripts)
                    if (item.Value == script)
                        item.Selected = true;
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
                        host.Name = txtHostName.Text;
                        host.Mac = Utility.FixMac(txtHostMac.Text);
                        host.Image = ddlHostImage.Text;
                        host.Group = ddlHostGroup.Text;
                        host.Description = txtHostDesc.Text;
                        host.Kernel = ddlHostKernel.Text;
                        host.BootImage = ddlHostBootImage.Text;
                        host.Args = txtHostArguments.Text;

                        host.Scripts = null;
                        foreach (ListItem item in lbScripts.Items)
                            if (item.Selected == true)
                                host.Scripts += item.Value + ",";
                        
                        host.Update(host);

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

    #endregion

    #region History Page
    protected void history_page()
    {
        if (!IsPostBack)
            ddlLimit.SelectedValue = "10";
        History history = new History();
        history.Type = "Host";
        history.TypeID = host.ID;
        gvHistory.DataSource = history.Read(history, ddlLimit.Text);
        gvHistory.DataBind();
        Master.Msgbox(Utility.Message);

    }

    protected void ddlLimit_SelectedIndexChanged(object sender, EventArgs e)
    {
        History history = new History();
        history.Type = "Host";
        history.TypeID = host.ID;
        gvHistory.DataSource = history.Read(history, ddlLimit.Text);
        gvHistory.DataBind();
        Master.Msgbox(Utility.Message);
    }

    #endregion

    #region Boot Menu Page

    protected void activebootmenu_page()
    {
        Utility settings = new Utility();
        Task task = new Task();
        string mode = settings.GetSettings("PXE Mode");
        string pxeHostMac = task.MacToPXE(host.Mac);
        string isActive = host.CheckActive(host.Mac);
        string path = null;
        string proxyDHCP = settings.GetSettings("Proxy Dhcp");

        try
        {
            if (isActive == "Active")
            {
                if (proxyDHCP == "Yes")
                {
                    divProxy.Visible = true;
                    string biosFile = settings.GetSettings("Proxy Bios File");
                    string efi32File = settings.GetSettings("Proxy Efi32 File");
                    string efi64File = settings.GetSettings("Proxy Efi64 File");

                    if (ddlEditProxyType.Text == "bios")
                    {
                        if (biosFile.Contains("ipxe"))
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                        else
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
                    }

                    if (ddlEditProxyType.Text == "efi32")
                    {
                        if (efi32File.Contains("ipxe"))
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                        else
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
                    }

                    if (ddlEditProxyType.Text == "efi64")
                    {
                        if (efi64File.Contains("ipxe"))
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                        else
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
                    }

                    lblFileName1.Text = path;
                    txtBootMenu.Text = File.ReadAllText(path);

                }
                else
                {
                    if (mode.Contains("ipxe"))
                    {
                        path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                       
                    }
                    else
                    {
                        path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
                    }

                    lblFileName1.Text = path;
                    txtBootMenu.Text = File.ReadAllText(path);

                }
                lblActiveBoot.Text = "Active Task Found <br> Displaying Task Boot Menu";

            }

            else if (host.IsCustomBootEnabled(host.Mac))
            {
                if (proxyDHCP == "Yes")
                {
                    divProxy.Visible = true;
                    string biosFile = settings.GetSettings("Proxy Bios File");
                    string efi32File = settings.GetSettings("Proxy Efi32 File");
                    string efi64File = settings.GetSettings("Proxy Efi64 File");

                    if (ddlEditProxyType.Text == "bios")
                    {
                        if (biosFile.Contains("ipxe"))
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                        else
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
                    }

                    if (ddlEditProxyType.Text == "efi32")
                    {
                        if (efi32File.Contains("ipxe"))
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                        else
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
                    }

                    if (ddlEditProxyType.Text == "efi64")
                    {
                        if (efi64File.Contains("ipxe"))
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                        else
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;
                    }

                    lblFileName1.Text = path;
                    txtBootMenu.Text = File.ReadAllText(path);


                }
                else
                {
                    if (mode.Contains("ipxe"))
                    {
                        path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac + ".ipxe";
                       
                    }
                    else
                    {
                        path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + pxeHostMac;

                    }
                    lblFileName1.Text = path;
                    txtBootMenu.Text = File.ReadAllText(path);
                }
                lblActiveBoot.Text = "No Active Task Found <br> Custom Boot Menu Found <br> Displaying Custom Boot Menu";
            }

            else //Not Active, display default global boot menu
            {
                if (proxyDHCP == "Yes")
                {
                    divProxy.Visible = true;
                    string biosFile = settings.GetSettings("Proxy Bios File");
                    string efi32File = settings.GetSettings("Proxy Efi32 File");
                    string efi64File = settings.GetSettings("Proxy Efi64 File");

                    if (ddlEditProxyType.Text == "bios")
                    {
                        if (biosFile.Contains("ipxe"))
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
                        else
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
                    }

                    if (ddlEditProxyType.Text == "efi32")
                    {
                        if (efi32File.Contains("ipxe"))
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
                        else
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
                    }

                    if (ddlEditProxyType.Text == "efi64")
                    {
                        if (efi64File.Contains("ipxe"))
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
                        else
                            path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
                    }

                    lblFileName1.Text = path;
                    txtBootMenu.Text = File.ReadAllText(path);

                }
                else
                {
                    if (mode.Contains("ipxe"))
                    {
                        path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
                    

                    }
                    else
                    {
                        path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";

                    }
                    lblFileName1.Text = path;
                    txtBootMenu.Text = File.ReadAllText(path);
                }

                lblActiveBoot.Text = "No Active Task Found <br> No Custom Boot Menu Found <br> Displaying Global Default Boot Menu";
            }
        }
        catch (Exception ex)
        {
            Master.Msgbox("Could Not Read Active Boot Menu.  Check The Exception Log For More Info");
            Logger.Log(ex.Message);
        }

    }

    protected void EditProxy_Changed(object sender, EventArgs e)
    {
        activebootmenu_page();
    }
    protected void custombootmenu_page()
    {
        ddlTemplate.DataSource = Utility.PopulateBootMenusDdl();
        ddlTemplate.DataBind();
        ddlTemplate.Items.Insert(0, "select template");
        ddlTemplate.Items.Insert(1, "default");
    }

    protected void btnSetBootMenu_Click(object sender, EventArgs e)
    {
        host.SetCustomBootMenu(host, txtCustomBootMenu.Text);
        Master.Msgbox(Utility.Message);
    }

    protected void btnRemoveBootMenu_Click(object sender, EventArgs e)
    {
        host.RemoveCustomBootMenu(host);
        Master.Msgbox(Utility.Message);
    }

    protected void ddlTemplate_SelectedIndexChanged(object sender, EventArgs e)
    {
        Utility settings = new Utility();
        string path = null;
        string mode = settings.GetSettings("PXE Mode");
        string proxyDHCP = settings.GetSettings("Proxy Dhcp");
        if (ddlTemplate.Text == "default")
        {
            if (proxyDHCP == "Yes")
            {
                string biosFile = settings.GetSettings("Proxy Bios File");

                if (biosFile.Contains("ipxe"))
                    path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
                else
                    path = settings.GetSettings("Tftp Path") + "proxy" + Path.DirectorySeparatorChar + ddlEditProxyType.Text + Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";

            }
            else
            {
                if (mode.Contains("ipxe"))
                    path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
                else
                    path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
            }
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

    #region Log Page
    protected void logs_page()
    {
        if (!IsPostBack)
            ddlLogLimit.SelectedValue = "All";

    }

    protected void btnExportLog_Click(object sender, EventArgs e)
    {
        string logType = null;
        if (ddlLogType.Text == "Upload")
            logType = ".upload";
        else if (ddlLogType.Text == "Deploy")
            logType = ".download";
        else
            return;
        try
        {
            string hostLogPath = "hosts" + Path.DirectorySeparatorChar + host.ID + logType;
            string logPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar;
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment; filename=" + host.ID + logType);
            HttpContext.Current.Response.TransmitFile(logPath + hostLogPath);
            HttpContext.Current.Response.End();
        }
        catch { }

    }
    protected void ddlLogType_SelectedIndexChanged(object sender, EventArgs e)
    {
        string logType = null;
        if (ddlLogType.Text != "Select A Log")
        {
            if (ddlLogType.Text == "Upload")
                logType = ".upload";
            else
                logType = ".download";

                gvHostLog.DataSource = Logger.ViewLog("hosts" + Path.DirectorySeparatorChar + host.ID + logType, ddlLogLimit.Text);
                gvHostLog.DataBind();
                Master.Msgbox(Utility.Message);

        }
    }

    protected void ddlLogLimit_SelectedIndexChanged(object sender, EventArgs e)
    {
        string logType = null;
        if (ddlLogType.Text != "Select A Log")
        {
            if (ddlLogType.Text == "Upload")
                logType = ".upload";
            else
                logType = ".download";

                gvHostLog.DataSource = Logger.ViewLog("hosts" + Path.DirectorySeparatorChar + host.ID + logType, ddlLogLimit.Text);
                gvHostLog.DataBind();
                Master.Msgbox(Utility.Message);

        }
    }
    #endregion
}