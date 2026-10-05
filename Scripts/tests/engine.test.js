// Tests for the storyboard engine. Run with `npm test` (Node's built-in test runner).
// Cases follow the osu! wiki examples and osu!'s own parser behaviour.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { parse } from '../storyboard/parse.js';
import { compile, stateAt, frameAt, valueAt, storyboardSpan } from '../storyboard/timeline.js';
import { ease, easings, easingByName } from '../storyboard/easing.js';

const close = (actual, expected, eps = 1e-6) =>
    assert.ok(Math.abs(actual - expected) <= eps, `expected ${expected}, got ${actual}`);
const errors = (model) => model.diagnostics.filter((d) => d.severity === 'error');
const one = (text) => {
    const model = parse(text);
    assert.deepEqual(errors(model), [], 'unexpected parse errors');
    return compile(model)[0];
};

test('easings: 35 of them, starting at 0 and ending at 1', () => {
    assert.equal(easings.length, 35);
    for (let i = 0; i < easings.length; i++) {
        close(ease(i, 0), 0, 1e-3);
        close(ease(i, 1), 1, 1e-3);
    }
    close(ease(1, 0.5), 0.75); // "Out" is quad out
    close(ease(2, 0.5), 0.25); // "In" is quad in
    assert.equal(easingByName('OutQuad'), 4);
    assert.equal(easingByName('out-bounce'), 33);
    assert.equal(easingByName('nope'), -1);
    assert.ok(ease(30, 0.7) > 1, 'Back Out overshoots');
});

test('wiki fade example: fade in to half, hold, fade out', () => {
    const o = one('Sprite,Pass,Centre,"Sample.png",320,240\n_F,0,1000,2000,0,0.5\n_F,0,4000,5000,0.5,0');
    assert.equal(o.start, 1000);
    assert.equal(o.end, 5000);
    assert.equal(stateAt(o, 500), null, 'not active before its first command');
    close(stateAt(o, 1500).opacity, 0.25);
    close(stateAt(o, 3000).opacity, 0.5, 1e-9); // holds between commands
    close(stateAt(o, 4500).opacity, 0.25);
    assert.equal(stateAt(o, 5001), null, 'gone after its last command');
});

test('object declaration: layers and origins by name or number, quotes optional, backslashes normalised', () => {
    const model = parse('Sprite,3,1,SB\\star.png,10,20\n F,0,0,1000,1');
    assert.deepEqual(errors(model), []);
    const o = model.objects[0];
    assert.equal(o.layer, 'Foreground');
    assert.equal(o.origin, 'Centre');
    assert.equal(o.path, 'SB/star.png');
});

test('names are case-sensitive, with a helpful suggestion', () => {
    const model = parse('Sprite,foreground,Centre,"a.png",0,0\n F,0,0,1,1');
    assert.match(errors(model)[0].message, /did you mean Foreground/);
});

test('sprites without commands never appear (warning)', () => {
    const model = parse('Sprite,Foreground,Centre,"a.png",320,240');
    assert.ok(model.diagnostics.some((d) => d.severity === 'warning' && /never appears/.test(d.message)));
    assert.equal(compile(model)[0].start, Infinity);
});

test('commands need an object, and objects need indented commands', () => {
    assert.match(errors(parse(' F,0,0,1000,1'))[0].message, /no object/);
    assert.match(errors(parse('Sprite,Foreground,Centre,"a.png",0,0\nF,0,0,1000,1'))[0].message, /indented/);
});

test('move: MoveX leaves y at the declared position', () => {
    const o = one('Sprite,Pass,Centre,"a.png",320,240\n_MX,0,1000,2000,0,100');
    assert.equal(stateAt(o, 1500).y, 240);
    close(stateAt(o, 1500).x, 50);
});

test('move: Move feeds the same x/y as MoveX, and first values apply from the start', () => {
    const o = one('Sprite,Pass,Centre,"a.png",320,240\n_MX,0,1000,2000,0,100\n_M,0,3000,4000,500,50,600,60');
    close(stateAt(o, 1500).x, 50);
    assert.equal(stateAt(o, 2500).x, 100, 'x holds 100 after the MoveX');
    // y's first command is the Move at 3000, so y already has its start value (50) at 1000
    assert.equal(stateAt(o, 1000).y, 50);
    close(stateAt(o, 3500).x, 550);
    close(stateAt(o, 3500).y, 55);
});

test('shorthands: empty end time, single value, and value sequences', () => {
    const instant = one('Sprite,Pass,Centre,"a.png",0,0\n_F,0,1000,,0.5');
    assert.equal(instant.end, 1000);
    close(stateAt(instant, 1000).opacity, 0.5);

    const hold = one('Sprite,Pass,Centre,"a.png",0,0\n_V,0,1000,2000,0.5,2');
    close(stateAt(hold, 1500).scaleX, 0.5);
    close(stateAt(hold, 1500).scaleY, 2);

    const model = parse('Sprite,Pass,Centre,"a.png",0,0\n_F,0,51000,52000,0,1,0.5,1,0');
    assert.ok(model.diagnostics.some((d) => d.severity === 'info' && /4 steps/.test(d.message)));
    const seq = compile(model)[0];
    assert.equal(seq.end, 55000);
    close(stateAt(seq, 52500).opacity, 0.75); // 1 -> 0.5 halfway
    close(stateAt(seq, 54500).opacity, 0.5);  // 1 -> 0 halfway
});

test('scale and vector scale multiply; colour defaults to white', () => {
    const o = one('Sprite,Pass,Centre,"a.png",0,0\n_S,0,0,,2\n_V,0,0,,1.5,0.5');
    const s = stateAt(o, 0);
    close(s.scaleX, 3);
    close(s.scaleY, 1);
    assert.deepEqual(s.color, [255, 255, 255]);
});

test('rotation is in radians and colour interpolates per channel', () => {
    const o = one('Sprite,Pass,Centre,"a.png",0,0\n_R,0,0,1000,0,3.14159\n_C,0,0,1000,0,0,0,255,128,0');
    close(stateAt(o, 500).rotation, 1.570795);
    const c = stateAt(o, 500).color;
    close(c[0], 127.5);
    close(c[1], 64);
    close(c[2], 0);
});

test('overlapping commands of the same type: the later one wins', () => {
    const o = one('Sprite,Pass,Centre,"a.png",0,0\n_F,0,0,2000,0,1\n_F,0,500,1000,1,0.2');
    close(stateAt(o, 750).opacity, 0.6);
    close(stateAt(o, 1500).opacity, 0.2, 1e-9);
});

test('commands that end before they start become instant (warning)', () => {
    const model = parse('Sprite,Pass,Centre,"a.png",0,0\n_F,0,2000,1000,0,1');
    assert.ok(model.diagnostics.some((d) => /ends before it starts/.test(d.message)));
    const o = compile(model)[0];
    assert.equal(o.start, 2000);
    assert.equal(o.end, 2000);
});

test('wiki loop example: fade in and out 30 times starting at 60000', () => {
    const o = one('Sprite,Pass,Centre,"Sample.png",320,240\n_L,60000,30\n__F,0,0,500,0,1\n__F,0,500,1000,1,0');
    assert.equal(o.start, 60000);
    assert.equal(o.end, 90000);
    close(stateAt(o, 60250).opacity, 0.5);
    close(stateAt(o, 61250).opacity, 0.5); // second iteration
    close(stateAt(o, 89750).opacity, 0.5); // last iteration fading out
});

test('loop period is measured from the first command in the body', () => {
    const o = one('Sprite,Pass,Centre,"a.png",0,0\n_L,1000,3\n__F,0,200,700,0,1');
    // body runs 200..700, so the period is 500 ms: starts at 1200, 1700, 2200
    assert.equal(o.start, 1200);
    assert.equal(o.end, 2700);
    close(stateAt(o, 1950).opacity, 0.5);
});

test('doubly indented commands outside a loop still apply (warning)', () => {
    const model = parse('Sprite,Pass,Centre,"a.png",0,0\n__F,0,0,1000,1');
    assert.ok(model.diagnostics.some((d) => /indented twice/.test(d.message)));
    assert.equal(compile(model)[0].end, 1000);
});

test('parameters apply while running; instant ones for the whole life', () => {
    const ranged = one('Sprite,Pass,Centre,"a.png",0,0\n_F,0,0,3000,1\n_P,0,1000,2000,H');
    assert.equal(stateAt(ranged, 500).flipH, false);
    assert.equal(stateAt(ranged, 1500).flipH, true);
    assert.equal(stateAt(ranged, 2500).flipH, false);

    const instant = one('Sprite,Pass,Centre,"a.png",0,0\n_F,0,0,3000,1\n_P,0,1000,,A');
    assert.equal(stateAt(instant, 0).additive, true);
    assert.equal(stateAt(instant, 3000).additive, true);
});

test('opacity above 1 wraps like osu!stable', () => {
    const o = one('Sprite,Pass,Centre,"a.png",0,0\n_F,0,0,,1.25');
    close(stateAt(o, 0).opacity, 0.25);
});

test('variables are substituted in events, unknown ones are reported', () => {
    const model = parse('[Variables]\n$green=0,255,0\n$file="Sample.png"\n[Events]\nSprite,Pass,Centre,$file,320,240\n_C,0,0,1000,$green');
    assert.deepEqual(errors(model), []);
    assert.equal(model.objects[0].path, 'Sample.png');
    assert.deepEqual(model.objects[0].commands[0].from, [0, 255, 0]);
    assert.match(errors(parse('Sprite,Pass,Centre,$nope,0,0'))[0].message, /Unknown variable \$nope/);
});

test('animations: frames advance from the first command; LoopOnce stops on the last frame', () => {
    const forever = one('Animation,Fail,BottomCentre,"Other/explosion.png",418,108,12,31,LoopForever\n_F,0,1000,5000,1');
    assert.equal(frameAt(forever, 1000), 0);
    assert.equal(frameAt(forever, 1031), 1);
    assert.equal(frameAt(forever, 1000 + 31 * 12), 0, 'loops back to the first frame');
    const once = one('Animation,Fail,Centre,"a.png",0,0,4,100,LoopOnce\n_F,0,0,5000,1');
    assert.equal(frameAt(once, 2000), 3);
});

test('samples, triggers and ignored beatmap events', () => {
    const model = parse('0,0,"bg.jpg",0,0\nVideo,0,"v.mp4"\n M,0,0,1,0,0\nSample,163520,2,"Audio\\Best End.mp3",80\nSprite,Foreground,Centre,"w.png",320,240\n_T,Passing,20000,40000\n__F,0,0,500,1\n__F,0,500,501,0');
    assert.deepEqual(errors(model), []);
    assert.equal(model.samples[0].layer, 'Pass');
    assert.equal(model.samples[0].volume, 80);
    assert.equal(model.objects.length, 1);
    assert.equal(model.objects[0].triggers[0].trigger, 'Passing');
    assert.equal(model.objects[0].triggers[0].commands.length, 2);
});

test('a pasted .osu file: only [Events] and the widescreen flag are read', () => {
    const model = parse('osu file format v14\n\n[General]\nWidescreenStoryboard: 1\n\n[HitObjects]\n256,192,1000,1,0\n\n[Events]\n//Storyboard Layer 0 (Background)\nSprite,Background,TopLeft,"bg.jpg",0,0\n F,0,0,1000,1');
    assert.deepEqual(errors(model), []);
    assert.equal(model.widescreen, true);
    assert.equal(model.objects.length, 1);
});

test('helpful errors for common mistakes', () => {
    assert.match(errors(parse('Sprite,Foreground,Centre,"a.png",0,0\n M,0,0,1000,1,2,3'))[0].message, /pairs of numbers/);
    assert.match(errors(parse('Sprite,Foreground,Centre,"a.png",0,0\n X,0,0,1000,1'))[0].message, /Unknown command "X"/);
    assert.match(errors(parse('Sprite,Foreground,Centre,"a.png",0,0\n F,0,abc,1000,1'))[0].message, /number for the start time/);
    assert.match(errors(parse('Sprite,Foreground,Middle,"a.png",0,0'))[0].message, /Unknown origin "Middle"/);
    assert.match(parse('Sprite,Foreground,Centre,"a.png",0,0\n F,99,0,1000,1').diagnostics[0].message, /Easing 99/);
});

test('storyboard span and valueAt fallbacks', () => {
    const objects = compile(parse('Sprite,Pass,Centre,"a.png",0,0\n_F,0,-500,1000,1\nSprite,Pass,Centre,"b.png",0,0\n_F,0,200,3000,1'));
    assert.deepEqual(storyboardSpan(objects), { start: -500, end: 3000 });
    assert.equal(valueAt([], 0, 42), 42);
});

test('easing names: storybrew\'s "None" means linear', () => {
    assert.equal(easingByName('None'), 0);
    assert.equal(easingByName('OutBack'), 30);
    assert.equal(easingByName('out back'), 30);
    assert.equal(easingByName('Wobbly'), -1);
});

test('trigger-only sprites get a note, not a "never appears" warning', () => {
    const model = parse('Sprite,Foreground,Centre,"sb/glow.png",320,240\n T,HitSoundClap,0,10000\n  F,0,0,300,1,0');
    assert.deepEqual(model.diagnostics.map((d) => d.severity), ['info']);
    assert.match(model.diagnostics[0].message, /only has triggers/);
});
