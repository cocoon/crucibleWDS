<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="logview" CodeFile="logview.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/admin/logview.aspx") %>" class="icon logs nav-current" data-info="Logs"></a>
			<a href="<%= ResolveUrl("~/views/admin/export.aspx") %>" class="icon export" data-info="Export Database"></a>
			<a href="<%= ResolveUrl("~/views/admin/reports.aspx") %>" class="icon reports" data-info="Reports"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-7 column">
		<asp:DropDownList ID="ddlLog" runat="server" CssClass="ddlist" AutoPostBack="True">
		</asp:DropDownList>
	</div>
	<br class="clear" />
	<div class="size-4 column" style="float: right; margin: 0px;">
		<asp:DropDownList ID="ddlLimit" runat="server" CssClass="ddlist" Style="width: 75px; float: right;" AutoPostBack="true" OnSelectedIndexChanged="ddlLimit_SelectedIndexChanged">
			<asp:ListItem>10</asp:ListItem>
			<asp:ListItem>25</asp:ListItem>
			<asp:ListItem>50</asp:ListItem>
			<asp:ListItem>100</asp:ListItem>
			<asp:ListItem>All</asp:ListItem>
		</asp:DropDownList>
        <br class="clear" />
        <asp:LinkButton ID="btnExportLog" runat="server" Text="Export Log" CssClass="submits" OnClick="btnExportLog_Click"></asp:LinkButton>
	</div>
    <br class="clear" />
	<asp:GridView ID="GridView1" runat="server" CssClass="Gridview log" ShowHeader="false">
	</asp:GridView>
</asp:Content>
