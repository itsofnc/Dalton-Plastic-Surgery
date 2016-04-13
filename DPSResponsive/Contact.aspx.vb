Public Class Contact
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub btnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click
        If g_EmailEnabled Then
            Dim strSubject As String = "Request for Information"
            Dim strMessage As String = "Request for Information -" & vbCrLf & vbCrLf & _
                "Name: " & txtName.Text.Replace("'", "''") & vbCrLf & _
                "Email: " & txtEmail.Text & vbCrLf & _
                "Subject: " & txtSubject.Text.Replace("'", "''") & vbCrLf & _
                "Message: " & txtMessage.Text.Replace("'", "''") & _
                vbCrLf & vbCrLf & _
                "This email was genereated from your website on: " & Date.Now.ToString("yyyy-MM-dd HH:mm:ss")

            g_sendEmail(g_defaultEmail, strSubject, strMessage)
            litScripts.Text = "<script>jQuery(document).ready(function() {document.getElementById('divRequestSubmitted').style.display = 'block'; document.getElementById('divRequestForm').style.display = 'none';});</script>"

        End If

    End Sub


End Class