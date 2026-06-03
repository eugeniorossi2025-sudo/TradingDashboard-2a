-- UI-only PLAYER P1→P5 threshold (Decisore config dictionary key)
-- Value must be numeric string, e.g. 107
-- Invalid/missing -> engine fallback 107

-- Example upsert (adjust table/schema to your Config store):
/*
IF NOT EXISTS (SELECT 1 FROM Config WHERE [K] = N'SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS')
    INSERT INTO Config ([K], [V]) VALUES (N'SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS', N'107');
ELSE
    UPDATE Config SET [V] = N'107' WHERE [K] = N'SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS';
*/

-- JSON mission payload fragment (no trailing commas):
-- "SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS": "107"
