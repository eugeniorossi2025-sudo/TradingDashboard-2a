Imports Telerik.Web.UI
Imports Controls
Imports System.Data
Imports System.IO
Imports System.ServiceModel
Imports System.Xml
Imports ServiceReference1
Imports System.Net
Imports System.Data.SqlClient

Partial Class Configurations
    Inherits WebAppPageBO
    Dim rs As New ADODB.Recordset
    Dim cn As New ADODB.Connection
    Dim SqlGrid As String = "Ups_Configurations_BO"
    Dim TableName As String = "dbo.Configurations"
    Dim _PageTitle As String = "Configurations"
    Dim bEdit As Boolean = True
    Dim bDelete As Boolean = True


    Protected Sub LnkExportExcel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LnkExportExcel.Click
        ExportGrid(_PageTitle.Replace(" ", "_"), RadGrid1)
    End Sub

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
            .CommandType = Data.CommandType.StoredProcedure
        End With

        Da.SelectCommand = Cmd
        Da.Fill(Dt)
        oCon.Close()
        oCon.Dispose()
        RadGrid1.DataSource = Dt

        RadGrid1.Columns(0).Visible = bEdit
        RadGrid1.Columns(1).Visible = bDelete
    End Sub

    Private Sub LoadById(id As Integer)

        Dim ConnString As String = ConfigurationManager.ConnectionStrings("WebAppADOConnectionString").ConnectionString
        cn.Open(ConnString)

        rs.Open("select * from " & TableName & " where k='" & TxtId.Text & "'", cn, 1, 3)

        Div_Error.Visible = False
        Div_Warning.Visible = False
        Div_Terminated.Visible = False
        LoadData(rs, Page.Controls)

        cn.Close()
    End Sub

    Protected Sub RadGrid1_ItemCommand(ByVal source As Object, ByVal e As Telerik.Web.UI.GridCommandEventArgs) Handles RadGrid1.ItemCommand
        If (e.CommandName.ToString().ToLower() = "open") Then
            MultiView.ActiveViewIndex = 1
            Clean(Page.Controls)
            TxtId.Text = e.CommandArgument.ToString

            LoadById(Utility.VerInt32(TxtId.Text))

            Div_Error.Visible = False
            Div_Warning.Visible = False
            Div_Terminated.Visible = False
        End If


        If (e.CommandName.ToString().ToLower() = "delete") Then
            ExecuteSQL("Delete from " & TableName & " where k='" & e.CommandArgument & "'")

            RadGrid1.Rebind()
        End If

        If e.CommandName = RadGrid.ExportToExcelCommandName Then
            RadGrid1.ExportSettings.Excel.Format = GridExcelExportFormat.Biff
            RadGrid1.ExportSettings.IgnorePaging = True
            RadGrid1.ExportSettings.ExportOnlyData = True
            RadGrid1.ExportSettings.OpenInNewWindow = True
        End If
    End Sub

    Protected Sub LnkNew_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LnkNew.Click
        MultiView.ActiveViewIndex = 1
        TxtId.Text = 0

        Clean(Page.Controls)
        Div_Error.Visible = False
        Div_Warning.Visible = False
        Div_Terminated.Visible = False
    End Sub

    Protected Sub LnkRefresh_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LnkRefresh.Click
        RadGrid1.Rebind()

    End Sub

    Protected Sub btnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        MultiView.ActiveViewIndex = 0
        TxtId.Text = 0
    End Sub
    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click

        Dim ConnString As String = ConfigurationManager.ConnectionStrings("WebAppADOConnectionString").ConnectionString
        cn.Open(ConnString)

        If TxtId.Text = "0" Or TxtId.Text = "" Then
            rs.Open("select * from " & TableName & " Where 1=0", cn, 1, 3)
        Else
            rs.Open("select * from " & TableName & " Where k='" & TxtId.Text & "'", cn, 1, 3)
        End If

        Div_Error.Visible = False
        Div_Warning.Visible = False
        Div_Terminated.Visible = False
        If CheckValidity(Page.Controls) Then

            If CheckRequired(Page.Controls) Then
                If rs.EOF Then
                    rs.AddNew()
                End If
                SaveData(rs, Page.Controls)
                rs.Update()

                Div_Terminated.Visible = True
                MultiView.ActiveViewIndex = 0
                RadGrid1.Rebind()
            Else
                Div_Error.Visible = True
            End If
        Else
            Div_Warning.Visible = True
        End If

        cn.Close()
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not (IsPostBack) Then
            If Utility.VerInt32(RetUserInfo("ID")) = 0 Then 'Or Utility.VerBool(RetUserInfo("Administrator")) = False Then
                Response.Redirect("/adminlogin.aspx")
            End If
        End If
        Enable(Page.Controls, True)
        LnkNew.Visible = True
        btnSave.Visible = True
        LnkExportExcel.Visible = False
        LblInfo.Text = _PageTitle



        ScriptManager.GetCurrent(Me.Page).RegisterPostBackControl(LnkExportExcel)
    End Sub


#Region "Automatics Behaviours"
    Private Sub LoadData(ByVal RsCarica As ADODB.Recordset, ByVal cs As ControlCollection)
        For Each c As Control In cs
            Try

                If TypeOf c Is Control_Txt Then
                    Dim Field As Control_Txt = c
                    Field.Text = RsCarica.Fields(Field.DataField).Value
                End If
                If TypeOf c Is Control_TxtFile Then
                    Dim Field As Control_TxtFile = c
                    Field.Text = RsCarica.Fields(Field.DataField).Value
                End If
                If TypeOf c Is Control_Chk Then
                    Dim Field As Control_Chk = c
                    Field.Value = RsCarica.Fields(Field.DataField).Value
                End If
                If TypeOf c Is Control_TxtDate Then
                    Dim Field As Control_TxtDate = c
                    Field.Text = RsCarica.Fields(Field.DataField).Value
                End If
                If TypeOf c Is Control_Drp Then
                    Dim Field As Control_Drp = c
                    Field.Text = RsCarica.Fields(Field.DataField).Value
                End If
                If c.Controls.Count > 0 Then
                    LoadData(RsCarica, c.Controls)
                End If
            Catch ex As Exception

            End Try
        Next
    End Sub

    Public Function CheckRequired(ByVal cs As ControlCollection) As Boolean
        For Each c As Control In cs
            Try

                Trace.Write("Field " + c.ID)
                If TypeOf c Is Control_Txt Then
                    Dim Field As Control_Txt = c
                    If Field.Required And Field.Text = "" Then
                        Return False
                    End If
                End If

                If TypeOf c Is Control_TxtFile Then
                    Dim Field As Control_TxtFile = c
                    If Field.Required And Field.Text = "" Then
                        Return False
                    End If
                End If

                If TypeOf c Is Control_TxtDate Then
                    Dim Field As Control_TxtDate = c
                    If Field.Required And Field.Text = Nothing Then
                        Return False
                    End If

                End If
                If TypeOf c Is Control_Drp Then
                    Dim Field As Control_Drp = c
                    If Field.Required And (Field.Text = "" Or Field.Text = "0") Then
                        Return False
                    End If

                End If
                If c.Controls.Count > 0 Then
                    Dim ret = CheckRequired(c.Controls)
                    If ret = False Then
                        Return False
                    End If
                End If
            Catch ex As Exception

            End Try
        Next
        Return True
    End Function

    Public Function CheckValidity(ByVal cs As ControlCollection) As Boolean
        For Each c As Control In cs
            Trace.Write("Field " + c.ID)
            Try

                If TypeOf c Is Control_Txt Then
                    Dim Field As Control_Txt = c
                    If Field.Numeric And Not (IsNumeric(Field.Text)) Then
                        Return False
                    End If
                End If
                If TypeOf c Is Control_TxtDate Then
                    Dim Field As Control_TxtDate = c
                    If Not (IsDate(Field.Text)) Then
                        Return False
                    End If
                End If
                If c.Controls.Count > 0 Then
                    Dim ret = CheckValidity(c.Controls)
                    If ret = False Then
                        Return False
                    End If
                End If
            Catch ex As Exception

            End Try
        Next
        Return True
    End Function

    Public Sub SaveData(ByVal RsSalva As ADODB.Recordset, ByVal cs As ControlCollection, Optional ByVal bSalvaId As Boolean = False)
        For Each c As Control In cs
            Try

                If TypeOf c Is Control_Txt Then
                    Dim Field As Control_Txt = c
                    RsSalva.Fields(Field.DataField).Value = Field.Text
                End If

                If TypeOf c Is Control_TxtFile Then
                    Dim Field As Control_TxtFile = c
                    RsSalva.Fields(Field.DataField).Value = Field.Text
                End If

                If TypeOf c Is Control_Chk Then
                    Dim Field As Control_Chk = c
                    RsSalva.Fields(Field.DataField).Value = Field.Value
                End If
                If TypeOf c Is Control_TxtDate Then
                    Dim Field As Control_TxtDate = c
                    If Field.Text = CDate("#12:00:00 AM#") Then
                        RsSalva.Fields(Field.DataField).Value = DBNull.Value
                    Else
                        RsSalva.Fields(Field.DataField).Value = Format(Field.Text, "yyyy-MM-dd")
                    End If
                End If
                If TypeOf c Is Control_Drp Then
                    Dim Field As Control_Drp = c
                    RsSalva.Fields(Field.DataField).Value = Field.Text
                End If
                If bSalvaId And TypeOf c Is TextBox Then
                    If c.ID.ToLower = "txtid" Then
                        Dim Field As TextBox = c
                        RsSalva.Fields("ID").Value = Field.Text
                    End If
                End If
                If c.Controls.Count > 0 Then
                    SaveData(RsSalva, c.Controls, bSalvaId)
                End If
            Catch ex As Exception
                Dim a As Integer
            End Try
        Next
    End Sub

    Public Function Clean(ByVal cs As ControlCollection) As Boolean
        For Each c As Control In cs
            Try

                Trace.Write("Field " + c.ID)
                If TypeOf c Is TextBox Then
                    Dim Field As TextBox = c
                    Field.Text = ""
                End If
                If TypeOf c Is Control_Txt Then
                    Dim Field As Control_Txt = c
                    Field.Text = ""
                End If
                If TypeOf c Is Control_TxtFile Then
                    Dim Field As Control_TxtFile = c
                    Field.Text = ""
                End If
                If TypeOf c Is Control_TxtDate Then
                    Dim Field As Control_TxtDate = c
                    Field.Text = Nothing
                End If
                If TypeOf c Is Control_Drp Then
                    Dim Field As Control_Drp = c
                    Field.Text = ""
                End If

                If TypeOf c Is CheckBox Then
                    Dim Field As CheckBox = c
                    Field.Checked = False
                End If
                If TypeOf c Is Control_Chk Then
                    Dim Field As Control_Chk = c
                    Field.Value = False
                End If
                If TypeOf c Is DropDownList Then
                    Dim Field As DropDownList = c
                    Field.SelectedIndex = 0
                End If
                If TypeOf c Is RadDatePicker Then
                    Dim Field As RadDatePicker = c
                    Field.DbSelectedDate = Date.Now
                End If
                If c.Controls.Count > 0 Then
                    Clean(c.Controls)
                End If
            Catch ex As Exception

            End Try
        Next

        Return True
    End Function

    Public Function Enable(ByVal cs As ControlCollection, ByVal enabled As Boolean) As Boolean
        For Each c As Control In cs
            Try

                Trace.Write("Field " + c.ID)
                If TypeOf c Is TextBox Then
                    Dim Field As TextBox = c
                    Field.Enabled = enabled
                End If
                If TypeOf c Is Control_Txt Then
                    Dim Field As Control_Txt = c
                    Field.Enabled = enabled
                End If
                If TypeOf c Is Control_TxtFile Then
                    Dim Field As Control_TxtFile = c
                    Field.Enabled = enabled
                End If
                If TypeOf c Is Control_TxtDate Then
                    Dim Field As Control_TxtDate = c
                    Field.Enabled = enabled
                End If
                If TypeOf c Is Control_Drp Then
                    Dim Field As Control_Drp = c
                    Field.Enabled = enabled
                End If

                If TypeOf c Is CheckBox Then
                    Dim Field As CheckBox = c
                    Field.Enabled = enabled
                End If
                If TypeOf c Is Control_Chk Then
                    Dim Field As Control_Chk = c
                    Field.Enabled = enabled
                End If
                If TypeOf c Is DropDownList Then
                    Dim Field As DropDownList = c
                    Field.Enabled = enabled
                End If
                If TypeOf c Is RadDatePicker Then
                    Dim Field As RadDatePicker = c
                    Field.Enabled = enabled
                End If
                If c.Controls.Count > 0 Then
                    Enable(c.Controls, enabled)
                End If
            Catch ex As Exception

            End Try
        Next

        Return True
    End Function

#End Region
End Class


