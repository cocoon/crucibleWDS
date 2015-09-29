<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="custom" CodeFile="ondemand.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/tasks/groups.aspx") %>" class="icon multicast" data-info="Start Multicast Session"></a>
			<a href="<%= ResolveUrl("~/views/tasks/ondemand.aspx") %>" class="icon ond nav-current" data-info="Start On Demand Session"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<asp:Label ID="secureMsg" runat="server" Visible="false"></asp:Label>
	<div id="secure" runat="server" visible="false">
		<div class="size-4 column">
			<asp:DropDownList ID="ddlImage" runat="server" CssClass="ddlist">
			</asp:DropDownList>
			<asp:LinkButton ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" Text="Start Multicast" CssClass="submits" />
		</div>
	</div>
</asp:Content>
