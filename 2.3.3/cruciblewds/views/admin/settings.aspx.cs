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
using System.Security.Principal;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public partial class settings : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {

        if (!Master.IsInMembership("Administrator"))
            Response.Redirect("~/views/dashboard/dash.aspx?access=denied");

        
        if (!IsPostBack)
        {
            Settings settings = new Settings();
            Utility utility = new Utility();
            txtIP.Text = utility.GetSettings("Server IP",true);
            txtPort.Text = utility.GetSettings("Web Server Port", true);
            txtImagePath.Text = utility.GetSettings("Image Store Path", true);
            txtQSize.Text = utility.GetSettings("Queue Size", true);
            txtSenderArgs.Text = utility.GetSettings("Sender Args", true);
            txtNFSPath.Text = utility.GetSettings("Nfs Upload Path",true);
            txtTFTPPath.Text = utility.GetSettings("Tftp Path", true);
            txtWebService.Text = utility.GetSettings("Web Path", true);
            ddlPXEMode.SelectedValue = utility.GetSettings("PXE Mode", true);
            ddlProxyDHCP.SelectedValue = utility.GetSettings("Proxy Dhcp", true);
            ddlProxyBios.SelectedValue = utility.GetSettings("Proxy Bios File", true);
            ddlProxyEfi32.SelectedValue = utility.GetSettings("Proxy Efi32 File", true);
            ddlProxyEfi64.SelectedValue = utility.GetSettings("Proxy Efi64 File", true);
            ddlCompAlg.SelectedValue = utility.GetSettings("Compression Algorithm", true);
            ddlCompLevel.SelectedValue = utility.GetSettings("Compression Level", true);
            txtADLogin.Text = utility.GetSettings("AD Login Domain", true);
            ddlImageXfer.SelectedValue = utility.GetSettings("Image Transfer Mode", true);
            ddlImageChecksum.SelectedValue = utility.GetSettings("Image Checksum", true);
            ddlHostView.SelectedValue = utility.GetSettings("Default Host View", true);
            ddlOnd.SelectedValue = utility.GetSettings("On Demand", true);
            txtRecArgs.Text = utility.GetSettings("Receiver Args", true);
            txtStartPort.Text = utility.GetSettings("Udpcast Start Port", true);
            txtEndPort.Text = utility.GetSettings("Udpcast End Port", true);
            txtServerKey.Text = utility.GetSettings("Server Key", true);
            ddlServerKeyMode.SelectedValue = utility.GetSettings("Server Key Mode", true);
            txtImageHoldPath.Text = utility.GetSettings("Image Hold Path", true);
            txtNFSDeploy.Text = utility.GetSettings("Nfs Deploy Path", true);
            ddlSSL.SelectedValue = utility.GetSettings("Force SSL", true);
            txtSMBPath.Text = utility.GetSettings("SMB Path", true);
            txtSMBUser.Text = utility.GetSettings("SMB User Name", true);
            txtSMBPass.Text = utility.GetSettings("SMB Password", true);
            txtRecClientArgs.Text = utility.GetSettings("Client Receiver Args", true);
            txtGlobalHostArgs.Text = utility.GetSettings("Global Host Args", true);
            ViewState["startPort"] = txtStartPort.Text;
            ViewState["endPort"] = txtEndPort.Text;

            //These require pxe boot menu or client iso to be recreated
            ViewState["serverIP"] = txtIP.Text;
            ViewState["serverPort"] = txtPort.Text;
            ViewState["serverKey"] = txtServerKey.Text;
            ViewState["serverKeyMode"] = ddlServerKeyMode.Text;
            ViewState["webService"] = txtWebService.Text;
            ViewState["pxeMode"] = ddlPXEMode.Text;
            ViewState["forceSSL"] = ddlSSL.Text;
            ViewState["proxyDHCP"] = ddlProxyDHCP.SelectedValue;
            ViewState["proxyBios"]= ddlProxyBios.SelectedValue;
            ViewState["proxyEfi32"]=ddlProxyEfi32.SelectedValue;
            ViewState["proxyEfi64"]=ddlProxyEfi64.SelectedValue;

            ShowXferMode();
            ShowProxyMode();
        }
    }

    protected void btnGenerate_Click(object sender, EventArgs e)
    {
        txtServerKey.Text = Utility.GenerateKey();
    }

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        
        Settings settings = new Settings();
        List<string> names = new List<string>();
        List<string> values = new List<string>();
        string pxeMode = null;

        if (!ValidateSettings())
            Master.Msgbox(Utility.Message);
        else
        {
            names.Add("Server IP");
            names.Add("Web Server Port");
            names.Add("Image Store Path");
            names.Add("Queue Size");
            names.Add("Sender Args");
            names.Add("Nfs Upload Path");
            names.Add("Tftp Path");
            names.Add("Web Path");
            names.Add("PXE Mode");
            names.Add("Proxy Dhcp");
            names.Add("Proxy Bios File");
            names.Add("Proxy Efi32 File");
            names.Add("Proxy Efi64 File");
            names.Add("Compression Algorithm");
            names.Add("Compression Level");
            names.Add("AD Login Domain");
            names.Add("Image Transfer Mode");
            names.Add("Image Checksum");
            names.Add("Default Host View");
            names.Add("On Demand");
            names.Add("Receiver Args");
            names.Add("Udpcast Start Port");
            names.Add("Udpcast End Port");
            names.Add("Server Key");
            names.Add("Server Key Mode");
            names.Add("Image Hold Path");
            names.Add("Nfs Deploy Path");
            names.Add("Force SSL");
            names.Add("SMB Path");
            names.Add("SMB User Name");
            if(!string.IsNullOrEmpty(txtSMBPass.Text))
               names.Add("SMB Password");
            names.Add("Client Receiver Args");
            names.Add("Global Host Args");
            

            values.Add(txtIP.Text);
            values.Add(txtPort.Text);
            values.Add(txtImagePath.Text);
            values.Add(txtQSize.Text);
            values.Add(txtSenderArgs.Text);
            values.Add(txtNFSPath.Text);
            values.Add(txtTFTPPath.Text);
            values.Add(txtWebService.Text);
            values.Add(ddlPXEMode.Text);
            values.Add(ddlProxyDHCP.Text);
            values.Add(ddlProxyBios.Text);
            values.Add(ddlProxyEfi32.Text);
            values.Add(ddlProxyEfi64.Text);
            values.Add(ddlCompAlg.Text);
            values.Add(ddlCompLevel.Text);
            values.Add(txtADLogin.Text);
            values.Add(ddlImageXfer.Text);
            values.Add(ddlImageChecksum.Text);
            values.Add(ddlHostView.Text);
            pxeMode = ddlPXEMode.Text;
            values.Add(ddlOnd.Text);
            values.Add(txtRecArgs.Text);
            values.Add(txtStartPort.Text);
            values.Add(txtEndPort.Text);
            values.Add(txtServerKey.Text);
            values.Add(ddlServerKeyMode.Text);
            values.Add(txtImageHoldPath.Text);
            values.Add(txtNFSDeploy.Text);
            values.Add(ddlSSL.Text);
            values.Add(txtSMBPath.Text); 
            values.Add(txtSMBUser.Text);
            if (!string.IsNullOrEmpty(txtSMBPass.Text))
               values.Add(txtSMBPass.Text); 
            values.Add(txtRecClientArgs.Text); 
            values.Add(txtGlobalHostArgs.Text);
            settings.Update(names, values);

            bool newBootMenu = false;
            bool newClientISO = false;
            if (Utility.Message.Contains("Successfully"))
            {

                if ((string)ViewState["proxyDHCP"] != ddlProxyDHCP.Text)
                    newBootMenu = true;
                if ((string)ViewState["proxyBios"] != ddlProxyBios.Text)
                    newBootMenu = true;
                if ((string)ViewState["proxyEfi32"] != ddlProxyEfi32.Text)
                    newBootMenu = true;
                if ((string)ViewState["proxyEfi64"] != ddlProxyEfi64.Text)
                    newBootMenu = true;
                if ((string)ViewState["serverIP"] != txtIP.Text)
                {
                    newBootMenu = true;
                    newClientISO = true;
                }
                if ((string)ViewState["serverPort"] != txtPort.Text)
                {
                    newBootMenu = true;
                    newClientISO = true;
                }
                if ((string)ViewState["serverKey"] != txtServerKey.Text)
                {
                    newBootMenu = true;
                    newClientISO = true;
                }
                if ((string)ViewState["serverKeyMode"] != ddlServerKeyMode.Text)
                {
                    newBootMenu = true;
                }
                if ((string)ViewState["webService"] != txtWebService.Text)
                {
                    newBootMenu = true;
                    newClientISO = true;
                }
                if ((string)ViewState["pxeMode"] != ddlPXEMode.Text)
                {
                    newBootMenu = true;
                }

                if ((string)ViewState["forceSSL"] != ddlSSL.Text)
                {
                    newBootMenu = true;
                    newClientISO = true;
                }
                if ((string)(ViewState["startPort"]) != txtStartPort.Text)
                {
                    int startPort = Convert.ToInt32(txtStartPort.Text);
                    startPort = startPort - 2;
                    settings.UpdateLastPort(startPort);
                }
            }
            settings.PXEmode(pxeMode);
            if (!newBootMenu && !newClientISO)
                Master.Msgbox(Utility.Message);
            else
            {
                lblTitle.Text = Utility.Message;
                lblTitle.Text += "<br> Your Settings Changes Require A New PXE Boot File Be Created.  <br>Create It Now?";
                if (newClientISO)
                    lblClientISO.Text = "If You Are Using The Client ISO, It Must Also Be Manually Updated.";
                ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
                Session.Remove("Message");
            }

            ViewState["serverKey"] = "";
        }
    }

    protected void OkButton_Click(object sender, EventArgs e)
    {
         Response.Redirect("~/views/admin/bootmenu.aspx?defaultmenu=true");
    }
    protected void ImageXfer_Changed(object sender, EventArgs e)
    {
         ShowXferMode();
    }

    protected void ProxyDhcp_Changed(object sender, EventArgs e)
    {
        ShowProxyMode();
    }
    protected bool ValidateSettings()
    {
        /*if(txtIP.Text.Contains(":"))
        {
            Utility.Message="The CrucibleWDS Server Port Cannot Be Changed";
            return false;
        }*/

        Task task = new Task();
        var isActiveTasks = task.ReadActive();
        if (isActiveTasks.Rows.Count > 0)
        {
            Utility.Message = "Settings Cannot Be Changed While Tasks Are Active";
            return false;
        }
        if (txtPort.Text != "80" && txtPort.Text != "443" && !string.IsNullOrEmpty(txtPort.Text))
        {
            txtWebService.Text = "http://[server-ip]:" + txtPort.Text + "/cruciblewds/service/client.asmx/";
        }
        if (txtPort.Text == "80" || txtPort.Text == "443" || string.IsNullOrEmpty(txtPort.Text))
        {
            txtWebService.Text = "http://[server-ip]/cruciblewds/service/client.asmx/";
        }
        if (ddlSSL.Text == "Yes")
        {
            if (txtWebService.Text.ToLower().Contains("http://"))
                txtWebService.Text = txtWebService.Text.Replace("http://", "https://");
        }
        else
            if (txtWebService.Text.ToLower().Contains("https://"))
                txtWebService.Text = txtWebService.Text.Replace("https://", "http://");
            

        if (!txtImagePath.Text.Trim().EndsWith(Path.DirectorySeparatorChar.ToString()))
            txtImagePath.Text += Path.DirectorySeparatorChar;

        if (!txtImageHoldPath.Text.Trim().EndsWith(Path.DirectorySeparatorChar.ToString()))
            txtImageHoldPath.Text += Path.DirectorySeparatorChar;

        if (!txtTFTPPath.Text.Trim().EndsWith(Path.DirectorySeparatorChar.ToString()))
            txtTFTPPath.Text += Path.DirectorySeparatorChar;

        if (!txtNFSPath.Text.Trim().EndsWith("/"))
            txtNFSPath.Text += "/";

        if (!txtNFSDeploy.Text.Trim().EndsWith("/"))
            txtNFSDeploy.Text += ("/");

        if (!txtWebService.Text.Trim().EndsWith("/"))
            txtWebService.Text += ("/");

        if (txtSMBPath.Text.Contains("\\"))
             txtSMBPath.Text = txtSMBPath.Text.Replace("\\","/");

        int startPort = Convert.ToInt32(txtStartPort.Text);
        int endPort = Convert.ToInt32(txtEndPort.Text);

        if (startPort % 2 != 0)
        {
            startPort++;
            txtStartPort.Text = startPort.ToString();
        }
        if (endPort % 2 != 0)
        {
            endPort++;
            txtEndPort.Text = endPort.ToString();
        }

        try
        {
            if ((startPort >= 2) && (endPort - startPort >= 2))
            {
                return true;
            }
            else
            {
                Utility.Message = "End Port Must Be At Least 2 More Than Starting Port";
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            return false;
        }


    }
    protected void ShowProxyMode()
    {
        ddlProxyBios.BackColor = System.Drawing.Color.White;
        ddlProxyEfi32.BackColor = System.Drawing.Color.White;
        ddlProxyEfi64.BackColor = System.Drawing.Color.White;
        ddlProxyBios.Font.Strikeout = false;
        ddlProxyEfi32.Font.Strikeout = false;
        ddlProxyEfi64.Font.Strikeout = false;
        ddlPXEMode.BackColor = System.Drawing.Color.White;
        ddlPXEMode.Font.Strikeout = false;
        if (ddlProxyDHCP.Text == "No")
        {
            ddlProxyBios.BackColor = System.Drawing.Color.LightGray;
            ddlProxyEfi32.BackColor = System.Drawing.Color.LightGray;
            ddlProxyEfi64.BackColor = System.Drawing.Color.LightGray;
            ddlProxyBios.Font.Strikeout = true;
            ddlProxyEfi32.Font.Strikeout = true;
            ddlProxyEfi64.Font.Strikeout = true;
        }
        else
        {
            ddlPXEMode.BackColor = System.Drawing.Color.LightGray;
            ddlPXEMode.Font.Strikeout = true;
        }
    }

    protected void ShowXferMode()
    {
         txtNFSDeploy.BackColor = System.Drawing.Color.White;
         txtNFSDeploy.Font.Strikeout = false;
         txtNFSPath.BackColor = System.Drawing.Color.White;
         txtNFSPath.Font.Strikeout = false;
        
         txtSMBPath.BackColor = System.Drawing.Color.White;
         txtSMBPath.Font.Strikeout = false;
         txtSMBUser.BackColor = System.Drawing.Color.White;
         txtSMBUser.Font.Strikeout = false;
         txtSMBPass.BackColor = System.Drawing.Color.White;
         txtSMBPass.Font.Strikeout = false;

         if (ddlImageXfer.Text == "smb" || ddlImageXfer.Text == "smb+http")
         {
              txtNFSDeploy.BackColor = System.Drawing.Color.LightGray;
              txtNFSDeploy.Font.Strikeout = true;
              txtNFSPath.BackColor = System.Drawing.Color.LightGray;
              txtNFSPath.Font.Strikeout = true;
              
         }

         if (ddlImageXfer.Text != "smb" && ddlImageXfer.Text != "smb+http")
         {
              txtSMBPath.BackColor = System.Drawing.Color.LightGray;
              txtSMBPath.Font.Strikeout = true;
              txtSMBUser.BackColor = System.Drawing.Color.LightGray;
              txtSMBUser.Font.Strikeout = true;
              txtSMBPass.BackColor = System.Drawing.Color.LightGray;
              txtSMBPass.Font.Strikeout = true;
         }

         if (ddlImageXfer.Text == "nfs+http")
         {
              txtNFSDeploy.BackColor = System.Drawing.Color.LightGray;
              txtNFSDeploy.Font.Strikeout = true;
         }

         if (ddlImageXfer.Text == "udp+http")
         {
              txtNFSDeploy.BackColor = System.Drawing.Color.LightGray;
              txtNFSDeploy.Font.Strikeout = true;
              txtNFSPath.BackColor = System.Drawing.Color.LightGray;
              txtNFSPath.Font.Strikeout = true;
           
              txtSMBPath.BackColor = System.Drawing.Color.LightGray;
              txtSMBPath.Font.Strikeout = true;
              txtSMBUser.BackColor = System.Drawing.Color.LightGray;
              txtSMBUser.Font.Strikeout = true;
              txtSMBPass.BackColor = System.Drawing.Color.LightGray;
              txtSMBPass.Font.Strikeout = true;
         }
    }
}