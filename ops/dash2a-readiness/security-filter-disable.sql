-- DASH2A — Disattiva Security Filter (solo flag, nessun deploy)
-- DB: Eugenio-Demo10 — colonna chiave [K]

SET NOCOUNT ON;

SELECT [K], [Value] FROM dbo.Configurations WHERE [K] = N'SECURITY_FILTER_ENABLED';

UPDATE dbo.Configurations
SET [Value] = N'0'
WHERE [K] = N'SECURITY_FILTER_ENABLED'
  AND [Value] <> N'0';

SELECT [K], [Value] FROM dbo.Configurations WHERE [K] = N'SECURITY_FILTER_ENABLED';

-- Dopo UPDATE: riavviare app pool Proactive sul Decisore (o attendere prossimo decide con reload config).
