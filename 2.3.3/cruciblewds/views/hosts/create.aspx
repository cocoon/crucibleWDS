<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="addhosts" CodeFile="create.aspx.cs" %>

<%@ MasterType VirtualPath="~/views/master/cruciblewds.master" %>
<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="Server">
	<script type="text/javascript">
		$(document).ready(function () {
			$('#nav-hosts').addClass("nav-current")
		});
	</script>
	<div class="size-1 column">
		<h1 class="remove-bottom icon-host-title">Hosts</h1>
	</div>
	<div class="size-2 column offset-1">
		<div class="nav-btn-round">
			<a href="<%= ResolveUrl("~/views/hosts/create.aspx") %>" class="icon create nav-current" data-info="Create New Host"></a>
			<a href="<%= ResolveUrl("~/views/hosts/search.aspx") %>" class="icon search" data-info="Search Hosts"></a>
			<a href="<%= ResolveUrl("~/views/hosts/import.aspx") %>" class="icon import" data-info="Import Hosts"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-4 column">
		Host Name:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtHostName" runat="server" CssClass="textbox"></asp:TextBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Host MAC Address:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtHostMac" runat="server" CssClass="textbox" MaxLength="17"></asp:TextBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Host Image:
	</div>
	<div class="size-5 column">
		<asp:DropDownList ID="ddlHostImage" runat="server" CssClass="ddlist">
		</asp:DropDownList>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Host Group:
	</div>
	<div class="size-5 column">
		<asp:DropDownList ID="ddlHostGroup" runat="server" CssClass="ddlist">
		</asp:DropDownList>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Host Description:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtHostDesc" runat="server" CssClass="descbox" TextMode="MultiLine"></asp:TextBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Host Kernel:
	</div>
	<div class="size-5 column">
		<asp:DropDownList ID="ddlHostKernel" runat="server" CssClass="ddlist">
		</asp:DropDownList>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Host Boot Image:
	</div>
	<div class="size-5 column">
		<asp:DropDownList ID="ddlHostBootImage" runat="server" CssClass="ddlist">
		</asp:DropDownList>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Host Arguments:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtHostArguments" runat="server" CssClass="textbox"></asp:TextBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Host Scripts:
	</div>
	<div class="size-5 column">
		<asp:ListBox ID="lbScripts" runat="server" SelectionMode="Multiple"></asp:ListBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Create Another?
		<asp:CheckBox runat="server" ID="createAnother" />
	</div>
	<br class="clear" />
	<div class="size-4 column">
		&nbsp;
	</div>
	<div class="size-5 column">
		<asp:LinkButton ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" Text="Add Host" CssClass="submits" />
	</div>
</asp:Content>
