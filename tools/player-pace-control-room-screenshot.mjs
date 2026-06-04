import { createRequire } from 'module';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const require = createRequire(import.meta.url);
const __dirname = dirname(fileURLToPath(import.meta.url));
const playwrightRoots = [
  join(__dirname, '..', 'frontend', 'node_modules', 'playwright'),
  join(__dirname, '..', '..', 'PCTEST45', 'TradingDashboard (7)', 'TradingDashboard', 'node_modules', 'playwright'),
  'playwright'
];
let chromium;
for (const root of playwrightRoots) {
  try {
    ({ chromium } = require(root));
    break;
  } catch {
    /* try next */
  }
}
if (!chromium) throw new Error('playwright non trovato — npm install playwright');
import { mkdirSync } from 'fs';

const root = join(__dirname, '..');
const outDir = join(root, 'artifacts', 'collaudo-screenshots');
const outFile = join(outDir, 'control-room-4-comandi.png');

const FRONTEND = process.env.FRONTEND_URL || 'http://localhost:5173';
const API = process.env.API_URL || 'http://localhost:5299';
const USER = process.env.COLLAUDO_USER || 'admin';
const PASS = process.env.COLLAUDO_PASS || 'Admin@123456';

const checks = [];

function record(name, ok, detail = '') {
  checks.push({ name, ok, detail });
  console.log(`${ok ? 'PASS' : 'FAIL'} ${name}${detail ? ` — ${detail}` : ''}`);
}

async function loginToken() {
  const res = await fetch(`${API}/api/Auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ Username: USER, Password: PASS }),
  });
  if (!res.ok) return null;
  const body = await res.json();
  return body.token || body.Token || null;
}

function mockTelemetryEnvelope() {
  return {
    success: true,
    message: 'Operation successful',
    data: {
      securityFilterEnabled: false,
      playerRace5FilterEnabled: true,
      playerRace5Ac3Enabled: false,
      playerRace8FilterEnabled: false,
      playerRace8Ac3Enabled: false,
      securityFilterByBot: {
        'COLLAUDO-01': {
          computer: 'COLLAUDO-01',
          playerStreakCount: 4,
          currentStreakOutcome: 'P',
          securityRiskScore: 0,
          securityFilterActive: false,
          playerRace5Alert: false,
          playerPaceTriggeredAC3: false,
          pauseBot: false,
          martingala: 3,
          avgHandSeconds: 24,
          lastReason: 'collaudo'
        }
      },
      margineTot: 0,
      margineMin: 0,
      margineMax: 0,
      elapsed: 0
    },
    errors: [],
    timestamp: new Date().toISOString()
  };
}

async function main() {
  mkdirSync(outDir, { recursive: true });

  const token = await loginToken();
  if (!token) {
    console.error('Login failed — avvia WebApi locale (Dash2A_Collaudo) e credenziali admin');
    process.exit(1);
  }
  record('login API per browser', true);

  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1600, height: 1400 } });
  const consoleErrors = [];
  page.on('console', (msg) => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });
  page.on('pageerror', (err) => consoleErrors.push(String(err)));

  const ok = (data) => ({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ success: true, data, errors: [], message: 'ok' })
  });

  await page.goto(`${FRONTEND}/auth/login`, { waitUntil: 'networkidle', timeout: 60000 });
  await page.getByPlaceholder('Username').fill(USER);
  await page.getByPlaceholder('Password').fill(PASS);
  await page.getByRole('button', { name: 'Accedi' }).click();
  await page.waitForURL((url) => !url.pathname.includes('/auth/login'), { timeout: 60000 });

  await page.route('**/api/**', async (route) => {
    const url = route.request().url();
    if (url.includes('/api/Dashboard/telemetry')) {
      await route.fulfill(ok(mockTelemetryEnvelope().data));
      return;
    }
    if (url.includes('/api/Dashboard/data')) {
      await route.fulfill(ok({ tables: [] }));
      return;
    }
    if (url.includes('/api/Dashboard/chart') || url.includes('/api/Dashboard/margini-chart')) {
      await route.fulfill(ok([]));
      return;
    }
    if (url.includes('/api/mission')) {
      await route.fulfill(ok({ hasOpenMission: false }));
      return;
    }
    if (url.includes('/api/Configuration') || url.includes('/api/configuration')) {
      await route.fulfill(ok({ value: 'PROACTIVE' }));
      return;
    }
    await route.continue();
  });

  await page.reload({ waitUntil: 'domcontentloaded' });
  await page.getByText('Control Room', { exact: false }).first().waitFor({ state: 'visible', timeout: 90000 });
  await page.getByText('Filtro 5', { exact: true }).first().scrollIntoViewIfNeeded();
  await page.waitForTimeout(1500);

  const bodyText = await page.locator('body').innerText();
  console.log('URL:', page.url());

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

  const raceCard = page.locator('text=Filtro 5').first();
  await raceCard.scrollIntoViewIfNeeded();
  await page.waitForTimeout(800);

  const fullCard = page.locator('text=Control Room').first();
  if (await fullCard.count()) {
    const section = fullCard.locator('xpath=ancestor::div[contains(@class,"card")][1]');
    if (await section.count()) {
      await section.screenshot({ path: outFile });
    } else {
      await page.locator('.card').filter({ hasText: 'Filtro 5' }).first().screenshot({ path: outFile });
    }
  } else {
    await page.locator('.card').filter({ hasText: 'Filtro 5' }).first().screenshot({ path: outFile });
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
