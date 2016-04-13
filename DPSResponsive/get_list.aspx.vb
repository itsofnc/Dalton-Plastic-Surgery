Public Class get_list
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim action As String = ""
        If IsNothing(Request.QueryString("action")) Then
            action = Request.Form("action")
        Else
            action = Request.QueryString("action")
        End If

        If action = "staff" Then
            litHeader.Text = "<h1 class='title_data'>Meet Our Staff</h1>"
            getStaffListing()
        ElseIf action = "products" Then
            litHeader.Text = "<h1 class='title_data'>Our Products</h1>"
            getProductListing()
        End If

    End Sub

    Private Sub getStaffListing()
        Dim strSQL As String = "Select * from mc_staff order by display_order,lname"
        Dim tblList As DataTable = g_IO_Execute_SQL(strSQL, False)
        'rprList.DataSource = tblList
        'rprList.DataBind()
        Dim strHTML As String = ""
        For Each row In tblList.Rows
            strHTML &= "<div class=""row"">"
            strHTML &= "<div class=""col-xs-12 col-sm-3"">"
            strHTML &= "<img src=""" & row("image_ref") & """ class=""shadow""/><br />"
            strHTML &= "</div>"
            strHTML &= "<div class=""col-xs-12 col-sm-9"">"
            strHTML &= "<strong>" & row("display_name") & "</strong><br /><p>" & row("content") & "</p>"
            strHTML &= "</div>"
            strHTML &= "</div>"
            strHTML &= "<hr />"
        Next
        lit2ColList.Text = strHTML
    End Sub

    Private Sub getProductListing()
        Dim strSQL As String = "Select * from mc_products order by display_order,display_name"
        Dim tblList As DataTable = g_IO_Execute_SQL(strSQL, False)
        'rprList.DataSource = tblList
        'rprList.DataBind()
    End Sub
End Class