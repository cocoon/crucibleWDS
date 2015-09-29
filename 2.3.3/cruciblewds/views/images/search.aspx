<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="searchimages" CodeFile="search.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/images/search.aspx") %>" class="icon search nav-current" data-info="Search Images"></a>
			<a href="<%= ResolveUrl("~/views/images/import.aspx") %>" class="icon import" data-info="Import Images"></a>
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
	<asp:GridView ID="gvImages" runat="server" AllowSorting="True" AutoGenerateColumns="False" OnSorting="gridView_Sorting" DataKeyNames="imageID" CssClass="Gridview" AlternatingRowStyle-CssClass="alt">
		<Columns>
              <asp:TemplateField ShowHeader="False" ItemStyle-CssClass="width_30 mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller">
                    <ItemTemplate>
                        <div style="width: 0px">
                            <asp:LinkButton ID="btnHds" runat="server" CausesValidation="false" CommandName="" Text="+" OnClick="btnHds_Click"></asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>

			<asp:TemplateField>
				<HeaderStyle CssClass="chkboxwidth"></HeaderStyle>
				<ItemStyle CssClass="chkboxwidth"></ItemStyle>
				<HeaderTemplate>
					<asp:CheckBox ID="chkSelectAll" runat="server" AutoPostBack="True" OnCheckedChanged="chkSelectAll_CheckedChanged" />
				</HeaderTemplate>
				<ItemTemplate>
					<asp:CheckBox ID="chkSelector" runat="server" />
				</ItemTemplate>
			</asp:TemplateField>
           
			<asp:TemplateField>
				<ItemTemplate>
					<asp:HiddenField ID="HiddenID" runat="server" Value='<%# Bind("imageID") %>' />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:BoundField DataField="imageID" HeaderText="imageID" SortExpression="imageID" Visible="False" />
			<asp:BoundField DataField="imageName" HeaderText="Name" SortExpression="imageName" ItemStyle-CssClass="width_200" />
			<asp:TemplateField ShowHeader="True" HeaderText="Size On Server" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller">
				<ItemTemplate>
					<asp:Label ID="lblSize" runat="server" CausesValidation="false" CssClass="lbl_file"></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField ShowHeader="True" HeaderText="Minimum Client Size" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller">
				<ItemTemplate>
					<asp:Label ID="lblSizeClient" runat="server" CausesValidation="false" CssClass="lbl_file"></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>
			
			<asp:HyperLinkField DataNavigateUrlFields="imageID" DataNavigateUrlFormatString="~/views/images/view.aspx?page=edit&imageid={0}" Text="View" />
			





             <asp:TemplateField>
                    <ItemTemplate>
                        <tr>
                            <td id="tdHds" runat="server" visible="false" colspan="900">
                                <asp:GridView ID="gvHDs" AutoGenerateColumns="false" runat="server" CssClass="Gridview gv_parts hdlist" ShowHeader="false" Visible="false" AlternatingRowStyle-CssClass="alt">
                                    <Columns>
                                        <asp:BoundField DataField="name" HeaderText="#" ItemStyle-CssClass="width_100"></asp:BoundField>
                                        <asp:TemplateField ShowHeader="True" HeaderText="Server Size" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller">
				<ItemTemplate>
					<asp:Label ID="lblHDSize" runat="server" CausesValidation="false" CssClass="lbl_file"></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField ShowHeader="True" HeaderText="Client Size" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller">
				<ItemTemplate>
					<asp:Label ID="lblHDSizeClient" runat="server" CausesValidation="false" CssClass="lbl_file"></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>
                                        
                                    </Columns>
                                </asp:GridView>
                            </td>


                        </tr>
                    </ItemTemplate>
                </asp:TemplateField>




		</Columns>
		<EmptyDataTemplate>
			No Images Found
		</EmptyDataTemplate>
	</asp:GridView>
	<a class="confirm" href="#">Delete Selected Images</a>
	<div id="confirmbox" class="confirm-box-outer">
		<div class="confirm-box-inner">
			<h4><asp:Label ID="lblTitle" runat="server" Text="Delete The Selected Images?" CssClass="modaltitle"></asp:Label></h4>
			<div class="confirm-box-btns">
				<asp:LinkButton ID="ConfirmButton" OnClick="btnSubmit_Click" runat="server" Text="Yes" CssClass="confirm_yes" />
				<asp:LinkButton ID="CancelButton" runat="server" Text="No" CssClass="confirm_no" />
			</div>
		</div>
	</div>
</asp:Content>
