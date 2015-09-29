<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="modifyhosts" CodeFile="view.aspx.cs" %>

<%@ MasterType VirtualPath="~/views/master/cruciblewds.master" %>
<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="Server">
	<!-- Page Header ========================================================
=========================================================================-->
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
			<a href="<%= ResolveUrl("~/views/hosts/search.aspx") %>" class="icon search" data-info="Search Hosts"></a>
			<a href="<%= ResolveUrl("~/views/hosts/import.aspx") %>" class="icon import" data-info="Import Hosts"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="SubNav" ContentPlaceHolderID="SubNav" runat="Server">
	<!-- Page Sub Navigation ================================================
=========================================================================-->
	<div class="nav-btn-square ">
		<h3 class="remove-bottom">
			<%=host.Name %><asp:Label ID="lblSubNav" runat="server"></asp:Label></h3>
		<a id="editoption" href="<%= ResolveUrl("~/views/hosts/view.aspx") %>?page=edit&hostid=<%=host.ID%>" class="icon edit" data-info="Edit Host"></a>
		<a id="historyoption" href="<%= ResolveUrl("~/views/hosts/view.aspx") %>?page=history&hostid=<%=host.ID%>" class="icon history" data-info="View Host History"></a>
		<a id="bootmenuoption" href="<%= ResolveUrl("~/views/hosts/view.aspx") %>?page=activebootmenu&hostid=<%=host.ID%>" class="icon boot" data-info="Modify Host Boot Menu"></a>
		<a id="logoption" href="<%= ResolveUrl("~/views/hosts/view.aspx") %>?page=log&hostid=<%=host.ID%>" class="icon logs" data-info="View Host Log"></a>
		<asp:LinkButton ID="btnDeploy" runat="server" CssClass="icon upload" data-info="Upload Host" OnClick="btnUpload_Click"></asp:LinkButton>
		<asp:LinkButton ID="btnUpload" runat="server" CssClass="icon deploy" data-info="Deploy Host" OnClick="btnDeploy_Click"></asp:LinkButton>
		<asp:LinkButton ID="btnDelete" runat="server" CssClass="icon delete" data-info="Delete Host" OnClick="btnDelete_Click"></asp:LinkButton>
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
			<asp:DropDownList ID="ddlHostImage" runat="server" CssClass="ddlist" />
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
			&nbsp;
		</div>
		<div class="size-5 column">
			<asp:LinkButton ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" Text="Update Host" CssClass="submits" />
		</div>
	</div>
	<!-- History Page ==================================================
=========================================================================-->
	<div id="history" runat="server" visible="false">
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
	<!-- Active Boot Menu Page ==============================================
=========================================================================-->
	<div id="activebootmenu" runat="server" visible="false">
		<script type="text/javascript">
			$(document).ready(function () {
				$('#bootmenuoption').addClass("nav-current")
			});
		</script>
		<a href="<%= ResolveUrl("~/views/hosts/view.aspx") %>?page=custombootmenu&hostid=<%=host.ID%>" class="submits static-width-nomarg">Custom</a>
		<a href="<%= ResolveUrl("~/views/hosts/view.aspx") %>?page=activebootmenu&hostid=<%=host.ID%>" class="submits static-width-nomarg boot-active">Active</a>
		<br class="clear" />
         <div id="divProxy" runat="server" visible="false">

			
        <div class="size-6 column">
            Select A Menu:
        </div>
        <br class="clear" />
		<div class="size-4 column">
			<asp:DropDownList ID="ddlEditProxyType" runat="server" CssClass="ddlist" OnSelectedIndexChanged="EditProxy_Changed" AutoPostBack="true">
				<asp:ListItem>bios</asp:ListItem>
				<asp:ListItem>efi32</asp:ListItem>
				<asp:ListItem>efi64</asp:ListItem>
			</asp:DropDownList>
		</div>
		<br class="clear" />
            </div>
		<asp:Label ID="lblActiveBoot" runat="server"></asp:Label> <asp:Label ID="lblFileName1" runat="server"></asp:Label>
		<asp:TextBox ID="txtBootMenu" runat="server" CssClass="descboxboot" Style="font-size: 12px;" TextMode="MultiLine"></asp:TextBox>
		<div id="bootmenu2" runat="server" visible="false">
			<asp:Label ID="lblFileName2" runat="server"></asp:Label>
			<asp:TextBox ID="txtBootMenu2" runat="server" CssClass="descboxboot" Style="font-size: 12px;" TextMode="MultiLine"></asp:TextBox>
		</div>
	</div>
	<br class="clear" />
	<!-- Custom Boot Menu Page ==============================================
=========================================================================-->
	<div id="custombootmenu" runat="server" visible="false">
		<script type="text/javascript">
			$(document).ready(function () {
				$('#bootmenuoption').addClass("nav-current")
			});
		</script>
		<a href="<%= ResolveUrl("~/views/hosts/view.aspx") %>?page=custombootmenu&hostid=<%=host.ID%>" class="submits static-width boot-active">Custom</a>
		<a href="<%= ResolveUrl("~/views/hosts/view.aspx") %>?page=activebootmenu&hostid=<%=host.ID%>" class="submits static-width">Active</a>
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
	<!-- Logs Page ========================================================
=========================================================================-->
	<div id="logs" runat="server" visible="false">
		<script type="text/javascript">
			$(document).ready(function () {
				$('#logoption').addClass("nav-current")
			});
		</script>
		<div class="size-4 column" style="float: right; margin: 0px;">
			<asp:DropDownList ID="ddlLogType" runat="server" CssClass="ddlist" Style="width: 200px; float: right;" AutoPostBack="true" OnSelectedIndexChanged="ddlLogLimit_SelectedIndexChanged">
				<asp:ListItem>Select A Log</asp:ListItem>
				<asp:ListItem>Upload</asp:ListItem>
				<asp:ListItem>Deploy</asp:ListItem>
			</asp:DropDownList>
			<br class="clear" />
			<asp:DropDownList ID="ddlLogLimit" runat="server" CssClass="ddlist" Style="width: 75px; float: right;" AutoPostBack="true" OnSelectedIndexChanged="ddlLogLimit_SelectedIndexChanged">
				<asp:ListItem>10</asp:ListItem>
				<asp:ListItem>25</asp:ListItem>
				<asp:ListItem>75</asp:ListItem>
				<asp:ListItem>All</asp:ListItem>
			</asp:DropDownList>
            <br class="clear" />
            <asp:LinkButton ID="btnExportLog" runat="server" Text="Export Log" CssClass="submits" OnClick="btnExportLog_Click"></asp:LinkButton>
		</div>
        <br class="clear" />
		<asp:GridView ID="gvHostLog" runat="server" CssClass="Gridview log" ShowHeader="false">
		</asp:GridView>
	</div>
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
