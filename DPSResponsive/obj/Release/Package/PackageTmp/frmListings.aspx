<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/mstSite.Master" CodeBehind="frmListings.aspx.vb" Inherits="DPSResponsive.frmListings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">
    <form id="frmGetList" runat="server" class="form-horizontal">
        <div id="formContent">
            <div id="divGridList" class="col-xs-12">
                <h1 class="title_data"><asp:literal id="litHeader" runat="server"></asp:literal></h1>
                <hr />                
                <asp:literal id="lit2ColList" runat="server"></asp:literal>
            </div> 
        </div>
    </form>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="sidebarContent" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="pageJavaScripts" runat="server">

</asp:Content>
