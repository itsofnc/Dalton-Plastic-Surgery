<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/mstSite.Master" CodeBehind="Contact.aspx.vb" Inherits="DPSResponsive.Contact" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <link href="assets/formValidation/css/formValidation.min.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">

    <form id="frmContact" class="form-horizontal" role="form" method="post" runat="server">

        <h1 class="title_data">Request For Information</h1>
        
        <div id="divRequestForm">
            <p class="body_paragraph contact-paragraph">
                We want to hear from you! If you would like more information about the services we offer, please fill out the email form below and someone will contact you soon!
            </p>
            <div class="container">
                <div class="form-group">
                    <div class="row">
                        <div class="col-xs-12 col-sm-8">
                            <label for="txtName">
                                Name *
                            </label>
                            <asp:TextBox ID="txtName" class="form-control" runat="server" 
                                data-fv-notempty="true" data-fv-notempty-message="Name is required and cannot be empty"
                                placeholder="Your Name"></asp:TextBox>
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <div class="row">
                        <div class="col-xs-12 col-sm-8">
                            <label for="txtEmail">
                                Email *
                            </label>
                            <asp:TextBox ID="txtEmail" class="form-control" runat="server" 
                                data-fv-notempty="true" data-fv-notempty-message="Email is required and cannot be empty"
                                data-fv-emailaddress="true"
                                data-fv-emailaddress-message="The value is not a valid email address"
                                placeholder="Your Email"></asp:TextBox>
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <div class="row">
                        <div class="col-xs-12 col-sm-8">
                            <label for="txtSubject">
                                Subject *
                            </label>
                            <asp:TextBox ID="txtSubject" class="form-control" runat="server" 
                                data-fv-notempty="true" data-fv-notempty-message="Subject is required and cannot be empty"
                                placeholder="Your Subject"></asp:TextBox>
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <div class="row">
                        <div class="col-xs-12 col-sm-8">
                            <label for="txtMessage">
                                Message *
                            </label>
                            <asp:TextBox ID="txtMessage" class="form-control" runat="server" 
                                data-fv-notempty="true" data-fv-notempty-message="Message is required and cannot be empty"
                                placeholder="Your Message" rows="3" TextMode="MultiLine"></asp:TextBox>
                        </div>
                    </div>
                </div>
                    
                <div class="form-group">
                    <div class="row">
                        <div class="col-xs-12 col-sm-8">
                            <!-- The captcha container -->
                            <div id="captchaContainer"></div>
                        </div>
                    </div>
                </div>
                    
                    
                <div class="row">
                    <div class="col-xs-8">
                        <asp:Button ID="btnSubmit" class="btn btn-primary" runat="server" Text="Submit" /> &nbsp;&nbsp;
                        <button type="button" class="btn btn-default" id="resetButton">Reset</button>
                    </div>
                </div>
            </div>
        </div>
        <div id="divRequestSubmitted" style="display:none;">
            <br />
            <p>Your request has been received.</p><br />
		        <br />
                <p class="normalText">A member of our staff will contact you soon.<br /><br />Thank You.</p><br /><br /><br /><br /><br />
        </div>

    </form>
    

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="sidebarContent" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="pageJavaScripts" runat="server"> 
       
    <script src="assets/formValidation/js/formValidation.min.js"></script>
    <script src="assets/formValidation/addons/dist/reCaptcha2.min.js"></script>
    <script src="assets/formValidation/js/framework/bootstrap.min.js"></script>


    <script>
        jQuery(document).ready(function () {
            jQuery('#frmContact')
                .formValidation({
                    framework: 'bootstrap',
                    addOns: {
                        reCaptcha2: {
                            element: 'captchaContainer',
                            language: 'en',
                            theme: 'light',
                            siteKey: '6LdfhwwTAAAAAD2iD0WRtk_U9OnfrdVOY-J4GIzn',
                            timeout: 120,
                            message: 'The captcha is not valid'
                        }
                    },
                    message: 'Please complete all areas of the form to submit an email.'
                });
            jQuery("<%= txtName.ClientID%>").focus();
        });
        jQuery('#resetButton').on('click', function () {
            // Reset the recaptcha
            FormValidation.AddOn.reCaptcha2.reset('captchaContainer');

            // Reset form
            jQuery('#frmContact').formValidation('resetForm', true);
        });
        
    </script>

    <asp:Literal ID="litScripts" runat="server"></asp:Literal>

    
</asp:Content>
