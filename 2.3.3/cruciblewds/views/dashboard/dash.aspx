<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="management" CodeFile="dash.aspx.cs" %>

<%@ MasterType VirtualPath="~/views/master/cruciblewds.master" %>
<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="Server">
	<div class="test" style="float: right; text-align: right; margin-top: 40px;">
		<h4>CrucibleWDS 2.3.3</h4>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<p class="denied_text">
		<asp:Label ID="lblDenied" runat="server"></asp:Label></p>
</asp:Content>
