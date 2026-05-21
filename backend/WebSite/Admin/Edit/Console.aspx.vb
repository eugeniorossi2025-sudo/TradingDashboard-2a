Imports Telerik.Web.UI
Imports Controls
Imports System.Data
Imports System.IO
Imports System.Text.Json
Imports EuGenio.ProattivoSempliceRegiaAstronave
Imports System.Timers
Imports System.Runtime.InteropServices
Imports System.Data.SqlClient
Imports System.Drawing
Imports EuGenio.ProattivoSempliceRegiaAstronave.ProactiveEngine
Imports DocumentFormat.OpenXml.Math
Imports Telerik.Web.UI.Skins
Imports DocumentFormat.OpenXml.Drawing.Diagrams
Imports System.Data.SqlTypes

Partial Class Console
    Inherits WebAppPageBO

    Private regiaSettings As ProactiveSettings
    Private telemetriaValues As StateData
    Private Shared WithEvents telemetryTimer As Timers.Timer
    Protected Overloads Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Try
            If Not (IsPostBack) Then
                LeggiSettings()
                AggiornaTelemetria()
            End If

        Catch ex As Exception
            Response.Write("<span style='color:red'>Errore inizializzazione Regia: {ex.Message}</span>")
        End Try
    End Sub

    Protected Sub TimerTelemetry_Tick(ByVal sender As Object, ByVal e As EventArgs)
        AggiornaTelemetria()
    End Sub

    Private Sub LeggiSettings()
        Dim json_text As String
        Dim oCon As SqlConnection = GetConn()

        Using cmd As New SqlCommand("UpS_SettingsJson", oCon) ' Nome della tua Stored Procedure
            cmd.CommandType = CommandType.StoredProcedure
            SqlCommandBuilder.DeriveParameters(cmd)
            json_text = cmd.ExecuteScalar()
        End Using
        oCon.Close()
        oCon.Dispose()
        Try
            regiaSettings = JsonSerializer.Deserialize(Of ProactiveSettings)(json_text)

            TxtHmaxHigh.Text = regiaSettings.HmaxHigh
            TxtHmaxMid.Text = regiaSettings.HmaxMid
            TxtHmaxLow.Text = regiaSettings.HmaxLow
            TxtCooldownHigh.Text = regiaSettings.CooldownHigh
            TxtCooldownMid.Text = regiaSettings.CooldownMid
            TxtCooldownLow.Text = regiaSettings.CooldownLow
            TxtHighThresh.Text = regiaSettings.HighThresh
            txtLowThresh.Text = regiaSettings.LowThresh
            txtGlobalHeavyCap.Text = regiaSettings.GlobalHeavyCap
            txtPerTableHeavyLimit.Text = regiaSettings.PerTableHeavyLimit
            txtWindowW10.Text = regiaSettings.WindowW10
            txtMaxRunPAllowed.Text = regiaSettings.MaxRunPAllowed
            TxtDebtTriggerRatio.Text = regiaSettings.DebtTriggerRatio
            txtSyncDelayMs.Text = regiaSettings.SyncDelayMs
        Catch ex As Exception

        End Try


    End Sub

    Protected Sub RadGrid1_NeedDataSource(sender As Object, e As GridNeedDataSourceEventArgs)
        Dim Dt As New DataTable

        RadGrid1.DataSource = Dt
    End Sub
    Private Sub AggiornaTelemetria()
        Dim json_text As String
        Dim oCon As SqlConnection = GetConn()
        Dim cnt As Integer
        Dim cntGreen As Integer
        Dim cntHeavy As Integer
        Dim _globalMargin As String
        Dim DtJson As New DataTable
        Dim Cmd As New SqlCommand
        Dim Da As New SqlDataAdapter

        'Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
        '    cmd.CommandType = CommandType.StoredProcedure
        '    SqlCommandBuilder.DeriveParameters(cmd)
        '    json_text = cmd.ExecuteScalar()
        'End Using

        '------

        With Cmd
            .Connection = oCon
            .CommandText = "UpS_ConsoleJson"
            .CommandType = Data.CommandType.StoredProcedure
        End With

        Da.SelectCommand = Cmd
        Da.Fill(DtJson)
        If DtJson.Rows.Count > 0 Then
            json_text = DtJson.Rows(0)("json")
            _globalMargin = DtJson.Rows(0)("margine")
        End If

        '------
        oCon.Close()
        oCon.Dispose()

        Try

            '****************************************************
            ' Crea una nuova istanza della DataTable
            Dim dt As New DataTable("TavoloDati")

            Dim colTavolo As New DataColumn("Tavolo", GetType(String))
            dt.Columns.Add(colTavolo)
            Dim colMargin As New DataColumn("Margin", GetType(Decimal))
            dt.Columns.Add(colMargin)
            Dim colHeavy As New DataColumn("Heavy", GetType(Boolean))
            dt.Columns.Add(colHeavy)
            Dim colSignal As New DataColumn("Signal", GetType(String))
            dt.Columns.Add(colSignal)
            Dim colZone As New DataColumn("Zone", GetType(String))
            dt.Columns.Add(colZone)
            Dim newRow As DataRow

            '****************************************************
            telemetriaValues = JsonSerializer.Deserialize(Of StateData)(json_text)

            'For Each item As KeyValuePair(Of Integer, RowState) In telemetriaValues.Rows
            '    If item.Key > 0 Then
            '        _globalMargin = 0 '_globalMargin + item.Value.PrevMargine
            '    End If
            'Next

            For Each item In telemetriaValues.LastAdvice
                If item.Key > 0 Then
                    cnt = cnt + 1
                    '_globalMargin = _globalMargin + item.Value.GlobalMargin
                    'SignalW10
                    If item.Value.SignalTableW10.ToUpper = "GREEN" Then
                        cntGreen = cntGreen + 1
                    End If
                    'AuthorizedHeavy
                    If item.Value.AuthorizedHeavy Then
                        cntHeavy = cntHeavy + 1
                    End If

                    'Aggiungo le righe al datatable

                    newRow = dt.NewRow()
                    newRow("Tavolo") = "PC" & item.Value.TableId.ToString
                    newRow("Margin") = telemetriaValues.Rows(item.Value.TableId).PrevMargine
                    newRow("Heavy") = item.Value.AuthorizedHeavy.ToString
                    newRow("Signal") = item.Value.SignalTableW10
                    newRow("Zone") = item.Value.HotZoneLabel
                    dt.Rows.Add(newRow)
                End If
            Next
            RadGrid1.DataSource = dt
            RadGrid1.Rebind()
            Lbl_GlobalMargin.Text = _globalMargin
            Lbl_tablesActive.Text = cnt
            'globalHeavy
            If cntHeavy > (cnt - cntHeavy) Then
                Lbl_globalHeavy.Text = "True (" & (cntHeavy).ToString() & "/" & cnt.ToString & ")"
            Else
                Lbl_globalHeavy.Text = "False (" & (cnt - cntHeavy).ToString() & "/" & cnt.ToString & ")"
            End If
            '
            'globalSignal
            If cntGreen > (cnt - cntGreen) Then
                Lbl_globalSignal.Text = "Green (" & (cntGreen).ToString() & "/" & cnt.ToString & ")"
            Else
                Lbl_globalSignal.Text = "YellowOrRed (" & (cnt - cntGreen).ToString() & "/" & cnt.ToString & ")"
            End If

            UpdatePanelTelemetry.Update()
        Catch ex As Exception

        End Try

    End Sub


    Protected Sub btnApply_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnApply.Click
        Dim oCon As SqlConnection
        Try

            ' 1. Suddividi la stringa pulita in un array di stringhe.
            Dim StringheLivelli As String() = txtLevels.Text.Replace("[", "").Replace("]", "").Trim.Split(","c)
            ' 2. Crea la lista finale di interi (List(Of Integer)).
            Dim ListaLivelliNumerici As New List(Of Integer)

            ' 3. Converti ogni stringa in un intero e aggiungilo alla lista.
            For Each s As String In StringheLivelli
                ' Assicurati di fare il Trim() anche qui per pulire eventuali spazi
                ' tra le virgole e i numeri (es. "155, 340")
                If Not String.IsNullOrEmpty(s.Trim()) Then
                    ListaLivelliNumerici.Add(Integer.Parse(s.Trim()))
                End If
            Next

            'imposto i settings
            regiaSettings = New ProactiveSettings() With {
            .HmaxHigh = Integer.Parse(TxtHmaxHigh.Text),
            .HmaxMid = Integer.Parse(TxtHmaxMid.Text),
            .HmaxLow = Integer.Parse(TxtHmaxLow.Text),
            .CooldownHigh = Integer.Parse(TxtCooldownHigh.Text),
            .CooldownMid = Integer.Parse(TxtCooldownMid.Text),
            .CooldownLow = Integer.Parse(TxtCooldownLow.Text),
            .HighThresh = Integer.Parse(TxtHighThresh.Text),
            .LowThresh = Integer.Parse(txtLowThresh.Text),
            .GlobalHeavyCap = Integer.Parse(txtGlobalHeavyCap.Text),
            .PerTableHeavyLimit = Integer.Parse(txtPerTableHeavyLimit.Text),
            .WindowW10 = Integer.Parse(txtWindowW10.Text),
            .MaxRunPAllowed = Integer.Parse(txtMaxRunPAllowed.Text),
            .DebtTriggerRatio = Decimal.Parse(TxtDebtTriggerRatio.Text),
            .SyncDelayMs = Integer.Parse(txtSyncDelayMs.Text)
            }
            ',.Levels = ListaLivelliNumerici.ToArray()}

            Dim json_text As String = JsonSerializer.Serialize(regiaSettings, New JsonSerializerOptions With {.WriteIndented = True})

            'SCRIVO IL JSON DEI SETTINGS
            oCon = Database.GetConn()
            Using cmd As New SqlCommand("UpI_SettingsJson", oCon) ' Nome della tua Stored Procedure
                cmd.CommandType = CommandType.StoredProcedure
                SqlCommandBuilder.DeriveParameters(cmd)
                cmd.Parameters("@json").Value = json_text
                cmd.ExecuteNonQuery()
            End Using
            oCon.Close()
            oCon.Dispose()

            lblStatus.Text = "✅ Parametri applicati."

        Catch ex As Exception
            lblStatus.Text = "<span style='color:red'>Errore Apply Settings: {ex.Message}</span>"
        End Try
    End Sub
    Private Sub LoadFactory()
        TxtHmaxHigh.Text = "4"
        TxtHmaxMid.Text = "2"
        TxtHmaxLow.Text = "0"
        TxtCooldownHigh.Text = "1"
        TxtCooldownMid.Text = "1"
        TxtCooldownLow.Text = "2"
        TxtHighThresh.Text = "500"

        txtLowThresh.Text = "-2000"
        txtGlobalHeavyCap.Text = "8"
        txtPerTableHeavyLimit.Text = "3"
        txtWindowW10.Text = "10"
        txtMaxRunPAllowed.Text = "3"
        TxtDebtTriggerRatio.Text = "0.60"
        txtSyncDelayMs.Text = "100"
        txtLevels.Text = "[1,3,7,15,35,75,155,340]"
        txtHotZones.Text = "[[11,20],[41,50],[51,60],[61,70]]"
    End Sub
    Private Sub btnLoadFactory_Click(sender As Object, e As EventArgs) Handles btnLoadFactory.Click
        LoadFactory()
    End Sub


End Class
