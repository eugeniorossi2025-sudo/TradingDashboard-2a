# Guida Risoluzione Errori SignalR

## Problema 1: "Method does not exist - ExecuteCommand"

**Errore:**
```
Error invoking ExecuteCommand: Error: Failed to invoke 'ExecuteCommand' due to an error on the server. 
HubException: Method does not exist.
```

**Soluzione:** ✅ Il metodo `ExecuteCommand` è stato aggiunto al `DashboardHub.cs`

### Metodo Aggiunto
```csharp
public async Task<object> ExecuteCommand(string commandName, object? parameters = null)
```

### Utilizzo dal Client
```typescript
// Invocare il comando
const result = await connection.invoke('ExecuteCommand', 'commandName', { param1: 'value' });
console.log(result);

// Ascoltare la notifica broadcast
connection.on('CommandExecuted', (result) => {
    console.log('Command executed:', result);
});
```

---

## Problema 2: "No client method with the name 'receivedashboardupdate' found"

**Errore:**
```
Warning: No client method with the name 'receivedashboardupdate' found.
```

**Causa:** SignalR è **case-sensitive** per i nomi dei metodi. Il server sta inviando `ReceiveDashboardUpdate` (PascalCase) ma il client sta registrando `receivedashboardupdate` (tutto minuscolo).

### ❌ Codice Client ERRATO
```typescript
// SBAGLIATO - tutto minuscolo
connection.on('receivedashboardupdate', (data) => {
    console.log(data);
});
```

### ✅ Codice Client CORRETTO

**Opzione 1: Usa il nome esatto (PascalCase)**
```typescript
// CORRETTO - rispetta il case del server
connection.on('ReceiveDashboardUpdate', (data) => {
    console.log('Dashboard update received:', data);
});
```

**Opzione 2: Configurare SignalR per essere case-insensitive (nel client)**
```typescript
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/dashboardHub')
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Registra entrambe le versioni se necessario
connection.on('ReceiveDashboardUpdate', handleDashboardUpdate);
connection.on('receivedashboardupdate', handleDashboardUpdate); // fallback

function handleDashboardUpdate(data) {
    console.log('Dashboard update:', data);
}
```

---

## Metodi Disponibili nel DashboardHub

### Metodi Server (Client → Server)

#### 1. Echo
Test della connessione SignalR.
```typescript
const response = await connection.invoke('Echo', 'Hello Server!');
// Riceve: EchoResponse { original: 'Hello Server!', timestamp: '...' }
```

#### 2. RequestDashboardRefresh
Richiesta di aggiornamento immediato della dashboard.
```typescript
await connection.invoke('RequestDashboardRefresh');
// Riceve: RefreshRequested { message: 'Refresh request received' }
```

#### 3. SendDashboardUpdate
Invia un aggiornamento dashboard a tutti i client (richiede privilegi).
```typescript
await connection.invoke('SendDashboardUpdate', { 
    timestamp: Date.now(),
    data: { /* your data */ }
});
```

#### 4. ExecuteCommand ⭐ (NUOVO)
Esegue un comando sul server.
```typescript
const result = await connection.invoke('ExecuteCommand', 'start-bot', { 
    botId: 123 
});
console.log(result);
// { success: true, command: 'start-bot', message: '...', timestamp: '...' }
```

### Metodi Client (Server → Client)

#### 1. Connected
Ricevuto quando il client si connette con successo.
```typescript
connection.on('Connected', (data) => {
    console.log('Connected:', data.connectionId);
});
```

#### 2. ReceiveDashboardUpdate ⚠️ (Case Sensitive!)
Riceve aggiornamenti in tempo reale della dashboard.
```typescript
connection.on('ReceiveDashboardUpdate', (statistics) => {
    console.log('Stats:', statistics);
    // Aggiorna la UI
});
```

#### 3. EchoResponse
Risposta al metodo Echo.
```typescript
connection.on('EchoResponse', (data) => {
    console.log('Echo:', data.original, 'at', data.timestamp);
});
```

#### 4. RefreshRequested
Conferma richiesta di refresh.
```typescript
connection.on('RefreshRequested', (data) => {
    console.log(data.message);
});
```

#### 5. CommandExecuted ⭐ (NUOVO)
Notifica broadcast quando un comando viene eseguito.
```typescript
connection.on('CommandExecuted', (result) => {
    console.log('Command executed:', result.command);
    if (result.success) {
        // Gestisci successo
    } else {
        console.error('Command failed:', result.error);
    }
});
```

---

## Esempio Completo di Connessione SignalR

### TypeScript/JavaScript
```typescript
import * as signalR from '@microsoft/signalr';

class SignalRService {
    private connection: signalR.HubConnection;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('https://your-api.com/dashboardHub', {
                accessTokenFactory: () => this.getAuthToken()
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.setupEventHandlers();
    }

    private getAuthToken(): string {
        return localStorage.getItem('jwt_token') || '';
    }

    private setupEventHandlers(): void {
        // ⚠️ IMPORTANTE: Usa PascalCase (come definito nel server)
        this.connection.on('Connected', (data) => {
            console.log('✅ Connected to SignalR:', data.connectionId);
        });

        this.connection.on('ReceiveDashboardUpdate', (statistics) => {
            console.log('📊 Dashboard update:', statistics);
            // Aggiorna la tua UI qui
            this.updateDashboard(statistics);
        });

        this.connection.on('CommandExecuted', (result) => {
            console.log('⚡ Command executed:', result);
            if (!result.success) {
                console.error('Command error:', result.error);
            }
        });

        this.connection.on('EchoResponse', (data) => {
            console.log('🔊 Echo:', data);
        });

        this.connection.on('RefreshRequested', (data) => {
            console.log('🔄 Refresh:', data.message);
        });

        this.connection.onclose((error) => {
            console.error('❌ SignalR disconnected:', error);
        });

        this.connection.onreconnecting((error) => {
            console.warn('🔄 SignalR reconnecting...', error);
        });

        this.connection.onreconnected((connectionId) => {
            console.log('✅ SignalR reconnected:', connectionId);
        });
    }

    async start(): Promise<void> {
        try {
            await this.connection.start();
            console.log('✅ SignalR connection started');
        } catch (error) {
            console.error('❌ SignalR connection failed:', error);
            setTimeout(() => this.start(), 5000); // Retry dopo 5s
        }
    }

    async executeCommand(commandName: string, parameters?: any): Promise<any> {
        try {
            const result = await this.connection.invoke('ExecuteCommand', commandName, parameters);
            return result;
        } catch (error) {
            console.error('Error executing command:', error);
            throw error;
        }
    }

    async requestRefresh(): Promise<void> {
        await this.connection.invoke('RequestDashboardRefresh');
    }

    async testConnection(): Promise<void> {
        const response = await this.connection.invoke('Echo', 'Test message');
        console.log('Test response:', response);
    }

    private updateDashboard(statistics: any): void {
        // Implementa qui la logica per aggiornare la UI
        // Esempio:
        // document.getElementById('active-bots').textContent = statistics.activeBots;
        // document.getElementById('total-commands').textContent = statistics.totalCommands;
    }
}

// Utilizzo
const signalR = new SignalRService();
signalR.start();

// Esegui un comando
signalR.executeCommand('start-bot', { botId: 123 });
```

---

## Checklist Risoluzione Problemi

- [x] ✅ Aggiunto metodo `ExecuteCommand` al DashboardHub
- [ ] ⚠️ Correggere il nome del metodo client da `receivedashboardupdate` a `ReceiveDashboardUpdate`
- [ ] ⚠️ Verificare che il client stia usando i nomi corretti (case-sensitive)
- [ ] ⚠️ Controllare che il token JWT sia valido e incluso nelle richieste SignalR

---

## Testing

### Test dalla Console del Browser
```javascript
// Dopo aver connesso SignalR
connection.invoke('Echo', 'Test').then(result => console.log(result));

connection.invoke('ExecuteCommand', 'test-command', { test: true })
    .then(result => console.log('Result:', result))
    .catch(error => console.error('Error:', error));
```

### Test con cURL (API REST)
```bash
# Login per ottenere token
curl -X POST "https://your-api.com/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'

# Usa il token nelle richieste successive
```

---

## Note Importanti

1. **Case Sensitivity:** SignalR è case-sensitive! `ReceiveDashboardUpdate` ≠ `receivedashboardupdate`
2. **Autenticazione:** Assicurati di passare il JWT token nella connessione SignalR
3. **CORS:** Verifica che il backend abbia configurato correttamente CORS per SignalR
4. **WebSocket:** SignalR usa WebSocket quando possibile, altrimenti fallback a Long Polling

## Riferimenti
- [SignalR JavaScript Client](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [SignalR Hub Methods](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs)
