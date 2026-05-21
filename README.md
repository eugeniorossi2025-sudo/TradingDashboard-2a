# NuovaDashboard-MarcoTurri

Repository strutturale pulito per separare i moduli della dashboard.

## Struttura

- `backend/` - backend dashboard
- `frontend/` - frontend dashboard
- `chrome-extension/` - estensione Chrome
- `decision-engine/` - motore decisionale
- `tools/` - script, tool e bot separati
- `config/` - configurazioni repo/server condivise

## Note import

Il primo import esclude artefatti runtime e pesanti: `bin`, `obj`, `node_modules`, `Release`, `Debug`, DLL, ZIP, BMP runtime, cache, database locali, `telegramSession` e `appData`.

