import { chromium } from '../../PCTEST45/TradingDashboard (7)/TradingDashboard/node_modules/playwright/index.mjs';
import { writeFileSync } from 'fs';

const FRONTEND = process.env.FRONTEND_URL || 'http://localhost:5001';
const API = process.env.API_URL || 'http://localhost:5299';
const USER = process.env.COLLAUDO_USER || 'admin';
const PASS = process.env.COLLAUDO_PASS || 'Admin@123456';

const pages = [
  { group: 'Auth', path: '/auth/login', auth: false, desktop: true },
  { group: 'Admin desktop', path: '/', auth: true, admin: true, desktop: true },
  { group: 'Admin desktop', path: '/pages/configurations', auth: true, admin: true, desktop: true },
  { group: 'Admin desktop', path: '/pages/user', auth: true, admin: true, desktop: true },
  { group: 'Admin desktop', path: '/pages/pc-configuration', auth: true, admin: true, desktop: true },
  { group: 'Admin desktop', path: '/pages/log', auth: true, admin: true, desktop: true },
  { group: 'Admin desktop', path: '/pages/bot-sessions', auth: true, admin: true, desktop: true },
  { group: 'Admin desktop', path: '/pages/console', auth: true, admin: true, desktop: true },
  { group: 'Admin desktop', path: '/pages/roles-permissions', auth: true, admin: true, desktop: true },
  { group: 'Admin mobile', path: '/admin/mobile-live', auth: true, admin: true, mobile: true },
  { group: 'Client desktop', path: '/client/desktop', auth: true, desktop: true },
  { group: 'Client mobile', path: '/client/mobile', auth: true, mobile: true },
];

const results = [];
const apiCalls = new Map();

function noteApi(method, url, status) {
  const key = `${method} ${url.split('?')[0]}`;
  if (!apiCalls.has(key)) apiCalls.set(key, []);
  apiCalls.get(key).push(status);
}

async function apiSmoke(token) {
  const headers = token ? { Authorization: `Bearer ${token}` } : {};
  const endpoints = [
    ['GET', '/api/Auth/test', false],
    ['GET', '/api/Configuration', true],
    ['GET', '/api/Dashboard/data', true],
    ['GET', '/api/mission/reports/index', true],
    ['GET', '/api/runtime-mode', true],
    ['GET', '/api/Dashboard/updates', true],
  ];
  const out = [];
  for (const [method, path, needsAuth] of endpoints) {
    try {
      const res = await fetch(`${API}${path}`, { headers: needsAuth ? headers : {} });
      out.push({ path, status: res.status, ok: res.ok });
      noteApi(method, path, res.status);
    } catch (e) {
      out.push({ path, status: 0, ok: false, error: e.message });
    }
  }
  return out;
}

async function run() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1366, height: 900 },
    ignoreHTTPSErrors: true,
  });
  const page = await context.newPage();

  const consoleErrors = [];
  const pageErrors = [];
  page.on('console', (msg) => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });
  page.on('pageerror', (err) => pageErrors.push(err.message));
  page.on('response', (res) => {
    const url = res.url();
    if (url.startsWith(`${API}/api/`)) noteApi(res.request().method(), url.replace(API, ''), res.status());
  });

  // Login flow
  let loginOk = false;
  let token = '';
  let redirectAfterLogin = '';
  let refreshKeepsSession = false;
  const loginErrors = [];

  try {
    await page.goto(`${FRONTEND}/auth/login`, { waitUntil: 'networkidle', timeout: 30000 });
    await page.locator('#username1').fill(USER);
    await page.locator('#password1 input[type="password"]').fill(PASS);
    await page.getByRole('button', { name: 'Accedi' }).click();
    await page.waitForURL((url) => !url.pathname.includes('/auth/login'), { timeout: 15000 });
    redirectAfterLogin = page.url();
    token = await page.evaluate(() => localStorage.getItem('authToken') || sessionStorage.getItem('authToken') || '');
    loginOk = !!token && !redirectAfterLogin.includes('/auth/login');
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForTimeout(1500);
    refreshKeepsSession = !page.url().includes('/auth/login');
  } catch (e) {
    loginErrors.push(e.message);
  }

  const apiDirect = await apiSmoke(token);

  for (const p of pages) {
    const entry = {
      group: p.group,
      path: p.path,
      viewport: p.mobile ? 'mobile' : 'desktop',
      esito: 'OK',
      finalUrl: '',
      networkErrors: [],
      consoleErrors: [],
      notes: [],
    };

    try {
      if (p.mobile) {
        await page.setViewportSize({ width: 390, height: 844 });
      } else {
        await page.setViewportSize({ width: 1366, height: 900 });
      }

      const beforeErrors = consoleErrors.length;
      await page.goto(`${FRONTEND}${p.path}`, { waitUntil: 'networkidle', timeout: 30000 });
      await page.waitForTimeout(1500);
      entry.finalUrl = page.url();

      if (p.auth && entry.finalUrl.includes('/auth/login')) {
        entry.esito = 'KO';
        entry.notes.push('redirect login');
      }
      if (p.admin && entry.finalUrl.includes('access-denied')) {
        entry.esito = 'KO';
        entry.notes.push('403 access-denied');
      }
      if (entry.finalUrl.includes('/auth/error')) {
        entry.esito = 'KO';
        entry.notes.push('auth error page');
      }

      const bodyText = await page.locator('body').innerText();
      if (/error|exception|failed to fetch/i.test(bodyText) && p.path !== '/auth/login') {
        entry.notes.push('possible error text in body');
      }

      entry.consoleErrors = consoleErrors.slice(beforeErrors);
      if (entry.consoleErrors.length) entry.esito = entry.esito === 'OK' ? 'WARN' : entry.esito;

      if (p.path === '/admin/mobile-live') {
        entry.notes.push(bodyText.includes('Live') || bodyText.includes('Margin') ? 'mobile layout rendered' : 'layout sparse/empty');
      }
      if (p.path === '/client/mobile' || p.path === '/client/desktop') {
        const hasAdmin = /configurazioni|roles|admin|console/i.test(bodyText);
        if (hasAdmin) {
          entry.esito = 'KO';
          entry.notes.push('admin UI visible on client page');
        } else {
          entry.notes.push('read-only client view');
        }
      }
    } catch (e) {
      entry.esito = 'KO';
      entry.notes.push(e.message);
    }

    results.push(entry);
  }

  await browser.close();

  const report = {
    generatedAt: new Date().toISOString(),
    frontend: FRONTEND,
    api: API,
    login: {
      ok: loginOk,
      redirectAfterLogin,
      tokenSaved: !!token,
      refreshKeepsSession,
      errors: loginErrors,
    },
    apiDirect,
    pages: results,
    apiCalls: Object.fromEntries(apiCalls),
    globalConsoleErrors: consoleErrors,
    globalPageErrors: pageErrors,
  };

  writeFileSync('tools/collaudo-funzionale-report.json', JSON.stringify(report, null, 2));
  console.log(JSON.stringify(report, null, 2));
}

run().catch((e) => {
  console.error(e);
  process.exit(1);
});
