// The learner's progress: which units they completed, when, and their best quiz scores.
// Always kept in localStorage; when logged in with osu! it's also saved on the server
// (/api/learn/progress), so it follows the learner to other devices.
//
// - Progress made before logging in is merged into the account on the next page load.
// - If this browser holds someone else's account progress, it starts empty instead.
// - Completions that couldn't reach the server stay "pending" and are sent next time.

import { computeStats, diffStats } from './gamification.js';

const KEY = 'osb.learn.v1';
const empty = (owner = null) => ({ owner, completions: {}, pending: [] });

function readLocal() {
    try {
        const data = JSON.parse(localStorage.getItem(KEY));
        if (data && typeof data.completions === 'object') return { owner: data.owner ?? null, completions: data.completions, pending: data.pending ?? [] };
    } catch { /* unreadable or blocked storage */ }
    return empty();
}

function writeLocal(data) {
    try { localStorage.setItem(KEY, JSON.stringify(data)); } catch { /* private mode: progress lasts for this page only */ }
}

const pick = (completions, ids) => Object.fromEntries(ids.filter((id) => completions[id]).map((id) => [id, completions[id]]));

const toServer = (completions) => Object.fromEntries(Object.entries(completions).map(([id, c]) =>
    [id, { completedAt: new Date(c.at).toISOString(), score: c.score ?? null, maxScore: c.max ?? null }]));

const fromServer = (completions) => Object.fromEntries(Object.entries(completions).map(([id, c]) =>
    [id, { at: Date.parse(c.completedAt), ...(c.score != null ? { score: c.score, max: c.maxScore } : {}) }]));

export class Progress {
    #catalog;
    #userId;
    #token;
    #data = empty();
    #listeners = new Set();
    synced = false;

    constructor(catalog, { userId, csrfToken }) {
        this.#catalog = catalog;
        this.#userId = userId || null;
        this.#token = csrfToken;
    }

    get loggedIn() { return this.#userId != null; }
    get completions() { return this.#data.completions; }
    isDone(unitId) { return this.#data.completions[unitId] != null; }
    get(unitId) { return this.#data.completions[unitId] ?? null; }
    stats() { return computeStats(this.#catalog, this.#data.completions); }
    onChange(listener) { this.#listeners.add(listener); }

    async init() {
        this.#data = readLocal();
        if (!this.#userId) {
            // Logged out: progress cached for an account isn't shown to whoever uses this browser now.
            if (this.#data.owner != null) {
                this.#data = empty();
                writeLocal(this.#data);
            }
            return;
        }

        const anonymous = this.#data.owner == null;
        if (!anonymous && this.#data.owner !== this.#userId) this.#data = empty(this.#userId);
        const outgoing = anonymous ? this.#data.completions : pick(this.#data.completions, this.#data.pending);
        const merged = await this.#send(outgoing);
        if (merged) {
            this.#data = { owner: this.#userId, completions: merged, pending: [] };
            this.synced = true;
        } else if (anonymous) {
            // Offline: keep everything local and send it all next time.
            this.#data = { owner: this.#userId, completions: this.#data.completions, pending: Object.keys(this.#data.completions) };
        }
        writeLocal(this.#data);
    }

    /**
     * Records a completed unit (quizzes pass their score). Returns what changed:
     * { xpGained, levelUp, newBadges, alreadyDone }.
     */
    async complete(unitId, { score, max } = {}) {
        const before = this.stats();
        const existing = this.#data.completions[unitId];
        const entry = existing ? { ...existing } : { at: Date.now() };
        if (score != null && (existing?.score == null || score > existing.score)) {
            entry.score = score;
            entry.max = max;
        }
        if (existing && existing.score === entry.score) return { ...diffStats(before, before), alreadyDone: true };

        this.#data.completions[unitId] = entry;
        if (this.#userId) this.#data.pending = [...new Set([...this.#data.pending, unitId])];
        writeLocal(this.#data);

        const after = this.stats();
        const diff = { ...diffStats(before, after), alreadyDone: existing != null };
        for (const listener of this.#listeners) listener(after, diff);

        if (this.#userId) {
            this.#send(pick(this.#data.completions, this.#data.pending)).then((merged) => {
                if (!merged) return;
                this.#data = { owner: this.#userId, completions: { ...merged, ...this.#data.completions }, pending: [] };
                this.synced = true;
                writeLocal(this.#data);
            });
        }
        return diff;
    }

    /** Sends completions (or just fetches, when there's nothing to send) and returns the merged set. */
    async #send(completions) {
        const hasData = Object.keys(completions).length > 0;
        try {
            const response = await fetch('/api/learn/progress', {
                method: hasData ? 'POST' : 'GET',
                credentials: 'same-origin',
                headers: hasData ? { 'Content-Type': 'application/json', RequestVerificationToken: this.#token ?? '' } : {},
                body: hasData ? JSON.stringify({ completions: toServer(completions) }) : undefined,
            });
            if (!response.ok) return null;
            const json = await response.json();
            return fromServer(json.completions ?? {});
        } catch {
            return null;
        }
    }
}
