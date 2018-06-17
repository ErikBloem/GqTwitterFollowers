<%@ Page Title="" Language="C#" MasterPageFile="~/Master/Twitter.Master" AutoEventWireup="true" CodeBehind="twitter_timeline.aspx.cs" Inherits="GQTwitterFollowers.twitter_timeline" %>
<%@ MasterType virtualPath="~/Master/Twitter.Master"%>
<%@ Register Assembly="GQPagingGridView" Namespace="GQPagingGridView" TagPrefix="geoqual" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="btnPlaceholder" runat="server">
    <script type="text/javascript">
        function CheckAllEmp(Checkbox) {
            var gvUsers = document.getElementById("<%=gvTimeLine.ClientID %>");
            for (i = 1; i < gvUsers.rows.length; i++) {
                if (gvUsers.rows[i].cells[6] != null) {
                    gvUsers.rows[i].cells[6].getElementsByTagName("INPUT")[0].checked = Checkbox.checked;
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

        function isNumberKey(evt) {
            var charCode = (evt.which) ? evt.which : event.keyCode
            if (charCode > 31 && (charCode < 48 || charCode > 57))
                return false;
            return true;
        }

        function document_Ready() {
            //do work
            var radioButtonlist = document.getElementById("<%=rbListTimelineTweetid.ClientID %>");
            if (radioButtonlist) {
                RadioButtonList_SelectedIndexChanged(radioButtonlist);
            }
        }

        function RadioButtonList_SelectedIndexChanged(radioButtonlist) {
            if (radioButtonlist) {
                var inputs = radioButtonlist.getElementsByTagName('input');
                var flag = false;
                var selected;

                for (var i = 0; i < inputs.length; i++) {
                    if (inputs[i].checked) {
                        selected = inputs[i];
                        flag = true;
                        break;
                    }
                }
                if (flag) {
                    var divbytimeline = document.getElementById('divbytimeline');
                    var divbytweetid = document.getElementById('divbytweetid');
                    if (divbytimeline != null & divbytweetid != null) {
                        if (selected.value == 0) {
                            divbytimeline.className = "checkboxTwitterData";
                            divbytweetid.className = "checkboxTwitterData listboxHidden";
                        } else {
                            divbytweetid.className = "checkboxTwitterData";
                            divbytimeline.className = "checkboxTwitterData listboxHidden";
                        }
                    }
                }
            }
        }
    </script>
    <div class="listboxTwitterData" id="bytimelineortweetid">
        <asp:RadioButtonList ID="rbListTimelineTweetid" runat="server" onclick="RadioButtonList_SelectedIndexChanged(this);">
            <asp:ListItem Text="By Timeline" Value="0"></asp:ListItem>
            <asp:ListItem Text="By Tweetid" Value="1"></asp:ListItem>
        </asp:RadioButtonList>
        <div id="divbytimeline" class="checkboxTwitterData listboxHidden" >
            <asp:CheckBox ID="cbxTwitterData" runat="server" Text="<%$ Resources:Resource, TwitterData %>" CssClass="twitter-widget-checkbox" AutoPostBack="true" OnCheckedChanged="cbxTwitterData_CheckedChanged" />
            <asp:Button ID="btnNext" runat="server" Text="<%$ Resources:Resource, Next %>" OnClick="btnNext_Click" Visible="false" />&nbsp;
            <asp:TextBox ID="txtLastID" runat="server" onblur="return isNumberKey(event);"></asp:TextBox>&nbsp;&nbsp;<asp:Label ID="lblCount" runat="server" Text="0"></asp:Label>
        </div>
        <div id="divbytweetid" class="checkboxTwitterData listboxHidden" >
            <asp:CheckBox ID="cbxTweetIdTwitterData" runat="server" Text="<%$ Resources:Resource, TwitterData %>" CssClass="twitter-widget-checkbox" AutoPostBack="false" />
            <asp:Button ID="btnFindByTweetId" runat="server" Text="<%$ Resources:Resource, TweetId %>" OnClick="btnFindByTweetId_Click" Visible="true" />&nbsp;
            <asp:CheckBox ID="cbxTweetIdExcelData" runat="server" Text="<%$ Resources:Resource, TweetIdExcelData %>" CssClass="twitter-widget-checkbox" AutoPostBack="false" />
        </div>
    </div>
    <div style="float: left;" class="twitter-widget twitter-widget-padding">
        <div class="twitter-widget twitter-widget-padding gridview-header-timeline">
            <geoqual:PagingGridView ID="gvTimeLine" runat="server" AllowPaging="true" PageSize="20" AutoGenerateColumns="false" OnPageIndexChanging="gvTimeLine_PageIndexChanging" OnRowDataBound="gvTimeLine_RowDataBound"
                AllowSorting="true" OnSorting="gvTimeLine_Sorting" ShowFooter="true">
                <FooterStyle CssClass="Grid-footer" />
                <Columns>
                    <asp:TemplateField HeaderText="<%$ Resources:Resource, Index %>" SortExpression="Index">
                        <ItemTemplate>
                            <asp:Label ID="lblIndex" runat="server" Text='<%# Bind("Index") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Resource, Createdat %>" SortExpression="Createdat" HeaderStyle-CssClass="CreatedatMinWidth">
                        <ItemTemplate>
                            <asp:Label ID="lblCreatedat" runat="server" Text='<%# Bind("Createdat") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Resource, ID %>" SortExpression="ID">
                        <ItemTemplate>
                            <asp:Label ID="lblID" runat="server" Text='<%# Bind("ID") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Resource, Text %>" SortExpression="Text" HeaderStyle-Width="100%" ItemStyle-Wrap="true">
                        <ItemTemplate>
                            <asp:Label ID="lblText" runat="server" Text='<%# Bind("Text") %>' />
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Button ID="btnDeleteAllReply" runat="server" Text="<%$ Resources:Resource, DeleteAllReply %>" OnClick="btnDeleteAllReply_Click" />
                            <asp:Button ID="btnDeleteAllRetweets" runat="server" Text="<%$ Resources:Resource, DeleteAllRetweets %>" OnClick="btnDeleteAllRetweets_Click" />
                            <asp:Button ID="btnDeleteAll" runat="server" Text="<%$ Resources:Resource, DeleteAll %>" OnClick="btnDeleteAll_Click" />
                        </FooterTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Resource, IsReply %>" SortExpression="IsReply" ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="IsReplyMinWidth">
                        <ItemTemplate>
                            <asp:CheckBox ID="cbxIsReply" runat="server" Checked='<%# Bind("IsReply") %>' Enabled="false" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Resource, User %>" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:Panel ID="pnlUser" runat="server"></asp:Panel>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Resource, Delete %>" ItemStyle-HorizontalAlign="Center">
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
        <asp:Label ID="lblTotal" runat="server" Visible="false"></asp:Label>
        <asp:Label ID="lblError" runat="server" Visible="false" CssClass="listboxTwitterData lblError"></asp:Label>
    </div>
</asp:Content>
