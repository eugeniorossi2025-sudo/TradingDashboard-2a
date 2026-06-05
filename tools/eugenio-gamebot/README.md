# Eugenio Gamebot

> **Documento padre GameBot:** [`DASH2A-INFRASTRUCTURE.md`](../DASH2A-INFRASTRUCTURE.md) **§14** — sorgente, release 1.2 unified, hash, blindatura.  
> **Non duplicare** qui patch o lineage; questa cartella è solo copia storica / artefatti.

Questa area contiene una **copia di riferimento** del Gamebot e artefatti runtime filtrati.  
Per patch, build e deploy usare **`tools/eugenio-bot`** (sorgente canonico — vedi §14 infrastruttura).

---

## Riferimento rapido (dettaglio in §14)

| Voce | Percorso |
|------|----------|
| **Sorgente 1.2 unified** | `tools\eugenio-bot` |
| **Runtime collaudo** | `CRIPTOOK\relisegiacomo ok 1.2-test` |
| **Hash build 1.2** | `572224381C363649ADF08680009611111A5EEDCC738FDCD31F2DC0595448E28D` |

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
