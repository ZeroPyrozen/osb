// An exercise: editor and live preview side by side, "Check my work", hints and the solution.
// The configuration comes from the unit's front matter, embedded as JSON by the server.

import { createEditor } from './editor.js';
import { Player } from './player.js';
import { runInWorker } from './run-script.js';
import { runChecks } from '../storyboard/checks.js';
import { learnReady } from '../learn/state.js';

const checkIcon = '<svg class="size-4 shrink-0" viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12.5 4.5 4.5L19 7.5" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round" fill="none"/></svg>';
const crossIcon = '<svg class="size-4 shrink-0" viewBox="0 0 24 24" aria-hidden="true"><path d="m7 7 10 10M17 7 7 17" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" fill="none"/></svg>';
const escape = (s) => String(s).replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]);

function storageKey(unitId) {
    return `osb.exercise.${unitId}`;
}

function loadDraft(unitId) {
    try { return localStorage.getItem(storageKey(unitId)); } catch { return null; }
}

function saveDraft(unitId, text) {
    try { localStorage.setItem(storageKey(unitId), text); } catch { /* private mode: drafts just aren't kept */ }
}

export function mountExercise(root) {
    const config = JSON.parse(root.querySelector('script[data-exercise-config]').textContent);
    const isScript = config.mode === 'script';
    const workspace = root.querySelector('[data-exercise-workspace]');

    workspace.innerHTML = `
        <div class="grid gap-4 xl:grid-cols-2">
            <div class="flex min-w-0 flex-col gap-2">
                <div class="flex flex-wrap items-center justify-between gap-2">
                    <p class="text-sm font-bold text-mint-50">${isScript ? 'Your script (JavaScript)' : 'Your storyboard (.osb)'}</p>
                    <div class="flex gap-1">
                        <button type="button" data-reset class="btn btn-ghost px-2.5 py-1 text-xs">Start over</button>
                        <button type="button" data-solution class="btn btn-ghost px-2.5 py-1 text-xs" disabled title="Try checking your work first">Show solution</button>
                    </div>
                </div>
                <div data-editor class="h-[22rem] overflow-hidden rounded-card border border-charcoal-600"></div>
                <p data-script-status class="min-h-5 text-xs text-sage" aria-live="polite"></p>
                ${isScript ? '<details class="text-xs text-sage"><summary class="cursor-pointer font-semibold text-mint-200">Generated .osb</summary><pre data-generated class="mt-2 max-h-64 overflow-auto rounded-card bg-charcoal-950 p-3 font-mono text-mint-100"></pre></details>' : ''}
            </div>
            <div class="min-w-0" data-player></div>
        </div>
        <div class="mt-5 flex flex-col gap-4 rounded-card border border-charcoal-600 bg-charcoal-900 p-5">
            <div class="flex flex-wrap items-center gap-3">
                <button type="button" data-check class="btn btn-primary">Check my work</button>
                ${config.hints?.length ? '<button type="button" data-hint class="btn btn-outline">Show a hint</button>' : ''}
                <p data-summary class="text-sm text-sage" aria-live="polite">Run the checks whenever you're ready.</p>
            </div>
            <ul data-results class="flex flex-col gap-1.5 text-sm"></ul>
            <ol data-hints class="flex list-decimal flex-col gap-2 pl-5 text-sm text-mint-100"></ol>
            <div data-success hidden class="flex flex-wrap items-center justify-between gap-3 rounded-card bg-mint-400/15 px-4 py-3 text-mint-50"></div>
        </div>`;

    const q = (sel) => workspace.querySelector(sel);
    const player = new Player(q('[data-player]'), {
        widescreen: config.widescreen ?? true,
        guides: config.guides ?? false,
        minDuration: config.duration ?? 3000,
        metronome: config.bpm ? { bpm: config.bpm, offset: config.offset ?? 0 } : undefined,
    });

    let generated = '';      // .osb produced by script mode
    let solutionOsb = null;  // the solution as .osb (scripts run once to get it)
    let attempts = 0;

    const status = q('[data-script-status]');
    const refresh = async (text) => {
        saveDraft(config.unitId, text);
        if (!isScript) {
            const model = player.load(text);
            const errors = model.diagnostics.filter((d) => d.severity === 'error').length;
            status.textContent = errors ? `${errors} ${errors === 1 ? 'error' : 'errors'}: hover the underlined lines in the editor.` : '';
            return;
        }
        status.textContent = 'Running…';
        const result = await runInWorker(text);
        if (result.cancelled) return;
        if (!result.ok) {
            status.innerHTML = `<span class="text-danger">${escape(result.error)}${result.line ? ` (line ${result.line})` : ''}</span>`;
            return;
        }
        generated = result.osb;
        q('[data-generated]').textContent = generated;
        status.textContent = `Generated ${result.objects} ${result.objects === 1 ? 'sprite' : 'sprites'} and ${result.commands} ${result.commands === 1 ? 'command' : 'commands'}.${result.logs.length ? ' Log: ' + result.logs.slice(-3).join(' | ') : ''}`;
        player.load(generated);
    };

    let timer;
    const editor = createEditor(q('[data-editor]'), {
        doc: loadDraft(config.unitId) ?? config.starter ?? '',
        mode: isScript ? 'script' : 'osb',
        label: config.title ? `Editor for ${config.title}` : undefined,
        onChange: (text) => {
            clearTimeout(timer);
            timer = setTimeout(() => refresh(text), isScript ? 500 : 200);
        },
    });
    refresh(editor.text);

    q('[data-reset]').addEventListener('click', () => {
        editor.text = config.starter ?? '';
        status.textContent = 'Back to the starting script. Ctrl+Z brings your version back.';
    });

    const solutionButton = q('[data-solution]');
    solutionButton.addEventListener('click', () => {
        editor.text = config.solution;
        status.textContent = 'This is one way to solve it. Ctrl+Z brings your version back.';
    });

    let hintIndex = 0;
    q('[data-hint]')?.addEventListener('click', (event) => {
        const hint = config.hints[hintIndex++];
        if (hint) {
            const li = document.createElement('li');
            li.textContent = hint;
            q('[data-hints]').append(li);
        }
        if (hintIndex >= config.hints.length) event.currentTarget.disabled = true;
    });

    q('[data-check]').addEventListener('click', async () => {
        clearTimeout(timer);
        await refresh(editor.text);
        let text = editor.text;
        if (isScript) {
            if (!generated) return;
            text = generated;
            if (solutionOsb == null) {
                const solution = await runInWorker(config.solution);
                solutionOsb = solution.ok ? solution.osb : '';
            }
        }
        const { passed, results } = runChecks(config.checks, text, { solutionText: isScript ? solutionOsb : config.solution });
        attempts++;
        const list = q('[data-results]');
        list.innerHTML = results.map((r) => `
            <li class="flex items-start gap-2 ${r.pass ? 'text-mint-200' : 'text-danger'}">
                ${r.pass ? checkIcon : crossIcon}
                <span><span class="${r.pass ? '' : 'font-semibold'}">${escape(r.label)}</span>${!r.pass && r.message ? `<span class="block text-mint-100/80">${escape(r.message)}</span>` : ''}</span>
            </li>`).join('');
        const passedCount = results.filter((r) => r.pass).length;
        q('[data-summary]').textContent = passed ? 'Everything checks out!' : `${passedCount} of ${results.length} checks pass. Keep going!`;
        if (!passed && attempts >= 1) {
            solutionButton.disabled = false;
            solutionButton.title = '';
        }
        if (passed) {
            player.seek(0);
            player.play();
            const progress = await learnReady;
            const outcome = await progress.complete(config.unitId);
            const success = q('[data-success]');
            success.hidden = false;
            success.innerHTML = `
                <p class="font-bold">Exercise complete!${outcome.xpGained ? ` <span class="text-gold-300">+${outcome.xpGained} XP</span>` : ' (already done before)'}</p>
                ${config.nextUrl ? `<a href="${config.nextUrl}" class="btn btn-primary">Next: ${escape(config.nextTitle ?? 'continue')}</a>` : ''}`;
        }
    });
}
