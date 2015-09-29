<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="modifyusers" CodeFile="view.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/users/import.aspx") %>" class="icon import" data-info="Import Users"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="SubNav" ContentPlaceHolderID="SubNav" runat="Server">
	<div class="nav-btn-square ">
		<h3 class="remove-bottom">
			<%=user.Name %><asp:Label ID="lblSubNav" runat="server"></asp:Label></h3>
		<a id="editoption" href="<%= ResolveUrl("~/views/users/view.aspx") %>?page=edit&userid=<%=user.ID%>" class="icon edit" data-info="Edit User"></a>
		<a id="historyoption" href="<%= ResolveUrl("~/views/users/view.aspx") %>?page=history&userid=<%=user.ID%>" class="icon history" data-info="View User History"></a>
		<asp:LinkButton ID="btnDelete" runat="server" CssClass="icon delete" data-info="Delete User" OnClick="btnDelete_Click"></asp:LinkButton>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div id="edit" runat="server" visible="false">
		<!-- Edit Page =====================================================
=========================================================================-->
		<script type="text/javascript">
			$(document).ready(function () {
				$('#editoption').addClass("nav-current")
			});
		</script>
		<div class="size-4 column">
			User Name:
		</div>
		<div class="size-5 column">
			<asp:TextBox ID="txtUserName" runat="server" CssClass="textbox"></asp:TextBox>
		</div>
		<br class="clear" />
		<div class="size-4 column">
			User Membership:
		</div>
		<div class="size-5 column">
			<asp:DropDownList ID="ddluserMembership" runat="server" CssClass="ddlist" OnSelectedIndexChanged="ddluserMembership_SelectedIndexChanged" AutoPostBack="true">
				<asp:ListItem>Administrator</asp:ListItem>
				<asp:ListItem>Power User</asp:ListItem>
				<asp:ListItem>User</asp:ListItem>
			</asp:DropDownList>
		</div>
		<br class="clear" />
		<div class="size-4 column">
			User Password:
		</div>
		<div class="size-5 column">
			<asp:TextBox ID="txtUserPwd" runat="server" CssClass="textbox" TextMode="Password"></asp:TextBox>
		</div>
		<br class="clear" />
		<div class="size-4 column">
			Confirm Password:
		</div>
		<div class="size-5 column">
			<asp:TextBox ID="txtUserPwdConfirm" runat="server" CssClass="textbox" TextMode="Password"></asp:TextBox>
		</div>
		<br class="clear" />
		<div id="permissions" runat="server" visible="false">
			<div class="size-4 column">
				On Demand Access:
			</div>
			<div class="size-5 column">
				<asp:CheckBox ID="chkOnd" runat="server" />
			</div>
			<br class="clear" />
			<div class="size-4 column">
				Debug Access:
			</div>
			<div class="size-5 column">
				<asp:CheckBox ID="chkDebug" runat="server" />
			</div>
			<br class="clear" />
			<div class="size-4 column">
				Diagnostics Access:
			</div>
			<div class="size-5 column">
				<asp:CheckBox ID="chkDiag" runat="server" />
			</div>
			<br class="clear" />
		</div>
		<div class="size-4 column">
			&nbsp;
		</div>
		<div class="size-5 column">
			<asp:LinkButton ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" Text="Update User" CssClass="submits" />
		</div>
		<br class="clear" />
		<div id="management" runat="server" visible="false">
			<h4>Group Management:</h4>
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
					<asp:BoundField DataField="groupImage" HeaderText="Image" SortExpression="groupImage" ItemStyle-CssClass="width_200 mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
				</Columns>
				<EmptyDataTemplate>
					No Groups Found
				</EmptyDataTemplate>
			</asp:GridView>
		</div>
	</div>
	<!-- History Page ==================================================
=========================================================================-->
	<div id="historypage" runat="server" visible="false">
		<script type="text/javascript">
			$(document).ready(function () {
				$('#historyoption').addClass("nav-current")
			});
		</script>
		<div class="size-4 column" style="float: right; margin: 0px;">
			<asp:DropDownList ID="ddlLimit" runat="server" CssClass="ddlist" Style="width: 75px; float: right;" AutoPostBack="true" OnSelectedIndexChanged="ddlLimit_SelectedIndexChanged">
				<asp:ListItem>10</asp:ListItem>
				<asp:ListItem>25</asp:ListItem>
				<asp:ListItem>50</asp:ListItem>
				<asp:ListItem>100</asp:ListItem>
				<asp:ListItem>All</asp:ListItem>
			</asp:DropDownList>
		</div>
		<asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="False" CssClass="Gridview" AlternatingRowStyle-CssClass="alt">
			<Columns>
				<asp:BoundField DataField="_historyevent" HeaderText="Event" ItemStyle-CssClass="width_200"></asp:BoundField>
				<asp:BoundField DataField="_historytypeout" HeaderText="Type" ItemStyle-CssClass="width_200" />
				<asp:BoundField DataField="_historytime" HeaderText="Date" ItemStyle-CssClass="width_200 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
				<asp:BoundField DataField="_historyip" HeaderText="IP" ItemStyle-CssClass="width_200 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
				<asp:BoundField DataField="_historynotes" HeaderText="Notes" ItemStyle-CssClass="width_200 mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small" />
			</Columns>
		</asp:GridView>
	</div>
	<div id="confirmbox" class="confirm-box-outer">
		<div class="confirm-box-inner">
			<h4><asp:Label ID="lblTitle" runat="server" CssClass="modaltitle"></asp:Label></h4>
			<div class="confirm-box-btns">
				<asp:LinkButton ID="OkButton" OnClick="OkButton_Click" runat="server" Text="Yes" CssClass="confirm_yes" />
				<asp:LinkButton ID="CancelButton" runat="server" Text="No" CssClass="confirm_no" />
			</div>
		</div>
	</div>
</asp:Content>
