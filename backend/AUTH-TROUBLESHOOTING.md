# Guida Risoluzione Problemi Permessi e Autenticazione

## Problema: Tutte le chiamate API restituiscono errori di permessi

### Causa Principale
Il sistema ASP.NET Core Identity non ha i ruoli e i claims configurati correttamente nel database.

## ✅ Soluzione Implementata

### 1. Inizializzatore Database Automatico
Ho creato `DbInitializer.cs` che viene eseguito automaticamente all'avvio dell'applicazione e:

- ✅ Crea automaticamente i 3 ruoli (Admin, User, BotOperator)
- ✅ Crea un utente admin di default se non esiste
- ✅ Assegna il ruolo Admin all'utente admin
- ✅ Aggiunge tutti i claims/permessi necessari all'admin
- ✅ Verifica e corregge i permessi dell'admin ad ogni avvio

### 2. Credenziali Admin Default

**File: `appsettings.json` e `appsettings.Development.json`**

```json
{
  "Admin": {
    "Username": "admin",
    "Password": "Admin@123456",
    "Email": "admin@botdashboard.local"
  }
}
```

**⚠️ IMPORTANTE:** Cambia la password in produzione!

### 3. Migliorato il Servizio di Autenticazione

L'`AuthService` ora:
- Carica i claims dal database (UserManager.GetClaimsAsync)
- Include i claims nel JWT token
- Aggiunge permessi di fallback se mancano nel database
- Garantisce che il claim `isAdmin = "true"` sia presente per gli admin

## 🚀 Come Usare

### Opzione 1: Avvio Automatico (CONSIGLIATO)

1. **Avvia l'applicazione:**
   ```bash
   cd WebApi
   dotnet run
   ```

2. **L'inizializzatore viene eseguito automaticamente** al primo avvio e:
   - Crea i ruoli nel database
   - Crea l'utente admin con username: `admin` e password: `Admin@123456`
   - Configura tutti i permessi

3. **Effettua il login:**
   ```bash
   curl -X POST "http://localhost:5000/api/auth/login" \
     -H "Content-Type: application/json" \
     -d '{"username": "admin", "password": "Admin@123456"}'
   ```

4. **Riceverai un JWT token:**
   ```json
   {
     "success": true,
     "data": {
       "token": "eyJhbGc...",
       "expiration": "2025-12-06T12:00:00Z"
     }
   }
   ```

5. **Usa il token nelle chiamate API:**
   ```bash
   curl -X GET "http://localhost:5000/api/user" \
     -H "Authorization: Bearer eyJhbGc..."
   ```

### Opzione 2: Script SQL Manuale

Se preferisci configurare manualmente, usa lo script `setup-admin-permissions.sql`.

## 🔍 Verifica della Configurazione

### 1. Controlla i Ruoli nel Database

```sql
SELECT * FROM AspNetRoles;
```

Dovresti vedere:
```
Id | Name        | NormalizedName
1  | Admin       | ADMIN
2  | User        | USER
3  | BotOperator | BOTOPERATOR
```

### 2. Controlla l'Utente Admin

```sql
SELECT u.Id, u.UserName, u.Email, u.Admin
FROM AspNetUsers u
WHERE u.UserName = 'admin';
```

### 3. Controlla i Ruoli Assegnati

```sql
SELECT u.UserName, r.Name as RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'admin';
```

Dovresti vedere: `admin | Admin`

### 4. Controlla i Claims/Permessi

```sql
SELECT u.UserName, c.ClaimType, c.ClaimValue
FROM AspNetUsers u
JOIN AspNetUserClaims c ON u.Id = c.UserId
WHERE u.UserName = 'admin'
ORDER BY c.ClaimType, c.ClaimValue;
```

Dovresti vedere:
```
admin | isAdmin     | true
admin | permissions | user.read
admin | permissions | user.write
admin | permissions | user.delete
admin | permissions | bot.manage
admin | permissions | bot.view
admin | permissions | config.manage
```

### 5. Decodifica il JWT Token

Vai su [jwt.io](https://jwt.io) e incolla il tuo token per verificare i claims.

Dovresti vedere nel payload:
```json
{
  "sub": "1",
  "unique_name": "admin",
  "email": "admin@botdashboard.local",
  "userId": "1",
  "isAdmin": "true",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin",
  "permissions": [
    "user.read",
    "user.write",
    "user.delete",
    "bot.manage",
    "bot.view",
    "config.manage"
  ],
  "jti": "...",
  "exp": ...,
  "iss": "WebApi",
  "aud": "WebApiUsers"
}
```

## 🐛 Troubleshooting

### Problema: "Unauthorized" anche con token valido

**Soluzione:**
1. Verifica che il token non sia scaduto
2. Controlla che l'header sia: `Authorization: Bearer YOUR_TOKEN`
3. Verifica che il claim `isAdmin` sia `"true"` (stringa, non boolean)

```bash
# Ottieni un nuovo token
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "Admin@123456"}'
```

### Problema: Policy "RequireAdmin" fallisce

**Causa:** Manca il claim `isAdmin = "true"`

**Verifica:**
```sql
SELECT * FROM AspNetUserClaims 
WHERE UserId = (SELECT Id FROM AspNetUsers WHERE UserName = 'admin')
  AND ClaimType = 'isAdmin';
```

**Soluzione:**
```sql
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT Id, 'isAdmin', 'true'
FROM AspNetUsers
WHERE UserName = 'admin'
  AND NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims 
    WHERE UserId = AspNetUsers.Id AND ClaimType = 'isAdmin'
  );
```

Poi fai un nuovo login per ottenere un token aggiornato.

### Problema: "403 Forbidden" per endpoint specifici

**Causa:** Mancano permessi specifici

**Verifica quale policy richiede l'endpoint:**
```csharp
[Authorize(Policy = AuthConstants.Policies.RequireAdmin)]  // Richiede isAdmin = "true"
[Authorize(Policy = AuthConstants.Policies.RequireBotOperator)]  // Richiede permissions = "bot.manage"
```

**Aggiungi i permessi mancanti:**
```bash
POST /api/user/{userId}/permissions
Body: {"permission": "bot.manage"}
```

### Problema: Claims non visibili nel token

**Causa:** I claims sono nel database ma non vengono inclusi nel token

**Soluzione:** Ho aggiornato `AuthService.cs` per caricare automaticamente i claims dal database.

Se il problema persiste:
1. Riavvia l'applicazione
2. Fai un nuovo login
3. Verifica il token su jwt.io

### Problema: Database non inizializzato

**Sintomo:** Errori tipo "Role 'Admin' does not exist"

**Soluzione:**
```bash
# Riavvia l'applicazione, l'inizializzatore si occuperà di tutto
dotnet run

# Oppure applica le migrations
dotnet ef database update
```

### Problema: L'utente admin già esiste ma non ha permessi

**Soluzione Automatica:**
L'inizializzatore verifica e corregge i permessi ad ogni avvio.

**Soluzione Manuale:**
```bash
# Elimina l'utente esistente
DELETE FROM AspNetUserClaims WHERE UserId = (SELECT Id FROM AspNetUsers WHERE UserName = 'admin');
DELETE FROM AspNetUserRoles WHERE UserId = (SELECT Id FROM AspNetUsers WHERE UserName = 'admin');
DELETE FROM AspNetUsers WHERE UserName = 'admin';

# Riavvia l'applicazione per ricreare l'utente con i permessi corretti
```

## 📋 Checklist Completa

Prima di testare le API, assicurati che:

- [ ] ✅ Database SQL Server è in esecuzione
- [ ] ✅ Connection string corretta in `appsettings.json`
- [ ] ✅ Migrations applicate: `dotnet ef database update`
- [ ] ✅ Applicazione avviata: `dotnet run`
- [ ] ✅ Ruoli creati (verificare con query SQL sopra)
- [ ] ✅ Utente admin creato
- [ ] ✅ Claims assegnati all'admin
- [ ] ✅ Login effettuato con successo
- [ ] ✅ Token JWT ricevuto
- [ ] ✅ Token contiene i claims necessari (verificare su jwt.io)
- [ ] ✅ Token usato nell'header: `Authorization: Bearer TOKEN`

## 🔐 Modificare le Credenziali Admin

### In Sviluppo
Modifica `appsettings.Development.json`:
```json
{
  "Admin": {
    "Username": "tuousername",
    "Password": "TuaPassword@123",
    "Email": "tuaemail@example.com"
  }
}
```

### In Produzione
Usa variabili d'ambiente o Azure Key Vault:
```bash
export Admin__Username="prodadmin"
export Admin__Password="SuperSecurePassword@123"
export Admin__Email="admin@production.com"
```

## 📚 Riferimenti

- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [Claims-based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims)
- [Policy-based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)

## 🆘 Supporto

Se continui ad avere problemi:

1. Controlla i log dell'applicazione
2. Verifica il database con le query SQL fornite
3. Decodifica il JWT token su jwt.io
4. Controlla che CORS sia configurato correttamente
5. Verifica che la porta non sia bloccata dal firewall
