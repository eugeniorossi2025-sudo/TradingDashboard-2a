/**
 * Collaudo LIVE Step 5 UI — verifica testo reale su Firebase prod.
 */
import { createRequire } from 'module';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const require = createRequire(import.meta.url);
const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');

const playwrightRoots = [
  join(root, 'frontend', 'node_modules', 'playwright'),
  join('C:/Users/eugen/Desktop/PCTEST45/TradingDashboard (7)/TradingDashboard/node_modules/playwright'),
  'playwright',
];
let chromium;
for (const p of playwrightRoots) {
  try {
    ({ chromium } = require(p));
    break;
  } catch {
    /* next */
  }
}
if (!chromium) throw new Error('playwright non trovato');

const FRONTEND = (process.env.FRONTEND_URL || 'https://eugenio-dashboard-2a.web.app').replace(/\/$/, '');
const USER = process.env.COLLAUDO_USER || 'admin';
const PASS = process.env.COLLAUDO_PASS || 'Admin@123456';

const NEED = ['L5 perse per generare crediti', 'Crediti L6 generati', 'Salva crediti L6'];
const OLD = ['Soglia L6 per bot', 'Salva soglia'];

let failed = 0;
function record(name, ok, detail = '') {
  console.log(`${ok ? 'PASS' : 'FAIL'} ${name}${detail ? ` — ${detail}` : ''}`);
  if (!ok) failed = 1;
}

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ bypassCSP: true });
  const page = await context.newPage({ viewport: { width: 1600, height: 1400 } });

  await page.goto(`${FRONTEND}/auth/login`, { waitUntil: 'domcontentloaded', timeout: 90000 });
  await page.getByPlaceholder('Username').fill(USER);
  await page.getByPlaceholder('Password').fill(PASS);
  await page.getByRole('button', { name: 'Accedi' }).click();
  await page.waitForURL((url) => !url.pathname.includes('/auth/login'), { timeout: 90000 });

  await page.waitForTimeout(8000);
  await page.getByText('SPOT L6 PER BOT', { exact: false }).first().waitFor({ state: 'visible', timeout: 120000 });

  const bodyText = await page.locator('body').innerText();
  for (const s of NEED) record(`live UI contiene: ${s}`, bodyText.includes(s));
  for (const s of OLD) record(`live UI NON contiene: ${s}`, !bodyText.includes(s));

  const panel = page.locator('text=SPOT L6 per bot').first().locator('xpath=ancestor::div[contains(@class,"rounded-xl")][1]');
  const panelText = await panel.innerText().catch(() => '');
  record('pannello SPOT L6 per bot visibile', panelText.length > 0, panelText.slice(0, 120));

  await browser.close();
  console.log(`\nVERDICT ${failed ? 'FAIL' : 'PASS'} — Step 5 UI live`);
  process.exit(failed);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
