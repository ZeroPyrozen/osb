// Runs script-mode code in a fresh Web Worker with a time limit, so an endless loop in a learner's
// script can't freeze the page.

const TIMEOUT_MS = 3000;
let running = null;

/** @returns {Promise<{ok: true, osb: string, logs: string[], objects: number, commands: number} | {ok: false, error: string, line?: number}>} */
export function runInWorker(code, { seed = 1 } = {}) {
    running?.cancel();
    return new Promise((resolve) => {
        const worker = new Worker('/js/script-worker.js', { type: 'module' });
        const finish = (result) => {
            clearTimeout(timer);
            worker.terminate();
            if (running?.worker === worker) running = null;
            resolve(result);
        };
        const timer = setTimeout(() => finish({
            ok: false,
            error: `Your script ran for more than ${TIMEOUT_MS / 1000} seconds, so it was stopped. Is there a loop that never ends?`,
        }), TIMEOUT_MS);
        worker.onmessage = (event) => finish(event.data);
        worker.onerror = (event) => {
            event.preventDefault();
            finish({ ok: false, error: event.message || 'The script crashed.' });
        };
        running = { worker, cancel: () => finish({ ok: false, error: 'Replaced by a newer run.', cancelled: true }) };
        worker.postMessage({ code, seed });
    });
}
