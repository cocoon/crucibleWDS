<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="addimages" CodeFile="create.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/images/create.aspx") %>" class="icon create nav-current" data-info="Create New Image"></a>
			<a href="<%= ResolveUrl("~/views/images/search.aspx") %>" class="icon search" data-info="Search Images"></a>
			<a href="<%= ResolveUrl("~/views/images/import.aspx") %>" class="icon import" data-info="Import Images"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-4 column">
		Image Name:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtImageName" runat="server" CssClass="textbox"></asp:TextBox>
	</div>
	<br class="clear" />

	<div class="size-4 column">
		Image Description:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtImageDesc" runat="server" TextMode="MultiLine" CssClass="descbox"></asp:TextBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Protected:
	</div>
	<div class="size-5 column">
		<asp:CheckBox ID="chkProtected" runat="server" />
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Visible In On Demand:
	</div>
	<div class="size-5 column">
		<asp:CheckBox ID="chkVisible" runat="server" />
	</div>
	<br class="clear" />
	<div class="size-4 column">
		&nbsp;
	</div>
	<div class="size-5 column">
		<asp:LinkButton ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" Text="Add Image" CssClass="submits" />
	</div>
	<br class="clear" />
</asp:Content>
