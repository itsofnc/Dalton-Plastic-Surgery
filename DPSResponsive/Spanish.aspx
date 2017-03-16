<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/mstSite.Master" CodeBehind="Spanish.aspx.vb" Inherits="DPSResponsive.Spanish" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">
    <h1 class="title_data"><asp:Literal ID="title_data" runat="server"></asp:Literal></h1>
       
    <asp:Literal ID="body_data" runat="server" ></asp:Literal>
        
    <div class="col-md-4">
        <br />
        <img src="images/happy-people.jpg" alt="Cirugía plástica" width="205" height="205" />
        <p class="lead">Cirugía plástica</p>
        <p>La cirugía plástica se ha incrementado dramáticamente en popularidad en los últimos 10 años. Con este aumento de la popularidad, 
            las actitudes han cambiado también. Menos personas envejecen con gracia, mientras que cada vez más están aceptando la alternativa 
            de cirugía estética.</p>
    </div>
    <div class="col-md-4">
        <br />
        <img src="images/facial-rejuvenation-sm.jpg" alt="Procedimientos cosméticos no quirúrgicos" width="205" height="205" />
        <p class="lead">Procedimientos cosméticos no quirúrgicos</p>
        <p>Nuestro objetivo es ayudarle a verse y sentirse lo mejor posible. Queremos que usted pueda lograr el resultado deseado a través de tratamientos quirúrgicos y / o 
            no quirúrgicos. soluciones personalizadas adaptadas para cada individuo para ayudar a satisfacer todas sus necesidades de cuidado de la piel.</p>
    </div>
    <div class="col-md-4">
        <br />
        <img src="images/spa-treatment.png" alt="Médico Spa" width="205" height="205" />
        <p class="lead">Spa Médico</p>
        <p>Dalton Medical Spa, ubicado dentro de las instalaciones médicas de Cirugía Plástica Dalton, no sólo es capaz para consentirlo con productos de élite de 
            calidad spa, pero nos proporcionan servicios que son complicados suficiente para requerir la supervisión y la experiencia de profesionales con licencia para 
            el cuidado de la salud.</p>
    </div>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="sidebarContent" runat="server">
</asp:Content>
<asp:Content ID="Content4" runat="server" ContentPlaceHolderID="pageJavaScripts">
</asp:Content>
