import { chromium } from '../../PCTEST45/TradingDashboard (7)/TradingDashboard/node_modules/playwright/index.mjs';

const FRONTEND = 'http://localhost:5001';
const API = 'http://localhost:5299';
const USER = 'admin';
const PASS = 'Admin@123456';

const pages = [
  { group: 'Admin desktop', path: '/', admin: true },
  { group: 'Admin desktop', path: '/pages/configurations', admin: true },
  { group: 'Admin desktop', path: '/pages/user', admin: true },
  { group: 'Admin desktop', path: '/pages/pc-configuration', admin: true },
  { group: 'Admin desktop', path: '/pages/log', admin: true },
  { group: 'Admin mobile', path: '/admin/mobile-live', admin: true, mobile: true },
  { group: 'Client desktop', path: '/client/desktop' },
  { group: 'Client mobile', path: '/client/mobile', mobile: true },
];

async function restSmoke(token) {
  const headers = { Authorization: `Bearer ${token}` };
  const paths = [
    '/api/Auth/test',
    '/api/Dashboard/data',
    '/api/decider/config',
    '/api/decider/health',
    '/api/Configuration',
    '/api/Log',
    '/api/runtime-mode',
  ];
  const out = [];
  for (const path of paths) {
    const needsAuth = path !== '/api/Auth/test';
    try {
      const res = await fetch(`${API}${path}`, { headers: needsAuth ? headers : {} });
      out.push({ path, status: res.status, ok: res.ok });
    } catch (e) {
      out.push({ path, status: 0, ok: false, error: e.message });
    }
  }
  return out;
}

async function run() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1366, height: 900 } });
  const consoleErrors = [];

  page.on('console', (msg) => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });

  let login = { ok: false, redirect: '', token: '', errors: [] };
  try {
    await page.goto(`${FRONTEND}/auth/login`, { waitUntil: 'networkidle', timeout: 30000 });
    await page.locator('#username1').fill(USER);
    await page.locator('#password1 input[type="password"]').fill(PASS);
    await page.getByRole('button', { name: 'Accedi' }).click();
    await page.waitForURL((url) => !url.pathname.includes('/auth/login'), { timeout: 15000 });
    login.redirect = page.url();
    login.token = await page.evaluate(
      () => localStorage.getItem('authToken') || sessionStorage.getItem('authToken') || ''
    );
    login.ok = !!login.token && !login.redirect.includes('/auth/login');
  } catch (e) {
    login.errors.push(e.message);
  }

  const api = await restSmoke(login.token);
  const nav = [];

  for (const p of pages) {
    const entry = { path: p.path, group: p.group, esito: 'OK', finalUrl: '', notes: [] };
    try {
      if (p.mobile) await page.setViewportSize({ width: 390, height: 844 });
      else await page.setViewportSize({ width: 1366, height: 900 });

      await page.goto(`${FRONTEND}${p.path}`, { waitUntil: 'networkidle', timeout: 30000 });
      await page.waitForTimeout(1200);
      entry.finalUrl = page.url();

      if (entry.finalUrl.includes('/auth/login')) {
        entry.esito = 'KO';
        entry.notes.push('redirect login');
      } else if (p.admin && entry.finalUrl.includes('access-denied')) {
        entry.esito = 'KO';
        entry.notes.push('403 access-denied');
      } else if (entry.finalUrl.includes('/auth/error')) {
        entry.esito = 'KO';
        entry.notes.push('auth error');
      }
    } catch (e) {
      entry.esito = 'KO';
      entry.notes.push(e.message);
    }
    nav.push(entry);
  }

  await browser.close();

  const report = {
    generatedAt: new Date().toISOString(),
    frontend: FRONTEND,
    api: API,
    credentials: { username: USER, password: '***' },
    login,
    api,
    navigation: nav,
    consoleErrors: consoleErrors.slice(0, 20),
  };

  console.log(JSON.stringify(report, null, 2));
  const failed = !login.ok || api.some((x) => !x.ok) || nav.some((x) => x.esito === 'KO');
  process.exit(failed ? 1 : 0);
}

run().catch((e) => {
  console.error(e);
  process.exit(1);
});
