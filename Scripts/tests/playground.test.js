// Tests for the playground's small helpers: share links, the time readout and animation frame paths.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { toBase64Url, fromBase64Url } from '../playground/share-link.js';
import { formatTime } from '../playground/player.js';
import { framePath } from '../storyboard/assets.js';

test('share links keep any code intact, Japanese included', () => {
    for (const code of [
        'Sprite,Foreground,Centre,"sb/dot.png",320,240\n F,0,0,1000,0,1',
        '// 夜に駆ける\nsb.layer("Foreground").sprite("sb/dot.png")',
        'emoji 🌸 and quotes "\' and <tags>',
        '',
    ]) {
        assert.equal(fromBase64Url(toBase64Url(code)), code);
    }
});

test('share links only use URL-safe characters, without padding', () => {
    const encoded = toBase64Url('???>>>~~~ subjects?');
    assert.match(encoded, /^[A-Za-z0-9_-]+$/);
});

test('the time readout shows minutes, seconds and milliseconds', () => {
    assert.equal(formatTime(0), '0:00.000');
    assert.equal(formatTime(62345), '1:02.345');
    assert.equal(formatTime(-1500), '-0:01.500');
    assert.equal(formatTime(999.6), '0:01.000');
});

test('animation frames add the frame number before the extension', () => {
    assert.equal(framePath('sb/spark.png', 2), 'sb/spark2.png');
    assert.equal(framePath('sb/folder.v2/spark.png', 0), 'sb/folder.v2/spark0.png');
    assert.equal(framePath('sb/noextension', 3), 'sb/noextension3');
});
