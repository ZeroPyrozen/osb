import { test } from 'node:test';
import assert from 'node:assert/strict';
import { computeStats, diffStats, streakOf, localDay, nextUnit } from '../learn/gamification.js';

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
    levels: [{ level: 1, title: 'Spectator', xp: 0 }, { level: 2, title: 'Sprite Wrangler', xp: 60 }, { level: 3, title: 'Fade Apprentice', xp: 150 }],
    achievements: [{ id: 'first-steps', name: 'First steps' }, { id: 'sharp-mind', name: 'Sharp mind' }, { id: 'hands-on', name: 'Hands on' }, { id: 'wizardry', name: 'Wizardry' }],
    quiz: { perfectBonus: 5 },
};

const day = (d, h = 12) => new Date(2026, 9, d, h).getTime(); // October 2026, local time

test('XP adds unit XP, perfect quiz bonus, module and path bonuses', () => {
    const s = computeStats(catalog, {
        'm1/a': { at: day(1) },
        'm1/b': { at: day(1), score: 4, max: 4 },
        'm2/a': { at: day(2) },
    }, day(2));
    assert.equal(s.xp, 10 + 20 + 5 + 50 + 30 + 50 + 150);
    assert.equal(s.level, 3);
    assert.equal(s.nextLevel, null);
    assert.equal(s.paths.p.complete, true);
    const earned = s.badges.filter((b) => b.earned).map((b) => b.id);
    assert.deepEqual(earned.sort(), ['achievement:first-steps', 'achievement:hands-on', 'achievement:sharp-mind', 'achievement:wizardry', 'module:m1', 'module:m2', 'path:p'].sort());
});

test('levels and progress towards the next one', () => {
    const s = computeStats(catalog, { 'm1/a': { at: day(1) }, 'm1/b': { at: day(1), score: 3, max: 4 } }, day(1));
    assert.equal(s.xp, 10 + 20 + 50); // no perfect bonus, module bonus
    assert.equal(s.level, 2);
    assert.equal(s.nextLevel.xp, 150);
    assert.ok(Math.abs(s.progressToNext - (80 - 60) / 90) < 1e-9);
});

test('unknown unit ids (removed lessons) are ignored', () => {
    const s = computeStats(catalog, { 'gone/x': { at: day(1) } }, day(1));
    assert.equal(s.xp, 0);
    assert.equal(s.unitsDone, 0);
});

test('streaks count consecutive local days ending today or yesterday', () => {
    const c = { a: { at: day(1) }, b: { at: day(2) }, c: { at: day(3, 23) }, d: { at: day(5) } };
    assert.equal(streakOf(c, day(5, 18)), 1);
    assert.equal(streakOf(c, day(4, 9)), 3, 'yesterday counts until the day ends');
    assert.equal(streakOf(c, day(7)), 0);
    assert.equal(localDay(day(3, 23)), '2026-10-03');
});

test('diffStats reports XP, level-ups and new badges', () => {
    const before = computeStats(catalog, { 'm1/a': { at: day(1) } }, day(1));
    const after = computeStats(catalog, { 'm1/a': { at: day(1) }, 'm1/b': { at: day(1), score: 4, max: 4 } }, day(1));
    const diff = diffStats(before, after);
    assert.equal(diff.xpGained, 20 + 5 + 50);
    assert.deepEqual(diff.levelUp, { level: 2, title: 'Sprite Wrangler' });
    assert.deepEqual(diff.newBadges.map((b) => b.id).sort(), ['achievement:sharp-mind', 'module:m1']);
    assert.equal(nextUnit(catalog, { 'm1/a': { at: 1 } }).id, 'm1/b');
});

/** One module of `count` lessons, with every achievement rule the site knows plus one it doesn't. */
const bigCatalog = (count, type = 'lesson') => ({
    ...catalog,
    units: Array.from({ length: count }, (_, i) => ({ id: `big/${i}`, module: 'big', type, xp: 10 })),
    modules: [{ slug: 'big', path: 'p', units: Array.from({ length: count }, (_, i) => `big/${i}`), bonus: 50, badge: { name: 'Big', description: '' } }],
    paths: [{ slug: 'p', modules: ['big'], bonus: 150, trophy: { name: 'Path', description: '' } }],
    achievements: ['on-a-roll', 'dedicated', 'quiz-master', 'no-such-rule'].map((id) => ({ id, name: id })),
});
const earnedIds = (stats) => stats.badges.filter((b) => b.earned).map((b) => b.id);

test('streak achievements count the longest run of days, even one that has ended', () => {
    const threeDays = { 'big/0': { at: day(1) }, 'big/1': { at: day(2) }, 'big/2': { at: day(3) }, 'big/3': { at: day(10) } };
    const s = computeStats(bigCatalog(8), threeDays, day(20));
    assert.equal(s.streak, 0);
    assert.equal(s.longestStreak, 3);
    assert.ok(earnedIds(s).includes('achievement:on-a-roll'));
    assert.ok(!earnedIds(s).includes('achievement:dedicated'));

    const week = Object.fromEntries(Array.from({ length: 7 }, (_, i) => [`big/${i}`, { at: day(i + 1) }]));
    assert.ok(earnedIds(computeStats(bigCatalog(8), week, day(7))).includes('achievement:dedicated'));
});

test('quiz master takes five perfect knowledge checks', () => {
    const perfect = (n) => Object.fromEntries(Array.from({ length: n }, (_, i) => [`big/${i}`, { at: day(1), score: 3, max: 3 }]));
    const quizzes = bigCatalog(6, 'quiz');
    assert.ok(!earnedIds(computeStats(quizzes, perfect(4), day(1))).includes('achievement:quiz-master'));
    assert.ok(earnedIds(computeStats(quizzes, perfect(5), day(1))).includes('achievement:quiz-master'));
});

test('achievements without a rule are never earned', () => {
    const everything = Object.fromEntries(Array.from({ length: 2 }, (_, i) => [`big/${i}`, { at: day(1) }]));
    const s = computeStats(bigCatalog(2), everything, day(1));
    assert.equal(s.badges.find((b) => b.id === 'achievement:no-such-rule').earned, false);
});

test('badges remember when their last unit was done', () => {
    const s = computeStats(catalog, { 'm1/a': { at: day(1) }, 'm1/b': { at: day(3) }, 'm2/a': { at: day(2) } }, day(3));
    assert.equal(s.badges.find((b) => b.id === 'module:m1').earnedAt, day(3));
    assert.equal(s.badges.find((b) => b.id === 'module:m2').earnedAt, day(2));
    assert.equal(s.badges.find((b) => b.id === 'path:p').earnedAt, day(3));
});

test('at the highest level, the progress bar is full', () => {
    const s = computeStats(catalog, { 'm1/a': { at: day(1) }, 'm1/b': { at: day(1), score: 4, max: 4 }, 'm2/a': { at: day(1) } }, day(1));
    assert.equal(s.nextLevel, null);
    assert.equal(s.progressToNext, 1);
});

test('when every unit is done there is no next unit', () => {
    assert.equal(nextUnit(catalog, { 'm1/a': { at: 1 }, 'm1/b': { at: 1 }, 'm2/a': { at: 1 } }), null);
    assert.equal(nextUnit(catalog, {}).id, 'm1/a');
});
