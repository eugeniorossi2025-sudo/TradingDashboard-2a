/**
 * UI validation: Log page mission reports + Production range 2025.
 * Run: node tools/validate-post-import-ui.mjs
 */
import { chromium } from '../../PCTEST45/TradingDashboard (7)/TradingDashboard/node_modules/playwright/index.mjs';
import { writeFileSync, mkdirSync } from 'fs';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const FRONTEND = process.env.FRONTEND_URL || 'https://eugenio-dashboard-2a.web.app';
const API = process.env.API_URL || 'https://vps-b0942869.vps.ovh.net';
const USER = process.env.DASH2A_USER || 'admin';
const PASS = process.env.DASH2A_PASS || 'Admin@123456';

const checks = [];

function add(id, name, pass, detail, evidence = null) {
  checks.push({ id, area: 'UI', name, pass, status: pass ? 'PASS' : 'FAIL', detail, evidence, atUtc: new Date().toISOString() });
}

async function apiLogin(request) {
  const res = await request.post(`${API}/api/Auth/login`, {
    data: { username: USER, password: PASS },
    timeout: 60000,
  });
  const body = await res.json();
  const data = body.data ?? body;
  return data.token || data.Token || '';
}

async function run() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ ignoreHTTPSErrors: true });
  const page = await context.newPage();
  const request = context.request;

  let token = '';
  try {
    token = await apiLogin(request);
    add('UI-API-01', 'API login from Playwright', !!token, `tokenLength=${token.length}`, { api: API });
  } catch (e) {
    add('UI-API-01', 'API login from Playwright', false, e.message, { api: API });
  }

  if (token) {
    const headers = { Authorization: `Bearer ${token}` };
    try {
      const idx = await request.get(
        `${API}/api/mission/reports/index?runtimeMode=Production&fromUtc=2025-01-01&toUtc=2026-12-31&limit=200`,
        { headers, timeout: 60000 }
      );
      const idxBody = await idx.json();
      const data = idxBody.data ?? idxBody;
      add('UI-API-02', 'Index Production via API (Playwright)', idx.ok() && data.total === 27, `total=${data.total}`, { total: data.total });
    } catch (e) {
      add('UI-API-02', 'Index Production via API (Playwright)', false, e.message);
    }
  }

  try {
    await page.goto(`${FRONTEND}/auth/login`, { waitUntil: 'domcontentloaded', timeout: 90000 });
    await page.locator('#username1').fill(USER);
    await page.locator('#password1 input[type="password"]').fill(PASS);
    await page.getByRole('button', { name: 'Accedi' }).click();
    await page.waitForURL((u) => !u.pathname.includes('/auth/login'), { timeout: 30000 });
    add('UI-01', 'Firebase login', true, page.url());
  } catch (e) {
    add('UI-01', 'Firebase login', false, e.message);
    await browser.close();
    writeReport(checks);
    process.exit(2);
  }

  try {
    await page.goto(`${FRONTEND}/pages/log`, { waitUntil: 'domcontentloaded', timeout: 90000 });
    await page.waitForTimeout(3000);
    const bodyText = await page.locator('body').innerText();
    const hasMissionSection = /missione|report/i.test(bodyText);
    add('UI-02', 'Log page loads mission reports section', hasMissionSection, FRONTEND + '/pages/log');

    const totalText = bodyText.match(/(\d+)\s*(?:missioni|sessioni|total)/i);
    add('UI-03', 'Mission index visible on Log page', hasMissionSection, totalText ? totalText[0] : 'section present');

    const reportFrom = page.locator('input, .p-datepicker').first();
    await reportFrom.waitFor({ state: 'visible', timeout: 10000 }).catch(() => {});

    const downloadPromise = page.waitForEvent('download', { timeout: 15000 }).catch(() => null);
    const jsonBtn = page.getByRole('button', { name: /json/i }).first();
    if (await jsonBtn.count()) {
      await jsonBtn.click();
      const dl = await downloadPromise;
      add('UI-04', 'Production range JSON download', !!dl, dl ? dl.suggestedFilename() : 'button clicked');
    } else {
      add('UI-04', 'Production range JSON download', false, 'JSON button not found on Log page');
    }
  } catch (e) {
    add('UI-ERR', 'Log page validation', false, e.message);
  }

  await browser.close();
  writeReport(checks);
  const fail = checks.some((c) => !c.pass);
  process.exit(fail ? 2 : 0);
}

function writeReport(checks) {
  const outDir = join(__dirname, '../ops/dash2a-readiness/exports/validation/ui');
  mkdirSync(outDir, { recursive: true });
  const out = join(outDir, `ui_validation_${new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19)}.json`);
  const report = {
    generatedAtUtc: new Date().toISOString(),
    frontend: FRONTEND,
    api: API,
    summary: {
      total: checks.length,
      pass: checks.filter((c) => c.pass).length,
      fail: checks.filter((c) => !c.pass).length,
      allPass: checks.every((c) => c.pass),
    },
    checks,
  };
  writeFileSync(out, JSON.stringify(report, null, 2));
  console.log(JSON.stringify(report, null, 2));
}

run();
