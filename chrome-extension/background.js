chrome.webNavigation.onCommitted.addListener(
    (details) => {
        if (details.frameId === 0) return;
        chrome.scripting.executeScript({
            target: { tabId: details.tabId, frameIds: [details.frameId] },
            files: ["content.js"]
        });
    },
    { url: [{ hostSuffix: "eplay24.it" }] }
);