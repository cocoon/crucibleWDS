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

public partial class modifyusers : System.Web.UI.Page
{
    public string requestedPage { get; set; }
    public WDSUser user { get; set; }

    #region View Page

    protected void Page_Load(object sender, EventArgs e)
    {
        user = new WDSUser();
        requestedPage = Request.QueryString["page"] as string;
        user.ID = Request.QueryString["userid"] as string;
        user.Read(user);

        if (!IsPostBack)
        {

            if (Master.IsInMembership("User"))
                Response.Redirect("~/views/dashboard/dash.aspx?access=denied");
                
            Master.Msgbox(Utility.Message);

            switch (requestedPage)
            {
                case "edit":
                    lblSubNav.Text = "| edit";
                    edit.Visible = true;
                    edit_page();
                    break;
                case "history":
                    lblSubNav.Text = "| history";
                    historypage.Visible = true;
                    history_page();
                    break;
                default:
                    break;
            }

           
        }
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

    protected void btnDelete_Click(object sender, EventArgs e)
    {
         string currentMembership = (string)(ViewState["currentMembership"]);
         int adminCount = user.GetAdminCount();
         if (adminCount == 1 && currentMembership == "Administrator")
         {
              Master.Msgbox("There Must Be At Least One Administrator");

         }
         else
         {
              lblTitle.Text = "Delete This User?";
              ClientScript.RegisterStartupScript(this.GetType(), "modalscript", "$(function() {  var menuTop = document.getElementById('confirmbox'),body = document.body;classie.toggle(menuTop, 'confirm-box-outer-open'); });", true);
         }
    }

    protected void OkButton_Click(object sender, EventArgs e)
    {

         List<int> delList = new List<int>();
         delList.Add(Convert.ToInt32(user.ID));
         user.Delete(delList);
         if (Utility.Message.Contains("Successfully"))
              Response.Redirect("~/views/users/search.aspx");
         else
              Master.Msgbox(Utility.Message);
    }

    #endregion

    #region Edit Page

    protected void edit_page()
    {
        ViewState["currentMembership"] = user.Membership;
        Group group = new Group();
        gvGroups.DataSource = group.Search("%");
        gvGroups.DataBind();

        if (user.Membership == "User")
        {
            management.Visible = true;
            permissions.Visible = true;
            List<string> listGroupManagement = new List<string>();
            foreach (GridViewRow row in gvGroups.Rows)
            {
                CheckBox cb = (CheckBox)row.FindControl("chkSelector");
                if (user.GroupManagement.Contains(gvGroups.DataKeys[row.RowIndex].Value.ToString()))
                    cb.Checked = true;
            }
        }
        txtUserName.Text = user.Name;
        ddluserMembership.Text = user.Membership;
        if (user.OndAccess == "1")
            chkOnd.Checked = true;
        if (user.DebugAccess == "1")
            chkDebug.Checked = true;
        if (user.DiagAccess == "1")
            chkDiag.Checked = true;
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (Utility.NoSpaceNotEmpty(txtUserName.Text))
        {
            if ((string.IsNullOrEmpty(txtUserPwd.Text)) && (string.IsNullOrEmpty(txtUserPwdConfirm.Text)))
            {
                string currentMembership = (string)(ViewState["currentMembership"]);

                int adminCount = user.GetAdminCount();
                if (adminCount == 1 && ddluserMembership.Text != "Administrator" && currentMembership == "Administrator")
                    Master.Msgbox("There Must Be At Least One Administrator");
                else
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
                    user.Membership = ddluserMembership.Text;
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
                    user.UpdateNoPass(user, user.ID);
                    Master.Msgbox(Utility.Message);
                }
            }

            else
            {
                if (Utility.NoSpaceNotEmpty(txtUserPwd.Text))
                {
                    string currentMembership = (string)(ViewState["currentMembership"]);

                    if (txtUserPwd.Text == txtUserPwdConfirm.Text)
                    {
                        int adminCount = user.GetAdminCount();
                        if (adminCount == 1 && ddluserMembership.Text != "Administrator" && currentMembership == "Administrator")
                            Master.Msgbox("There Must Be At Least One Administrator");
                        else
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
                            user.Update(user, user.ID);
                            Master.Msgbox(Utility.Message);
                        }
                    }
                    else
                        Master.Msgbox("Passwords Did Not Match");
                }
                else
                    Master.Msgbox("Password Cannot Be Empty Or Contain Spaces");
            }
        }
        else
            Master.Msgbox("Name Cannot Be Empty Or Contain Spaces");
    }
    #endregion

    #region History Page
    protected void history_page()
    {
        if (!IsPostBack)
            ddlLimit.SelectedValue = "10";
        History history = new History();
        history.Type = "User";
        history.TypeID = user.ID;
        gvHistory.DataSource = history.ReadUser(history, ddlLimit.Text, user.Name);
        gvHistory.DataBind();
        Master.Msgbox(Utility.Message);

    }

    protected void ddlLimit_SelectedIndexChanged(object sender, EventArgs e)
    {
        History history = new History();
        history.Type = "User";
        history.TypeID = user.ID;
        gvHistory.DataSource = history.ReadUser(history, ddlLimit.Text, user.Name);
        gvHistory.DataBind();
        Master.Msgbox(Utility.Message);
    }

    #endregion
}