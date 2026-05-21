
Imports System.Data.SqlClient

Partial Class _login
    Inherits System.Web.UI.Page


    Protected Sub Lnk_Login_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Btn_Login.Click

        Dim oCon As SqlConnection = GetConn()
        Dim Dt As New Data.DataTable

        If Users.ValidateUser(TxtUsername.Text, TxtPassword.Text, oCon) Then
            FormsAuthentication.SetAuthCookie(TxtUsername.Text, True)

            '****************************************************
            Dim Cmd_User As New SqlCommand
            Dim Da As New SqlDataAdapter
            Dt = New Data.DataTable
            With Cmd_User
                .CommandTimeout = 100000
                .Connection = oCon
                .CommandText = "upS_Users"
                .CommandType = Data.CommandType.StoredProcedure
                Data.SqlClient.SqlCommandBuilder.DeriveParameters(Cmd_User)
                .Parameters("@Username").Value = TxtUsername.Text
            End With

            Da.SelectCommand = Cmd_User
            Da.Fill(Dt)


            If Dt.Rows.Count > 0 Then
                HttpContext.Current.Session("User_Info") = Dt
            End If

            'recupero le configurazioni
            Dt = New Data.DataTable
            Dim Cmd_Configurations As New SqlCommand
            Dt = New Data.DataTable
            With Cmd_Configurations
                .CommandTimeout = 100000
                .Connection = oCon
                .CommandText = "Ups_Configurations_BO"
                .CommandType = Data.CommandType.StoredProcedure
                Data.SqlClient.SqlCommandBuilder.DeriveParameters(Cmd_User)
            End With

            Da.SelectCommand = Cmd_Configurations
            Da.Fill(Dt)

            If Dt.Rows.Count > 0 Then
                HttpContext.Current.Session("Configurations") = Dt
            End If
            '************************
            oCon.Close()
            oCon.Dispose()

            Response.Redirect("/Admin/Default.aspx")
        Else
            oCon.Close()
            oCon.Dispose()
            Lit_Info.Visible = True
        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'If Not (IsPostBack) Then
        '    Dim Value As String = RetConfiguration("GOOGLE_ANALYTICS")
        '    Dim lt As New Literal
        '    If Value <> "" Then
        '        Dim str As String = "<script async src=""https://www.googletagmanager.com/gtag/js?id="" & Value & ""></script> <script>window.dataLayer = window.dataLayer || [];function gtag(){dataLayer.push(arguments);}gtag('js', new Date()); gtag('config', '" & Value & "');</script>"
        '        lt.Text = str
        '        Head1.Controls.Add(lt)

        '    End If
        'End If
    End Sub


End Class
