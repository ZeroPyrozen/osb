// Tests for lesson code highlighting. learn.js puts the result into the page as HTML, so whatever the
// code says must come out as text.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { highlight } from '../storyboard/highlight.js';

const tokens = (html, kind) => [...html.matchAll(new RegExp(`<span class="tok-${kind}">([^<]*)</span>`, 'g'))].map((m) => m[1]);

test('HTML inside a code block stays text, in every language', () => {
    const code = '<img src=x onerror="alert(1)"> & <script>alert("hi")</script>';
    for (const language of ['osb', 'js', 'javascript', 'cs', 'csharp', 'python', 'py', 'text', undefined]) {
        const html = highlight(code, language);
        assert.doesNotMatch(html, /<(img|script)/, `${language}: markup got through`);
        assert.match(html, /&lt;script&gt;/, `${language}: the code was lost`);
    }
});

test('.osb: objects, commands, numbers, strings, keywords, variables and comments', () => {
    const html = highlight([
        '// a comment',
        '[Events]',
        '$colour=255,128,0',
        'Sprite,Foreground,Centre,"sb/dot.png",320,240',
        ' F,0,1000,2000,0,1',
        ' C,0,0,,$colour',
        ' T,HitSoundClap,0,5000',
    ].join('\n'), 'osb');

    assert.deepEqual(tokens(html, 'comment'), ['// a comment']);
    assert.deepEqual(tokens(html, 'section'), ['[Events]']);
    assert.deepEqual(tokens(html, 'object'), ['Sprite']);
    assert.deepEqual(tokens(html, 'event'), ['F', 'C', 'T']);
    assert.deepEqual(tokens(html, 'keyword'), ['Foreground', 'Centre', 'HitSoundClap']);
    assert.deepEqual(tokens(html, 'string'), ['&quot;sb/dot.png&quot;']);
    assert.deepEqual(tokens(html, 'variable'), ['$colour', '$colour']);
    assert.ok(tokens(html, 'number').includes('320'));
});

test('C#, JavaScript and Python get keywords, strings, numbers and comments', () => {
    const cs = highlight('var sprite = layer.CreateSprite("sb/dot.png"); // add it', 'cs');
    assert.deepEqual(tokens(cs, 'keyword'), ['var']);
    assert.deepEqual(tokens(cs, 'string'), ['&quot;sb/dot.png&quot;']);
    assert.deepEqual(tokens(cs, 'object'), ['CreateSprite']);
    assert.deepEqual(tokens(cs, 'comment'), ['// add it']);

    const py = highlight('for i in range(10):  # ten', 'python');
    assert.deepEqual(tokens(py, 'keyword'), ['for', 'in', 'range']);
    assert.deepEqual(tokens(py, 'number'), ['10']);
    assert.deepEqual(tokens(py, 'comment'), ['# ten']);
});

test('lines are kept, and a final newline is dropped', () => {
    assert.equal(highlight('a\nb\n', 'text'), 'a\nb');
});
