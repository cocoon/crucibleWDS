<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="searchhosts" CodeFile="search.aspx.cs" %>

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
				<a href="<%= ResolveUrl("~/views/hosts/create.aspx") %>" class="icon create" data-info="Create New Host"></a>
			<a href="<%= ResolveUrl("~/views/hosts/search.aspx") %>" class="icon search nav-current" data-info="Search Hosts"></a>
			<a href="<%= ResolveUrl("~/views/hosts/import.aspx") %>" class="icon import" data-info="Import Hosts"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-7 column">
		<asp:TextBox ID="txtSearch" runat="server" CssClass="searchbox" OnTextChanged="search_Changed"></asp:TextBox>
	</div>
	<br class="clear" />
	<p class="total">
		<asp:Label ID="lblTotal" runat="server"></asp:Label></p>
	<asp:GridView ID="gvHosts" runat="server" AllowSorting="True" DataKeyNames="hostID" OnSorting="gridView_Sorting" AutoGenerateColumns="False" CssClass="Gridview" AlternatingRowStyle-CssClass="alt">
		<Columns>
			<asp:TemplateField>
				<HeaderStyle CssClass="chkboxwidth"></HeaderStyle>
				<ItemStyle CssClass="chkboxwidth"></ItemStyle>
				<HeaderTemplate>
					<asp:CheckBox ID="chkSelectAll" runat="server" AutoPostBack="True" OnCheckedChanged="chkSelectAll_CheckedChanged" />
				</HeaderTemplate>
				<ItemTemplate>
					<asp:CheckBox ID="chkSelector" runat="server" />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:BoundField DataField="hostID" HeaderText="hostID" SortExpression="hostID" Visible="False" />
			<asp:BoundField DataField="hostName" HeaderText="Name" SortExpression="hostName" ItemStyle-CssClass="width_200"></asp:BoundField>
			<asp:BoundField DataField="hostMac" HeaderText="MAC" SortExpression="hostMac" ItemStyle-CssClass="width_200 mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
			<asp:BoundField DataField="hostImage" HeaderText="Image" SortExpression="hostImage" ItemStyle-CssClass="width_200 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
			<asp:BoundField DataField="hostGroup" HeaderText="Group" SortExpression="hostGroup" ItemStyle-CssClass="width_200 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
			<asp:HyperLinkField DataNavigateUrlFields="hostID" DataNavigateUrlFormatString="~/views/hosts/view.aspx?page=edit&hostid={0}" Text="View" />
		</Columns>
		<EmptyDataTemplate>
			No Hosts Found
		</EmptyDataTemplate>
	</asp:GridView>
	<a class="confirm" href="#">Delete Selected Hosts</a>
	<div id="confirmbox" class="confirm-box-outer">
		<div class="confirm-box-inner">
			<h4><asp:Label ID="lblTitle" runat="server" Text="Delete The Selected Hosts?"></asp:Label></h4>
			<div class="confirm-box-btns">
				<asp:LinkButton ID="ConfirmButton" OnClick="btnSubmit_Click" runat="server" Text="Yes" CssClass="confirm_yes" />
				<asp:LinkButton ID="CancelButton" runat="server" Text="No" CssClass="confirm_no" />
			</div>
		</div>
	</div>
</asp:Content>
