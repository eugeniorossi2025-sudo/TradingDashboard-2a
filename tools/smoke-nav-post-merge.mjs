import { chromium } from '../../PCTEST45/TradingDashboard (7)/TradingDashboard/node_modules/playwright/index.mjs';

const FRONTEND = 'http://localhost:5001';
const API = 'http://localhost:5299';
const USER = 'admin';
const PASS = 'Admin@123456';
const PAGES = ['/', '/pages/configurations', '/admin/mobile-live', '/client/desktop', '/client/mobile'];

async function run() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1366, height: 900 } });
  const consoleErrors = [];
  const network404 = [];
  page.on('console', (m) => { if (m.type() === 'error') consoleErrors.push(m.text()); });
  page.on('response', (res) => {
    if (res.status() === 404 && res.url().includes('/api/')) network404.push(res.url().replace(API, ''));
  });

  await page.goto(`${FRONTEND}/auth/login`, { waitUntil: 'networkidle' });
  await page.locator('#username1').fill(USER);
  await page.locator('#password1 input[type="password"]').fill(PASS);
  await page.getByRole('button', { name: 'Accedi' }).click();
  await page.waitForURL((u) => !u.pathname.includes('/auth/login'), { timeout: 15000 });

  const nav = [];
  for (const path of PAGES) {
    const mobile = path.includes('mobile');
    if (mobile) await page.setViewportSize({ width: 390, height: 844 });
    else await page.setViewportSize({ width: 1366, height: 900 });
    const c0 = consoleErrors.length;
    const n0 = network404.length;
    await page.goto(`${FRONTEND}${path}`, { waitUntil: 'networkidle', timeout: 30000 });
    await page.waitForTimeout(1200);
    const url = page.url();
    nav.push({
      path,
      esito: url.includes('/auth/login') || url.includes('access-denied') ? 'KO' : consoleErrors.length > c0 ? 'WARN' : 'OK',
      console: consoleErrors.length - c0,
      network404: network404.length - n0,
      finalUrl: url,
    });
  }
  await browser.close();
  console.log(JSON.stringify({ navigation: nav, network404: [...new Set(network404)], consoleSample: consoleErrors.slice(0, 5) }, null, 2));
}
run();
