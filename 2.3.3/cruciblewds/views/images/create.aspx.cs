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

public partial class addimages : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Master.IsInMembership("User"))
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied");

            chkVisible.Checked = true;
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (Utility.NoSpaceNotEmpty(txtImageName.Text))
        {
            Image image = new Image();
            image.Name = txtImageName.Text;
            //OS no longer needed as of 2.3.0
            image.OS = "";
            image.Description = txtImageDesc.Text;
            if (chkProtected.Checked)
                image.Protected = 1;
            else
                image.Protected = 0;
            if (chkVisible.Checked)
                image.IsVisible = 1;
            else
                image.IsVisible = 0;
            image.Create(image);
            if (Utility.Message.Contains("Successfully"))
                Response.Redirect("~/views/images/view.aspx?page=edit&imageid=" + image.GetImageID(image.Name));
            else
            Master.Msgbox(Utility.Message);
        }
        else
            Master.Msgbox("Name Cannot Be Empty Or Contain Spaces");
    }
}
