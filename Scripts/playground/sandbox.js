// The free playground at /learn/playground: write .osb or script-mode code, preview it, add your
// own images, share a link or download the result.

import { createEditor } from './editor.js';
import { Player, assets } from './player.js';
import { runInWorker } from './run-script.js';
import { toBase64Url, fromBase64Url } from './share-link.js';
import { Assets } from '../storyboard/assets.js';

const DRAFT_KEY = 'osb.playground.v1';

const EXAMPLES = {
    osb: {
        'Hello, osu!': `// A title that rises, pops and fades out. Press play!
Sprite,Background,TopLeft,"bg.jpg",-107,0
 F,0,0,4000,1

Sprite,Foreground,Centre,"sb/text/hello.png",320,240
 F,0,0,500,0,1
 MY,30,0,800,300,240
 S,4,1500,1750,1,1.3
 S,4,1750,2000,1.3,1
 F,0,3500,4000,1,0
`,
        'Bouncing ball': `Sprite,Foreground,BottomCentre,"sb/circle.png",320,420
 S,0,0,,0.5
 MY,33,0,1200,60,420
 MX,0,0,1200,120,520
 C,0,0,1200,255,255,255,97,188,166
`,
        'Pulsing glow (loop)': `Sprite,Foreground,Centre,"sb/glow.png",320,240
 P,0,0,,A
 C,0,0,,97,188,166
 L,0,8
  S,17,0,250,1,2
  S,17,250,500,2,1
  F,0,0,250,1,0.6
  F,0,250,500,0.6,1
`,
        'Sparkle animation': `Animation,Foreground,Centre,"sb/spark.png",320,240,6,80,LoopForever
 F,0,0,3000,1
 R,0,0,3000,0,6.283
 S,0,0,3000,2
`,
    },
    script: {
        'Circle of dots': `// 24 dots around a circle, appearing one after another.
const layer = sb.layer('Foreground');
const count = 24;
for (let i = 0; i < count; i++) {
    const angle = (i / count) * Math.PI * 2;
    const x = 320 + Math.cos(angle) * 160;
    const y = 240 + Math.sin(angle) * 160;
    const dot = layer.sprite('sb/dot.png', 'Centre', x, y);
    const start = i * 60;
    dot.fade(start, start + 300, 0, 1);
    dot.scale('OutBack', start, start + 300, 0, 1);
    dot.fade(3000, 3500, 1, 0);
}
`,
        Snowfall: `// Seeded random snow: the same flakes every run.
const layer = sb.layer('Foreground');
for (let i = 0; i < 60; i++) {
    const flake = layer.sprite('sb/particle.png', 'Centre');
    const start = random(0, 3000);
    const x = random(-107, 747);
    const drift = random(-60, 60);
    flake.move(start, start + 3000, [x, -20], [x + drift, 500]);
    flake.fade(start, start + 400, 0, random(0.4, 1));
    flake.scale(start, random(0.6, 1.6));
}
`,
    },
};

function readShared() {
    const m = /^#(osb|js)=(.+)$/.exec(location.hash);
    if (!m) return null;
    try {
        return { mode: m[1] === 'js' ? 'script' : 'osb', text: fromBase64Url(m[2]) };
    } catch {
        return null;
    }
}

function readDraft() {
    try { return JSON.parse(localStorage.getItem(DRAFT_KEY)); } catch { return null; }
}

function saveDraft(draft) {
    try { localStorage.setItem(DRAFT_KEY, JSON.stringify(draft)); } catch { /* not kept in private mode */ }
}

export function mountSandbox(root) {
    const start = readShared() ?? readDraft() ?? { mode: 'osb', text: EXAMPLES.osb['Hello, osu!'] };
    let mode = start.mode;
    let osb = '';

    const q = (sel) => root.querySelector(sel);
    const status = q('[data-status]');
    const player = new Player(q('[data-player]'), { minDuration: 3000, guides: false });

    const library = q('[data-library]');
    library.innerHTML = Assets.catalog.map((item) => `
        <li><button type="button" data-insert="${item.path}" class="flex w-full items-baseline justify-between gap-3 rounded-md px-2 py-1.5 text-left hover:bg-charcoal-700">
            <code class="font-mono text-xs text-gold-200">${item.path}</code><span class="text-xs text-sage">${item.description}</span>
        </button></li>`).join('');

    const examples = q('[data-examples]');
    const fillExamples = () => {
        examples.innerHTML = '<option value="">Load an example…</option>' +
            Object.keys(EXAMPLES[mode]).map((name) => `<option>${name}</option>`).join('');
    };

    const refresh = async (text) => {
        saveDraft({ mode, text });
        if (mode === 'osb') {
            osb = text;
            const model = player.load(text);
            const errors = model.diagnostics.filter((d) => d.severity === 'error').length;
            status.textContent = errors ? `${errors} ${errors === 1 ? 'error' : 'errors'}: hover the underlined lines.` : `${model.objects.length} objects.`;
            return;
        }
        status.textContent = 'Running…';
        const result = await runInWorker(text);
        if (result.cancelled) return;
        if (!result.ok) {
            status.textContent = result.error + (result.line ? ` (line ${result.line})` : '');
            return;
        }
        osb = result.osb;
        q('[data-generated]').textContent = osb;
        status.textContent = `Generated ${result.objects} sprites and ${result.commands} commands.${result.logs.length ? ' Log: ' + result.logs.slice(-3).join(' | ') : ''}`;
        player.load(osb);
    };

    let timer;
    const editor = createEditor(q('[data-editor]'), {
        doc: start.text,
        mode,
        onChange: (text) => {
            clearTimeout(timer);
            timer = setTimeout(() => refresh(text), mode === 'script' ? 500 : 200);
        },
    });

    const setMode = (next, text) => {
        mode = next;
        for (const tab of root.querySelectorAll('[data-mode]')) tab.setAttribute('aria-pressed', String(tab.dataset.mode === mode));
        q('[data-generated-wrap]').hidden = mode !== 'script';
        editor.setMode(mode);
        fillExamples();
        if (text != null) editor.text = text;
        refresh(editor.text);
    };

    for (const tab of root.querySelectorAll('[data-mode]')) {
        tab.addEventListener('click', () => {
            if (tab.dataset.mode === mode) return;
            const first = Object.values(EXAMPLES[tab.dataset.mode])[0];
            setMode(tab.dataset.mode, first);
        });
    }

    examples.addEventListener('change', () => {
        if (examples.value) editor.text = EXAMPLES[mode][examples.value];
        examples.value = '';
    });

    library.addEventListener('click', (event) => {
        const button = event.target.closest('[data-insert]');
        if (!button) return;
        const path = button.dataset.insert;
        if (mode === 'osb') {
            const isAnimation = path === 'sb/spark0.png';
            editor.insertLine(isAnimation
                ? 'Animation,Foreground,Centre,"sb/spark.png",320,240,6,80,LoopForever\n F,0,0,3000,1'
                : `Sprite,Foreground,Centre,"${path}",320,240\n F,0,0,3000,1`);
        } else {
            editor.insertLine(`sb.layer('Foreground').sprite('${path}', 'Centre', 320, 240).fade(0, 3000, 1, 1);`);
        }
    });

    q('[data-upload]').addEventListener('change', async (event) => {
        const names = [];
        for (const file of event.target.files) {
            try {
                names.push(await assets().addFile(file));
            } catch {
                status.textContent = `Couldn't read ${file.name} as an image.`;
            }
        }
        if (names.length) {
            status.textContent = `Added ${names.map((n) => `"${n}"`).join(', ')}. Use the file name as the sprite path.`;
            refresh(editor.text);
        }
        event.target.value = '';
    });

    q('[data-share]').addEventListener('click', async () => {
        const url = `${location.origin}${location.pathname}#${mode === 'script' ? 'js' : 'osb'}=${toBase64Url(editor.text)}`;
        history.replaceState(null, '', url);
        try {
            await navigator.clipboard.writeText(url);
            status.textContent = 'Link copied. Anyone with it sees this storyboard.';
        } catch {
            status.textContent = 'Copy the link from the address bar to share this storyboard.';
        }
    });

    q('[data-download]').addEventListener('click', () => {
        const blob = new Blob([osb.startsWith('[Events]') ? osb : `[Events]\n${osb}`], { type: 'text/plain' });
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = 'playground.osb';
        a.click();
        setTimeout(() => URL.revokeObjectURL(a.href), 1000);
    });

    setMode(mode);
}
