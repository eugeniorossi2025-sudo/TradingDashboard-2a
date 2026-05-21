<%@ Application Language="VB" %>

<script runat="server">

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs on application startup
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs on application shutdown
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        If Server.GetLastError() is Nothing then Exit Sub
        Dim objErr As Exception = Server.GetLastError().GetBaseException()
        Dim computer_name As String() = System.Net.Dns.GetHostEntry(Request.ServerVariables("remote_addr")).HostName.Split(New [Char]() {"."c})
        Dim ecn As [String] = System.Environment.MachineName
        'txtECN.Text = computer_name(0).ToString()

        Dim Dp As Integer

        Dp = DatePart(DateInterval.WeekOfYear, Now)
        Dim err As String = "Error automatically generated on " & Now & "<br/>" &
                            System.Environment.NewLine &
                            "Error in: " & Request.Url.ToString() & "<br/>" &
                            System.Environment.NewLine &
                            "User: " & HttpContext.Current.User.Identity.Name & "<br/>" &
                             System.Environment.NewLine &
                            "Computer Name: " & computer_name(0).ToString() & "<br/>" &
                            System.Environment.NewLine &
                            "Ip Address: " & RetIPAddress() & "<br/>" &
                            System.Environment.NewLine &
                            "Error Messagge: " & objErr.Message.ToString() & "<br/>" &
                            System.Environment.NewLine &
                            "Stack Trace:" & objErr.StackTrace.ToString() & System.Environment.NewLine & "<br/>" &
                            "____________________________________________________________________________________________________" & "<br/>" &
                            System.Environment.NewLine
        'Dim aName As String = System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName
        'Dim aPath As String = System.IO.Path.GetDirectoryName(aName)
        'If System.IO.Directory.Exists(aPath & "\log") = False Then
        '    System.IO.Directory.CreateDirectory(aPath & "\log")
        'End If
        Dim aPath As String = ConfigurationSettings.AppSettings("Path.Log")
        aPath = Server.MapPath(aPath)
        Try
            System.IO.File.AppendAllText(aPath & "\log_" & Year(Now) & "_" & Dp.ToString() & ".txt", err)
            SendMail(ReturnValueWebConfig("Mail.Error.Subject"), err, ReturnValueWebConfig("Mail.Admin"))
        Catch ex As Exception

        End Try

        'EventLog.WriteEntry("Sample_WebApp", err, EventLogEntryType.Error)
        Server.ClearError()

        Response.Redirect("~/adminlogin.aspx")

    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when a new session is started
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when a session ends. 
        ' Note: The Session_End event is raised only when the sessionstate mode
        ' is set to InProc in the Web.config file. If session mode is set to StateServer 
        ' or SQLServer, the event is not raised.
    End Sub

    Protected Sub Application_BeginRequest(ByVal sender As Object, ByVal e As System.EventArgs)
        If (Request.IsSecureConnection = False) Then
             Response.Redirect("https://" & HttpContext.Current.Request.ServerVariables("HTTP_HOST") & Request.RawUrl)

        End If
    End Sub
</script>