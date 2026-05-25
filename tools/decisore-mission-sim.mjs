/**
 * Simulazione missione Gamebot → Decisore + mirror live dashboard DASH2A (PC96).
 * Legge app.config del bot (Dashboard.Url, credenziali, PC, tavolo).
 *
 * Uso base (solo Decisore):
 *   node tools/decisore-mission-sim.mjs
 *
 * Collaudo live su https://eugenio-dashboard-2a.web.app/ (PC96 in tabella):
 *   set COLLAUDO_MIRROR_SECRET=...
 *   set DASH2A_API_URL=https://vps-b0942869.vps.ovh.net
 *   node tools/decisore-mission-sim.mjs --live-dashboard
 *
 * Env:
 *   BOT_APP_CONFIG          path app.config bot
 *   COLLAUDO_MIRROR_SECRET  secret mirror WebApi (obbligatorio con --live-dashboard)
 *   DASH2A_API_URL          base WebApi dashboard (default HTTPS prod)
 *
 * NON chiama /reset. NON cleanup automatico. NON tocca DB Decisore direttamente.
 */

import { readFileSync, writeFileSync, mkdirSync } from 'fs';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const DEFAULT_CONFIG = 'C:\\Users\\eugen\\Desktop\\BOTITALIA\\app.config';
const DEFAULT_DASH2A_API = 'https://vps-b0942869.vps.ovh.net';
const MIRROR_HEADER = 'X-Collaudo-Mirror-Secret';
const LIVE_STEP_MS = 1500;

const DECIDE_MEANINGS = {
  0: 'Nulla',
  1: 'Stop PC',
  2: 'Azzera Martingala',
  3: 'Pausa Scalping Forzata',
};

function parseArgs(argv) {
  const out = {
    config: process.env.BOT_APP_CONFIG || DEFAULT_CONFIG,
    hands: 10,
    liveDashboard: false,
    dashApi: (process.env.DASH2A_API_URL || DEFAULT_DASH2A_API).replace(/\/+$/, ''),
    mirrorSecret: process.env.COLLAUDO_MIRROR_SECRET || '',
  };
  for (let i = 2; i < argv.length; i++) {
    if (argv[i] === '--config' && argv[i + 1]) out.config = argv[++i];
    else if (argv[i] === '--hands' && argv[i + 1]) out.hands = Number(argv[++i]) || 10;
    else if (argv[i] === '--live-dashboard') out.liveDashboard = true;
    else if (argv[i] === '--dash-api' && argv[i + 1]) out.dashApi = argv[++i].replace(/\/+$/, '');
    else if (argv[i] === '--help' || argv[i] === '-h') out.help = true;
  }
  return out;
}

function parseAppConfig(xml) {
  const get = (key) => {
    const m = xml.match(new RegExp(`key="${key}"\\s+value="([^"]*)"`, 'i'));
    return m ? m[1] : '';
  };
  const url = get('Dashboard.Url').replace(/\/+$/, '');
  return {
    url,
    urlDev: get('Dashboard.UrlDev'),
    username: get('Dashboard.Username'),
    password: get('Dashboard.Password'),
    account: get('Value.Account'),
    computer: get('Value.Computer'),
    tavolo: get('Value.Tavolo'),
  };
}

function validateConfig(cfg) {
  const errors = [];
  if (!cfg.url) errors.push('Dashboard.Url mancante');
  if (cfg.url.includes('51.83.159.175')) errors.push('Dashboard.Url punta a WebApi dashboard — deve essere Decisore 51.178.16.37');
  if (cfg.url.includes('51.210.181.37')) errors.push('Dashboard.Url obsoleto 51.210.181.37');
  if (!cfg.username || !cfg.password) errors.push('Username/Password mancanti');
  if (!cfg.computer) errors.push('Value.Computer mancante');
  if (cfg.computer !== 'PC96') errors.push(`Value.Computer deve essere PC96 (trovato: ${cfg.computer})`);
  return errors;
}

async function postForm(base, path, fields) {
  const body = new URLSearchParams(fields);
  const res = await fetch(`${base}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body,
  });
  const text = await res.text();
  let json = null;
  try {
    json = JSON.parse(text);
  } catch {
    /* plain */
  }
  return { status: res.status, ok: res.ok, body: text.slice(0, 500), json };
}

async function getDecide(base, params) {
  const qs = new URLSearchParams(params).toString();
  const res = await fetch(`${base}/api/proactive/decide?${qs}`, { method: 'GET' });
  const text = (await res.text()).trim();
  const cmd = /^\d+$/.test(text) ? Number(text) : null;
  return { status: res.status, ok: res.ok, body: text.slice(0, 200), command: cmd };
}

function buildMirrorPayload(cfg, { margine, martingala, mazzo, stato, pbt, saldoIniziale = 1000, ore = 0.033 }) {
  const saldoIstantaneo = saldoIniziale + margine;
  return {
    computer: cfg.computer,
    account: cfg.account,
    tavolo: cfg.tavolo,
    saldoIniziale,
    saldoIstantaneo,
    margine,
    valoreGiocato: 10,
    colpoMartingala: martingala + 1,
    stato: stato || 'ATTESA',
    mazzo: String(mazzo),
    pbt: pbt || ' ',
    ore,
    colore: '',
  };
}

async function mirrorToDashboard(dashApi, secret, payload) {
  const res = await fetch(`${dashApi}/api/Collaudo/mirror-pc-status`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      [MIRROR_HEADER]: secret,
    },
    body: JSON.stringify(payload),
  });
  const text = await res.text();
  let json = null;
  try {
    json = JSON.parse(text);
  } catch {
    /* plain */
  }
  const ok = res.ok && json?.success !== false;
  return { status: res.status, ok, body: text.slice(0, 400), json };
}

async function verifyDashboardPc(dashApi, secret, computer, expected) {
  const res = await fetch(`${dashApi}/api/Collaudo/pc-status/${encodeURIComponent(computer)}`, {
    headers: { [MIRROR_HEADER]: secret },
  });
  const text = await res.text();
  let json = null;
  try {
    json = JSON.parse(text);
  } catch {
    /* plain */
  }
  const row = json?.data ?? json?.Data ?? null;
  const ok =
    res.ok &&
    row &&
    String(row.computer ?? row.Computer) === computer &&
    Math.abs(Number(row.margine ?? row.Margine) - Number(expected.margine)) < 0.02 &&
    String(row.mazzo ?? row.Mazzo) === String(expected.mazzo) &&
    String(row.stato ?? row.Stato).toUpperCase() === String(expected.stato).toUpperCase();

  return {
    ok,
    status: res.status,
    row,
    body: text.slice(0, 400),
  };
}

function buildDecideParams(cfg, pbt, margine, martingala, mazzo) {
  const saldoIniziale = '1000';
  const saldoIstantaneo = String(1000 + margine);
  return {
    USERNAME: cfg.username,
    PASSWORD: cfg.password,
    COMPUTER: cfg.computer,
    ACCOUNT: cfg.account,
    TAVOLO: cfg.tavolo,
    SALDO_INIZIALE: saldoIniziale,
    SALDO_ISTANTANEO: saldoIstantaneo,
    MARGINE: String(margine),
    STATO: 'ATTESA',
    COLPO_MARTINGALA: String(martingala),
    VINCITA: '0',
    MAZZO: String(mazzo),
    TEMPO: '00:02',
    AVVIO: '1',
    VALORE_GIOCATO: '10',
    PBT: pbt,
    CHOSEN_COLOR: '',
  };
}

const DEFAULT_PBT_SEQUENCE = ['P', 'B', 'P', 'P', 'B', 'T', 'B', 'P', 'B', 'P'];

async function runSimulation(cfg, handCount, liveOpts) {
  const base = cfg.url;
  const report = {
    generatedAt: new Date().toISOString(),
    configPath: cfg._configPath,
    decisoreBaseUrl: base,
    dash2aApiUrl: liveOpts?.dashApi ?? null,
    liveDashboard: Boolean(liveOpts?.enabled),
    botIdentity: {
      username: cfg.username,
      computer: cfg.computer,
      account: cfg.account,
      tavolo: cfg.tavolo,
    },
    steps: [],
    decideCalls: [],
    mirrorCalls: [],
    dashboardVerifications: [],
    errors: [],
  };

  const profit0 = await postForm(base, '/api/proactive/get-global-profit', {
    USERNAME: cfg.username,
    PASSWORD: cfg.password,
    COMPUTER: cfg.computer,
  });
  report.steps.push({ step: 'A-get-global-profit-initial', ...profit0 });
  if (!profit0.ok) report.errors.push(`get-global-profit initial HTTP ${profit0.status}`);

  const authOk =
    profit0.ok &&
    (profit0.json?.margine !== undefined ||
      profit0.json?.Margine !== undefined ||
      profit0.json?.saldoIniziale !== undefined ||
      profit0.json?.SaldoIniziale !== undefined ||
      profit0.status === 200);

  const initParams = {
    USERNAME: cfg.username,
    PASSWORD: cfg.password,
    COMPUTER: cfg.computer,
    TAVOLO: cfg.tavolo,
    SALDO_INIZIALE: '1000',
    SALDO_ISTANTANEO: '1000',
    MARGINE: '0',
    VALORE_GIOCATO: '0',
    COLPO_MARTINGALA: '0',
    MAZZO: '1',
    STATO: 'ATTESA',
    TEMPO: '00:01',
    CHOSEN_COLOR: '',
  };
  const updateInit = await postForm(base, '/api/proactive/update-params', initParams);
  report.steps.push({ step: 'B-update-params-initial', ...updateInit });
  if (!updateInit.ok) report.errors.push(`update-params initial HTTP ${updateInit.status}`);

  let lastMirrorState = { margine: 0, martingala: 0, mazzo: 1, stato: 'ATTESA', pbt: ' ' };

  async function doMirror(state, label) {
    if (!liveOpts?.enabled) return null;
    const payload = buildMirrorPayload(cfg, state);
    const mirror = await mirrorToDashboard(liveOpts.dashApi, liveOpts.secret, payload);
    const entry = { label, ...mirror, payload };
    report.mirrorCalls.push(entry);
    if (!mirror.ok) report.errors.push(`mirror ${label}: HTTP ${mirror.status}`);
    await new Promise((r) => setTimeout(r, LIVE_STEP_MS));
    return mirror;
  }

  if (liveOpts?.enabled) {
    await doMirror({ ...lastMirrorState, pbt: 'P' }, 'mirror-initial');
  }

  const sequence = DEFAULT_PBT_SEQUENCE.slice(0, handCount);
  while (sequence.length < handCount) {
    sequence.push(DEFAULT_PBT_SEQUENCE[sequence.length % DEFAULT_PBT_SEQUENCE.length]);
  }

  let margine = 0;
  let martingala = 0;
  for (let i = 0; i < handCount; i++) {
    const pbt = sequence[i];
    const mazzo = i + 1;
    if (pbt === 'T') margine += 0;
    else if (i % 3 === 0) {
      margine += 5;
      martingala = 0;
    } else {
      margine -= 3;
      martingala += 1;
    }

    const params = buildDecideParams(cfg, pbt, margine, martingala, mazzo);
    const decide = await getDecide(base, params);
    const entry = {
      hand: i + 1,
      pbt,
      margine,
      martingala,
      mazzo,
      httpStatus: decide.status,
      command: decide.command,
      meaning: decide.command !== null ? DECIDE_MEANINGS[decide.command] ?? '?' : null,
      body: decide.body,
      ok: decide.ok && decide.command !== null,
    };
    report.decideCalls.push(entry);
    if (!entry.ok) report.errors.push(`decide hand ${i + 1}: status=${decide.status} body=${decide.body}`);

    lastMirrorState = { margine, martingala, mazzo, stato: 'ATTESA', pbt };
    if (liveOpts?.enabled) {
      await doMirror(lastMirrorState, `mirror-hand-${i + 1}`);
    } else {
      await new Promise((r) => setTimeout(r, 300));
    }
  }

  const profitF = await postForm(base, '/api/proactive/get-global-profit', {
    USERNAME: cfg.username,
    PASSWORD: cfg.password,
    COMPUTER: cfg.computer,
  });
  report.steps.push({ step: 'E-get-global-profit-final', ...profitF });

  const decideOk = report.decideCalls.filter((d) => d.ok).length;
  const mirrorOk = liveOpts?.enabled ? report.mirrorCalls.filter((m) => m.ok).length : 0;
  const mirrorExpected = liveOpts?.enabled ? report.mirrorCalls.length : 0;

  let dashboardLiveOk = false;
  if (liveOpts?.enabled) {
    const verify = await verifyDashboardPc(liveOpts.dashApi, liveOpts.secret, cfg.computer, {
      margine: lastMirrorState.margine,
      mazzo: lastMirrorState.mazzo,
      stato: lastMirrorState.stato,
    });
    report.dashboardVerifications.push({ step: 'final-pc96-verify', ...verify });
    dashboardLiveOk = verify.ok;
    if (!dashboardLiveOk) {
      report.errors.push(
        `dashboard verify PC96: expected margine=${lastMirrorState.margine} mazzo=${lastMirrorState.mazzo}, got ${JSON.stringify(verify.row)}`
      );
    }
  }

  const decisoreApiOk = decideOk === handCount && updateInit.ok && authOk;

  report.summary = {
    appConfigValid: true,
    decisoreHost: base,
    dash2aApiUrl: liveOpts?.dashApi ?? null,
    decisoreApiOk,
    dashboardLivePc96Ok: liveOpts?.enabled ? dashboardLiveOk : null,
    authProbable: authOk ? 'OK' : 'KO',
    pcAccepted: updateInit.ok && decideOk > 0 ? 'OK' : 'KO',
    missionSimulated: decisoreApiOk && (!liveOpts?.enabled || (mirrorOk === mirrorExpected && dashboardLiveOk)),
    decideOkCount: decideOk,
    decideTotal: handCount,
    mirrorOkCount: mirrorOk,
    mirrorTotal: mirrorExpected,
    resetCalled: false,
    cleanupPerformed: false,
    firebaseTouched: false,
  };

  return report;
}

async function main() {
  const args = parseArgs(process.argv);
  if (args.help) {
    console.log(`Usage: node tools/decisore-mission-sim.mjs [--config path] [--hands N] [--live-dashboard]`);
    process.exit(0);
  }

  if (args.liveDashboard && !args.mirrorSecret) {
    console.error(JSON.stringify({ error: 'COLLAUDO_MIRROR_SECRET required for --live-dashboard' }, null, 2));
    process.exit(1);
  }

  let xml;
  try {
    xml = readFileSync(args.config, 'utf8');
  } catch (e) {
    console.error(JSON.stringify({ error: `Cannot read config: ${args.config}`, detail: e.message }, null, 2));
    process.exit(1);
  }

  const cfg = parseAppConfig(xml);
  cfg._configPath = args.config;
  const configErrors = validateConfig(cfg);
  if (configErrors.length) {
    console.error(JSON.stringify({ appConfigValid: false, errors: configErrors, cfg: { url: cfg.url, computer: cfg.computer } }, null, 2));
    process.exit(1);
  }

  const liveOpts = args.liveDashboard
    ? { enabled: true, dashApi: args.dashApi, secret: args.mirrorSecret }
    : null;

  const report = await runSimulation(cfg, args.hands, liveOpts);
  const outDir = join(__dirname, '..', 'artifacts');
  mkdirSync(outDir, { recursive: true });
  const outFile = join(outDir, `decisore-mission-sim-${Date.now()}.json`);
  writeFileSync(outFile, JSON.stringify(report, null, 2));

  console.log(
    JSON.stringify(
      {
        ...report.summary,
        reportFile: outFile,
        errors: report.errors,
      },
      null,
      2
    )
  );
  process.exit(report.summary.missionSimulated ? 0 : 1);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
