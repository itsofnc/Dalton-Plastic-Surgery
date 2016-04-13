<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/mstSite.Master" CodeBehind="miraDryVideos.aspx.vb" Inherits="DPSResponsive.miraDryVideos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">
    <h1 class="title_data"><asp:Literal ID="title_data" runat="server"></asp:Literal></h1>
       
    <asp:Literal ID="body_data" runat="server" ></asp:Literal>
    
     <div class="col-md-12">
        <!-- 16:9 aspect ratio -->
        <div class="embed-responsive embed-responsive-16by9">
          <iframe width="560" height="315" src="https://www.youtube.com/embed/1ySJHUI5PM8" frameborder="0" allowfullscreen></iframe>
        </div>
    </div>
    
    <div class="col-md-12">
        <hr />
       <center>Check out our <a href="miraPRLinks.aspx">Public Relations Links</a> about miraDry & miraSmooth.</center>
   </div>
</asp:Content>
