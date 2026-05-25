import { chromium } from '../../PCTEST45/TradingDashboard (7)/TradingDashboard/node_modules/playwright/index.mjs';

const FRONTEND = process.env.FRONTEND_URL || 'https://eugenio-dashboard-2a.web.app';
const API = process.env.API_URL || 'https://vps-b0942869.vps.ovh.net';
const USER = 'admin';
const PASS = 'Admin@123456';

async function run() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1366, height: 900 } });
  const consoleErrors = [];
  page.on('console', (m) => { if (m.type() === 'error') consoleErrors.push(m.text()); });

  await page.goto(`${FRONTEND}/auth/login`, { waitUntil: 'load', timeout: 45000 });
  await page.locator('#username1').fill(USER);
  await page.locator('#password1 input[type="password"]').fill(PASS);
  await page.getByRole('button', { name: 'Accedi' }).click();
  await page.waitForURL((u) => !u.pathname.includes('/auth/login'), { timeout: 20000 });

  const token = await page.evaluate(() =>
    localStorage.getItem('authToken') || sessionStorage.getItem('authToken') || ''
  );
  const loginOk = !!token;

  await page.goto(`${FRONTEND}/pages/user`, { waitUntil: 'networkidle', timeout: 45000 });
  await page.waitForTimeout(2000);
  const userPageOk = !(await page.locator('text=Impossibile caricare utenti e notifiche').count());
  const hasUsersTable = (await page.locator('text=Gestione destinatari notifiche').count()) > 0;

  await browser.close();
  const ok = loginOk && userPageOk && hasUsersTable;
  console.log(JSON.stringify({ loginOk, userPageOk, hasUsersTable, api: API, consoleErrors: consoleErrors.slice(0, 3) }, null, 2));
  process.exit(ok ? 0 : 1);
}

run();
