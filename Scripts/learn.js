// Learn pages: progress (XP, levels, badges), lesson completion, quizzes, code highlighting, and the
// interactive parts (previews, exercises, the playground), which load only on pages that use them.
// The server embeds the course catalog as JSON (#learn-catalog) and marks elements with data-*
// attributes; this script fills them in.

import { Progress } from './learn/progress.js';
import { provideLearn } from './learn/state.js';
import { celebrate } from './learn/ui.js';
import { mountQuiz } from './learn/quiz.js';
import { nextUnit } from './learn/gamification.js';
import { highlight } from './storyboard/highlight.js';

const escape = (s) => String(s).replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]);
const catalogElement = document.getElementById('learn-catalog');
const catalog = catalogElement ? JSON.parse(catalogElement.textContent) : null;

// Code blocks first: they don't depend on progress.
for (const code of document.querySelectorAll('.prose-osb pre > code[class*="language-"]')) {
    const language = /language-(\w+)/.exec(code.className)?.[1];
    code.innerHTML = highlight(code.textContent, language);
}
const osbBlocks = [...document.querySelectorAll('.prose-osb pre > code.language-osb')].map((c) => c.parentElement);
const scriptBlocks = [...document.querySelectorAll('.prose-osb pre > code.language-js')].map((c) => c.parentElement);
if (osbBlocks.length || scriptBlocks.length) {
    import('./playground/tryit.js').then(({ mountTryIt, mountScriptLink }) => {
        osbBlocks.forEach(mountTryIt);
        scriptBlocks.forEach(mountScriptLink);
    });
}
const explorers = document.querySelectorAll('[data-easing-explorer]');
if (explorers.length) import('./playground/easing-explorer.js').then(({ mountEasingExplorer }) => explorers.forEach(mountEasingExplorer));

if (catalog) {
    const progress = new Progress(catalog, {
        userId: document.body.dataset.userId,
        csrfToken: document.querySelector('meta[name="csrf-token"]')?.content,
    });
    await progress.init();
    provideLearn(progress);

    render(progress);
    progress.onChange((_, diff) => {
        render(progress);
        celebrate(diff);
    });

    for (const button of document.querySelectorAll('[data-complete-unit]')) {
        button.addEventListener('click', async () => {
            button.disabled = true;
            await progress.complete(button.dataset.completeUnit);
            const next = button.dataset.next;
            if (next) setTimeout(() => { location.href = next; }, 900);
        });
    }

    for (const form of document.querySelectorAll('form[data-quiz]')) mountQuiz(form, progress);
}

const exercise = document.querySelector('[data-exercise]');
if (exercise) import('./playground/exercise.js').then(({ mountExercise }) => mountExercise(exercise));
const sandbox = document.querySelector('[data-playground]');
if (sandbox) import('./playground/sandbox.js').then(({ mountSandbox }) => mountSandbox(sandbox));

/** Fills every progress hook on the page from the current stats. */
function render(progress) {
    const stats = progress.stats();

    for (const el of document.querySelectorAll('[data-unit]')) el.toggleAttribute('data-done', progress.isDone(el.dataset.unit));

    const fill = (el, done, total, complete) => {
        el.toggleAttribute('data-complete', complete);
        el.toggleAttribute('data-started', done > 0);
        const count = el.querySelector('[data-count]');
        if (count) count.textContent = `${done} / ${total}`;
        const bar = el.querySelector('[data-bar]');
        if (bar) bar.style.width = `${total ? (100 * done) / total : 0}%`;
    };
    for (const el of document.querySelectorAll('[data-module]')) {
        const m = stats.modules[el.dataset.module];
        if (m) fill(el, m.done, m.total, m.complete);
    }
    for (const el of document.querySelectorAll('[data-path]')) {
        const p = stats.paths[el.dataset.path];
        if (p) fill(el, p.unitsDone, p.unitsTotal, p.complete);
    }

    const values = {
        xp: stats.xp.toLocaleString(),
        level: stats.level,
        'level-title': stats.levelTitle,
        streak: stats.streak,
        'units-done': stats.unitsDone,
        'units-total': stats.unitsTotal,
        'badges-earned': stats.badges.filter((b) => b.earned).length,
        'badges-total': stats.badges.length,
        'next-level': stats.nextLevel ? `${(stats.nextLevel.xp - stats.xp).toLocaleString()} XP to level ${stats.nextLevel.level}` : 'Highest level reached',
    };
    for (const el of document.querySelectorAll('[data-stat]')) {
        if (values[el.dataset.stat] != null) el.textContent = values[el.dataset.stat];
    }
    for (const el of document.querySelectorAll('[data-stat-bar="level"]')) el.style.width = `${Math.round(stats.progressToNext * 100)}%`;
    for (const el of document.querySelectorAll('[data-streak]')) el.toggleAttribute('data-active', stats.streak > 0);

    for (const el of document.querySelectorAll('[data-sync-status]')) {
        el.textContent = progress.loggedIn
            ? (progress.synced ? 'Saved to your osu! account.' : 'Saved in this browser. It will sync to your account when the server is reachable.')
            : 'Saved in this browser. Log in with osu! to keep it on every device.';
    }

    for (const el of document.querySelectorAll('[data-continue]')) {
        // data-continue="module-slug" continues within that module; an empty value, the whole course.
        const scope = el.dataset.continue;
        const units = scope ? { ...catalog, units: catalog.units.filter((u) => u.module === scope) } : catalog;
        const upNext = nextUnit(units, progress.completions);
        if (!upNext) {
            if (scope) {
                el.querySelector('[data-continue-label]')?.replaceChildren('Review the module');
                continue;
            }
            el.href = '/learn/progress';
            el.querySelector('[data-continue-label]')?.replaceChildren('See your achievements');
            continue;
        }
        el.href = upNext.url;
        const started = units.units.some((u) => progress.isDone(u.id));
        el.querySelector('[data-continue-label]')?.replaceChildren(started ? `Continue: ${upNext.title}` : 'Start with the first lesson');
    }

    const badges = document.querySelector('[data-badges]');
    if (badges) badges.innerHTML = stats.badges.map(badgeHtml).join('');

    const history = document.querySelector('[data-history]');
    if (history) {
        const unitById = new Map(catalog.units.map((u) => [u.id, u]));
        const recent = Object.entries(progress.completions)
            .filter(([id]) => unitById.has(id))
            .sort((a, b) => b[1].at - a[1].at)
            .slice(0, 12);
        history.innerHTML = recent.length
            ? recent.map(([id, c]) => {
                const u = unitById.get(id);
                const score = c.max ? ` <span class="text-sage">(${c.score}/${c.max})</span>` : '';
                return `<li class="flex items-baseline justify-between gap-4 border-b border-charcoal-600 py-2 last:border-0">
                    <a href="${u.url}" class="min-w-0 truncate font-semibold">${escape(u.title)}</a>${score}
                    <time class="shrink-0 text-xs text-sage" datetime="${new Date(c.at).toISOString()}">${new Date(c.at).toLocaleDateString(undefined, { day: 'numeric', month: 'short' })}</time></li>`;
            }).join('')
            : '<li class="py-2 text-sage">Nothing yet. Your completed units will show up here.</li>';
    }
}

const BADGE_COLOURS = { beginner: 'text-mint-300', intermediate: 'text-gold-300', advanced: 'text-[#c5a3ff]' };

function badgeHtml(badge) {
    const colour = badge.kind === 'achievement' ? 'text-gold-300' : BADGE_COLOURS[badge.path] ?? 'text-mint-300';
    const shape = badge.kind === 'trophy'
        ? '<path d="M8 4.5h8v5a4 4 0 0 1-8 0v-5ZM8 6.5H5a3 3 0 0 0 3 3.5M16 6.5h3a3 3 0 0 1-3 3.5M12 13.5V17m-3.5 2.5h7M9.5 17h5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>'
        : badge.kind === 'module'
            ? '<path d="m12 3.5 2.6 5.27 5.82.85-4.21 4.1.99 5.79L12 16.77l-5.2 2.74.99-5.79-4.21-4.1 5.82-.85L12 3.5Z" fill="currentColor"/>'
            : '<path d="M10 3.5c.6 3.6 2.4 5.4 6 6-3.6.6-5.4 2.4-6 6-.6-3.6-2.4-5.4-6-6 3.6-.6 5.4-2.4 6-6ZM17.5 14c.3 1.8 1.2 2.7 3 3-1.8.3-2.7 1.2-3 3-.3-1.8-1.2-2.7-3-3 1.8-.3 2.7-1.2 3-3Z" fill="currentColor"/>';
    const kind = badge.kind === 'trophy' ? 'Path trophy' : badge.kind === 'module' ? 'Module badge' : 'Achievement';
    return `<li class="flex items-start gap-3 rounded-card p-3 ${badge.earned ? 'bg-charcoal-700' : 'bg-charcoal-900 opacity-60'}">
        <span class="flex size-11 shrink-0 items-center justify-center rounded-full ${badge.earned ? `bg-charcoal-950 ${colour}` : 'bg-charcoal-800 text-charcoal-500'}">
            <svg class="size-6" viewBox="0 0 24 24" aria-hidden="true">${shape}</svg>
        </span>
        <span class="min-w-0">
            <span class="block font-bold ${badge.earned ? 'text-mint-50' : 'text-mint-200'}">${escape(badge.name)}${badge.earned ? '' : '<span class="sr-only"> (not earned yet)</span>'}</span>
            <span class="block text-xs text-sage">${kind}${badge.description ? ` · ${escape(badge.description)}` : ''}</span>
        </span>
    </li>`;
}
