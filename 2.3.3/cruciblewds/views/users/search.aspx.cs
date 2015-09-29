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
using System.Collections.Generic;
using System.Data;

public partial class searchusers : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (!Master.IsInMembership("Administrator"))
            {
                WDSUser wdsuser = new WDSUser();
                string userID = wdsuser.GetID(HttpContext.Current.User.Identity.Name);
                 if(string.IsNullOrEmpty(userID)) //Fix for clicking logout button when on users page
                     Response.Redirect("~/");
                 else
                Response.Redirect("~/views/users/resetpass.aspx?userid=" + userID);
            }
            PopulateGrid();
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        WDSUser user = new WDSUser();
        List<int> listDelete = new List<int>();
        bool adminError = false;

        foreach (GridViewRow row in gvUsers.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkSelector");
            if (cb != null && cb.Checked)
            {
                listDelete.Add(Convert.ToInt32(gvUsers.DataKeys[row.RowIndex].Value));
                user.Membership = row.Cells[3].Text;
            }
            if (user.Membership == "Administrator")
            {
                Master.Msgbox("Administrators Must Be Changed To A Lower Level User Before They Can Be Deleted");
                adminError = true;
                break;
            }
        }

        if (!adminError)
        {
            if (listDelete.Count > 0)
            {
                user.Delete(listDelete);
                PopulateGrid();
                Master.Msgbox(Utility.Message);
            }
        }
    }

    protected void search_Changed(object sender, EventArgs e)
    {
        PopulateGrid();
    }

    protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox hcb = (CheckBox)gvUsers.HeaderRow.FindControl("chkSelectAll");

        if (hcb.Checked == true)
            ToggleCheckState(true);
        else
            ToggleCheckState(false);
    }

    private void ToggleCheckState(bool checkState)
    {
        foreach (GridViewRow row in gvUsers.Rows)
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
        PopulateGrid();
        DataTable dataTable = gvUsers.DataSource as DataTable;
        if (dataTable != null)
        {
            DataView dataView = new DataView(dataTable);
            dataView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
            gvUsers.DataSource = dataView;
            gvUsers.DataBind();
        }
    }

    protected void PopulateGrid()
    {
        WDSUser user = new WDSUser();
        gvUsers.DataSource = user.Search(txtSearch.Text);
        gvUsers.DataBind();
        lblTotal.Text = gvUsers.Rows.Count.ToString() + " Result(s) / " + user.GetTotalCount() + " Total User(s)";
    }
}