// Script mode: write JavaScript that generates a storyboard, the same idea as storybrew (C#) or osbpy
// (Python). The API mirrors storybrew's names so the skills transfer:
//
//   const layer = sb.layer('Foreground');
//   const dot = layer.sprite('sb/dot.png', 'Centre', 320, 240);
//   dot.fade(0, 1000, 0, 1);                       // (start, end, from, to)
//   dot.move('OutQuad', 0, 1000, [100, 240], [540, 240]);
//   dot.scale(1000, 2);                            // (time, value): an instant change
//   dot.startLoopGroup(2000, 4);                   // commands until endGroup() repeat 4 times
//   dot.fade(0, 250, 1, 0.2); dot.fade(250, 500, 0.2, 1);
//   dot.endGroup();
//
// runScript() turns the code into .osb text. In the browser it runs inside a Web Worker
// (see script-worker.js) so a runaway loop can be stopped.

import { easingByName } from './easing.js';
import { LAYERS, ORIGINS, VALUE_SIZES } from './parse.js';

const MAX_OBJECTS = 10000;
const MAX_COMMANDS = 300000;

/** A small seeded random number generator (mulberry32), so a script draws the same storyboard every run. */
export function seededRandom(seed) {
    let a = seed >>> 0;
    return () => {
        a = (a + 0x6d2b79f5) >>> 0;
        let t = a;
        t = Math.imul(t ^ (t >>> 15), t | 1);
        t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
        return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
}

const fmt = (n) => {
    if (!Number.isFinite(n)) throw new ScriptError(`Got ${n} where a number was expected.`);
    return String(Math.round(n * 10000) / 10000);
};
const fmtTime = (n) => {
    if (!Number.isFinite(n)) throw new ScriptError(`Got ${n} where a time was expected.`);
    return String(Math.round(n));
};

export class ScriptError extends Error {}

class Counter {
    objects = 0;
    commands = 0;
}

class Sprite {
    #lines = [];
    #inGroup = false;
    #counter;
    #header;

    constructor(header, counter) {
        this.#header = header;
        this.#counter = counter;
    }

    /** Builds one command line from storybrew-style arguments: ([easing,] start, end, from, to) or (time, value). */
    #command(type, args) {
        let easing = 0;
        if (typeof args[0] === 'string') {
            easing = easingByName(args[0]);
            if (easing < 0) throw new ScriptError(`Unknown easing "${args[0]}". Try names like "OutQuad" or "InOutSine".`);
            args = args.slice(1);
        }
        const flat = args.flat();
        const size = VALUE_SIZES[type];
        let start, end, from, to;
        if (flat.length === size + 1) {
            start = end = flat[0];
            from = to = flat.slice(1);
        } else if (flat.length === 2 + 2 * size) {
            [start, end] = flat;
            from = flat.slice(2, 2 + size);
            to = flat.slice(2 + size);
        } else {
            const value = size === 1 ? 'value' : size === 2 ? '[x, y]' : '[r, g, b]';
            throw new ScriptError(`${type} takes (start, end, from, to) or (time, ${value}), with an optional easing name first.`);
        }
        if (end < start) throw new ScriptError(`A ${type} command ends (${end}) before it starts (${start}).`);
        const same = from.every((v, i) => v === to[i]);
        const values = (same ? from : [...from, ...to]).map(fmt).join(',');
        this.#push(` ${type},${easing},${fmtTime(start)},${start === end ? '' : fmtTime(end)},${values}`);
        return this;
    }

    #param(letter, start, end = start) {
        this.#push(` P,0,${fmtTime(start)},${start === end ? '' : fmtTime(end)},${letter}`);
        return this;
    }

    #push(line) {
        if (++this.#counter.commands > MAX_COMMANDS) throw new ScriptError(`More than ${MAX_COMMANDS} commands. Is a loop running away?`);
        this.#lines.push(this.#inGroup ? ' ' + line : line);
    }

    fade(...args) { return this.#command('F', args); }
    move(...args) { return this.#command('M', args); }
    moveX(...args) { return this.#command('MX', args); }
    moveY(...args) { return this.#command('MY', args); }
    scale(...args) { return this.#command('S', args); }
    scaleVec(...args) { return this.#command('V', args); }
    rotate(...args) { return this.#command('R', args); }
    color(...args) { return this.#command('C', args); }
    flipH(start, end) { return this.#param('H', start, end); }
    flipV(start, end) { return this.#param('V', start, end); }
    additive(start, end) { return this.#param('A', start, end); }

    /** Commands until endGroup() repeat `loopCount` times; their times count from the start of each loop. */
    startLoopGroup(startTime, loopCount) {
        if (this.#inGroup) throw new ScriptError('Loops can\'t be nested. Call endGroup() first.');
        this.#push(` L,${fmtTime(startTime)},${Math.max(1, Math.floor(loopCount))}`);
        this.#inGroup = true;
        return this;
    }

    /** Commands until endGroup() play when the trigger fires (e.g. "HitSoundClap", "Passing"). */
    startTriggerGroup(triggerName, startTime, endTime, group = 0) {
        if (this.#inGroup) throw new ScriptError('Triggers can\'t be nested. Call endGroup() first.');
        this.#push(` T,${triggerName},${fmtTime(startTime)},${fmtTime(endTime)}${group ? ',' + group : ''}`);
        this.#inGroup = true;
        return this;
    }

    endGroup() {
        if (!this.#inGroup) throw new ScriptError('endGroup() was called without startLoopGroup() or startTriggerGroup().');
        this.#inGroup = false;
        return this;
    }

    toString() {
        return [this.#header, ...this.#lines].join('\n');
    }
}

class Layer {
    #name;
    #counter;
    objects = [];

    constructor(name, counter) {
        this.#name = name;
        this.#counter = counter;
    }

    #add(header) {
        if (++this.#counter.objects > MAX_OBJECTS) throw new ScriptError(`More than ${MAX_OBJECTS} sprites. Is a loop running away?`);
        const sprite = new Sprite(header, this.#counter);
        this.objects.push(sprite);
        return sprite;
    }

    sprite(path, origin = 'Centre', x = 320, y = 240) {
        checkOrigin(origin);
        if (Array.isArray(x)) [x, y] = x;
        return this.#add(`Sprite,${this.#name},${origin},"${path}",${fmt(x)},${fmt(y)}`);
    }

    animation(path, frameCount, frameDelay, loopType = 'LoopForever', origin = 'Centre', x = 320, y = 240) {
        checkOrigin(origin);
        if (loopType !== 'LoopForever' && loopType !== 'LoopOnce') throw new ScriptError('loopType is "LoopForever" or "LoopOnce".');
        if (Array.isArray(x)) [x, y] = x;
        return this.#add(`Animation,${this.#name},${origin},"${path}",${fmt(x)},${fmt(y)},${Math.floor(frameCount)},${fmt(frameDelay)},${loopType}`);
    }
}

function checkOrigin(origin) {
    if (!ORIGINS.includes(origin)) throw new ScriptError(`Unknown origin "${origin}". Use one of: ${ORIGINS.join(', ')}.`);
}

/**
 * Runs storyboard-generating code and returns the .osb text it produced.
 * @param {string} code  The learner's script.
 * @param {{seed?: number}} options
 */
export function runScript(code, { seed = 1 } = {}) {
    const counter = new Counter();
    const layers = new Map(LAYERS.map((name) => [name, new Layer(name, counter)]));
    const logs = [];
    const random = seededRandom(seed);

    const sb = {
        layer(name = 'Foreground') {
            const layer = layers.get(name);
            if (!layer) throw new ScriptError(`Unknown layer "${name}". Use one of: ${LAYERS.join(', ')}.`);
            return layer;
        },
    };
    const api = {
        sb,
        /** random() is 0-1; random(max) or random(min, max) for a range. Seeded, so results repeat. */
        random: (a, b) => (a == null ? random() : b == null ? random() * a : a + random() * (b - a)),
        randomInt: (a, b) => (b == null ? Math.floor(random() * a) : a + Math.floor(random() * (b - a + 1))),
        lerp: (a, b, t) => a + (b - a) * t,
        /** Milliseconds per beat at a BPM. */
        beatLength: (bpm) => 60000 / bpm,
        log: (...values) => logs.push(values.map((v) => (typeof v === 'string' ? v : JSON.stringify(v))).join(' ')),
    };

    const run = new Function(...Object.keys(api), `"use strict";\n${code}`);
    run(...Object.values(api));

    const sections = ['[Events]'];
    LAYERS.forEach((name, i) => {
        sections.push(`//Storyboard Layer ${i} (${name})`);
        for (const sprite of layers.get(name).objects) sections.push(sprite.toString());
    });
    return { osb: sections.join('\n') + '\n', logs, objects: counter.objects, commands: counter.commands };
}

/** The line in the learner's script where an error happened, if the browser tells us. */
export function errorLine(error) {
    // new Function puts three lines ("function anonymous(...", ") {" and "use strict") before the code.
    const match = /(?:<anonymous>|anonymous|Function):(\d+):\d+/.exec(error?.stack ?? '');
    return match ? Math.max(1, Number(match[1]) - 3) : null;
}
