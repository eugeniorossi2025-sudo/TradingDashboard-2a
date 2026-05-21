Imports Microsoft.VisualBasic
Imports System.Data.SqlClient
Imports System.Data.SqlTypes
Imports System.Net.Mail
Imports System.Data
Public Module Functions

    Public Function GetEmptyDt() As DataTable
        Dim Dt As New DataTable
        Dt.Columns.Add("Id", GetType(Integer))
        Dt.Columns.Add("Description", GetType(String))
        Dt.Columns.Add("Index", GetType(String))
        Return Dt
    End Function


    Public Sub WriteRequestLog(testo As String)

        Dim aPath As String = ConfigurationSettings.AppSettings("Path.Log") + "Requests"
        aPath = HttpContext.Current.Server.MapPath(aPath)
        Dim file_name As String = "Log_" + DateTime.Now.ToString("yyyy_MM_dd_HH") + ".txt"
        Try

            If Not System.IO.Directory.Exists(aPath) Then
                System.IO.Directory.CreateDirectory(aPath)
            End If

            testo = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + " " + testo + Environment.NewLine

            System.IO.File.AppendAllText(aPath & "\logRequest_" & file_name, testo)

        Catch ex As Exception

        End Try

    End Sub



    Public Function RetConfiguration(Field As String, Optional Con As SqlConnection = Nothing, Optional bReadFromDb As Boolean = False) As String

        Dim oCon As SqlConnection = Nothing
        Dim Dt_Configuration As DataTable = HttpContext.Current.Session("Configuration")
        Dim RetValue As String = ""
        Dim oRow() As DataRow


        If Dt_Configuration Is Nothing Or bReadFromDb Then

            If Con Is Nothing Then
                oCon = GetConn()
            Else
                oCon = Con
            End If

            Dt_Configuration = GetDt("select * from Configurations order by pos", oCon)
            HttpContext.Current.Session("Configuration") = Dt_Configuration


            If Con Is Nothing Then
                oCon.Close()
                oCon.Dispose()
            End If
        End If

        oRow = Dt_Configuration.Select("K='" & Field & "'")
        If oRow.Length > 0 Then
            RetValue = Utility.VerString(oRow(0)("Value"))
        Else
            RetValue = ""
        End If

        Return RetValue


    End Function

    Public Sub LoadDropDown(ByVal cboObject As DropDownList, ByVal Sql As String, ByVal oCon As SqlConnection, Optional ByVal BlankRow As Boolean = False, Optional ByVal SelectedValue As String = "")
        Try
            cboObject.DataSource = GetDs(Sql, Nothing, oCon)
            cboObject.DataValueField = "Id"
            cboObject.DataTextField = "Description"
            cboObject.DataBind()

            If BlankRow Then
                Dim lst As New ListItem
                lst.Text = ""
                lst.Value = 0
                cboObject.Items.Insert(0, lst)
                'cboObject.Items.Insert(0, "")
            End If
            If SelectedValue <> "" Then cboObject.SelectedValue = SelectedValue
        Catch ex As Exception
            Dim strErrore As String = ""
        End Try
    End Sub

    Sub Log(ByVal MethodName As String, ByVal str As String)

        Dim Dp As Integer
        Dim Pc1 As String = ""
        Dim Pc2 As String = ""
        Dim Pc3 As String = ""
        Try
            Dim computer_name As String() = System.Net.Dns.GetHostEntry(HttpContext.Current.Request.ServerVariables("remote_addr")).HostName.Split(New [Char]() {"."c})
            Pc1 = computer_name(0).ToString()
            Pc2 = computer_name(1).ToString()
            Pc3 = computer_name(2).ToString()
        Catch ex As Exception
            Pc1 = ""
            Pc2 = ""
            Pc3 = ""
        End Try
        Dim ecn As [String] = System.Environment.MachineName

        Dim err As String = "Error automatically generated at " & Now & "<br/>" &
                            System.Environment.NewLine &
                            "MethodName:" & MethodName & "<br/>" &
                            System.Environment.NewLine &
                            "Error in: " & HttpContext.Current.Request.Url.ToString() & "<br/>" &
                            System.Environment.NewLine &
                            "User: " & HttpContext.Current.User.Identity.Name & "<br/>" &
                            System.Environment.NewLine &
                            "Language: " & HttpContext.Current.Session("language") & "<br/>" &
                            System.Environment.NewLine &
                            "Computer Name: " & Pc1 & "     " & Pc2 & "     " & Pc3 & "     (" & ecn & ")<br/>" &
                            System.Environment.NewLine &
                            "User Agent: " & HttpContext.Current.Request.UserAgent & "<br/>" &
                            System.Environment.NewLine &
                            "Ip Address: " & RetIPAddress() & "<br/>" &
                            System.Environment.NewLine &
                            "Error Message: " & str & "<br/>" &
                            "____________________________________________________________________________________________________" & "<br/>" &
                            System.Environment.NewLine


        Dp = DatePart(DateInterval.WeekOfYear, Now)
        Dim aPath As String = ConfigurationSettings.AppSettings("Path.Log")
        aPath = HttpContext.Current.Server.MapPath(aPath)

        Try
            System.IO.File.AppendAllText(aPath & "\log_" & Year(Now) & "_" & Dp.ToString() & ".txt", err)
            SendMail(ReturnValueWebConfig("Mail.Error.Subject"), err, ReturnValueWebConfig("Mail.Admin"))
        Catch ex As Exception

        End Try
    End Sub
    Public Sub SignOut()

        HttpContext.Current.Session.Remove("User_Info")
        HttpContext.Current.Session.Remove("RefreshGraph")
        HttpContext.Current.Session.Remove("Configurations")
        FormsAuthentication.SignOut()
        HttpContext.Current.Session.Clear()
        HttpContext.Current.Response.Redirect("~/adminlogin.aspx")
    End Sub
    Public Function IsAuthenticated() As Boolean
        If RetUserInfo("ID") = "" Or Not (HttpContext.Current.User.Identity.IsAuthenticated) Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Function RetConfiguration(ByVal Key As String) As String
        Dim Dt_Configurations As DataTable = HttpContext.Current.Session("Configurations")
        Dim RetValue As String = ""
        Dim oRow As DataRow()
        If Dt_Configurations Is Nothing Then
            RetValue = ""
        Else

            If Dt_Configurations.Rows.Count > 0 Then
                oRow = Dt_Configurations.Select("K='" & Key & "'")
                RetValue = Utility.VerString(oRow(0)("Value"))
            Else
                RetValue = ""
            End If
        End If

        Return RetValue
    End Function

    Public Function RetUserInfo(ByVal Field As String) As String
        Dim Dt_UserInfo As DataTable = HttpContext.Current.Session("User_Info")
        Dim RetValue As String = ""
        If Dt_UserInfo Is Nothing Then
            RetValue = ""
        Else
            If Dt_UserInfo.Rows.Count > 0 Then
                RetValue = Utility.VerString(Dt_UserInfo.Rows(0)(Field))
            Else
                RetValue = ""
            End If
        End If

        Return RetValue
    End Function

    Public Function RetUrl() As String
        Return ConfigurationSettings.AppSettings("Project.Url") & HttpContext.Current.Request.RawUrl
    End Function
    Public Function RetTitle() As String
        Dim p As Page = HttpContext.Current.Handler
        Return p.Title
    End Function

    Public Function StrToGuid(ByVal strValue As String) As Guid
        Try
            If Not String.IsNullOrEmpty(strValue) Then
                Dim value As New Guid(strValue)
                Return value
            Else
                Return Guid.Empty
            End If
        Catch generatedExceptionName As FormatException
            Dim value As Guid = Guid.Empty
            Return value
        End Try
    End Function


    Public Function ReturnValueWebConfig(ByVal value As String)
        Return ConfigurationSettings.AppSettings(value)
    End Function

 
    Public Function GetUrlAppRoot(ByVal Page As System.Web.UI.Page) As String
        Dim Res As String = Page.Request.Url.AbsoluteUri.Replace(Page.Request.Url.AbsolutePath, String.Empty)
        If Not String.IsNullOrEmpty(Page.Request.Url.Query) Then
            Res = Res.Replace(Page.Request.Url.Query, String.Empty)
        End If
        Res &= Page.Request.ApplicationPath
        If Res.Substring(Res.Length() - 1) <> "/" Then
            Res &= "/"
        End If
        Return Res
    End Function
    Public Function IsValidEmail(ByVal email As String) As Boolean
        'regular expression pattern for valid email
        'addresses, allows for the following domains:
        'com,edu,info,gov,int,mil,net,org,biz,name,museum,coop,aero,pro,tv
        Dim pattern As String = "^[-a-zA-Z0-9][-.a-zA-Z0-9]*@[-.a-zA-Z0-9]+(\.[-.a-zA-Z0-9]+)*\." & _
        "(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$"
        'Regular expression object
        Dim check As New Text.RegularExpressions.Regex(pattern, RegexOptions.IgnorePatternWhitespace)
        'boolean variable to return to calling method
        Dim valid As Boolean = False

        'make sure an email address was provided
        If String.IsNullOrEmpty(email) Then
            valid = False
        Else
            'use IsMatch to validate the address
            valid = check.IsMatch(email)
        End If
        'return the value to the calling method
        Return valid
    End Function

    Public Function RetIPAddress() As String
        Dim context As System.Web.HttpContext = System.Web.HttpContext.Current
        Dim sIPAddress As String = context.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrEmpty(sIPAddress) Then
            Return context.Request.ServerVariables("REMOTE_ADDR")
        Else
            Dim ipArray As String() = sIPAddress.Split(New [Char]() {","c})
            Return ipArray(0)
        End If
    End Function
    Public Function FormatValueForWebAddress(ByVal value As String) As String
        Dim Risultato As String = ""

        Risultato = value
        Risultato = Risultato.ToLower()
        Risultato = Risultato.Replace(" ", "-")
        Risultato = Risultato.Replace(",", "")
        Risultato = Risultato.Replace(".", "")
        Risultato = Risultato.Replace("\", "_")
        Risultato = Risultato.Replace("/", "_")
        Risultato = Risultato.Replace(":", "-")
        Risultato = Risultato.Replace("*", "-")
        Risultato = Risultato.Replace("?", "-")
        Risultato = Risultato.Replace("<", "-")
        Risultato = Risultato.Replace(">", "-")
        Risultato = Risultato.Replace("|", "-")
        Risultato = Risultato.Replace("""", "")
        Risultato = Risultato.Replace("'", "-")
        Risultato = Risultato.Replace("ò", "o")
        Risultato = Risultato.Replace("à", "a")
        Risultato = Risultato.Replace("è", "e")
        Risultato = Risultato.Replace("é", "e")
        Risultato = Risultato.Replace("ì", "i")
        Risultato = Risultato.Replace("ù", "u")
        Risultato = Risultato.Replace("&", "And")
        Risultato = Risultato.Replace("+", "_")
        Return Risultato
    End Function
End Module
     