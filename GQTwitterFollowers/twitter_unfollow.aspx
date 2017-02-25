<%@ Page Title="" Language="C#" MasterPageFile="~/Master/Twitter.Master" AutoEventWireup="true" CodeBehind="twitter_unfollow.aspx.cs" Inherits="GQTwitterFollowers.twitter_unfollow" %>
<%@ MasterType virtualPath="~/Master/Twitter.Master"%>
<%@ Register Assembly="GQPagingGridView" Namespace="GQPagingGridView" TagPrefix="geoqual" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="btnPlaceholder" runat="server">
    <script type="text/javascript">
        function CheckAllEmp(Checkbox) {
            var gvUsers = document.getElementById("<%=gvUsers.ClientID %>");
            for (i = 1; i < gvUsers.rows.length; i++) {
                if (gvUsers.rows[i].cells[5] != null) {
                    gvUsers.rows[i].cells[5].getElementsByTagName("INPUT")[0].checked = Checkbox.checked;
                }
            }
        }

        function UserDeleteConfirmation(all) {
            if (all) {
                return confirm('<%=Resources.Resource.FollowDeleteAll %>');
            } else {
                return confirm('<%=Resources.Resource.FollowDelete %>');
            }
        }
    </script>
    <div class="twitter-widget gridview-header">
        <geoqual:PagingGridView ID="gvUsers" runat="server" AllowPaging="true" PageSize="20" AutoGenerateColumns="false" OnPageIndexChanging="gvUsers_PageIndexChanging" OnRowDataBound="gvUsers_RowDataBound"
            AllowSorting="true" OnSorting="gvUsers_Sorting" ShowFooter="true">
            <FooterStyle CssClass="Grid-footer" />
            <Columns>
                <asp:TemplateField HeaderText="<%$ Resources:Resource, Index %>" SortExpression="Index">
                    <ItemTemplate>
                        <asp:Label ID="lblIndex" runat="server" Text='<%# Bind("Index") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Resource, ScreenName %>" SortExpression="ScreenName">
                    <ItemTemplate>
                        <asp:Label ID="lblScreenName" runat="server" Text='<%# Bind("ScreenName") %>' />
                    </ItemTemplate>
                    <FooterTemplate>
                        <asp:Button ID="btnAddExclude" runat="server" Text="<%$ Resources:Resource, AddExcludeList %>" OnClick="btnAddExclude_Click" />
                        <asp:Button ID="btnAddNeverFollow" runat="server" Text="<%$ Resources:Resource, AddNeverFollowList %>" OnClick="btnAddNeverFollow_Click" Visible="false" />
                    </FooterTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Resource, ProfileImage %>" SortExpression="ProfileImage">
                    <ItemTemplate>
                        <asp:Label ID="lblProfileImage" runat="server" Text='<%# Bind("ProfileImage") %>' />
                    </ItemTemplate>
                    <FooterTemplate>
                        <asp:Button ID="btnFollow" runat="server" Text="<%$ Resources:Resource, Follow %>" OnClick="btnFollow_Click" Visible="False" />
                        <asp:Button ID="btnFollowAll" runat="server" Text="<%$ Resources:Resource, FollowAll %>" OnClick="btnFollowAll_Click" Visible="False" />
                        <asp:Button ID="btnDestroy" runat="server" Text="<%$ Resources:Resource, Destroy %>" OnClick="btnDestroy_Click" OnClientClick="if (!UserDeleteConfirmation(false)) return false;" />
                        <asp:Button ID="btnDestroyAll" runat="server" Text="<%$ Resources:Resource, DestroyAll %>" OnClick="btnDestroyAll_Click" OnClientClick="if (!UserDeleteConfirmation(true)) return false;" />
                        <asp:Label ID="lblTotal" runat="server" CssClass="grid-total"></asp:Label>
                    </FooterTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Resource, UserId %>" SortExpression="UserId">
                    <ItemTemplate>
                        <asp:Label ID="lblUserId" runat="server" Text='<%# Bind("UserId") %>' />
                    </ItemTemplate>
                    <FooterTemplate>
                        <asp:Button ID="btnSave" runat="server" Text="<%$ Resources:Resource, Save %>" OnClick="btnSave_Click" Visible="false" />
                    </FooterTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <li id="liImage" runat="server" />
                    </ItemTemplate>
                    <FooterTemplate>
                        <asp:Button ID="btnboxOpenAll" runat="server" CssClass="hortext" OnClick="btnOpen_Click" Text="v" Width="12" />
                    </FooterTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Resource, Delete %>">
                    <HeaderTemplate>
                        <asp:CheckBox ID="chkboxSelectAll" runat="server" onclick="javascript:CheckAllEmp(this);" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="cbxDelete" runat="server" />
                    </ItemTemplate>
                    <FooterTemplate>
                        <asp:Button ID="btnDelete" runat="server" Text="<%$ Resources:Resource, Delete %>" OnClick="btnDelete_Click" />
                    </FooterTemplate>
                </asp:TemplateField>
            </Columns>
        </geoqual:PagingGridView>
    </div>
</asp:Content>
