// Exercise checks: small declarative tests that lesson authors write in a unit's YAML front matter,
// for example:
//
//   checks:
//     - kind: value
//       time: 1500
//       property: opacity
//       equals: 0.5
//       label: Halfway through the fade the dot is half visible
//
// Kinds: parses, count, object, command, value, visible, lifetime, matches-solution.
// Objects are picked with an optional `object:` matcher (path, layer, origin, is: sprite|animation, index,
// and for animations frameCount, frameDelay, loopType). The `time` of value and visible checks can also be
// `start` or `end`: each object's own first or last moment, for checks over many generated objects.

import { parse } from './parse.js';
import { compile, stateAt, storyboardSpan } from './timeline.js';

const DEFAULT_TOLERANCE = 0.01;

function prepare(text) {
    const model = parse(text);
    return { model, objects: compile(model), errors: model.diagnostics.filter((d) => d.severity === 'error') };
}

function matches(object, matcher) {
    if (!matcher) return true;
    const src = object.source;
    if (matcher.index != null && object.index !== matcher.index) return false;
    if (matcher.path != null && src.path.toLowerCase() !== String(matcher.path).toLowerCase()) return false;
    if (matcher.layer != null && src.layer !== matcher.layer) return false;
    if (matcher.origin != null && src.origin !== matcher.origin) return false;
    if (matcher.is != null && src.kind !== matcher.is) return false;
    if (matcher.frameCount != null && src.frameCount !== matcher.frameCount) return false;
    if (matcher.frameDelay != null && !(Math.abs(src.frameDelay - matcher.frameDelay) <= 0.5)) return false;
    if (matcher.loopType != null && src.loopType !== matcher.loopType) return false;
    return true;
}

/** A check's time for one object: a number, or "start"/"end" for the object's own lifetime. */
function timeFor(check, object) {
    if (check.time === 'start') return object.start;
    if (check.time === 'end') return object.end;
    return check.time;
}

function select(objects, matcher) {
    return objects.filter((o) => matches(o, matcher));
}

function within(actual, check) {
    const tolerance = check.tolerance ?? DEFAULT_TOLERANCE;
    if (check.equals != null) {
        const expected = Array.isArray(check.equals) ? check.equals : [check.equals];
        const values = Array.isArray(actual) ? actual : [actual];
        return expected.length === values.length && expected.every((e, i) => Math.abs(values[i] - e) <= tolerance);
    }
    if (check.min != null && !(actual >= check.min - tolerance)) return false;
    if (check.max != null && !(actual <= check.max + tolerance)) return false;
    return true;
}

/** Reads one property from a state; colours are 0-255 and rotation is in radians. */
function property(state, name) {
    switch (name) {
        case 'opacity': return state ? state.opacity : 0;
        case 'visible': return state != null && state.opacity > 0.001;
        case 'x': case 'y': case 'scaleX': case 'scaleY': case 'rotation':
            return state ? state[name] : undefined;
        case 'scale': return state ? state.scaleX : undefined;
        case 'position': return state ? [state.x, state.y] : undefined;
        case 'color': return state ? state.color : undefined;
        case 'r': return state ? state.color[0] : undefined;
        case 'g': return state ? state.color[1] : undefined;
        case 'b': return state ? state.color[2] : undefined;
        default: throw new Error(`Unknown property "${name}" in an exercise check.`);
    }
}

function allCommands(object) {
    const source = object.source;
    return [...source.commands, ...source.loops.flatMap((l) => l.commands), ...source.triggers.flatMap((t) => t.commands)];
}

function sameValues(a, b, tolerance) {
    if (Array.isArray(a)) return a.length === b.length && a.every((v, i) => Math.abs(v - b[i]) <= tolerance);
    return Math.abs(a - b) <= tolerance;
}

const kinds = {
    parses(check, ctx) {
        return ctx.errors.length === 0;
    },

    count(check, ctx) {
        const objects = select(ctx.objects, check.object);
        let n;
        switch (check.of ?? 'objects') {
            case 'objects': n = objects.length; break;
            case 'sprites': n = objects.filter((o) => o.source.kind === 'sprite').length; break;
            case 'animations': n = objects.filter((o) => o.source.kind === 'animation').length; break;
            case 'samples': n = ctx.model.samples.length; break;
            case 'loops': n = objects.reduce((sum, o) => sum + o.source.loops.length, 0); break;
            case 'triggers': n = objects.reduce((sum, o) => sum + o.source.triggers.length, 0); break;
            case 'commands': n = objects.flatMap(allCommands).filter((c) => !check.command || c.type === check.command).length; break;
            case 'command-lines': n = new Set(objects.flatMap(allCommands).filter((c) => !check.command || c.type === check.command).map((c) => c.line)).size; break;
            case 'easings': n = new Set(objects.flatMap(allCommands).map((c) => c.easing)).size; break;
            default: throw new Error(`Unknown count "${check.of}" in an exercise check.`);
        }
        return within(n, { ...check, tolerance: 0 });
    },

    object(check, ctx) {
        return ctx.objects.some((o) => matches(o, check)
            && (check.x == null || Math.abs(o.source.x - check.x) <= (check.tolerance ?? 0.5))
            && (check.y == null || Math.abs(o.source.y - check.y) <= (check.tolerance ?? 0.5)));
    },

    command(check, ctx) {
        const tolerance = check.tolerance ?? DEFAULT_TOLERANCE;
        const wantedEasing = check.easing;
        return select(ctx.objects, check.object).some((o) => allCommands(o).some((c) =>
            (check.type == null || c.type === check.type)
            && (wantedEasing == null || (Array.isArray(wantedEasing) ? wantedEasing.includes(c.easing) : c.easing === wantedEasing))
            && (check.start == null || Math.abs(c.start - check.start) <= 1)
            && (check.end == null || Math.abs(c.end - check.end) <= 1)
            && (check.from == null || sameValues(c.from, [].concat(check.from), tolerance))
            && (check.to == null || sameValues(c.to, [].concat(check.to), tolerance))
            && (check.param == null || c.param === check.param)));
    },

    value(check, ctx) {
        const objects = select(ctx.objects, check.object);
        if (objects.length === 0) return false;
        const test = (o) => within(property(stateAt(o, timeFor(check, o)), check.property), check);
        return check.every ? objects.every(test) : test(objects[0]);
    },

    visible(check, ctx) {
        const objects = select(ctx.objects, check.object);
        if (objects.length === 0) return check.expect === false;
        const test = (o) => property(stateAt(o, timeFor(check, o)), 'visible') === (check.expect ?? true);
        return check.every ? objects.every(test) : test(objects[0]);
    },

    lifetime(check, ctx) {
        const objects = select(ctx.objects, check.object);
        const tolerance = check.tolerance ?? 1;
        const test = (o) => (check.start == null || Math.abs(o.start - check.start) <= tolerance)
            && (check.end == null || Math.abs(o.end - check.end) <= tolerance)
            && (check.endsBy == null || o.end <= check.endsBy + tolerance);
        return objects.length > 0 && (check.every ? objects.every(test) : test(objects[0]));
    },

    'matches-solution'(check, ctx) {
        if (!ctx.solution) throw new Error('A matches-solution check needs the exercise solution.');
        const mine = ctx.objects;
        const theirs = ctx.solution.objects;
        if (mine.length !== theirs.length) return false;
        const span = storyboardSpan(theirs);
        const step = check.step ?? 50;
        const tolerance = check.tolerance ?? 0.5;
        for (let t = span.start; t <= span.end + 0.001; t += step) {
            for (let i = 0; i < theirs.length; i++) {
                const a = stateAt(mine[i], t);
                const b = stateAt(theirs[i], t);
                const visibleA = a != null && a.opacity > 0.001;
                const visibleB = b != null && b.opacity > 0.001;
                if (visibleA !== visibleB) return false;
                if (!visibleB) continue;
                if (Math.abs(a.x - b.x) > tolerance || Math.abs(a.y - b.y) > tolerance) return false;
                if (Math.abs(a.opacity - b.opacity) > 0.02) return false;
                if (Math.abs(a.scaleX - b.scaleX) > 0.02 || Math.abs(a.scaleY - b.scaleY) > 0.02) return false;
                if (Math.abs(a.rotation - b.rotation) > 0.02) return false;
                if (!sameValues(a.color, b.color, 2)) return false;
                if (a.flipH !== b.flipH || a.flipV !== b.flipV || a.additive !== b.additive) return false;
            }
        }
        return true;
    },
};

/**
 * Runs an exercise's checks against a learner's script.
 * @returns {{ passed: boolean, results: {label: string, pass: boolean, message?: string}[] }}
 */
export function runChecks(checks, text, { solutionText } = {}) {
    const ctx = prepare(text);
    if (solutionText != null) ctx.solution = prepare(solutionText);

    // A script that doesn't parse fails every check, with one clear reason.
    if (ctx.errors.length > 0) {
        return {
            passed: false,
            results: [{ label: 'Your script has no errors', pass: false, message: `Line ${ctx.errors[0].line}: ${ctx.errors[0].message}` }],
        };
    }

    const results = checks.map((check) => {
        const run = kinds[check.kind];
        if (!run) throw new Error(`Unknown exercise check kind "${check.kind}".`);
        const pass = Boolean(run(check, ctx));
        return { label: check.label ?? check.kind, pass, message: pass ? undefined : check.message };
    });
    return { passed: results.every((r) => r.pass), results };
}
