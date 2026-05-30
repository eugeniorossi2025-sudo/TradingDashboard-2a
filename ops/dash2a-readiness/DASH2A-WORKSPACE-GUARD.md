# DASH2A — Workspace Guard (obbligatorio)

> **Leggere questo file prima di ogni audit, patch, commit, validazione pre-deploy o attività agent su DASH2.**
> Se una condizione non è soddisfatta: **fermarsi subito** e non modificare nulla.

---

## 1. Repository corretto

```text
eugeniorossi2025-sudo/TradingDashboard-2a
```

URL GitHub:

```text
https://github.com/eugeniorossi2025-sudo/TradingDashboard-2a.git
```

---

## 2. Clone locale corretto

```text
C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri
```

---

## 3. Branch operativo

```text
main
```

---

## 4. Regola assoluta

**Qualsiasi attività su DASH2 deve partire da questo path:**

```text
C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri
```

Non aprire, non patchare e non committare da altri workspace Cursor.

---

## 5. Checklist obbligatoria (prima di ogni audit / patch / commit)

Eseguire **sempre** dalla root del clone DASH2A:

```powershell
cd "C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri"
git remote -v
git branch --show-current
git status
git rev-parse --show-toplevel
```

Verificare che l'output sia coerente con le sezioni 1–3 di questo file.

---

## 6. Stop immediato se il path non è quello corretto

Se `git rev-parse --show-toplevel` **non** restituisce esattamente:

```text
C:/Users/eugen/Desktop/NuovaDashboard-MarcoTurri
```

(o l'equivalente con backslash `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri`)

→ **fermarsi subito. Non modificare nulla.**

---

## 7. Stop immediato se il remote non è quello corretto

Se `git remote -v` **non** contiene:

```text
eugeniorossi2025-sudo/TradingDashboard-2a
```

→ **fermarsi subito. Non modificare nulla.**

Remote tipici **vietati** per lavori DASH2:

- `TradingDashboard-iis`
- repo / path legati a Dashboard 1
- `PCTEST45\TradingDashboard`
- qualsiasi clone diverso da `NuovaDashboard-MarcoTurri`

---

## 8. Dashboard 1 — fuori scope

Dashboard 1 (**TradingDashboard-iis**, `PCTEST45\TradingDashboard`, Eugenio-Demo10, Firebase Dashboard 1, VPS legacy IIS) è **fuori scope** per DASH2.

| Azione | Consentita per DASH2? |
|--------|----------------------|
| Leggere codice Dashboard 1 | **No** |
| Patchare Dashboard 1 | **No** |
| Committare su Dashboard 1 per lavori DASH2 | **No** |
| Usare credenziali / DB / URL Dashboard 1 | **No** |

Per DASH2 usare **solo** il clone e il remote indicati in questo documento.

---

## Riferimenti correlati

- Infrastruttura completa: [`DASH2A-INFRASTRUCTURE.md`](../../DASH2A-INFRASTRUCTURE.md) (root repo)
- Report contabilità: [`REPORT-CONTABILITA-CANONICA.md`](./REPORT-CONTABILITA-CANONICA.md)
- Validazione report: [`validate-report-coherence.ps1`](./validate-report-coherence.ps1)

---

*Documento operativo DASH2A — aggiornato 2026-05-30*
