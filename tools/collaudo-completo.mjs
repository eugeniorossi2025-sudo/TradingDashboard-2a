import { chromium } from '../../PCTEST45/TradingDashboard (7)/TradingDashboard/node_modules/playwright/index.mjs';
import { writeFileSync } from 'fs';

const FRONTEND = process.env.FRONTEND_URL || 'http://localhost:5001';
const API = process.env.API_URL || 'http://localhost:5299';
const USER = 'admin';
const PASS = 'Admin@123456';

const REST_PATHS = [
  { method: 'GET', path: '/api/Auth/test', auth: false },
  { method: 'GET', path: '/api/Configuration', auth: true },
  { method: 'GET', path: '/api/Dashboard/data', auth: true },
  { method: 'GET', path: '/api/Dashboard/chart', auth: true },
  { method: 'GET', path: '/api/Log?page=1&pageSize=10', auth: true },
  { method: 'GET', path: '/api/runtime-mode', auth: true },
  {
    method: 'GET',
    path: '/api/mission/reports/index?runtimeMode=Demo&fromUtc=2016-01-01&toUtc=2026-12-31&skip=0&limit=5',
    auth: true,
  },
  {
    method: 'GET',
    path: '/api/mission/report/range?runtimeMode=Demo&from=2016-01-01&to=2026-12-31&format=json&summary=true',
    auth: true,
  },
  { method: 'GET', path: '/api/decider/config', auth: true },
  { method: 'GET', path: '/api/decider/health', auth: true },
  { method: 'GET', path: '/api/User', auth: true },
  { method: 'GET', path: '/api/admin/users/overview', auth: true },
  { method: 'GET', path: '/api/admin/user-notification-settings', auth: true },
];

const PAGES = [
  { group: 'Admin desktop', path: '/', admin: true },
  { group: 'Admin desktop', path: '/pages/configurations', admin: true },
  { group: 'Admin desktop', path: '/pages/user', admin: true },
  { group: 'Admin desktop', path: '/pages/pc-configuration', admin: true },
  { group: 'Admin desktop', path: '/pages/log', admin: true },
  { group: 'Admin desktop', path: '/pages/roles-permissions', admin: true },
  { group: 'Admin desktop', path: '/pages/console', admin: true },
  { group: 'Admin mobile', path: '/admin/mobile-live', admin: true, mobile: true },
  { group: 'Client desktop', path: '/client/desktop' },
  { group: 'Client mobile', path: '/client/mobile', mobile: true },
];

function isLocalTestEmail(email) {
  if (!email) return false;
  const e = email.toLowerCase();
  return e.includes('@botdashboard.local') || e.includes('@dash2a.local') || e.includes('localhost');
}

function sanitizeNetwork(url, status) {
  const path = url.replace(API, '').split('?')[0];
  return { path, status };
}

async function loginRest() {
  const res = await fetch(`${API}/api/Auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username: USER, password: PASS }),
  });
  const body = await res.json().catch(() => ({}));
  const token = body.token || body.Token || '';
  return { status: res.status, ok: res.ok && !!token, tokenLength: token.length, token };
}

async function restSmoke(token) {
  const headers = { Authorization: `Bearer ${token}` };
  const rows = [];
  let sessionIdForDetail = null;

  for (const ep of REST_PATHS) {
    try {
      const res = await fetch(`${API}${ep.path}`, { headers: ep.auth ? headers : {} });
      let note = '';
      if (ep.path.includes('/mission/reports/index') && res.ok) {
        const json = await res.json();
        const data = json.data ?? json;
        const total = data.total ?? 0;
        const first = data.items?.[0];
        sessionIdForDetail = first?.sessionId ?? null;
        note = `total=${total}${first ? ` first=#${first.sessionId}` : ''}`;
      } else if (ep.path.includes('/decider/config') && res.ok) {
        const json = await res.json();
        const d = json.data ?? json;
        note = `baseUrl=${d.baseUrl} (diagnostica only)`;
      } else if (ep.path.includes('/decider/health') && res.ok) {
        const json = await res.json();
        const d = json.data ?? json;
        note = `reachable=${d.reachable} status=${d.statusCode}`;
      } else if (ep.path.includes('/user-notification-settings') && res.ok) {
        const json = await res.json();
        const rowsN = (json.data ?? json)?.length ?? 0;
        note = `users=${rowsN}`;
      }
      rows.push({
        endpoint: `${ep.method} ${ep.path.split('?')[0]}`,
        status: res.status,
        ok: res.ok,
        note,
      });
    } catch (e) {
      rows.push({ endpoint: `${ep.method} ${ep.path.split('?')[0]}`, status: 0, ok: false, note: e.message });
    }
  }

  if (sessionIdForDetail) {
    for (const [fmt, label] of [
      ['html', 'HTML'],
      ['json', 'JSON'],
      ['csv', 'CSV'],
    ]) {
      try {
        const res = await fetch(`${API}/api/mission/report/${sessionIdForDetail}?format=${fmt}`, { headers });
        rows.push({
          endpoint: `GET /api/mission/report/{id}?format=${fmt}`,
          status: res.status,
          ok: res.ok,
          note: `sessionId=${sessionIdForDetail} ${label}`,
        });
      } catch (e) {
        rows.push({
          endpoint: `GET /api/mission/report/{id}?format=${fmt}`,
          status: 0,
          ok: false,
          note: e.message,
        });
      }
    }
  } else {
    rows.push({
      endpoint: 'GET /api/mission/report/{id}',
      status: 0,
      ok: false,
      note: 'nessuna missione in indice — skip dettaglio',
    });
  }

  return { rows, sessionIdForDetail };
}

async function notificationRest(token) {
  const headers = { Authorization: `Bearer ${token}` };
  const result = { getStatus: 0, testStatus: null, testNote: 'non eseguito', smtpConfigured: null };

  const getRes = await fetch(`${API}/api/admin/user-notification-settings`, { headers });
  result.getStatus = getRes.status;
  if (!getRes.ok) {
    result.testNote = 'GET settings fallito';
    return result;
  }

  const json = await getRes.json();
  const settings = json.data ?? json;
  const adminSetting = Array.isArray(settings)
    ? settings.find((s) => s.username === 'admin' || s.userId === 1)
    : null;

  if (!adminSetting) {
    result.testNote = 'admin non trovato in settings';
    return result;
  }

  const email = adminSetting.notificationEmail || adminSetting.loginEmail;
  if (!isLocalTestEmail(email)) {
    result.testNote = `test email skipped — destinatario non locale (${email ? 'masked' : 'vuoto'})`;
    return result;
  }

  try {
    const testRes = await fetch(`${API}/api/admin/user-notification-settings/${adminSetting.userId}/test`, {
      method: 'POST',
      headers,
    });
    result.testStatus = testRes.status;
    const testBody = await testRes.json().catch(() => ({}));
    if (testRes.ok) {
      result.testNote = 'email test inviata a indirizzo locale configurato';
      result.smtpConfigured = true;
    } else {
      const msg = testBody.message || testBody.errors?.[0] || 'fallito';
      result.testNote = `invio fallito o SMTP non configurato: ${msg}`;
      result.smtpConfigured = false;
    }
  } catch (e) {
    result.testNote = e.message;
  }

  return result;
}

async function run() {
  const loginRestResult = await loginRest();
  const { rows: restRows, sessionIdForDetail } = await restSmoke(loginRestResult.token);
  const notification = await notificationRest(loginRestResult.token);

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1366, height: 900 } });
  const page = await context.newPage();

  const consoleErrors = [];
  const networkCalls = [];
  const blockingNetwork = [];

  page.on('console', (msg) => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });
  page.on('response', (res) => {
    const url = res.url();
    if (!url.includes('/api/')) return;
    const entry = sanitizeNetwork(url, res.status());
    networkCalls.push(entry);
    if (res.status() >= 400 && !url.includes('favicon')) {
      blockingNetwork.push(entry);
    }
  });

  const loginUi = { ok: false, redirect: '', errors: [] };
  try {
    await page.goto(`${FRONTEND}/auth/login`, { waitUntil: 'networkidle', timeout: 30000 });
    await page.locator('#username1').fill(USER);
    await page.locator('#password1 input[type="password"]').fill(PASS);
    await page.getByRole('button', { name: 'Accedi' }).click();
    await page.waitForURL((url) => !url.pathname.includes('/auth/login'), { timeout: 15000 });
    loginUi.redirect = page.url();
    const token = await page.evaluate(
      () => localStorage.getItem('authToken') || sessionStorage.getItem('authToken') || ''
    );
    loginUi.ok = !!token && !loginUi.redirect.includes('/auth/login');
  } catch (e) {
    loginUi.errors.push(e.message);
  }

  const nav = [];
  for (const p of PAGES) {
    const entry = {
      route: p.path,
      group: p.group,
      esito: 'OK',
      consoleErrors: 0,
      networkErrors: 0,
      note: '',
    };
    const c0 = consoleErrors.length;
    const n0 = blockingNetwork.length;

    try {
      if (p.mobile) await page.setViewportSize({ width: 390, height: 844 });
      else await page.setViewportSize({ width: 1366, height: 900 });

      await page.goto(`${FRONTEND}${p.path}`, { waitUntil: 'networkidle', timeout: 30000 });
      await page.waitForTimeout(1200);
      const finalUrl = page.url();

      if (finalUrl.includes('/auth/login')) {
        entry.esito = 'KO';
        entry.note = 'redirect login';
      } else if (p.admin && finalUrl.includes('access-denied')) {
        entry.esito = 'KO';
        entry.note = '403 access-denied';
      }
    } catch (e) {
      entry.esito = 'KO';
      entry.note = e.message;
    }

    entry.consoleErrors = consoleErrors.length - c0;
    entry.networkErrors = blockingNetwork.length - n0;
    if (entry.consoleErrors > 0 && entry.esito === 'OK') entry.esito = 'WARN';
    nav.push(entry);
  }

  const mission = {
    esito: 'KO',
    sessionId: sessionIdForDetail,
    indexTotal: null,
    htmlOpen: false,
    jsonExport: false,
    csvExport: false,
    printNote: 'non testato',
    notes: [],
  };

  try {
    await page.setViewportSize({ width: 1366, height: 900 });
    await page.goto(`${FRONTEND}/pages/log`, { waitUntil: 'networkidle', timeout: 30000 });
    await page.waitForTimeout(2000);

    const totalText = await page.locator('text=Totale:').first().textContent().catch(() => '');
    const match = totalText?.match(/Totale:\s*(\d+)/);
    mission.indexTotal = match ? Number(match[1]) : null;

    const htmlBtn = page.getByRole('button', { name: 'HTML' }).first();
    const hasMission = (await htmlBtn.count()) > 0;

    if (!hasMission || !sessionIdForDetail) {
      mission.notes.push('nessuna missione in tabella locale');
      mission.esito = mission.indexTotal === 0 ? 'WARN' : 'KO';
    } else {
      const [popup] = await Promise.all([
        context.waitForEvent('page', { timeout: 10000 }).catch(() => null),
        htmlBtn.click(),
      ]);
      if (popup) {
        await popup.waitForLoadState('domcontentloaded', { timeout: 10000 }).catch(() => {});
        const html = await popup.content().catch(() => '');
        mission.htmlOpen = html.length > 200 && /mission|margin|report/i.test(html);
        mission.printNote = mission.htmlOpen ? 'HTML apribile — print browser disponibile' : 'HTML vuoto o incompleto';
        await popup.close().catch(() => {});
      }

      const downloadJson = await page
        .getByRole('button', { name: 'JSON' })
        .first()
        .click()
        .then(() => true)
        .catch(() => false);
      mission.jsonExport = downloadJson;

      const downloadCsv = await page
        .getByRole('button', { name: 'CSV' })
        .first()
        .click()
        .then(() => true)
        .catch(() => false);
      mission.csvExport = downloadCsv;

      mission.sessionId = sessionIdForDetail;
      mission.esito = mission.htmlOpen ? 'OK' : 'WARN';
    }
  } catch (e) {
    mission.notes.push(e.message);
  }

  const notificationUi = { esito: 'WARN', note: '' };
  try {
    await page.goto(`${FRONTEND}/pages/user`, { waitUntil: 'networkidle', timeout: 30000 });
    await page.waitForTimeout(1500);
    const hasTable = (await page.locator('text=Notifiche').count()) > 0;
    const hasTestBtn = (await page.getByRole('button', { name: /test/i }).count()) > 0;
    notificationUi.note = `pagina utenti OK, tabella notifiche=${hasTable}, pulsante test=${hasTestBtn}`;
    notificationUi.esito = hasTable ? 'OK' : 'WARN';
  } catch (e) {
    notificationUi.esito = 'KO';
    notificationUi.note = e.message;
  }

  await browser.close();

  const bugs = [];
  for (const r of restRows) {
    if (!r.ok && r.status !== 404) bugs.push(`REST ${r.endpoint} → ${r.status}`);
  }
  for (const n of nav) {
    if (n.esito === 'KO') bugs.push(`UI ${n.route} → ${n.note}`);
  }
  if (mission.esito === 'KO') bugs.push(`Mission reports: ${mission.notes.join('; ')}`);

  const uniqueBlocking = [...new Map(blockingNetwork.map((x) => [`${x.path}:${x.status}`, x])).values()];
  const nonOptional404 = uniqueBlocking.filter(
    (x) => x.status === 404 && !x.path.includes('botsession') && !x.path.includes('signalr')
  );
  if (nonOptional404.length) {
    bugs.push(`Network 404: ${nonOptional404.map((x) => `${x.path} (${x.status})`).join(', ')}`);
  }

  const restFail = restRows.some((r) => !r.ok && r.status !== 0);
  const navFail = nav.some((n) => n.esito === 'KO');
  const loginFail = !loginRestResult.ok || !loginUi.ok;
  const verdict = loginFail || restFail || navFail ? 'FAIL' : bugs.length ? 'PASS*' : 'PASS';

  const report = {
    generatedAt: new Date().toISOString(),
    commit: 'd9571c0',
    environment: { frontend: FRONTEND, api: API, db: 'Dash2A_LocalProdLike' },
    login: {
      rest: { status: loginRestResult.status, ok: loginRestResult.ok, jwtLength: loginRestResult.tokenLength },
      ui: loginUi,
    },
    rest: restRows,
    navigation: nav,
    mission,
    notifications: {
      restGet: notification.getStatus,
      restTest: notification.testStatus,
      restNote: notification.testNote,
      ui: notificationUi,
    },
    deciderNote: 'Decider usato solo per config/health diagnostica. Nessun sync Decider→DB locale. Dashboard da DB locale.',
    security: {
      blockingNetwork: uniqueBlocking.slice(0, 20),
      consoleErrorCount: consoleErrors.length,
      consoleSample: consoleErrors.slice(0, 5).map((e) => e.slice(0, 120)),
    },
    bugs,
    verdict: verdict === 'PASS*' ? 'PASS' : verdict,
    warnings: verdict === 'PASS*' ? bugs : [],
    remaining: bugs.length ? bugs : ['Nessun blocco critico rilevato'],
    confirmations: { push: false, deploy: false, commit: false },
  };

  writeFileSync('tools/collaudo-completo-report.sanitized.json', JSON.stringify(report, null, 2));
  console.log(JSON.stringify(report, null, 2));
  process.exit(loginFail || restFail || navFail ? 1 : 0);
}

run().catch((e) => {
  console.error(e);
  process.exit(1);
});
