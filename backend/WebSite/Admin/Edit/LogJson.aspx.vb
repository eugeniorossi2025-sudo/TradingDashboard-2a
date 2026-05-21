Imports Telerik.Web.UI
Imports Controls
Imports System.Data
Imports System.IO
Imports System.ServiceModel
Imports System.Xml
Imports ServiceReference1
Imports System.Net
Imports System.Data.SqlClient
Imports System.Drawing

Partial Class LogJson
    Inherits WebAppPageBO
    Dim rs As New ADODB.Recordset
    Dim cn As New ADODB.Connection
    Dim SqlGrid As String = "Select * from SafeGuardJson order by ID"
    Dim TableName As String = "SafeGuardJson"
    Dim _PageTitle As String = "LogJson"


    Protected Sub RadGrid1_ColumnCreated(ByVal sender As Object, ByVal e As Telerik.Web.UI.GridColumnCreatedEventArgs) Handles RadGrid1.ColumnCreated
        'Give the DataField value as the Unique Name of the Column that you want to hide 
        If e.Column.UniqueName.ToUpper = "ID" Then
            ' e.Column.Display = False
        End If
    End Sub

    Protected Sub RadGrid1_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles RadGrid1.Init
        Dim menu As GridFilterMenu = RadGrid1.FilterMenu
        Dim i As Integer = 0

        While i < menu.Items.Count

            If menu.Items(i).Text = "NoFilter" OrElse menu.Items(i).Text = "Contains" OrElse menu.Items(i).Text = "EqualTo" OrElse menu.Items(i).Text = "GreaterThan" OrElse menu.Items(i).Text = "LessThan" Then
                i += 1
            Else
                menu.Items.RemoveAt(i)
            End If
        End While
    End Sub

    Protected Sub RadGrid1_NeedDataSource(ByVal source As Object, ByVal e As GridNeedDataSourceEventArgs) Handles RadGrid1.NeedDataSource
        Dim Cmd As New Data.SqlClient.SqlCommand
        Dim Dt As New Data.DataTable()
        Dim Da As New System.Data.SqlClient.SqlDataAdapter()
        Dim oCon As SqlClient.SqlConnection

        oCon = GetConn()
        With Cmd
            .Connection = oCon
            .CommandText = SqlGrid
            .CommandType = Data.CommandType.Text
        End With

        Da.SelectCommand = Cmd
        Da.Fill(Dt)
        oCon.Close()
        oCon.Dispose()
        RadGrid1.DataSource = Dt
    End Sub




    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not (IsPostBack) Then
            If Utility.VerInt32(RetUserInfo("ID")) = 0 Then
                Response.Redirect("/adminlogin.aspx")
            End If
        End If

        LblInfo.Text = _PageTitle

    End Sub
End Class


