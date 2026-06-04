import { createRequire } from 'module';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';
import { mkdirSync } from 'fs';

const require = createRequire(import.meta.url);
const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');
const outDir = join(root, 'artifacts', 'collaudo-screenshots');
const outFile = join(outDir, 'control-room-4-comandi.png');

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

const FRONTEND = process.env.FRONTEND_URL || 'http://localhost:5173';
const checks = [];

function record(name, ok, detail = '') {
  checks.push({ name, ok, detail });
  console.log(`${ok ? 'PASS' : 'FAIL'} ${name}${detail ? ` — ${detail}` : ''}`);
}

async function main() {
  mkdirSync(outDir, { recursive: true });

  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1600, height: 1400 } });
  const consoleErrors = [];
  page.on('console', (msg) => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });
  page.on('pageerror', (err) => consoleErrors.push(String(err)));

  await page.goto(`${FRONTEND}/auth/login`, { waitUntil: 'domcontentloaded', timeout: 60000 });
  await page.getByPlaceholder('Username').fill(process.env.COLLAUDO_USER || 'admin');
  await page.getByPlaceholder('Password').fill(process.env.COLLAUDO_PASS || 'Admin@123456');
  await page.getByRole('button', { name: 'Accedi' }).click();
  await page.waitForURL((url) => !url.pathname.includes('/auth/login'), { timeout: 60000 });
  record('login browser', true, page.url());

  await page.waitForTimeout(6000);
  await page.getByText('Control Room', { exact: false }).first().waitFor({ state: 'visible', timeout: 90000 });
  await page.getByText('Filtro 5', { exact: true }).first().scrollIntoViewIfNeeded();
  await page.waitForTimeout(1200);

  const bodyText = await page.locator('body').innerText();
  for (const label of ['Filtro 5', 'AC3 Filtro 5', 'Filtro 8', 'AC3 Filtro 8']) {
    record(`UI ${label}`, bodyText.includes(label));
  }
  const attivaCount = (bodyText.match(/ATTIVA/g) || []).length;
  const spegniCount = (bodyText.match(/SPEGNI/g) || []).length;
  record('UI ATTIVA (>=4)', attivaCount >= 4, `count=${attivaCount}`);
  record('UI SPEGNI (>=4)', spegniCount >= 4, `count=${spegniCount}`);

  for (const p of ['P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'P7', 'P8']) {
    record(`blocco ${p}`, (await page.getByText(p, { exact: true }).count()) > 0);
  }

  const raceCard = page.locator('.card').filter({ hasText: 'Filtro 5' }).first();
  if (await raceCard.count()) {
    await raceCard.screenshot({ path: outFile });
  } else {
    await page.screenshot({ path: outFile, fullPage: true });
  }
  record('screenshot salvato', true, outFile);

  const ignorable = consoleErrors.filter(
    (e) => !/favicon|404|signalr|websocket|Failed to load resource/i.test(e)
  );
  record('browser console senza errori critici', ignorable.length === 0, ignorable.slice(0, 3).join(' | '));

  await browser.close();

  console.log(`\nScreenshot: ${outFile}`);
  const failed = checks.filter((c) => !c.ok);
  if (failed.length) {
    console.error('\nControlli visivi falliti:', failed.map((f) => f.name).join(', '));
    process.exit(1);
  }
  console.log('\nCONTROL_ROOM_VISUAL_CHECK PASS');
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
