# Agenti nominati — DASH2A (repo TradingDashboard-2a)

> **Un documento = un agente.** Non mescolare scope.
> Dashboard 1 Eugenio → repo **`TradingDashboard-iis`** → **`marco`** / **`marco1`**.

---

## giacom — Dashboard DASH2A

| | |
|--|--|
| **Scrivi** | `giacom` o `/giacom` |
| **File agente** | `.cursor/agents/giacom.md` |
| **Documento unico** | **`docs/AGENT-GIACOM.md`** |

**Esempio:**

```text
giacom fix pagina dashboard Vue
```

**Gestisce:** frontend Vue, WebApi, Decisore, deploy DASH2A.  
**Non tocca:** GameBot exe, EUGENIO13, admin IIS Eugenio.

---

## giacomo1 — GameBot Giacomo (linea 1.7)

| | |
|--|--|
| **Scrivi** | `giacomo1` o `/giacomo1` |
| **File agente** | `.cursor/agents/giacomo1.md` |
| **Documento unico** | **`docs/AGENT-GIACOMO1.md`** |

**Esempio:**

```text
giacomo1 patch waiting probe su tools/eugenio-bot
```

**Gestisce:** `tools/eugenio-bot`, runtime `relisegiacomo ok 1.6/1.7`, build/promozione bot.  
**Non tocca:** Vue dashboard, Eugenio13, `baccarat-bot-main-socket-artifact`.

---

## Tabella tutti gli agenti (2 repo)

| Nome | Repo | Documento unico |
|------|------|-----------------|
| **marco** | TradingDashboard-iis | `docs/AGENT-MARCO.md` |
| **marco1** | TradingDashboard-iis | `docs/AGENT-MARCO1.md` |
| **giacom** | NuovaDashboard-MarcoTurri | `docs/AGENT-GIACOM.md` |
| **giacomo1** | NuovaDashboard-MarcoTurri | `docs/AGENT-GIACOMO1.md` |
