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

public partial class active : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["clickTracker"] = "1";
            Task task = new Task();
            gvTasks.DataSource = task.ReadActive();
            gvTasks.DataBind();
            gvUcTasks.DataSource = task.ReadActiveUC();
            gvUcTasks.DataBind();
            gvMcTasks.DataSource = task.ReadActiveMC();
            gvMcTasks.DataBind();
            GetMCInfo();
            Master.Msgbox(Utility.Message);
        }
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        Task task = new Task();
        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        int taskID = Convert.ToInt32(gvTasks.DataKeys[gvRow.RowIndex].Value);
        task.DeleteActiveTask(taskID);
        Master.Msgbox(Utility.Message);
        gvTasks.DataSource = task.ReadActive();
        gvTasks.DataBind();
        gvUcTasks.DataSource = task.ReadActiveUC();
        gvUcTasks.DataBind();
    }

    protected void btnCancelMc_Click(object sender, EventArgs e)
    {
        Task task = new Task();
        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        int mcTaskID = Convert.ToInt32(gvMcTasks.DataKeys[gvRow.RowIndex].Value);
        string groupName = gvRow.Cells[2].Text;
        string pid = (gvRow.Cells[3].Text);
        task.DeleteActiveMcTask(mcTaskID, groupName, pid);
        Master.Msgbox(Utility.Message);
        gvMcTasks.DataSource = task.ReadActiveMC();
        gvMcTasks.DataBind();
        gvTasks.DataSource = task.ReadActive();
        gvTasks.DataBind();
    }

    protected void btnMembers_Click(object sender, EventArgs e)
    {
        int cTracker = Convert.ToInt16(ViewState["clickTracker"]);
        if (cTracker % 2 == 0)
            TimerMC.Enabled = true;
        else
            TimerMC.Enabled = false;
        ViewState["clickTracker"] = cTracker + 1;
        Task task = new Task();
        GridViewRow gvRow = (GridViewRow)(sender as Control).Parent.Parent;
        GridView gv = (GridView)gvRow.FindControl("gvMembers");

        int mcTaskID = Convert.ToInt32(gvMcTasks.DataKeys[gvRow.RowIndex].Value);
        string groupName = gvRow.Cells[2].Text;
        if (gv.Visible == false)
        {
            var td = gvRow.FindControl("tdMembers");
            td.Visible = true;
            gv.Visible = true;

            DataTable table = task.ReadMcMembers(groupName);
            gv.DataSource = table;
            gv.DataBind();
        }
        else
        {
            gv.Visible = false;
            var td = gvRow.FindControl("tdMembers");
            td.Visible = false;

        }
        Master.Msgbox(Utility.Message);
    }
    protected void cancelTasks_Click(object sender, EventArgs e)
    {
        Task task = new Task();
        task.CancelAll();
        gvMcTasks.DataSource = task.ReadActiveMC();
        gvMcTasks.DataBind();
        gvUcTasks.DataSource = task.ReadActiveUC();
        gvUcTasks.DataBind();
        gvTasks.DataSource = task.ReadActive();
        gvTasks.DataBind();
        Master.Msgbox(Utility.Message);
    }

    protected void Timer_Tick(object sender, EventArgs e)
    {

        Task task = new Task();
        gvTasks.DataSource = task.ReadActive();
        gvTasks.DataBind();
        gvUcTasks.DataSource = task.ReadActiveUC();
        gvUcTasks.DataBind();
        UpdatePanel1.Update();

    }

    protected void TimerMC_Tick(object sender, EventArgs e)
    {
        Task task = new Task();
      
        gvMcTasks.DataSource = task.ReadActiveMC();
        gvMcTasks.DataBind();
        GetMCInfo();
    }

    protected void GetMCInfo()
    {
        foreach (GridViewRow row in gvMcTasks.Rows)
        {
            try
            {
                Task task = new Task();
                DataTable tblProgress = task.ReadMCProgress(row.Cells[2].Text);
                Label lblPartition = row.FindControl("lblPartition") as Label;
                Label lblElapsed = row.FindControl("lblElapsed") as Label;
                Label lblRemaining = row.FindControl("lblRemaining") as Label;
                Label lblCompleted = row.FindControl("lblCompleted") as Label;
                Label lblRate = row.FindControl("lblRate") as Label;
                foreach (DataRow pRow in tblProgress.Rows)
                {
                    lblPartition.Text = pRow["_taskpartition"].ToString();
                    lblElapsed.Text = pRow["_taskelapsed"].ToString();
                    lblRemaining.Text = pRow["_taskremaining"].ToString();
                    lblCompleted.Text = pRow["_taskcompleted"].ToString();
                    lblRate.Text = pRow["_taskrate"].ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }
    }

    protected void btnShowAll_Click(object sender, EventArgs e)
    {
        if (gvTasks.Visible)
            gvTasks.Visible = false;
        else
            gvTasks.Visible = true;
    }
}
