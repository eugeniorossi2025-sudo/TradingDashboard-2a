# Fix 401 Unauthorized - Guida Rapida

## Problema: 401 Unauthorized durante il login

### ✅ Soluzioni Applicate

1. **Aggiunto `[AllowAnonymous]` a livello di AuthController**
   - Tutti gli endpoint di autenticazione ora NON richiedono token
   - `/api/auth/login` è accessibile senza autenticazione
   - `/api/auth/test` endpoint di verifica aggiunto

2. **Verificato ordine middleware in Program.cs**
   - `UseAuthentication()` viene prima di `UseAuthorization()` ✅
   - CORS configurato correttamente ✅

3. **Endpoint di Test Aggiunto**
   - `GET /api/auth/test` - Verifica che l'API risponda

## 🧪 Test Rapido

### Opzione 1: Script Automatico
```bash
./test-auth.sh
```

### Opzione 2: cURL Manuale

#### 1. Test endpoint pubblico
```bash
curl http://localhost:5000/api/auth/test
```
**Risposta attesa:** 200 OK con messaggio di test

#### 2. Test login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "Admin@123456"}'
```
**Risposta attesa:** 200 OK con token JWT

#### 3. Test chiamata autenticata
```bash
# Salva il token dalla risposta precedente
TOKEN="il_tuo_token_jwt"

curl http://localhost:5000/api/user \
  -H "Authorization: Bearer $TOKEN"
```
**Risposta attesa:** 200 OK con lista utenti

### Opzione 3: Swagger UI
1. Apri browser: `http://localhost:5000/swagger`
2. Espandi `POST /api/auth/login`
3. Click "Try it out"
4. Inserisci:
   ```json
   {
     "username": "admin",
     "password": "Admin@123456"
   }
   ```
5. Click "Execute"
6. Copia il token dalla risposta
7. Click su "Authorize" (icona lucchetto in alto)
8. Incolla: `Bearer IL_TUO_TOKEN`
9. Prova altri endpoint

## 🔍 Diagnosi Problemi

### Se ancora ottieni 401 sul /login

#### Problema 1: Server non avviato o porta errata
```bash
# Verifica che il server sia in esecuzione
curl http://localhost:5000/api/auth/test

# Se fallisce, controlla il processo
ps aux | grep dotnet

# Avvia il server
cd WebApi
dotnet run
```

#### Problema 2: CORS blocking (dal browser)
**Sintomo:** Errore CORS nella console del browser

**Soluzione:** Già configurato `AllowAll` in Program.cs, ma verifica:
```csharp
app.UseCors("AllowAll");  // Deve essere PRIMA di UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
```

#### Problema 3: Database non inizializzato
```bash
# Verifica che il database esista
# Controlla i log all'avvio per vedere se DbInitializer è stato eseguito

# Dovresti vedere nei log:
# "Role 'Admin' created successfully"
# "Admin user 'admin' created successfully"
```

#### Problema 4: Password errata
Le credenziali di default sono:
- **Username:** `admin`
- **Password:** `Admin@123456`

Verifica in `appsettings.json`:
```json
{
  "Admin": {
    "Username": "admin",
    "Password": "Admin@123456",
    "Email": "admin@botdashboard.local"
  }
}
```

#### Problema 5: Token JWT configuration
Verifica in `appsettings.json` che la chiave JWT sia presente:
```json
{
  "Jwt": {
    "Key": "...una lunga stringa...",
    "Issuer": "WebApi",
    "Audience": "WebApiUsers",
    "ExpirationMinutes": "60"
  }
}
```

### Se ottieni 401 sugli altri endpoint (dopo login)

#### Problema 1: Token non incluso nell'header
**Errato:**
```bash
curl http://localhost:5000/api/user \
  -H "Token: eyJhbGc..."
```

**Corretto:**
```bash
curl http://localhost:5000/api/user \
  -H "Authorization: Bearer eyJhbGc..."
```

#### Problema 2: Token scaduto
I token JWT scadono dopo 60 minuti. Fai un nuovo login:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "Admin@123456"}'
```

#### Problema 3: Claims mancanti
Verifica che il token contenga i claims necessari su [jwt.io](https://jwt.io):

Copia il token e incollalo su jwt.io. Dovresti vedere:
```json
{
  "userId": "1",
  "isAdmin": "true",
  "permissions": ["user.read", "user.write", "user.delete", "bot.manage", "bot.view", "config.manage"],
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin"
}
```

Se mancano claims, riesegui lo script SQL o riavvia l'app per far girare il DbInitializer.

## 📊 Verifica Database

```sql
-- Verifica utente admin esiste
SELECT * FROM AspNetUsers WHERE UserName = 'admin';

-- Verifica ruolo assegnato
SELECT u.UserName, r.Name 
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'admin';

-- Verifica claims
SELECT ClaimType, ClaimValue 
FROM AspNetUserClaims 
WHERE UserId = (SELECT Id FROM AspNetUsers WHERE UserName = 'admin');
```

## 🐛 Debug con Logs

Attiva logging dettagliato in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.AspNetCore.Authentication": "Debug",
      "Microsoft.AspNetCore.Authorization": "Debug"
    }
  }
}
```

Riavvia e controlla i log per vedere cosa succede durante l'autenticazione.

## ✅ Checklist Finale

- [ ] Server in esecuzione su porta corretta
- [ ] Database SQL Server connesso
- [ ] DbInitializer eseguito (controlla log)
- [ ] Utente admin creato nel database
- [ ] Ruolo Admin assegnato
- [ ] Claims configurati
- [ ] `/api/auth/test` risponde con 200
- [ ] `/api/auth/login` risponde con 200 e token
- [ ] Token contiene claims corretti (verificato su jwt.io)
- [ ] Header `Authorization: Bearer TOKEN` usato correttamente
- [ ] Altri endpoint rispondono con 200 usando il token

## 🆘 Se Nulla Funziona

1. **Ricrea completamente il database:**
   ```bash
   cd WebApi
   dotnet ef database drop -f
   dotnet ef database update
   dotnet run
   ```

2. **Verifica connection string** in `appsettings.json`

3. **Controlla firewall/antivirus** che non blocchi la porta 5000

4. **Prova porta diversa** in `launchSettings.json`

5. **Controlla i log completi** per errori durante l'avvio

## 📞 Test da Postman

1. New Request → POST
2. URL: `http://localhost:5000/api/auth/login`
3. Headers: `Content-Type: application/json`
4. Body (raw JSON):
   ```json
   {
     "username": "admin",
     "password": "Admin@123456"
   }
   ```
5. Send → Dovresti ricevere 200 OK
6. Copia il token dalla risposta
7. Nuova richiesta → GET `http://localhost:5000/api/user`
8. Headers → Add `Authorization: Bearer IL_TUO_TOKEN`
9. Send → Dovresti ricevere 200 OK con lista utenti
