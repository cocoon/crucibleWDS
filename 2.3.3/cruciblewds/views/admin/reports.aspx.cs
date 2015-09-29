using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


public partial class reports : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Master.IsInMembership("Administrator"))
            Response.Redirect("~/views/dashboard/dash.aspx?access=denied");

        Reports reports = new Reports();
        
        gvLastFiveUsers.DataSource = reports.LastUsers();
        gvLastFiveUsers.DataBind();

        gvLastFiveUnicasts.DataSource = reports.LastUnicasts();
        gvLastFiveUnicasts.DataBind();

        gvLastFiveMulticasts.DataSource = reports.LastMulticasts();
        gvLastFiveMulticasts.DataBind();

        gvTopFiveUnicasts.DataSource = reports.TopFiveUnicast();
        gvTopFiveUnicasts.DataBind();

        gvTopFiveMulticasts.DataSource = reports.TopFiveMulticast();
        gvTopFiveMulticasts.DataBind();

        gvUserStats.DataSource = reports.UserStats();
        gvUserStats.DataBind();
    }
}
