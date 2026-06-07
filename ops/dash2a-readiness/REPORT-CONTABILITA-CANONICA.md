# Report finanziario — formula canonica e piano di riallineamento

**Stato:** implementato 2026-06-07 (`mission-report-html:v2026-06-07-period-result-hero`, filtro Rome start-day)  
**Repo:** DASH2A (`NuovaDashboard-MarcoTurri`)  
**Audit di riferimento:** maggio 2026 Production (−94,94 € header attuale vs +915,90 € tabella vs +441,61 € Final Margin)  
**Regola:** nessun commit di reportistica finché ogni metrica derivata non mappa esplicitamente alla formula sotto.

---

## 1. Verdetto audit (confermato)

| Grandezza attuale | Tipo | Valore maggio 2026 | Usabile come risultato periodo? |
|-------------------|------|--------------------|---------------------------------|
| Net P&L header | Flusso (delta giorno cross-session) | −94,94 € | **Parziale** — unico con segno corretto del periodo osservato, ma formula non allineata alla tabella |
| Σ Net P&L missioni | Flusso (delta per sessione, finestra intera sessione) | +915,90 € | **No** — non rispetta clip temporale del periodo |
| Final Margin header | Stock (Σ margini assoluti PBT a chiusura) | +441,61 € | **No** — non è performance economica |

**Problema:** stock e flussi mostrati allo stesso peso visivo → lettura fuorviante.  
**Non è un bug aritmetico:** le implementazioni attuali riproducono fedelmente il codice; è un **errore di impostazione contabile**.

---

## 2. Formula canonica unica

### 2.1 Definizione

```
RisultatoPeriodoEuro =
  Σ  PnL_sessione,nel_periodo
  su tutte le missioni M incluse nel report
```

Per ogni missione `M`:

```
PnL_sessione(M, [T0, T1]) =
  Margine(ultimo_sample_M_in_finestra) − Margine(primo_sample_M_in_finestra)
```

Dove:
- `Margine(sample) = MissionMarginSamples.TotalMargin` (livello PBT al timestamp del sample)
- Se la missione ha **un solo sample** nella finestra → `PnL_sessione = 0`
- Se la missione **non ha sample** nella finestra → **esclusa** da tabella e index (non contabilizzata nel periodo)

### 2.2 Finestra temporale (clip)

**Timezone canonica:** `Europe/Rome` (CET/CEST).

```
T0 = inizio periodo = FromDate alle 00:00:00 Europe/Rome
T1 = fine periodo   = ToDate alle 23:59:59.999 Europe/Rome
                      (equivalente API: ToDateExclusive = giorno dopo ToDate 00:00:00 Rome)
```

**Inclusione missione — accounting day Europe/Rome** (rev. 2026-05-30, sostituisce overlap puro per tabella/index):

```
Missione inclusa in tabella report e index se:
  RuntimeMode = filtro
  AND Completed = true
  AND RomeDate(StartTime) ∈ [FromDate, ToDate]   (inclusi)
  AND ∃ almeno un sample clippato in [T0, T1]
```

Equivalente implementativo su timestamp UTC:

```
StartTime >= T0
AND StartTime < T1_exclusive
AND EXISTS sample con Timestamp ∈ [T0, T1_exclusive)
```

> **Revocato:** la regola overlap `(EndTime ?? StartTime) >= T0 AND StartTime < T1_exclusive` non si applica più alla tabella Mission Sessions né all'index. Una missione iniziata il 28/05 (Rome) non deve comparire nel report 29–30/05 anche se termina dopo mezzanotte del 29.

**Clip sample:**

```
Samples(M) = { s | s.SessionId = M.Id
                 AND s.Timestamp convertito a Europe/Rome ∈ [T0, T1] }
Ordinati per Timestamp ASC
```

**Primo/ultimo sample:** primo e ultimo elemento di `Samples(M)` dopo clip.

### 2.3 Proprietà obbligatorie (test di accettazione)

1. **Header = Σ tabella missioni** (stessa finestra, stessa timezone).
2. **Curva equity:** ultimo punto cumulativo = `RisultatoPeriodoEuro`.
3. **Σ giorni (Daily Net P&L)** = `RisultatoPeriodoEuro` (daily costruito come in §3.2).
4. **Final Margin non entra** nel calcolo del Risultato Periodo.
5. **Stop Win (Target header):** valore **per missione**, non somma su N sessioni. Con più missioni nel periodo: `GlobalTargetEuro` header = `MAX(session.GlobalTargetEuro)`, non `SUM`.

---

## 3. Metriche derivate (tutte dal Risultato Periodo)

### 3.1 Hero — un solo numero principale

| UI (IT) | Campo API | Formula |
|---------|-----------|---------|
| **Risultato periodo** | `periodResultEuro` | `RisultatoPeriodoEuro` (§2) |
| Colore | — | Verde se ≥ 0, rosso se < 0 |

**Rimosso dall'hero:** `FinalMarginEuro` aggregate.

### 3.2 Daily performance e curva equity

Per ogni **giorno calendario** `G` in `[T0, T1]` (Europe/Rome):

```
DailyPnL(G) = Σ  [ Margine(ultimo_sample_M_in_G) − Margine(primo_sample_M_in_G) ]
              M    su tutte le missioni con almeno un sample in G
```

Poi:

```
CumulativePnL(G) = Σ DailyPnL(g)  per g ≤ G
```

**Curva equity (grafico):** serie `CumulativePnL` — **non** margine assoluto PBT.

**Verifica telescopica:** per ogni missione, somma dei daily sui giorni in finestra = `PnL_sessione`; somma missioni = `RisultatoPeriodoEuro`.

### 3.3 Tabella missioni

| Colonna attuale | Colonna nuova | Formula |
|-----------------|---------------|---------|
| Net P&L | **P&L periodo** | `PnL_sessione(M, [T0,T1])` — clip applicato |
| Final Margin | **Margine PBT a chiusura** | `Margine(ultimo_sample_M_in_finestra)` se presente; altrimenti `—` |

Tooltip colonna stock: *"Livello assoluto del margine PBT al termine della finestra; non è il profitto del periodo."*

### 3.4 Period Return

```
PeriodReturnPct = RisultatoPeriodoEuro / InvestedCapitalBase × 100
InvestedCapitalBase = config REPORT_INVESTED_CAPITAL_BASE (default 5000)
```

### 3.5 Annualised Return

**Formula canonica (sostituisce scaling lineare su reporting days):**

```
workingDays = conteggio giorni G con DailyPnL(G) definito (≥1 sample)
periodReturnDecimal = RisultatoPeriodoEuro / InvestedCapitalBase

Se workingDays >= 7:
  AnnualisedReturnPct = ( (1 + periodReturnDecimal) ^ (365 / workingDays) − 1 ) × 100
Altrimenti:
  AnnualisedReturnPct = null   (UI/HTML: "N/D")
```

Footnote UI obbligatoria: *"Proiezione da performance osservata; non rendimento garantito."*

`ReportingDays` resta informativo (giorni calendario del filtro), **non** usato per annualizzazione.

### 3.6 Max Drawdown

Curva: `CumulativePnL` da §3.2.

```
peak(G) = max(peak(G−1), InvestedCapitalBase + CumulativePnL(G))
drawdown(G) = peak(G) − (InvestedCapitalBase + CumulativePnL(G))
MaxDrawdownEuro = max(drawdown(G))
MaxDrawdownPct = MaxDrawdownEuro / max(peak(G)) × 100   se peak > 0, altrimenti 0
```

Equity = capitale base + P&L cumulato (standard investitore). Capitale può restare mascherato in UI, ma il denominatore usa la base configurata.

### 3.7 Metriche secondarie (invariate nella logica, input aggiornato)

| Metrica | Input |
|---------|-------|
| Best / Worst day | max/min `DailyPnL(G)` |
| Win rate | giorni con `DailyPnL > 0` / `workingDays` |
| Average daily P&L | `RisultatoPeriodoEuro / workingDays` |
| Sharpe | su serie `DailyReturnPct(G) = DailyPnL(G) / InvestedCapitalBase × 100` |

---

## 4. Timezone — regole implementative

| Aspetto | Regola |
|---------|--------|
| Storage DB | UTC (invariato) |
| Raggruppamento giorno | Converti `Timestamp` UTC → `Europe/Rome`, poi `.Date` |
| Parametri API `from` / `to` | Interpretati come **date civili Europe/Rome** |
| Parametri `fromUtc` / `toUtc` (index) | Stessa semantica: date civili Rome (rinominare in docs; opzionale alias `fromDate`/`toDate` in fase 2) |
| Frontend `formatDateParam` | Continua a inviare `yyyy-MM-dd`; backend interpreta come Rome |
| HTML `GeneratedAt` | `Europe/Rome` per data/ora generazione |
| HTML `Start` / `End` sessioni | sempre convertiti UTC → `Europe/Rome` prima della stampa |
| Report singolo `/api/mission/report/{id}` | `from`/`to` derivati da `RomeDate(StartTime)` e `RomeDate(EndTime)`, non `.Date` UTC |

Helper consigliato (backend): `MissionReportTime.ToRome(DateTime utc)` + `MissionReportTime.PeriodBounds(fromDate, toDate)`.

---

## 5. Mapping codice → interventi (checklist — completata 2026-06-07)

| File | Intervento | Stato |
|------|------------|-------|
| `MissionReportBuilder.cs` | Filtro `StartTime ∈ [T0,T1)` Rome; range = `WithSamples` | ✅ |
| `MissionReportHtmlBuilder.cs` | Hero `periodResultEuro`; tabella P&L periodo + margine missione | ✅ |
| `FinancialReportService.ts` | Tipo `periodNetPnlEuro` index | ✅ |
| `Log.vue` | Colonne P&L periodo + Margine missione | ✅ |
| `ClientDesktop.vue` | KPI periodo + margine live separati | ✅ |
| `AdminMobile*.vue`, `ClientMobile.vue` | Solo `periodResultEuro` (no fallback stock) | ✅ |
| `validate-report-coherence.ps1` | ACC-02/03 allineati a formula canonica | ✅ |

---

## 6. Esempio atteso post-fix (maggio 2026 Production)

> Valori **indicativi** — da ricalcolare con clip Rome al momento dell'implementazione.  
> L'obiettivo del test non è mantenere −94,94 €, ma **coerenza interna**.

| Check | Atteso |
|-------|--------|
| `periodResultEuro` | ≠ `finalMarginEuro` (salvo caso degenere) |
| Header | = Σ colonna P&L periodo tabella |
| Σ daily | = header |
| Final Margin hero | **assente** |
| Period Return | derivato da header |
| Curva | ultimo punto = header |

---

## 7. Criteri di merge

- [x] Spec §2 implementata (`EnsureReportCoherence` runtime + `validate-report-coherence.ps1`)
- [x] Hero HTML = `RISULTATO PERIODO` (`periodResultEuro`)
- [x] `totalMarginEuro` / `missionMarginEuro` = stock chiusura (non hero)
- [ ] Validazione post-deploy su prod (maggio 2026 + 29–30/05 ACC-10)
- [x] Index `/api/mission/reports/index` — stesso filtro Rome + `periodNetPnlEuro`

---

## 8. Fuori scope (fase successiva)

- Cambiare cosa memorizza `MissionMarginSamples.TotalMargin` (resta stock PBT)
- Unificare naming API `from` vs `fromUtc`
- Mostrare capitale investito in chiaro in UI

---

## 9. Glossario UI (tre grandezze)

| Grandezza | Campo API | Tipo | Dove in UI |
|-----------|-----------|------|------------|
| Margine live | `currentMargin`, `margine` tavolo | Stock PBT istantaneo | Dashboard, AdminMobileLive hero, ClientDesktop «Margine live» |
| Margine di chiusura | `totalMarginEuro`, `missionMarginEuro` | Stock fine missione | Log, HTML tabella dettaglio |
| P&L periodo | `periodResultEuro`, `periodNetPnlEuro` | Flusso (delta sample clippati) | Hero report, mobile, Log colonna P&L, ClientDesktop KPI periodo |

*Implementazione 2026-06-07 — verifica post-deploy con `validate-report-coherence.ps1`.*
