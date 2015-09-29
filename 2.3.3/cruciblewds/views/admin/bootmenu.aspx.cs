using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using Mono.Unix.Native;

public partial class views_admin_bootmenu : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Master.IsInMembership("Administrator"))
            Response.Redirect("~/views/dashboard/dash.aspx?access=denied");

        string opendefault = Request.QueryString["defaultmenu"] as string;

        if (!IsPostBack)
        {
            if (opendefault == "true")
            {
                bootEditor.Visible = false;
                bootTemplates.Visible = true;

                Utility utility = new Utility();
              
                createNewTemplate.Visible = false;
                modifyTemplate.Visible = false;

                string proxyDHCP = utility.GetSettings("Proxy Dhcp");

                if (proxyDHCP == "Yes")
                {
                    divProxyDHCP.Visible = true;
                    string biosFile = utility.GetSettings("Proxy Bios File");
                    string efi32File = utility.GetSettings("Proxy Efi32 File");
                    string efi64File = utility.GetSettings("Proxy Efi64 File");
                    if (biosFile.Contains("linux") || efi32File.Contains("linux") || efi64File.Contains("linux"))
                    {
                        proxyPassBoxes.Visible = true;
                    }

                    try
                    {
                        ddlBiosKernel.DataSource = Utility.GetKernels();
                        ddlBiosKernel.DataBind();
                        ddlEfi32Kernel.DataSource = Utility.GetKernels();
                        ddlEfi32Kernel.DataBind();
                        ddlEfi64Kernel.DataSource = Utility.GetKernels();
                        ddlEfi64Kernel.DataBind();

                        ListItem itemKernel = ddlBiosKernel.Items.FindByText("3.18.1-WDS");
                        if (itemKernel != null)
                        {
                            ddlBiosKernel.SelectedValue = "3.18.1-WDS";
                            ddlEfi32Kernel.SelectedValue = "3.18.1-WDS";
                        }
                        else
                        {
                            ddlBiosKernel.Items.Insert(0, "Select Kernel");
                            ddlEfi32Kernel.Items.Insert(0, "Select Kernel");
                        }

                        ListItem itemKernel64 = ddlEfi64Kernel.Items.FindByText("3.18.1x64-WDS");
                        if (itemKernel64 != null)
                            ddlEfi64Kernel.SelectedValue = "3.18.1x64-WDS";
                        else
                            ddlEfi64Kernel.Items.Insert(0, "Select Kernel");


                        ddlBiosBootImage.DataSource = Utility.GetBootImages();
                        ddlBiosBootImage.DataBind();
                        ddlEfi32BootImage.DataSource = Utility.GetBootImages();
                        ddlEfi32BootImage.DataBind();
                        ddlEfi64BootImage.DataSource = Utility.GetBootImages();
                        ddlEfi64BootImage.DataBind();

                        ListItem itemBootImage = ddlBiosBootImage.Items.FindByText("initrd.gz");
                        if (itemBootImage != null)
                        {
                            ddlBiosBootImage.SelectedValue = "initrd.gz";
                            ddlEfi32BootImage.SelectedValue = "initrd.gz";
                        }
                        else
                        {
                            ddlBiosBootImage.Items.Insert(0, "Select Boot Image");
                            ddlEfi32BootImage.Items.Insert(0, "Select Boot Image");
                        }

                        ListItem itemBootImage64 = ddlEfi64BootImage.Items.FindByText("initrd64.gz");
                        if (itemBootImage != null)
                            ddlEfi64BootImage.SelectedValue = "initrd64.gz";
                        else
                            ddlEfi64BootImage.Items.Insert(0, "Select Boot Image");
                    }
                    catch { }
                }
                else
                {
                    bootPasswords.Visible = true;
                    divPXEMode.Visible = true;
                    string pxeMode = utility.GetSettings("PXE Mode");
                    if (pxeMode.Contains("ipxe"))
                    {
                        passboxes.Visible = false;
                        btnSubmitDefault.Visible = false;
                        btnSubmitIPXE.Visible = true;
                    }
                    try
                    {
                        ddlHostKernel.DataSource = Utility.GetKernels();
                        ddlHostKernel.DataBind();
                        ListItem itemKernel = ddlHostKernel.Items.FindByText("3.18.1-WDS");
                        if (itemKernel != null)
                            ddlHostKernel.SelectedValue = "3.18.1-WDS";
                        else
                            ddlHostKernel.Items.Insert(0, "Select Kernel");
                    }
                    catch { }
                    try
                    {
                        ddlHostBootImage.DataSource = Utility.GetBootImages();
                        ddlHostBootImage.DataBind();
                        ListItem itemBootImage = ddlHostBootImage.Items.FindByText("initrd.gz");
                        if (itemBootImage != null)
                            ddlHostBootImage.SelectedValue = "initrd.gz";
                        else
                            ddlHostBootImage.Items.Insert(0, "Select Boot Image");
                    }
                    catch { }
                }
            }
        }
    }
 
    protected void showEditor_Click(object sender, EventArgs e)
    {
        Utility settings = new Utility();
        bootEditor.Visible = true;
        bootTemplates.Visible = false;
        string path = null;
        string mode = settings.GetSettings("PXE Mode");
        string proxyDHCP = settings.GetSettings("Proxy Dhcp");

        if (proxyDHCP == "Yes")
        {
            string biosFile = settings.GetSettings("Proxy Bios File");
            string efi32File = settings.GetSettings("Proxy Efi32 File");
            string efi64File = settings.GetSettings("Proxy Efi64 File");
            proxyEditor.Visible = true;
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



        }
        else
        {
            proxyEditor.Visible = false;

            if (mode.Contains("ipxe"))
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
            else
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
        }
        lblFileName1.Text = path;
        try
        {
             txtBootMenu.Text = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
             Logger.Log(ex.Message);
             path = path.Replace(@"\",@"\\");
             Master.Msgbox("Could Not Find " + path);
        }

    }

    protected void saveEditor_Click(object sender, EventArgs e)
    {
        Utility settings = new Utility();
        string path = null;
        string mode = settings.GetSettings("PXE Mode");
        string proxyDHCP = settings.GetSettings("Proxy Dhcp");

        if (proxyDHCP == "Yes")
        {
            string biosFile = settings.GetSettings("Proxy Bios File");
            string efi32File = settings.GetSettings("Proxy Efi32 File");
            string efi64File = settings.GetSettings("Proxy Efi64 File");
            proxyEditor.Visible = true;
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
            using (StreamWriter file = new StreamWriter(path))
            {
                file.WriteLine(txtBootMenu.Text);
                file.Close();
            }
            if (Environment.OSVersion.ToString().Contains("Unix"))
            {
                Syscall.chmod(path, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
            }
            Master.Msgbox("Successfully Updated Default Global Boot Menu");
        }

        catch (Exception ex)
        {
            Master.Msgbox("Could Not Update Default Global Boot Menu.  Check The Exception Log For More Info.");
            Logger.Log(ex.Message);
        }
    }

    protected void EditProxy_Changed(object sender, EventArgs e)
    {
        Utility settings = new Utility();
        string path = null;
        string mode = settings.GetSettings("PXE Mode");
        string proxyDHCP = settings.GetSettings("Proxy Dhcp");

        if (proxyDHCP == "Yes")
        {
            string biosFile = settings.GetSettings("Proxy Bios File");
            string efi32File = settings.GetSettings("Proxy Efi32 File");
            string efi64File = settings.GetSettings("Proxy Efi64 File");
            proxyEditor.Visible = true;
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



        }
        else
        {
            proxyEditor.Visible = false;

            if (mode.Contains("ipxe"))
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
            else
                path = settings.GetSettings("Tftp Path") + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
        }
        lblFileName1.Text = path;
        try
        {
            txtBootMenu.Text = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            path = path.Replace(@"\", @"\\");
            Master.Msgbox("Could Not Find " + path);
        }

    }
    protected void ddlTemplate_SelectedIndexChanged(object sender, EventArgs e)
    {
        Utility utility = new Utility();
        if (ddlTemplate.Text == "default")
        {
           
            createNewTemplate.Visible = false;
            modifyTemplate.Visible = false;

            string proxyDHCP = utility.GetSettings("Proxy Dhcp");

            if (proxyDHCP == "Yes")
            {
                divProxyDHCP.Visible = true;
                string biosFile = utility.GetSettings("Proxy Bios File");
                string efi32File = utility.GetSettings("Proxy Efi32 File");
                string efi64File = utility.GetSettings("Proxy Efi64 File");
                if (biosFile.Contains("linux") || efi32File.Contains("linux") || efi64File.Contains("linux"))
                {
                    proxyPassBoxes.Visible = true;
                }

                try
                {
                    ddlBiosKernel.DataSource = Utility.GetKernels();
                    ddlBiosKernel.DataBind();
                    ddlEfi32Kernel.DataSource = Utility.GetKernels();
                    ddlEfi32Kernel.DataBind();
                    ddlEfi64Kernel.DataSource = Utility.GetKernels();
                    ddlEfi64Kernel.DataBind();

                    ListItem itemKernel = ddlBiosKernel.Items.FindByText("3.18.1-WDS");
                    if (itemKernel != null)
                    {
                        ddlBiosKernel.SelectedValue = "3.18.1-WDS";
                        ddlEfi32Kernel.SelectedValue = "3.18.1-WDS";
                    }
                    else
                    {
                        ddlBiosKernel.Items.Insert(0, "Select Kernel");
                        ddlEfi32Kernel.Items.Insert(0, "Select Kernel");                 
                    }

                    ListItem itemKernel64 = ddlEfi64Kernel.Items.FindByText("3.18.1x64-WDS");
                    if(itemKernel64 != null)
                        ddlEfi64Kernel.SelectedValue = "3.18.1x64-WDS";
                    else
                        ddlEfi64Kernel.Items.Insert(0, "Select Kernel");


                    ddlBiosBootImage.DataSource = Utility.GetBootImages();
                    ddlBiosBootImage.DataBind();
                    ddlEfi32BootImage.DataSource = Utility.GetBootImages();
                    ddlEfi32BootImage.DataBind();
                    ddlEfi64BootImage.DataSource = Utility.GetBootImages();
                    ddlEfi64BootImage.DataBind();

                    ListItem itemBootImage = ddlBiosBootImage.Items.FindByText("initrd.gz");
                    if (itemBootImage != null)
                    {
                        ddlBiosBootImage.SelectedValue = "initrd.gz";
                        ddlEfi32BootImage.SelectedValue = "initrd.gz";
                    }
                    else
                    {
                        ddlBiosBootImage.Items.Insert(0, "Select Boot Image");
                        ddlEfi32BootImage.Items.Insert(0, "Select Boot Image");
                    }

                    ListItem itemBootImage64 = ddlEfi64BootImage.Items.FindByText("initrd64.gz");
                    if (itemBootImage != null)
                        ddlEfi64BootImage.SelectedValue = "initrd64.gz";
                    else
                        ddlEfi64BootImage.Items.Insert(0, "Select Boot Image");
                }
                catch { }
            }
            else
            {
                bootPasswords.Visible = true;
                divPXEMode.Visible = true;
                string pxeMode = utility.GetSettings("PXE Mode");
                if (pxeMode.Contains("ipxe"))
                {
                    passboxes.Visible = false;
                    btnSubmitDefault.Visible = false;
                    btnSubmitIPXE.Visible = true;
                }
                try
                {
                    ddlHostKernel.DataSource = Utility.GetKernels();
                    ddlHostKernel.DataBind();
                    ListItem itemKernel = ddlHostKernel.Items.FindByText("3.18.1-WDS");
                    if (itemKernel != null)
                        ddlHostKernel.SelectedValue = "3.18.1-WDS";
                    else
                        ddlHostKernel.Items.Insert(0, "Select Kernel");
                }
                catch { }

                try
                {
                    ddlHostBootImage.DataSource = Utility.GetBootImages();
                    ddlHostBootImage.DataBind();
                    ListItem itemBootImage = ddlHostBootImage.Items.FindByText("initrd.gz");
                    if (itemBootImage != null)
                        ddlHostBootImage.SelectedValue = "initrd.gz";
                    else
                        ddlHostBootImage.Items.Insert(0, "Select Boot Image");
                }
                catch { }
            }
        }

        else if (ddlTemplate.Text == "new template")
        {
            createNewTemplate.Visible = true;
            bootPasswords.Visible = false;
            modifyTemplate.Visible = false;
        }

        else if (ddlTemplate.Text == "select template")
        {
            createNewTemplate.Visible = false;
            bootPasswords.Visible = false;
            modifyTemplate.Visible = false;
        }
        else if (ddlTemplate.Text == "---------------")
        {
            //do nothing
        }
        else
        {
            modifyTemplate.Visible = true;
            createNewTemplate.Visible = false;
            bootPasswords.Visible = false;
            txtModifyTemplate.ReadOnly = true;
            BootTemplate template = new BootTemplate().Read(ddlTemplate.Text);
            txtModifyTemplate.Text = template.templateName;
            txtAreaModifyTemplate.Text = template.templateContent;
        }
    }

    public void btnProxSubmitDefault_Click(object sender, EventArgs e)
    {
        Settings bootMenu = new Settings();
        bootMenu.DebugPwd = proxconsoleSha.Value;
        bootMenu.AddPwd = proxaddhostSha.Value;
        bootMenu.OndPwd = proxondsha.Value;
        bootMenu.DiagPwd = proxdiagsha.Value;
        bootMenu.CreateBootMenu(bootMenu, ddlBiosKernel.SelectedValue, ddlBiosBootImage.SelectedValue,"bios");
        bootMenu.CreateBootMenu(bootMenu, ddlEfi32Kernel.SelectedValue, ddlEfi32BootImage.SelectedValue, "efi32");
        bootMenu.CreateBootMenu(bootMenu, ddlEfi64Kernel.SelectedValue, ddlEfi64BootImage.SelectedValue, "efi64");
        Master.Msgbox(Utility.Message);
    }

    public void btnSubmitDefault_Click(object sender, EventArgs e)
    {
        Settings bootMenu = new Settings();
        bootMenu.DebugPwd = consoleSha.Value;
        bootMenu.AddPwd = addhostSha.Value;
        bootMenu.OndPwd = ondsha.Value;
        bootMenu.DiagPwd = diagsha.Value;
        bootMenu.CreateBootMenu(bootMenu, ddlHostKernel.SelectedValue, ddlHostBootImage.SelectedValue,"noprox");
        Master.Msgbox(Utility.Message);

    }
    protected void showTemplates_Click(object sender, EventArgs e)
    {
        bootEditor.Visible = false;
        bootTemplates.Visible = true;

        ddlTemplate.DataSource = Utility.PopulateBootMenusDdl();
        ddlTemplate.DataBind();
        ddlTemplate.Items.Insert(0, "select template");
        ddlTemplate.Items.Insert(1, "default");
        ddlTemplate.Items.Insert(2, "new template");
        ddlTemplate.Items.Insert(3, "---------------");

    }

    public void btnNewTemplate_Click(object sender, EventArgs e)
    {
        if (Utility.NoSpaceNotEmpty(txtNewTemplate.Text))
        {
            BootTemplate template = new BootTemplate();
            template.templateName = txtNewTemplate.Text;
            template.templateContent = txtAreaNewTemplate.Text;
            template.Create(template);
            showTemplates_Click(sender, e);
            Master.Msgbox(Utility.Message);
        }

        else
            Master.Msgbox("Template Name Cannot Be Empty Or Contain Spaces");
    }

    protected void btnDeleteTemplate_Click(object sender, EventArgs e)
    {
        BootTemplate template = new BootTemplate();
        template.Delete(txtModifyTemplate.Text);
        Master.Msgbox(Utility.Message);
        modifyTemplate.Visible = false;
        showTemplates_Click(sender, e);
    }

    protected void btnUpdateTemplate_Click(object sender, EventArgs e)
    {
        BootTemplate template = new BootTemplate();
        template.templateName = txtModifyTemplate.Text;
        template.templateContent = txtAreaModifyTemplate.Text;
        template.Update(template);
        Master.Msgbox(Utility.Message);
    }
}