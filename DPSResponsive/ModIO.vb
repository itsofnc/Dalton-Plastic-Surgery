Imports System.Collections.Specialized

Module ModIO

    '***********************************Last Edit**************************************************

    'Last Edit Date: 5/19/15 
    'Last Edit By: T3
    'Last Edit Proj: T3CommonCode
    '-----------------------------------------------------
    'Change Log: 
    ' 5/19/15 T3 - added ext prop for regexp
    ' 3/9/15 T3 - added g_GetIndexes to look at table indexes (for unique field validation)

    '02/20/15 T3 - added to g_GetTableExtProperty to handle custom links 
    ' 02/02/15 cpb add ext propery for column display location left,right,center
    ' 01/15/15 CP =ext property not case sensitive 
    ' 01/06/15 T3 - moved check for table/view to common function, used in multiple locations
    ' 01/06/15 T3 - look for list of fields to enable (at table level)
    ' 01/05/2015 RLO - a view or table might have square brackets around it, if so must be removed
    '11/11/14 - cpb
    '   added sql_update routine for updating last mod date and last mod user-rquired adding g_UserRecId to modmain
    '10/13/14 - added g_GetTableExtProperty to get extended properties (was somewhat functioning inside g_GetTableDescrip...)
    '09/29/14-t3 - added required field
    '09/15/14-t3 - added g_GetTableDescription (moved from frmListManager)
    '09/08/14 cs/cb - determine if ext property comes from view or table
    '08/29/14 - t3
    ' added  nvcColumnIndex to getColumns
    '08/27/14 - CS/RO
    '   If statement in Try:Catch of IO_Execute_SQL to check for DBNull b/c slows program to halt if lots of null values exist

    '7/14/14 - t3
    '   added insert & delete SQL IO call with nvc

    '7/11/14 - t3
    '   clean to all common code/ remove patient/mod auto coder specifics.
    '   IO_ExecuteSQL-check for session variable called connectionString 

    '**********************************************************************************************
    '**********************************Local Variable Definition**********************************
    '
    ' these variables are defined in web.config
    Public g_intNumberOfRetriesToAccessDatabase As Integer = ConfigurationManager.AppSettings("inNumberOfRetriesToAccessDatabase")
    Public g_ConnectionToUse As String = ConfigurationManager.AppSettings("ConnectionType")
    Public g_ConnectionSchema As String = ConfigurationManager.AppSettings("schema")
    Public g_strConnectionString As String = ConfigurationManager.ConnectionStrings("ConnectionString").ToString

    Public g_strIOError As String = ""
    Public g_blnAbort As Boolean = False

    Private m_nvcTables As New NameValueCollection               ' key=tablename, data=index to nvcFields 
    Private m_nvcFields() As NameValueCollection                 ' (=index#fromnvcTables),Key=FieldName,Data=index to nvcFieldAttributes
    Private m_nvcFieldAttributes() As NameValueCollection        ' (=index#fromnvcFields),Key=AttributeName,Data=AttributeValue

    '08/06/15 T3
    Public g_auditUpdatesTblExists As String = False
    Public g_ErrorTblExists As String = False

    '
    '**********************************Public Sub-Routines for Data IO**********************************
    Public Function g_IO_Execute_SQL(ByVal strSQL As String, ByRef blnReturnErrorCode As Boolean) As DataTable
        Return g_IO_Execute_SQL(strSQL, blnReturnErrorCode, "MSSQL")
    End Function
    Public Function g_IO_Execute_SQL(ByVal strSQL As String, ByRef blnReturnErrorCode As Boolean, ByVal strDatabaseType As String) As DataTable

        Dim blnInsert As Boolean = False
        If strSQL.ToUpper.IndexOf("INSERT ") = 0 Then
            Try
                '01/21/15 RLO move the code to capture the last recid up to the top of the segment.
                strSQL &= ";Select @@IDENTITY as ID;"    ' on MSSQL inserts must retrieve the new RECID now
                blnInsert = True

                ' convert MySQL True/False references to 0/1
                Dim strFields As String = strSQL.Substring(0, strSQL.ToUpper.IndexOf(" VALUES"))
                Dim strValues As String = strSQL.Substring(strSQL.ToUpper.IndexOf(" VALUES"))
                strValues = strValues.Replace(", True", ", 1")
                strValues = strValues.Replace(",True", ",1")
                strValues = strValues.Replace(", TRUE", ", 1")
                strValues = strValues.Replace(",TRUE", ",1")
                strValues = strValues.Replace(", False", ", 0")
                strValues = strValues.Replace(",False", ",0")
                strValues = strValues.Replace(", FALSE", ", 0")
                strValues = strValues.Replace(",FALSE", ",0")
                strSQL = strFields & strValues

            Catch : End Try
        ElseIf strSQL.ToUpper.IndexOf("UPDATE ") = 0 Then
            Try
                ' convert MySQL True/False references to 0/1
                Dim strFields As String = strSQL.Substring(0, strSQL.ToUpper.IndexOf(" SET"))
                Dim strValues As String = strSQL.Substring(strSQL.ToUpper.IndexOf(" SET"))
                strValues = strValues.Replace("= True", "=1")
                strValues = strValues.Replace("=True", "=1")
                strValues = strValues.Replace("= TRUE", "= 1")
                strValues = strValues.Replace("=TRUE", "=1")
                strValues = strValues.Replace("= False", "= 0")
                strValues = strValues.Replace("=False", "=0")
                strValues = strValues.Replace("= FALSE", "= 0")
                strValues = strValues.Replace("=FALSE", "=0")
                strSQL = strFields & strValues
            Catch : End Try
        ElseIf strSQL.ToUpper.IndexOf("SELECT ") = 0 Then
            If strSQL.ToUpper.IndexOf("CONCAT(") = -1 Then
            Else
                Call convertConcatStatement(strSQL)
            End If
        End If

        Try
            Dim tblTemp As DataTable = IO_Execute_MSSQL(strSQL, blnReturnErrorCode)
            If strSQL.ToUpper.IndexOf("SELECT ") = 0 Then
                ' trim all text entries
                For Each rowTemp As DataRow In tblTemp.Rows
                    For i = 0 To tblTemp.Columns.Count - 1
                        Try
                            ' 08/27/14 CS/RO
                            If IsDBNull(rowTemp(i)) Then
                            Else
                                rowTemp(i) = Trim(rowTemp(i))
                            End If
                            : Catch : End Try
                    Next
                Next
            ElseIf blnInsert Then
                System.Web.HttpContext.Current.Session("NewMSSQLRECID") = tblTemp.Rows(0)("ID")
            End If

            g_auditLog(strSQL)

            Return tblTemp
        Catch ex As Exception
            System.Web.HttpContext.Current.Response.Write("*SQL_ERROR*" & ex.Message & "*SQL_ERROR*")
            g_logError(strSQL, ex.Message)
            Return Nothing
        End Try

    End Function

    Private Sub convertConcatStatement(ByRef strSQL As String)

        Dim strEndOfFieldMarker As String = ","
        Dim blnThisCharIsAQuote As Boolean = False
        Dim intIndex As Integer = 0
        Dim strFieldValue As String = ""
        Dim blnEndOfFieldFound As Boolean = False
        Dim intLargestRowIndex As Integer = 0
        Dim blnConversionComplete As Boolean = False



        Dim intStart As Integer = strSQL.ToUpper.IndexOf("CONCAT(")
        Dim strFrontSQL As String = Left(strSQL, intStart) & "("
        Dim strBackSQL As String = Mid(strSQL, intStart + 8)
        Dim I As Integer = 1

        Do Until blnConversionComplete
            Dim strCurrentChar As String = Mid(strBackSQL, I, 1)

            blnEndOfFieldFound = False

            If strCurrentChar = ")" And (strEndOfFieldMarker = "," Or strEndOfFieldMarker = "") Then

                Exit Do
            ElseIf strEndOfFieldMarker = "" Then
                ' end the middle of evaluating a string

                Select Case strCurrentChar
                    Case ","
                        strEndOfFieldMarker = ","
                        strFrontSQL &= " + "
                    Case "'"
                        strEndOfFieldMarker = "'"
                End Select
            ElseIf strEndOfFieldMarker = "'" Then

                ' this is a string under evaluation
                If Mid(strBackSQL, I, 2) = "''" Then
                    'this is a quote within the string
                    blnThisCharIsAQuote = True
                    I += 1   ' this quote takes up two spaces
                Else
                    blnThisCharIsAQuote = False
                End If

                If blnThisCharIsAQuote Then
                    strFrontSQL &= "'"
                Else
                    ' is this the end of the string?
                    If strCurrentChar = strEndOfFieldMarker Then
                        strFrontSQL &= "'"
                        strEndOfFieldMarker = ""
                        intIndex += 1
                    Else
                        ' build current field char by char
                        strFrontSQL &= strCurrentChar
                    End If

                End If


            Else
                ' this should be a field name (might not be if there are spaces before the starting quote of a string)
                If strCurrentChar = "'" Then
                    ' oops, this is a string after all
                    strEndOfFieldMarker = "'"
                    strFrontSQL &= "'"
                ElseIf strCurrentChar = strEndOfFieldMarker Then
                    strEndOfFieldMarker = ","
                    strFrontSQL &= " + "
                    intIndex += 1
                Else
                    strFrontSQL &= strCurrentChar
                End If
            End If

            I += 1
        Loop

        strSQL = strFrontSQL & Mid(strBackSQL, I)

    End Sub

    Public Function g_IO_ReadPageOfRecords(ByVal strSQL As String, ByVal intSkip As Integer, ByVal intNumberOfRecords As Integer, ByRef blnReturnErrorCode As Boolean) As DataTable

        If g_ConnectionToUse = "MYSQL" Then
            strSQL &= " limit " & intSkip & "," & intNumberOfRecords
        Else
            Dim strOrderBy As String = UCase(strSQL.Substring(UCase(strSQL).IndexOf(" ORDER BY ")))  ' extract Order By clause

            Dim strOrderByReversed As String = (strOrderBy)

            '  When adding asc or desc to grid this needs to be fixed
            If (strOrderBy & " ").Contains(" ASC ") Then
                strOrderByReversed = (strOrderByReversed & " ").Replace(" ASC ", " DESC ")
            Else
                strOrderBy = (strOrderBy & " ").Replace(" ASC ", " DESC ")
                strOrderByReversed = (strOrderByReversed & " ").Replace(" DESC ", " ASC ")
            End If

            '            Dim strOrderByReversed As String = (strOrderBy & " ").Replace(" ASC ", " %%").Replace(" DESC ", " ASC ").Replace("%%", "ASC")
            ''strSQL = strSQL.Substring(0, UCase(strSQL).IndexOf(" ORDER BY "))   ' remove Order By clause
            strSQL = "Select * from (" & _
                " Select top " & intNumberOfRecords & " * from (" & _
                strSQL.Substring(0, UCase(strSQL).IndexOf("SELECT") + 6) & _
                " Top " & intSkip + intNumberOfRecords & " " & _
                strSQL.Substring(UCase(strSQL).IndexOf("SELECT") + 6) & _
                ") as Dummy1 " & strOrderByReversed & _
                ") as Dummy2 " & strOrderBy
        End If

        Return g_IO_Execute_SQL(strSQL, blnReturnErrorCode)
    End Function

    Public Function g_IO_GetLastRecId() As Integer
        Dim intLastRecId As Integer
        If g_ConnectionToUse = "MYSQL" Then
            intLastRecId = System.Web.HttpContext.Current.Session("NewMYSQLRECID")
        Else
            intLastRecId = System.Web.HttpContext.Current.Session("NewMSSQLRECID")
        End If
        Return intLastRecId
    End Function

    Public Function g_IO_GetLastRecId(ByVal connection As String) As Integer
        Dim intLastRecId As Integer
        If g_ConnectionToUse = "MYSQL" Or connection = "MYSQL" Then
            intLastRecId = System.Web.HttpContext.Current.Session("NewMYSQLRECID")
        Else
            intLastRecId = System.Web.HttpContext.Current.Session("NewMSSQLRECID")
        End If
        Return intLastRecId
    End Function
    '
    '**********************************Private Functions - SQL Executes**********************************
    '
    '
    '_____ sql final executions functions 
    ' 


    Private Function IO_Execute_MSSQL(ByVal strSQL As String, ByRef ReturnCode As Boolean) As DataTable

        'System.Web.HttpContext.Current.Response.Write(ConfigurationManager.ConnectionStrings("ConnectionString").ToString & "<br>")

        Try

            '            System.Web.HttpContext.Current.Response.Write("IO_Execute: 1<br>")
            Dim adapt As New SqlClient.SqlDataAdapter

            Dim blnProcessWasSuccessful As Boolean = True
            Dim blnRetry As Boolean = True
            Dim tblDatatable As New DataTable
            '           System.Web.HttpContext.Current.Response.Write("IO_Execute: 2<br>")

            Dim g_strConnectionStringMSSQL As New System.Data.SqlClient.SqlConnection

            ' System.Web.HttpContext.Current.Response.Write("IO_Execute: 3<br>")

            Dim strConnectionString As String = ConfigurationManager.ConnectionStrings("ConnectionString").ToString

            ' 07/01/14 CS/RO Get correct connection string for demo database
            If IsNothing(HttpContext.Current.Session("connectionString")) Then
                g_strConnectionStringMSSQL = New System.Data.SqlClient.SqlConnection(g_strConnectionString)
            Else
                g_strConnectionStringMSSQL = New System.Data.SqlClient.SqlConnection(HttpContext.Current.Session("connectionString"))
            End If

            'System.Web.HttpContext.Current.Response.Write("IO_Execute: 4<br>")

            Dim command As New SqlClient.SqlCommand(strSQL, g_strConnectionStringMSSQL)
            Debug.WriteLine(strSQL)
            'System.Web.HttpContext.Current.Response.Write("IO_Execute: 5<br>")

            command.CommandTimeout = 600
            adapt.SelectCommand = command

            Dim strErrorCode As String = ""
            For i = 1 To g_intNumberOfRetriesToAccessDatabase
                adapt.Fill(tblDatatable)
                'System.Web.HttpContext.Current.Response.Write("IO_Execute: 6<br>")
                ReturnCode = True
                blnRetry = False
                Return tblDatatable
                Exit For
            Next
        Catch ex As Exception

            ReturnCode = False
            'Will throw exception message back to g_ioExecute
            Throw New System.Exception(ex.Message)
        End Try
        Return Nothing
    End Function

    Public Function g_GetTableDescription(ByVal TableName As String, ByVal extPropertyName As String) As String
        Dim strTableDescription As String = g_GetTableExtProperty(TableName, extPropertyName)
        If strTableDescription = "" Then
            If TableName.Contains("__") Then
                strTableDescription = Split(TableName, "__")(1)
            End If
            strTableDescription = strTableDescription.Replace("_", " ")
        End If
        Return strTableDescription
    End Function

    ' 03/16/15 T3 add ability to get exteneded property for specific column within the table
    Public Function g_GetColumnExtPropertyValue(ByVal TableName As String, ByVal extPropertyName As String, ByVal Column As String) As String
        Dim strColumnDescription As String = ""
        TableName = TableName.Trim("[").Trim("]")
        Dim strJoinTo As String = determineViewOrTable(TableName, False)
        Dim strSQL As String = "SELECT isnull(sep.name, '') [extName], isnull(sep.value, '') [extValue], sc.name [Column] FROM " & strJoinTo & " st " & _
          "inner join sys.columns sc on st.object_id = sc.object_id " & _
          "left join sys.extended_properties sep on st.object_id = sep.major_id and sc.column_id = sep.minor_id " & _
          "left outer join INFORMATION_SCHEMA.COLUMNS as ic on ic.TABLE_NAME = st.name and ic.COLUMN_NAME = sc.name " & _
          "Where st.name = '" & TableName & _
          "' and sc.name='" & Column & "' and sep.name='" & extPropertyName & "'"

        Dim tblColumns = g_IO_Execute_SQL(strSQL, False)
        If tblColumns.Rows.Count > 0 Then
            strColumnDescription = tblColumns.Rows(0)("extValue")
        End If

        Return strColumnDescription
    End Function

    Public Function g_GetTableExtProperty(ByVal TableName As String, ByVal extPropertyName As String) As String
        ' 01/05/2015 RLO - a view or table might have square brackets around it, if so must be removed
        TableName = TableName.Trim("[").Trim("]")

        Dim strExtPropertyValue As String = ""

        ' look for an extended properties description for the table
        ' 09/08/14 cs/cb - determine if ext property comes from view or table
        ' 01/06/15 T3 moved check for table/view to common function, used in multiple locations
        Dim strJoinTo As String = determineViewOrTable(TableName, False)

        Dim strSQL As String = "select p.value, p.name from sys.extended_properties p inner join " & strJoinTo & " t on p.major_id = t.object_id where class = 1 and " & _
                    "t.name = '" & TableName & "' and p.name = '" & extPropertyName & "'"
        If extPropertyName = "link" Then
            strSQL = strSQL.Replace("p.name = ", "p.name like ").Replace(extPropertyName, extPropertyName & "%")
        End If
        Dim tblExtDescr As DataTable = g_IO_Execute_SQL(strSQL, False)
        Dim strDelim As String = ""
        If tblExtDescr.Rows.Count > 0 Then
            For Each rowPropName In tblExtDescr.Rows
                If extPropertyName = "link" Then
                    ' 03/06/15 need to same ext prop name if a link for unique key
                    strExtPropertyValue &= strDelim & rowPropName("name") & "++" & rowPropName("value")
                Else
                    strExtPropertyValue &= strDelim & rowPropName("value")
                End If

                strDelim = "##"
            Next
        End If

        Return strExtPropertyValue

    End Function
    Public Function g_GetIndexes(ByVal tableName As String) As NameValueCollection
        Dim nvcIndexes As NameValueCollection = g_GetIndexes(tableName, False)
        Return nvcIndexes
    End Function
    Public Function g_GetIndexes(ByVal tableName As String, ByVal blnUnique As Boolean) As NameValueCollection
        Dim nvcIndexes As New NameValueCollection

        ' Determine if we are dealing with a table or view
        Dim blnIsView As Boolean = False
        Dim strTable As String = determineViewOrTable(tableName, blnIsView)
        If blnIsView Then
            ' get master table from ext propteries
            Dim strMasterTable As String = g_GetTableExtProperty(tableName, "MasterTable")
            If strMasterTable = "" Then
                ' get out w/ empty nvc b/c cannot pull indexes from a view
                Return nvcIndexes
            Else
                tableName = strMasterTable
            End If
        End If

        Dim strSQL As String = "SELECT  i.name as index_name, AC.name as field_name " & _
        "    FROM sys.tables AS T INNER JOIN  " & _
        "    sys.indexes AS I ON T.object_id = I.object_id INNER JOIN " & _
        "    sys.index_columns AS IC ON I.object_id = IC.object_id AND I.index_id = IC.index_id AND I.index_id = IC.index_id INNER JOIN  " & _
        "    sys.all_columns AS AC ON T.object_id = AC.object_id AND IC.column_id = AC.column_id  " & _
        "    WHERE (I.object_id =  " & _
        "        (SELECT     object_id     " & _
        "        FROM          sys.objects " & _
        "        WHERE (name = '" & tableName & "'))) AND (IC.is_included_column = 0)"
        If blnUnique Then
            strSQL &= " AND (I.is_unique = 1)"
        End If
        Dim tblIndexes As DataTable = g_IO_Execute_SQL(strSQL, False)
        Dim strKey As String = ""
        Dim strFields As String = ""
        Dim strDelim As String = ""
        For Each index In tblIndexes.Rows
            If strKey = "" Then
            Else
                If strKey = index("index_name") Then
                Else
                    nvcIndexes(strKey) = strFields
                    strFields = ""
                    strDelim = ""
                End If
            End If
            strFields &= strDelim & index("field_name")
            strDelim = "||"
            strKey = index("index_name")
        Next
        ' save last key/field(s)
        If strKey = "" Then
        Else
            nvcIndexes(strKey) = strFields
        End If
        Return nvcIndexes

    End Function

    Public Sub g_GetColumns(ByVal tableName As String, ByRef nvcColumnLength As NameValueCollection, _
                            ByRef nvcColumnType As NameValueCollection, _
                            ByRef nvcColumnDescription As NameValueCollection, _
                            ByRef nvcColumnDDLTableName As NameValueCollection, _
                            ByRef nvcColumnDDLValue As NameValueCollection, _
                            ByRef nvcColumnDDLText As NameValueCollection, _
                            ByRef nvcColumnPwd As NameValueCollection, _
                            ByRef nvcShowColumnInGrid As NameValueCollection, _
                            ByRef nvcHidden As NameValueCollection, _
                            ByRef nvcDisabled As NameValueCollection, _
                            ByRef nvcColumnIndex As NameValueCollection, _
                            ByRef nvcColumnRequired As NameValueCollection, _
                            ByRef nvcColumnEmail As NameValueCollection, _
                            ByRef nvcColumnPhone As NameValueCollection, _
                            ByRef nvcColumnTotal As NameValueCollection, _
                            ByRef nvcColumnDisplayLocn As NameValueCollection, _
                            ByRef nvcColumnUnique As NameValueCollection, _
                            ByRef nvcColumnNames As NameValueCollection, _
                            ByRef nvcRegExpPattern As NameValueCollection, _
                            ByRef nvcRegExpMessage As NameValueCollection, _
                            ByRef nvcMinValue As NameValueCollection, _
                            ByRef nvcMaxValue As NameValueCollection, _
                            ByRef nvcColumnDefaultValue As NameValueCollection, _
                            ByRef m_nvcaSign As NameValueCollection, _
                            ByRef m_nvcpSign As NameValueCollection, _
                            ByRef m_nvcPercentage As NameValueCollection, _
                            ByRef m_nvcShowSeconds As NameValueCollection)
        ' 01/06/15 CS/CB Look for list of fields that are the only ones to be enabled
        Dim strEnabledCols As String = g_GetTableExtProperty(tableName, "enabledList")

        Dim strTotalsCols As String = g_GetTableExtProperty(tableName, "totalsList")
        If strTotalsCols = "" Then
        Else
            For Each strColumn As String In Split(strTotalsCols, ",")
                nvcColumnTotal(strColumn) = "True"
            Next
        End If

        ' 01/05/2015 RLO - a view or table might have square brackets around it, if so must be removed
        tableName = tableName.Trim("[").Trim("]")
        ' 09/08/14 cs/cb - determine if ext property comes from view or table
        ' 01/06/15 T3 moved check for table/view to common function, used in multiple locations
        Dim strJoinTo As String = determineViewOrTable(tableName, False)
        Dim strSQL As String = "SELECT isnull(sep.name, '') [extName], isnull(sep.value, '') [extValue], sc.name [Column], sc.max_length [MaxLength], ic.DATA_TYPE, " & _
                               "CAST(ISNULL(ic.numeric_precision, '') AS char(5)) as numeric_precision, CAST(ISNULL(ic.numeric_scale, '') AS char(5)) as numeric_scale, column_default " & _
                               "FROM " & strJoinTo & " st " & _
                              "inner join sys.columns sc on st.object_id = sc.object_id " & _
                              "left join sys.extended_properties sep on st.object_id = sep.major_id and sc.column_id = sep.minor_id " & _
                              "left outer join INFORMATION_SCHEMA.COLUMNS as ic on ic.TABLE_NAME = st.name and ic.COLUMN_NAME = sc.name " & _
                              "Where st.name = '" & tableName & "'"


        Dim tblColumns = g_IO_Execute_SQL(strSQL, False)
        Dim intIndex As Integer = 0
        For Each rowColumns As DataRow In tblColumns.Rows
            If UCase(rowColumns("Column")) = "RECID" Then
            Else
                '6/30/15 T3 Get default value from table/column to use when adding a new record
                If IsNothing(nvcColumnDefaultValue(rowColumns("Column"))) Then
                    If IsDBNull(rowColumns("column_default")) Then
                    Else
                        nvcColumnDefaultValue(rowColumns("Column")) = CStr(rowColumns("column_default")).Trim("(").Trim(")").Trim("'")
                    End If
                End If

                ' 5/19/15 T3 Add regexp to handle decimal position validiation
                ' This regexp could be overwritten by ext prop on field (below)
                If IsNothing(nvcRegExpPattern(rowColumns("Column"))) Then
                    If UCase(rowColumns("NUMERIC_PRECISION")) > 0 AndAlso UCase(rowColumns("DATA_TYPE")) <> "TINYINT" Then
                        ' check for ext property for min/max values
                        Dim strMaxValue As String = Space(CInt(rowColumns("NUMERIC_PRECISION")) - CInt(rowColumns("NUMERIC_SCALE"))).Replace(" ", "9")
                        If rowColumns("NUMERIC_SCALE") > "0" Then
                            If UCase(rowColumns("DATA_TYPE")) = "MONEY" Then
                                strMaxValue &= ".99"
                            Else
                                strMaxValue &= "." & Space(rowColumns("NUMERIC_SCALE")).Replace(" ", "9")
                            End If
                        End If
                        If IsNothing(nvcMaxValue(rowColumns("Column"))) Then
                            nvcMaxValue(rowColumns("Column")) = strMaxValue
                        End If
                        If IsNothing(nvcMinValue(rowColumns("Column"))) Then
                            nvcMinValue(rowColumns("Column")) = "-" & strMaxValue
                        End If

                        '07/02/2015 T3
                        Dim intNewLength As Integer = CInt(rowColumns("NUMERIC_PRECISION"))
                        If rowColumns("NUMERIC_SCALE") > "0" Then
                            intNewLength += 1
                        End If
                        intNewLength += Int((CInt(rowColumns("NUMERIC_PRECISION")) - CInt(rowColumns("NUMERIC_SCALE"))) / 3)
                        rowColumns("MaxLength") = intNewLength

                        '07/09/2015 Remmed out because using autnumeric
                        ' set column length for numeric fields
                        'If rowColumns("NUMERIC_PRECISION") > 0 Then
                        '    ' Rodney() 's code (haven't tested in our app because the js we found handles commas automatically)
                        '    Dim intLeftOfDec As Integer = CInt(rowColumns("NUMERIC_PRECISION")) - CInt(rowColumns("NUMERIC_SCALE"))
                        '    Dim intRightOfDec As Integer = CInt(rowColumns("NUMERIC_SCALE"))
                        '    Dim strRegExpRight As String = ""
                        '    Dim strRegExpDelim As String = "\."
                        '    For i = 1 To intRightOfDec
                        '        strRegExpRight &= strRegExpDelim & "[0-9]"
                        '        strRegExpDelim = ""
                        '    Next

                        '    Dim strRegExpLeft As String = ""
                        '    Dim strRegExpLeftDelim As String = ""
                        '    Dim strREgExpLeftComma As String = ""
                        '    For i = 1 To intLeftOfDec
                        '        strRegExpLeft = ")" & strRegExpLeftDelim & strRegExpLeft
                        '        strREgExpLeftComma = ""
                        '        For j = 1 To i
                        '            strRegExpLeft = "[0-9]" & strREgExpLeftComma & strRegExpLeft
                        '            strREgExpLeftComma = ""
                        '            If j Mod 3 = 0 Then strREgExpLeftComma = ","
                        '        Next
                        '        strRegExpLeft = "(" & strRegExpLeft
                        '        strRegExpLeftDelim = "|"
                        '    Next
                        '    'nvcRegExpPattern(rowColumns("Column")) = "^(" & strRegExpLeft & ")" & strRegExpRight & "$"

                        'End If
                        'EOM 07/09/2015
                    End If
                End If

                ' 01/15/15 CP ext property not case sensitive 
                Select Case UCase(rowColumns("extName"))
                    Case "DDLTABLE"
                        nvcColumnDDLTableName(rowColumns("Column")) = rowColumns("extValue")
                    Case "DDLTEXT"
                        nvcColumnDDLText(rowColumns("Column")) = rowColumns("extValue")
                    Case "DDLVALUE"
                        nvcColumnDDLValue(rowColumns("Column")) = rowColumns("extValue")
                    Case "PWD"
                        nvcColumnPwd(rowColumns("Column")) = rowColumns("extValue")
                    Case "MS_DESCRIPTION"
                        nvcColumnDescription(rowColumns("Column")) = rowColumns("extValue")
                    Case "SHOWINGRID"
                        nvcShowColumnInGrid(rowColumns("Column")) = rowColumns("extValue")
                    Case "HIDDEN"
                        nvcHidden(rowColumns("Column")) = rowColumns("extValue")
                    Case "DISABLED"
                        nvcDisabled(rowColumns("Column")) = rowColumns("extValue")
                    Case "REQUIRED"
                        nvcColumnRequired(rowColumns("Column")) = rowColumns("extValue")
                    Case "EMAIL"
                        nvcColumnEmail(rowColumns("Column")) = rowColumns("extValue")
                    Case "PHONE"
                        nvcColumnPhone(rowColumns("Column")) = rowColumns("extValue")
                    Case "TOTAL"
                        nvcColumnTotal(rowColumns("Column")) = rowColumns("extValue")
                    Case "DISPLAYLOCATION"
                        ' 2/2/15 cpb
                        nvcColumnDisplayLocn(rowColumns("Column")) = rowColumns("extValue")
                    Case "UNIQUE"
                        ' 3/13/15 T3
                        nvcColumnUnique(rowColumns("Column")) = rowColumns("extValue")
                    Case "REGEXPPATTERN"
                        ' 5/19/15 T3
                        nvcRegExpPattern(rowColumns("Column")) = rowColumns("extValue")
                    Case "REGEXPMESSAGE"
                        ' 5/19/15 T3
                        nvcRegExpMessage(rowColumns("Column")) = rowColumns("extValue")
                    Case "MINVALUE"
                        nvcMinValue(rowColumns("Column")) = rowColumns("extValue")
                        If IsDBNull(rowColumns("column_default")) Then
                            nvcColumnDefaultValue(rowColumns("Column")) = rowColumns("extValue")
                        Else
                            ' test for default within min/max range
                            ' we have max at this point in loop
                            If rowColumns("column_default") < rowColumns("extValue") Or rowColumns("column_default") > nvcMaxValue(rowColumns("Column")) Then
                                nvcColumnDefaultValue(rowColumns("Column")) = rowColumns("extValue")
                            End If
                        End If
                    Case "MAXVALUE"
                        nvcMaxValue(rowColumns("Column")) = rowColumns("extValue")
                    Case "ASIGN"
                        m_nvcaSign(rowColumns("Column")) = rowColumns("extValue")
                    Case "PSIGN"
                        m_nvcpSign(rowColumns("Column")) = rowColumns("extValue")
                    Case "PERCENTAGE"
                        m_nvcPercentage(rowColumns("Column")) = rowColumns("extValue")
                    Case "SHOWSECONDS"
                        m_nvcShowSeconds(rowColumns("Column")) = rowColumns("extValue")
                End Select

                If IsNothing(nvcShowColumnInGrid(rowColumns("Column"))) Then
                    nvcShowColumnInGrid(rowColumns("Column")) = "TRUE"
                End If
                If UCase(nvcShowColumnInGrid(rowColumns("Column"))) = "TRUE" Then
                    '04/30/15 Add column to nvcColumnName
                    nvcColumnNames(rowColumns("Column")) = rowColumns("Column")
                End If

                If IsNothing(nvcHidden(rowColumns("Column"))) Then
                    nvcHidden(rowColumns("Column")) = "FALSE"
                End If

                If IsNothing(nvcDisabled(rowColumns("Column"))) Then
                    nvcDisabled(rowColumns("Column")) = "FALSE"
                End If

                ' 01/06/15 T3 loop any fields identified in enabledLIst ext prop
                If strEnabledCols = "" Then
                Else
                    Dim blnEnabledFound As Boolean = False
                    If ("," & strEnabledCols.Replace(" ", "") & ",").Contains("," & rowColumns("Column") & ",") Then
                        blnEnabledFound = True
                    End If

                    If blnEnabledFound Then
                        nvcDisabled(rowColumns("Column")) = "FALSE"
                    Else
                        nvcDisabled(rowColumns("Column")) = "TRUE"
                    End If
                End If

                'these are standard SQL Server column properties
                If IsNothing(nvcColumnLength(rowColumns("Column"))) Then
                    nvcColumnLength(rowColumns("Column")) = rowColumns("MaxLength")
                End If

                nvcColumnType(rowColumns("Column")) = rowColumns("Data_type")
                If IsNothing(nvcColumnDescription(rowColumns("Column"))) Then
                    nvcColumnDescription(rowColumns("Column")) = rowColumns("Column").Replace("_", " ")
                End If
            End If
            nvcColumnIndex(rowColumns("column")) = intIndex
            intIndex += 1
        Next
    End Sub
    Public Sub g_auditLog(ByVal strSQL As String)
        If g_auditUpdates Then


            '''''Audit Updates Log''''' 
            'Updated 08/06/15 T3 From CONTAINS to StartsWith
            If strSQL.ToUpper.StartsWith("UPDATE") Or strSQL.ToUpper.StartsWith("DELETE") Or strSQL.ToUpper.StartsWith("INSERT") Then
                If strSQL.ToUpper.Contains("AUDITUPDATES_LOG") Or strSQL.ToUpper.Contains("ERROR_LOG") Then
                    'Prevent a duplicate from being intered into the system
                Else
                    Dim userRecid As Integer = -1
                    If IsNothing(System.Web.HttpContext.Current.Session("user_link_id")) Then
                    Else
                        userRecid = System.Web.HttpContext.Current.Session("user_link_id")
                    End If
                    If g_auditUpdatesTblExists Then
                    Else
                        Dim tblTableCheck As DataTable = g_IO_Execute_SQL("select count(*) as tblCount from sys.tables where name = 'auditUpdates_log'", False)
                        If tblTableCheck.Rows(0)("tblCount") = 0 Then
                            'we need to create autUdates_log table
                            Dim strCreateTbl As String = "CREATE TABLE [dbo].[auditUpdates_log](" & _
                          "      [recid] [int] IDENTITY(1,1) NOT NULL," & _
                          "      [sqlStatement] [varchar](8000) NULL," & _
                          "      [timestamp] [datetime] NULL," & _
                          "      [user_recid] [int] NULL," & _
                          "      CONSTRAINT [PK_auditUpdates_log] PRIMARY KEY CLUSTERED " & _
                          "  ( " & _
                          "  [recid] Asc" & _
                          "  )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]" & _
                          "  ) ON [PRIMARY]"
                            IO_Execute_MSSQL(strCreateTbl, False)
                        End If
                        g_auditUpdatesTblExists = True
                    End If

                    IO_Execute_MSSQL("Insert into auditUpdates_log (sqlStatement,  timestamp, user_recid) " & _
                                     " VALUES ('" & Left(strSQL.Replace("'", "''"), 8000) & "', '" & DateTime.Now & "'," & userRecid & ")", False)
                End If

            End If
            '''''END Audit Updates Log'''''
        End If
    End Sub
    Public Sub g_logError(ByVal form As String, ByVal routine As String, ByVal notes As String, ByVal isError As Boolean)
        g_logError(notes, "")
        'g_logError(form, routine, notes, isError, "")
        'This routine is used mostly by the modDataSync, but it can be used for anything
    End Sub
    Public Sub g_logError(ByVal strSQL As String, ByVal strErrorMessage As String)
        If g_errorLog Then
            Dim userRecid As Integer = -1
            If IsNothing(System.Web.HttpContext.Current.Session("user_link_id")) Then
            Else
                userRecid = System.Web.HttpContext.Current.Session("user_link_id")
            End If
            If g_ErrorTblExists Then
            Else
                'Create error log table
                Dim tblTableCheck As DataTable = g_IO_Execute_SQL("select count(*) as tblCount from sys.tables where name = 'error_log'", False)
                If tblTableCheck.Rows(0)("tblCount") = 0 Then
                    'we need to create autUdates_log table
                    Dim strCreateTbl As String = "CREATE TABLE [dbo].[error_log](" & _
                  "      [recid] [int] IDENTITY(1,1) NOT NULL," & _
                  "      [sqlStatement] [varchar](8000) NULL," & _
                  "      [errorMessage] [varchar](8000) NULL," & _
                  "      [timestamp] [datetime] NULL," & _
                  "      [user_recid] [int] NULL," & _
                  "      CONSTRAINT [PK_error_log] PRIMARY KEY CLUSTERED " & _
                  "  ( " & _
                  "  [recid] Asc" & _
                  "  )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]" & _
                  "  ) ON [PRIMARY]"
                    IO_Execute_MSSQL(strCreateTbl, False)

                 
                End If
                g_ErrorTblExists = True
            End If
            IO_Execute_MSSQL("Insert into error_log (sqlStatement, errorMessage, timestamp, user_recid) " & _
                             " VALUES ('" & Left(strSQL.Replace("'", "''"), 8000) & "', '" & Left(strErrorMessage.Replace("'", "''"), 8000) & "', '" & DateTime.Now & "'," & userRecid & ")", False)

        End If
    End Sub
    '**********************************SQL Insert**********************************
    ' data inserts via nvc - 5 overloads
    Public Sub g_IO_SQLInsert(ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection)
        Call IO_Execute_SQLInsert(TableName, nvcFieldValues, "", False, True, False)
    End Sub
    Public Sub g_IO_SQLInsert(ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection, _
                              ByVal FormName As String)
        Call IO_Execute_SQLInsert(TableName, nvcFieldValues, FormName, False, True, False)
    End Sub
    Public Sub g_IO_SQLInsert(ByRef blnRecidProvided As Boolean, ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection, _
                            ByVal FormName As String, ByRef blnReturnError As Boolean)
        Call IO_Execute_SQLInsert(TableName, nvcFieldValues, FormName, blnReturnError, True, blnRecidProvided)
    End Sub
    Public Sub g_IO_SQLInsert(ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection, _
                              ByVal FormName As String, ByRef blnReturnError As Boolean)
        Call IO_Execute_SQLInsert(TableName, nvcFieldValues, FormName, blnReturnError, True, False)
    End Sub
    Public Sub g_IO_SQLInsert(ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection, _
                              ByVal FormName As String, ByRef blnReturnError As Boolean, ByRef blnAudit As Boolean)
        Call IO_Execute_SQLInsert(TableName, nvcFieldValues, FormName, blnReturnError, blnAudit, False)
    End Sub
    Public Sub g_IO_SQLInsert(ByRef TableName As String, ByRef TableDataRow As DataRow, ByVal FormName As String, _
                             ByRef blnReturnError As Boolean, ByRef blnAudit As Boolean, ByRef blnRecidProvided As Boolean)
        Dim nvcnvcFieldValues As New NameValueCollection

        ' extract all the columns in this table from the database (just read one record and extract the info from the datatable)
        Dim tblDataTable As DataTable

        If g_ConnectionToUse = "MYSQL" Then
            tblDataTable = g_IO_Execute_SQL("Select * from " & TableName & " Limit 1", False)
        Else
            tblDataTable = g_IO_Execute_SQL("select TOP 1 * from " & TableName, False)
        End If

        For Each colTableColumn As DataColumn In tblDataTable.Columns
            ' loop on column list retrieved from database
            Dim strColumnName As String = colTableColumn.ColumnName
            Try
                nvcnvcFieldValues(strColumnName) = TableDataRow(strColumnName)
            Catch ex As Exception
            End Try
        Next

        If nvcnvcFieldValues.Count > 0 Then
            Call IO_Execute_SQLInsert(TableName, nvcnvcFieldValues, FormName, blnReturnError, blnAudit, blnRecidProvided)
        End If

    End Sub
    '
    '**********************************SQL Delete**********************************
    Public Sub g_IO_SQLDelete(ByRef TableName As String, ByVal WherePhrase As String)
        Call IO_Execute_SQLDelete(TableName, WherePhrase, Nothing, False, True)
    End Sub

    Public Sub g_IO_SQLDelete(ByRef TableName As String, ByVal WherePhrase As String, _
                             ByVal FormName As String, ByRef blnReturnError As Boolean)
        Call IO_Execute_SQLDelete(TableName, WherePhrase, FormName, blnReturnError, True)
    End Sub

    Public Sub g_IO_SQLDelete(ByRef TableName As String, ByVal WherePhrase As String, _
                              ByVal FormName As String, ByRef blnReturnError As Boolean, ByRef blnAudit As Boolean)
        IO_Execute_SQLDelete(TableName, WherePhrase, FormName, blnReturnError, blnAudit)
    End Sub
    '
    '**********************************SQL Update**********************************
    ' 11/11/14 - cpb
    Public Sub g_IO_SQLUpdate(ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection, _
                          ByVal FormName As String, ByRef WherePhrase As String)
        Call IO_Execute_SQLUpdate(TableName, nvcFieldValues, WherePhrase, FormName, False, True)
    End Sub

    Public Sub g_IO_SQLUpdate(ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection, _
                                ByVal FormName As String, ByRef WherePhrase As String, _
                                ByRef blnReturnError As Boolean)
        Call IO_Execute_SQLUpdate(TableName, nvcFieldValues, WherePhrase, FormName, blnReturnError, True)
    End Sub

    Public Sub g_IO_SQLUpdate(ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection, _
                              ByVal FormName As String, ByRef WherePhrase As String, _
                              ByRef blnReturnError As Boolean, ByRef blnAudit As Boolean)
        Call IO_Execute_SQLUpdate(TableName, nvcFieldValues, WherePhrase, FormName, blnReturnError, blnAudit)
    End Sub

    Private Function IO_Execute_SQLInsert(ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection, _
                                      ByVal FormName As String, ByRef ReturnACompletionCode As Boolean, _
                                      ByVal AuditInsert As Boolean, ByRef blnRecidProvided As Boolean) As Boolean
        ' this routine will receive from the programmer a list of columns in a table with associated values 
        ' to be inserted into a table (see parameter list above)

        Dim blnRetry As Boolean = True

        ' define audit variables to be built during insert
        Dim nvcAuditKeyValues As New NameValueCollection

        Dim m_nvcConvertedInserts As New NameValueCollection '  take Column collection array from programmer and make all keys capital

        ' make all keys upper case
        For Each strcolumn As String In nvcFieldValues.AllKeys
            If IsDBNull(nvcFieldValues(strcolumn)) Or IsNothing(nvcFieldValues(strcolumn)) Then
                nvcFieldValues.Remove(strcolumn)
            Else
                m_nvcConvertedInserts(UCase(strcolumn)) = nvcFieldValues(strcolumn)
            End If
        Next

        ' start to build INSERT SQL command
        Dim strInsertSQL As String = ""
        Dim strDelimiter As String = " "

        Do Until Not blnRetry

            blnRetry = False

            If blnRecidProvided Then
                strInsertSQL &= " set identity_insert " & TableName & " on; "
            End If

            strInsertSQL = "Insert into " & TableName & " "

            Dim strFields As String = " ("              ' list of columns sent by programmer
            Dim strValues As String = " VALUES ("       ' list of associated values sent by programmer

            ' column info from memory variables
            Dim nvcFields As NameValueCollection
            nvcFields = getTableFieldList(TableName)

            ' loop through column names extract from database and build INSERT (also check to see if programmer provided bogus columns)
            ' any columns extracted not listed by programmer will be defaulted to an appropriate value
            Dim strFieldsDelimiter As String = ""
            Dim strValuesDelimiter As String = ""
            Dim strValue As String = ""

            For Each strColumnName As String In nvcFields.Keys
                Dim strColumnType As String = getFieldAttributeValue(TableName, strColumnName, "TYPE")
                Dim strColumnKey As String = getFieldAttributeValue(TableName, strColumnName, "INDEX")
                ' determine if field was supplied by programmer
                If m_nvcConvertedInserts(strColumnName) Is Nothing Then

                    ' was not in list of fields supplied by programmer - get default value
                    Dim strColumnDefault As String = getFieldAttributeValue(TableName, strColumnName, "DEFAULT")
                    Dim strColumnNull As String = getFieldAttributeValue(TableName, strColumnName, "NULL")
                    Dim strColumnAutoInc As String = getFieldAttributeValue(TableName, strColumnName, "AUTOINC")
                    If strColumnDefault <> "" Or strColumnNull = "1" Or strColumnAutoInc = "1" Then
                    Else
                        ' column in data file not supplied by programer insert data
                        ' does not have default and can not be null -- fill it for programmer
                        If g_Debug Then
                            MsgBox("Programmer You Did Not supply a field that can not be left null and does not have default value.", MsgBoxStyle.Exclamation, "Programmer-NOTICE")
                        Else
                            Select Case UCase(strColumnType)
                                Case "STR"
                                    ' this is a string (I hope)
                                    strValues &= strValuesDelimiter & "''"
                                    strValue = ""
                                Case "DAT"
                                    ' this is a date
                                    strValues &= strValuesDelimiter & "'1970-01-01'"
                                    strValue = "1970-01-01"
                                Case "TIM"
                                    ' this is a date
                                    strValues &= strValuesDelimiter & "'00:00:00'"
                                    strValue = "00:00:00"
                                Case "BOO"
                                    ' this is a boolean
                                    strValues &= strValuesDelimiter & False
                                    strValue = "0"
                                Case "NUM"
                                    ' this is a number so don't include quotes
                                    strValues &= strValuesDelimiter & 0
                                    strValue = "0"
                                Case Else
                                    ' this is a string (I hope)
                                    strValues &= strValuesDelimiter & "''"
                                    strValue = ""
                            End Select
                            nvcFieldValues(strColumnName) = strValue
                            If strColumnKey = "1" Then
                                nvcAuditKeyValues(strColumnName) = strValue
                            End If
                        End If
                        ' strFields &= strFieldsDelimiter & strColumnName
                        'strFieldsDelimiter = ", "
                        'strValuesDelimiter = ", "
                    End If
                Else
                    Try
                        ' create column list and values list to be assembled later into Insert SQL statement
                        '       if column being extracted from table is not in programmer's list then will error to catch
                        strValue = m_nvcConvertedInserts(strColumnName)
                        Select Case strColumnType
                            Case "STR"
                                ' STRING TYPE
                                strValues &= strValuesDelimiter & "'" & strValue.Replace("'", "''") & "'"
                            Case "DAT"
                                If IsDate(strValue) Then
                                    ' this is a date
                                    Dim dteDate As DateTime = strValue
                                    If strValue.IndexOf(":") > -1 Then
                                        strValue = Format(dteDate, "yyyy-MM-dd HH:mm:ss")
                                        strValues &= strValuesDelimiter & "'" & strValue & "'"
                                    Else
                                        strValue = Format(dteDate, "yyyy-MM-dd")
                                        strValues &= strValuesDelimiter & "'" & strValue & "'"
                                    End If
                                Else
                                    strValue = "null"
                                    strValues &= strValuesDelimiter & strValue
                                End If
                                nvcFieldValues(strColumnName) = strValue   ' convert date format for future use
                            Case "TIM"
                                ' this is a date
                                strValues &= strValuesDelimiter & "'" & strValue & "'"
                            Case "BOO"
                                ' this is a boolean
                                strValues &= strValuesDelimiter & strValue
                            Case "NUM"
                                If IsNumeric(strValue) Then
                                    strValues &= strValuesDelimiter & strValue.Replace(",", "")
                                Else
                                    strValues &= strValuesDelimiter & "0"
                                End If
                            Case Else
                                ' Don't know what this is but assume it needs quotes
                                strValues &= strValuesDelimiter & "'" & strValue.Replace("'", "''") & "'"
                        End Select
                        strValuesDelimiter = ", "
                        strFields &= strFieldsDelimiter & strColumnName
                        strFieldsDelimiter = ", "
                        If strColumnKey = "1" Then
                            nvcAuditKeyValues(strColumnName) = strValue
                        End If

                        ' remove from user supplied list to indicate that this entry is handled
                        m_nvcConvertedInserts.Remove(UCase(strColumnName))  ' any columns left in this array at end of process will cause an error

                    Catch ex As Exception
                        MsgBox("Dear Programmer -- You have data problems -- Invalid Data Sent", MsgBoxStyle.Critical, "OOPS!")
                    End Try

                    'strFields &= strFieldsDelimiter & strColumnName
                    'strFieldsDelimiter = ", "
                    'strValuesDelimiter = ", "

                    ' remove from user supplied list to indicate that this entry is handled (since it is empty just let FoxPro initialize it)
                    m_nvcConvertedInserts.Remove(UCase(strColumnName))  ' any columns left in this array at end of process will cause an error
                End If

            Next
            strInsertSQL &= strFields & ")" & strValues & ")"

            ' were there any programmer supplied columns not used?
            If m_nvcConvertedInserts.Count = 0 Then
                'array is empty so all columns used
                blnRetry = False
            Else
                Dim strUnusedColumnsProvided As String = ""
                strDelimiter = ""
                For Each strcolumn As String In m_nvcConvertedInserts.AllKeys
                    m_nvcConvertedInserts(UCase(strcolumn)) = nvcFieldValues(strcolumn)
                    strUnusedColumnsProvided &= strDelimiter & strcolumn
                    strDelimiter = ", "
                Next
                MsgBox("The following fields were provided but don't seem to exist in " & TableName & " (Could be database communication error): " & Chr(13) & Chr(10) & Chr(13) & Chr(10) & strUnusedColumnsProvided, MsgBoxStyle.RetryCancel, "Unused Input")
            End If
        Loop

        ' write out to SQL 
        Dim LastRecId As Integer = Nothing

        Try

            If g_ConnectionToUse = "MYSQL" Then
                g_IO_Execute_SQL(strInsertSQL, ReturnACompletionCode)
            Else
                ' MSSQL must retrieve the new RECID now, 12/23/11 cpb
                strInsertSQL &= ";Select @@IDENTITY as ID;"
                Dim tblTemp As DataTable = g_IO_Execute_SQL(strInsertSQL, ReturnACompletionCode)
                LastRecId = tblTemp.Rows(0)("ID")
            End If

            'g_IO_Execute_SQL(strInsertSQL, ReturnACompletionCode)
            ' got to get command into data table to get the recid out back out


        Catch ex As Exception
            MsgBox("Error attempting to write: " & TableName & ".", MsgBoxStyle.Information, "Unable To Write To Table")
        End Try

        ' EVERY TABLE MUST HAVE RECID
        ' get record back to write the recid for audit data
        Dim strSelectSQL As String = ""
        If g_ConnectionToUse = "MYSQL" Then
            strSelectSQL = "Select Last_Insert_Id() as RecId from " & TableName
            LastRecId = g_IO_Execute_SQL(strSelectSQL, False).Rows(0)("RecId")
        Else
            ' 12/23/11 cpb handled above
            ''''strSelectSQL = "SELECT IDENT_CURRENT('" & TableName & "') as RecId"
            '''''  strSelectSQL = "SELECT SCOPE_IDENTITY() as RecId" --- '''THIS RETURNS NULL...BUT THE ABOVE IS NOT LIMITED TO OWN SESSION???NOT SURE ABOUT THIS???
        End If

        'Dim LastRecId As Integer = g_IO_Execute_SQL(strSelectSQL).Rows(0)("RecId")
        nvcAuditKeyValues("RecId") = LastRecId

        ' audit the insert
        'If AuditInsert And ReturnACompletionCode Then
        '    Audit_Data(TableName, FormName, nvcFieldValues, nvcAuditKeyValues, "", "I")
        'End If
        Return ReturnACompletionCode

    End Function
    '_____ sql delete functions 
    Private Function IO_Execute_SQLDelete(ByRef TableName As String, ByVal WherePhrase As String, _
                                         ByVal FormName As String, ByRef ReturnACompletionCode As Boolean, _
                                         ByVal AuditDelete As Boolean) As Boolean

        If AuditDelete Then
            ' get column info from memory variables to build list of keys for audit write
            Dim nvcFields As NameValueCollection
            nvcFields = getTableFieldList(TableName)

            ' build table of records to be deleted
            Dim tblLiveDatabaseRecordsBeingDeleted As DataTable = Nothing
            Dim strSQLToReadRecordsBeingDeleted As String = "SELECT * FROM " & TableName & _
                            IIf(Trim(WherePhrase) = "", "", " WHERE " & WherePhrase)
            tblLiveDatabaseRecordsBeingDeleted = g_IO_Execute_SQL(strSQLToReadRecordsBeingDeleted, False)

            ' write out audit entry for each data row being deleted
            For Each rowLiveDatabaseRecordsBeingDeleted As DataRow In tblLiveDatabaseRecordsBeingDeleted.Rows

                ' get key and data values for deleted keys
                Dim nvcAuditKeyValues As New NameValueCollection
                Dim nvcAuditFieldValues As New NameValueCollection
                For Each field In nvcFields.Keys
                    If IsDBNull(rowLiveDatabaseRecordsBeingDeleted(field)) Then

                    Else

                        nvcAuditFieldValues(field) = rowLiveDatabaseRecordsBeingDeleted(field)
                        Dim strColumnKey As String = getFieldAttributeValue(TableName, field, "INDEX")
                        If strColumnKey = "1" Then
                            nvcAuditKeyValues(field) = rowLiveDatabaseRecordsBeingDeleted(field)
                        End If
                    End If
                Next

                'Audit_Data(TableName, FormName, nvcAuditFieldValues, nvcAuditKeyValues, _
                '           IIf(Trim(WherePhrase) = "", "", WherePhrase), "D")
            Next
        End If

        ' perform delete
        Dim strSQLToDeleteRecords As String = "Delete FROM " & TableName & _
                        IIf(Trim(WherePhrase) = "", "", " WHERE " & WherePhrase)
        g_IO_Execute_SQL(strSQLToDeleteRecords, ReturnACompletionCode)


        Return ReturnACompletionCode

    End Function

    '
    '_____ sql update functions 
    ' 11/11/14 - cpb
    Private Sub IO_Execute_SQLUpdate(ByRef TableName As String, ByRef nvcFieldValues As NameValueCollection, _
                                         ByVal WherePhrase As String, ByVal FormName As String, _
                                         ByRef ReturnACompletionCode As Boolean, _
                                         ByVal blnAudit As Boolean)

        ' this routine will receive a list of columns in a table with associated values and insert a new fully initialized record
        If nvcFieldValues.Count = 0 Then
            ' no field updateds provided
        Else
            Dim strUpdateSQL As String = TableName & " set "
            Dim strDelimiter As String = " "

            Dim tblLiveDatabaseRecordBeforeUpdate As DataTable = Nothing

            ' this routine just prepares for auditing the change by reading the record before changing
            ' get record before update for audit purposes
            ' build Select sql statement to get post update data
            Dim strSelectSQL As String = "Select"

            ' column info from memory variables
            Dim nvcKeyFields As New NameValueCollection
            Dim nvcFields As NameValueCollection
            nvcFields = getTableFieldList(TableName)

            ' first add all key fields and fields sent by programmer to the select 
            For Each strColumnName As String In nvcFields.Keys
                Dim strColumnKey As String = getFieldAttributeValue(TableName, strColumnName, "INDEX")
                If strColumnKey = "1" Then
                    If nvcFieldValues(strColumnName) Is Nothing Then
                    Else
                        strSelectSQL += strDelimiter + Trim(UCase(strColumnName)) & " "
                        strDelimiter = ", "
                        nvcKeyFields(strColumnName) = "*NOKEYVALUE*"
                    End If

                Else
                    If nvcFieldValues(strColumnName) Is Nothing Then
                    Else
                        strSelectSQL += strDelimiter + Trim(UCase(strColumnName)) & " "
                        strDelimiter = ", "
                    End If
                End If

            Next

            strSelectSQL &= " from " & TableName & IIf(Trim(WherePhrase) = "", "", " WHERE " & WherePhrase)
            tblLiveDatabaseRecordBeforeUpdate = g_IO_Execute_SQL(strSelectSQL, False)

            ' define audit variables to be built during insert
            Dim nvcAuditKeyValues As New NameValueCollection
            Dim nvcAuditFieldValues As New NameValueCollection
            Dim blnBuildUpdateSQL As Boolean = True
            strUpdateSQL = "Update " & TableName & " " & " set "
            strDelimiter = ""

            ' put users changes in the record before calling audit routines below
            For Each rowLiveDatabaseRecordBeforeUpdate As DataRow In tblLiveDatabaseRecordBeforeUpdate.Rows

                nvcAuditFieldValues.Clear()
                nvcAuditKeyValues.Clear()

                For Each colColumnName As DataColumn In tblLiveDatabaseRecordBeforeUpdate.Columns
                    Dim strColumnName As String = colColumnName.ColumnName
                    ' save all keys before changes
                    If nvcKeyFields(strColumnName) Is Nothing Then
                    Else
                        nvcAuditKeyValues(strColumnName) = rowLiveDatabaseRecordBeforeUpdate(strColumnName)
                    End If
                    If (Not IsDBNull(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) AndAlso _
                        Trim(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) = Trim(nvcFieldValues(strColumnName))) Or _
                        nvcFieldValues(strColumnName) Is Nothing Then
                    Else
                        nvcAuditFieldValues(strColumnName) = nvcFieldValues(strColumnName)
                        Dim strColumnType As String = getFieldAttributeValue(TableName, strColumnName, "TYPE")
                        Dim blnSetDelim As Boolean = True
                        If blnBuildUpdateSQL Then
                            Select Case strColumnType
                                Case "STR"
                                    strUpdateSQL &= strDelimiter & strColumnName & "='" & nvcFieldValues(strColumnName) & "'"
                                Case "DAT"
                                    If IsDate(nvcFieldValues(strColumnName)) Then
                                        Dim dteDate As Date = nvcFieldValues(strColumnName)
                                        If nvcFieldValues(strColumnName).IndexOf(":") > -1 Then
                                            strUpdateSQL &= strDelimiter & strColumnName & "='" & Format(dteDate, "yyyy-MM-dd HH:mm:ss") & "'"
                                        Else
                                            strUpdateSQL &= strDelimiter & strColumnName & "='" & Format(dteDate, "yyyy-MM-dd") & "'"
                                        End If
                                    Else
                                        strUpdateSQL &= strDelimiter & strColumnName & "=null"
                                    End If
                                Case "TIM"
                                    strUpdateSQL &= strDelimiter & strColumnName & "='" & nvcFieldValues(strColumnName) & "'"
                                Case "BOO"
                                    Try
                                        If Trim(nvcFieldValues(strColumnName)) = "True" AndAlso Not IsDBNull(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) AndAlso Trim(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) = 1 Or _
                                            Trim(nvcFieldValues(strColumnName)) = "False" AndAlso Not IsDBNull(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) AndAlso Trim(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) = 0 Then
                                            blnSetDelim = False
                                        Else

                                            If nvcFieldValues(strColumnName) Then
                                                strUpdateSQL &= strDelimiter & strColumnName & "= 1"
                                            Else
                                                strUpdateSQL &= strDelimiter & strColumnName & "= 0"
                                            End If
                                        End If
                                    Catch ex As Exception
                                        If Trim(nvcFieldValues(strColumnName)) = "True" AndAlso Not IsDBNull(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) AndAlso Trim(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) = True Or _
                                            Trim(nvcFieldValues(strColumnName)) = "False" AndAlso Not IsDBNull(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) AndAlso Trim(rowLiveDatabaseRecordBeforeUpdate(strColumnName)) = False Then
                                            blnSetDelim = False
                                        Else

                                            If nvcFieldValues(strColumnName) Then
                                                strUpdateSQL &= strDelimiter & strColumnName & "= 1"
                                            Else
                                                strUpdateSQL &= strDelimiter & strColumnName & "= 0"
                                            End If
                                        End If
                                    End Try
                                Case "NUM"
                                    strUpdateSQL &= strDelimiter & strColumnName & " = " & CType(nvcFieldValues(strColumnName), Single)
                                Case Else
                                    ' Don't know what this is but assume string
                                    strUpdateSQL &= strDelimiter & strColumnName & "='" & nvcFieldValues(strColumnName) & "'"
                            End Select
                            If blnSetDelim Then
                                strDelimiter = ","
                            End If
                            If strColumnType = "DAT" Then
                                If IsDate(nvcFieldValues(strColumnName)) Then
                                    Dim dteDate As Date = nvcFieldValues(strColumnName)
                                    If nvcFieldValues(strColumnName).IndexOf(":") > -1 Then
                                        nvcFieldValues(strColumnName) = Format(dteDate, "yyyy-MM-dd HH:mm:ss")
                                    Else
                                        nvcFieldValues(strColumnName) = Format(dteDate, "yyyy-MM-dd")
                                    End If
                                Else
                                    nvcFieldValues(strColumnName) = "null"
                                End If

                            End If
                        End If
                    End If
                Next
                '11/11/14 remmed out - other brought in routines seemd to be remmed out
                'If blnAudit Then
                '    Call Audit_Data(TableName, FormName, nvcAuditFieldValues, nvcAuditKeyValues, WherePhrase, "U")
                'End If
            Next

            ' 11/11/14 auto update modified date and modified user 
            strUpdateSQL &= strDelimiter & "last_modified_date ='" & Date.Now & "'" & "," &
                "last_modified_recid = " & g_UserRecId

            If strUpdateSQL = "Update " & TableName & " " & " set " Then
            Else
                strUpdateSQL &= IIf(Trim(WherePhrase) = "", "", " WHERE " & WherePhrase)
                Call g_IO_Execute_SQL(strUpdateSQL, False)
            End If

        End If
    End Sub
    Private Function getFieldAttributeValue(ByVal TableName As String, ByVal FieldName As String, _
                                    ByVal AttributeName As String) As String

        Call EvaluateTableEntries(TableName)

        If m_nvcFields(m_nvcTables(TableName))(FieldName) Is Nothing Then
            ' MsgBox("Invalid field name in request", MsgBoxStyle.Information, "Table Field Attribute Evaluation")
            Return "UKN"
        Else
            If m_nvcFieldAttributes(m_nvcFields(m_nvcTables(TableName))(FieldName))(AttributeName) Is Nothing Then
                MsgBox("Invalid attribuate name in request", MsgBoxStyle.Information, "Table Field Attribute Evaluation")
                Return Nothing
            Else
                Return m_nvcFieldAttributes(m_nvcFields(m_nvcTables(TableName))(FieldName))(AttributeName)
            End If
        End If
    End Function
    Private Sub EvaluateTableEntries(ByVal TableName As String)

        If m_nvcTables(TableName) Is Nothing Then

            Dim tblColumns = g_getTableColumnInfo(TableName)  ' MySQL or MSSQL

            Dim intFieldsIndex As Integer = 0
            Dim intFieldAttributesIndex As Integer = 0

            Try : intFieldsIndex = m_nvcFields.Count : Catch : End Try
            ReDim Preserve m_nvcFields(intFieldsIndex)
            m_nvcFields(intFieldsIndex) = New NameValueCollection
            m_nvcTables(TableName) = intFieldsIndex
            For Each rowColumn In tblColumns.Rows
                Try : intFieldAttributesIndex = m_nvcFieldAttributes.Count : Catch : End Try
                m_nvcFields(intFieldsIndex)(UCase(rowColumn("FieldName"))) = intFieldAttributesIndex
                ReDim Preserve m_nvcFieldAttributes(intFieldAttributesIndex)
                m_nvcFieldAttributes(intFieldAttributesIndex) = New NameValueCollection
                m_nvcFieldAttributes(intFieldAttributesIndex)("TYPE") = rowColumn("TYPE")
                m_nvcFieldAttributes(intFieldAttributesIndex)("NULL") = rowColumn("AllowNull")
                m_nvcFieldAttributes(intFieldAttributesIndex)("DEFAULT") = rowColumn("Default")
                m_nvcFieldAttributes(intFieldAttributesIndex)("INDEX") = IIf(IsDBNull(rowColumn("Index")), 0, rowColumn("Index"))
                m_nvcFieldAttributes(intFieldAttributesIndex)("AUTOINC") = rowColumn("AutoInc")
            Next

        End If
    End Sub
    '**********************************Maintain Tables Referenced from DataBase**********************************
    ' 
    ' returns a list of fields for the requested table as a name value collection
    Private Function getTableFieldList(ByVal TableName As String) As NameValueCollection
        Call EvaluateTableEntries(TableName)
        If m_nvcFields(m_nvcTables(TableName)) Is Nothing Then
            MsgBox("Invalid table name in request", MsgBoxStyle.Information, "Table List Evaluation")
        End If
        Return m_nvcFields(m_nvcTables(TableName))
    End Function
    Public Function g_getTableColumnInfo(ByRef TableName As String) As DataTable
        Dim tblColumns As New DataTable
        tblColumns.Columns.Add("FieldName", GetType(System.String))
        tblColumns.Columns.Add("Type", GetType(System.String))
        tblColumns.Columns.Add("AllowNull", GetType(System.String))
        tblColumns.Columns.Add("Default", GetType(System.String))
        tblColumns.Columns.Add("Index", GetType(System.String))
        tblColumns.Columns.Add("AutoInc", GetType(System.String))
        Dim rowColumns As DataRow = Nothing

        If g_ConnectionToUse = "MYSQL" Then
            'Dim tblColumnInfo As DataTable = IO_Execute_MYSQL("Show Columns from " & TableName, False)
            'For Each rowFieldInfoExtracted In tblColumnInfo.Rows
            '    rowColumns = tblColumns.NewRow
            '    rowColumns("FieldName") = rowFieldInfoExtracted("Field")
            '    rowColumns("Type") = getSQLDataType(rowFieldInfoExtracted("Type"))
            '    rowColumns("AllowNull") = IIf(rowFieldInfoExtracted("Null") = "YES", "1", "0")
            '    rowColumns("Default") = IIf(IsDBNull(rowFieldInfoExtracted("Default")), "", rowFieldInfoExtracted("Default"))
            '    rowColumns("Index") = IIf(rowFieldInfoExtracted("Key") = "", "0", "1")
            '    rowColumns("AutoInc") = IIf(rowFieldInfoExtracted("Extra") = "auto_increment", "1", "0")
            '    tblColumns.Rows.Add(rowColumns)
            'Next
        Else
            'MSSQL
            Dim tblColumnInfo As DataTable = IO_Execute_MSSQL("Select Column_Name, Data_Type, Is_Nullable, " & _
                                                  "Column_Default from Information_Schema.columns where " & _
                                                  "Table_Catalog = '" & g_ConnectionSchema & "' and Table_Name ='" & TableName & "'", _
                                                  False)
            If g_blnAbort Then
                Return Nothing
            End If
            Dim tblColumnKeyInfo As DataTable = IO_Execute_MSSQL("Select Column_Name from Information_Schema.KEY_COLUMN_USAGE where " & _
                                                    "Table_Catalog = '" & g_ConnectionSchema & "' and Table_Name ='" & TableName & "'", _
                                                    False)
            If g_blnAbort Then
                Return Nothing
            End If
            ' 12/27/2011.cpb.
            Dim tblIsIdentity As DataTable = IO_Execute_MSSQL("SELECT c2.Name,c2.Is_Identity FROM sysobjects c1 inner join sys.columns c2 on c1.id = c2.object_id where c1.name like '" & _
                                                              TableName & "' AND c2.Is_Identity = 1", False)
            If g_blnAbort Then
                Return Nothing
            End If

            For Each rowFieldInfoExtracted In tblColumnInfo.Rows
                rowColumns = tblColumns.NewRow
                rowColumns("FieldName") = rowFieldInfoExtracted("Column_Name")
                rowColumns("Type") = getSQLDataType(rowFieldInfoExtracted("Data_Type"))
                rowColumns("AllowNull") = IIf(rowFieldInfoExtracted("Is_Nullable") = "YES", "1", "0")
                rowColumns("Default") = IIf(IsDBNull(rowFieldInfoExtracted("Column_Default")), "", _
                                            rowFieldInfoExtracted("Column_Default"))
                For Each rowKeyFieldExtracted In tblColumnKeyInfo.Rows
                    rowColumns("Index") = "0"     ' default to false
                    If rowColumns("FieldName") = rowKeyFieldExtracted("Column_Name") Then
                        rowColumns("Index") = "1"
                    End If
                Next
                '12/28/2011.cpb.set autoinc based on auto inc field from table..mssql can only have 1 auto inc field per table

                If tblIsIdentity.Rows.Count > 0 AndAlso rowFieldInfoExtracted("Column_Name") = tblIsIdentity(0)("Name") Then
                    rowColumns("AutoInc") = 1
                Else
                    rowColumns("AutoInc") = 0
                End If
                tblColumns.Rows.Add(rowColumns)
            Next
        End If

        Return tblColumns

    End Function
    Private Function getSQLDataType(ByVal DataType As String) As String
        Dim strString As String = "CHAR,TEXT,BLOB"
        Dim strNumber As String = "INT,DECI,DOUB,FLOA,BINA,MONEY,IDENT"
        Dim strDATE As String = "DATE"
        Dim strTime As String = "TIME"
        Dim strBoolean As String = "TINYINT,BIT"
        Dim strTypeDetermined As String = ""
        Do
            DataType = UCase(DataType)
            Try : DataType = Microsoft.VisualBasic.Left(DataType, DataType.IndexOf("(")) : Catch : End Try
            For Each strType As String In Split(strBoolean, ",")
                If DataType.Contains(strType) Then
                    strTypeDetermined = "BOO"
                    Exit Do
                End If
            Next
            For Each strType As String In Split(strString, ",")

                If DataType.Contains(strType) Then
                    strTypeDetermined = "STR"
                    Exit Do
                End If
            Next
            For Each strType As String In Split(strNumber, ",")
                If DataType.Contains(strType) Then
                    strTypeDetermined = "NUM"
                    Exit Do
                End If
            Next
            For Each strType As String In Split(strDATE, ",")
                If DataType.Contains(strType) Then
                    strTypeDetermined = "DAT"
                    Exit Do
                End If
            Next
            For Each strType As String In Split(strTime, ",")
                If DataType.Contains(strType) Then
                    strTypeDetermined = "TIM"
                    Exit Do
                End If
            Next
            strTypeDetermined = "STR"
            Exit Do
        Loop
        Return strTypeDetermined

    End Function
    Public Function g_ArchiveData(ByVal ActiveTableName As String, _
                            ByVal ArchiveTableName As String, _
                            ByVal WherePhrase_DoNotIncludeTheWordWhere As String, _
                            ByVal FormName As String) As Integer

        ' function returns # of records archived

        Dim strSQL As String = "Select * from " & ActiveTableName & " where " & WherePhrase_DoNotIncludeTheWordWhere
        Dim tblActiveData As DataTable = g_IO_Execute_SQL(strSQL, False)

        ' get field information for archive table so as to know auto increment filed
        Dim strAutoIncFields As String = ""
        Dim strDelim As String = ""
        Dim tblArchiveTable As DataTable = g_getTableColumnInfo(ArchiveTableName)
        For Each rowColumn In tblArchiveTable.Rows
            If rowColumn("AutoInc") = 1 Then
                strAutoIncFields &= strDelim & UCase(rowColumn("FieldName"))
                strDelim = ","
            End If
        Next
        Dim arrAutoIncFields() As String = Split(strAutoIncFields, ",")


        'move this over to the history table
        Dim nvcInsert As New NameValueCollection
        For Each rowItem As DataRow In tblActiveData.Rows
            For Each colFields As DataColumn In tblActiveData.Columns
                Dim blnIncludeColumn As Boolean = True
                If IsDBNull(rowItem(colFields.ColumnName)) Then
                    blnIncludeColumn = False
                    'skip adding this column since it's NULL
                Else
                    Dim strColumnName As String = UCase(colFields.ColumnName)
                    For Each strArrColumnName As String In arrAutoIncFields
                        If strArrColumnName = strColumnName Then
                            blnIncludeColumn = False
                            Exit For
                        End If
                    Next
                End If
                If blnIncludeColumn Then
                    nvcInsert(colFields.ColumnName) = rowItem(colFields.ColumnName)
                End If
            Next
            g_IO_SQLInsert(ArchiveTableName, nvcInsert, FormName)
        Next

        ' delete the records from the active
        g_IO_SQLDelete(ActiveTableName, WherePhrase_DoNotIncludeTheWordWhere)

        Return tblActiveData.Rows.Count
    End Function

    Private Function determineViewOrTable(ByVal tableName As String, ByRef blnIsView As Boolean) As String
        Dim strJoinTo As String = "sys.tables"
        Dim strSQL As String = "SELECT CAST(count(*) as tinyint) as isView FROM sys.views where name = '" & tableName & "'"
        blnIsView = g_IO_Execute_SQL(strSQL, False).Rows(0)("isView")
        If blnIsView Then
            strJoinTo = "sys.views"
        End If
        Return strJoinTo

    End Function
End Module
