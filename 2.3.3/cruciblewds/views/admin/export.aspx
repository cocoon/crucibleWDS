<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="backup" CodeFile="export.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/admin/export.aspx") %>" class="icon export nav-current" data-info="Export Database"></a>
			<a href="<%= ResolveUrl("~/views/admin/reports.aspx") %>" class="icon reports" data-info="Reports"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-4 column">
		<asp:LinkButton ID="btnExport" runat="server" Text="Export Database" OnClick="btnExport_Click" CssClass="submits" />
	</div>
</asp:Content>
