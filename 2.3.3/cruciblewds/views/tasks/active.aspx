<%@ Page Title="" Language="C#" MasterPageFile="~/views/master/cruciblewds.master" AutoEventWireup="true" Inherits="active" CodeFile="active.aspx.cs" %>

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
			<a href="<%= ResolveUrl("~/views/tasks/active.aspx") %>" class="icon active nav-current" data-info="View Active Tasks"></a>
			<a href="<%= ResolveUrl("~/views/tasks/hosts.aspx") %>" class="icon unicast" data-info="Start Unicast Session"></a>
			<a href="<%= ResolveUrl("~/views/tasks/groups.aspx") %>" class="icon multicast" data-info="Start Multicast Session"></a>
			<a href="<%= ResolveUrl("~/views/tasks/ondemand.aspx") %>" class="icon ond" data-info="Start On Demand Session"></a>
		</div>
	</div>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="Server">
	<asp:ScriptManager ID="ScriptManager1" runat="server">
	</asp:ScriptManager>
	<asp:UpdatePanel ID="UpdatePanel" runat="server">
		<ContentTemplate>
			<asp:Timer ID="TimerMC" runat="server" Interval="2000" OnTick="TimerMC_Tick">
			</asp:Timer>
			<h2>Multicasts</h2>
			<asp:GridView ID="gvMcTasks" runat="server" CssClass="Gridview" AutoGenerateColumns="False" DataKeyNames="mcTaskID" AlternatingRowStyle-CssClass="alt">
				<Columns>
					<asp:BoundField DataField="mcTaskID" HeaderText="mcTaskID" InsertVisible="False" ReadOnly="True" SortExpression="mcTaskID" Visible="False" />
					<asp:TemplateField ShowHeader="False" ItemStyle-CssClass="width_30">
						<ItemTemplate>
							<div style="width: 0px">
								<asp:LinkButton ID="btnMembers" runat="server" CausesValidation="false" CommandName="" Text="+" OnClick="btnMembers_Click"></asp:LinkButton>
							</div>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="mcTaskName" HeaderText="Name" SortExpression="mcTaskName" />
					<asp:BoundField DataField="mcPID" HeaderText="PID" SortExpression="mcPID" ItemStyle-CssClass="mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
					<asp:TemplateField ShowHeader="True" HeaderText="Partition" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller">
						<ItemTemplate>
							<asp:Label ID="lblPartition" runat="server"></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField ShowHeader="True" HeaderText="Elapsed" ItemStyle-CssClass="mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small">
						<ItemTemplate>
							<asp:Label ID="lblElapsed" runat="server"></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField ShowHeader="True" HeaderText="Remaining" ItemStyle-CssClass="mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small">
						<ItemTemplate>
							<asp:Label ID="lblRemaining" runat="server"></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField ShowHeader="True" HeaderText="Completed">
						<ItemTemplate>
							<asp:Label ID="lblCompleted" runat="server"></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField ShowHeader="True" HeaderText="Rate" ItemStyle-CssClass="mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest">
						<ItemTemplate>
							<asp:Label ID="lblRate" runat="server"></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField ShowHeader="False">
						<ItemTemplate>
							<asp:LinkButton ID="btnCancelMc" runat="server" CausesValidation="false" CommandName="" Text="Cancel" OnClick="btnCancelMc_Click"></asp:LinkButton>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField>
						<ItemTemplate>
							<tr>
								<td runat="server" id="tdMembers" colspan="700" visible="false">
									<asp:GridView ID="gvMembers" AutoGenerateColumns="false" runat="server" CssClass="Gridview gv_members" ShowHeader="false" Visible="false" AlternatingRowStyle-CssClass="alt">
										<Columns>
											<asp:BoundField DataField="_taskName" HeaderText="Name" SortExpression="_taskName" ItemStyle-CssClass="width_150" />
											<asp:BoundField DataField="_taskStatus" HeaderText="Status" SortExpression="_taskStatus" ItemStyle-CssClass="width_50" />
											<asp:BoundField DataField="_taskPartition" HeaderText="Partition" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
											<asp:BoundField DataField="_taskElapsed" HeaderText="Elapsed" ItemStyle-CssClass="mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small" />
											<asp:BoundField DataField="_taskRemaining" HeaderText="Remaining" ItemStyle-CssClass="mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small" />
											<asp:BoundField DataField="_taskCompleted" HeaderText="Completed" />
											<asp:BoundField DataField="_taskRate" HeaderText="Rate" ItemStyle-CssClass="mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
										</Columns>
									</asp:GridView>
								</td>
							</tr>
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
				<EmptyDataTemplate>
					No Active Multicasts
				</EmptyDataTemplate>
				<HeaderStyle CssClass="taskgridheader"></HeaderStyle>
			</asp:GridView>
		</ContentTemplate>
	</asp:UpdatePanel>
	<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
		<ContentTemplate>
			<asp:Timer ID="Timer" runat="server" Interval="2000" OnTick="Timer_Tick">
			</asp:Timer>
			<h2>Unicasts</h2>
			<asp:GridView ID="gvUcTasks" runat="server" AutoGenerateColumns="False" DataKeyNames="_taskID" CssClass="Gridview" AlternatingRowStyle-CssClass="alt">
				<Columns>
					<asp:BoundField DataField="_taskID" HeaderText="taskID" SortExpression="_taskID" InsertVisible="False" ReadOnly="True" Visible="False" />
					<asp:BoundField DataField="_taskName" HeaderText="Name" SortExpression="_taskName" ItemStyle-CssClass="width_150" />
					<asp:BoundField DataField="_taskStatus" HeaderText="Status" SortExpression="_taskStatus" ItemStyle-CssClass="width_50" />
					<asp:BoundField DataField="_taskPartition" HeaderText="Partition" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
					<asp:BoundField DataField="_taskElapsed" HeaderText="Elapsed" ItemStyle-CssClass="mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small" />
					<asp:BoundField DataField="_taskRemaining" HeaderText="Remaining" ItemStyle-CssClass="mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small" />
					<asp:BoundField DataField="_taskCompleted" HeaderText="Completed" />
					<asp:BoundField DataField="_taskRate" HeaderText="Rate" ItemStyle-CssClass="mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
					<asp:TemplateField ShowHeader="False">
						<ItemTemplate>
							<asp:LinkButton ID="btnCancel" runat="server" CausesValidation="false" CommandName="" Text="Cancel" OnClick="btnCancel_Click"></asp:LinkButton>
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
				<EmptyDataTemplate>
					No Active Unicasts
				</EmptyDataTemplate>
			</asp:GridView>
			<h2>All Tasks</h2>
            <asp:LinkButton ID="btnShowAll" runat="server" CausesValidation="false" CommandName="" Text="Show / Hide" OnClick="btnShowAll_Click" CssClass="submits left" ></asp:LinkButton>
			<br class="clear"/>
            <asp:GridView ID="gvTasks" Visible="false" runat="server" AutoGenerateColumns="False" DataKeyNames="_taskID" CssClass="Gridview" AlternatingRowStyle-CssClass="alt">
				<Columns>
					<asp:BoundField DataField="_taskID" HeaderText="taskID" SortExpression="_taskID" InsertVisible="False" ReadOnly="True" Visible="False" />
					<asp:BoundField DataField="_taskName" HeaderText="Name" SortExpression="_taskName" ItemStyle-CssClass="width_150" />
					<asp:BoundField DataField="_taskStatus" HeaderText="Status" SortExpression="_taskStatus" ItemStyle-CssClass="width_50" />
					<asp:BoundField DataField="_taskPartition" HeaderText="Partition" ItemStyle-CssClass="mobi-hide-smaller" HeaderStyle-CssClass="mobi-hide-smaller" />
					<asp:BoundField DataField="_taskElapsed" HeaderText="Elapsed" ItemStyle-CssClass="mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small" />
					<asp:BoundField DataField="_taskRemaining" HeaderText="Remaining" ItemStyle-CssClass="mobi-hide-small" HeaderStyle-CssClass="mobi-hide-small" />
					<asp:BoundField DataField="_taskCompleted" HeaderText="Completed" />
					<asp:BoundField DataField="_taskRate" HeaderText="Rate" ItemStyle-CssClass="mobi-hide-smallest" HeaderStyle-CssClass="mobi-hide-smallest" />
					<asp:TemplateField ShowHeader="False">
						<ItemTemplate>
							<asp:LinkButton ID="btnCancel" runat="server" CausesValidation="false" CommandName="" Text="Cancel" OnClick="btnCancel_Click"></asp:LinkButton>
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
				<EmptyDataTemplate>
					No Active Tasks
				</EmptyDataTemplate>
			</asp:GridView>
		</ContentTemplate>
	</asp:UpdatePanel>
	<asp:LinkButton ID="cancelTasks" runat="server" Text="Cancel All Tasks" CssClass="submits" OnClick="cancelTasks_Click" />
</asp:Content>
