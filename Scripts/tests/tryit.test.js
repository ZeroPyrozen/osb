// Tests for the buttons on lesson code blocks (Scripts/playground/tryit.js).

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { open } from './dom.js';
import { mountTryIt, mountScriptLink } from '../playground/tryit.js';
import { fromBase64Url } from '../playground/share-link.js';

/** A lesson code block, as Markdig renders it. */
function codeBlock(language, code) {
    open(`<div class="prose-osb"><pre><code class="language-${language}"></code></pre></div>`);
    const pre = document.querySelector('pre');
    pre.querySelector('code').textContent = code;
    return pre;
}

const linkTo = (pre) => pre.previousElementSibling?.querySelector('a')?.getAttribute('href') ?? null;
const codeIn = (href, kind) => fromBase64Url(href.slice(`/learn/playground#${kind}=`.length));

test('"Run in playground" is only for scripts that build a storyboard', () => {
    const plain = codeBlock('js', 'const answer = 42;');
    mountScriptLink(plain);
    assert.equal(linkTo(plain), null);

    const script = 'const layer = sb.layer("Foreground");\nlayer.sprite("sb/dot.png").fade(0, 1000, 0, 1);';
    const builds = codeBlock('js', script);
    mountScriptLink(builds);
    assert.match(linkTo(builds), /^\/learn\/playground#js=[A-Za-z0-9_-]+$/);
    assert.equal(codeIn(linkTo(builds), 'js'), script);
});

test('playground links keep the code intact, Japanese titles included', () => {
    const script = '// 夜に駆ける (YOASOBI)\nsb.layer("Foreground").sprite("sb/text/夜.png");';
    const pre = codeBlock('js', script);

    mountScriptLink(pre);

    assert.equal(codeIn(linkTo(pre), 'js'), script);
});

test('.osb examples with an object get Preview and Open in playground; fragments get nothing', () => {
    const fragment = codeBlock('osb', ' F,0,1000,2000,0,1');
    mountTryIt(fragment);
    assert.equal(fragment.previousElementSibling, null);

    const code = 'Sprite,Foreground,Centre,"sb/dot.png",320,240\n F,0,1000,2000,0,1';
    const example = codeBlock('osb', code);
    mountTryIt(example);
    const bar = example.previousElementSibling;
    assert.equal(codeIn(linkTo(example), 'osb'), code);
    assert.equal(bar.querySelector('button').textContent, 'Preview');
    assert.equal(bar.querySelector('button').getAttribute('aria-expanded'), 'false');
});
