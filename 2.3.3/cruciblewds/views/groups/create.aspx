<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="addgroups" CodeFile="create.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/groups/create.aspx") %>" class="icon create nav-current" data-info="Create New Group"></a>
			<a href="<%= ResolveUrl("~/views/groups/search.aspx") %>" class="icon search" data-info="Search Groups"></a>
			<a href="<%= ResolveUrl("~/views/groups/import.aspx") %>" class="icon import" data-info="Import Groups"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-4 column">
		Group Name:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtGroupName" runat="server" CssClass="textbox"></asp:TextBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Group Image:
	</div>
	<div class="size-5 column">
		<asp:DropDownList ID="ddlGroupImage" runat="server" CssClass="ddlist">
		</asp:DropDownList>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Group Description:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtGroupDesc" runat="server" CssClass="descbox" TextMode="MultiLine"></asp:TextBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Group Kernel:
	</div>
	<div class="size-5 column">
		<asp:DropDownList ID="ddlGroupKernel" runat="server" CssClass="ddlist">
		</asp:DropDownList>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Group Boot Image:
	</div>
	<div class="size-5 column">
		<asp:DropDownList ID="ddlGroupBootImage" runat="server" CssClass="ddlist">
		</asp:DropDownList>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Group Arguments:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtGroupArguments" runat="server" CssClass="textbox"></asp:TextBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Sender Arguments:
	</div>
	<div class="size-5 column">
		<asp:TextBox ID="txtGroupSenderArgs" runat="server" CssClass="textbox"></asp:TextBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
	</div>
	<div class="size-5 column">
	</div>
	<br class="clear" />
	<div class="size-4 column">
		Group Scripts:
	</div>
	<div class="size-5 column">
		<asp:ListBox ID="lbScripts" runat="server" SelectionMode="Multiple"></asp:ListBox>
	</div>
	<br class="clear" />
	<div class="size-4 column">
		&nbsp;
	</div>
	<div class="size-5 column">
		<asp:LinkButton ID="Submit" runat="server" OnClick="Submit_Click" Text="Add Group" CssClass="submits" />
	</div>
	<br class="clear" />
	<hr />
	<div class="size-7 column">
		<asp:TextBox ID="txtSearchHosts" runat="server" AutoPostBack="True" CssClass="searchbox label-host-search" OnTextChanged="txtSearchHosts_TextChanged" on></asp:TextBox>
	</div>
	<br class="clear" />
	<p class="total">
		<asp:Label ID="lblTotal" runat="server"></asp:Label></p>
	<asp:GridView ID="gvHosts" runat="server" AutoGenerateColumns="False" CssClass="Gridview" AlternatingRowStyle-CssClass="alt" DataKeyNames="hostID">
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
			<asp:BoundField DataField="hostID" HeaderText="hostID" SortExpression="hostID" Visible="False" />
			<asp:BoundField DataField="hostName" HeaderText="Name" SortExpression="hostName" ItemStyle-CssClass="width_200" />
			<asp:BoundField DataField="hostMac" HeaderText="MAC" SortExpression="hostMac" ItemStyle-CssClass="width_200 mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
			<asp:BoundField DataField="hostGroup" HeaderText="Group" SortExpression="hostGroup" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
		</Columns>
		<EmptyDataTemplate>
			No Hosts Found
		</EmptyDataTemplate>
	</asp:GridView>
</asp:Content>
