// Parses storyboard scripts (.osb files, or the [Events] section of a .osu file) into a plain model.
// Mistakes are collected as diagnostics with line numbers instead of throwing, so the playground can
// underline them. Behaviour follows osu!'s own parser (LegacyStoryboardDecoder in osu!lazer), plus the
// "sequence" shorthand from the osu! wiki that osu!stable supports.

export const LAYERS = ['Background', 'Fail', 'Pass', 'Foreground', 'Overlay'];

export const ORIGINS = ['TopLeft', 'Centre', 'CentreLeft', 'TopRight', 'BottomCentre', 'TopCentre', 'Custom', 'CentreRight', 'BottomLeft', 'BottomRight'];

/** Where each origin sits on the image, as fractions of its width and height. */
export const ORIGIN_POINTS = {
    TopLeft: [0, 0], TopCentre: [0.5, 0], TopRight: [1, 0],
    CentreLeft: [0, 0.5], Centre: [0.5, 0.5], CentreRight: [1, 0.5],
    BottomLeft: [0, 1], BottomCentre: [0.5, 1], BottomRight: [1, 1],
    Custom: [0, 0],
};

/** How many numbers make up one value of each command (Move needs x and y, Colour needs r, g, b). */
export const VALUE_SIZES = { F: 1, S: 1, R: 1, MX: 1, MY: 1, M: 2, V: 2, C: 3 };

export const COMMAND_NAMES = {
    F: 'Fade', M: 'Move', MX: 'Move X', MY: 'Move Y', S: 'Scale', V: 'Vector scale',
    R: 'Rotate', C: 'Colour', P: 'Parameter', L: 'Loop', T: 'Trigger',
};

const EVENT_TYPES = { 0: 'Background', 1: 'Video', 2: 'Break', 3: 'Colour', 4: 'Sprite', 5: 'Sample', 6: 'Animation' };
const IGNORED_EVENTS = new Set(['Background', 'Video', 'Break', 'Colour']);

/**
 * @param {string} text
 * @returns {{objects: object[], samples: object[], variables: Map<string,string>, widescreen: boolean|null,
 *            diagnostics: {line:number, severity:'error'|'warning'|'info', message:string}[]}}
 */
export function parse(text) {
    const result = { objects: [], samples: [], variables: new Map(), widescreen: null, diagnostics: [] };
    const report = (line, severity, message) => result.diagnostics.push({ line, severity, message });

    let section = 'Events';
    let current = null; // the object that commands attach to
    let group = null;   // the loop or trigger that doubly indented commands go into
    let ignoredObject = false;

    const lines = String(text).split(/\r?\n/);
    for (let i = 0; i < lines.length; i++) {
        const lineNumber = i + 1;
        let line = lines[i];
        if (line.trim() === '' || line.trimStart().startsWith('//') || line.startsWith('osu file format v')) continue;

        const header = /^\s*\[(\w+)\]\s*$/.exec(line);
        if (header) {
            section = header[1];
            continue;
        }

        if (section === 'Variables') {
            const eq = line.indexOf('=');
            const name = line.slice(0, eq).trim();
            if (eq < 1 || !name.startsWith('$')) {
                report(lineNumber, 'error', 'Variables look like $name=value.');
                continue;
            }
            result.variables.set(name, line.slice(eq + 1).trim());
            continue;
        }

        if (section === 'General') {
            const m = /^\s*WidescreenStoryboard\s*:\s*(\d)/.exec(line);
            if (m) result.widescreen = m[1] === '1';
            continue;
        }

        if (section !== 'Events') continue; // [Metadata], [HitObjects] and friends of a pasted .osu file

        // Variables are plain text replacements, applied once each in the order they were declared.
        if (line.includes('$')) {
            for (const [name, value] of result.variables) line = line.split(name).join(value);
            const unknown = /\$[A-Za-z0-9_]+/.exec(line);
            if (unknown) report(lineNumber, 'error', `Unknown variable ${unknown[0]}. Declare it in a [Variables] section above.`);
        }

        let depth = 0;
        while (depth < line.length && (line[depth] === ' ' || line[depth] === '_')) depth++;
        const parts = line.slice(depth).split(',').map((p) => p.trim());

        if (depth === 0) {
            current = null;
            group = null;
            ignoredObject = false;
            const type = EVENT_TYPES[parts[0]] ?? parts[0];
            if (IGNORED_EVENTS.has(type)) {
                ignoredObject = true;
                continue;
            }
            if (type === 'Sprite' || type === 'Animation') current = parseObject(type, parts, lineNumber, report, result);
            else if (type === 'Sample') parseSample(parts, lineNumber, report, result);
            else if (VALUE_SIZES[parts[0]] !== undefined || ['P', 'L', 'T'].includes(parts[0]))
                report(lineNumber, 'error', `"${parts[0]}" is a command, so it needs to be indented with a space or underscore under its Sprite.`);
            else report(lineNumber, 'error', `Unknown event "${parts[0]}". Objects start with Sprite, Animation or Sample.`);
            continue;
        }

        if (ignoredObject) continue; // commands on a Video line
        if (!current) {
            report(lineNumber, 'error', 'This command has no object to apply to. Put it under a Sprite or Animation line.');
            continue;
        }

        if (depth === 1) group = null;
        const command = parts[0];

        if (command === 'L' || command === 'T') {
            if (depth > 1) report(lineNumber, 'warning', 'Loops and triggers can\'t be nested. This one is treated as a separate group on the sprite.');
            group = command === 'L' ? parseLoop(parts, lineNumber, report) : parseTrigger(parts, lineNumber, report);
            if (group) (command === 'L' ? current.loops : current.triggers).push(group);
            continue;
        }

        const parsed = parseCommand(parts, lineNumber, report);
        if (!parsed) continue;
        if (depth >= 2 && !group) {
            report(lineNumber, 'warning', 'This command is indented twice but isn\'t inside a loop or trigger, so it applies to the sprite directly.');
            current.commands.push(...parsed);
        } else {
            (depth >= 2 ? group.commands : current.commands).push(...parsed);
        }
    }

    for (const object of result.objects) {
        if (object.commands.length > 0 || object.loops.length > 0) continue;
        if (object.triggers.length > 0)
            report(object.line, 'info', `This ${object.kind} only has triggers. They need gameplay to fire, so the preview can't show it.`);
        else
            report(object.line, 'warning', `This ${object.kind} has no commands, so it never appears. Objects are only visible while their commands run.`);
    }
    return result;
}

function parseObject(type, parts, line, report, result) {
    const needed = type === 'Sprite' ? 6 : 8;
    if (parts.length < needed) {
        report(line, 'error', type === 'Sprite'
            ? 'A Sprite needs Sprite,layer,origin,"file",x,y'
            : 'An Animation needs Animation,layer,origin,"file",x,y,frameCount,frameDelay[,loopType]');
        return null;
    }
    const layer = parseEnum(parts[1], LAYERS, 'layer', line, report);
    const origin = parseEnum(parts[2], ORIGINS, 'origin', line, report);
    const path = cleanPath(parts[3]);
    const x = parseNumber(parts[4], 'x', line, report);
    const y = parseNumber(parts[5], 'y', line, report);
    if (layer == null || origin == null || x == null || y == null) return null;
    if (/\s/.test(path) && !parts[3].startsWith('"'))
        report(line, 'warning', 'File names with spaces must be wrapped in "double quotes".');
    if (origin === 'Custom') report(line, 'info', 'Custom behaves like TopLeft; the wiki recommends not using it.');

    const object = { kind: type === 'Sprite' ? 'sprite' : 'animation', layer, origin, path, x, y, line, commands: [], loops: [], triggers: [] };
    if (type === 'Animation') {
        object.frameCount = parseNumber(parts[6], 'frame count', line, report);
        object.frameDelay = parseNumber(parts[7], 'frame delay', line, report);
        object.loopType = parts[8] && parts[8] !== '' ? parts[8] : 'LoopForever';
        if (object.frameCount == null || object.frameDelay == null) return null;
        if (!Number.isInteger(object.frameCount) || object.frameCount < 1) {
            report(line, 'error', 'The frame count must be a whole number of at least 1.');
            return null;
        }
        if (object.loopType !== 'LoopForever' && object.loopType !== 'LoopOnce') {
            report(line, 'warning', `Unknown loop type "${object.loopType}"; using LoopForever. Use LoopForever or LoopOnce.`);
            object.loopType = 'LoopForever';
        }
    }
    result.objects.push(object);
    return object;
}

function parseSample(parts, line, report, result) {
    if (parts.length < 4) {
        report(line, 'error', 'A Sample needs Sample,time,layer,"file"[,volume]');
        return;
    }
    const time = parseNumber(parts[1], 'time', line, report);
    const layer = parseEnum(parts[2], LAYERS, 'layer', line, report);
    const volume = parts.length > 4 && parts[4] !== '' ? parseNumber(parts[4], 'volume', line, report) : 100;
    if (time == null || layer == null || volume == null) return;
    result.samples.push({ time, layer, path: cleanPath(parts[3]), volume, line });
}

function parseLoop(parts, line, report) {
    if (parts.length < 3) {
        report(line, 'error', 'A loop needs L,startTime,loopCount');
        return null;
    }
    const start = parseNumber(parts[1], 'loop start time', line, report);
    const count = parseNumber(parts[2], 'loop count', line, report);
    if (start == null || count == null) return null;
    if (count < 1) report(line, 'warning', 'A loop runs at least once, even with a loop count below 1.');
    return { kind: 'loop', start, count: Math.max(1, Math.floor(count)), commands: [], line };
}

function parseTrigger(parts, line, report) {
    if (parts.length < 2 || parts[1] === '') {
        report(line, 'error', 'A trigger needs T,triggerName,startTime,endTime');
        return null;
    }
    const start = parts.length > 2 && parts[2] !== '' ? parseNumber(parts[2], 'trigger start time', line, report) : -Infinity;
    const end = parts.length > 3 && parts[3] !== '' ? parseNumber(parts[3], 'trigger end time', line, report) : Infinity;
    const groupNumber = parts.length > 4 && parts[4] !== '' ? parseNumber(parts[4], 'group number', line, report) : 0;
    if (start == null || end == null) return null;
    if (!/^(HitSound\w*|Passing|Failing)$/.test(parts[1]))
        report(line, 'warning', `Unknown trigger "${parts[1]}". Triggers are HitSound..., Passing or Failing.`);
    return { kind: 'trigger', trigger: parts[1], start, end, groupNumber, commands: [], line };
}

/** One command line, expanded into one or more segments (the sequence shorthand makes several). */
function parseCommand(parts, line, report) {
    const type = parts[0];
    if (type !== 'P' && VALUE_SIZES[type] === undefined) {
        report(line, 'error', `Unknown command "${type}". Commands are F, M, MX, MY, S, V, R, C, P, L and T.`);
        return null;
    }
    if (parts.length < 5) {
        report(line, 'error', `${COMMAND_NAMES[type]} needs ${type},easing,startTime,endTime,${type === 'P' ? 'H|V|A' : 'values'}`);
        return null;
    }

    let easing = parseNumber(parts[1], 'easing', line, report);
    const start = parseNumber(parts[2], 'start time', line, report);
    let end = parts[3] === '' ? start : parseNumber(parts[3], 'end time', line, report);
    if (easing == null || start == null || end == null) return null;
    if (!Number.isInteger(easing) || easing < 0 || easing > 34) {
        report(line, 'warning', `Easing ${parts[1]} doesn't exist (use 0-34), so it plays as linear.`);
        easing = 0;
    }
    if (end < start) {
        report(line, 'warning', 'This command ends before it starts, so osu! treats it as instant. The ranking criteria call these illogical commands.');
        end = start;
    }

    if (type === 'P') {
        const param = parts[4];
        if (param !== 'H' && param !== 'V' && param !== 'A') {
            report(line, 'error', 'Parameter takes H (flip horizontally), V (flip vertically) or A (additive blending).');
            return null;
        }
        return [{ type, easing, start, end, param, line }];
    }

    const size = VALUE_SIZES[type];
    const raw = parts.slice(4);
    const values = [];
    for (const value of raw) {
        const n = parseNumber(value, 'value', line, report);
        if (n == null) return null;
        values.push(n);
    }
    if (values.length % size !== 0) {
        const unit = { 1: 'one number', 2: 'pairs of numbers (x,y)', 3: 'groups of three numbers (r,g,b)' }[size];
        report(line, 'error', `${COMMAND_NAMES[type]} values come in ${unit}; this line has ${values.length}.`);
        return null;
    }
    if (type === 'F' && values.some((v) => v < 0)) report(line, 'warning', 'Opacity goes from 0 (invisible) to 1 (fully visible).');
    if (type === 'C' && values.some((v) => v < 0 || v > 255)) report(line, 'warning', 'Colour values go from 0 to 255.');

    const groups = [];
    for (let k = 0; k < values.length; k += size) groups.push(values.slice(k, k + size));
    if (groups.length === 1) groups.push(groups[0]); // "start and end values are the same" shorthand

    const duration = end - start;
    const segments = [];
    for (let k = 0; k + 1 < groups.length; k++) {
        segments.push({ type, easing, start: start + k * duration, end: end + k * duration, from: groups[k], to: groups[k + 1], line });
    }
    if (segments.length > 1)
        report(line, 'info', `Sequence shorthand: ${segments.length} steps of ${duration} ms. osu!(lazer) currently reads only the first two values.`);
    return segments;
}

function parseEnum(raw, names, what, line, report) {
    if (/^\d+$/.test(raw) && Number(raw) < names.length) return names[Number(raw)];
    if (names.includes(raw)) return raw;
    const close = names.find((n) => n.toLowerCase() === raw.toLowerCase());
    report(line, 'error', close
        ? `Unknown ${what} "${raw}". Names are case-sensitive: did you mean ${close}?`
        : `Unknown ${what} "${raw}". Use one of: ${names.join(', ')}.`);
    return null;
}

function parseNumber(raw, what, line, report) {
    const n = raw === '' ? NaN : Number(raw);
    if (Number.isFinite(n)) return n;
    report(line, 'error', `Expected a number for the ${what}, but got "${raw}".`);
    return null;
}

function cleanPath(raw) {
    return raw.replace(/^"|"$/g, '').replace(/\\/g, '/');
}
