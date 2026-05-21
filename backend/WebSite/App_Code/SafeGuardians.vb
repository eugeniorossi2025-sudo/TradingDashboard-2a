Imports System
Imports System.Collections.Generic
Imports System.Linq

Public Enum SafeReason
    None
    HitStopWin            ' raggiunto target utile
    VolatilitySpike       ' volatilità alta + drift negativo
    DrawdownRisk          ' drawdown > soglia rispetto al picco
    LevelRiskNearShoeEnd  ' livello alto vicino a fine shoe
    RiskBudget            ' superato budget perdita per shoe
End Enum

Public Class SafeDecision
    Public Property PressSafe As Boolean
    Public Property Reason As SafeReason
    Public Property Note As String

    Public Sub New(pressSafe As Boolean, reason As SafeReason, note As String)
        Me.PressSafe = pressSafe
        Me.Reason = reason
        Me.Note = note
    End Sub
End Class

Public Class SafeGuardianConfig
    ' Martingala (solo per stimare il rischio del prossimo colpo)
    Public Property BaseUnit As Decimal = 0.2D          ' K
    Public Property UnitsByLevel As Integer() = {1, 3, 7, 16, 33, 67, 135, 271, 543, 1087}

    Public ReadOnly Property MaxLevel As Integer
        Get
            Return UnitsByLevel.Length
        End Get
    End Property

    Public Property BankerCommission As Decimal = 0.05D  ' 5%

    ' Soglie SAFE
    Public Property StopWinPartial As Decimal = 50D      ' es. +10€ per shoe
    Public Property RiskBudgetPerShoe As Decimal = -50D  ' perdita massima per shoe prima di SAFE
    Public Property ShoeMaxHand As Integer = 60          ' ci fermiamo entro mano 60
    Public Property HighLevelThreshold As Integer = 6    ' da questo livello in su più prudenza

    ' Volatilità / trend
    Public Property Window As Integer = 20               ' n° osservazioni per stime
    Public Property VolatilityZ As Decimal = 1.8D        ' trigger spike (σ alto)
    Public Property NegativeDriftSlope As Decimal = -0.2D ' drift medio (€/mano) considerato "negativo" (approssimato)

    ' Drawdown
    Public Property DrawdownFactorNextStake As Decimal = 1.5D ' se DD > 1.5 * prossimo stake -> SAFE
End Class

Public Class SafeGuardian
    Private ReadOnly _cfg As SafeGuardianConfig
    Private ReadOnly _margins As New List(Of Decimal)()  ' storico Margine (saldo iniziale tavolo vs istantaneo)
    Private _peak As Decimal                             ' picco di margine raggiunto nello shoe

    Public Sub New(cfg As SafeGuardianConfig)
        _cfg = cfg
        Reset()
    End Sub

    Public Sub Reset()
        _margins.Clear()
        _peak = 0D
    End Sub

    ''' <summary>
    ''' Aggiorna con il margine corrente (es. +5.60, -2.40, ...), mano corrente e livello martingala (1..N).
    ''' </summary>
    Public Function Evaluate(marginNow As Decimal, currentHand As Integer, currentLevel As Integer, sideIsBanker As Boolean) As SafeDecision
        ' aggiorna serie + picco
        _margins.Add(marginNow)
        If marginNow > _peak Then _peak = marginNow

        ' 1) STOP-WIN parziale
        If marginNow >= _cfg.StopWinPartial Then
            Return New SafeDecision(True, SafeReason.HitStopWin, "SAFE: raggiunto stop-win + " & marginNow.ToString())
        End If

        ' 2) Budget rischio per shoe
        If marginNow <= _cfg.RiskBudgetPerShoe Then
            Return New SafeDecision(True, SafeReason.RiskBudget, "SAFE: budget shoe " + marginNow.ToString() + " ≤ " + _cfg.RiskBudgetPerShoe.ToString())
        End If

        ' servono almeno 6–8 punti per stime sensate
        If _margins.Count >= Math.Max(8, _cfg.Window) Then
            ' Δ margine (variazioni mano-mano)
            Dim deltas As New List(Of Decimal)()
            For i As Integer = 1 To _margins.Count - 1
                deltas.Add(_margins(i) - _margins(i - 1))
            Next

            ' drift (media delle ultime N Δ)
            Dim tail = deltas.Skip(Math.Max(0, deltas.Count - _cfg.Window)).ToArray()
            Dim drift As Decimal = tail.Average()

            ' σ delle ultime N Δ
            Dim mean As Decimal = tail.Average()
            Dim variance As Decimal = tail.Select(Function(x) (x - mean) * (x - mean)).Average()
            Dim sigma As Decimal = CDec(Math.Sqrt(CDbl(variance)))

            ' 3) Volatilità alta + drift negativo
            If sigma >= _cfg.VolatilityZ * Math.Max(0.1D, Math.Abs(mean)) AndAlso drift <= _cfg.NegativeDriftSlope Then
                Return New SafeDecision(True, SafeReason.VolatilitySpike, "SAFE: σ alto " & sigma.ToString() & " e drift " + drift.ToString())
            End If

            ' 4) Drawdown pericoloso rispetto al prossimo stake
            Dim nextStake As Decimal = EstimateNextStake(currentLevel, sideIsBanker)
            Dim drawdown As Decimal = _peak - marginNow ' quanto ho perso dal picco
            If drawdown >= _cfg.DrawdownFactorNextStake * nextStake Then
                Return New SafeDecision(True, SafeReason.DrawdownRisk, "SAFE: DD " & drawdown.ToString() & " ≥ " & (_cfg.DrawdownFactorNextStake * nextStake).ToString())
            End If
        End If

        ' 5) Fine shoe: se livello alto, chiudi in sicurezza
        If currentHand >= _cfg.ShoeMaxHand - 1 AndAlso currentLevel >= _cfg.HighLevelThreshold Then
            Return New SafeDecision(True, SafeReason.LevelRiskNearShoeEnd, "SAFE: mano " & currentHand.ToString() & "/" & _cfg.ShoeMaxHand.ToString() & " con " & currentLevel.ToString())
        End If

        ' Nessun SAFE
        Return New SafeDecision(False, SafeReason.None, "Prosegui")
    End Function

    Private Function EstimateNextStake(currentLevel As Integer, sideIsBanker As Boolean) As Decimal
        ' Sostituisce Math.Clamp con una funzione personalizzata
        Dim idx As Integer = Clamp(currentLevel - 1, 0, _cfg.MaxLevel - 1)
        Dim stake As Decimal = _cfg.UnitsByLevel(idx) * _cfg.BaseUnit
        stake = RoundToChips(stake)

        ' Se stimiamo il "costo rischio" netto lato Banker, consideriamo la commissione come perdita attesa aggiuntiva
        If sideIsBanker Then
            Dim comm As Decimal = Math.Round(stake * _cfg.BankerCommission, 2, MidpointRounding.AwayFromZero)
            stake += comm ' prudenziale
        End If
        Return stake
    End Function

    Private Shared Function RoundToChips(v As Decimal) As Decimal
        Return Math.Round(v * 10D, MidpointRounding.AwayFromZero) / 10D ' 0,10€ – adatta alle tue fiches
    End Function

    ' Funzione Clamp personalizzata per sostituire Math.Clamp
    Private Shared Function Clamp(value As Integer, min As Integer, max As Integer) As Integer
        If value < min Then Return min
        If value > max Then Return max
        Return value
    End Function
End Class