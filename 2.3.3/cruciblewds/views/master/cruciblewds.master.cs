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
using System.IO;
using System.Security.Principal;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Globalization;
using Npgsql;
using Newtonsoft.Json;

public partial class cruciblewds : System.Web.UI.MasterPage
{
    public void Page_Init(object sender, EventArgs e)
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

        if (!Request.IsAuthenticated)
            Response.Redirect("~/", true);
    }
    public void Page_Load(object sender, EventArgs e)
    { 
        Page.MaintainScrollPositionOnPostBack = true;
    }

    public void Msgbox(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            string msgType = "showSuccessToast";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "msgBox", "$(function() { $().toastmessage('" + msgType + "', " + "\"" + message + "\"); });", true);
            Session.Remove("Message");
        }
    }

    public string GetIP()
    {
        string ip = null;
        try
        {
            string ipList = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipList))
            {
                ip = ipList.Split(',')[0];
            }

            ip = Request.ServerVariables["REMOTE_ADDR"];
        }
        catch { }

        return ip;
    }
   
    public bool IsInMembership(string membership)
    {
        string[] userMembership = new string[1];
        using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
        {
            NpgsqlCommand cmd = new NpgsqlCommand("users_getmembership", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userName", HttpContext.Current.User.Identity.Name);
            conn.Open();
            userMembership[0] = cmd.ExecuteScalar() as string;
        }

        HttpContext.Current.User = new GenericPrincipal(HttpContext.Current.User.Identity, userMembership);

        IPrincipal user = HttpContext.Current.User;
        if ((user.IsInRole(membership)))
            return true;
        else
            return false;
    }
}
