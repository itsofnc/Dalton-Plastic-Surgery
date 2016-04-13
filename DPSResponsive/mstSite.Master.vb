Public Class mstSite
    Inherits System.Web.UI.MasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
        Else
            If IsNothing(Session("user_link_id")) Then                
                litNavigation.Text = g_BuildNavigation(0)
                'litNavbarFooter.Text = g_BuildNavigation(0)
            Else
                litHeaderGreeting.Text = "<p class=""navbar-text navbar-right""><span>Welcome, " & _
                    IIf(IsNothing(Session("user_name")), "", Session("user_name")) & _
                    "&nbsp;&nbsp;&nbsp;</span><input id=""btnLogout"" onclick=""logOut();"" class=""btn btn-sm btn-warning"" value=""Logout"" type=""button""/></p>"

                litNavigation.Text = g_BuildNavigation(0)
                'litNavbarFooter.Text = g_BuildNavigation(0)
            End If

        End If
    End Sub

End Class