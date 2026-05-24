# DASH2A Self-Hosted Runner Runbook

Obiettivo: collegare il server DASH2A a GitHub Actions con un runner self-hosted Windows, evitando deploy via RDP e password nei job.

## Stato target

- Repository: `eugeniorossi2025-sudo/TradingDashboard-2a`
- Runner OS: Windows Server
- Runner labels obbligatorie:
  - `self-hosted`
  - `Windows`
  - `DASH2A`
- Workflow inventory: `.github/workflows/dash2a-rdp-readiness.yml`
- Modalità: manuale tramite `workflow_dispatch`
- Conferma richiesta: `I_UNDERSTAND_INVENTORY_ONLY`

## Principi di sicurezza

- Il runner deve essere dedicato a DASH2A.
- Non usare password RDP nei workflow.
- Non stampare secret nei log.
- Non abilitare deploy automatico finché inventory e recovery non sono validati.
- Non eseguire restart IIS/server nei workflow di inventory.
- Non eseguire script distruttivi.
- Il workflow attuale è solo read-only inventory.

## Installazione runner sul server

Eseguire questi passaggi dentro il server DASH2A con un account amministrativo.

1. Aprire GitHub:

`Settings > Actions > Runners > New self-hosted runner`

2. Selezionare:

- Runner image: Windows
- Architecture: x64

3. Copiare i comandi ufficiali generati da GitHub.

4. Durante la configurazione impostare il nome runner in modo riconoscibile, per esempio:

`dash2a-windows-runner-01`

5. Aggiungere label custom:

`DASH2A`

6. Installare il runner come servizio Windows:

`.
svc install`

7. Avviare il servizio:

`.
svc start`

## Verifica runner

Da GitHub:

`Settings > Actions > Runners`

Il runner deve risultare:

- Online
- Idle
- Labels: `self-hosted`, `Windows`, `X64`, `DASH2A`

## Esecuzione inventory

Aprire GitHub Actions:

`DASH2A Server Inventory`

Eseguire manualmente con:

`confirm_inventory_only = I_UNDERSTAND_INVENTORY_ONLY`

Risultato atteso:

- job completato
- artifact `dash2a-server-inventory`
- file `inventory.md`

## Cosa raccoglie l'inventory

- Computer / dominio / memoria
- Sistema operativo / build / ultimo boot
- IPv4
- Porte TCP in ascolto
- Profili firewall
- Servizi rilevanti
- Scheduled task rilevanti
- IIS Sites
- IIS App Pools
- IIS Applications

## Cosa NON fa

- Non fa deploy
- Non riavvia IIS
- Non modifica file server
- Non modifica app pool
- Non apre porte
- Non cambia firewall
- Non legge/stampa password RDP

## Prossimo step dopo inventory

Dopo avere scaricato `inventory.md`, aggiornare:

- `ops/dash2a-readiness/INFRASTRUCTURE-READINESS.md`
- `ops/dash2a-readiness/DEPLOY-RUNBOOK.md`

Solo dopo audit completo si può progettare un workflow deploy manuale protetto.
