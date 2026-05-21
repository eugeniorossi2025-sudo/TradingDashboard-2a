
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Services
Imports Telerik.Web.UI
Imports EuGenio
Imports System.Web.Script.Serialization
Imports System.Text.Json
Partial Class Edit_Default
    Inherits WebAppPageBO
    Dim _PageTitle As String = "Dashboard"
    Private totaleGeneraleMargine As Decimal = 0
    Private totaleRighe As Decimal = 0
    Private tempoTrascorso As String = ""
    Private StopWin As Decimal = 1500

    ' ============================================================
    ' 🔺 EuGenio® – Dashboard Adaptive 2.9 (Warm-Up Light 5 %)
    ' Filosofia: missione proporzionale, warm-up rapido e fluido,
    ' Vm dinamico e coerente con la Regia Astronave Adaptiva v2.8
    ' ============================================================

    ' 🎯 Parametri di missione configurabili
    Private TargetMarginUnits As Double = 1500.0      ' obiettivo totale in unità
    Private TargetMinutesTotal As Double = 540.0      ' durata missione (min)
    Private TargetTables As Integer = 10              ' riferimento proporzionale
    Private WarmUpPercent As Double = 0.05            ' 5 % del tempo missione

    ' ============================================================
    ' 🔹 Funzione principale di valutazione missione
    ' ============================================================
    Public Function Evaluate(currentMargin As Double,
                         elapsedMinutes As Double,
                         tavoliAttivi As Integer, _regiaAdaptive As ProattivoL.RegiaAdaptiva,
                         Optional K As Double = 1.0) As ProattivoL.ValutazioneRisultato

        ' 1) Ottieni lo snapshot dalla Regia (ATTENZIONE: chiama la tua istanza reale)
        Dim snap = _regiaAdaptive.GetDashboardSnapshot(
                    currentMarginUnits:=currentMargin / Math.Max(0.000001, K),
                    elapsedMinutes:=elapsedMinutes,
                    tavoliAttivi:=tavoliAttivi,
                    k:=K)

        ' 2) Aggiorna le 4 label
        LblTarget.Text = Utility.VerDecimal(snap.TargetEuro.ToString("F0")) & " €"
        LblMissionTime.Text = snap.MissionMinutesAdj.ToString("F0") & " min"
        LblSpeed.Text = Utility.VerDecimal(snap.VmTargetEuro.ToString("F2")) & " €/min"
        LblAchievement.Text = snap.AchievementPercent.ToString("F2") & "%"

        ' 3) Costruisci lo stato (ValutazioneRisultato)
        '    currentMargin è già in €, quindi lo passo diretto.
        Dim stato = _regiaAdaptive.BuildValutazione(snap, currentMarginEuro:=currentMargin, elapsedMinutes:=elapsedMinutes)

        Return stato
    End Function



    Public Function Evaluate(currentMargin As Double,
                         elapsedMinutes As Double,
                         tavoliAttivi As Integer, _regiaAdaptive As ProattivoSempliceRegiaAstronaveAdattivaPro220_1.RegiaAdaptiva,
                         Optional K As Double = 1.0) As ProattivoSempliceRegiaAstronaveAdattivaPro220_1.ValutazioneRisultato

        ' 1) Ottieni lo snapshot dalla Regia (ATTENZIONE: chiama la tua istanza reale)
        Dim snap = _regiaAdaptive.GetDashboardSnapshot(
                    currentMarginUnits:=currentMargin / Math.Max(0.000001, K),
                    elapsedMinutes:=elapsedMinutes,
                    tavoliAttivi:=tavoliAttivi,
                    k:=K)

        ' 2) Aggiorna le 4 label
        LblTarget.Text = Utility.VerDecimal(snap.TargetEuro.ToString("F0")) & " €"
        LblMissionTime.Text = snap.MissionMinutesAdj.ToString("F0") & " min"
        LblSpeed.Text = Utility.VerDecimal(snap.VmTargetEuro.ToString("F2")) & " €/min"
        LblAchievement.Text = snap.AchievementPercent.ToString("F2") & "%"

        ' 3) Costruisci lo stato (ValutazioneRisultato)
        '    currentMargin è già in €, quindi lo passo diretto.
        Dim stato = _regiaAdaptive.BuildValutazione(snap, currentMarginEuro:=currentMargin, elapsedMinutes:=elapsedMinutes)

        Return stato
    End Function
    Private Enum Azione
        Nulla = 0
        StopPc = 1
        AzzeraMartingala = 2
        StartPc = 3
    End Enum

    Private Sub chkToggle_CheckedChanged(sender As Object, e As EventArgs) Handles chkToggle.CheckedChanged
        AbilitaColonneGriglia()
    End Sub

    Private Sub BtnResetDashboard_Click(sender As Object, e As EventArgs) Handles BtnResetDashboard.Click
        Dim oCon As SqlClient.SqlConnection

        'CANCELLO LA TABELLA DEI JSON
        oCon = Database.GetConn()

        Using cmd As New SqlCommand("UpD_Values", oCon) ' Nome della tua Stored Procedure
            cmd.CommandType = CommandType.StoredProcedure
            SqlCommandBuilder.DeriveParameters(cmd)

            cmd.ExecuteNonQuery()
        End Using

        Using cmd As New SqlCommand("UpD_SafeGuardJson", oCon) ' Nome della tua Stored Procedure
            cmd.CommandType = CommandType.StoredProcedure
            SqlCommandBuilder.DeriveParameters(cmd)
            cmd.ExecuteNonQuery()
        End Using


        oCon.Close()
        oCon.Dispose()

        Response.Redirect("/Admin/Default.aspx")
    End Sub
    Private Sub BtnSaveConfiguration_Click(sender As Object, e As EventArgs) Handles BtnSaveConfiguration.Click
        SaveGrid(_PageTitle.Replace(" ", "_"), RetUserInfo("Id"), "RadGrid1", RadGrid1)
    End Sub
    Private Sub BtnStart_Click(sender As Object, e As EventArgs) Handles BtnStart.Click
        GridTimer.Enabled = True
    End Sub

    Private Sub BtnStop_Click(sender As Object, e As EventArgs) Handles BtnStop.Click
        GridTimer.Enabled = False
    End Sub
    Protected Sub GridTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        RadGrid1.Rebind()
    End Sub

#Region "API"

    <WebMethod()>
    Public Shared Function StartPC(ByVal PC As String) As String
        Try
            'Dim oCon As SqlConnection

            'oCon = Database.GetConn()

            'Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
            '    cmd.CommandType = CommandType.StoredProcedure
            '    SqlCommandBuilder.DeriveParameters(cmd)
            '    cmd.Parameters("@ID_Command").Value = Azione.StartPc
            '    cmd.Parameters("@PC").Value = PC
            '    cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

            '    cmd.ExecuteNonQuery()
            'End Using
            'oCon.Close()
            'oCon.Dispose()


            Using oCon As SqlConnection = Database.GetConn()
                Using cmd As New SqlCommand("UpI_Commands", oCon)
                    cmd.CommandType = CommandType.StoredProcedure

                    ' 🔥 Eliminato DeriveParameters (lentissimo)
                    cmd.Parameters.AddWithValue("@ID_Command", Azione.StartPc)
                    cmd.Parameters.AddWithValue("@PC", PC)
                    cmd.Parameters.AddWithValue("@ID_User", RetUserInfo("Id"))

                    ' 🔥 Apertura immediata + esecuzione istantanea
                    If oCon.State <> ConnectionState.Open Then oCon.Open()
                    cmd.ExecuteNonQuery()

                    ' 🔥 Flush immediato per evitare ritardi di scrittura
                    oCon.Close()
                End Using
            End Using


            Return "OK" ' Successo

        Catch ex As Exception
            Return "Errore: " & ex.Message
        End Try
    End Function

    <WebMethod()>
    Public Shared Function StopPC(ByVal PC As String) As String
        Try
            'Dim oCon As SqlConnection

            'oCon = Database.GetConn()

            'Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
            '    cmd.CommandType = CommandType.StoredProcedure
            '    SqlCommandBuilder.DeriveParameters(cmd)
            '    cmd.Parameters("@ID_Command").Value = Azione.StopPc
            '    cmd.Parameters("@PC").Value = PC
            '    cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

            '    cmd.ExecuteNonQuery()
            'End Using
            'oCon.Close()
            'oCon.Dispose()

            Using oCon As SqlConnection = Database.GetConn()
                Using cmd As New SqlCommand("UpI_Commands", oCon)
                    cmd.CommandType = CommandType.StoredProcedure

                    ' 🔥 Eliminato DeriveParameters
                    cmd.Parameters.AddWithValue("@ID_Command", Azione.StopPc)
                    cmd.Parameters.AddWithValue("@PC", PC)
                    cmd.Parameters.AddWithValue("@ID_User", RetUserInfo("Id"))

                    If oCon.State <> ConnectionState.Open Then oCon.Open()
                    cmd.ExecuteNonQuery()
                    oCon.Close()
                End Using
            End Using

            Return "OK" ' Successo

        Catch ex As Exception
            Return "Errore: " & ex.Message
        End Try
    End Function

    <WebMethod()>
    Public Shared Function AzzeraMartingala(ByVal PC As String) As String
        Try
            'Dim oCon As SqlConnection

            'oCon = Database.GetConn()

            'Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
            '    cmd.CommandType = CommandType.StoredProcedure
            '    SqlCommandBuilder.DeriveParameters(cmd)
            '    cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
            '    cmd.Parameters("@PC").Value = PC
            '    cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

            '    cmd.ExecuteNonQuery()
            'End Using
            'oCon.Close()
            'oCon.Dispose()

            Using oCon As SqlConnection = Database.GetConn()
                Using cmd As New SqlCommand("UpI_Commands", oCon)
                    cmd.CommandType = CommandType.StoredProcedure

                    ' 🔥 Eliminato DeriveParameters
                    cmd.Parameters.AddWithValue("@ID_Command", Azione.AzzeraMartingala)
                    cmd.Parameters.AddWithValue("@PC", PC)
                    cmd.Parameters.AddWithValue("@ID_User", RetUserInfo("Id"))

                    If oCon.State <> ConnectionState.Open Then oCon.Open()
                    cmd.ExecuteNonQuery()
                    oCon.Close()
                End Using
            End Using


            Return "OK" ' Successo

        Catch ex As Exception
            Return "Errore: " & ex.Message
        End Try
    End Function
#End Region

    Protected ReadOnly Property IsAdmin() As Boolean
        Get

            Return IIf(Utility.VerBool(RetUserInfo("Administrator")), True, False)
        End Get
    End Property

    Protected Sub RadGrid1_NeedDataSource(sender As Object, e As GridNeedDataSourceEventArgs)
        Dim oCon As SqlConnection
        Dim oCmd As SqlCommand
        Dim Ds As DataSet = New DataSet()
        'Dim Dt As DataTable = New DataTable()
        Dim Da As SqlDataAdapter = New SqlDataAdapter()

        oCon = Database.GetConn()
        oCmd = New SqlCommand()
        oCmd.Connection = oCon
        oCmd.CommandText = "ups_Dashboard"
        oCmd.CommandType = CommandType.StoredProcedure
        SqlCommandBuilder.DeriveParameters(oCmd)
        'Dt = New DataTable()
        Da.SelectCommand = oCmd
        Da.Fill(Ds)
        oCon.Close()
        oCon.Dispose()
        RadGrid1.DataSource = Ds.Tables(0)
        totaleRighe = Ds.Tables(0).Rows.Count

        'grafico 

        ' Recupera l'ora dell'ultima azione salvata
        Dim lastActionTimeObj As Object = Session("RefreshGraph")

        ' *** L'IF CHE VERIFICA IL TEMPO ***
        If lastActionTimeObj Is Nothing OrElse (DateTime.Now.Subtract(DirectCast(lastActionTimeObj, DateTime)).TotalSeconds >= 15) Then
            Session("RefreshGraph") = DateTime.Now
            RadHtmlChart1.PlotArea.XAxis.LabelsAppearance.Step = 20
            RadHtmlChart1.DataSource = Ds.Tables(1)
            RadHtmlChart1.DataBind()
            UpdatePanelChart.Update()
        End If



        Try
            totaleGeneraleMargine = Ds.Tables(0).AsEnumerable() _
                                   .Sum(Function(row) Convert.ToDecimal(row("MARGINE")))
            tempoTrascorso = Ds.Tables(0).AsEnumerable() _
                                   .Max(Function(row) row("ORE"))
        Catch ex As Exception
            totaleGeneraleMargine = 0
        End Try

        'Dim margini = Ds.Tables(1).AsEnumerable().Select(Function(r) Convert.ToDecimal(r("MARGINE"))).ToList()
        Dim margini = Ds.Tables(1).AsEnumerable().Select(Function(r) _
                  If(r.IsNull("MARGINE"),
                     CDec(0),
                     Convert.ToDecimal(r("MARGINE")))
                ).ToList()

        lblMargineAttuale.Text = totaleGeneraleMargine
        lblTempoTrascorso.Text = tempoTrascorso
        Try
            Dim margineMin As Decimal = margini.Min()
            Dim margineMax As Decimal = margini.Max()


            lblMargineMin.Text = margineMin
            lblMargineMax.Text = margineMax
        Catch ex As Exception

        End Try


    End Sub

    Private Function FormatTooltipJsonToHtml(json As String) As String
        If String.IsNullOrWhiteSpace(json) Then
            Return "Nessuna nota disponibile."
        End If

        Try
            Dim serializer As New JavaScriptSerializer()
            Dim data = serializer.Deserialize(Of Dictionary(Of String, Object))(json)
            Dim html As String = "<div style='font-size:16px; line-height:1.4em; padding:5px; max-width:300px;'>"

            For Each kv In data
                html &= "<b>" & kv.Key & "</b><br/>"

                ' Ogni voce è un dizionario con logic/algebra
                Dim inner = TryCast(kv.Value, Dictionary(Of String, Object))
                If inner IsNot Nothing Then
                    html &= "<span style='color:#3366cc;'>" & inner("logic") & "</span><br/>"
                    html &= "<span style='color:#444;'>" & inner("algebra") & "</span><hr style='border:0;border-top:1px solid #ccc;'/>"
                Else
                    html &= "<span>{kv.Value}</span><hr/>"
                End If
            Next

            html &= "</div>"
            Return html
        Catch ex As Exception
            Return "Errore formattando JSON: {ex.Message}"
        End Try
    End Function

    Protected Sub RadGrid1_ItemCommand(sender As Object, e As Telerik.Web.UI.GridCommandEventArgs) Handles RadGrid1.ItemCommand
        If e.CommandName = "ShowTooltip" Then
            Dim dataItem As Telerik.Web.UI.GridDataItem = CType(e.Item, Telerik.Web.UI.GridDataItem)
            Dim account As String = e.CommandArgument.ToString()


            ' Converte il JSON in HTML leggibile
            Dim htmlText As String = FormatTooltipJsonToHtml(dataItem("NOTE").Text)

            ' Mostra il RadWindow come popup
            RadWindow1.Title = "📘 Dettaglio decisione - " & account
            RadWindow1.ContentTemplate = Nothing
            RadWindow1.NavigateUrl = Nothing
            RadWindow1.VisibleOnPageLoad = True
            RadWindow1.Modal = True
            RadWindow1.Height = 400
            RadWindow1.Width = 450
            RadWindow1.Behaviors = Telerik.Web.UI.WindowBehaviors.Close

            ' Mostra il tooltip via script (RadWindow)
            Dim safeHtml As String = htmlText.Replace("""", "\""").Replace(vbCrLf, " ").Replace(vbLf, " ")
            Dim script As String = "radalert(""" & safeHtml & """, 400, 550, '📘 Dettaglio');"
            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "ShowNote", script, True)
        End If
    End Sub

    Protected Sub RadGrid1_ItemDataBound(ByVal sender As Object, ByVal e As Telerik.Web.UI.GridItemEventArgs) Handles RadGrid1.ItemDataBound
        If TypeOf e.Item Is GridDataItem Then
            Dim dataItem As GridDataItem = CType(e.Item, GridDataItem)
            Dim oCon As SqlConnection
            'Prendo il valore del campo COLORE dalla riga corrente
            Dim classeColore As String = DataBinder.Eval(dataItem.DataItem, "COLORE").ToString()
            Dim classeAllarme As String = DataBinder.Eval(dataItem.DataItem, "ALLARME").ToString()
            Dim valoreGiocato As Decimal = Utility.VerDecimal(DataBinder.Eval(dataItem.DataItem, "VALORE_GIOCATO").ToString())
            Dim P_B As String = Utility.VerString(DataBinder.Eval(dataItem.DataItem, "PBT").ToString())

            If P_B = "" Then P_B = " "

            'Assegno la classe CSS alla cella della colonna COLORE

            If classeColore = "ROSSO" Then
                If totaleGeneraleMargine >= 0 Then
                    classeColore = "GIALLO"
                End If
            End If
            dataItem("COLORE").CssClass = classeColore
            dataItem("COLORE").Text = ""

            If Utility.VerInt32(DataBinder.Eval(dataItem.DataItem, "MAZZO")) > 72 Then
                dataItem("MAZZO").CssClass = "ROSSO"
            End If

            If Utility.VerInt32(DataBinder.Eval(dataItem.DataItem, "MINUTI_PASSATI")) >= Utility.VerInt32(RetConfiguration("CHECK_PC_OFF")) Then
                dataItem("MINUTI_PASSATI").CssClass = "ROSSO"
            End If

            '***************************

            Dim item As Telerik.Web.UI.GridDataItem = CType(e.Item, Telerik.Web.UI.GridDataItem)
            Dim litStatoIcona As Literal = CType(item.FindControl("litStatoIcona"), Literal)

            If litStatoIcona IsNot Nothing Then
                Dim valoreString As String = DataBinder.Eval(dataItem.DataItem, "VINCITA").ToString()
                Dim valoreNumerico As Integer

                If Integer.TryParse(valoreString, valoreNumerico) Then
                    If valoreNumerico = 1 Then
                        litStatoIcona.Text = "<span style='color: green; font-size: 1.2em;'>▲</span>"
                    ElseIf valoreNumerico = -1 Then
                        litStatoIcona.Text = "<span style='color: red; font-size: 1.2em;'>▼</span>"
                    End If
                End If
            End If

            ' Ad ogni aggiornamento:
            Dim margine As Decimal = Utility.VerDecimal(DataBinder.Eval(dataItem.DataItem, "MARGINE"))    ' “MARGINE” visualizzato a tabella (saldoNow - saldoInit)
            Dim manoCorrente As Integer = Utility.VerInt32(DataBinder.Eval(dataItem.DataItem, "MAZZO"))   ' mano 1..60
            Dim tavolo As Integer = Utility.VerInt32(DataBinder.Eval(dataItem.DataItem, "TAVOLO"))
            Dim livello As Integer = Utility.VerInt32(DataBinder.Eval(dataItem.DataItem, "COLPO_MARTINGALA"))      ' livello martingala attuale (1..10)
            Dim latoBanker As Boolean = True         ' stai giocando Banker? (per la commissione)

            Dim timeString As String = DataBinder.Eval(dataItem.DataItem, "ORE")
            Dim timeSpan As TimeSpan = TimeSpan.Parse(timeString)
            Dim totalMinutes As Double = timeSpan.TotalMinutes


            Select Case RetConfiguration("DECISION_METHOD").ToUpper
                Case "E", "F", "G", "H", "I", "L"
                    StopWin = Utility.VerDecimal(LblTarget.Text.Replace("€", "").Trim)
                Case Else
                    StopWin = Utility.VerDecimal(RetConfiguration("STOPWIN"))
            End Select


            If totaleGeneraleMargine >= StopWin And StopWin > 0 Then 'mando uno stop

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    oCon = Database.GetConn()

                    Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                        cmd.CommandType = CommandType.StoredProcedure
                        SqlCommandBuilder.DeriveParameters(cmd)
                        cmd.Parameters("@ID_Command").Value = Azione.StopPc
                        cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                        cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                        cmd.ExecuteNonQuery()
                    End Using
                    oCon.Close()
                    oCon.Dispose()
                End If
                LblMissionCompleted.Visible = True
                Exit Sub
            End If
            'L 
            If RetConfiguration("DECISION_METHOD").ToUpper = "L" Then
                Dim json_text As String
                Dim settings As ProattivoL.ProactiveSettings
                Dim engine As ProattivoL.ProactiveEngine
                Dim fullText As String
                Dim adv
                Dim regiaAdaptive As ProattivoL.RegiaAdaptiva
                Dim coordination As New ProattivoL.CoordinationLayer()
                Dim valRis As ProattivoL.ValutazioneRisultato
                oCon = Database.GetConn()

                'leggo i settings dal database
                Using cmd As New SqlCommand("UpS_SettingsJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                settings = JsonSerializer.Deserialize(Of ProattivoL.ProactiveSettings)(json_text)
                engine = New ProattivoL.ProactiveEngine(settings)
                engine.SetK(1.0 / Utility.VerDecimal(RetConfiguration("BALANCE_DIVIDER")))
                'leggo l'ultimo valore del motore per ripassarglielo
                Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                engine.LoadStateFromJson(json_text)


                regiaAdaptive = engine.GetRegiaAdaptive()

                valRis = Evaluate(margine, totalMinutes, totaleRighe, regiaAdaptive, 1.0 / Utility.VerDecimal(RetConfiguration("BALANCE_DIVIDER")))

                dataItem("VALUTAZIONE").Text = valRis.Message & "<BR/>" & valRis.Color & "<BR/>" & Math.Round(valRis.VmValue, 2).ToString()


                fullText = json_text

                If Not String.IsNullOrEmpty(fullText) Then
                    Dim maxLen As Integer = 50
                    Dim shortText As String = If(fullText.Length > maxLen, fullText.Substring(0, maxLen) & "...", fullText)

                    ' 🔹 Crea HTML con attributo data-fulltext
                    Dim htmljson As String = String.Format("<span class='copyCell' data-fulltext=""{0}"" title=""{0}"">{1}</span>", Server.HtmlEncode(fullText), "..click to copy..")

                    dataItem("JSON").Text = htmljson
                End If


                adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False), P_B(0), totalMinutes, totaleRighe)

                Dim info = coordination.ApplyCoordinatedAdjustments(settings,
                                                    engine.GetRows(),
                                                    totaleRighe,
                                                    totalMinutes,
                                                    engine.GetGlobalMarginUnits())


                LblNote.Text = "SevereTables: " & info.SevereTables & "<br/>" &
               "SimulHeavyCandidates: " & info.SimulHeavyCandidates & "<br/>" &
               "TablesStressed: " & info.TablesStressed & "<br/>" &
               "Vm: " & info.Vm & "<br/>" &
               "VmLocal20Avg: " & info.VmLocal20Avg & "<br/>" &
               "VmLocal20PositiveRate: " & info.VmLocal20PositiveRate & "<br/>" &
               "VmTarget: " & info.VmTarget & "<br/>" &
               "VmValveTriggered: " & info.VmValveTriggered
                'salvataggio della struttura json
                json_text = engine.GetJson

                Using cmd As New SqlCommand("UpI_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    cmd.Parameters("@json").Value = json_text
                    cmd.Parameters("@margine").Value = totaleGeneraleMargine.ToString()
                    cmd.ExecuteNonQuery()
                End Using
                oCon.Close()
                oCon.Dispose()

                'Ritorno dei valori
                dataItem("LEVELINDEX").Text = adv.LevelIndex
                dataItem("STAKEUNIT").Text = adv.StakeUnits
                dataItem("STOPATL5").Text = adv.StopAtL5
                dataItem("FUTUREL5PRED").Text = adv.FutureL5Prediction
                dataItem("AUTHORIZERHEAVY").Text = adv.AuthorizedHeavy
                dataItem("REASON").Text = adv.Reason
                'dataItem("NOTE").Text = adv.GlobalMargin
                'dataItem("SIGNALW10").Text = adv.SignalW10
                dataItem("HOTZONE").Text = adv.HotZone & "<br/>" & adv.HotZoneLabel
                dataItem("NOTE").Text = adv.TooltipJson  'I
                dataItem("VMLOCAL20").Text = adv.VmLocal20
                dataItem("PREDICTION").Text = adv.Prediction
                If adv.StopAtL5 Then
                    dataItem("STOPATL5").CssClass = "ROSSO"
                Else
                    dataItem("STOPATL5").CssClass = "VERDE"
                End If

                If adv.AuthorizedHeavy Then
                    dataItem("AUTHORIZERHEAVY").CssClass = "VERDE"
                Else
                    dataItem("AUTHORIZERHEAVY").CssClass = "ROSSO"
                End If

                Dim history = engine.GetHistory(tavolo)

                ' Converte in HTML colorato
                Dim html As String = ""
                For Each h As Char In history
                    Select Case h
                        Case "B"c
                            html &= "<span style='color:red;font-weight:bold;font-size:12px'>B</span> "
                        Case "P"c
                            html &= "<span style='color:blue;font-weight:bold;font-size:12px'>P</span> "
                        Case "T"c
                            html &= "<span style='color:green;font-weight:bold;font-size:12px'>T</span> "
                        Case Else
                            html &= "<span style='color:gray;font-size:12px'>" & h & "</span> "
                    End Select
                Next

                ' Inserisce l'HTML nella cella
                Dim litW10 As Literal = CType(item.FindControl("litW10"), Literal)
                litW10.Text = html.Trim()

                Dim litNote As Literal = CType(item.FindControl("litNote"), Literal)
                litNote.Text = adv.SignalW10

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    'If (adv.StopAtL5 And adv.Reason.ToString().ToUpper().Contains("STOP-WIN")) Then 'se true allora va in STOP
                    '    'INVIO LO STOP
                    '    oCon = Database.GetConn()

                    '    Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                    '        cmd.CommandType = CommandType.StoredProcedure
                    '        SqlCommandBuilder.DeriveParameters(cmd)
                    '        cmd.Parameters("@ID_Command").Value = Azione.StopPc
                    '        cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                    '        cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                    '        cmd.ExecuteNonQuery()
                    '    End Using
                    '    oCon.Close()
                    '    oCon.Dispose()
                    'Else

                    If (adv.StopAtL5) Then 'se true allora va in pause scalping 
                        'INVIO IL SAFE 
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    End If
                End If
            End If




            'I - PROATTIVO SEMPLICE REGIA ASTRONAVE ADATTIVA PRO FIXED CON 20 CENT
            If RetConfiguration("DECISION_METHOD").ToUpper = "I" Then
                Dim json_text As String
                Dim settings As ProattivoSempliceRegiaAstronaveAdattivaPro220_1.ProactiveSettings
                Dim engine As ProattivoSempliceRegiaAstronaveAdattivaPro220_1.ProactiveEngine
                Dim fullText As String
                Dim adv
                Dim regiaAdaptive As ProattivoSempliceRegiaAstronaveAdattivaPro220_1.RegiaAdaptiva
                Dim coordination As New ProattivoSempliceRegiaAstronaveAdattivaPro220_1.CoordinationLayer()
                Dim valRis As ProattivoSempliceRegiaAstronaveAdattivaPro220_1.ValutazioneRisultato
                oCon = Database.GetConn()

                'leggo i settings dal database
                Using cmd As New SqlCommand("UpS_SettingsJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                settings = JsonSerializer.Deserialize(Of ProattivoSempliceRegiaAstronaveAdattivaPro220_1.ProactiveSettings)(json_text)
                engine = New ProattivoSempliceRegiaAstronaveAdattivaPro220_1.ProactiveEngine(settings)
                engine.SetK(1.0 / Utility.VerDecimal(RetConfiguration("BALANCE_DIVIDER")))
                'leggo l'ultimo valore del motore per ripassarglielo
                Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                engine.LoadStateFromJson(json_text)


                regiaAdaptive = engine.GetRegiaAdaptive()

                valRis = Evaluate(margine, totalMinutes, totaleRighe, regiaAdaptive, 1.0 / Utility.VerDecimal(RetConfiguration("BALANCE_DIVIDER")))

                dataItem("VALUTAZIONE").Text = valRis.Message & "<BR/>" & valRis.Color & "<BR/>" & Math.Round(valRis.VmValue, 2).ToString()


                fullText = json_text

                If Not String.IsNullOrEmpty(fullText) Then
                    Dim maxLen As Integer = 50
                    Dim shortText As String = If(fullText.Length > maxLen, fullText.Substring(0, maxLen) & "...", fullText)

                    ' 🔹 Crea HTML con attributo data-fulltext
                    Dim htmljson As String = String.Format("<span class='copyCell' data-fulltext=""{0}"" title=""{0}"">{1}</span>", Server.HtmlEncode(fullText), "..click to copy..")

                    dataItem("JSON").Text = htmljson
                End If


                adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False), P_B(0), totalMinutes, totaleRighe)

                Dim info = coordination.ApplyCoordinatedAdjustments(settings,
                                                    engine.GetRows(),
                                                    totaleRighe,
                                                    totalMinutes,
                                                    engine.GetGlobalMarginUnits())


                LblNote.Text = "SevereTables: " & info.SevereTables & "<br/>" &
               "SimulHeavyCandidates: " & info.SimulHeavyCandidates & "<br/>" &
               "TablesStressed: " & info.TablesStressed & "<br/>" &
               "Vm: " & info.Vm & "<br/>" &
               "VmLocal20Avg: " & info.VmLocal20Avg & "<br/>" &
               "VmLocal20PositiveRate: " & info.VmLocal20PositiveRate & "<br/>" &
               "VmTarget: " & info.VmTarget & "<br/>" &
               "VmValveTriggered: " & info.VmValveTriggered
                'salvataggio della struttura json
                json_text = engine.GetJson

                Using cmd As New SqlCommand("UpI_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    cmd.Parameters("@json").Value = json_text
                    cmd.Parameters("@margine").Value = totaleGeneraleMargine.ToString()
                    cmd.ExecuteNonQuery()
                End Using
                oCon.Close()
                oCon.Dispose()

                'Ritorno dei valori
                dataItem("LEVELINDEX").Text = adv.LevelIndex
                dataItem("STAKEUNIT").Text = adv.StakeUnits
                dataItem("STOPATL5").Text = adv.StopAtL5
                dataItem("AUTHORIZERHEAVY").Text = adv.AuthorizedHeavy
                dataItem("REASON").Text = adv.Reason
                'dataItem("NOTE").Text = adv.GlobalMargin
                'dataItem("SIGNALW10").Text = adv.SignalW10
                dataItem("HOTZONE").Text = adv.HotZone & "<br/>" & adv.HotZoneLabel
                dataItem("NOTE").Text = adv.TooltipJson  'I
                dataItem("VMLOCAL20").Text = adv.VmLocal20
                dataItem("PREDICTION").Text = adv.Prediction
                If adv.StopAtL5 Then
                    dataItem("STOPATL5").CssClass = "ROSSO"
                Else
                    dataItem("STOPATL5").CssClass = "VERDE"
                End If

                If adv.AuthorizedHeavy Then
                    dataItem("AUTHORIZERHEAVY").CssClass = "VERDE"
                Else
                    dataItem("AUTHORIZERHEAVY").CssClass = "ROSSO"
                End If

                Dim history = engine.GetHistory(tavolo)

                ' Converte in HTML colorato
                Dim html As String = ""
                For Each h As Char In history
                    Select Case h
                        Case "B"c
                            html &= "<span style='color:red;font-weight:bold;font-size:12px'>B</span> "
                        Case "P"c
                            html &= "<span style='color:blue;font-weight:bold;font-size:12px'>P</span> "
                        Case "T"c
                            html &= "<span style='color:green;font-weight:bold;font-size:12px'>T</span> "
                        Case Else
                            html &= "<span style='color:gray;font-size:12px'>" & h & "</span> "
                    End Select
                Next

                ' Inserisce l'HTML nella cella
                Dim litW10 As Literal = CType(item.FindControl("litW10"), Literal)
                litW10.Text = html.Trim()

                Dim litNote As Literal = CType(item.FindControl("litNote"), Literal)
                litNote.Text = adv.SignalW10

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    'If (adv.StopAtL5 And adv.Reason.ToString().ToUpper().Contains("STOP-WIN")) Then 'se true allora va in STOP
                    '    'INVIO LO STOP
                    '    oCon = Database.GetConn()

                    '    Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                    '        cmd.CommandType = CommandType.StoredProcedure
                    '        SqlCommandBuilder.DeriveParameters(cmd)
                    '        cmd.Parameters("@ID_Command").Value = Azione.StopPc
                    '        cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                    '        cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                    '        cmd.ExecuteNonQuery()
                    '    End Using
                    '    oCon.Close()
                    '    oCon.Dispose()
                    'Else

                    If (adv.StopAtL5) Then 'se true allora va in pause scalping 
                            'INVIO IL SAFE 
                            oCon = Database.GetConn()

                            Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                                cmd.CommandType = CommandType.StoredProcedure
                                SqlCommandBuilder.DeriveParameters(cmd)
                                cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                                cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                                cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                                cmd.ExecuteNonQuery()
                            End Using
                            oCon.Close()
                            oCon.Dispose()
                        End If
                    End If
            End If


            'H - PROATTIVO SEMPLICE REGIA ASTRONAVE ADATTIVA PRO FIXED
            If RetConfiguration("DECISION_METHOD").ToUpper = "H" Then
                Dim json_text As String
                Dim settings As ProattivoSempliceRegiaAstronaveAdattivaPro2.ProactiveSettings
                Dim engine As ProattivoSempliceRegiaAstronaveAdattivaPro2.ProactiveEngine
                Dim fullText As String
                Dim adv

                oCon = Database.GetConn()

                'leggo i settings dal database
                Using cmd As New SqlCommand("UpS_SettingsJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                settings = JsonSerializer.Deserialize(Of ProattivoSempliceRegiaAstronaveAdattivaPro2.ProactiveSettings)(json_text)
                engine = New ProattivoSempliceRegiaAstronaveAdattivaPro2.ProactiveEngine(settings)

                'leggo l'ultimo valore del motore per ripassarglielo
                Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                engine.LoadStateFromJson(json_text)


                fullText = json_text

                If Not String.IsNullOrEmpty(fullText) Then
                    Dim maxLen As Integer = 50
                    Dim shortText As String = If(fullText.Length > maxLen, fullText.Substring(0, maxLen) & "...", fullText)

                    ' 🔹 Crea HTML con attributo data-fulltext
                    Dim htmljson As String = String.Format("<span class='copyCell' data-fulltext=""{0}"" title=""{0}"">{1}</span>", Server.HtmlEncode(fullText), "..click to copy..")

                    dataItem("JSON").Text = htmljson
                End If



                'If RetConfiguration("SEND_P_B") = "1" Then
                adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False), P_B(0), totalMinutes, totaleRighe)
                'Else
                '    adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False))
                'End If
                'salvataggio della struttura json
                json_text = engine.GetJson

                Using cmd As New SqlCommand("UpI_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    cmd.Parameters("@json").Value = json_text
                    cmd.Parameters("@margine").Value = totaleGeneraleMargine.ToString()
                    cmd.ExecuteNonQuery()
                End Using
                oCon.Close()
                oCon.Dispose()

                'Ritorno dei valori
                dataItem("LEVELINDEX").Text = adv.LevelIndex
                dataItem("STAKEUNIT").Text = adv.StakeUnits
                dataItem("STOPATL5").Text = adv.StopAtL5
                dataItem("AUTHORIZERHEAVY").Text = adv.AuthorizedHeavy
                dataItem("REASON").Text = adv.Reason
                'dataItem("NOTE").Text = adv.GlobalMargin
                'dataItem("SIGNALW10").Text = adv.SignalW10
                dataItem("HOTZONE").Text = adv.HotZone & "<br/>" & adv.HotZoneLabel
                dataItem("NOTE").Text = adv.TooltipJson  'D

                If adv.StopAtL5 Then
                    dataItem("STOPATL5").CssClass = "ROSSO"
                Else
                    dataItem("STOPATL5").CssClass = "VERDE"
                End If

                If adv.AuthorizedHeavy Then
                    dataItem("AUTHORIZERHEAVY").CssClass = "VERDE"
                Else
                    dataItem("AUTHORIZERHEAVY").CssClass = "ROSSO"
                End If

                Dim history = engine.GetHistory(tavolo)

                ' Converte in HTML colorato
                Dim html As String = ""
                For Each h As Char In history
                    Select Case h
                        Case "B"c
                            html &= "<span style='color:red;font-weight:bold;font-size:12px'>B</span> "
                        Case "P"c
                            html &= "<span style='color:blue;font-weight:bold;font-size:12px'>P</span> "
                        Case "T"c
                            html &= "<span style='color:green;font-weight:bold;font-size:12px'>T</span> "
                        Case Else
                            html &= "<span style='color:gray;font-size:12px'>" & h & "</span> "
                    End Select
                Next

                ' Inserisce l'HTML nella cella
                Dim litW10 As Literal = CType(item.FindControl("litW10"), Literal)
                litW10.Text = html.Trim()

                Dim litNote As Literal = CType(item.FindControl("litNote"), Literal)
                litNote.Text = adv.SignalW10

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    If (adv.StopAtL5 And adv.Reason.ToString().ToUpper().Contains("STOP-WIN")) Then 'se true allora va in STOP
                        'INVIO LO STOP
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.StopPc
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    ElseIf (adv.StopAtL5) Then 'se true allora va in pause scalping 
                        'INVIO IL SAFE 
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    End If
                End If
            End If

            'G - PROATTIVO SEMPLICE REGIA ASTRONAVE ADATTIVA PRO
            If RetConfiguration("DECISION_METHOD").ToUpper = "G" Then
                Dim json_text As String
                Dim settings As ProattivoSempliceRegiaAstronaveAdattivaPro.ProactiveSettings
                Dim engine As ProattivoSempliceRegiaAstronaveAdattivaPro.ProactiveEngine
                Dim fullText As String
                Dim adv

                oCon = Database.GetConn()

                'leggo i settings dal database
                Using cmd As New SqlCommand("UpS_SettingsJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                settings = JsonSerializer.Deserialize(Of ProattivoSempliceRegiaAstronaveAdattivaPro.ProactiveSettings)(json_text)
                engine = New ProattivoSempliceRegiaAstronaveAdattivaPro.ProactiveEngine(settings)

                'leggo l'ultimo valore del motore per ripassarglielo
                Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                engine.LoadStateFromJson(json_text)


                fullText = json_text

                If Not String.IsNullOrEmpty(fullText) Then
                    Dim maxLen As Integer = 50
                    Dim shortText As String = If(fullText.Length > maxLen, fullText.Substring(0, maxLen) & "...", fullText)

                    ' 🔹 Crea HTML con attributo data-fulltext
                    Dim htmljson As String = String.Format("<span class='copyCell' data-fulltext=""{0}"" title=""{0}"">{1}</span>", Server.HtmlEncode(fullText), "..click to copy..")

                    dataItem("JSON").Text = htmljson
                End If



                'If RetConfiguration("SEND_P_B") = "1" Then
                adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False), P_B(0), totalMinutes, totaleRighe)
                'Else
                '    adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False))
                'End If
                'salvataggio della struttura json
                json_text = engine.GetJson

                Using cmd As New SqlCommand("UpI_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    cmd.Parameters("@json").Value = json_text
                    cmd.Parameters("@margine").Value = totaleGeneraleMargine.ToString()
                    cmd.ExecuteNonQuery()
                End Using
                oCon.Close()
                oCon.Dispose()

                'Ritorno dei valori
                dataItem("LEVELINDEX").Text = adv.LevelIndex
                dataItem("STAKEUNIT").Text = adv.StakeUnits
                dataItem("STOPATL5").Text = adv.StopAtL5
                dataItem("AUTHORIZERHEAVY").Text = adv.AuthorizedHeavy
                dataItem("REASON").Text = adv.Reason
                'dataItem("NOTE").Text = adv.GlobalMargin
                'dataItem("SIGNALW10").Text = adv.SignalW10
                dataItem("HOTZONE").Text = adv.HotZone & "<br/>" & adv.HotZoneLabel
                dataItem("NOTE").Text = adv.TooltipJson  'D

                If adv.StopAtL5 Then
                    dataItem("STOPATL5").CssClass = "ROSSO"
                Else
                    dataItem("STOPATL5").CssClass = "VERDE"
                End If

                If adv.AuthorizedHeavy Then
                    dataItem("AUTHORIZERHEAVY").CssClass = "VERDE"
                Else
                    dataItem("AUTHORIZERHEAVY").CssClass = "ROSSO"
                End If

                Dim history = engine.GetHistory(tavolo)

                ' Converte in HTML colorato
                Dim html As String = ""
                For Each h As Char In history
                    Select Case h
                        Case "B"c
                            html &= "<span style='color:red;font-weight:bold;font-size:12px'>B</span> "
                        Case "P"c
                            html &= "<span style='color:blue;font-weight:bold;font-size:12px'>P</span> "
                        Case "T"c
                            html &= "<span style='color:green;font-weight:bold;font-size:12px'>T</span> "
                        Case Else
                            html &= "<span style='color:gray;font-size:12px'>" & h & "</span> "
                    End Select
                Next

                ' Inserisce l'HTML nella cella
                Dim litW10 As Literal = CType(item.FindControl("litW10"), Literal)
                litW10.Text = html.Trim()

                Dim litNote As Literal = CType(item.FindControl("litNote"), Literal)
                litNote.Text = adv.SignalW10

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    If (adv.StopAtL5 And adv.Reason.ToString().ToUpper().Contains("STOP-WIN")) Then 'se true allora va in STOP
                        'INVIO LO STOP
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.StopPc
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    ElseIf (adv.StopAtL5) Then 'se true allora va in pause scalping 
                        'INVIO IL SAFE 
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    End If
                End If
            End If

            'F - PROATTIVO SEMPLICE REGIA ASTRONAVE ADATTIVA
            If RetConfiguration("DECISION_METHOD").ToUpper = "F" Then
                Dim json_text As String
                Dim settings As ProattivoSempliceRegiaAstronaveAdattiva.ProactiveSettings
                Dim engine As ProattivoSempliceRegiaAstronaveAdattiva.ProactiveEngine
                Dim fullText As String
                Dim adv

                oCon = Database.GetConn()

                'leggo i settings dal database
                Using cmd As New SqlCommand("UpS_SettingsJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                settings = JsonSerializer.Deserialize(Of ProattivoSempliceRegiaAstronaveAdattiva.ProactiveSettings)(json_text)
                engine = New ProattivoSempliceRegiaAstronaveAdattiva.ProactiveEngine(settings)

                'leggo l'ultimo valore del motore per ripassarglielo
                Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                engine.LoadStateFromJson(json_text)


                fullText = json_text

                If Not String.IsNullOrEmpty(fullText) Then
                    Dim maxLen As Integer = 50
                    Dim shortText As String = If(fullText.Length > maxLen, fullText.Substring(0, maxLen) & "...", fullText)

                    ' 🔹 Crea HTML con attributo data-fulltext
                    Dim htmljson As String = String.Format("<span class='copyCell' data-fulltext=""{0}"" title=""{0}"">{1}</span>", Server.HtmlEncode(fullText), "..click to copy..")

                    dataItem("JSON").Text = htmljson
                End If



                'If RetConfiguration("SEND_P_B") = "1" Then
                adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False), P_B(0), totalMinutes, totaleRighe)
                'Else
                '    adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False))
                'End If
                'salvataggio della struttura json
                json_text = engine.GetJson

                Using cmd As New SqlCommand("UpI_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    cmd.Parameters("@json").Value = json_text
                    cmd.Parameters("@margine").Value = totaleGeneraleMargine.ToString()
                    cmd.ExecuteNonQuery()
                End Using
                oCon.Close()
                oCon.Dispose()

                'Ritorno dei valori
                dataItem("LEVELINDEX").Text = adv.LevelIndex
                dataItem("STAKEUNIT").Text = adv.StakeUnits
                dataItem("STOPATL5").Text = adv.StopAtL5
                dataItem("AUTHORIZERHEAVY").Text = adv.AuthorizedHeavy
                dataItem("REASON").Text = adv.Reason
                'dataItem("NOTE").Text = adv.GlobalMargin
                'dataItem("SIGNALW10").Text = adv.SignalW10
                dataItem("HOTZONE").Text = adv.HotZone & "<br/>" & adv.HotZoneLabel
                dataItem("NOTE").Text = adv.TooltipJson  'D

                If adv.StopAtL5 Then
                    dataItem("STOPATL5").CssClass = "ROSSO"
                Else
                    dataItem("STOPATL5").CssClass = "VERDE"
                End If

                If adv.AuthorizedHeavy Then
                    dataItem("AUTHORIZERHEAVY").CssClass = "VERDE"
                Else
                    dataItem("AUTHORIZERHEAVY").CssClass = "ROSSO"
                End If

                Dim history = engine.GetHistory(tavolo)

                ' Converte in HTML colorato
                Dim html As String = ""
                For Each h As Char In history
                    Select Case h
                        Case "B"c
                            html &= "<span style='color:red;font-weight:bold;font-size:12px'>B</span> "
                        Case "P"c
                            html &= "<span style='color:blue;font-weight:bold;font-size:12px'>P</span> "
                        Case "T"c
                            html &= "<span style='color:green;font-weight:bold;font-size:12px'>T</span> "
                        Case Else
                            html &= "<span style='color:gray;font-size:12px'>" & h & "</span> "
                    End Select
                Next

                ' Inserisce l'HTML nella cella
                Dim litW10 As Literal = CType(item.FindControl("litW10"), Literal)
                litW10.Text = html.Trim()

                Dim litNote As Literal = CType(item.FindControl("litNote"), Literal)
                litNote.Text = adv.SignalW10

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    If (adv.StopAtL5 And adv.Reason.ToString().ToUpper().Contains("STOP-WIN")) Then 'se true allora va in STOP
                        'INVIO LO STOP
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.StopPc
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    ElseIf (adv.StopAtL5) Then 'se true allora va in pause scalping 
                        'INVIO IL SAFE 
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    End If
                End If
            End If


            'E - PROATTIVO SEMPLICE REGIA ASTRONAVE
            If RetConfiguration("DECISION_METHOD").ToUpper = "E" Then
                Dim json_text As String
                Dim settings As ProattivoSempliceRegiaAstronave.ProactiveSettings
                Dim engine As ProattivoSempliceRegiaAstronave.ProactiveEngine
                Dim fullText As String
                Dim adv

                oCon = Database.GetConn()

                'leggo i settings dal database
                Using cmd As New SqlCommand("UpS_SettingsJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                settings = JsonSerializer.Deserialize(Of ProattivoSempliceRegiaAstronave.ProactiveSettings)(json_text)
                engine = New ProattivoSempliceRegiaAstronave.ProactiveEngine(settings)

                'leggo l'ultimo valore del motore per ripassarglielo
                Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                engine.LoadStateFromJson(json_text)


                fullText = json_text

                If Not String.IsNullOrEmpty(fullText) Then
                    Dim maxLen As Integer = 50
                    Dim shortText As String = If(fullText.Length > maxLen, fullText.Substring(0, maxLen) & "...", fullText)

                    ' 🔹 Crea HTML con attributo data-fulltext
                    Dim htmljson As String = String.Format("<span class='copyCell' data-fulltext=""{0}"" title=""{0}"">{1}</span>", Server.HtmlEncode(fullText), "..click to copy..")

                    dataItem("JSON").Text = htmljson
                End If

                'If RetConfiguration("SEND_P_B") = "1" Then
                adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False), P_B(0))
                'Else
                '    adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False))
                'End If
                'salvataggio della struttura json
                json_text = engine.GetJson

                Using cmd As New SqlCommand("UpI_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    cmd.Parameters("@json").Value = json_text
                    cmd.Parameters("@margine").Value = totaleGeneraleMargine.ToString()
                    cmd.ExecuteNonQuery()
                End Using
                oCon.Close()
                oCon.Dispose()

                'Ritorno dei valori
                dataItem("LEVELINDEX").Text = adv.LevelIndex
                dataItem("STAKEUNIT").Text = adv.StakeUnits
                dataItem("STOPATL5").Text = adv.StopAtL5
                dataItem("AUTHORIZERHEAVY").Text = adv.AuthorizedHeavy
                dataItem("REASON").Text = adv.Reason
                'dataItem("NOTE").Text = adv.GlobalMargin
                'dataItem("SIGNALW10").Text = adv.SignalW10
                dataItem("HOTZONE").Text = adv.HotZone & "<br/>" & adv.HotZoneLabel
                dataItem("NOTE").Text = adv.TooltipJson  'D

                If adv.StopAtL5 Then
                    dataItem("STOPATL5").CssClass = "ROSSO"
                Else
                    dataItem("STOPATL5").CssClass = "VERDE"
                End If

                If adv.AuthorizedHeavy Then
                    dataItem("AUTHORIZERHEAVY").CssClass = "VERDE"
                Else
                    dataItem("AUTHORIZERHEAVY").CssClass = "ROSSO"
                End If

                Dim history = engine.GetHistory(tavolo)

                ' Converte in HTML colorato
                Dim html As String = ""
                For Each h As Char In history
                    Select Case h
                        Case "B"c
                            html &= "<span style='color:red;font-weight:bold;font-size:12px'>B</span> "
                        Case "P"c
                            html &= "<span style='color:blue;font-weight:bold;font-size:12px'>P</span> "
                        Case "T"c
                            html &= "<span style='color:green;font-weight:bold;font-size:12px'>T</span> "
                        Case Else
                            html &= "<span style='color:gray;font-size:12px'>" & h & "</span> "
                    End Select
                Next

                ' Inserisce l'HTML nella cella
                Dim litW10 As Literal = CType(item.FindControl("litW10"), Literal)
                litW10.Text = html.Trim()

                Dim litNote As Literal = CType(item.FindControl("litNote"), Literal)
                litNote.Text = adv.SignalW10

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    If (adv.StopAtL5) Then 'se true allora va in pause scalping 
                        'INVIO IL SAFE 
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    End If
                End If
            End If


            'D - PROATTIVO SEMPLICE REGIA  
            If RetConfiguration("DECISION_METHOD").ToUpper = "D" Then
                Dim json_text As String
                Dim settings As ProattivoSempliceRegia.ProactiveSettings = New ProattivoSempliceRegia.ProactiveSettings()
                Dim engine As ProattivoSempliceRegia.ProactiveEngine = New ProattivoSempliceRegia.ProactiveEngine(settings)
                Dim fullText As String
                Dim adv

                oCon = Database.GetConn()



                Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                engine.LoadStateFromJson(json_text)


                fullText = json_text

                If Not String.IsNullOrEmpty(fullText) Then
                    Dim maxLen As Integer = 50
                    Dim shortText As String = If(fullText.Length > maxLen, fullText.Substring(0, maxLen) & "...", fullText)

                    ' 🔹 Crea HTML con attributo data-fulltext
                    Dim htmljson As String = String.Format("<span class='copyCell' data-fulltext=""{0}"" title=""{0}"">{1}</span>", Server.HtmlEncode(fullText), "..click to copy..")

                    dataItem("JSON").Text = htmljson
                End If

                'If RetConfiguration("SEND_P_B") = "1" Then
                adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False), P_B(0))
                'Else
                '    adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False))
                'End If
                'salvataggio della struttura json
                json_text = engine.GetJson

                Using cmd As New SqlCommand("UpI_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    cmd.Parameters("@json").Value = json_text
                    cmd.Parameters("@margine").Value = totaleGeneraleMargine.ToString()
                    cmd.ExecuteNonQuery()
                End Using
                oCon.Close()
                oCon.Dispose()

                'Ritorno dei valori
                dataItem("LEVELINDEX").Text = adv.LevelIndex
                dataItem("STAKEUNIT").Text = adv.StakeUnits
                dataItem("STOPATL5").Text = adv.StopAtL5
                dataItem("AUTHORIZERHEAVY").Text = adv.AuthorizedHeavy
                dataItem("REASON").Text = adv.Reason
                'dataItem("NOTE").Text = adv.GlobalMargin
                'dataItem("SIGNALW10").Text = adv.SignalW10
                dataItem("HOTZONE").Text = adv.HotZone & "<br/>" & adv.HotZoneLabel
                dataItem("NOTE").Text = adv.TooltipJson  'D

                If adv.StopAtL5 Then
                    dataItem("STOPATL5").CssClass = "ROSSO"
                Else
                    dataItem("STOPATL5").CssClass = "VERDE"
                End If

                If adv.AuthorizedHeavy Then
                    dataItem("AUTHORIZERHEAVY").CssClass = "VERDE"
                Else
                    dataItem("AUTHORIZERHEAVY").CssClass = "ROSSO"
                End If

                Dim history = engine.GetHistory(tavolo)

                ' Converte in HTML colorato
                Dim html As String = ""
                For Each h As Char In history
                    Select Case h
                        Case "B"c
                            html &= "<span style='color:red;font-weight:bold;font-size:12px'>B</span> "
                        Case "P"c
                            html &= "<span style='color:blue;font-weight:bold;font-size:12px'>P</span> "
                        Case "T"c
                            html &= "<span style='color:green;font-weight:bold;font-size:12px'>T</span> "
                        Case Else
                            html &= "<span style='color:gray;font-size:12px'>" & h & "</span> "
                    End Select
                Next

                ' Inserisce l'HTML nella cella
                Dim litW10 As Literal = CType(item.FindControl("litW10"), Literal)
                litW10.Text = html.Trim()

                Dim litNote As Literal = CType(item.FindControl("litNote"), Literal)
                litNote.Text = adv.SignalW10

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    If (adv.StopAtL5) Then 'se true allora va in pause scalping 
                        'INVIO IL SAFE 
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    End If
                End If
            End If


            'C - PROATTIVO SEMPLICE
            If RetConfiguration("DECISION_METHOD").ToUpper = "C" Then
                Dim json_text As String
                Dim settings As ProattivoSemplice.ProactiveSettings = New ProattivoSemplice.ProactiveSettings()
                Dim engine As ProattivoSemplice.ProactiveEngine = New ProattivoSemplice.ProactiveEngine(settings)
                Dim fullText As String
                Dim adv

                oCon = Database.GetConn()



                Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                engine.LoadStateFromJson(json_text)


                fullText = json_text

                If Not String.IsNullOrEmpty(fullText) Then
                    Dim maxLen As Integer = 50
                    Dim shortText As String = If(fullText.Length > maxLen, fullText.Substring(0, maxLen) & "...", fullText)

                    ' 🔹 Crea HTML con attributo data-fulltext
                    Dim htmljson As String = String.Format("<span class='copyCell' data-fulltext=""{0}"" title=""{0}"">{1}</span>", Server.HtmlEncode(fullText), "..click to copy..")

                    dataItem("JSON").Text = htmljson
                End If

                'If RetConfiguration("SEND_P_B") = "1" Then
                adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False), P_B(0))
                'Else
                '    adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False))
                'End If
                'salvataggio della struttura json
                json_text = engine.GetJson

                Using cmd As New SqlCommand("UpI_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    cmd.Parameters("@json").Value = json_text
                    cmd.Parameters("@margine").Value = totaleGeneraleMargine.ToString()
                    cmd.ExecuteNonQuery()
                End Using
                oCon.Close()
                oCon.Dispose()

                'Ritorno dei valori
                dataItem("LEVELINDEX").Text = adv.LevelIndex
                dataItem("STAKEUNIT").Text = adv.StakeUnits
                dataItem("STOPATL5").Text = adv.StopAtL5
                dataItem("AUTHORIZERHEAVY").Text = adv.AuthorizedHeavy
                dataItem("REASON").Text = adv.Reason
                'dataItem("NOTE").Text = adv.GlobalMargin
                'dataItem("SIGNALW10").Text = adv.SignalW10
                dataItem("HOTZONE").Text = adv.HotZone & "<br/>" & adv.HotZoneLabel
                dataItem("NOTE").Text = adv.TooltipJson   'C

                If adv.StopAtL5 Then
                    dataItem("STOPATL5").CssClass = "ROSSO"
                Else
                    dataItem("STOPATL5").CssClass = "VERDE"
                End If

                If adv.AuthorizedHeavy Then
                    dataItem("AUTHORIZERHEAVY").CssClass = "VERDE"
                Else
                    dataItem("AUTHORIZERHEAVY").CssClass = "ROSSO"
                End If

                Dim history = engine.GetHistory(tavolo)

                ' Converte in HTML colorato
                Dim html As String = ""
                For Each h As Char In history
                    Select Case h
                        Case "B"c
                            html &= "<span style='color:red;font-weight:bold;font-size:12px'>B</span> "
                        Case "P"c
                            html &= "<span style='color:blue;font-weight:bold;font-size:12px'>P</span> "
                        Case "T"c
                            html &= "<span style='color:green;font-weight:bold;font-size:12px'>T</span> "
                        Case Else
                            html &= "<span style='color:gray;font-size:12px'>" & h & "</span> "
                    End Select
                Next

                ' Inserisce l'HTML nella cella
                Dim litW10 As Literal = CType(item.FindControl("litW10"), Literal)
                litW10.Text = html.Trim()

                Dim litNote As Literal = CType(item.FindControl("litNote"), Literal)
                litNote.Text = adv.SignalW10

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    If (adv.StopAtL5) Then 'se true allora va in pause scalping 
                        'INVIO IL SAFE 
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    End If
                End If
            End If

            'B - PROATTIVO AVANZATO
            If RetConfiguration("DECISION_METHOD").ToUpper = "B" Then
                Dim json_text As String
                Dim settings As ProattivoAvanzato.ProactiveSettings = New ProattivoAvanzato.ProactiveSettings()
                Dim engine As ProattivoAvanzato.ProactiveEngine = New ProattivoAvanzato.ProactiveEngine(settings)
                Dim fullText As String
                Dim adv

                oCon = Database.GetConn()


                'LEttura parametri json decisionali
                Using cmd As New SqlCommand("UpS_SafeGuadJson", oCon)
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    json_text = cmd.ExecuteScalar()
                End Using
                engine.LoadStateFromJson(json_text)


                fullText = json_text

                If Not String.IsNullOrEmpty(fullText) Then
                    Dim maxLen As Integer = 50
                    Dim shortText As String = If(fullText.Length > maxLen, fullText.Substring(0, maxLen) & "...", fullText)

                    ' 🔹 Crea HTML con attributo data-fulltext
                    Dim htmljson As String = String.Format("<span class='copyCell' data-fulltext=""{0}"" title=""{0}"">{1}</span>", Server.HtmlEncode(fullText), "..click to copy..")

                    dataItem("JSON").Text = htmljson
                End If


                'If RetConfiguration("SEND_P_B") = "1" Then
                adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, totaleGeneraleMargine, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False), P_B(0))
                'Else
                'adv = engine.FeedAndDecide(tavolo, manoCorrente, margine, livello, totaleGeneraleMargine, IIf(RetConfiguration("SIGNALW10") = "1", True, False), IIf(RetConfiguration("HOT_ZONE") = "1", True, False))
                'End If

                'salvataggio della struttura json
                json_text = engine.GetJson

                Using cmd As New SqlCommand("UpI_SafeGuadJson", oCon) ' Nome della tua Stored Procedure
                    cmd.CommandType = CommandType.StoredProcedure
                    SqlCommandBuilder.DeriveParameters(cmd)
                    cmd.Parameters("@json").Value = json_text
                    cmd.Parameters("@margine").Value = totaleGeneraleMargine.ToString()
                    cmd.ExecuteNonQuery()
                End Using
                oCon.Close()
                oCon.Dispose()

                'Ritorno dei valori

                dataItem("TABLESCORE").Text = engine.GetTableScore(tavolo)
                dataItem("LEVELINDEX").Text = adv.LevelIndex
                dataItem("STAKEUNIT").Text = adv.StakeUnits
                dataItem("STOPATL5").Text = adv.StopAtL5
                dataItem("AUTHORIZERHEAVY").Text = adv.AuthorizedHeavy
                dataItem("REASON").Text = adv.Reason
                ' dataItem("NOTE").Text = adv.GlobalMargin   'B
                dataItem("HOTZONE").Text = adv.HotZone & "<br/>" & adv.HotZoneLabel

                If adv.StopAtL5 Then
                    dataItem("STOPATL5").CssClass = "ROSSO"
                Else
                    dataItem("STOPATL5").CssClass = "VERDE"
                End If

                If adv.AuthorizedHeavy Then
                    dataItem("AUTHORIZERHEAVY").CssClass = "VERDE"
                Else
                    dataItem("AUTHORIZERHEAVY").CssClass = "ROSSO"
                End If

                Dim history = engine.GetHistory(tavolo)

                ' Converte in HTML colorato
                Dim html As String = ""
                For Each h As Char In history
                    Select Case h
                        Case "B"c
                            html &= "<span style='color:red;font-weight:bold;font-size:12px'>B</span> "
                        Case "P"c
                            html &= "<span style='color:blue;font-weight:bold;font-size:12px'>P</span> "
                        Case "T"c
                            html &= "<span style='color:green;font-weight:bold;font-size:12px'>T</span> "
                        Case Else
                            html &= "<span style='color:gray;font-size:12px'>" & h & "</span> "
                    End Select
                Next

                ' Inserisce l'HTML nella cella
                Dim litW10 As Literal = CType(item.FindControl("litW10"), Literal)
                litW10.Text = html.Trim()

                Dim litNote As Literal = CType(item.FindControl("litNote"), Literal)
                litNote.Text = adv.SignalW10

                If Utility.VerBool(RetUserInfo("Administrator")) Then
                    If (adv.StopAtL5) Then 'se true allora va in pause scalping 
                        'INVIO IL SAFE 
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    End If
                End If
            End If

            'A -SAFEGUARDIAN
            If RetConfiguration("DECISION_METHOD").ToUpper = "A" Then

                '*******************SAFEGUARDIANCONFIG************************************

                ' Config condivisa
                Dim cfg As New SafeGuardianConfig With {
                .BaseUnit = Utility.VerDecimal(RetConfiguration("BASEUNIT")),
                .UnitsByLevel = New Integer() {1, 3, 7, 16, 33, 67, 135, 271, 543, 1087},
                .BankerCommission = Utility.VerDecimal(RetConfiguration("BANKERCOMMISSION")),
                .StopWinPartial = Utility.VerDecimal(RetConfiguration("STOPWINPARTIAL")),
                .RiskBudgetPerShoe = Utility.VerDecimal(RetConfiguration("RISKBUDGETPERSHOE")),
                .ShoeMaxHand = Utility.VerInt32(RetConfiguration("SHOEMAXHAND")),
                .HighLevelThreshold = Utility.VerInt32(RetConfiguration("HIGHLEVELTHRESHOLD")),
                .Window = Utility.VerInt32(RetConfiguration("WINDOW")),
                .VolatilityZ = Utility.VerDecimal(RetConfiguration("VOLATILITYZ")),
                .NegativeDriftSlope = Utility.VerDecimal(RetConfiguration("NEGATIVEDRIFTSLOPE")),
                .DrawdownFactorNextStake = Utility.VerDecimal(RetConfiguration("DRAWDOWNFACTORNEXTSTAKE"))
            }

                ' istanzia un guardian per tavolo
                Dim guardian As New SafeGuardian(cfg)
                ' … uno per ogni PCx …

                Dim decision = guardian.Evaluate(margine, manoCorrente, livello, latoBanker)

                If decision.PressSafe Then
                    ' Mostra “SAFE” e invia il comando al tuo bottone SAFE
                    dataItem("NOTE").Text = "SAFE " & decision.Note  'A
                    If Utility.VerBool(RetUserInfo("Administrator")) Then


                        'INVIO IL SAFE 
                        oCon = Database.GetConn()

                        Using cmd As New SqlCommand("UpI_Commands", oCon) ' Nome della tua Stored Procedure
                            cmd.CommandType = CommandType.StoredProcedure
                            SqlCommandBuilder.DeriveParameters(cmd)
                            cmd.Parameters("@ID_Command").Value = Azione.AzzeraMartingala
                            cmd.Parameters("@PC").Value = DataBinder.Eval(dataItem.DataItem, "ACCOUNT")
                            cmd.Parameters("@ID_User").Value = RetUserInfo("Id")

                            cmd.ExecuteNonQuery()
                        End Using
                        oCon.Close()
                        oCon.Dispose()
                    End If
                Else
                    dataItem("NOTE").Text = "RUN " & decision.Note
                End If
            End If
        End If
    End Sub
    Public Function LimitDataPoints(input As DataTable, maxPoints As Integer) As DataTable
        If input Is Nothing OrElse input.Rows.Count <= maxPoints Then
            Return input
        End If

        Dim reduced As DataTable = input.Clone()
        Dim totalRows As Integer = input.Rows.Count
        Dim stepSize As Double = totalRows / CDbl(maxPoints)

        For i As Integer = 0 To maxPoints - 1
            Dim index As Integer = CInt(Math.Floor(i * stepSize))
            If index >= totalRows Then Exit For
            reduced.ImportRow(input.Rows(index))
        Next

        Return reduced
    End Function


    Public Shared Function CleanAndLimitDataTable(input As DataTable, valueColumn As String,
                                                         Optional maxPoints As Integer = 200,
                                                         Optional threshold As Double = 2.5) As DataTable
        If input Is Nothing OrElse input.Rows.Count = 0 Then
            Return input
        End If

        ' --- 1️⃣ Calcolo statistico dei valori ---
        Dim values = input.AsEnumerable().Select(Function(r) Convert.ToDouble(r(valueColumn))).ToList()
        Dim mean As Double = values.Average()
        Dim stdDev As Double = Math.Sqrt(values.Average(Function(v) Math.Pow(v - mean, 2)))

        If stdDev = 0 Then
            Return input.Copy()
        End If

        ' Limiti per tagliare i picchi
        Dim minLimit As Double = mean - threshold * stdDev
        Dim maxLimitVal As Double = mean + threshold * stdDev

        ' --- 2️⃣ Tronca (clamp) i valori anomali ---
        Dim cleaned As DataTable = input.Clone()
        For Each row In input.Rows
            Dim newRow As DataRow = cleaned.NewRow()
            For Each col As DataColumn In input.Columns
                newRow(col.ColumnName) = row(col)
            Next

            Dim v As Double = Convert.ToDouble(row(valueColumn))
            If v < minLimit Then v = minLimit
            If v > maxLimitVal Then v = maxLimitVal
            newRow(valueColumn) = v
            cleaned.Rows.Add(newRow)
        Next

        ' --- 3️⃣ Limita il numero di punti ---
        If cleaned.Rows.Count <= maxPoints Then
            Return cleaned
        End If

        Dim reduced As DataTable = cleaned.Clone()
        Dim totalRows As Integer = cleaned.Rows.Count
        Dim stepSize As Double = totalRows / CDbl(maxPoints)

        For i As Integer = 0 To maxPoints - 1
            Dim index As Integer = CInt(Math.Floor(i * stepSize))
            If index >= totalRows Then Exit For
            reduced.ImportRow(cleaned.Rows(index))
        Next

        Return reduced
    End Function
    Private Sub Dashboard_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not (IsPostBack) Then
            If IsAdmin Then
                BtnResetDashboard.Visible = True
            Else
                BtnResetDashboard.Visible = False
            End If

            'RadHtmlChart1.DataSource = Nothing
            'RadHtmlChart1.DataBind()
            'UpdatePanelChart.Update()


            Session("RefreshGraph") = Nothing
            If Utility.VerInt32(RetUserInfo("ID")) = 0 Then
                Response.Redirect("/adminlogin.aspx")
            End If

            LoadGrid(_PageTitle.Replace(" ", "_"), RetUserInfo("Id"), "RadGrid1", RadGrid1)
            GridTimer.Interval = Utility.VerInt32(RetConfiguration("Refresh"))
            'BtnStop.Visible = IsAdmin
            'BtnStart.Visible = IsAdmin
            LblInfo.Text = _PageTitle & " " & ConfigurationSettings.AppSettings("Note") & " | Decision method: " & RetConfiguration("DECISION_METHOD").ToUpper
        End If
    End Sub

    Private Sub AbilitaColonneGriglia()
        If chkToggle.Checked Then
            RadGrid1.Columns(0).Visible = False
            RadGrid1.Columns(1).Visible = False
            RadGrid1.Columns(2).Visible = False
            RadGrid1.Columns(3).Visible = False
            RadGrid1.Columns(4).Visible = False
            RadGrid1.Columns(5).Visible = False
            RadGrid1.Columns(6).Visible = False
            RadGrid1.Columns(7).Visible = False
            RadGrid1.Columns(8).Visible = False
            RadGrid1.Columns(9).Visible = False
            RadGrid1.Columns(10).Visible = False
            RadGrid1.Columns(11).Visible = False
            RadGrid1.Columns(12).Visible = False


            RadGrid1.Columns(13).Visible = False
            RadGrid1.Columns(14).Visible = False
            RadGrid1.Columns(15).Visible = False
            RadGrid1.Columns(16).Visible = False
            RadGrid1.Columns(17).Visible = False
            RadGrid1.Columns(18).Visible = False
            RadGrid1.Columns(19).Visible = False
            RadGrid1.Columns(20).Visible = False
            RadGrid1.Columns(21).Visible = False
            RadGrid1.Columns(22).Visible = False

            RadGrid1.MasterTableView.GetColumn("ACCOUNT").Visible = True
            RadGrid1.MasterTableView.GetColumn("MARGINE").Visible = True
            RadGrid1.MasterTableView.GetColumn("MARTINGALA").Visible = True

            RadGrid1.Rebind()

        Else
            RadGrid1.Columns(0).Visible = True
            RadGrid1.Columns(1).Visible = True
            RadGrid1.Columns(2).Visible = True
            RadGrid1.Columns(3).Visible = True
            RadGrid1.Columns(4).Visible = True
            RadGrid1.Columns(5).Visible = True
            RadGrid1.Columns(6).Visible = True
            RadGrid1.Columns(7).Visible = True
            RadGrid1.Columns(8).Visible = True
            RadGrid1.Columns(9).Visible = True
            RadGrid1.Columns(10).Visible = True
            RadGrid1.Columns(11).Visible = True
            RadGrid1.Columns(12).Visible = True
            RadGrid1.Columns(13).Visible = True
            RadGrid1.Columns(14).Visible = True
            RadGrid1.Columns(15).Visible = True
            RadGrid1.Columns(16).Visible = True
            RadGrid1.Columns(17).Visible = True
            RadGrid1.Columns(18).Visible = True
            RadGrid1.Columns(19).Visible = True
            RadGrid1.Columns(20).Visible = True
            RadGrid1.Columns(21).Visible = True
            RadGrid1.Columns(21).Visible = True
            RadGrid1.MasterTableView.GetColumn("NOTE").Visible = False
            RadGrid1.MasterTableView.GetColumn("JSON").Visible = False
            RadGrid1.Rebind()
        End If
    End Sub

End Class
