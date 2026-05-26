# IIS Configuration (ASP.NET Core Bot Server)

Questo progetto è un'applicazione ASP.NET Core (C#) progettata per essere ospitata su IIS e utilizzata da bot concorrenti che condividono uno stato applicativo in memoria.

## Framework

* .NET: `net10.0`
* SDK: `Microsoft.NET.Sdk.Web`
* Hosting su IIS: **Out-of-process** (Kestrel dietro IIS)

In IIS l'Application Pool deve essere configurato con:
* **.NET CLR Version**: `No Managed Code`

---

## Architettura dello stato

L'applicazione mantiene uno **stato condiviso in memoria** tramite servizi `Singleton`.

Esempi:

* `ProactiveEngineService`
* `AppStateService`

Questo garantisce che tutte le richieste dei bot accedano agli stessi oggetti.

---

## Configurazione IIS (Application Pool)

Per evitare reset dello stato in memoria, è necessario disabilitare tutte le forme di idle e recycling.

### Process Model

* **Idle Time-out (minutes)**: `0`
* **Idle Time-out Action**: `Suspend`
* **Start Mode**: `AlwaysRunning`

### Recycling

* **Regular Time Interval**: `0`
* **Periodic Recycling**: `Disabled`
* **Memory-based Recycling**: `Disabled` (tutti i limiti)

### Rapid-Fail Protection

* **Enabled**: `False`

---

## Motivazione della configurazione

Queste impostazioni servono a:

* Evitare restart automatici del worker process
* Mantenere vivo lo stato in memoria (singleton)
* Garantire continuità operativa per i bot


---

## Middleware e servizi principali

* Logging: `ApiLoggingMiddleware`, `LoggingService`
* Database: `DatabaseRepository` (Scoped)
* Stato globale: `AppStateService` (Singleton)
* Engine: `ProactiveEngineService` (Singleton)
* Startup init: `StartupInitializer` (Hosted Service)

---

## Note finali

Questa configurazione è intenzionalmente ottimizzata per applicazioni **stateful in-memory**.

Qualsiasi modifica alla configurazione IIS potrebbe impattare direttamente la persistenza dello stato applicativo.

<!-- deploy trigger: 2026-05-26 v2 -->
