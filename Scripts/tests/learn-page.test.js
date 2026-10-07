// Tests for Scripts/learn.js on a progress page: it fills the page from the learner's saved progress.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { open, fresh } from './dom.js';

const catalog = {
    units: [
        { id: 'm1/a', module: 'm1', type: 'lesson', xp: 10, title: 'Your first sprite', url: '/learn/m1/a' },
        { id: 'm1/b', module: 'm1', type: 'quiz', xp: 20, title: 'Knowledge check', url: '/learn/m1/b' },
        { id: 'm2/a', module: 'm2', type: 'exercise', xp: 30, title: 'Loop it <fast>', url: '/learn/m2/a' },
    ],
    modules: [
        { slug: 'm1', title: 'Sprites', path: 'beginner', url: '/learn/m1', units: ['m1/a', 'm1/b'], bonus: 50, badge: { name: 'Picture Hanger', description: 'Finished Sprites.' } },
        { slug: 'm2', title: 'Loops', path: 'intermediate', url: '/learn/m2', units: ['m2/a'], bonus: 50, badge: { name: 'Loop Tamer', description: 'Finished Loops.' } },
    ],
    paths: [
        { slug: 'beginner', title: 'Your first storyboard', level: 'Beginner', modules: ['m1'], bonus: 150, trophy: { name: 'First Light', description: '' } },
        { slug: 'intermediate', title: 'Motion and music', level: 'Intermediate', modules: ['m2'], bonus: 150, trophy: { name: 'In Sync', description: '' } },
    ],
    levels: [{ level: 1, title: 'Spectator', xp: 0 }, { level: 2, title: 'Sprite Wrangler', xp: 60 }, { level: 3, title: 'Fade Apprentice', xp: 160 }],
    achievements: [{ id: 'first-steps', name: 'First steps', description: 'Complete your first unit.' }],
    quiz: { perfectBonus: 5 },
};

/** The hooks Views/Learn/Progress.cshtml gives learn.js, with this learner's saved completions. */
async function progressPage(completions) {
    open(`
        <script type="application/json" id="learn-catalog">${JSON.stringify(catalog)}</script>
        <span data-stat="level"></span> <span data-stat="level-title"></span> <span data-stat="xp"></span>
        <span data-stat="next-level"></span> <span data-stat="units-done"></span>
        <span data-stat="badges-earned"></span>/<span data-stat="badges-total"></span>
        <a href="/learn/m1/a" data-continue=""><span data-continue-label>Start with the first lesson</span></a>
        <div data-module="m1"><span data-count></span><span data-bar></span></div>
        <a data-unit="m1/a"></a>
        <p data-sync-status></p>
        <ul data-badges></ul>
        <ul data-history></ul>`);
    localStorage.setItem('osb.learn.v1', JSON.stringify({ owner: null, completions, pending: [] }));
    await fresh('../learn.js');
}

const text = (selector) => document.querySelector(selector).textContent.trim();
const badge = (name) => [...document.querySelectorAll('[data-badges] li')].find((li) => li.textContent.includes(name));
const at = Date.parse('2026-10-01T10:00:00Z');

/** learn.js used to read its badge colours before declaring them, so this page crashed while loading. */
test('the progress page shows every badge, in its path colour once earned', async () => {
    await progressPage({ 'm1/a': { at }, 'm1/b': { at, score: 3, max: 3 }, 'm2/a': { at } });

    assert.equal(document.querySelectorAll('[data-badges] li').length, 5);
    assert.equal(text('[data-stat="badges-earned"]'), '5');
    assert.equal(text('[data-stat="badges-total"]'), '5');
    assert.match(badge('Picture Hanger').innerHTML, /text-mint-300/);
    assert.match(badge('Loop Tamer').innerHTML, /text-gold-300/);
    assert.match(badge('In Sync').innerHTML, /text-gold-300/);
    assert.match(badge('First steps').innerHTML, /Achievement/);
});

test('stats, the continue link and the history follow the saved progress', async () => {
    await progressPage({ 'm1/a': { at } });

    assert.equal(text('[data-stat="xp"]'), '10');
    assert.equal(text('[data-stat="level-title"]'), 'Spectator');
    assert.equal(text('[data-stat="next-level"]'), '50 XP to level 2');
    assert.equal(text('[data-stat="units-done"]'), '1');
    assert.equal(document.querySelector('[data-continue]').getAttribute('href'), '/learn/m1/b');
    assert.equal(text('[data-continue-label]'), 'Continue: Knowledge check');
    assert.equal(text('[data-module="m1"] [data-count]'), '1 / 2');
    assert.ok(document.querySelector('[data-module="m1"]').hasAttribute('data-started'));
    assert.ok(!document.querySelector('[data-module="m1"]').hasAttribute('data-complete'));
    assert.ok(document.querySelector('[data-unit="m1/a"]').hasAttribute('data-done'));
    assert.equal(text('[data-sync-status]'), 'Saved in this browser. Log in with osu! to keep it on every device.');
    assert.equal(document.querySelectorAll('[data-history] li').length, 1);
    assert.equal(document.querySelector('[data-history] a').getAttribute('href'), '/learn/m1/a');
    assert.ok(!badge('Picture Hanger').innerHTML.includes('text-mint-300'), 'unearned badges are greyed out');
});

test('when everything is done, continue leads to the achievements', async () => {
    await progressPage({ 'm1/a': { at }, 'm1/b': { at, score: 2, max: 3 }, 'm2/a': { at } });

    assert.equal(document.querySelector('[data-continue]').getAttribute('href'), '/learn/progress');
    assert.equal(text('[data-continue-label]'), 'See your achievements');
    assert.match(document.querySelector('[data-history]').innerHTML, /\(2\/3\)/);
});

test('unit titles are shown as text, never as markup', async () => {
    await progressPage({ 'm2/a': { at } });

    assert.match(document.querySelector('[data-history]').innerHTML, /Loop it &lt;fast&gt;/);
});
