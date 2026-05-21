# Sistema di Gestione Errori e Permessi

## Implementazione Completata

### ✅ Pagine Create

1. **`/src/views/pages/auth/AccessDenied.vue`** - Pagina 403
   - Design professionale con icona lock
   - Mostra username dell'utente corrente
   - Pulsanti: "Torna Indietro" e "Vai alla Dashboard"
   - Messaggio chiaro: "Contatta l'amministratore"

### ✅ Gestione Errori API (apiClient.ts)

**401 Unauthorized:**
```typescript
// Token scaduto o non valido
sessionStorage.removeItem('authToken');
sessionStorage.removeItem('currentUser');

// Salva URL corrente per redirect post-login
sessionStorage.setItem('redirectAfterLogin', currentPath);

// Redirect → /auth/login
```

**403 Forbidden:**
```typescript
// Autenticato ma senza permessi
console.warn('Access denied: 403 Forbidden');

// Redirect → /auth/access-denied
```

### ✅ Router Guards Aggiornati

Tutti i guard ora reindirizzano a **`/auth/access-denied`** invece della dashboard:

```javascript
// Prima
next({ name: 'dashboard' }); // ❌

// Ora
next({ name: 'access-denied' }); // ✅
```

**Guards aggiornati:**
- `adminGuard` → 403 se non admin
- `botOperatorGuard` → 403 se non bot operator
- `policyGuard(policy)` → 403 se policy non soddisfatta
- `permissionGuard(...perms)` → 403 se mancano permessi
- `roleGuard(...roles)` → 403 se mancano ruoli

### ✅ Login con Redirect Intelligente

```javascript
// 1. Utente prova ad accedere a /pages/user (senza permessi)
// 2. Guard intercetta → redirect /auth/login?redirect=/pages/user
// 3. Login salva redirect in sessionStorage
// 4. Dopo login → torna a /pages/user (o 403 se ancora senza permessi)
```

### ✅ Protezione Array Undefined

**AuthService.ts:**
```typescript
// Prima
currentUser.value.permissions.includes(permission); // ❌ Crash se undefined

// Ora
if (!currentUser.value || !currentUser.value.permissions) return false; // ✅
```

**loadUserFromStorage():**
```typescript
currentUser.value = { 
    ...user, 
    token,
    roles: user.roles || [],           // ✅ Sempre array
    permissions: user.permissions || [] // ✅ Sempre array
};
```

## Flussi Implementati

### Scenario 1: Utente Non Autenticato

```
Utente naviga → /pages/user
         ↓
Router Guard (authGuard)
         ↓
Redirect → /auth/login?redirect=/pages/user
         ↓
Utente fa login
         ↓
Redirect → /pages/user (se ha permessi)
         ↓ (oppure)
Redirect → /auth/access-denied (se manca Users.View)
```

### Scenario 2: API Restituisce 401

```
Frontend → API Call
         ↓
Backend → 401 Unauthorized (token scaduto)
         ↓
Axios Interceptor
         ↓
sessionStorage.clear()
sessionStorage.setItem('redirectAfterLogin', currentPath)
         ↓
window.location.href = '/auth/login'
         ↓
Utente fa login
         ↓
Redirect → URL salvato (o dashboard)
```

### Scenario 3: API Restituisce 403

```
Frontend → API Call (es. DELETE /users/123)
         ↓
Backend → 403 Forbidden (utente non admin)
         ↓
Axios Interceptor
         ↓
console.warn('Access denied: 403')
         ↓
window.location.href = '/auth/access-denied'
         ↓
Pagina 403 con messaggio e pulsanti
```

### Scenario 4: Tentativo di Navigazione Senza Permessi

```
Utente autenticato (ruolo: Viewer)
         ↓
Clicca link → /pages/console
         ↓
Router Guard (adminGuard)
         ↓
AuthService.checkPolicy('RequireAdmin') → false
         ↓
next({ name: 'access-denied' })
         ↓
Pagina 403 con username visibile
```

## Come Testare

### Test 1: Login e Redirect
```bash
1. Logout (se autenticato)
2. Naviga manualmente a: http://localhost:5173/pages/user
3. Dovresti essere reindirizzato a /auth/login
4. Fai login
5. Dovresti tornare automaticamente a /pages/user
```

### Test 2: 401 da API
```bash
1. Fai login
2. Apri DevTools → Application → Storage
3. Elimina manualmente 'authToken'
4. Esegui una chiamata API (es. carica utenti)
5. Dovresti essere reindirizzato al login
6. Dopo login, torni all'URL precedente
```

### Test 3: 403 da Router Guard
```bash
1. Fai login come utente NON admin (senza Users.View)
2. Prova a navigare a /pages/user
3. Dovresti vedere la pagina 403
4. Clicca "Vai alla Dashboard" → torni a /
```

### Test 4: 403 da API
```bash
# Simula una chiamata API che restituisce 403
# (questo dipende dal backend - se implementato)

1. Fai login come utente con permessi limitati
2. Prova a eseguire un'azione non consentita
3. Se il BE risponde 403 → redirect a /auth/access-denied
```

### Test 5: Menu Dinamico
```bash
1. Fai login come Admin
   → Dovresti vedere tutte le voci del menu

2. Logout e login come User
   → Dovresti vedere solo Dashboard

3. Logout e login come BotOperator
   → Dovresti vedere Dashboard + sezioni bot
```

## Sicurezza

### ✅ Implementato
- JWT parsing con estrazione ruoli e permessi
- Guards router per tutte le route protette
- Intercettori Axios per 401/403
- UI nasconde elementi senza permessi (`v-permission`)
- Funzioni JS verificano permessi prima delle azioni
- Pagina 403 dedicata per accesso negato
- Redirect intelligente post-login

### ⚠️ Importante
**Il frontend è solo UX!**

Tutti i controlli devono essere ripetuti nel backend:
```csharp
// Backend C#
[Authorize(Policy = "RequireAdmin")]
public async Task<IActionResult> DeleteUser(int id)
{
    // Verifica autorizzazione
    // Logica di business
}
```

## Troubleshooting

### Errore: "Cannot read properties of undefined (reading 'includes')"
✅ **RISOLTO** - Aggiunto controllo null su permissions e roles

### Errore: "Redirect loop"
Controlla che:
- `/auth/login` abbia `guestGuard` (non `authGuard`)
- `/auth/access-denied` NON abbia guard
- I guard non reindirizzino a se stessi

### Errore: "Non torna alla pagina dopo login"
Verifica che:
- `sessionStorage.getItem('redirectAfterLogin')` sia valorizzato
- Login.vue legga e rimuova questo valore
- Router guard salvino correttamente il path

### Menu non si aggiorna dopo login
```javascript
// Forza reload
AuthService.reloadUser();

// O ricarica la pagina
window.location.reload();
```

## Estensioni Future

### Possibili Miglioramenti
1. **Toast notification** per 403 invece di redirect completo
2. **Modal** di conferma prima di redirect a 403
3. **Logging** degli accessi negati per audit
4. **Rate limiting** sul frontend
5. **Session timeout warning** prima del 401
6. **Breadcrumb** nella pagina 403 per capire da dove vieni

### Metriche da Monitorare
- Numero di 403 per utente (possibile attacco)
- Pagine più protette (più 403)
- Tempo medio tra login e primo 401 (session lifetime)
