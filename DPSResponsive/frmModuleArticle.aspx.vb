Public Class frmModuleArticle
    Inherits System.Web.UI.Page

    Dim m_intModuleRecId As Integer = -1
    Dim m_intSubModuleRecId As Integer = -1
    Dim m_intArticleRecId As Integer = -1
    Dim m_intSubArticleRecId As Integer = -1

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If IsPostBack Then
        Else
            buildFormData()
        End If

    End Sub

    Private Sub buildFormData()

        ' get module/article id to display
        If IsNothing(Request.QueryString("id")) Then
            m_intModuleRecId = 1
        Else
            Try
                Dim strId() As String = Request.QueryString("id").Split(",")
                If strId.Length > 0 Then
                    m_intModuleRecId = strId(0)
                    m_intSubModuleRecId = strId(1)
                    m_intArticleRecId = strId(2)
                    m_intSubArticleRecId = strId(3)
                End If
            Catch ex As Exception
            End Try
        End If
        If IsNothing(Session("user_link_id")) Then
        Else
            ' determine if this module/article is currently being edited
            Dim blnBeingEdited As Boolean = False
            Dim tblTmpTable As DataTable = Nothing
            If m_intArticleRecId > -1 Then
                'buildArticleFormData()
                'displaySideMenu()
            ElseIf m_intSubModuleRecId > -1 Then
                'buildSubModuleFormData()
                'displaySideMenu()
            ElseIf m_intModuleRecId > -1 Then
                Dim strSQL As String = "Select * from tmpModulesVw Where modulesRecid = " & m_intModuleRecId
                tblTmpTable = g_IO_Execute_SQL(strSQL, False)
                If tblTmpTable.Rows.Count > 0 Then
                    blnBeingEdited = True
                End If
            Else
            End If

            Dim editID As String = ""
            If IsNothing(Request.QueryString("id")) Then
                editID = 1
            Else
                editID = Request.QueryString("id")
            End If
            Dim strHrefSave As String = "adminEdit.aspx?id=" & editID
            litAdminEdit.Text = _
                "<div id=""divAdminEdit"">" & _
                "<a href=""#"" onclick=verifyEditType(" & """" & blnBeingEdited & """" & "," & """" & strHrefSave & """" & ");>Edit</a>"

            If blnBeingEdited Then
                litAdminEdit.Text &= "<br />" & _
                    "<span style=""display:inline; line-height:1em; font-size:.8em;font-style:italic;"">" & _
                    "Currently Being Edited By " & UCase(tblTmpTable.Rows(0)("userId")) & _
                    "</span>"
            End If

            litAdminEdit.Text &= "</div>" & _
                "<div id=""divBeingEdited"" title=""Choose Edit File"" style=""display:none"">" & _
                "This page is currently being Edited." & vbCrLf & _
                "Please select version to edit." & _
                "</div>"


        End If

        ' determine level to display
        If m_intArticleRecId > -1 Then
            buildArticleFormData()
            displaySideMenu()
        ElseIf m_intSubModuleRecId > -1 Then
            buildSubModuleFormData()
            displaySideMenu()
        ElseIf m_intModuleRecId > -1 Then
            buildModuleFormData()
            displaySideMenu()
        Else
            Response.Redirect("Default.aspx")
        End If
    End Sub

    ' specific article selected for display
    Private Sub buildArticleFormData()
        Dim strHeaderArticle As String = ""
        Dim strHeaderSubModule As String = ""
        Dim strHeaderModule As String = ""
        Dim strHeader As String = ""
        Dim strHeaderDelim As String = ""

        ' read the article
        Dim strWhere As String = " recId = " & m_intArticleRecId
        Dim tblArticles As DataTable = readArticle(strWhere)

        ' read the article's related sub module
        If tblArticles.Rows.Count > 0 Then
            strHeaderArticle = tblArticles(0)("title")
            strWhere = " recid = " & tblArticles(0)("subModulesRecId")
            Dim tblSubmodules As DataTable = readSubModule(strWhere)
            ' read the sub module's module 
            If tblSubmodules.Rows.Count > 0 Then
                strHeaderSubModule = "<a href='frmModuleArticle.aspx?id=" & _
                    "-1," & tblSubmodules(0)("recid") & _
                    "' > " & tblSubmodules(0)("title") & "</a>"
                Dim tblModules As DataTable = readModule(tblSubmodules(0)("modulesRecId"))
                If tblModules.Rows.Count > 0 Then
                    strHeaderModule = "<a href='frmModuleArticle.aspx?id=" & _
                    tblModules(0)("recid") & _
                    "' > " & tblModules(0)("title") & "</a>"
                End If
            End If

            ' display the header
            displayHeader(strHeaderModule, strHeaderSubModule, strHeaderArticle)

            ' display the body
            displayBody(tblArticles(0)("body"), "", "", "")

            ' read for sub articles
            strWhere = "articleRecId = " & tblArticles(0)("recid")
            Dim tblSubArticles As DataTable = readSubArticle(strWhere)
            If tblSubArticles.Rows.Count > 0 Then
                ' build sub article menu
                Dim intIdxSubArticle As Integer = 0
                Dim intLoopIdx As Integer = 0
                Dim strSubArticleMenu As String = "<div class=""col-xs-12 col-sm-4""><Table >"
                For Each subArticle In tblSubArticles.Rows
                    strSubArticleMenu &= "<tr><td>" & _
                        "<a href='frmModuleArticle.aspx?id=" & _
                        "-1,-1," & tblArticles(0)("recid") & _
                        "," & subArticle("recid") & _
                        "' > " & subArticle("title") & "</a>" & "</td>"
                    strSubArticleMenu &= "</tr>"
                    If m_intSubArticleRecId > -1 Then
                        If subArticle("recid") = m_intSubArticleRecId Then
                            intIdxSubArticle = intLoopIdx
                        End If
                    Else

                    End If
                    intLoopIdx += 1
                Next
                strSubArticleMenu &= "</table></div>"
                litSubArticleMenu.Text = strSubArticleMenu

                ' display gallery header

                litSubArticleGalleryTitle.Text = "<div class=""col-xs-12 col-sm-8"">"
                litSubArticleGalleryTitle.Text &= tblSubArticles(intIdxSubArticle)("title")
                litSubArticleGalleryTitle.Text &= "</div>"

                ' build sub article gallery
                Dim tblSubArticleImages As DataTable = readSubArticleImages(tblSubArticles(intIdxSubArticle)("recid"))
                If tblSubArticleImages.Rows.Count > 0 Then
                    litSubArticleGallery.Text = "<div class=""col-xs-12 col-sm-8"">"
                    Dim strGallery As String = buildGallery(tblSubArticleImages, "article")
                    litSubArticleGallery.Text &= strGallery
                    litSubArticleGallery.Text &= "</div>"
                Else
                    litNoGalleryImages.Text = "<div class=""col-xs-12 col-sm-8"">No Images Available at this Time.</div>"
                End If
            End If
        End If

    End Sub

    Private Sub buildSubModuleFormData()
        Dim strHeaderSubModule As String = ""
        Dim strHeaderModule As String = ""
        Dim strHeader As String = ""
        Dim strHeaderDelim As String = ""

        ' read the subModule
        Dim strWhere As String = " recid = " & m_intSubModuleRecId
        Dim tblSubModule As DataTable = readSubModule(strWhere)

        ' build header display
        If tblSubModule.Rows.Count > 0 Then
            strHeaderSubModule = tblSubModule(0)("title")
            Dim tblModules As DataTable = readModule(tblSubModule(0)("modulesRecId"))
            If tblModules.Rows.Count > 0 Then
                strHeaderModule = "<a href='frmModuleArticle.aspx?id=" & _
                tblModules(0)("recid") & _
                "' > " & tblModules(0)("title") & "</a>"
            End If
            ' display the header
            displayHeader(strHeaderModule, strHeaderSubModule, "")

            ' display the body
            displayBody(tblSubModule(0)("body"), "", "", "")

            ' build list of articles to display on left
            strWhere = " subModulesRecId = " & tblSubModule(0)("recid")
            Dim tblArticles As DataTable = readArticle(strWhere)
            If tblArticles.Rows.Count > 0 Then
                ' build article carousel
                litSubArticleGallery.Text = "<div class=""col-xs-12"">"
                Dim strGallery As String = buildGallery(tblArticles, "subModule")
                litSubArticleGallery.Text &= strGallery
                litSubArticleGallery.Text &= "</div>"
            Else
                litNoGalleryImages.Text = "<div class=""col-xs-12"">Sorry, no information available at this time...</div>"
            End If
        End If

    End Sub

    Private Sub buildModuleFormData()
        Dim strHeaderModule As String = ""

        ' read the subModule
        Dim tblModule As DataTable = readModule(m_intModuleRecId)

        ' build header display
        If tblModule.Rows.Count > 0 Then

            ' back image
            If IsDBNull(tblModule(0)("backgroundImage")) Then
            Else
                litBackGroundTop.Text = "<div id=""BackGround"" class=""img-responsive"" style=""background-image:url(" & tblModule(0)("backgroundImage") & ");  background-repeat: no-repeat; min-height:550px;"" >"
                litBackGroundBottom.Text = "</div>"
            End If

            strHeaderModule = tblModule(0)("title")
            ' display the header
            displayHeader(strHeaderModule, "", "")

            ' display the body
            displayBody(tblModule(0)("body"), tblModule(0)("bodyImage"), tblModule(0)("bodyImageUrl"), tblModule(0)("bodyImageAlt"))

            ' display right side image
            If tblModule(0)("rightSidebarImage") <> "" Then
                If tblModule(0)("rightSidebarImageUrl") <> "" Then
                    litRightSideImage.Text = "<a href=""" & tblModule(0)("rightSidebarImageUrl") & """ ><img alt=""" & tblModule(0)("rightSidebarImageAlt") & """ src=""" & tblModule(0)("rightSidebarImage") & """ class=""shadow img-responsive"" /></a><br /><br />"
                Else
                    litRightSideImage.Text = "<img alt=""" & tblModule(0)("rightSidebarImageAlt") & """ src=""" & tblModule(0)("rightSidebarImage") & """ class=""shadow img-responsive"" /><br /><br />"
                End If
            End If
        End If
    End Sub

    Private Sub buildMessageForm()
        ' redirect to form messages 
    End Sub

    Private Sub displayHeader(ByVal strHeaderModule As String,
                              ByVal strHeaderSubModule As String,
                              ByVal strHeaderArticle As String)
        Dim strHeader As String = ""
        Dim strHeaderDelim As String = ""
        If Trim(strHeaderModule) <> "" Then
            strHeader &= strHeaderDelim & strHeaderModule
            strHeaderDelim = " | "
        End If
        If Trim(strHeaderSubModule) <> "" Then
            strHeader &= strHeaderDelim & strHeaderSubModule
            strHeaderDelim = " | "
        End If
        If Trim(strHeaderArticle) <> "" Then
            strHeader &= strHeaderDelim & strHeaderArticle
        End If
        litTitleData.Text = strHeader
    End Sub

    Private Sub displayBody(ByVal strBody As String, ByVal strBodyImage As String, ByVal strBodyImageUrl As String, ByVal strBodyImageAlt As String)
        litBodyText.Text = strBody.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&quot;", "'").Replace("&nbsp;", " ").Replace("img", "img class=""shadow img-responsive"" ").Replace("<p>", "<div class=""row""><div class=""col-xs-12"">").Replace("</p>", "</div></div>")
        If Trim(strBodyImage) <> "" Then
            If System.IO.File.Exists(MapPath(strBodyImage.Replace("..", ""))) Then
                If strBodyImageUrl <> "" Then
                    litBodyImage.Text = "<a href=""" & strBodyImageUrl & """ ><img alt=""" & strBodyImageAlt & """ src=""" & strBodyImage & """ class=""shadow img-responsive"" border=""0"" /></a>"
                Else
                    litBodyImage.Text = "<img id=""MainContent_image_data"" alt=""" & strBodyImageAlt & """ src=""" & strBodyImage & """ class=""shadow img-responsive"" border=""0"" />"
                End If

            End If

        End If
    End Sub

    Private Sub displaySideMenu()
        ' see if we know what the top level module is
        Dim strWhere As String = ""
        Dim tblSubModules As DataTable = Nothing
        Dim tblArticles As DataTable = Nothing
        If m_intModuleRecId > -1 Then
        Else
            ' check for 1 level up - subModule
            If m_intSubModuleRecId > -1 Then
                strWhere = " recId = " & m_intSubModuleRecId
                tblSubModules = readSubModule(strWhere)
                If tblSubModules.Rows.Count > 0 Then
                    m_intModuleRecId = tblSubModules(0)("modulesRecId")
                End If
            Else
                ' check 1 more leve up - article
                strWhere = " recId = " & m_intArticleRecId
                tblArticles = readArticle(strWhere)
                If tblArticles.Rows.Count > 0 Then
                    strWhere = " recId = " & tblArticles(0)("subModulesRecId")
                    tblSubModules = readSubModule(strWhere)
                    If tblSubModules.Rows.Count > 0 Then
                        m_intModuleRecId = tblSubModules(0)("modulesRecId")
                    End If
                End If
            End If
        End If

        ' display as a list group
        Dim strSubMenu As String = "<div class=""list-group"">"
        Dim intSubModuleCounter As Integer = 0
        strWhere = " modulesRecId = " & m_intModuleRecId
        tblSubModules = readSubModule(strWhere)
        For Each subModule In tblSubModules.Rows
            strSubMenu &= "<a href=""frmModuleArticle.aspx?id=-1," & subModule("recid") & """ class=""list-group-item active"">" & _
                subModule("title") & "</a>"
            strWhere = " subModulesRecId = " & subModule("recid")
            tblArticles = readArticle(strWhere)
            For Each Article In tblArticles.Rows
                strSubMenu &= "<a href=""frmModuleArticle.aspx?id=-1,-1," & Article("recid") & """ class=""list-group-item""> " &
                Article("title") & "</a>"
            Next
            'strSubMenu &= "</div>"
            'If intSubModuleCounter > 0 Then
            '    strSubMenu &= "<hr />"
            'End If
            intSubModuleCounter += 1
        Next
        strSubMenu &= "  </div>"
        If Trim(strSubMenu) <> "" Then
            litSubMenu.Text = strSubMenu
        End If
    End Sub

    Private Function readModule(ByVal m_intModuleRecId As Integer) As DataTable
        Dim strSQL As String = "Select * from modules Where recId = " & m_intModuleRecId
        strSQL &= " Order by title"
        Dim tblModules As DataTable = g_IO_Execute_SQL(strSQL, False)
        Return tblModules
    End Function

    Private Function readSubModule(ByVal strWhere As String) As DataTable
        Dim strSQL As String = "Select * from subModules "
        If Trim(strWhere) <> "" Then
            strSQL &= " Where " & strWhere
        End If
        strSQL &= " Order by title"
        Dim tblSubModules As DataTable = g_IO_Execute_SQL(strSQL, False)
        Return tblSubModules
    End Function

    Private Function readArticle(ByVal strWhere As String) As DataTable
        Dim strSQL As String = "Select * From articles_vw "
        If Trim(strWhere) <> "" Then
            strSQL &= " Where " & strWhere
        End If
        strSQL &= " Order by title"
        Dim tblArticles As DataTable = g_IO_Execute_SQL(strSQL, False)
        Return tblArticles
    End Function

    Private Function readSubArticle(ByVal strWhere As String) As DataTable
        Dim strSQL As String = "Select * From subArticles_vw"
        If Trim(strWhere) <> "" Then
            strSQL &= " Where " & strWhere
        End If
        strSQL &= " Order by title"
        Dim tblSubArticles As DataTable = g_IO_Execute_SQL(strSQL, False)
        Return tblSubArticles
    End Function

    Private Function readSubArticleImages(ByVal intSubArticleImagesRecId As Integer) As DataTable
        Dim strSQL As String = "Select * From subArticleImages Where subArticlesRecId = " & intSubArticleImagesRecId & " Order By displayOrder "
        Dim tblSubArticleImages As DataTable = g_IO_Execute_SQL(strSQL, False)
        Return tblSubArticleImages
    End Function

    Private Function buildGallery(ByRef tblGallery As DataTable, _
                                   ByVal strGalleryType As String) As String
        Dim strHTMLOutput As String = ""
        If strGalleryType = "article" Then
            ' build gallery of images associated w/ article
            strHTMLOutput &= "   <div id=""links""> "
            Dim strGalleryEntry As String = ""
            Dim strGalleryImage As String = ""
            For Each entry In tblGallery.Rows
                strGalleryImage = getArticleImage(entry)
                strGalleryEntry &= strGalleryImage
            Next
            strHTMLOutput &= strGalleryEntry
            'close out gallery
            strHTMLOutput &= "   </div>  "
            strHTMLOutput &= "<script>" & _
                "    document.getElementById('links').onclick = function (event) {" & _
                "        event = event || window.event;" & _
                "        var target = event.target || event.srcElement," & _
                "            link = target.src ? target.parentNode : target," & _
                "            options = { index: link, event: event }," & _
                "            links = this.getElementsByTagName('a');" & _
                "        blueimp.Gallery(links, options);" & _
                "    };" & _
                "    </script>"
        Else
            ' build list group of articles associated w/ subModule            
            strHTMLOutput &= "<div class=""panel-group"" id=""accordion"" role=""tablist"" aria-multiselectable=""true"">"
            Dim strListEntry As String = ""
            Dim strListText As String = ""
            Dim intListNumber As Integer = 1
            For Each entry In tblGallery.Rows
                strListText = getArticleList(entry, CType(intListNumber, String))
                strListEntry &= strListText
                intListNumber += 1
            Next
            strHTMLOutput &= strListEntry
            'close out list group
            strHTMLOutput &= "   </div>  "
        End If

        Return strHTMLOutput
    End Function

    Private Function getArticleImage(ByVal galleryEntry As DataRow) As String
        Dim strImage As String = _
            "<div class=""col-xs-12 col-sm-6"" style=""padding-bottom:10px"">" & _
            "<a href=""" & galleryEntry("image") & """ title=""" & galleryEntry("imageText") & """>       " & _
            "  <img class=""img-responsive"" src=""" & galleryEntry("image") & """ alt=""" & galleryEntry("imageText") & """" & _
            "  height=""186"" width=""247"" /> " & _
            "</a>" & _
            "</div> "

        Return strImage
    End Function

    Private Function getArticleList(ByVal listEntry As DataRow, ByVal listNumber As String) As String
        ' get subArticle to link w/in heading
        Dim strSubArticleLink As String = "#"
        Dim tblSubArticle As DataTable = g_IO_Execute_SQL("select top(1) * from articles_vw where Recid = " & listEntry("recid"), False)
        If tblSubArticle.Rows.Count > 0 Then
            strSubArticleLink = "frmModuleArticle.aspx?id=-1,-1," & listEntry("recid") 'tblSubArticle.Rows(0)("recid")
        End If
        Dim strPanelOptions As String = "aria-expanded=""true"""
        Dim strDivOptions As String = "class=""panel-collapse collapse in"""
        If listNumber > 1 Then
            strPanelOptions = "class=""collapsed"" aria-expanded=""false"""
            strDivOptions = "class=""panel-collapse collapse"""
        End If
        ' build up an list group item
        Dim strListText As String = _
        "  <div class=""panel panel-default"">" & _
        "    <div class=""panel-heading"" role=""tab"" id=""heading" & listNumber & """ > " & _
        "      <h4 class=""panel-title"">" & _
        "        <a " & strPanelOptions & " role=""button"" data-toggle=""collapse"" data-parent=""#accordion"" href=""#collapse" & listNumber & """ aria-controls=""collapse" & listNumber & """>" & _
                  listEntry("title") & _
        "        </a>" & _
        "      </h4>" & _
        "    </div>" & _
        "    <div id=""collapse" & listNumber & """" & strDivOptions & " role=""tabpanel"" aria-labelledby=""heading" & listNumber & """>" & _
        "      <div class=""panel-body"">" & _
                  listEntry("short_body") & _
        "      </div>" & _
        "      <div class=""panel-footer""><a href=""" & strSubArticleLink & """>Read More</a></div>" & _
        "    </div>" & _
        "  </div>"
        Return strListText
    End Function

    Private Function carouselSubModuleLi(ByVal carouselEntry As DataRow) As String

        Dim strCarouselLi As String = _
                    "<li class=""page"" onclick=""redirect('frmModuleArticle.aspx?id=-1,-1," & carouselEntry("recid") & "')"" >" & _
                    "<strong><a class='menulink' href=""frmModuleArticle.aspx?id=-1,-1," & _
                    carouselEntry("recid") & """>" & _
                    carouselEntry("title") & "</a></strong><p >" & _
                    carouselEntry("short_body") & "</p>" & _
                    "</li>" & vbCrLf
        Return strCarouselLi
    End Function

End Class