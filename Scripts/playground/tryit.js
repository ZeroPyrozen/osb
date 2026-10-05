// "Preview" and "Open in playground" buttons for .osb code blocks in lessons, and "Run in playground"
// for script-mode examples.

import { Player } from './player.js';

const toBase64Url = (s) => btoa(String.fromCharCode(...new TextEncoder().encode(s))).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

export function mountTryIt(pre) {
    const code = pre.querySelector('code').textContent;
    if (!/^(Sprite|Animation)\b/m.test(code)) return; // fragments without an object can't be previewed

    const bar = document.createElement('div');
    bar.className = 'not-prose -mb-2 flex justify-end gap-2';
    bar.innerHTML = `
        <a href="/learn/playground#osb=${toBase64Url(code)}" class="btn btn-ghost px-2.5 py-1 text-xs">Open in playground</a>
        <button type="button" class="btn btn-outline px-2.5 py-1 text-xs" aria-expanded="false">Preview</button>`;
    pre.before(bar);

    const button = bar.querySelector('button');
    let holder = null;
    let player = null;
    button.addEventListener('click', () => {
        if (player) {
            player.destroy();
            holder.remove();
            player = holder = null;
            button.textContent = 'Preview';
            button.setAttribute('aria-expanded', 'false');
            return;
        }
        holder = document.createElement('div');
        holder.className = 'not-prose my-4';
        pre.after(holder);
        player = new Player(holder, { compact: true, minDuration: 2000 });
        player.load(code, { keepTime: false });
        player.play();
        button.textContent = 'Close preview';
        button.setAttribute('aria-expanded', 'true');
    });
}

/** "Run in playground" for JavaScript examples that build a storyboard (they call sb.layer). */
export function mountScriptLink(pre) {
    const code = pre.querySelector('code').textContent;
    if (!/\bsb\.layer\(/.test(code)) return;

    const bar = document.createElement('div');
    bar.className = 'not-prose -mb-2 flex justify-end gap-2';
    bar.innerHTML = `<a href="/learn/playground#js=${toBase64Url(code)}" class="btn btn-outline px-2.5 py-1 text-xs">Run in playground</a>`;
    pre.before(bar);
}
