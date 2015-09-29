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
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

public partial class searchgroups : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Master.Msgbox(Utility.Message); //Display message after redirect to this page.
            PopulateGrid();
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        Group group = new Group();
        List<int> listDelete = new List<int>();
        foreach (GridViewRow row in gvGroups.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkSelector");
            if (cb != null && cb.Checked)
                listDelete.Add(Convert.ToInt32(gvGroups.DataKeys[row.RowIndex].Value));    
        }

        if (listDelete.Count > 0)
        {
            group.Delete(listDelete);
            PopulateGrid();
            Master.Msgbox(Utility.Message);
        }
    }

    protected void search_Changed(object sender, EventArgs e)
    {
        PopulateGrid();
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
        PopulateGrid();

        DataTable dataTable = gvGroups.DataSource as DataTable;
        if (dataTable != null)
        {
            DataView dataView = new DataView(dataTable);
            dataView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
            gvGroups.DataSource = dataView;
            gvGroups.DataBind();
            foreach (GridViewRow row in gvGroups.Rows)
            {
                Label lbl = row.FindControl("lblCount") as Label;
                lbl.Text = group.GetMemberCount(row.Cells[2].Text);
            }
        }
    }

    protected void PopulateGrid()
    {
        Group group = new Group();

        if (Master.IsInMembership("User"))
            gvGroups.DataSource = group.TableForUser(txtSearch.Text);
        else
            gvGroups.DataSource = group.Search(txtSearch.Text);

        gvGroups.DataBind();

        foreach (GridViewRow row in gvGroups.Rows)
        {
            Label lbl = row.FindControl("lblCount") as Label;
            lbl.Text = group.GetMemberCount(row.Cells[2].Text);
        }

        lblTotal.Text = gvGroups.Rows.Count.ToString() + " Result(s) / " + group.GetTotalCount() + " Total Group(s)";
    }
}