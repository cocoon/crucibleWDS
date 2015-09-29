<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="searchgroups" CodeFile="search.aspx.cs" %>

<%@ MasterType VirtualPath="~/views/master/cruciblewds.master" %>
<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="Server">
	<script type="text/javascript">
		$(document).ready(function () {
			$('#nav-groups').addClass("nav-current")
		});
	</script>
	<div class="size-1 column">
		<h1 class="remove-bottom icon-group-title">Groups</h1>
	</div>
	<div class="size-2 column offset-1">
		<div class="nav-btn-round">
			<a href="<%= ResolveUrl("~/views/groups/create.aspx") %>" class="icon create" data-info="Create New Group"></a>
			<a href="<%= ResolveUrl("~/views/groups/search.aspx") %>" class="icon search nav-current" data-info="Search Groups"></a>
			<a href="<%= ResolveUrl("~/views/groups/import.aspx") %>" class="icon import" data-info="Import Groups"></a>
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
	<asp:GridView ID="gvGroups" runat="server" AllowSorting="true" AutoGenerateColumns="False" OnSorting="gridView_Sorting" CssClass="Gridview" DataKeyNames="groupID" AlternatingRowStyle-CssClass="alt">
		<Columns>
			<asp:TemplateField>
				<HeaderStyle CssClass="chkboxwidth"></HeaderStyle>
				<ItemStyle CssClass="chkboxwidth"></ItemStyle>
				<HeaderTemplate>
					<asp:CheckBox ID="chkSelectAll" runat="server" AutoPostBack="True" OnCheckedChanged="SelectAll_CheckedChanged" />
				</HeaderTemplate>
				<ItemTemplate>
					<asp:CheckBox ID="chkSelector" runat="server" />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:BoundField DataField="groupID" HeaderText="groupID" InsertVisible="False" SortExpression="groupID" Visible="False" />
			<asp:BoundField DataField="groupName" HeaderText="Name" SortExpression="groupName" ItemStyle-CssClass="width_200" />
			<asp:BoundField DataField="groupImage" HeaderText="Image" SortExpression="groupImage" HeaderStyle-CssClass="mobi-hide-smallest" ItemStyle-CssClass="width_200 mobi-hide-smallest" />
			<asp:TemplateField ShowHeader="True" HeaderText="Members">
				<ItemTemplate>
					<asp:Label ID="lblCount" runat="server" CausesValidation="false" CssClass="lbl_file "></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:HyperLinkField DataNavigateUrlFields="groupID" DataNavigateUrlFormatString="~/views/groups/view.aspx?page=edit&groupid={0}" Text="View" />
		</Columns>
		<EmptyDataTemplate>
			No Groups Found
		</EmptyDataTemplate>
	</asp:GridView>
	<a class="confirm" href="#">Delete Selected Groups</a>
	<div id="confirmbox" class="confirm-box-outer">
		<div class="confirm-box-inner">
			<h4><asp:Label ID="lblTitle" runat="server" Text="Delete The Selected Groups?"></asp:Label></h4>
			<div class="confirm-box-btns">
				<asp:LinkButton ID="ConfirmButton" OnClick="btnSubmit_Click" runat="server" Text="Yes" CssClass="confirm_yes" />
				<asp:LinkButton ID="CancelButton" runat="server" Text="No" CssClass="confirm_no" />
			</div>
		</div>
	</div>
</asp:Content>
