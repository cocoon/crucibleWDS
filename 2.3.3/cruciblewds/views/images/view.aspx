<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="modifyimages" CodeFile="view.aspx.cs" %>

<%@ MasterType VirtualPath="~/views/master/cruciblewds.master" %>
<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="Server">
	<script type="text/javascript">
		$(document).ready(function () {
			$('#nav-images').addClass("nav-current")
		});
	</script>
	<div class="size-1 column">
		<h1 class="remove-bottom icon-image-title">Images</h1>
	</div>
	<div class="size-2 column offset-1">
		<div class="nav-btn-round">
			<a href="<%= ResolveUrl("~/views/images/create.aspx") %>" class="icon create" data-info="Create New Image"></a>
			<a href="<%= ResolveUrl("~/views/images/search.aspx") %>" class="icon search" data-info="Search Images"></a>
			<a href="<%= ResolveUrl("~/views/images/import.aspx") %>" class="icon import" data-info="Import Images"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="SubNav" ContentPlaceHolderID="SubNav" runat="Server">
	<div class="nav-btn-square ">
		<h3 class="remove-bottom">
			<%=image.Name %><asp:Label ID="lblSubNav" runat="server"></asp:Label></h3>
		<a id="editoption" href="<%= ResolveUrl("~/views/images/view.aspx") %>?page=edit&imageid=<%=image.ID%>" class="icon edit" data-info="Edit Image"></a>
		<a id="historyoption" href="<%= ResolveUrl("~/views/images/view.aspx") %>?page=history&imageid=<%=image.ID%>" class="icon history" data-info="View Image History"></a>
        <a id="logoption" href="<%= ResolveUrl("~/views/images/view.aspx") %>?page=specs&imageid=<%=image.ID%>" class="icon logs" data-info="View Image Specs"></a>
		<asp:LinkButton ID="btnDelete" runat="server" CssClass="icon delete" data-info="Delete Image" OnClick="btnDelete_Click"></asp:LinkButton>
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
			Image Name:
		</div>
		<div class="size-5 column">
			<asp:TextBox ID="txtImageName" runat="server" CssClass="textbox"></asp:TextBox>
		</div>
		<br class="clear" />
		
		<div class="size-4 column">
			Image Description:
		</div>
		<div class="size-5 column">
			<asp:TextBox ID="txtImageDesc" runat="server" TextMode="MultiLine" CssClass="descbox"></asp:TextBox>
		</div>
		<br class="clear" />
		<div class="size-4 column">
			Protected:
		</div>
		<div class="size-5 column">
			<asp:CheckBox ID="chkProtected" runat="server" />
		</div>
		<br class="clear" />
		<div class="size-4 column">
			Visible In On Demand:
		</div>
		<div class="size-5 column">
			<asp:CheckBox ID="chkVisible" runat="server" />
		</div>
		<br class="clear" />
		<div class="size-4 column">
			&nbsp;
		</div>
		<div class="size-5 column">
			<asp:LinkButton ID="btnUpdateImage" runat="server" OnClick="btnUpdateImage_Click" Text="Update Image" CssClass="submits" />
		</div>
		<br class="clear" />
		<div class="size-5 column" style="margin-left:-15px;">
		<h3>Image Directory Status:</h3>
            </div>
		<br class="clear" />
        <div class="size-5 column">
		<asp:Label ID="lblImageHold" runat="server"></asp:Label><br />
		<asp:Label ID="lblImage" runat="server"></asp:Label><br />
		<asp:Label ID="lblImageHoldStatus" runat="server"></asp:Label><br />
		<asp:Label ID="lblImageStatus" runat="server"></asp:Label>
		<br class="clear" />

		<div class="size-5 column">
			<asp:LinkButton ID="btnFixImage" runat="server" OnClick="btnFixImage_Click" Text="Fix Image Directories" CssClass="submits" />
		</div>
		<br class="clear" />
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
				<asp:BoundField DataField="_historyuser" HeaderText="User" ItemStyle-CssClass="width_200" />
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
    	<!-- Specifications Page ========================================================
=========================================================================-->
	<div id="specspage" runat="server" visible="false">
		<script type="text/javascript">
		    $(document).ready(function () {
		        $('#logoption').addClass("nav-current")
		    });
		</script>
           
        <asp:Label ID="lblSpecsUnavailable" Visible="false" runat="server"></asp:Label>

         <br class="clear" />
        <div class="column size-1" style="border:1px solid red; padding:5px;" id="incorrectChecksum" runat="server" visible="false">
        <p>The Image Checksum Does Not Match What Was Previously Reported.  If You Have Recently Uploaded This Image, You Must Confirm It Before It Can Be Deployed.  Otherwise, It May Have Been Tampered With and Should Be Deleted.</p>
        <asp:LinkButton ID="btnConfirmChecksum" runat="server" OnClick="btnConfirmChecksum_Click" Text="Confirm" CssClass="submits" />
        <br class="clear" />
        </div>
         <br class="clear" />
        <asp:GridView ID="gvHDs" runat="server" AutoGenerateColumns="false" CssClass="Gridview" AlternatingRowStyle-CssClass="alt">
            <Columns>

                <asp:TemplateField ShowHeader="False" ItemStyle-CssClass="width_30 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller">
                    <ItemTemplate>
                        <div style="width: 0px">
                            <asp:LinkButton ID="btnParts" runat="server" CausesValidation="false" CommandName="" Text="+" OnClick="btnParts_Click"></asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:HiddenField ID="HiddenActive" runat="server" Value='<%# Bind("active") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-CssClass="width_50" HeaderText="Active">
                    <ItemTemplate>
                        <asp:CheckBox ID="chkHDActive" runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>


                <asp:BoundField DataField="name" HeaderText="Name" ItemStyle-CssClass="width_100"></asp:BoundField>
                <asp:BoundField DataField="size" HeaderText="Size(Reported / Usable)" ItemStyle-CssClass="width_200"></asp:BoundField>
                <asp:BoundField DataField="table" HeaderText="Table" ItemStyle-CssClass="width_100"></asp:BoundField>
                <asp:BoundField DataField="boot" HeaderText="Boot Flag" ItemStyle-CssClass="width_100"></asp:BoundField>
                <asp:BoundField DataField="lbs" HeaderText="LBS" ItemStyle-CssClass="width_100"></asp:BoundField>
                <asp:BoundField DataField="pbs" HeaderText="PBS" ItemStyle-CssClass="width_100"></asp:BoundField>
                <asp:BoundField DataField="guid" HeaderText="GUID" ItemStyle-CssClass="width_100"></asp:BoundField>

                <asp:TemplateField>
                    <ItemTemplate>
                        <tr>
                            <td id="tdParts" runat="server" visible="false" colspan="900">
                                <asp:GridView ID="gvParts" AutoGenerateColumns="false" runat="server" CssClass="Gridview gv_parts" ShowHeader="true" Visible="false" AlternatingRowStyle-CssClass="alt">
                                    <Columns>

                                        <asp:TemplateField ShowHeader="False" ItemStyle-CssClass="width_30 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller">
                                            <ItemTemplate>
                                                <div style="width: 20px">
                                                    <asp:LinkButton ID="partClick" runat="server" CausesValidation="false" CommandName="" Text="+" OnClick="btnPart_Click"></asp:LinkButton>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:HiddenField ID="HiddenActivePart" runat="server" Value='<%# Bind("active") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-CssClass="width_50" HeaderText="Active">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkPartActive" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="number" HeaderText="#" ItemStyle-CssClass="width_100"></asp:BoundField>
                                        <asp:BoundField DataField="start" HeaderText="Start" ItemStyle-CssClass="width_100"></asp:BoundField>
                                        <asp:BoundField DataField="end" HeaderText="End" ItemStyle-CssClass="width_100"></asp:BoundField>
                                        <asp:BoundField DataField="size" HeaderText="Size" ItemStyle-CssClass="width_100"></asp:BoundField>
                                        <asp:BoundField DataField="resize" HeaderText="Resize" ItemStyle-CssClass="width_100"></asp:BoundField>
                                        <asp:BoundField DataField="type" HeaderText="Type" ItemStyle-CssClass="width_100"></asp:BoundField>
                                        <asp:BoundField DataField="fstype" HeaderText="FS" ItemStyle-CssClass="width_100"></asp:BoundField>
                                        <asp:BoundField DataField="fsid" HeaderText="FSID" ItemStyle-CssClass="width_105"></asp:BoundField>
                                        <asp:BoundField DataField="used_mb" HeaderText="Used" ItemStyle-CssClass="width_100"></asp:BoundField>
                                        <asp:TemplateField ItemStyle-CssClass="width_100" HeaderText="Custom Size (MB)">
                                            <ItemTemplate>
                                                <div id="settings">
                                                <asp:TextBox ID="txtCustomSize" runat="server" Text='<%# Bind("size_override") %>' CssClass="textbox_specs"/>
                                                    </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                </td>
                                                <tr>
                                                    <td></td>
                                                    <td></td>
                                                    <td></td>
                                                    <td></td>
                                                    <td></td>
                                                    <td></td>
                                                    <td></td>
                                                    <td></td>
                                                    <td></td>
                                                    <td></td>
                                                    <td></td>

                                                    <td>
                                                        <asp:Label ID="Label1" runat="server" Text="UUID" Font-Bold="true" />
                                                        <asp:Label ID="lblUUID" runat="server" Text='<%# Bind("uuid") %>' />

                                                    </td>
                                                    <td>
                                                        <asp:Label ID="Label2" runat="server" Text="GUID" Font-Bold="true" />
                                                        <asp:Label ID="lblGUID" runat="server" Text='<%# Bind("guid") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <tr>
                                                    <td id="tdVG" runat="server" visible="false" colspan="900">
                                                        <h4><asp:Label ID="LVM" runat="server" Text="Volume Group" style="margin-left:30px;"></asp:Label></h4>
                                                        <asp:GridView ID="gvVG" AutoGenerateColumns="false" runat="server" CssClass="Gridview gv_vg" ShowHeader="true" Visible="false" AlternatingRowStyle-CssClass="alt">
                                                            <Columns>
                                                                                                                                <asp:TemplateField ShowHeader="False" ItemStyle-CssClass="width_30 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller">
                                                                    <ItemTemplate>
                                                                        <div style="width: 20px">
                                                                            <asp:LinkButton ID="vgClick" runat="server" CausesValidation="false" CommandName="" Text="+" OnClick="btnVG_Click"></asp:LinkButton>
                                                                        </div>
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                 <asp:BoundField DataField="name" HeaderText="Name" ItemStyle-CssClass="width_100" />
                                                                <asp:BoundField DataField="pv" HeaderText="PV" ItemStyle-CssClass="width_200" />
                                                                <asp:BoundField DataField="uuid" HeaderText="UUID" ItemStyle-CssClass="width_200" />

                                                                <asp:TemplateField>
                                                                    <ItemTemplate>
                                                                        <tr>
                                                                            <td id="tdLVS" runat="server" visible="false" colspan="900">
                                                                                <asp:GridView ID="gvLVS" AutoGenerateColumns="false" runat="server" CssClass="Gridview gv_parts" ShowHeader="true" Visible="false" AlternatingRowStyle-CssClass="alt">
                                                                                    <Columns>
                                                                                        <asp:TemplateField>
                                                                                            <ItemTemplate>
                                                                                                <asp:HiddenField ID="HiddenActivePart" runat="server" Value='<%# Bind("active") %>' />
                                                                                            </ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                        <asp:TemplateField ItemStyle-CssClass="width_50" HeaderText="Active">
                                                                                            <ItemTemplate>
                                                                                                <asp:CheckBox ID="chkPartActive" runat="server" />
                                                                                            </ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                        <asp:BoundField DataField="name" HeaderText="Name" ItemStyle-CssClass="width_100"></asp:BoundField>
                                                                                     
                                                                                        <asp:BoundField DataField="size" HeaderText="Size" ItemStyle-CssClass="width_100"></asp:BoundField>
                                                                                        <asp:BoundField DataField="resize" HeaderText="Resize" ItemStyle-CssClass="width_100"></asp:BoundField>
                                                                   
                                                                                        <asp:BoundField DataField="fstype" HeaderText="FS" ItemStyle-CssClass="width_100"></asp:BoundField>
                                                                                        <asp:BoundField DataField="uuid" HeaderText="UUID" ItemStyle-CssClass="width_100"></asp:BoundField>

                                                                                        <asp:BoundField DataField="used_mb" HeaderText="Used" ItemStyle-CssClass="width_100"></asp:BoundField>

                                                                                        <asp:TemplateField ItemStyle-CssClass="width_100" HeaderText="Custom Size (MB)">
                                                                                            <ItemTemplate>
                                                                                                <div id="settings">
                                                                                                    <asp:TextBox ID="txtCustomSize" runat="server" Text='<%# Bind("size_override") %>' CssClass="textbox_specs" />
                                                                                                </div>
                                                                                            </ItemTemplate>
                                                                                        </asp:TemplateField>

                                                                                    </Columns>
                                                                                </asp:GridView>
                                                                            </td>
                                                                        </tr>
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                
                                                                 </Columns>


                                                        </asp:GridView>
                                                    </td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <tr>
                                                    <td id="tdFile" runat="server" visible="false" colspan="900">
                                                        <asp:GridView ID="gvFiles" AutoGenerateColumns="false" runat="server" CssClass="Gridview gv_parts" ShowHeader="true" Visible="false" AlternatingRowStyle-CssClass="alt">
                                                            <Columns>

                                                                <asp:BoundField DataField="fileName" HeaderText="File Name" ItemStyle-CssClass="width_100" />
                                                                <asp:BoundField DataField="serverSize" HeaderText="Server Size" ItemStyle-CssClass="width_200" />

                                                            </Columns>
                                                        </asp:GridView>
                                                    </td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>

                                
                            </td>


                        </tr>
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>
             <div class="full column">
			<asp:LinkButton ID="btnUpdateSpecs" runat="server" OnClick="btnUpdateImageSpecs_Click" Text="Update Image Specs" CssClass="submits" />
                    <br class="clear" />
            <asp:LinkButton ID="btnRestoreSpecs" runat="server" OnClick="btnRestoreImageSpecs_Click" Text="Restore Image Specs" CssClass="submits" />
		</div>
		<br class="clear" />
    </div>
</asp:Content>
