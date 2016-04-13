<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/mstSite.Master" CodeBehind="Default.aspx.vb" Inherits="DPSResponsive._Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">
    <h1 class="title_data"><asp:Literal ID="title_data" runat="server"></asp:Literal></h1>
       
    <asp:Literal ID="body_data" runat="server" ></asp:Literal>
        
    <div class="col-md-4">
        <br />
        <a href="frmModuleArticle.aspx?id=1"><img src="images/happy-people.jpg" alt="Plastic Sugery" width="205" height="205" /></a>
        <p class="lead"><a href="frmModuleArticle.aspx?id=1">Plastic Surgery</a></p>
        <p>Plastic surgery has increased dramatically in popularity over the past 10 years. With this increase in popularity, attitudes have changed as well. Fewer people are growing older gracefully, while more and more are accepting the cosmetic surgery alternative.</p>
        <p><a class="btn btn-primary" href="frmModuleArticle.aspx?id=1" role="button">Learn More &raquo;</a></p>
    </div>
    <div class="col-md-4">
        <br />
        <a href="frmModuleArticle.aspx?id=4"><img src="images/facial-rejuvenation-sm.jpg" alt="Plastic Sugery" width="205" height="205" /></a>
        <p class="lead"><a href="frmModuleArticle.aspx?id=4">Non-Surgical Cosmetic Procedures</a></p>
        <p>Our goal is to help you look and feel your best. We want you to achieve your desired result through surgical and/or non-surgical treatments. customized solutions tailored for each individual to help meet all their skin care needs.  </p>
        <p><a class="btn btn-primary" href="frmModuleArticle.aspx?id=4" role="button">Learn More &raquo;</a></p>
    </div>
    <div class="col-md-4">
        <br />
        <a href="frmModuleArticle.aspx?id=3"><img src="images/spa-treatment.png" alt="Plastic Sugery" width="205" height="205" /></a>
        <p class="lead"><a href="frmModuleArticle.aspx?id=3">Medical Spa</a></p>
        <p>Dalton Medical Spa, located within Dalton Plastic Surgery medical facility, is not only able to pamper you with elite spa quality products, but we provide services that are complicated enough to require the supervision and expertise of licensed health care professionals.</p>
        <p><a class="btn btn-primary" href="frmModuleArticle.aspx?id=3" role="button">Learn More &raquo;</a></p>
    </div>
</asp:Content>