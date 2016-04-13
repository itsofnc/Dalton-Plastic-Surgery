<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/mstSite.Master" CodeBehind="Products.aspx.vb" Inherits="DPSResponsive.Products" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">
    <div id="divGridList" style="float:left; width: 650px;"> <!-- max-height: 590px; overflow: auto;>-->
        <%--Built during document.ready getList() from 'get_list.aspx'--%>
    </div> 
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="sidebarContent" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="pageJavaScripts" runat="server">
    <script type="text/javascript">
        jQuery(document).ready(function ($) {
            $.noConflict();
        });

        jQuery(document).ready(function () {
            getList();
        });

        // load the group data grid list
        function getList() {
            var action = "products";
            jQuery.post('get_list.aspx', { action: action }, function (data) {
                jQuery('#divGridList').html(data);
                jQuery("th").addClass("tableheader");
            });
        }

    </script>
</asp:Content>
