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
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Security;
using System.Data;
using System.DirectoryServices.AccountManagement;


public partial class _default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Utility utility = new Utility();
        if (utility.GetSettings("Force SSL") == "Yes")
        {
            if (!HttpContext.Current.Request.IsSecureConnection)
            {
                string root = Request.Url.GetLeftPart(UriPartial.Authority);
                root = root + Page.ResolveUrl("~/");
                root = root.Replace("http://", "https://");
                Response.Redirect(root);
            }
        }

        if (Request.IsAuthenticated)
            Response.Redirect("~/views/dashboard/dash.aspx");
    }
    protected void CrucibleLogin_Authenticate(object sender, AuthenticateEventArgs e)
    {
        Utility login = new Utility();
        History history = new History();
        WDSUser wdsuser = new WDSUser();

        string loginDomain = login.GetSettings("AD Login Domain");
        history.Type = "User";
        history.IP = GetIP();
        history.EventUser = CrucibleLogin.UserName;
        wdsuser.ID = wdsuser.GetID(CrucibleLogin.UserName);
        history.TypeID = wdsuser.ID;

        if (string.IsNullOrEmpty(wdsuser.ID))
        {
            history.Event = "Failed Login";
            e.Authenticated = false;
            lblError.Visible = true;
        }

        else
        {
            if (string.IsNullOrEmpty(loginDomain))
            {
                bool result = login.UserLogin(CrucibleLogin.UserName, CrucibleLogin.Password);
                if ((result))
                {
                    history.Event = "Successful Login";
                    e.Authenticated = true;
                }
                else
                {
                    history.Event = "Failed Login";
                    e.Authenticated = false;
                    lblError.Visible = true;
                }

            }
            else
            {
                try
                {
                    PrincipalContext context = new PrincipalContext(ContextType.Domain, loginDomain, CrucibleLogin.UserName, CrucibleLogin.Password);
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, CrucibleLogin.UserName);
                    if (user != null)
                    {
                        history.Event = "Successful Login";
                        e.Authenticated = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                    bool result = login.UserLogin(CrucibleLogin.UserName, CrucibleLogin.Password);
                    if ((result))
                    {
                        history.Event = "Successful Login";
                        e.Authenticated = true;
                    }
                    else
                    {
                        history.Event = "Failed Login";
                        e.Authenticated = false;
                        lblError.Visible = true;
                    }
                }
            }
        }

        history.CreateEvent(history);
    }

    public string GetIP()
    {
        string ip = null;
        try
        {
            string ipList = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipList))
                ip = ipList.Split(',')[0];

            ip = Request.ServerVariables["REMOTE_ADDR"];
        }
        catch { }
        Session["ip_address"] = ip;
        return ip;
    }
}
