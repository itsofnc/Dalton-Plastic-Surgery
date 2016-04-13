Public Class frmListings
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim lst As String = ""
        If IsPostBack Then
        Else
            If IsNothing(Request.QueryString("lst")) Then
                lst = Request.Form("lst")
            Else
                lst = Request.QueryString("lst")
            End If

            Select Case lst
                Case "staff"
                    litHeader.Text = "<h1 class='title_data'>Meet Our Staff</h1>"
                    getStaffListing()
                Case "products"
                    litHeader.Text = "<h1 class='title_data'>Our Products</h1>"
                    getProductListing()
            End Select
        End If


    End Sub

    Private Sub getStaffListing()
        Dim strSQL As String = "Select * from mc_staff order by display_order,lname"
        Dim tblList As DataTable = g_IO_Execute_SQL(strSQL, False)
        Dim strHTML As String = ""
        For Each row In tblList.Rows
            strHTML &= "<div class=""row"">"
            strHTML &= "<div class=""col-xs-12 col-sm-4 col-md-3"">"
            strHTML &= "<img src=""" & row("image_ref") & """ class=""shadow img-responsive"" /><br />"
            strHTML &= "</div>"
            strHTML &= "<div class=""col-xs-12 col-sm-8 col-md-9"">"
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
        Dim strHTML As String = ""
        For Each row In tblList.Rows
            strHTML &= "<div class=""row"">"
            strHTML &= "<div class=""col-xs-12 col-sm-4 col-md-3"">"
            strHTML &= "<img src=""" & row("image_ref") & """ class=""shadow img-responsive""/><br />"
            strHTML &= "</div>"
            strHTML &= "<div class=""col-xs-12 col-sm-8 col-md-9"">"
            strHTML &= "<strong>" & row("display_name") & "</strong><br /><p>" & row("content") & "</p>"
            strHTML &= "</div>"
            strHTML &= "</div>"
            strHTML &= "<hr />"
        Next
        lit2ColList.Text = strHTML
    End Sub

End Class