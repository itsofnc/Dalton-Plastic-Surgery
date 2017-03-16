<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/mstSite.Master" CodeBehind="frmModuleArticle.aspx.vb" Inherits="DPSResponsive.frmModuleArticle" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">  
    <link href="assets/Gallery-2.16.0/css/blueimp-gallery.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">
   <form id="frmMaster" runat="server" class="form-horizontal">
    <asp:Literal ID="litAdminEdit" runat="server"></asp:Literal>
    <asp:Literal ID="litBackGroundTop" runat="server"></asp:Literal>
       <div id="blueimp-gallery" class="blueimp-gallery">
        <div class="slides"></div>
        <h3 class="title"></h3>
        <a class="prev">‹</a>
        <a class="next">›</a>
        <a class="close">×</a>
        <a class="play-pause"></a>
        <ol class="indicator"></ol>
    </div>
        <div id="formContent">
            <div class="col-xs-12">
                <h1 class="title_data"><asp:Literal ID="litTitleData" runat="server"></asp:Literal></h1>
            </div>
            <div class="col-xs-12">
                <asp:Literal ID="litBodyText" runat="server"></asp:Literal>
            </div>
            <div class="col-xs-12">                    
                <asp:Literal ID="litBodyImage" runat="server"></asp:Literal>
            </div>
            <div class="col-xs-12">
                <br />
                <%--SubArticle Menu--%>                
                <asp:Literal ID="litSubArticleMenu" runat="server"></asp:Literal>

                <%--SubArticle Gallery--%>                
                <asp:Literal ID="litSubArticleGalleryTitle" runat="server"></asp:Literal>

                <asp:Literal ID="litSubArticleGallery" runat="server"></asp:Literal>

                <asp:Literal ID="litNoGalleryImages" runat="server"></asp:Literal>
                    
            </div>
        </div>
        <asp:Literal ID="litBackGroundBottom" runat="server"></asp:Literal>

   </form>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="sidebarContent" runat="server">
   <%-- <div class="col-xs-12">--%>
        <asp:Literal ID="litRightSideImage" runat="server"></asp:Literal>
    <%--</div>--%>
    <%--<div id="divSliderFAMenu" class="col-sm-12">--%>
        <asp:Literal ID="litSubMenu" runat="server"></asp:Literal>
    <%--</div>--%>
</asp:Content>
<asp:Content ID="Content4" runat="server" ContentPlaceHolderID="pageJavaScripts">
    <script src="assets/js/BuildFormData.js"></script>
    <script src="assets/Gallery-2.16.0/js/blueimp-gallery.min.js"></script>
    <asp:Literal ID="litScripts" runat="server"></asp:Literal>
    <script type="text/javascript">

        $(function () {
            var zIndexNumber = 1000;
            $('ul').each(function () {
                $(this).css('zIndex', zIndexNumber);
                zIndexNumber -= 10;
            });
        });

        // pop up to verify Edit Current or Previously being edited data
        function verifyEditType(varEdited, varRedirect) {
            if (varEdited == 'True') {
                //display the dialog
                $("#divBeingEdited").dialog({
                    resizable: false,
                    height: 200,
                    modal: true,
                    buttons: {
                        "Use Edit Copy": function () {
                            varRedirect += "&tmpEdit=true";
                            window.location = varRedirect;
                        },
                        "Use Current Copy": function () {
                            window.location = varRedirect;
                        }
                    }
                });
            } else {
                window.location = varRedirect;
            }
        }

    </script>
</asp:Content>
