// Tests for learner progress in the browser (Scripts/learn/progress.js): what's kept in localStorage,
// and how it's merged with the account on the server. localStorage and fetch are replaced by fakes.

import { test, beforeEach } from 'node:test';
import assert from 'node:assert/strict';
import { Progress } from '../learn/progress.js';

const KEY = 'osb.learn.v1';

const catalog = {
    units: [
        { id: 'm1/a', module: 'm1', type: 'lesson', xp: 10 },
        { id: 'm1/b', module: 'm1', type: 'quiz', xp: 20 },
        { id: 'm2/a', module: 'm2', type: 'exercise', xp: 30 },
    ],
    modules: [
        { slug: 'm1', path: 'p', units: ['m1/a', 'm1/b'], bonus: 50, badge: { name: 'One', description: '' } },
        { slug: 'm2', path: 'p', units: ['m2/a'], bonus: 50, badge: { name: 'Two', description: '' } },
    ],
    paths: [{ slug: 'p', modules: ['m1', 'm2'], bonus: 150, trophy: { name: 'Path', description: '' } }],
    levels: [{ level: 1, title: 'Spectator', xp: 0 }, { level: 2, title: 'Sprite Wrangler', xp: 60 }],
    achievements: [],
    quiz: { perfectBonus: 5 },
};

class MemoryStorage {
    #items = new Map();
    getItem(key) { return this.#items.has(key) ? this.#items.get(key) : null; }
    setItem(key, value) { this.#items.set(key, String(value)); }
    removeItem(key) { this.#items.delete(key); }
}

const setGlobal = (name, value) => Object.defineProperty(globalThis, name, { value, configurable: true, writable: true });
const saved = () => JSON.parse(localStorage.getItem(KEY));
const save = (data) => localStorage.setItem(KEY, JSON.stringify(data));
const settle = () => new Promise((resolve) => setTimeout(resolve, 10));

/** A fake /api/learn/progress for one account: it keeps what it's sent and answers with everything it has. */
function fakeServer(completions = {}) {
    const server = { online: true, completions: { ...completions }, requests: [] };
    setGlobal('fetch', async (url, options = {}) => {
        const body = options.body ? JSON.parse(options.body) : null;
        server.requests.push({ url, method: options.method, token: options.headers?.RequestVerificationToken, completions: body?.completions });
        if (!server.online) throw new TypeError('Failed to fetch');
        for (const [id, c] of Object.entries(body?.completions ?? {})) server.completions[id] ??= c;
        return { ok: true, json: async () => ({ completions: server.completions }) };
    });
    return server;
}

beforeEach(() => {
    setGlobal('localStorage', new MemoryStorage());
    setGlobal('fetch', async () => { throw new Error('Nothing should call the server here.'); });
});

test('logged out, progress stays in this browser', async () => {
    const progress = new Progress(catalog, { userId: null });
    await progress.init();

    await progress.complete('m1/a');

    assert.equal(progress.isDone('m1/a'), true);
    assert.deepEqual(Object.keys(saved().completions), ['m1/a']);
    assert.equal(saved().owner, null);
});

test("logged out, someone else's saved progress isn't shown", async () => {
    save({ owner: '42', completions: { 'm1/a': { at: 1 } }, pending: [] });

    const progress = new Progress(catalog, { userId: null });
    await progress.init();

    assert.deepEqual(progress.completions, {});
    assert.deepEqual(saved().completions, {});
});

test('logging in moves progress made before into the account', async () => {
    save({ owner: null, completions: { 'm1/a': { at: Date.parse('2026-10-01T10:00:00Z') } }, pending: [] });
    const server = fakeServer({ 'm2/a': { completedAt: '2026-09-30T08:00:00Z', score: null, maxScore: null } });

    const progress = new Progress(catalog, { userId: '42', csrfToken: 'token' });
    await progress.init();

    assert.equal(server.requests.length, 1);
    assert.equal(server.requests[0].method, 'POST');
    assert.equal(server.requests[0].token, 'token');
    assert.deepEqual(server.requests[0].completions, { 'm1/a': { completedAt: '2026-10-01T10:00:00.000Z', score: null, maxScore: null } });
    assert.deepEqual(Object.keys(progress.completions).sort(), ['m1/a', 'm2/a']);
    assert.equal(progress.synced, true);
    assert.equal(saved().owner, '42');
    assert.deepEqual(saved().pending, []);
});

test("another account's saved progress is replaced by this account's", async () => {
    save({ owner: '7', completions: { 'm1/a': { at: 1 } }, pending: ['m1/a'] });
    const server = fakeServer({ 'm2/a': { completedAt: '2026-09-30T08:00:00Z', score: null, maxScore: null } });

    const progress = new Progress(catalog, { userId: '42', csrfToken: 'token' });
    await progress.init();

    assert.equal(server.requests[0].method, 'GET', 'nothing of the other account is sent');
    assert.deepEqual(Object.keys(progress.completions), ['m2/a']);
});

test('offline, logging in keeps local progress and sends it next time', async () => {
    save({ owner: null, completions: { 'm1/a': { at: 1000 } }, pending: [] });
    const server = fakeServer();
    server.online = false;

    const offline = new Progress(catalog, { userId: '42', csrfToken: 'token' });
    await offline.init();

    assert.equal(offline.isDone('m1/a'), true);
    assert.equal(offline.synced, false);
    assert.deepEqual(saved().pending, ['m1/a']);

    server.online = true;
    const later = new Progress(catalog, { userId: '42', csrfToken: 'token' });
    await later.init();

    assert.deepEqual(Object.keys(server.requests.at(-1).completions), ['m1/a']);
    assert.deepEqual(saved().pending, []);
    assert.equal(later.synced, true);
});

test('logged in, each completion is sent; if that fails it stays pending', async () => {
    const server = fakeServer();
    const progress = new Progress(catalog, { userId: '42', csrfToken: 'token' });
    await progress.init();

    await progress.complete('m1/a');
    await settle();
    assert.ok(server.completions['m1/a'], 'the server got the completion');
    assert.deepEqual(saved().pending, []);

    server.online = false;
    await progress.complete('m2/a');
    await settle();
    assert.deepEqual(saved().pending, ['m2/a']);
});

test('a retry keeps the best quiz score, and repeating a unit earns nothing', async () => {
    const progress = new Progress(catalog, { userId: null });
    await progress.init();

    assert.equal((await progress.complete('m1/b', { score: 2, max: 4 })).xpGained, 20);

    const worse = await progress.complete('m1/b', { score: 1, max: 4 });
    assert.equal(worse.alreadyDone, true);
    assert.equal(worse.xpGained, 0);
    assert.equal(progress.get('m1/b').score, 2);

    const perfect = await progress.complete('m1/b', { score: 4, max: 4 });
    assert.equal(perfect.xpGained, 5, 'only the perfect-score bonus is new');
    assert.equal(progress.get('m1/b').score, 4);

    await progress.complete('m1/a');
    assert.equal((await progress.complete('m1/a')).xpGained, 0);
});

test('listeners hear about every change, and only changes', async () => {
    const progress = new Progress(catalog, { userId: null });
    await progress.init();
    const heard = [];
    progress.onChange((stats, diff) => heard.push({ xp: stats.xp, gained: diff.xpGained, badges: diff.newBadges.map((b) => b.id) }));

    await progress.complete('m1/a');
    await progress.complete('m1/a');
    await progress.complete('m1/b', { score: 4, max: 4 });

    assert.deepEqual(heard, [
        { xp: 10, gained: 10, badges: [] },
        { xp: 10 + 20 + 5 + 50, gained: 75, badges: ['module:m1'] },
    ]);
});

test('unreadable saved progress starts empty instead of breaking the page', async () => {
    localStorage.setItem(KEY, 'not json');

    const progress = new Progress(catalog, { userId: null });
    await progress.init();

    assert.deepEqual(progress.completions, {});
});
