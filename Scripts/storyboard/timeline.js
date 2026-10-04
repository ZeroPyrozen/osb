// Turns a parsed storyboard into per-property timelines and answers "what does this object look like
// at time t?". The rules mirror osu!:
// - an object exists from its first command's start to its last command's end;
// - before a property's first command it already has that command's start value;
// - after a command ends its end value holds; when commands overlap, the one that started last wins;
// - Move feeds the same x/y as MoveX/MoveY, and Scale multiplies with Vector scale;
// - loops repeat their commands every (last end - first start) ms of the loop body;
// - Parameter commands apply only while they run, except instant ones, which last the object's life;
// - triggers depend on gameplay, so they are kept but not played.

import { ease } from './easing.js';

const DEFAULTS = { opacity: 1, scale: 1, rotation: 0 };

/** Compiles a parse() result. Returns objects in declaration order, ready for stateAt(). */
export function compile(model) {
    return model.objects.map((source, index) => compileObject(source, index));
}

function compileObject(source, index) {
    const tracks = { x: [], y: [], scale: [], vscale: [], rotation: [], color: [], opacity: [] };
    const params = { H: [], V: [], A: [] };

    const add = (cmd, offset) => {
        const start = cmd.start + offset;
        const end = cmd.end + offset;
        const seg = (from, to) => ({ start, end, from, to, easing: cmd.easing, line: cmd.line });
        switch (cmd.type) {
            case 'M': tracks.x.push(seg(cmd.from[0], cmd.to[0])); tracks.y.push(seg(cmd.from[1], cmd.to[1])); break;
            case 'MX': tracks.x.push(seg(cmd.from[0], cmd.to[0])); break;
            case 'MY': tracks.y.push(seg(cmd.from[0], cmd.to[0])); break;
            case 'S': tracks.scale.push(seg(cmd.from[0], cmd.to[0])); break;
            case 'V': tracks.vscale.push(seg(cmd.from, cmd.to)); break;
            case 'R': tracks.rotation.push(seg(cmd.from[0], cmd.to[0])); break;
            case 'C': tracks.color.push(seg(cmd.from, cmd.to)); break;
            case 'F': tracks.opacity.push(seg(cmd.from[0], cmd.to[0])); break;
            case 'P': params[cmd.param].push({ start, end, instant: cmd.start === cmd.end, line: cmd.line }); break;
        }
    };

    for (const cmd of source.commands) add(cmd, 0);

    for (const loop of source.loops) {
        if (loop.commands.length === 0) continue;
        const bodyStart = Math.min(...loop.commands.map((c) => c.start));
        const bodyEnd = Math.max(...loop.commands.map((c) => c.end));
        const period = bodyEnd - bodyStart;
        const iterations = period > 0 ? loop.count : 1;
        for (let i = 0; i < iterations; i++)
            for (const cmd of loop.commands) add(cmd, loop.start + i * period);
    }

    let start = Infinity;
    let end = -Infinity;
    for (const list of [...Object.values(tracks), ...Object.values(params)]) {
        list.sort((a, b) => a.start - b.start || a.end - b.end);
        for (const s of list) {
            if (s.start < start) start = s.start;
            if (s.end > end) end = s.end;
        }
    }

    return { index, source, tracks, params, start, end };
}

/** Index of the last segment starting at or before t (segments are sorted by start), or -1. */
function lastStartedAt(list, t) {
    let lo = 0;
    let hi = list.length - 1;
    let found = -1;
    while (lo <= hi) {
        const mid = (lo + hi) >> 1;
        if (list[mid].start <= t) {
            found = mid;
            lo = mid + 1;
        } else {
            hi = mid - 1;
        }
    }
    return found;
}

function lerp(from, to, p) {
    if (Array.isArray(from)) return from.map((f, i) => f + (to[i] - f) * p);
    return from + (to - from) * p;
}

/** The value of one property track at time t, or `fallback` if no command sets it. */
export function valueAt(list, t, fallback) {
    if (list.length === 0) return fallback;
    if (t < list[0].start) return list[0].from;
    const seg = list[lastStartedAt(list, t)];
    if (t >= seg.end) return seg.to;
    return lerp(seg.from, seg.to, ease(seg.easing, (t - seg.start) / (seg.end - seg.start)));
}

function paramOn(list, t) {
    return list.some((p) => p.instant || (t >= p.start && t <= p.end));
}

/**
 * What a compiled object looks like at time t: null when it doesn't exist at that time,
 * otherwise its position, scale, rotation (radians), colour (0-255), opacity and parameters.
 */
export function stateAt(object, t) {
    if (!(t >= object.start && t <= object.end)) return null;
    const { tracks, params, source } = object;
    let opacity = valueAt(tracks.opacity, t, DEFAULTS.opacity);
    // osu!stable wraps opacity above 1 (so 1.5 shows as 0.5); storyboarders use it for flicker effects.
    if (opacity > 1) opacity %= 1;
    const scale = valueAt(tracks.scale, t, DEFAULTS.scale);
    const vscale = valueAt(tracks.vscale, t, [1, 1]);
    return {
        x: valueAt(tracks.x, t, source.x),
        y: valueAt(tracks.y, t, source.y),
        scaleX: scale * vscale[0],
        scaleY: scale * vscale[1],
        rotation: valueAt(tracks.rotation, t, DEFAULTS.rotation),
        color: valueAt(tracks.color, t, [255, 255, 255]),
        opacity: Math.max(0, opacity),
        flipH: paramOn(params.H, t),
        flipV: paramOn(params.V, t),
        additive: paramOn(params.A, t),
    };
}

/** Which animation frame shows at time t (frames count from the object's first command). */
export function frameAt(object, t) {
    const { frameCount, frameDelay, loopType } = object.source;
    if (!frameCount || !(frameDelay > 0)) return 0;
    const frame = Math.max(0, Math.floor((t - object.start) / frameDelay));
    return loopType === 'LoopOnce' ? Math.min(frame, frameCount - 1) : frame % frameCount;
}

/** The time span the storyboard covers: from its earliest command to its latest. */
export function storyboardSpan(objects) {
    let start = Infinity;
    let end = -Infinity;
    for (const o of objects) {
        if (o.start < start) start = o.start;
        if (o.end > end) end = o.end;
    }
    return Number.isFinite(start) ? { start, end } : { start: 0, end: 0 };
}
