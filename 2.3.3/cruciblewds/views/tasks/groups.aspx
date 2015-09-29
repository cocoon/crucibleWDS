<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="multicast" CodeFile="groups.aspx.cs" %>

<%@ MasterType VirtualPath="~/views/master/cruciblewds.master" %>
<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="Server">
	<script type="text/javascript">
		$(document).ready(function () {
			$('#nav-tasks').addClass("nav-current")
		});
	</script>
	<div class="size-1 column">
		<h1 class="remove-bottom icon-task-title">Tasks</h1>
	</div>
	<div class="size-2 column offset-1">
		<div class="nav-btn-round">
			<a href="<%= ResolveUrl("~/views/tasks/active.aspx") %>" class="icon active" data-info="View Active Tasks"></a>
			<a href="<%= ResolveUrl("~/views/tasks/hosts.aspx") %>" class="icon unicast" data-info="Start Unicast Session"></a>
			<a href="<%= ResolveUrl("~/views/tasks/groups.aspx") %>" class="icon multicast nav-current" data-info="Start Multicast Session"></a>
			<a href="<%= ResolveUrl("~/views/tasks/ondemand.aspx") %>" class="icon ond" data-info="Start On Demand Session"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<div class="size-7 column">
		<asp:TextBox ID="txtSearch" runat="server" CssClass="searchbox"></asp:TextBox>
	</div>
	<br class="clear" />
	<p class="total">
		<asp:Label ID="lblTotal" runat="server"></asp:Label></p>
	<asp:GridView ID="gvGroups" runat="server" AutoGenerateColumns="False" DataKeyNames="groupID" CssClass="Gridview" AlternatingRowStyle-CssClass="alt">
		<Columns>
			<asp:BoundField DataField="groupID" HeaderText="groupID" InsertVisible="False" ReadOnly="True" SortExpression="groupID" Visible="False" />
			<asp:BoundField DataField="groupName" HeaderText="Name" SortExpression="groupName" ItemStyle-CssClass="width_200" />
			<asp:BoundField DataField="groupImage" HeaderText="Image" SortExpression="groupImage" ItemStyle-CssClass="width_200 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
			<asp:TemplateField ItemStyle-CssClass="width_100">
				<ItemTemplate>
					<asp:LinkButton ID="btnMulticast" runat="server" OnClick="btnMulticast_Click" Text="Multicast" />
					<asp:LinkButton ID="btnUnicast" runat="server" OnClick="btnUnicast_Click" Text="Unicast" />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField ItemStyle-CssClass="width_100">
				<ItemTemplate>
				</ItemTemplate>
			</asp:TemplateField>
		</Columns>
	</asp:GridView>
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
