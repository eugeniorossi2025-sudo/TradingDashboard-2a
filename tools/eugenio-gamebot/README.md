# Eugenio Gamebot

Questa area contiene una **copia di riferimento** del Gamebot e artefatti runtime filtrati.  
Per patch, build e Step 3 usare **`tools/eugenio-bot`** (sorgente reale verificato — vedi sotto).

---

## GAMEBOT — FONTE VERIFICATA PER RELEASE 1.0

Verificato da storico Cursor + hash binario (2026-06-03 build, audit 2026-06-05).

| Voce | Percorso |
|------|----------|
| **Runtime ufficiale** | `C:\Users\eugen\Desktop\CRIPTOOK\CRIPTOOK\relisegiacomo ok 1.0` |
| **Sorgente reale (patch / build)** | `C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri\tools\eugenio-bot` |
| **Solution** | `tools\eugenio-bot\Gamebot.sln` |
| **Configuratore** | `tools\eugenio-bot\Gamebot\UI\WindowForm\Configuratore.cs` |
| **Punto patch Step 3** | `Configuratore.start_all()` — prima di `Player.Instance.Start()` |
| **Build** | Release \| Any CPU |
| **Output build** | `tools\eugenio-bot\Gamebot\bin\Release\` |
| **Copia runtime** | `Gamebot.exe` + `Gamebot.pdb` → `relisegiacomo ok 1.0` |
| **Hash `Gamebot.exe` runtime** | `D1A8AAF0F6AAF50BAB41FFE19DDE2B19904D0723352CDDA1179D0E520150C273` |

### Storia runtime `relisegiacomo ok 1.0`

1. Cartella creata per **robocopy** da `CRIPTOOK\CRIPTOOK\relisegiacomo ok` (exe iniziale hash `DFEF9CDC…`, 2026-05-08).
2. Il **2026-06-03 ~17:52** exe e pdb in `1.0` sono stati **sovrascritti** con l’output di build da `tools\eugenio-bot` (hash `D1A8AAF0…`).

**Regola:** non modificare `relisegiacomo ok 1.0` come riferimento ufficiale; per lavoro Step 3 usare copia `relisegiacomo ok 1.1` se presente.

### Build (MSBuild)

```text
MSBuild tools\eugenio-bot\Gamebot.sln /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild
```

Output atteso:

```text
C:\Users\eugen\Desktop\NuovaDashboard-MarcoTurri\tools\eugenio-bot\Gamebot\bin\Release\Gamebot.exe
```

---

## Cartelle in questo tree (`eugenio-gamebot`)

| Cartella | Ruolo | Fonte verificata release 1.0? |
|----------|--------|--------------------------------|
| **`../eugenio-bot/`** | **Sorgente reale DASH2A** — patch, build, Step 3 | **Sì** |
| `source/` | Copia importata da `C:\Users\eugen\Documents\baccarat-bot-main OK\baccarat-bot-main` | **No** — rebuild non produce byte-identico a `relisegiacomo ok 1.0` |
| `release-reference/` | Config/XML filtrati da `C:\Users\eugen\Desktop\CRIPTOOK\CRIPTOOK\relisegiacomo ok` (senza exe/DLL/PDB) | Solo riferimento config |

La cartella `release-reference/` esclude binari, DLL, PDB, BMP/OCR runtime, cache, log, `telegramSession`, `appData`, `x64`, `x86` e altri artefatti generati.

---

## Documentazione correlata

- Canonico infra DASH2A: [`DASH2A-INFRASTRUCTURE.md`](../../DASH2A-INFRASTRUCTURE.md) — §14 GameBot runtime/source truth
