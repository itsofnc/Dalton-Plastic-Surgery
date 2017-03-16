Imports System.Web

Module ModAppCustoms
    Public Function g_LoginRedirect(ByVal strWhereFrom As String) As String
        Dim strLoginRedirect As String = ""
        If UCase(strWhereFrom) = "LOGIN" Then
            strLoginRedirect = "frmListManager.aspx?id=ReleaseNotes"
        Else
            strLoginRedirect = "Default.aspx"
            If UCase(System.Web.HttpContext.Current.Session("user_role")) = "ADMINISTRATOR" Then
                strLoginRedirect = "frmListManager.aspx?id=ReleaseNotes"
            Else
                strLoginRedirect = "frmListManager.aspx?id=ReleaseNotes"
            End If
        End If

        Return strLoginRedirect
    End Function

    Public Function g_BuildNavigation() As String()

        ' build up navigation for this app
        Dim arrNavigation() As String = Nothing
        ReDim Preserve arrNavigation(1)
        If HttpContext.Current.Session("lang") = "Spanish" Then
            arrNavigation(0) = "<ul class=""nav navbar-nav"">"
            arrNavigation(0) &= _
                "<li id=""#ContactUs""><a href=""Contact.aspx?id=8"">Contáctenos</a></li></ul>" & _
                "<ul class=""nav navbar-nav navbar-right"">" & _
                "<li id=""#PatientPortal""><a href=""https://daltonplasticsurgery.nextechweb.com/NexWebPortal507/PatientSummary.aspx"" " & _
                        " target=""_blank"">Portal del Paciente</a></li>" & _
                "<li id=""#English""><a href=""Default.aspx"">Back to English</a></li>"
            arrNavigation(0) &= "</ul>"
        Else
            arrNavigation(0) = "<ul class=""nav navbar-nav"">"
            arrNavigation(0) &=
                "<li id=""#Home"" class=""active""><a href=""Default.aspx"">Home</a></li>" &
                "<li id=""#About"" ><a href=""frmModuleArticle.aspx?id=6"">About</a></li>" &
                "<li id=""#Services"" class=""dropdown"">" &
                "    <a href=""frmModuleArticle.aspx?id=-1,1"" class=""dropdown-toggle"" data-toggle=""dropdown"" role=""button"" aria-expanded=""false"">Services<span class=""caret""></span></a>" &
                "    <ul class=""dropdown-menu"" role=""menu"">" &
                "       <li id=""#PlasticSurgery"" ><a href=""frmModuleArticle.aspx?id=1"">Plastic Surgery</a></li>" &
                "       <li id=""#MedicalSpa"" ><a href=""frmModuleArticle.aspx?id=3"">Medical Spa</a></li>" &
                "       <li id=""#FacialRejuvenation"" ><a href=""frmModuleArticle.aspx?id=4"">Facial Rejuvenation</a></li>" &
                "       <li id=""miraIntro"" ><a href=""miraDryIntro.aspx"">Introducing miraDry! </a></li>" &
                "       <li id=""miraDry"" ><a href=""miraDry.aspx"">miraDry-Stop the Sweat</a></li>" &
                "       <li id=""miraSmooth"" ><a href=""miraSmooth.aspx"">miraSmooth-Premium Hair Removal</a></li>" &
                "    </ul>" &
                "</li>" &
                "<li id=""#Products"" ><a href=""frmListings.aspx?lst=products"">Products</a></li>" &
                "<li id=""#FAQ""><a href=""frmModuleArticle.aspx?id=7"">FAQ</a></li>" &
                "<ul class=""nav navbar-nav navbar-right"">" &
                "<li id=""#PatientPortal""><a href=""https://daltonplasticsurgery.nextechweb.com/NexWebPortal507/PatientSummary.aspx"" " &
                        " target=""_blank"">Patient Portal Access</a></li>" &
                "<li id=""#Spanish""><a href=""Spanish.aspx"">Hablamos Español?</a></li>"

            ' 3/9/17 cpb removed the contact us page per Robin
            '"<li id=""#ContactUs""><a href=""Contact.aspx?id=8"">Contact Us</a></li></ul>" &

            arrNavigation(0) &= "</ul>"
        End If

        arrNavigation(1) = "href=""Default.aspx"""

        Return arrNavigation
    End Function

End Module
