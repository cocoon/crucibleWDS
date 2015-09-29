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

public partial class addusers : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Master.IsInMembership("User"))
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied");

            Group group = new Group();
            gvGroups.DataSource = group.Search("%");
            gvGroups.DataBind();
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (Utility.NoSpaceNotEmpty(txtUserName.Text))
        {
            if (Utility.NoSpaceNotEmpty(txtUserPwd.Text))
            {
                WDSUser user = new WDSUser();
                if (txtUserPwd.Text == txtUserPwdConfirm.Text)
                {
                    Group group = new Group();
                    List<string> listGroupManagement = new List<string>();
                    foreach (GridViewRow row in gvGroups.Rows)
                    {
                        CheckBox cb = (CheckBox)row.FindControl("chkSelector");
                        if (cb != null && cb.Checked)
                            listGroupManagement.Add(gvGroups.DataKeys[row.RowIndex].Value.ToString());
                    }

                    user.GroupManagement = String.Join(" ", listGroupManagement);
                    user.Name = txtUserName.Text;
                    user.Password = txtUserPwd.Text;
                    user.Membership = ddluserMembership.Text;
                    user.Salt = user.CreateSalt(16);

                    if (permissions.Visible == true)
                    {
                        if (chkOnd.Checked)
                            user.OndAccess = "1";
                        else
                            user.OndAccess = "0";
                        if (chkDebug.Checked)
                            user.DebugAccess = "1";
                        else
                            user.DebugAccess = "0";
                        if (chkDiag.Checked)
                            user.DiagAccess = "1";
                        else
                            user.DiagAccess = "0";
                    }
                    else
                    {
                        user.OndAccess = "1";
                        user.DiagAccess = "1";
                        user.DebugAccess = "1";
                    }
                    user.Create(user);
                    Master.Msgbox(Utility.Message);
                }
                else
                    Master.Msgbox("Passwords Did Not Match");
            }
            else
                Master.Msgbox("Password Cannot Be Empty Or Contain Spaces");
        }
        else
            Master.Msgbox("Name Cannot Be Empty Or Contain Spaces");
    }

    protected void SelectAll_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox hcb = (CheckBox)gvGroups.HeaderRow.FindControl("chkSelectAll");

        if (hcb.Checked == true)
            ToggleCheckState(true);
        else
            ToggleCheckState(false);
    }

    private void ToggleCheckState(bool checkState)
    {
        foreach (GridViewRow row in gvGroups.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkSelector");
            if (cb != null)
                cb.Checked = checkState;
        }
    }

    public string GetSortDirection(string SortExpression)
    {
        if (ViewState[SortExpression] == null)
            ViewState[SortExpression] = "Desc";
        else
            ViewState[SortExpression] = ViewState[SortExpression].ToString() == "Desc" ? "Asc" : "Desc";

        return ViewState[SortExpression].ToString();
    }

    protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
    {
        Group group = new Group();
        gvGroups.DataSource = group.Search("%");
        DataTable dataTable = gvGroups.DataSource as DataTable;

        if (dataTable != null)
        {
            DataView dataView = new DataView(dataTable);
            dataView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
            gvGroups.DataSource = dataView;
            gvGroups.DataBind();
        }
    }

    protected void ddluserMembership_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddluserMembership.Text == "User")
        {
            management.Visible = true;
            permissions.Visible = true;
        }
        else
        {
            management.Visible = false;
            permissions.Visible = false;
        }
    }
}