<%@ Page Title="" Language="C#" MasterPageFile="~/Master/Twitter.Master" AutoEventWireup="true" CodeBehind="twitter_users.aspx.cs" Inherits="GQTwitterFollowers.twitter_users" %>
<%@ MasterType virtualPath="~/Master/Twitter.Master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        !function (d, s, id) {
            var js, fjs = d.getElementsByTagName(s)[0];
            if (!d.getElementById(id)) {
                js = d.createElement(s);
                js.id = id;
                js.src = "//platform.twitter.com/widgets.js";
                fjs.parentNode.insertBefore(js, fjs);
            }
        }
        (document, "script", "twitter-wjs");
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="btnPlaceholder" runat="server">
    <div class="checkboxTwitterData" >
        <asp:CheckBox ID="cbxTwitterData" runat="server" Text="<%$ Resources:Resource, TwitterData %>" CssClass="twitter-widget-checkbox" />
    </div>
    <div style="float: left;" class="twitter-widget">
        <asp:Label ID="lblTotalFollowers" runat="server"></asp:Label>
        <ul id="ulTwitterFollowers" runat="server">

        </ul>
        <asp:Label ID="lblError" runat="server" Visible="false"></asp:Label>
    </div>
</asp:Content>