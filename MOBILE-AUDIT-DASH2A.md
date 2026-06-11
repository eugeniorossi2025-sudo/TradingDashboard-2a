# MOBILE-AUDIT-DASH2A

Data audit: 2026-06-11
Agente: giacom
Repository verificato: C:/Users/eugen/Desktop/NuovaDashboard-MarcoTurri
Remote verificato: https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a.git
Branch verificato: main

## Vincoli rispettati

- Audit solo read-only sul codice, salvo creazione di questo report.
- Nessuna patch applicata.
- Nessun commit.
- Nessun deploy.
- Nessuna chiamata a endpoint live / smoke custom.
- Nessun uso di TradingDashboard-iis / Dashboard 1.

## Executive summary

La source of truth contabile dei report DASH2A e' il backend WebApi, in particolare:

- `backend/WebApi/Controllers/MissionController.cs`
- `backend/WebApi/Services/Implementations/MissionReportBuilder.cs`
- tabelle `MissionSessions` e `MissionMarginSamples`

La dashboard mobile usa la stessa API dei report admin/desktop:

- mobile: `frontend/src/views/mobile/AdminMobileReports.vue`, `frontend/src/views/mobile/AdminMobileLive.vue`
- admin: `frontend/src/views/pages/Log.vue`
- service condiviso: `frontend/src/service/FinancialReportService.ts`

Verdetto contabile: i valori DAY/WEEK/MONTH/YEAR mostrati nelle card mobile derivano dalla stessa source of truth dei report admin, tramite `GET /api/mission/report/range`. Non ho trovato un secondo calcolo contabile mobile separato.

Bug principali trovati:

1. Tempo missione presente backend/DTO ma ignorato dalla UI mobile live.
2. `AdminMobileLive.vue` richiama `openFinancialReports` ma la funzione non esiste nello script.
3. Le card mobile mostrano solo un sottoinsieme contabile: risultato periodo, target, progress, min/max, media giornaliera. Non mostrano dettaglio missioni, start/end/durata, P&L periodo vs margine missione, real hands, active tables.
4. Gross e commissioni non risultano modellati nei DTO/API/report: il sistema usa delta margine come net/P&L contabile.

## Source of truth contabile

### Backend

`backend/WebApi/Services/Implementations/MissionReportBuilder.cs`

- `BuildRangeReportAsync` seleziona missioni completate per runtime mode e periodo contabile.
- `ApplyAccountingPeriodSessionFilterWithSamples` include solo sessioni con `StartTime` nel periodo contabile Rome e con campioni nella finestra.
- `BuildSampleSummaries` calcola il P&L come delta tra primo e ultimo sample della sessione nella finestra.
- `ApplyCanonicalAccounting` imposta:
  - `PeriodResultEuro` = somma dei `NetPnl` per sessione.
  - `TotalMarginEuro` = somma dei margini missione a chiusura (`MissionMarginEuro`).
  - `FinalMarginEuro` = somma dei final margin sample.
  - `DailyRows` e metriche qualita'.
- `EnsureReportCoherence` verifica che `PeriodResultEuro`, somma sessioni, somma daily e ultima curva cumulata coincidano.

Linee evidenza:

- `MissionReportBuilder.cs:58` filtra le sessioni candidate.
- `MissionReportBuilder.cs:80` legge `MissionMarginSamples` nel periodo.
- `MissionReportBuilder.cs:124` applica accounting canonico.
- `MissionReportBuilder.cs:134` assegna `PeriodNetPnlEuro`.
- `MissionReportBuilder.cs:154` assegna `Totals.PeriodResultEuro`.
- `MissionReportBuilder.cs:178` esegue coerenza interna.
- `MissionReportBuilder.cs:380` documenta periodo contabile Rome basato su `StartTime`.

### API

`backend/WebApi/Controllers/MissionController.cs`

- `GET /api/mission/report/range` genera report range JSON/HTML/CSV dalla stessa builder canonica.
- `GET /api/mission/reports/index` genera lista missioni admin con start/end, P&L periodo, margine missione, target, mani reali, samples.
- `GET /api/mission/current` espone stato missione aperta.

Linee evidenza:

- `MissionController.cs:72` endpoint `report/range`.
- `MissionController.cs:85` chiama `BuildRangeReportAsync`.
- `MissionController.cs:100` in summary JSON ritorna `Totals`, `QualityMetrics`, conteggi.
- `MissionController.cs:119` endpoint `reports/index`.
- `MissionController.cs:201` espone `StartUtc`.
- `MissionController.cs:202` espone `EndUtc`.
- `MissionController.cs:206` espone `PeriodNetPnlEuro`.
- `MissionController.cs:207` espone `FinalMarginEuro`.

### Tabelle contabili

`backend/Entities/MissionSession.cs`

- `StartTime`, `EndTime`, `TotalMargin`, `RealHandsCount`, `GlobalTarget`, `ActiveTables`, `RuntimeMode`, `Completed`, `FinalizationReason`.

`backend/Entities/MissionMarginSample.cs`

- `SessionId`, `Timestamp`, `TotalMargin`, `ActiveTables`, `VmCurrent`, `RuntimeMode`.

## Report DAY / WEEK / MONTH / YEAR

`frontend/src/composables/useReportPeriod.ts`

- Chip supportati: `day`, `week`, `month`, `year`.
- Il range e' calcolato in data Europe/Rome tramite `toRomeIsoDate`.
- DAY: oggi Rome.
- WEEK: lunedi' della settimana corrente fino a oggi.
- MONTH: primo giorno del mese fino a oggi.
- YEAR: primo gennaio fino a oggi.

Linee evidenza:

- `useReportPeriod.ts:4` tipo `ReportPeriodChip`.
- `useReportPeriod.ts:6` chip DAY/WEEK/MONTH/YEAR.
- `useReportPeriod.ts:14` `getPeriodRange`.

Conclusione: mobile DAY/WEEK/MONTH/YEAR non ha formule contabili proprie; cambia solo `from`/`to` e richiama lo stesso endpoint canonico.

## Mobile vs admin

### Admin desktop / Log

`frontend/src/views/pages/Log.vue`

- Usa `FinancialReportService.openHtmlReport`, `downloadJson`, `downloadCsv` per range report.
- Usa `FinancialReportService.getReportsIndex` per lista missioni.
- Mostra start/end, P&L periodo, margine missione, target, mani reali, samples.

Linee evidenza:

- `Log.vue:107` apre report finanziario HTML.
- `Log.vue:137` carica index missioni.
- `Log.vue:312` mostra `periodNetPnlEuro`.

### Mobile reports

`frontend/src/views/mobile/AdminMobileReports.vue`

- Usa `FinancialReportService.getRangeReport(runtimeMode, from, to)`.
- Mostra solo `PeriodResultEuro`, `sampleCount`, `globalTargetEuro`, `progressPct`, `margineMin`, `margineMax`, `averageDailyPnl`.
- Download HTML/CSV/JSON usa lo stesso endpoint admin.

Linee evidenza:

- `AdminMobileReports.vue:81` carica range report.
- `AdminMobileReports.vue:188` mostra `periodResultEuro`.
- `AdminMobileReports.vue:189` mostra `sampleCount`.
- `AdminMobileReports.vue:196` mostra `globalTargetEuro`.
- `AdminMobileReports.vue:204` mostra min/max.
- `AdminMobileReports.vue:208` mostra daily avg.

Conclusione: i numeri principali mobile sono coerenti con admin perche' arrivano dalla stessa API. La differenza e' di completezza informativa, non di formula.

## Mission lifecycle

### Start missione

`backend/WebApi/Services/Implementations/MissionLifecycleService.cs`

La missione parte solo se:

- non c'e' missione aperta;
- start non e' suppressed fino al reset;
- esiste reset boundary;
- esiste un `PcCurrentStatus.LastAdvice` post-reset;
- c'e' almeno un tavolo attivo;
- esiste primo punto `Margini` post-reset e post-decisione.

Linee evidenza:

- `MissionLifecycleService.cs:174` `ObserveLiveStateAsync`.
- `MissionLifecycleService.cs:207` blocco suppress.
- `MissionLifecycleService.cs:210` reset boundary.
- `MissionLifecycleService.cs:214` primo decide con `LastAdvice`.
- `MissionLifecycleService.cs:223` active tables.
- `MissionLifecycleService.cs:226` primo punto margine.
- `MissionLifecycleService.cs:479` crea missione dal primo punto margine.

### End missione

La missione viene finalizzata se:

- `LastAdvice.ActionCode == 1` e `Reason == STOP_WIN`;
- oppure `currentMargin >= GlobalTarget`;
- oppure reset/manual finalize.

Linee evidenza:

- `MissionLifecycleService.cs:186` stop win action code.
- `MissionLifecycleService.cs:194` soglia margine >= target.
- `MissionLifecycleService.cs:304` finalize current.
- `MissionLifecycleService.cs:585` assegna `EndTime`.
- `MissionLifecycleService.cs:588` calcola `RealHandsCount`.
- `MissionLifecycleService.cs:589` setta `Completed = true`.

## Tempo missione / elapsed time / duration

### Backend

Il backend ha gia' start/end:

- `MissionLifecycleState.StartTime` e `EndTime` in `IMissionLifecycleService.cs:61-62`.
- `MissionSession.StartTime` e `EndTime` in `MissionSession.cs:19-21`.
- `MissionReportSession.StartTime` e `EndTime` in `MissionController.cs:385-386`.

`MissionReportHtmlBuilder.cs` calcola e mostra la durata nel report HTML:

- `MissionReportHtmlBuilder.cs:62` calcola `dur`.
- `MissionReportHtmlBuilder.cs:76` colonna `Durata`.
- `MissionReportHtmlBuilder.cs:129` `FormatDuration(start, end)`.

### DTO frontend

`frontend/src/service/FinancialReportService.ts`

- `MissionLifecycleState` dichiara `startTime?: string | null` e `endTime?: string | null`.

Linee evidenza:

- `FinancialReportService.ts:71` `startTime`.
- `FinancialReportService.ts:72` `endTime`.

### UI mobile

`frontend/src/composables/useOpenMissionHero.ts`

- Carica `getCurrentMission()` ma usa solo `sessionId`, `currentMargin`, progresso e target.
- Non calcola `elapsed` da `startTime`.
- Non espone `startTime`, `endTime`, `elapsedTime` o `missionDuration` al template.

Linee evidenza:

- `useOpenMissionHero.ts:16` carica missione corrente.
- `useOpenMissionHero.ts:25` espone session id.
- `useOpenMissionHero.ts:29` espone margine corrente.
- `useOpenMissionHero.ts:50` nota: `Missione #... aperta`.
- `useOpenMissionHero.ts:55` label progresso.

`AdminMobileLive.vue` mostra:

- sessione;
- margine live;
- strategia;
- progresso;
- report periodo.

Non mostra start missione, durata, elapsed time.

Linee evidenza:

- `AdminMobileLive.vue:238` mostra `heroProgressLabel`.
- `AdminMobileLive.vue:249` mostra sessione.
- `AdminMobileLive.vue:364` link report mobile.

Verdetto tempo missione: il tempo missione esiste backend/DTO, viene inviato da `/api/mission/current`, ma viene ignorato dalla UI mobile live. Nei report HTML la durata esiste; nelle card mobile live/report summary non viene visualizzata.

## Margin / net / gross / commissioni

### Margin

Il margine contabile e' basato su `MissionMarginSamples.TotalMargin` e `MissionSessions.TotalMargin`.

### Net

Il net/P&L periodo e' `last sample - first sample` nella finestra contabile, aggregato per sessione e giorno.

Linee evidenza:

- `MissionReportBuilder.cs:223` se due sample: `last - first`.
- `MissionReportBuilder.cs:288` daily net: `endMargin - startMargin`.

### Gross / commissioni

Non ho trovato campi contabili dedicati a:

- gross;
- commissioni;
- fee/rake;
- lordo/netto separati.

Nei file rilevanti compare solo `NetPnl`, `PeriodNetPnlEuro`, `MissionMarginEuro`, `TotalMarginEuro`, `FinalMarginEuro`. Quindi la dashboard non puo' mostrare o riconciliare commissioni separate se la source of truth non le produce.

Rischio contabile: medio, se il business richiede separazione gross/commissioni/net. Basso, se il margine gia' incorpora commissioni a monte nel Decisore/DB.

## Bug trovati

### BUG 1 - Tempo missione non mostrato su mobile live

File coinvolti:

- `backend/WebApi/Services/IMissionLifecycleService.cs`
- `frontend/src/service/FinancialReportService.ts`
- `frontend/src/composables/useOpenMissionHero.ts`
- `frontend/src/views/mobile/AdminMobileLive.vue`
- potenzialmente `frontend/src/views/mobile/ClientMobile.vue`

Evidenza:

- Backend espone `StartTime`/`EndTime`.
- DTO frontend dichiara `startTime`/`endTime`.
- Composable mobile non espone durata/elapsed.
- Template mobile non mostra durata.

Rischio contabile: basso sul calcolo P&L, medio sull'operativita': l'utente non vede da quanto e' aperta la missione e puo' perdere contesto su stop time / durata.

Patch proposta:

- In `useOpenMissionHero.ts`, aggiungere computed `missionStartedAt`, `missionEndedAt`, `missionElapsedMs`, `missionElapsedLabel` calcolati da `currentMission.startTime/endTime`.
- Aggiornare `AdminMobileLive.vue` e `ClientMobile.vue` per mostrare una card/pill `Durata missione` quando `hasOpenMission`.
- Usare normalizzazione UTC robusta come gia' fatto in `Dashboard.vue` con `parseServerUtcDate`.

### BUG 2 - `openFinancialReports` non definito in AdminMobileLive

File coinvolto:

- `frontend/src/views/mobile/AdminMobileLive.vue`

Evidenza:

- Template: `AdminMobileLive.vue:364` usa `@click="openFinancialReports"`.
- Nessuna funzione `openFinancialReports` nello script.
- Esiste route `/admin/mobile-reports` in `frontend/src/router/index.js:197`.
- Esiste nav corretta in `MobileAdminQuickNav.vue` verso `/admin/mobile-reports`.

Rischio contabile: basso sui numeri, alto su UX mobile: il bottone "Pick period & download" puo' non funzionare o generare errore runtime.

Patch proposta:

- Importare/inizializzare `useRouter` in `AdminMobileLive.vue` e aggiungere:
  - `const router = useRouter();`
  - `function openFinancialReports() { router.push('/admin/mobile-reports'); }`
- In alternativa sostituire il bottone con `<RouterLink>` coerente allo stile.

### BUG 3 - Mobile report summary meno completo dell'admin

File coinvolti:

- `frontend/src/views/mobile/AdminMobileReports.vue`
- `frontend/src/views/mobile/AdminMobileLive.vue`
- `frontend/src/views/pages/Log.vue`
- `frontend/src/service/FinancialReportService.ts`

Evidenza:

- Admin `Log.vue` mostra lista missioni con start/end, P&L periodo, margine missione, target, mani reali, samples.
- Mobile summary mostra solo aggregati essenziali.
- `getRangeReport(... summary: true)` omette `Sessions`, `Samples`, `DailyRows` e ritorna solo totali/conteggi.

Rischio contabile: medio lato lettura: i numeri principali sono corretti, ma mobile non permette di verificare differenze tra `P&L periodo` e `Margine missione`, soprattutto quando una missione attraversa il taglio giorno/settimana/mese/anno.

Patch proposta:

- In mobile report detail usare anche `getReportsIndex` oppure `getRangeReport(summary: false)` per mostrare almeno ultime missioni del periodo.
- Mostrare per ogni missione: start/end/durata, `PeriodNetPnlEuro`, `TotalMarginEuro`/`MissionMarginEuro`, `GlobalTargetEuro`, `RealHandsCount`, `SamplesCount`.
- Mantenere la hero aggregata, ma aggiungere sezione "Dettaglio missioni" coerente con admin.

### BUG 4 - Gross e commissioni assenti dal contratto contabile

File coinvolti:

- `backend/WebApi/Controllers/MissionController.cs`
- `backend/WebApi/Services/Implementations/MissionReportBuilder.cs`
- `frontend/src/service/FinancialReportService.ts`
- `frontend/src/views/mobile/AdminMobileReports.vue`
- `frontend/src/views/pages/Log.vue`

Evidenza:

- Nessun campo `gross`, `commission`, `commissioni`, `fee`, `rake` nei contratti contabili rilevanti.
- `BuildCsv` esporta solo `NetPnL`, return, cumulative, sample count.

Rischio contabile: dipende dal dominio dati. Se il margine e' gia' netto, rischio basso ma serve documentazione esplicita. Se commissioni devono essere scorporate, rischio alto per riconciliazione incompleta.

Patch proposta:

- Chiarire source of truth: `Margini.TotalMargin` e' netto o lordo?
- Se esistono commissioni a monte, aggiungere campi backend `GrossPnlEuro`, `CommissionEuro`, `NetPnlEuro` nei DTO e nei report.
- Se non esistono, aggiungere nota esplicita nei report: "P&L = delta margine contabile gia' netto secondo source of truth".

## Differenze mobile vs admin

| Area | Admin desktop | Mobile | Esito |
|---|---|---|---|
| Range report | `openHtmlReport/downloadJson/downloadCsv` | stesso service/API | coerente |
| DAY/WEEK/MONTH/YEAR | date manuali; mobile chips | mobile chips Rome | coerente come API, diversa UX |
| Lista missioni | presente in `Log.vue` | assente nelle card mobile | gap UX/trasparenza |
| Start/end missione | presente nella lista missioni admin | assente mobile live/report cards | bug/gap |
| Durata missione | presente nel report HTML | assente mobile live | bug/gap |
| P&L periodo vs margine missione | esplicitato in admin e HTML | non esplicitato nelle card | rischio interpretativo |
| Commissioni/gross | assenti | assenti | gap modello contabile |

## Patch proposta complessiva

Nessuna patch applicata in questo audit. Patch consigliata, in ordine:

1. Fix immediato UI mobile:
   - definire `openFinancialReports` in `AdminMobileLive.vue`.

2. Aggiungere durata missione mobile:
   - estendere `useOpenMissionHero.ts` con `missionElapsedLabel`.
   - mostrare durata/start in `AdminMobileLive.vue` e `ClientMobile.vue`.

3. Allineare mobile report al dettaglio admin:
   - usare `getReportsIndex` nella pagina mobile report o aggiungere endpoint summary con `sessions` compatto.
   - mostrare `P&L periodo` e `Margine missione` separati.

4. Decisione prodotto su commissioni:
   - documentare se il margine e' gia' netto;
   - oppure estendere source of truth con gross/commission/net.

## Comandi di verifica suggeriti prima di patch futura

Solo dopo approvazione patch, in locale:

```powershell
npm run build --prefix frontend
dotnet build .\backend\WebApi\WebApi.csproj
git diff --check
```

Nessun deploy automatico. Nessun workflow manuale senza conferma esplicita.
