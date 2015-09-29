<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="searchusers" CodeFile="search.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/users/search.aspx") %>" class="icon search nav-current" data-info="Search Users"></a>
			<a href="<%= ResolveUrl("~/views/users/import.aspx") %>" class="icon import" data-info="Import Users"></a>
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
	<asp:GridView ID="gvUsers" runat="server" AllowSorting="True" DataKeyNames="userID" OnSorting="gridView_Sorting" AutoGenerateColumns="False" CssClass="Gridview" AlternatingRowStyle-CssClass="alt">
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
			<asp:BoundField DataField="userID" HeaderText="userID" SortExpression="userID" Visible="False" />
			<asp:BoundField DataField="userName" HeaderText="Name" SortExpression="userName" />
			<asp:BoundField DataField="userMembership" HeaderText="Membership" SortExpression="userMembership" ItemStyle-CssClass="mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
			<asp:HyperLinkField DataNavigateUrlFields="userID" DataNavigateUrlFormatString="~/views/users/view.aspx?page=edit&userid={0}" Text="View" />
		</Columns>
		<EmptyDataTemplate>
			No Users Found
		</EmptyDataTemplate>
	</asp:GridView>
	<a class="confirm" href="#">Delete Selected Users</a>
	<div id="confirmbox" class="confirm-box-outer">
		<div class="confirm-box-inner">
			<h4><asp:Label ID="lblTitle" runat="server" Text="Delete The Selected Users?" CssClass="modaltitle"></asp:Label></h4>
			<div class="confirm-box-btns">
				<asp:LinkButton ID="ConfirmButton" OnClick="btnSubmit_Click" runat="server" Text="Yes" CssClass="confirm_yes" />
				<asp:LinkButton ID="CancelButton" runat="server" Text="No" CssClass="confirm_no" />
			</div>
		</div>
	</div>
</asp:Content>
