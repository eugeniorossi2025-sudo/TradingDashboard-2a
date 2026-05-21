const KEY = "hideScrollbars";
const STYLE_ID = "ext-hide-scrollbars-style";

function ensureStyleTag() {
  let el = document.getElementById(STYLE_ID);
  if (!el) {
    el = document.createElement("style");
    el.id = STYLE_ID;
    el.textContent = `
      /* Viewport scrollbar */
      html { scrollbar-width: none !important; } /* Firefox: none = scrollbar nascosta ma scroll attivo */
      body { -ms-overflow-style: none !important; } /* legacy Edge/IE */
      html::-webkit-scrollbar,
      body::-webkit-scrollbar {
        width: 0 !important;
        height: 0 !important;
        display: none !important;
      }
    `;
    (document.documentElement || document).appendChild(el);
  }
  return el;
}

function setEnabled(enabled) {
  if (enabled) {
    ensureStyleTag();
  } else {
    const el = document.getElementById(STYLE_ID);
    if (el) el.remove();
  }
}

(async () => {
  const data = await chrome.storage.sync.get({ [KEY]: false });
  setEnabled(Boolean(data[KEY]));
})();

chrome.storage.onChanged.addListener((changes, area) => {
  if (area !== "sync") return;
  if (changes[KEY]) setEnabled(Boolean(changes[KEY].newValue));
});
