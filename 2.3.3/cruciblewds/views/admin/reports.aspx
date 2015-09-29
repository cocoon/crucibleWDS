<%@ Page Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" CodeFile="reports.aspx.cs" Inherits="reports" %>

<%@ MasterType VirtualPath="~/views/master/cruciblewds.master" %>
<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="Server">
	<script type="text/javascript">
		$(document).ready(function () {
			$('#nav-settings').addClass("nav-current")
		});
	</script>
	<div class="size-1 column">
		<h1 class="remove-bottom icon-setting-title">Admin</h1>
	</div>
	<div class="size-2 column offset-1">
		<div class="nav-btn-round">
			<a href="<%= ResolveUrl("~/views/admin/settings.aspx") %>" class="icon global " data-info="Global Settings"></a>
			<a href="<%= ResolveUrl("~/views/admin/bootmenu.aspx") %>" class="icon boot" data-info="Boot Menu"></a>
			<a href="<%= ResolveUrl("~/views/admin/logview.aspx") %>" class="icon logs" data-info="Logs"></a>
			<a href="<%= ResolveUrl("~/views/admin/export.aspx") %>" class="icon export" data-info="Export Database"></a>
			<a href="<%= ResolveUrl("~/views/admin/reports.aspx") %>" class="icon reports nav-current" data-info="Reports"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-6 column">
		<h4>Last 5 Logins</h4>
		<asp:GridView ID="gvLastFiveUsers" runat="server" Width="350px" CssClass="Gridview" ShowHeader="false" AlternatingRowStyle-CssClass="alt">
			<EmptyDataTemplate>
				No Data
			</EmptyDataTemplate>
		</asp:GridView>
		<h4>Last 5 Unicasts</h4>
		<asp:GridView ID="gvLastFiveUnicasts" runat="server" CssClass="Gridview" Width="350px" ShowHeader="false" AlternatingRowStyle-CssClass="alt">
			<EmptyDataTemplate>
				No Data
			</EmptyDataTemplate>
		</asp:GridView>
		<h4>Last 5 Multicasts</h4>
		<asp:GridView ID="gvLastFiveMulticasts" runat="server" Width="350px" CssClass="Gridview" ShowHeader="false" AlternatingRowStyle-CssClass="alt">
			<EmptyDataTemplate>
				No Data
			</EmptyDataTemplate>
		</asp:GridView>
		<h4>Top 5 Unicasts</h4>
		<asp:GridView ID="gvTopFiveUnicasts" runat="server" Width="350px" CssClass="Gridview" ShowHeader="false" AlternatingRowStyle-CssClass="alt">
			<EmptyDataTemplate>
				No Data
			</EmptyDataTemplate>
		</asp:GridView>
		<h4>Top 5 Multicasts</h4>
		<asp:GridView ID="gvTopFiveMulticasts" runat="server" Width="350px" CssClass="Gridview" ShowHeader="false" AlternatingRowStyle-CssClass="alt">
			<EmptyDataTemplate>
				No Data
			</EmptyDataTemplate>
		</asp:GridView>
	</div>
	<div class="size-5 column">
		<h4>User Stats</h4>
		<asp:GridView ID="gvUserStats" runat="server" CssClass="Gridview" Width="500px" ShowHeader="false" AlternatingRowStyle-CssClass="alt">
			<EmptyDataTemplate>
				No Data
			</EmptyDataTemplate>
		</asp:GridView>
	</div>
	<br class="clear" />
</asp:Content>
