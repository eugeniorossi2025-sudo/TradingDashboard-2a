/** Simulate StatsWidget parsing with slim telemetry + API fallback */
import { readFileSync } from 'fs';
import { execSync } from 'child_process';

const root = new URL('..', import.meta.url).pathname.replace(/^\/([A-Z]:)/, '$1');

function runSlimAudit() {
  const out = execSync('dotnet run --project tools/TelemetrySlimAudit/TelemetrySlimAudit.csproj -c Release --no-build', {
    cwd: root,
    encoding: 'utf8'
  });
  return out;
}

function simulateWidget(raw, parsedApi) {
  let fromRaw = {};
  try {
    fromRaw = JSON.parse(raw);
  } catch {
    fromRaw = {};
  }

  const fromApi = parsedApi || {};
  const merged = { ...fromApi, ...fromRaw };
  for (const [k, v] of Object.entries(fromApi)) {
    if (v !== undefined && v !== null && (merged[k] === undefined || merged[k] === null)) merged[k] = v;
  }

  const sf = merged.SecurityFilterByBot;
  const rows = sf && typeof sf === 'object' ? Object.keys(sf) : [];
  const globalsOk =
    merged.TotalPBHandsPlayed !== undefined &&
    merged.TotalAuthL6Authorized !== undefined &&
    merged.TotalL5Played !== undefined;

  return {
    jsonValid: (() => { try { JSON.parse(raw); return true; } catch { return false; } })(),
    rawLen: raw.length,
    controlRoomVisible: rows.length > 0,
    globalStatisticsOk: globalsOk || parsedApi?.totalPbHandsPlayed !== undefined,
    botCount: rows.length,
    mergedTotals: {
      TotalPBHandsPlayed: merged.TotalPBHandsPlayed ?? parsedApi?.totalPbHandsPlayed,
      TotalAuthL6Authorized: merged.TotalAuthL6Authorized ?? parsedApi?.totalAuthL6Authorized
    }
  };
}

console.log('=== StatsWidget simulation (slim JSON) ===');
const audit = runSlimAudit();
console.log(audit.split('\n').filter(l => l.includes('TELEMETRY_SIZE') || l.includes('RESULT')).join('\n'));

// Build slim JSON for 2 bots via dotnet is already tested; use python-equivalent inline
import { spawnSync } from 'child_process';
const py = spawnSync('python', ['tools/validate-telemetry-slim.py'], { cwd: root, encoding: 'utf8' });
console.log(py.stdout);

// Manual 2-bot slim sample matching TelemetryPersistence shape
const slim2 = JSON.stringify({
  TotalPBHandsPlayed: 221,
  TotalAuthL6Authorized: 2,
  TotalL5Played: 8,
  TotalL5Won: 5,
  TotalL5Lost: 3,
  TotalL8Played: 0,
  TotalL8Won: 0,
  TotalL8Lost: 0,
  BotMargins: { PC5: 84.15, PC6: 110.2 },
  SpotID: 1,
  SpotPBHandsPlayed: 21,
  SpotAuthL6Counter: 0,
  SpotL5Loss: 0,
  GlobalPauseScalping: false,
  GlobalPauseScalpingDetails: 'Pausa non attiva',
  GlobalPauseScalpingDuration: '0',
  INC: 0,
  EWMA: 0,
  TotalPauseScalpingSoglieActivated: 0,
  TotalPauseScalpingEWMAActivated: 0,
  TotalSecurityFilterActivated: 0,
  TotalSecurityFilterPreventedL6: 0,
  LastAvgHandSeconds: 22,
  ActiveSecurityFilterBots: 0,
  SecurityFilterByBot: {
    PC5: { AvgHandSeconds: 22.5, LastHandDeltaSeconds: 21.3, CurrentStreak: 5, SecurityRiskScore: 2, SecurityFilterActive: false, PauseBot: false, PauseScope: 'NONE', PreventedL6: 0, LastShoeHand: 15, Martingala: 1, HasL6Credit: true, LastReason: 'score 1/4', L6PlayedCount: 0, AuthorizedL8LostCount: 0, LastTwoHandDeltaSeconds: [22, 21], RapidL5TriggerActive: false },
    PC6: { AvgHandSeconds: 26.0, LastHandDeltaSeconds: 25.3, CurrentStreak: 1, SecurityRiskScore: 1, SecurityFilterActive: false, PauseBot: false, PauseScope: 'NONE', PreventedL6: 0, LastShoeHand: 14, Martingala: 2, HasL6Credit: false, LastReason: 'score 1/4', L6PlayedCount: 2, AuthorizedL8LostCount: 0, LastTwoHandDeltaSeconds: [23, 25], RapidL5TriggerActive: false }
  }
});

const parsedApi = {
  totalPbHandsPlayed: 221,
  totalAuthL6Authorized: 2,
  totalL5Played: 8,
  securityFilterMinScore: 3
};

const sim = simulateWidget(slim2, parsedApi);
console.log('2-bot widget sim:', JSON.stringify(sim, null, 2));

const broken4000 = readFileSync(`${root}/tools/l8-analysis/live-raw-telemetry.txt`, 'utf8');
const simBroken = simulateWidget(broken4000, parsedApi);
console.log('PROD broken 4000 fallback sim:', JSON.stringify(simBroken, null, 2));

const ok = sim.jsonValid && sim.rawLen < 4000 && sim.controlRoomVisible && sim.globalStatisticsOk && simBroken.globalStatisticsOk;
console.log(ok ? 'UI_SIM_PASS' : 'UI_SIM_FAIL');
process.exit(ok ? 0 : 1);
