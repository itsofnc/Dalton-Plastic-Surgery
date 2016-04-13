Imports System.Net.Mail
Imports System.Net.Mail.SmtpClient
Imports System.Web.HttpContext
Imports System.Configuration

' 2/16/15 t3 Imports for encryption functions
Imports System
Imports System.IO
Imports System.Xml
Imports System.Text
Imports System.Security.Cryptography

Module ModMain
    '***********************************Last Edit**************************************************
    'Last Edit Date: 06/26/15
    'Last Edit By: t3
    'Last Edit Proj: T3CommonCode
    '-----------------------------------------------------
    'Change Log: 
    ' 06/26/15 t3 check to see if we have a key to auto login when debugging
    ' 03/06/15 t3 email timeout per install
    ' 02/16/15 t3 add encrypt/decrpt functions
    ' 02/13/15 changed to use default email address instead of user name
    ' 02/04/15 cpb match up to web.config -- validated to patient portal needs
    ' 01/22/15 cpb get project title for generic forms use
    ' 01/19/15 T3 Set home links for Admin or Default users
    ' 01/09/15 CPB The replace caused issues when address not defined in web.config
    ' 11/12/14 - cpb
    '   add g_loadStateDropDown to load states dropdown list
    ' 11/11/14 - cpb
    '   -add user recid
    ' 09/29/14 -t3- Added newInstall flag and getFieldTypes variables (see litScripts in Admin Master)
    ' 09/17/14 - cpb
    '   Added site display name b/c of error pulling common code in.
    ' 7/11/14 - t3
    '   clean to all common code/ remove patient/mod auto coder specifics.
    '    
    '**********************************************************************************************
    ' session variables that will be available by default
    ' Company Data------------------
    '   Session("CompanyContactName") 
    '   Session("CompanyPhone")
    '   Session("CompanyName") 
    '   Session("CompanyShortName") 
    '   Session("CompanyAddress")
    '   Session("CompanyEmailAddress")
    '   Session("CompanyURL")
    '   Session("CompanyRecid")
    ' Session Preservation ----------
    '   Session("preserveSession")
    ' Demo Variables - Only in apps w/ demo mode available
    '   Session("DemoMode")
    '   Session("DemoSchema")

    ' ---Default global variables
    Public g_Debug As Boolean = False
    Public g_strFormObjectTypes = "input+select+textarea"
    Public g_StringArrayOuterSplitParameter = "||"
    Public g_StringArrayValueSplitParameter = "~~"
    Public g_UserRecId As Integer = -1                   ' 11/11/15 cpb needed for sql_update modio

    '-------------------------------------------------Build Web Config Info----------------------------------------------------------------------------
    ' Build global variables from web config
    '---New Install
    ' 09/29/14 T3
    Public g_newInstall As String = ConfigurationManager.AppSettings("newInstall")

    '---Site Keys
    Public g_SiteUrl As String = ConfigurationManager.AppSettings("siteUrl")
    Public g_SiteDisplayName As String = ConfigurationManager.AppSettings("SiteDisplayName")    ' 9/17/14

    '---Application Keys-->
    'moduleCode---used in autocode    
    'Public g_projectTitle As String = ConfigurationManager.AppSettings("ProjectTitle")          ' 01/22/15 cpb get project title for generic forms use
    Public g_loginPage As String = ConfigurationManager.AppSettings("loginPage")
    Public g_LoginIdStyle As String = ConfigurationManager.AppSettings("LoginIdStyle")
    Public g_DefaultStateRecid As Integer = IIf(ConfigurationManager.AppSettings("defaultStateRecid") = "", ConfigurationManager.AppSettings("T3defaultStateRecid"), ConfigurationManager.AppSettings("defaultStateRecid")) ' 91  North Carolina

    '---Company Keys-->
    Public g_CompanyLongName As String = ConfigurationManager.AppSettings("CompanyName")
    Public g_CompanyShortName As String = ConfigurationManager.AppSettings("CompanyShortName")
    Public g_CompanyLogo As String = ConfigurationManager.AppSettings("LogoImageFullPath")
    Public g_CompanyMainPhone As String = ConfigurationManager.AppSettings("MainPhoneContact")
    ' 01/09/15 CPB The replace caused issues when address not defined in web.config
    ' will need to do the .Replace wherever you use this variable in your app
    'Public g_CompanyAddress As String = ConfigurationManager.AppSettings("Address").Replace("||", "<br>")
    Public g_CompanyAddress As String = ConfigurationManager.AppSettings("CompanyAddress")

    '---Email Key-->
    Public g_EmailEnabled As Boolean = IIf(UCase(ConfigurationManager.AppSettings("emailEnabled")) = "TRUE", True, False)
    Public g_EmailHostIPAddress As String = ConfigurationManager.AppSettings("emailIPAddress")
    Public g_EmailTimeout As Integer = ConfigurationManager.AppSettings("emailTimeout")     ' 03/06/15 email timeout control per install
    Public g_EmailPort As String = ConfigurationManager.AppSettings("emailPort")
    Public g_EmailUserName As String = ConfigurationManager.AppSettings("emailUserName")
    Public g_EmailPassword As String = ConfigurationManager.AppSettings("emailPassword")
    Public g_emailEnableSSL As String = ConfigurationManager.AppSettings("emailEnableSSL")
    Public g_defaultEmail As String = ConfigurationManager.AppSettings("defaultEmail")

    '--------Audit Keys----08/06/15 T3-->
    Public g_errorLog As Boolean = IIf(UCase(ConfigurationManager.AppSettings("errorLog")) = "TRUE", True, False)
    Public g_auditUpdates As Boolean = IIf(UCase(ConfigurationManager.AppSettings("auditUpdates")) = "TRUE", True, False)
    '-------------------------------------------------Build Web Config Info----------------------------------------------------------------------------




    '-----------------These are being worked on for new login
    ' 01/19/15 T3 Set home links for Admin or Default users
    ' Public g_AdminHome As String = ConfigurationManager.AppSettings("AdminHome")
    ' Public g_DefaultHome As String = ConfigurationManager.AppSettings("DefaultHome")
    ' Public g_UnconfirmedAdminHome As String = ConfigurationManager.AppSettings("unconfirmedAdminHome")
    ' Public g_UnconfirmedDefaultHome As String = ConfigurationManager.AppSettings("unconfirmedDefaultHome")
    ' 01/19/15 T3 Setting to determine whether or not to show new user div & confirm new users
    'Public g_allowNewUser As Boolean = IIf(UCase(ConfigurationManager.AppSettings("allowNewUser")) = "TRUE", True, False)
    'Public g_confirmNewUser As Boolean = IIf(UCase(ConfigurationManager.AppSettings("confirmNewUser")) = "TRUE", True, False)

    

    Public Sub g_resetSessionVariables()
        System.Web.HttpContext.Current.Session("preserveSession") = ""
    End Sub

    Public Sub g_RetrieveSessions(ByRef txtSessions As TextBox)
        g_RetrieveSessions(txtSessions.Text)
    End Sub
    Public Sub g_RetrieveSessions(ByRef txtSessions As HiddenField)
        g_RetrieveSessions(txtSessions.Value)
    End Sub
    Public Sub g_RetrieveSessions(ByRef txtSessions As String)

        If Trim(txtSessions = "") Then
            ' no Sessions string area sent to client form
        Else
            Dim arrStrSessionVariables() As String = Split(txtSessions, "^^")


            ' restore session variables
            For Each strSessionVariable As String In arrStrSessionVariables
                Dim strSessionVariablePair() As String = Split(strSessionVariable, "||")
                System.Web.HttpContext.Current.Session(strSessionVariablePair(0)) = strSessionVariablePair(1)
            Next
        End If

    End Sub
    Public Sub g_SendSessions(ByRef txtSessions As TextBox)
        txtSessions.Text = g_SendSessions(txtSessions.Text)
    End Sub
    Public Sub g_SendSessions(ByRef txtSessions As HiddenField)
        txtSessions.Value = g_SendSessions(txtSessions.Value)
    End Sub
    Public Function g_SendSessions(ByRef txtSessions As String) As String

        '  Only do this if user is not signed on
        Dim strSessionsName As String = ""
        Dim strDelimiter As String = ""

        For Each txtFieldName As String In System.Web.HttpContext.Current.Session.Keys
            If txtFieldName.ToUpper = "RELAYMESSAGE" Then
            Else
                strSessionsName = strSessionsName & strDelimiter & txtFieldName & "||" & System.Web.HttpContext.Current.Session(txtFieldName)
                strDelimiter = "^^"
            End If
        Next

        Return strSessionsName

    End Function

    Public Sub g_ValidatePhone(ByRef PhoneTextBox As TextBox)
        PhoneTextBox.Attributes.Add("onfocus", "PhoneFocus(this);")
        PhoneTextBox.Attributes.Add("onblur", "PhoneBlur(this);")
        PhoneTextBox.Attributes.Add("onkeyup", "PhoneChange(this);")
        PhoneTextBox.Attributes.Add("onkeypress", "PhoneKey(this,event);")
        PhoneTextBox.Attributes.Add("onchange", "PhoneChange(this);")
        'PhoneTextBox.Attributes.Add("onpropertychange", "PhoneFocus(this);")   ' covers autofill

    End Sub
    Public Sub g_ValidateDate(ByRef DateTextBox As TextBox)
        DateTextBox.Attributes.Add("onfocus", "DateFocus(this);")
        DateTextBox.Attributes.Add("onblur", "DateBlur(this);")
        DateTextBox.Attributes.Add("onkeyup", "DateChange(this);")
        DateTextBox.Attributes.Add("onkeypress", "DateKey(this,event);")
        DateTextBox.Attributes.Add("onchange", "DateChange(this);")

    End Sub
    Public Sub g_TextMaxLengthEntry(ByRef TextBoxUsed As TextBox, ByVal MaxLength As Integer)
        TextBoxUsed.Attributes.Add("onkeyup", "TextLimitMessage(this," & MaxLength & ");")
        TextBoxUsed.Attributes.Add("onblur", "TextLimitMessageRemove(this);")

    End Sub
    Public Sub g_sendEmail(ByVal ToAddress As String, ByVal Subject As String, ByVal Message As String)
        g_sendEmail(ToAddress, Subject, Message, "")
    End Sub

    Public Sub g_sendEmail(ByVal ToAddress As String, ByVal Subject As String, ByVal Message As String, ByVal CCAddress As String)

        'Debug.WriteLine("Email Module -- To: " & ToAddress & "  Subject: " & Subject & "   Message: " & Message)

        If g_EmailEnabled = True Then
            Dim Mail As New System.Net.Mail.MailMessage
            Mail.Subject = Subject
            If ToAddress = "" Then
                'Debug.Print("ModMain (g_SendEmail): No email address provided. Can't send it.")
            Else
                ToAddress = ToAddress.Replace(",", ";")
                For Each strEmailAddress As String In Split(ToAddress, ";")
                    Mail.To.Add(strEmailAddress)
                Next
                If CCAddress = "" Then
                Else
                    CCAddress = CCAddress.Replace(",", ";")
                    For Each strEmailAddress As String In Split(CCAddress, ";")
                        Mail.CC.Add(strEmailAddress)
                    Next
                End If

                'Mail.From = New System.Net.Mail.MailAddress(g_EmailUserName)
                ' 2/13/15 changed to use default email address instead of user name
                Mail.From = New System.Net.Mail.MailAddress(g_defaultEmail)
                Mail.Body = Message
                Dim strHTMLCheck As String = UCase(Message)
                Mail.IsBodyHtml = strHTMLCheck.ToUpper.Contains("<BODY") Or strHTMLCheck.ToUpper.Contains("<TABLE") Or strHTMLCheck.ToUpper.Contains("<DIV") Or strHTMLCheck.ToUpper.Contains("<BR") Or strHTMLCheck.ToUpper.Contains("<P")
                Dim SMTPServer As New System.Net.Mail.SmtpClient()
                SMTPServer.Timeout = g_EmailTimeout    ' 03/06/15 control per install     '20000 '5000
                SMTPServer.Host = g_EmailHostIPAddress
                If g_emailEnableSSL = True Then
                    SMTPServer.EnableSsl = True
                End If
                If g_EmailPort <> "" Then
                    SMTPServer.Port = g_EmailPort
                End If
                SMTPServer.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network
                SMTPServer.Credentials = New System.Net.NetworkCredential(g_EmailUserName, g_EmailPassword)
                'Debug.Print(ToAddress & " - " & Subject)
                If g_EmailEnabled Then
                    SMTPServer.Send(Mail)
                End If
            End If
        End If
    End Sub
    '09/29/14 T3- Created
    Public Sub g_changeAppSettings(ByVal key As String, ByVal NewValue As String)
        Dim cfg As Configuration
        cfg = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~")
        Dim setting As KeyValueConfigurationElement = CType(cfg.AppSettings.Settings(key), KeyValueConfigurationElement)
        If setting Is Nothing Then
        Else
            setting.Value = NewValue
            cfg.Save()
        End If
    End Sub
    Public Sub g_loadStateDropDown(ByRef ddlState As DropDownList, ByVal blnAddHeaderRow As Boolean)
        ' 11/12/14 cpb common routine that can be used on all forms to load states dropdown
        Dim strSQL As String = ""
        strSQL = " Select RECID, Abbr, state FROM states "

        Dim tblStates As DataTable = g_IO_Execute_SQL(strSQL, False)

        If blnAddHeaderRow Then
            Dim rowState As DataRow = tblStates.NewRow
            rowState("State") = "State"
            rowState("RECID") = -1
            tblStates.Rows.InsertAt(rowState, 0)
        End If

        ddlState.DataSource = tblStates
        ddlState.DataValueField = "RECID"
        ddlState.DataTextField = "State"
        ddlState.DataBind()
        ddlState.SelectedValue = g_DefaultStateRecid
    End Sub

    ' 02/16/15 t3 add encryption/decription to string routines
    Private key() As Byte = {}
    Private IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}
    Private Const EncryptionKey As String = "abcdefgh"
    Public Function g_Decrypt(ByVal stringToDecrypt As String) As String
        Try
            Dim inputByteArray(stringToDecrypt.Length) As Byte
            key = System.Text.Encoding.UTF8.GetBytes(Left(EncryptionKey, 8))
            Dim des As New DESCryptoServiceProvider
            inputByteArray = Convert.FromBase64String(stringToDecrypt)
            Dim ms As New MemoryStream
            Dim cs As New CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write)
            cs.Write(inputByteArray, 0, inputByteArray.Length)
            cs.FlushFinalBlock()
            Dim encoding As System.Text.Encoding = System.Text.Encoding.UTF8
            Return ConvertHexToString(encoding.GetString(ms.ToArray()))
        Catch ex As Exception
            'oops - add your exception logic
            Return stringToDecrypt
        End Try
    End Function
   
    Public Function g_Encrypt(ByVal stringToEncrypt As String) As String
        Try
            stringToEncrypt = ConvertStringToHex(stringToEncrypt)

            key = System.Text.Encoding.UTF8.GetBytes(Left(EncryptionKey, 8))
            Dim des As New DESCryptoServiceProvider
            Dim inputByteArray() As Byte = Encoding.UTF8.GetBytes(stringToEncrypt)
            Dim ms As New MemoryStream
            Dim cs As New CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write)
            cs.Write(inputByteArray, 0, inputByteArray.Length)
            cs.FlushFinalBlock()
            Return Convert.ToBase64String(ms.ToArray())
        Catch ex As Exception
            'oops - add your exception logic
            Return stringToEncrypt
        End Try
    End Function
    Public Function ConvertStringToHex(ByVal strToConvert As String) As String
        Dim arrBytes() = strToConvert.ToCharArray
        Dim strHexStringReturn As String = ""
        For Each strChar As Char In arrBytes
            strHexStringReturn &= Hex(Asc(strChar))
        Next
        Return strHexStringReturn
    End Function
    Public Function ConvertHexToString(ByVal HexToConvert As String) As String
        Dim numberChars As Integer = HexToConvert.Length
        Dim strStringToReturn As String = ""
        For i = 1 To numberChars Step 2
            strStringToReturn &= Chr(Convert.ToInt32(Mid(HexToConvert, i, 2), 16))
        Next
        Return strStringToReturn

    End Function

    Public Sub g_flmSaveFunction(ByVal functionToCall As String, ByVal strTable As String, ByVal intRecid As Integer)
        ' Called from frmListManager: Table has ext prop to run a save function
        Dim blnSaveFunction As Boolean = functionToCall & "(" & strTable & ", " & intRecid & ")"
    End Sub

    Public Sub g_DebugAutoLogin()
        ' 6/26/15 this will be launched from frmLogin Load event
        ' required key in web.config ConfigurationManager.AppSettings("debugAutoLogin") = "true"
        If Debugger.IsAttached Then
            System.Web.HttpContext.Current.Session("user_link_id") = "1"
            System.Web.HttpContext.Current.Session("user_role") = "ADMINISTRATOR"
            System.Web.HttpContext.Current.Session("user_name") = "Admin"
            System.Web.HttpContext.Current.Response.Redirect(g_LoginRedirect("Login"))
        End If
    End Sub
   
End Module
