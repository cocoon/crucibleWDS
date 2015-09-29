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
using System.Data;
using System.Collections.Generic;

public partial class resetpass : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (!Master.IsInMembership("Administrator"))
            {
                WDSUser wdsuser = new WDSUser();
                string tmpUserID = wdsuser.GetID(HttpContext.Current.User.Identity.Name);
                if(tmpUserID != Request.QueryString["userid"] as string)
                    Response.Redirect("~/views/dashboard/dash.aspx?access=denied");
            }
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {

        if (Utility.NoSpaceNotEmpty(txtUserPwd.Text))
        {
            WDSUser user = new WDSUser();
            user.ID = Request.QueryString["userid"] as string;
            user = user.Read(user);

            if (txtUserPwd.Text == txtUserPwdConfirm.Text)
            {
                    user.Password = txtUserPwd.Text;
                    user.Salt = user.CreateSalt(16);
                    user.Update(user, user.ID);
                    Master.Msgbox(Utility.Message);
            }
            else
                Master.Msgbox("Passwords Did Not Match");
        }
        else
            Master.Msgbox("Password Cannot Be Empty Or Contain Spaces");

    }
}