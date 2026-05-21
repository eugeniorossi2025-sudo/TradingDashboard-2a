# API Endpoints per Gestione Ruoli e Permessi

Documentazione completa degli endpoint per la gestione di ruoli e permessi nel BotDashboard.

## Endpoints Disponibili (Admin Only)

Tutti questi endpoint richiedono autenticazione con token JWT e policy `RequireAdmin`.

### 1. Visualizzare Ruoli e Permessi di un Utente

**GET** `/api/user/{userId}/roles-and-permissions`

Recupera tutti i ruoli e permessi assegnati a un utente specifico.

**Response:**
```json
{
  "success": true,
  "message": "User roles and permissions retrieved successfully",
  "data": {
    "userId": "1",
    "userName": "admin",
    "roles": ["Admin"],
    "permissions": [
      "user.read",
      "user.write",
      "user.delete",
      "bot.manage",
      "bot.view",
      "config.manage"
    ],
    "isAdmin": true
  }
}
```

### 2. Assegnare un Ruolo a un Utente

**POST** `/api/user/{userId}/roles`

Assegna un ruolo a un utente.

**Request Body:**
```json
{
  "roleName": "Admin"
}
```

**Ruoli Disponibili:**
- `Admin` - Amministratore con accesso completo
- `User` - Utente standard con accesso limitato
- `BotOperator` - Operatore bot con permessi intermedi

**Response:**
```json
{
  "success": true,
  "message": "Role 'Admin' assigned successfully",
  "data": {}
}
```

### 3. Rimuovere un Ruolo da un Utente

**DELETE** `/api/user/{userId}/roles/{roleName}`

Rimuove un ruolo da un utente.

**Example:** `DELETE /api/user/1/roles/Admin`

**Response:**
```json
{
  "success": true,
  "message": "Role 'Admin' removed successfully",
  "data": {}
}
```

### 4. Assegnare un Permesso a un Utente

**POST** `/api/user/{userId}/permissions`

Assegna un permesso (claim) a un utente.

**Request Body:**
```json
{
  "permission": "bot.manage"
}
```

**Permessi Disponibili:**
- `user.read` - Lettura utenti
- `user.write` - Creazione/modifica utenti
- `user.delete` - Eliminazione utenti
- `bot.manage` - Gestione completa bot
- `bot.view` - Solo visualizzazione bot
- `config.manage` - Gestione configurazioni

**Response:**
```json
{
  "success": true,
  "message": "Permission 'bot.manage' assigned successfully",
  "data": {}
}
```

### 5. Rimuovere un Permesso da un Utente

**DELETE** `/api/user/{userId}/permissions/{permission}`

Rimuove un permesso da un utente.

**Example:** `DELETE /api/user/1/permissions/bot.manage`

**Response:**
```json
{
  "success": true,
  "message": "Permission 'bot.manage' removed successfully",
  "data": {}
}
```

### 6. Ottenere tutti i Ruoli Disponibili

**GET** `/api/user/available-roles`

Restituisce l'elenco di tutti i ruoli disponibili nel sistema.

**Response:**
```json
{
  "success": true,
  "message": "Available roles retrieved successfully",
  "data": ["Admin", "User", "BotOperator"]
}
```

### 7. Ottenere tutti i Permessi Disponibili

**GET** `/api/user/available-permissions`

Restituisce l'elenco di tutti i permessi disponibili nel sistema.

**Response:**
```json
{
  "success": true,
  "message": "Available permissions retrieved successfully",
  "data": [
    "user.read",
    "user.write",
    "user.delete",
    "bot.manage",
    "bot.view",
    "config.manage"
  ]
}
```

## Esempi di Utilizzo con cURL

### Visualizzare ruoli e permessi
```bash
curl -X GET "https://api.example.com/api/user/1/roles-and-permissions" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Assegnare ruolo Admin
```bash
curl -X POST "https://api.example.com/api/user/1/roles" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"roleName": "Admin"}'
```

### Assegnare permesso bot.manage
```bash
curl -X POST "https://api.example.com/api/user/1/permissions" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"permission": "bot.manage"}'
```

### Rimuovere un ruolo
```bash
curl -X DELETE "https://api.example.com/api/user/1/roles/BotOperator" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Rimuovere un permesso
```bash
curl -X DELETE "https://api.example.com/api/user/1/permissions/user.delete" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Policy e Requisiti

### Policy Definite nel Sistema

1. **RequireAdmin**
   - Richiede: `isAdmin = "true"` claim
   - Accesso: Solo amministratori

2. **RequireUser**
   - Richiede: Utente autenticato
   - Accesso: Tutti gli utenti autenticati

3. **RequireBotOperator**
   - Richiede: `permissions` contenente `"bot.manage"`
   - Accesso: Operatori bot e admin

4. **RequireAdminOrBotOperator**
   - Richiede: `isAdmin = "true"` OPPURE `permissions` contenente `"bot.manage"`
   - Accesso: Admin o operatori bot

## Configurazione Completa per Admin

Per configurare un utente con tutti i permessi da admin:

1. **Assegnare ruolo Admin:**
   ```bash
   POST /api/user/{userId}/roles
   Body: {"roleName": "Admin"}
   ```

2. **Assegnare tutti i permessi:**
   ```bash
   POST /api/user/{userId}/permissions
   Body: {"permission": "user.read"}
   
   POST /api/user/{userId}/permissions
   Body: {"permission": "user.write"}
   
   POST /api/user/{userId}/permissions
   Body: {"permission": "user.delete"}
   
   POST /api/user/{userId}/permissions
   Body: {"permission": "bot.manage"}
   
   POST /api/user/{userId}/permissions
   Body: {"permission": "bot.view"}
   
   POST /api/user/{userId}/permissions
   Body: {"permission": "config.manage"}
   ```

Oppure utilizzare lo script SQL fornito in `setup-admin-permissions.sql`.

## Note Importanti

- Tutti questi endpoint richiedono il ruolo Admin
- I JWT token vengono aggiornati al prossimo login
- Le modifiche ai permessi sono immediate per le nuove richieste API
- Il claim `isAdmin` viene sincronizzato automaticamente con il ruolo Admin
- I permessi sono case-sensitive
