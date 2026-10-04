// Tests for exercise checks and script mode.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { runChecks } from '../storyboard/checks.js';
import { runScript, seededRandom, errorLine } from '../storyboard/script.js';
import { parse } from '../storyboard/parse.js';
import { compile, stateAt } from '../storyboard/timeline.js';

const fadeIn = 'Sprite,Foreground,Centre,"sb/dot.png",320,240\n F,0,1000,2000,0,1';

test('value, visible and lifetime checks', () => {
    const checks = [
        { kind: 'value', time: 1000, property: 'opacity', equals: 0, label: 'starts invisible' },
        { kind: 'value', time: 1500, property: 'opacity', equals: 0.5, label: 'half way' },
        { kind: 'visible', time: 2000, expect: true, label: 'visible at the end' },
        { kind: 'lifetime', start: 1000, end: 2000, label: 'lives 1000-2000' },
    ];
    const ok = runChecks(checks, fadeIn);
    assert.equal(ok.passed, true, JSON.stringify(ok.results));
    const late = runChecks(checks, fadeIn.replace('1000,2000', '1200,2000'));
    assert.equal(late.passed, false);
    assert.deepEqual(late.results.map((r) => r.pass), [true, false, true, false]);
});

test('a script with errors fails with the first error', () => {
    const result = runChecks([{ kind: 'count', of: 'sprites', min: 1 }], 'Sprite,Front,Centre,"a.png",0,0');
    assert.equal(result.passed, false);
    assert.match(result.results[0].message, /^Line 1: Unknown layer/);
});

test('count checks: sprites, commands, loops, distinct easings', () => {
    const text = 'Sprite,Foreground,Centre,"a.png",0,0\n F,4,0,100,0,1\n L,0,4\n  S,0,0,50,1,2\nSprite,Background,TopLeft,"b.png",0,0\n F,33,0,100,1';
    const run = (check) => runChecks([check], text).passed;
    assert.equal(run({ kind: 'count', of: 'sprites', equals: 2 }), true);
    assert.equal(run({ kind: 'count', of: 'sprites', equals: 1, object: { layer: 'Background' } }), true);
    assert.equal(run({ kind: 'count', of: 'commands', command: 'F', equals: 2 }), true);
    assert.equal(run({ kind: 'count', of: 'loops', min: 1 }), true);
    assert.equal(run({ kind: 'count', of: 'easings', min: 3 }), true);
    assert.equal(run({ kind: 'count', of: 'commands', max: 2 }), false);
});

test('object and command checks', () => {
    const text = 'Sprite,Foreground,TopLeft,"sb/osb.png",0,0\n F,0,0,1000,1\n M,30,0,1000,0,0,100,100';
    const run = (check) => runChecks([check], text).passed;
    assert.equal(run({ kind: 'object', layer: 'Foreground', origin: 'TopLeft', path: 'SB/OSB.png', x: 0, y: 0 }), true);
    assert.equal(run({ kind: 'object', origin: 'Centre' }), false);
    assert.equal(run({ kind: 'command', type: 'M', easing: [29, 30, 31], to: [100, 100] }), true);
    assert.equal(run({ kind: 'command', type: 'R' }), false);
});

test('matches-solution compares what learners see, not how they wrote it', () => {
    const solution = 'Sprite,Foreground,Centre,"a.png",320,240\n F,0,0,1000,0,1\n F,0,1000,2000,1,0';
    const shorthand = 'Sprite,Foreground,Centre,"a.png",320,240\n F,0,0,1000,0,1,0';
    const different = 'Sprite,Foreground,Centre,"a.png",320,240\n F,0,0,1000,0,1\n F,0,1000,2500,1,0';
    const check = [{ kind: 'matches-solution', step: 25 }];
    assert.equal(runChecks(check, shorthand, { solutionText: solution }).passed, true);
    assert.equal(runChecks(check, different, { solutionText: solution }).passed, false);
});

test('script mode writes idiomatic .osb that parses cleanly', () => {
    const { osb, objects, commands } = runScript(`
        const layer = sb.layer('Foreground');
        for (let i = 0; i < 3; i++) {
            const dot = layer.sprite('sb/dot.png', 'Centre', 100 + i * 100, 240);
            dot.fade(i * 100, i * 100 + 500, 0, 1);
            dot.move('OutQuad', 0, 1000, [100, 240], [540, 240]);
            dot.scale(1000, 2);
        }
        sb.layer('Background').sprite('bg.jpg', 'TopLeft', 0, 0).fade(0, 3000, 1, 1);
    `);
    assert.equal(objects, 4);
    assert.equal(commands, 10);
    assert.match(osb, /^\[Events\]\n\/\/Storyboard Layer 0 \(Background\)\nSprite,Background,TopLeft,"bg.jpg",0,0\n F,0,0,3000,1\n/);
    assert.match(osb, /Sprite,Foreground,Centre,"sb\/dot.png",200,240\n F,0,100,600,0,1\n M,4,0,1000,100,240,540,240\n S,0,1000,,2/);
    const model = parse(osb);
    assert.deepEqual(model.diagnostics.filter((d) => d.severity !== 'info'), []);
});

test('script mode loops produce L groups with relative times', () => {
    const { osb } = runScript(`
        const s = sb.layer('Foreground').sprite('sb/dot.png');
        s.startLoopGroup(2000, 4);
        s.fade(0, 250, 1, 0.2);
        s.fade(250, 500, 0.2, 1);
        s.endGroup();
    `);
    assert.match(osb, / L,2000,4\n  F,0,0,250,1,0.2\n  F,0,250,500,0.2,1/);
    const object = compile(parse(osb))[0];
    assert.equal(object.end, 4000);
    assert.ok(Math.abs(stateAt(object, 3625).opacity - 0.6) < 1e-9);
});

test('script mode explains misuse', () => {
    assert.throws(() => runScript(`sb.layer('Front')`), /Unknown layer "Front"/);
    assert.throws(() => runScript(`sb.layer().sprite('a.png').fade(1, 2, 3)`), /F takes/);
    assert.throws(() => runScript(`sb.layer().sprite('a.png').fade('Bouncy', 0, 1, 0, 1)`), /Unknown easing/);
    assert.throws(() => runScript(`sb.layer().sprite('a.png').endGroup()`), /without startLoopGroup/);
    assert.throws(() => runScript(`const s = sb.layer().sprite('a.png'); s.startLoopGroup(0, 2); s.startLoopGroup(0, 2);`), /can't be nested/);
    assert.throws(() => runScript(`for (;;) sb.layer().sprite('a.png')`), /More than 10000 sprites/);
});

test('seeded randomness repeats, and errors point at the learner\'s line', () => {
    const a = seededRandom(7);
    const b = seededRandom(7);
    assert.deepEqual([a(), a(), a()], [b(), b(), b()]);
    const first = runScript(`log(Math.round(random(0, 1000)), randomInt(1, 6))`, { seed: 3 }).logs[0];
    const second = runScript(`log(Math.round(random(0, 1000)), randomInt(1, 6))`, { seed: 3 }).logs[0];
    assert.equal(first, second);

    try {
        runScript('const a = 1;\nnull.boom();');
        assert.fail('should throw');
    } catch (e) {
        assert.equal(errorLine(e), 2);
    }
});

test('animation matchers check frame count, delay and loop type', () => {
    const text = 'Animation,Foreground,Centre,"sb/spark.png",320,240,6,80,LoopOnce\n F,0,0,1000,1';
    const run = (check) => runChecks([check], text).passed;
    assert.equal(run({ kind: 'object', is: 'animation', path: 'sb/spark.png', frameCount: 6, frameDelay: 80, loopType: 'LoopOnce' }), true);
    assert.equal(run({ kind: 'object', frameCount: 5 }), false);
    assert.equal(run({ kind: 'object', loopType: 'LoopForever' }), false);
    assert.equal(run({ kind: 'count', of: 'animations', equals: 1, object: { frameDelay: 80 } }), true);
});

test('value and visible checks can use each object\'s own start and end', () => {
    const text = [
        'Sprite,Foreground,Centre,"sb/particle.png",0,0', ' M,0,100,1100,50,-20,60,500',
        'Sprite,Foreground,Centre,"sb/particle.png",0,0', ' M,0,700,1700,300,-20,280,500',
    ].join('\n');
    const run = (check) => runChecks([check], text).passed;
    assert.equal(run({ kind: 'value', time: 'start', property: 'y', max: -10, every: true }), true);
    assert.equal(run({ kind: 'value', time: 'end', property: 'y', min: 490, every: true }), true);
    assert.equal(run({ kind: 'value', time: 'end', property: 'x', min: 100, every: true }), false);
    assert.equal(run({ kind: 'visible', time: 'start', every: true }), true);
});
