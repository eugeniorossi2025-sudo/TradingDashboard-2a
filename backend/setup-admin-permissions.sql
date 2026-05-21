-- ============================================================================
-- Script per configurare tutti i permessi per l'utente Admin
-- ============================================================================
-- Questo script configura un utente admin con tutti i ruoli e permessi necessari
-- per accedere a tutte le policy dell'applicazione.
--
-- ISTRUZIONI:
-- 1. Trova l'ID del tuo utente admin con la query sottostante
-- 2. Sostituisci 'TUO_USER_ID' con l'ID effettivo
-- 3. Esegui lo script
-- ============================================================================

-- Query per trovare l'ID dell'utente admin
SELECT Id, UserName, Email, Admin FROM AspNetUsers WHERE UserName = 'admin';

-- ============================================================================
-- PARTE 1: CONFIGURAZIONE RUOLI
-- ============================================================================

-- Verifica che i ruoli esistano nella tabella AspNetRoles
SELECT * FROM AspNetRoles;

-- Se i ruoli non esistono, inseriscili (in genere vengono creati automaticamente dall'applicazione)
-- INSERT INTO AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
-- VALUES 
--     ('Admin', 'ADMIN', NEWID()),
--     ('User', 'USER', NEWID()),
--     ('BotOperator', 'BOTOPERATOR', NEWID());

-- Assegna il ruolo Admin all'utente (sostituisci TUO_USER_ID e RoleId appropriati)
-- Prima trova l'ID del ruolo Admin
SELECT Id, Name FROM AspNetRoles WHERE Name = 'Admin';

-- Assegna il ruolo (sostituisci i valori)
-- Esempio: se UserId = 1 e AdminRoleId = 1
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT 1, Id FROM AspNetRoles WHERE Name = 'Admin'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles 
    WHERE UserId = 1 AND RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin')
);

-- ============================================================================
-- PARTE 2: CONFIGURAZIONE CLAIMS (Permessi)
-- ============================================================================
-- I claims vengono utilizzati dalle Authorization Policies

-- CLAIM: isAdmin = "true"
-- Necessario per: RequireAdmin policy
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'isAdmin', 'true'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims 
    WHERE UserId = 1 AND ClaimType = 'isAdmin'
);

-- ============================================================================
-- CLAIM: permissions
-- Questi sono tutti i permessi definiti nel sistema
-- ============================================================================

-- Permesso: user.read - Lettura utenti
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'user.read'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims 
    WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'user.read'
);

-- Permesso: user.write - Scrittura/creazione utenti
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'user.write'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims 
    WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'user.write'
);

-- Permesso: user.delete - Eliminazione utenti
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'user.delete'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims 
    WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'user.delete'
);

-- Permesso: bot.manage - Gestione completa bot
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'bot.manage'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims 
    WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'bot.manage'
);

-- Permesso: bot.view - Solo visualizzazione bot (per utenti standard)
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'bot.view'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims 
    WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'bot.view'
);

-- Permesso: config.manage - Gestione configurazioni
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'config.manage'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserClaims 
    WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'config.manage'
);

-- ============================================================================
-- PARTE 3: AGGIORNA IL CAMPO Admin NELLA TABELLA Users
-- ============================================================================

UPDATE AspNetUsers
SET Admin = 1
WHERE Id = 1;

-- ============================================================================
-- VERIFICA FINALE
-- ============================================================================

-- Verifica i ruoli assegnati
SELECT 
    u.Id, 
    u.UserName, 
    u.Email, 
    u.Admin,
    r.Name AS RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Id = 1;

-- Verifica tutti i claims/permessi
SELECT 
    u.Id,
    u.UserName,
    uc.ClaimType,
    uc.ClaimValue
FROM AspNetUsers u
LEFT JOIN AspNetUserClaims uc ON u.Id = uc.UserId
WHERE u.Id = 1
ORDER BY uc.ClaimType, uc.ClaimValue;

-- ============================================================================
-- RIEPILOGO POLICY ATTIVE
-- ============================================================================
-- Con questa configurazione, l'utente admin avrà accesso a:
--
-- ✓ RequireAdmin - richiede claim isAdmin = "true"
-- ✓ RequireUser - richiede solo autenticazione (automatico con login)
-- ✓ RequireBotOperator - richiede permissions = "bot.manage"
-- ✓ RequireAdminOrBotOperator - richiede isAdmin=true OPPURE permissions="bot.manage"
--
-- PERMESSI COMPLETI:
-- - user.read      : Lettura utenti
-- - user.write     : Creazione/modifica utenti
-- - user.delete    : Eliminazione utenti
-- - bot.manage     : Gestione completa bot
-- - bot.view       : Visualizzazione bot
-- - config.manage  : Gestione configurazioni
-- ============================================================================

-- ============================================================================
-- SCRIPT RAPIDO - Copia e incolla sostituendo UserId
-- ============================================================================
/*
-- Sostituisci '1' con il tuo UserId effettivo

-- Assegna ruolo Admin
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT 1, Id FROM AspNetRoles WHERE Name = 'Admin'
WHERE NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 1 AND RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin'));

-- Claims
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'isAdmin', 'true' WHERE NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 1 AND ClaimType = 'isAdmin');

INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'user.read' WHERE NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'user.read');

INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'user.write' WHERE NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'user.write');

INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'user.delete' WHERE NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'user.delete');

INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'bot.manage' WHERE NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'bot.manage');

INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'bot.view' WHERE NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'bot.view');

INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT 1, 'permissions', 'config.manage' WHERE NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 1 AND ClaimType = 'permissions' AND ClaimValue = 'config.manage');

-- Aggiorna campo Admin
UPDATE AspNetUsers SET Admin = 1 WHERE Id = 1;
*/
