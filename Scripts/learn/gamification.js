// XP, levels, badges and streaks, all derived from the units a learner completed and the course
// catalog the server embeds in each learn page. Nothing derived is stored, so changing XP values or
// adding units never needs a data migration.
//
// Catalog shape (see LearnCatalog.ToClientJson on the server):
//   units:   [{ id, module, type: 'lesson'|'quiz'|'exercise', xp, title, url }]
//   modules: [{ slug, title, path, url, units: [unitId], bonus, badge: { name, description } }]
//   paths:   [{ slug, title, level, modules: [slug], bonus, trophy: { name, description } }]
//   levels:  [{ level, title, xp }]           (ascending)
//   achievements: [{ id, name, description }]
//   quiz:    { perfectBonus }

/** Local calendar day ("2026-10-03") of a timestamp, in the learner's own time zone. */
export function localDay(time) {
    const d = new Date(time);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function previousDay(day) {
    const [y, m, d] = day.split('-').map(Number);
    return localDay(new Date(y, m - 1, d - 1).getTime());
}

/** Days in a row with at least one completed unit, ending today (or yesterday, if today has none yet). */
export function streakOf(completions, now = Date.now()) {
    const days = new Set(Object.values(completions).map((c) => localDay(c.at)));
    let day = localDay(now);
    if (!days.has(day)) day = previousDay(day);
    let streak = 0;
    while (days.has(day)) {
        streak++;
        day = previousDay(day);
    }
    return streak;
}

/** The longest run of consecutive active days, for streak achievements earned in the past. */
function longestStreak(completions) {
    const days = [...new Set(Object.values(completions).map((c) => localDay(c.at)))].sort();
    let best = 0;
    let run = 0;
    let last = null;
    for (const day of days) {
        run = last && previousDay(day) === last ? run + 1 : 1;
        best = Math.max(best, run);
        last = day;
    }
    return best;
}

const isPerfect = (c) => c.max > 0 && c.score === c.max;

/** Rules for each achievement id, given the computed stats so far. */
const ACHIEVEMENT_RULES = {
    'first-steps': (s) => s.unitsDone >= 1,
    'hands-on': (s) => s.exercisesDone >= 1,
    'sharp-mind': (s) => s.perfectQuizzes >= 1,
    'quiz-master': (s) => s.perfectQuizzes >= 5,
    'on-a-roll': (s) => s.longestStreak >= 3,
    dedicated: (s) => s.longestStreak >= 7,
    wizardry: (s) => s.unitsTotal > 0 && s.unitsDone === s.unitsTotal,
};

/**
 * Everything the learn pages show about a learner's progress.
 * @param {object} catalog
 * @param {Record<string, {at: number, score?: number, max?: number}>} completions  by unit id
 */
export function computeStats(catalog, completions, now = Date.now()) {
    const done = (id) => completions[id] != null;
    const unitById = new Map(catalog.units.map((u) => [u.id, u]));
    const perfectBonus = catalog.quiz?.perfectBonus ?? 0;

    let xp = 0;
    let exercisesDone = 0;
    let perfectQuizzes = 0;
    let unitsDone = 0;
    for (const [id, c] of Object.entries(completions)) {
        const unit = unitById.get(id);
        if (!unit) continue; // a unit that no longer exists
        unitsDone++;
        xp += unit.xp;
        if (unit.type === 'exercise') exercisesDone++;
        if (unit.type === 'quiz' && isPerfect(c)) {
            perfectQuizzes++;
            xp += perfectBonus;
        }
    }

    const modules = {};
    const badges = [];
    for (const module of catalog.modules) {
        const finished = module.units.filter(done);
        const complete = module.units.length > 0 && finished.length === module.units.length;
        const earnedAt = complete ? Math.max(...module.units.map((id) => completions[id].at)) : null;
        modules[module.slug] = { done: finished.length, total: module.units.length, complete };
        if (complete) xp += module.bonus;
        badges.push({ id: `module:${module.slug}`, kind: 'module', name: module.badge.name, description: module.badge.description, path: module.path, earned: complete, earnedAt, url: module.url });
    }

    const paths = {};
    for (const path of catalog.paths) {
        const finished = path.modules.filter((slug) => modules[slug]?.complete);
        const complete = path.modules.length > 0 && finished.length === path.modules.length;
        const unitIds = catalog.modules.filter((m) => path.modules.includes(m.slug)).flatMap((m) => m.units);
        paths[path.slug] = {
            modulesDone: finished.length,
            modulesTotal: path.modules.length,
            unitsDone: unitIds.filter(done).length,
            unitsTotal: unitIds.length,
            complete,
        };
        if (complete) xp += path.bonus;
        const earnedAt = complete ? Math.max(...unitIds.map((id) => completions[id].at)) : null;
        badges.push({ id: `path:${path.slug}`, kind: 'trophy', name: path.trophy.name, description: path.trophy.description, path: path.slug, earned: complete, earnedAt });
    }

    const levels = catalog.levels;
    let current = levels[0];
    for (const l of levels) if (xp >= l.xp) current = l;
    const next = levels.find((l) => l.xp > xp) ?? null;

    const stats = {
        xp,
        level: current.level,
        levelTitle: current.title,
        levelXp: current.xp,
        nextLevel: next ? { level: next.level, title: next.title, xp: next.xp } : null,
        progressToNext: next ? (xp - current.xp) / (next.xp - current.xp) : 1,
        unitsDone,
        unitsTotal: catalog.units.length,
        exercisesDone,
        perfectQuizzes,
        streak: streakOf(completions, now),
        longestStreak: longestStreak(completions),
        modules,
        paths,
        badges,
    };

    for (const a of catalog.achievements ?? []) {
        const rule = ACHIEVEMENT_RULES[a.id];
        badges.push({ id: `achievement:${a.id}`, kind: 'achievement', name: a.name, description: a.description, earned: rule ? rule(stats) : false, earnedAt: null });
    }
    return stats;
}

/** What changed between two snapshots: XP gained, a new level, newly earned badges. */
export function diffStats(before, after) {
    const had = new Set(before.badges.filter((b) => b.earned).map((b) => b.id));
    return {
        xpGained: after.xp - before.xp,
        levelUp: after.level > before.level ? { level: after.level, title: after.levelTitle } : null,
        newBadges: after.badges.filter((b) => b.earned && !had.has(b.id)),
    };
}

/** The first unit the learner hasn't done, following the catalog order. */
export function nextUnit(catalog, completions) {
    return catalog.units.find((u) => completions[u.id] == null) ?? null;
}
