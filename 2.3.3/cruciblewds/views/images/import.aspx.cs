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
using System.IO;
using System.Data.OleDb;
using System.Data;
using Mono.Unix.Native;

public partial class importimages : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Master.IsInMembership("User"))
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied");
        }
    }

    protected void btnImport_Click(object sender, EventArgs e)
    {
        Image image = new Image();
        string csvFilePath = Server.MapPath("~") + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "csvupload" + Path.DirectorySeparatorChar + "images.csv";
        FileUpload.SaveAs(csvFilePath);
        if (Environment.OSVersion.ToString().Contains("Unix"))
        {
            Syscall.chmod(csvFilePath, (FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH | FilePermissions.S_IRUSR));
        }
        image.Import();
        Master.Msgbox(Utility.Message);   
    }
}