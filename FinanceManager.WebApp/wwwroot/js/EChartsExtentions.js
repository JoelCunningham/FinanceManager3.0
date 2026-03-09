const handlers = new WeakMap();

function findChartInstance(wrapper) {
    if (!wrapper || !window.echarts) return null;

    const candidates = [wrapper, ...wrapper.querySelectorAll("div")];
    for (const el of candidates) {
        const instance = window.echarts.getInstanceByDom(el);
        if (instance) return instance;
    }

    return null;
}

export function attachClickHandler(wrapper, dotnetRef, attempt = 0) {
    if (!wrapper || !dotnetRef) return;
    if (!window.echarts) return;

    const existing = handlers.get(wrapper);

    // Clear any pending retry from a previous render
    if (existing?.retryTimer) clearTimeout(existing.retryTimer);
    

    const chart = findChartInstance(wrapper);

    // Chart not ready yet -> retry a few times
    if (!chart) {
        if (attempt < 20) {
            const retryTimer = setTimeout(() => attachClickHandler(wrapper, dotnetRef, attempt + 1), 50);
            handlers.set(wrapper, { ...(existing ?? {}), retryTimer });
        }
        return;
    }

    // Remove old handler (e.g., after chart re-init)
    if (existing?.chart && existing?.handler) {
        try {
            existing.chart.off("click", existing.handler);
        } catch {
            // ignore
        }
    }

    const handler = (params) => {
        const key = params?.data?.key ?? null;
        dotnetRef.invokeMethodAsync("NotifyChartClicked", key);
    };

    chart.on("click", handler);
    handlers.set(wrapper, { chart, handler, retryTimer: null });
}