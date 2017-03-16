<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/mstSite.Master" CodeBehind="Staff.aspx.vb" Inherits="DPSResponsive.Staff" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">
    <form id="frmGetList" runat="server" class="form-horizontal">
        <div id="formContent">
            <div id="divGridList" class="col-xs-12"> <!-- max-height: 590px; overflow: auto;>-->
                <%--Built during document.ready getList() from 'get_list.aspx'--%>
            </div> 
        </div>
    </form>
</asp:Content>
<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="sidebarContent"></asp:Content>
<asp:Content ID="Content4" runat="server" ContentPlaceHolderID="pageJavaScripts">
    <script type="text/javascript">
        jQuery(document).ready(function ($) {
            $.noConflict();
        });

        jQuery(document).ready(function () {
            getList();
        });

        // load the group data grid list
        function getList() {
            var action = "staff";
            jQuery.post('get_list.aspx', { action: action }, function (data) {
                jQuery('#divGridList').html(data);
                jQuery("th").addClass("tableheader");
            });
        }

    </script>
</asp:Content>