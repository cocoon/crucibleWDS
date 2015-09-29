<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="importimages" CodeFile="import.aspx.cs" %>

<%@ MasterType VirtualPath="~/views/master/cruciblewds.master" %>
<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="Server">
	<script type="text/javascript">
		$(document).ready(function () {
			$('#nav-images').addClass("nav-current")
		});
	</script>
	<div class="size-1 column">
		<h1 class="remove-bottom icon-image-title">Images</h1>
	</div>
	<div class="size-2 column offset-1">
		<div class="nav-btn-round">
			<a href="<%= ResolveUrl("~/views/images/create.aspx") %>" class="icon create" data-info="Create New Image"></a>
			<a href="<%= ResolveUrl("~/views/images/search.aspx") %>" class="icon search" data-info="Search Images"></a>
			<a href="<%= ResolveUrl("~/views/images/import.aspx") %>" class="icon import nav-current " data-info="Import Images"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-4 column">
		<asp:FileUpload ID="FileUpload" runat="server" />
		<asp:LinkButton ID="btnImport" runat="server" Text="Upload" OnClick="btnImport_Click" CssClass="submits" />
	</div>
	<br class="clear" />
</asp:Content>
