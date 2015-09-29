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

public partial class custom : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Utility utility = new Utility();
        WDSUser user = new WDSUser();
        user.ID = user.GetID(HttpContext.Current.User.Identity.Name);
        user = user.Read(user);

        if (utility.GetSettings("On Demand") == "Disabled")
        {
            secure.Visible = false;
            secureMsg.Text = "On Demand Mode Has Been Globally Disabled";
            secureMsg.Visible = true;
        }
        else if (user.OndAccess == "0")
        {
             secure.Visible = false;
             secureMsg.Text = "On Demand Mode Has Been Disabled For This Account";
             secureMsg.Visible = true;
        }
        else
        {
             secure.Visible = true;
             secureMsg.Visible = false;
        }
        if (!IsPostBack)
        {
            ddlImage.DataSource = Utility.PopulateImagesDdl();
            ddlImage.DataBind();
            ddlImage.Items.Insert(0, "Select Image");
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (ddlImage.Text != "Select Image")
        {
            Multicast custom = new Multicast();

            custom.StartCustomMC(ddlImage.Text);

            Master.Msgbox(Utility.Message);
        }
        else
            Master.Msgbox("Select An Image");
    }

}

