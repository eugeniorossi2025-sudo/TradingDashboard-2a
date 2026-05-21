function findInShadow(root, selector, acc = []) {
    root.querySelectorAll?.(selector)?.forEach(el => acc.push(el));
    root.querySelectorAll?.("*")?.forEach(host => {
        if (host.shadowRoot) findInShadow(host.shadowRoot, selector, acc);
    });
    return acc;
}

export function findInDocument(selector) {
    return findInShadow(document, selector);
}

export function applyLayout(config, existingState = null) {
    const state = existingState || {
        fullpage: new Map(),
        hidden: new Map(),
        custom: new Map(),
    };

    const shouldProcess = (el) => {
        if (el.dataset.extProcessed) return false;
        el.dataset.extProcessed = "true";
        return true;
    };

    (config.fullPageSelectors || []).forEach(item => {
        const els = findInDocument(item.selector);
        const targets = item.all ? els : [els[item.index || 0]];

        targets.forEach(el => {
            if (!el || !shouldProcess(el)) return;

            const saved = {};
            ["position", "inset", "width", "height", "zIndex", "background"].forEach(p => {
                saved[p] = el.style[p];
            });

            state.fullpage.set(el, saved);

            Object.assign(el.style, {
                position: "fixed", inset: "0", width: "100%", height: "100%", zIndex: "99999", background: "#000"
            });
        });
    });

    (config.hideSelectors || []).forEach(item => {
        const els = findInDocument(item.selector);
        const targets = item.all ? els : [els[item.index || 0]];

        targets.forEach(el => {
            if (!el || !shouldProcess(el)) return;
            state.hidden.set(el, el.style.display);
            el.style.display = "none";
        });
    });

    (config.customSelectors || []).forEach(item => {
        const els = findInDocument(item.selector);
        const targets = item.all ? els : [els[item.index || 0]];

        targets.forEach(el => {
            if (!el || !shouldProcess(el)) return;
            const saved = {};
            Object.keys(item.style).forEach(p => saved[p] = el.style[p]);
            state.custom.set(el, saved);
            Object.assign(el.style, item.style);
        });
    });

    return state;
}

export function restoreLayout(state) {
    if (!state) return;

    state.fullpage.forEach((saved, el) => {
        Object.assign(el.style, saved);
        delete el.dataset.extProcessed;
    });
    state.hidden.forEach((saved, el) => {
        el.style.display = saved || "";
        delete el.dataset.extProcessed;
    });
    state.custom.forEach((saved, el) => {
        Object.assign(el.style, saved);
        delete el.dataset.extProcessed;
    });
    
    state.fullpage.clear();
    state.hidden.clear();
    state.custom.clear();
}
