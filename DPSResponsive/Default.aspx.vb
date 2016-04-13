Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Session("lang") = "English"
        If IsPostBack Then
        Else
            BuildFormData()
        End If
    End Sub

    Private Sub BuildFormData()
        ' load page content data
        Dim strSQL As String = "Select * from frm_data where form_id = 'Home'"
        Dim tblPrimaryTable As DataTable = g_IO_Execute_SQL(strSQL, False)

        If tblPrimaryTable.Rows.Count > 0 Then
            If IsDBNull(tblPrimaryTable.Rows(0)("Form_Title")) Then
            Else
                title_data.Text = tblPrimaryTable.Rows(0)("Form_Title")
            End If
            If IsDBNull(tblPrimaryTable.Rows(0)("Form_Content")) Then
            Else
                body_data.Text = tblPrimaryTable.Rows(0)("Form_Content").replace("&lt;", "<")
            End If
        End If

    End Sub


End Class