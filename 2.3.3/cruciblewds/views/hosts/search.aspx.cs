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
using System.Collections.Generic;
using System.Data;
using System.IO;

public partial class searchhosts : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Utility utility = new Utility();
            Master.Msgbox(Utility.Message); //For Redirects
            if(utility.GetSettings("Default Host View") == "all")
                PopulateGrid();
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        Host host = new Host();
        List<int> listDelete = new List<int>();
        foreach (GridViewRow row in gvHosts.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkSelector");
            if (cb != null && cb.Checked)
                listDelete.Add(Convert.ToInt32(gvHosts.DataKeys[row.RowIndex].Value));
        }

        if (listDelete.Count > 0)
        {
            host.Delete(listDelete);
            PopulateGrid();
            Master.Msgbox(Utility.Message);
        }
    }

    protected void search_Changed(object sender, EventArgs e)
    {
        PopulateGrid();
    }

    protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox hcb = (CheckBox)gvHosts.HeaderRow.FindControl("chkSelectAll");

        if (hcb.Checked == true)
            ToggleCheckState(true);
        else
            ToggleCheckState(false);
    }

    private void ToggleCheckState(bool checkState)
    {
        foreach (GridViewRow row in gvHosts.Rows)
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

        DataTable dataTable = gvHosts.DataSource as DataTable;

        if (dataTable != null)
        {
            DataView dataView = new DataView(dataTable);
            dataView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
            gvHosts.DataSource = dataView;
            gvHosts.DataBind();
        }

    }

    protected void PopulateGrid()
    {
        Host host = new Host();
        if (Master.IsInMembership("User"))
            gvHosts.DataSource = host.TableForUser(txtSearch.Text);
        else
            gvHosts.DataSource = host.Search(txtSearch.Text);

        gvHosts.DataBind();

        lblTotal.Text = gvHosts.Rows.Count.ToString() + " Result(s) / " + host.GetTotalCount() + " Total Host(s)";
    }
}
