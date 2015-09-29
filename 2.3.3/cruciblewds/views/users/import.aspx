<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="importusers" CodeFile="import.aspx.cs" %>

<%@ MasterType VirtualPath="~/views/master/cruciblewds.master" %>
<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="Server">
	<script type="text/javascript">
		$(document).ready(function () {
			$('#nav-users').addClass("nav-current")
		});
	</script>
	<div class="size-1 column">
		<h1 class="remove-bottom icon-user-title">Users</h1>
	</div>
	<div class="size-2 column offset-1">
		<div class="nav-btn-round">
			<a href="<%= ResolveUrl("~/views/users/create.aspx") %>" class="icon create" data-info="Create New User"></a>
			<a href="<%= ResolveUrl("~/views/users/search.aspx") %>" class="icon search" data-info="Search Users"></a>
			<a href="<%= ResolveUrl("~/views/users/import.aspx") %>" class="icon import nav-current" data-info="Import Users"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-4 column">
		<asp:FileUpload ID="FileUpload" runat="server" />
		<asp:LinkButton ID="btnImport" runat="server" Text="Upload" OnClick="btnImport_Click" CssClass="submits" />
	</div>
</asp:Content>
