<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="modifygroups" CodeFile="view.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/groups/search.aspx") %>" class="icon search" data-info="Search Groups"></a>
			<a href="<%= ResolveUrl("~/views/groups/import.aspx") %>" class="icon import" data-info="Import Groups"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="SubNav" ContentPlaceHolderID="SubNav" runat="Server">
	<div class="nav-btn-square ">
		<h3 class="remove-bottom">
			<%=group.Name %><asp:Label ID="lblSubNav" runat="server"></asp:Label></h3>
		<a id="editoption" href="<%= ResolveUrl("~/views/groups/view.aspx") %>?page=edit&groupid=<%=group.ID%>" class="icon edit" data-info="Edit Group"></a>
		<a id="historyoption" href="<%= ResolveUrl("~/views/groups/view.aspx") %>?page=history&groupid=<%=group.ID%>" class="icon history" data-info="View Group History"></a>
		<a id="bootmenuoption" href="<%= ResolveUrl("~/views/groups/view.aspx") %>?page=custombootmenu&groupid=<%=group.ID%>" class="icon boot" data-info="Modify Group Boot Menu"></a>
		<asp:LinkButton ID="btnMulticast" runat="server" CssClass="icon multicast" data-info="Multicast Group" OnClick="btnMulticast_Click"></asp:LinkButton>
		<asp:LinkButton ID="btnUnicast" runat="server" CssClass="icon unicast" data-info="Unicast Group" OnClick="btnUnicast_Click"></asp:LinkButton>
		<asp:LinkButton ID="btnDelete" runat="server" CssClass="icon delete" data-info="Delete Group" OnClick="btnDelete_Click"></asp:LinkButton>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<!-- Edit Page =====================================================
=========================================================================-->
	<div id="edit" runat="server" visible="false">
		<script type="text/javascript">
			$(document).ready(function () {
				$('#editoption').addClass("nav-current")
			});
		</script>
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
			<asp:LinkButton ID="btnSubmit" runat="server" Text="Update Group" OnClick="btnSubmit_Click" CssClass="submits" />
		</div>
		<br class="clear" />
		<hr />
		<h5 style="text-align: left">
			Current Members - Select To Remove</h5>
		<asp:GridView ID="gvRemove" runat="server" AutoGenerateColumns="False" CssClass="Gridview" AlternatingRowStyle-CssClass="alt" DataKeyNames="hostID">
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
				<asp:BoundField DataField="hostID" HeaderText="hostID" InsertVisible="False" SortExpression="hostID" Visible="False" />
				<asp:BoundField DataField="hostName" HeaderText="Name" SortExpression="hostName" ItemStyle-CssClass="width_200" />
				<asp:BoundField DataField="hostMac" HeaderText="MAC" SortExpression="hostMac" ItemStyle-CssClass="mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
				<asp:BoundField DataField="hostGroup" HeaderText="Group" SortExpression="hostGroup" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
			</Columns>
			<EmptyDataTemplate>
				No Hosts Found
			</EmptyDataTemplate>
		</asp:GridView>
		<h5 style="text-align: left">
			New Members - Select To Add</h5>
		<div class="size-7 column">
			<asp:TextBox ID="txtSearchHosts" runat="server" CssClass="searchbox label-host-search" AutoPostBack="True" OnTextChanged="txtSearchHosts_TextChanged"></asp:TextBox>
		</div>
		<br class="clear" />
		<p class="total">
			<asp:Label ID="lblTotal" runat="server"></asp:Label></p>
		<asp:GridView ID="gvAdd" runat="server" AutoGenerateColumns="False" CssClass="Gridview" AlternatingRowStyle-CssClass="alt" DataKeyNames="hostID">
			<Columns>
				<asp:TemplateField>
					<HeaderStyle CssClass="chkboxwidth"></HeaderStyle>
					<ItemStyle CssClass="chkboxwidth"></ItemStyle>
					<HeaderTemplate>
						<asp:CheckBox ID="chkSelectAll" runat="server" AutoPostBack="True" OnCheckedChanged="SelectAllAdd_CheckedChanged" />
					</HeaderTemplate>
					<ItemTemplate>
						<asp:CheckBox ID="chkSelector" runat="server" />
					</ItemTemplate>
				</asp:TemplateField>
				<asp:BoundField DataField="hostID" HeaderText="hostID" SortExpression="hostID" InsertVisible="False" Visible="False" />
				<asp:BoundField DataField="hostName" HeaderText="Name" SortExpression="hostName" ItemStyle-CssClass="width_200" />
				<asp:BoundField DataField="hostMac" HeaderText="MAC" SortExpression="hostMac" ItemStyle-CssClass="mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
				<asp:BoundField DataField="hostGroup" HeaderText="Group" SortExpression="hostGroup" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
			</Columns>
			<EmptyDataTemplate>
				No Hosts Found
			</EmptyDataTemplate>
		</asp:GridView>
		<div class="row">
		</div>
	</div>
	<!-- History Page =====================================================
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
				<asp:BoundField DataField="_historyuser" HeaderText="User" ItemStyle-CssClass="width_200" />
				<asp:BoundField DataField="_historytime" HeaderText="Date" ItemStyle-CssClass="width_200 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
				<asp:BoundField DataField="_historyip" HeaderText="IP" ItemStyle-CssClass="width_200 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
				<asp:BoundField DataField="_historynotes" HeaderText="Notes" ItemStyle-CssClass="width_200 mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small" />
			</Columns>
		</asp:GridView>
	</div>
	<!-- Custom Boot Menu Page ==============================================
=========================================================================-->
	<div id="custombootmenu" runat="server" visible="false">
		<script type="text/javascript">
			$(document).ready(function () {
				$('#bootmenuoption').addClass("nav-current")
			});
		</script>
		<a href="<%= ResolveUrl("~/views/groups/view.aspx") %>?page=custombootmenu&groupid=<%=group.ID%>" class="submits static-width boot-active">Custom</a>
		<br class="clear" />
		<div class="size-4 column" style="float: right; margin: 0;">
			<asp:DropDownList ID="ddlTemplate" runat="server" CssClass="ddlist" Style="float: right; width: 200px; margin-right: 5px; margin-top: 5px;" OnSelectedIndexChanged="ddlTemplate_SelectedIndexChanged" AutoPostBack="true">
			</asp:DropDownList>
		</div>
		<br class="clear" />
		<div class="size-8 column">
			<asp:LinkButton ID="btnSetBootMenu" runat="server" Text="Set" OnClick="btnSetBootMenu_Click" CssClass="submits static-width" Style="float: left;" />
		</div>
		<div class="size-8 column">
			<asp:LinkButton ID="btnRemoveBootMenu" runat="server" Text="Remove" OnClick="btnRemoveBootMenu_Click" CssClass="submits static-width" Style="float: left;" />
		</div>
		<br class="clear" />
		<asp:TextBox ID="txtCustomBootMenu" runat="server" CssClass="descboxboot" Style="font-size: 12px;" TextMode="MultiLine"></asp:TextBox>
	</div>
	<br class="clear" />
	<div id="confirmbox" class="confirm-box-outer">
		<div class="confirm-box-inner">
			<h4><asp:Label ID="lblTitle" runat="server" CssClass="modaltitle"></asp:Label></h4>
			<asp:GridView ID="gvConfirm" runat="server" CssClass="Gridview gv-confirm" AutoGenerateColumns="false">
				<Columns>
					<asp:BoundField DataField="_groupname" HeaderText="Name" ItemStyle-CssClass="width_200" />
					<asp:BoundField DataField="_groupimage" HeaderText="Image" ItemStyle-CssClass="width_200" />
				</Columns>
			</asp:GridView>
			<div class="confirm-box-btns">
				<asp:LinkButton ID="OkButton" OnClick="btnConfirm_Click" runat="server" Text="Yes" CssClass="confirm_yes" />
				<asp:LinkButton ID="CancelButton" runat="server" Text="No" CssClass="confirm_no" />
			</div>
		</div>
	</div>
     <div id="incorrectChecksum" class="confirm-box-outer">
		<div class="confirm-box-inner">
			<h4><asp:Label ID="lblIncorrectChecksum" runat="server" CssClass="modaltitle"></asp:Label></h4>
		
			<div class="confirm-box-btns">
				<asp:LinkButton ID="LinkButton1" OnClick="OkButtonChecksum_Click" runat="server" Text="Yes" CssClass="confirm_yes" />
				<asp:LinkButton ID="LinkButton2" runat="server" Text="No" CssClass="confirm_no" />
			</div>
			
		</div>
       </div>
</asp:Content>
