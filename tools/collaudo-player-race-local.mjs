/**
 * Collaudo locale Player Race 4 comandi — WebApi + API roundtrip.
 * Usage: node tools/collaudo-player-race-local.mjs
 */
const API = process.env.API_URL || 'http://localhost:5299';
const USER = process.env.COLLAUDO_USER || 'admin';
const PASS = process.env.COLLAUDO_PASS || 'Admin@123456';

const endpoints = [
  { name: 'Filtro 5', path: '/api/player-race-5/filter' },
  { name: 'AC3 Filtro 5', path: '/api/player-race-5/ac3' },
  { name: 'Filtro 8', path: '/api/player-race-8/filter' },
  { name: 'AC3 Filtro 8', path: '/api/player-race-8/ac3' },
];

const report = [];

function record(section, name, ok, detail = '') {
  const row = { section, name, ok, detail };
  report.push(row);
  console.log(`${ok ? 'PASS' : 'FAIL'} [${section}] ${name}${detail ? ` — ${detail}` : ''}`);
}

function unwrap(data) {
  if (!data || typeof data !== 'object') return data;
  if ('data' in data) return data.data;
  if ('Data' in data) return data.Data;
  return data;
}

async function login() {
  const res = await fetch(`${API}/api/Auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ Username: USER, Password: PASS }),
  });
  const text = await res.text();
  if (!res.ok) {
    record('1.WebApi', 'login admin', false, `HTTP ${res.status} ${text.slice(0, 200)}`);
    return null;
  }
  let body;
  try {
    body = JSON.parse(text);
  } catch {
    record('1.WebApi', 'login admin', false, 'risposta non JSON');
    return null;
  }
  const token = body.token || body.Token || unwrap(body)?.token || unwrap(body)?.Token;
  if (!token) {
    record('1.WebApi', 'login admin', false, 'token assente');
    return null;
  }
  record('1.WebApi', 'login admin', true, 'nessun 401');
  return token;
}

async function apiGet(token, path) {
  const res = await fetch(`${API}${path}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  const body = await res.json().catch(() => ({}));
  return { status: res.status, body };
}

async function apiPut(token, path, enabled) {
  const res = await fetch(`${API}${path}`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ enabled }),
  });
  const body = await res.json().catch(() => ({}));
  return { status: res.status, body };
}

async function testEndpoint(token, ep) {
  const offGet = await apiGet(token, ep.path);
  if (offGet.status === 401) {
    record('2.API', `${ep.name} GET`, false, '401');
    return;
  }
  if (offGet.status !== 200) {
    record('2.API', `${ep.name} GET`, false, `HTTP ${offGet.status}`);
    return;
  }
  const offPut = await apiPut(token, ep.path, false);
  if (offPut.status === 401 || offPut.status === 403) {
    record('2.API', `${ep.name} PUT OFF`, false, `HTTP ${offPut.status}`);
    return;
  }
  if (offPut.status !== 200) {
    record('2.API', `${ep.name} PUT OFF`, false, `HTTP ${offPut.status}`);
    return;
  }
  const offRead = await apiGet(token, ep.path);
  const offVal = unwrap(offRead.body)?.enabled;
  record('2.API', `${ep.name} OFF roundtrip`, offVal === false, `enabled=${offVal}`);

  const onPut = await apiPut(token, ep.path, true);
  if (onPut.status !== 200) {
    record('2.API', `${ep.name} PUT ON`, false, `HTTP ${onPut.status}`);
    return;
  }
  const onRead = await apiGet(token, ep.path);
  const onVal = unwrap(onRead.body)?.enabled;
  record('2.API', `${ep.name} ON roundtrip`, onVal === true, `enabled=${onVal}`);

  await apiPut(token, ep.path, false);
}

async function probeNoMissionImpact(token) {
  const paths = [
    '/api/Dashboard/telemetry',
    '/api/mission/accounting-health',
  ];
  for (const path of paths) {
    const res = await fetch(`${API}${path}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const ok = res.status !== 500 && res.status !== 401;
    record('5.Sicurezza', `${path} non 500/401`, ok, `HTTP ${res.status}`);
  }
}

async function main() {
  console.log(`Collaudo API → ${API}\n`);
  const token = await login();
  if (!token) {
    writeReport();
    process.exit(1);
  }

  for (const ep of endpoints) {
    await testEndpoint(token, ep);
  }

  await probeNoMissionImpact(token);

  writeReport();
  const failed = report.filter((r) => !r.ok);
  process.exit(failed.length ? 1 : 0);
}

function writeReport() {
  const failed = report.filter((r) => !r.ok);
  console.log('\n--- REPORT ---');
  for (const r of report) {
    console.log(`${r.ok ? 'PASS' : 'FAIL'}\t${r.section}\t${r.name}\t${r.detail}`);
  }
  console.log(`\nTotale: ${report.length}  PASS: ${report.length - failed.length}  FAIL: ${failed.length}`);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
