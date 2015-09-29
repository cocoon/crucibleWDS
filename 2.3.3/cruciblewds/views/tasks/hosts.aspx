<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="unicast" CodeFile="hosts.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/tasks/hosts.aspx") %>" class="icon unicast nav-current" data-info="Start Unicast Session"></a>
			<a href="<%= ResolveUrl("~/views/tasks/groups.aspx") %>" class="icon multicast" data-info="Start Multicast Session"></a>
			<a href="<%= ResolveUrl("~/views/tasks/ondemand.aspx") %>" class="icon ond" data-info="Start On Demand Session"></a>
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
	<asp:GridView ID="gvHosts" runat="server" AutoGenerateColumns="False" DataKeyNames="hostID" CssClass="Gridview" AllowSorting="True" OnSorting="gridView_Sorting" AlternatingRowStyle-CssClass="alt">
		<Columns>
			<asp:BoundField DataField="hostID" HeaderText="hostID" InsertVisible="False" ReadOnly="True" SortExpression="hostID" Visible="False" />
			<asp:BoundField DataField="hostName" HeaderText="Name" SortExpression="hostName" ItemStyle-CssClass="width_200" />
			<asp:BoundField DataField="hostImage" HeaderText="Image" SortExpression="hostImage" ItemStyle-CssClass="width_200 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
			<asp:BoundField DataField="hostGroup" HeaderText="Group" SortExpression="hostGroup" ItemStyle-CssClass="width_200 mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small" />
			<asp:TemplateField>
				<ItemTemplate>
					<asp:LinkButton ID="btnDeploy" runat="server" OnClick="btnDeploy_Click" Text="Deploy" />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField>
				<ItemTemplate>
					<asp:LinkButton ID="btnUpload" runat="server" OnClick="btnUpload_Click" Text="Upload" />
				</ItemTemplate>
			</asp:TemplateField>
		</Columns>
		<EmptyDataTemplate>
			No Hosts Found
		</EmptyDataTemplate>
	</asp:GridView>
	<div id="confirmbox" class="confirm-box-outer">
		<div class="confirm-box-inner">
			<h4><asp:Label ID="lblTitle" runat="server" CssClass="modaltitle"></asp:Label></h4>
			<asp:GridView ID="gvConfirm" runat="server" CssClass="Gridview gv-confirm " AutoGenerateColumns="false">
				<Columns>
					<asp:BoundField DataField="_hostname" HeaderText="Name" />
					<asp:BoundField DataField="_hostmac" HeaderText="MAC" ItemStyle-CssClass="mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
					<asp:BoundField DataField="_hostimage" HeaderText="Image" />
				</Columns>
			</asp:GridView>
			<div class="confirm-box-btns">
				<asp:LinkButton ID="OkButton" OnClick="OkButton_Click" runat="server" Text="Yes" CssClass="confirm_yes" />
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
