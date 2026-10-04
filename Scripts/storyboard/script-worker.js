// Runs a learner's storyboard script off the main thread. The page terminates this worker if a
// script takes too long, so an endless loop can't freeze the tab.

import { runScript, errorLine, ScriptError } from './script.js';

self.onmessage = (event) => {
    const { code, seed } = event.data;
    try {
        self.postMessage({ ok: true, ...runScript(code, { seed }) });
    } catch (error) {
        self.postMessage({
            ok: false,
            error: error instanceof ScriptError || error instanceof Error ? error.message : String(error),
            line: errorLine(error),
        });
    }
};
