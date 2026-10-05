// The storyboard preview player: canvas, transport controls and display toggles.
// Used by exercises, the playground and the "Preview" buttons on lesson code blocks.

import { parse } from '../storyboard/parse.js';
import { compile, storyboardSpan } from '../storyboard/timeline.js';
import { StoryboardRenderer } from '../storyboard/render.js';
import { Assets } from '../storyboard/assets.js';

let sharedAssets;
export const assets = () => (sharedAssets ??= new Assets());

const icon = {
    play: '<svg class="size-5" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 5.5v13a1 1 0 0 0 1.52.85l10.5-6.5a1 1 0 0 0 0-1.7L9.52 4.65A1 1 0 0 0 8 5.5Z" fill="currentColor"/></svg>',
    pause: '<svg class="size-5" viewBox="0 0 24 24" aria-hidden="true"><rect x="6" y="5" width="4" height="14" rx="1" fill="currentColor"/><rect x="14" y="5" width="4" height="14" rx="1" fill="currentColor"/></svg>',
    restart: '<svg class="size-5" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 12a8 8 0 1 0 2.34-5.66M4 4v4.5h4.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/></svg>',
};

/** "1:02.345" from milliseconds; negative times (intros) keep their sign. */
export function formatTime(ms) {
    const sign = ms < 0 ? '-' : '';
    const abs = Math.abs(Math.round(ms));
    const minutes = Math.floor(abs / 60000);
    const seconds = Math.floor((abs % 60000) / 1000);
    return `${sign}${minutes}:${String(seconds).padStart(2, '0')}.${String(abs % 1000).padStart(3, '0')}`;
}

export class Player {
    #root;
    #canvas;
    #renderer;
    #elements = {};
    #time = 0;
    #range = { start: 0, end: 1000 };
    #playing = false;
    #speed = 1;
    #frame = 0;
    #lastTick = 0;
    #visible = true;
    #observers = [];
    #audio;
    #options;
    objects = [];
    model = null;

    /**
     * @param {HTMLElement} root  Element to render into.
     * @param {object} options  compact, widescreen, guides, loop, minDuration, metronome: {bpm, offset}
     */
    constructor(root, options = {}) {
        this.#options = { widescreen: true, guides: false, loop: true, compact: false, minDuration: 1000, ...options };
        this.#root = root;
        this.#build();
        this.#renderer = new StoryboardRenderer(this.#canvas, assets());
        this.#renderer.options.widescreen = this.#options.widescreen;
        this.#renderer.options.guides = this.#options.guides;
        this.#watch();
    }

    #build() {
        const o = this.#options;
        const toggles = o.compact ? '' : `
            <label class="flex items-center gap-1.5"><input type="checkbox" data-opt="widescreen" class="accent-mint-400" ${o.widescreen ? 'checked' : ''}>Widescreen</label>
            <label class="flex items-center gap-1.5"><input type="checkbox" data-opt="passing" class="accent-mint-400" checked>Pass state</label>
            <label class="flex items-center gap-1.5"><input type="checkbox" data-opt="guides" class="accent-mint-400" ${o.guides ? 'checked' : ''}>Guides</label>
            <label class="flex items-center gap-1.5">Speed
                <select data-speed class="rounded border border-charcoal-500 bg-charcoal-900 px-1 py-0.5 text-mint-100">
                    <option value="0.25">0.25×</option><option value="0.5">0.5×</option><option value="1" selected>1×</option><option value="2">2×</option>
                </select></label>`;
        this.#root.innerHTML = `
            <div class="flex flex-col gap-2">
                <div data-stage class="relative w-full overflow-hidden rounded-card bg-black shadow-card ${o.widescreen ? 'aspect-video' : 'aspect-[4/3]'}">
                    <canvas class="absolute inset-0 size-full" tabindex="0" aria-label="Storyboard preview. Space plays or pauses, arrow keys seek."></canvas>
                    <div data-coords class="pointer-events-none absolute top-2 left-2 hidden rounded bg-night/80 px-2 py-0.5 font-mono text-xs text-mint-100"></div>
                    <div data-missing class="absolute inset-x-2 bottom-2 hidden rounded bg-night/85 px-3 py-1.5 text-xs text-gold-200"></div>
                </div>
                <div class="flex flex-wrap items-center gap-x-3 gap-y-2 text-xs text-sage">
                    <button type="button" data-play class="flex size-9 items-center justify-center rounded-md bg-mint-400 text-charcoal-950 transition hover:bg-mint-300" aria-label="Play">${icon.play}</button>
                    <button type="button" data-restart class="icon-btn" aria-label="Back to the start">${icon.restart}</button>
                    <span data-clock class="min-w-[9.5rem] font-mono text-mint-100 tabular-nums"></span>
                    <input type="range" data-seek min="0" max="1000" step="1" value="0" class="min-w-32 flex-1 accent-mint-400" aria-label="Time">
                    ${toggles}
                </div>
            </div>`;
        const q = (sel) => this.#root.querySelector(sel);
        this.#canvas = q('canvas');
        Object.assign(this.#elements, {
            stage: q('[data-stage]'), play: q('[data-play]'), restart: q('[data-restart]'), clock: q('[data-clock]'),
            seek: q('[data-seek]'), coords: q('[data-coords]'), missing: q('[data-missing]'), speed: q('[data-speed]'),
        });

        const e = this.#elements;
        e.play.addEventListener('click', () => this.toggle());
        e.restart.addEventListener('click', () => this.seek(this.#range.start));
        e.seek.addEventListener('input', () => this.seek(Number(e.seek.value)));
        e.speed?.addEventListener('change', () => { this.#speed = Number(e.speed.value); });
        for (const box of this.#root.querySelectorAll('[data-opt]')) {
            box.addEventListener('change', () => {
                const name = box.dataset.opt;
                this.#renderer.options[name] = box.checked;
                if (name === 'widescreen') {
                    e.stage.classList.toggle('aspect-video', box.checked);
                    e.stage.classList.toggle('aspect-[4/3]', !box.checked);
                    this.#renderer.resize();
                }
                this.#draw();
            });
        }

        this.#canvas.addEventListener('keydown', (event) => {
            if (event.key === ' ' || event.key === 'k') { event.preventDefault(); this.toggle(); }
            if (event.key === 'ArrowRight') { event.preventDefault(); this.seek(this.#time + (event.shiftKey ? 1000 : 100)); }
            if (event.key === 'ArrowLeft') { event.preventDefault(); this.seek(this.#time - (event.shiftKey ? 1000 : 100)); }
        });
        this.#canvas.addEventListener('pointermove', (event) => {
            const p = this.#renderer.toStoryboard(event.clientX, event.clientY);
            e.coords.textContent = `x ${Math.round(p.x)}  y ${Math.round(p.y)}`;
            e.coords.classList.remove('hidden');
        });
        this.#canvas.addEventListener('pointerleave', () => e.coords.classList.add('hidden'));
    }

    #watch() {
        const resize = new ResizeObserver(() => { this.#renderer.resize(); this.#draw(); });
        resize.observe(this.#canvas);
        const visibility = new IntersectionObserver(([entry]) => { this.#visible = entry.isIntersecting; });
        visibility.observe(this.#root);
        this.#observers.push(resize, visibility);
    }

    /** Parses and shows a script. Returns the parse result so callers can show diagnostics. */
    load(text, { keepTime = true } = {}) {
        this.model = parse(text);
        this.objects = compile(this.model);
        const span = storyboardSpan(this.objects);
        const start = Math.min(0, span.start);
        const end = Math.max(span.end, start + this.#options.minDuration);
        this.#range = { start, end };
        const e = this.#elements;
        e.seek.min = String(start);
        e.seek.max = String(end);
        if (!keepTime || this.#time < start || this.#time > end) this.#time = start;
        if (this.model.widescreen != null && this.#root.querySelector('[data-opt="widescreen"]')) {
            const box = this.#root.querySelector('[data-opt="widescreen"]');
            if (box.checked !== this.model.widescreen) box.click();
        }

        const ready = this.#renderer.setObjects(this.objects);
        const missing = [...this.#renderer.missing];
        e.missing.classList.toggle('hidden', missing.length === 0);
        e.missing.textContent = missing.length
            ? `Not in the playground library: ${missing.slice(0, 3).join(', ')}${missing.length > 3 ? ` and ${missing.length - 3} more` : ''}. Shown as a checkerboard.`
            : '';
        ready.then(() => this.#draw());
        this.#draw();
        return this.model;
    }

    get time() { return this.#time; }
    get playing() { return this.#playing; }

    seek(ms) {
        this.#time = Math.min(this.#range.end, Math.max(this.#range.start, ms));
        this.#draw();
    }

    play() {
        if (this.#playing) return;
        if (this.#time >= this.#range.end) this.#time = this.#range.start;
        this.#playing = true;
        this.#lastTick = performance.now();
        this.#elements.play.innerHTML = icon.pause;
        this.#elements.play.setAttribute('aria-label', 'Pause');
        this.#frame = requestAnimationFrame((now) => this.#tick(now));
    }

    pause() {
        this.#playing = false;
        cancelAnimationFrame(this.#frame);
        this.#elements.play.innerHTML = icon.play;
        this.#elements.play.setAttribute('aria-label', 'Play');
    }

    toggle() {
        this.#playing ? this.pause() : this.play();
    }

    #tick(now) {
        if (!this.#playing) return;
        const elapsed = (now - this.#lastTick) * this.#speed;
        this.#lastTick = now;
        const before = this.#time;
        let next = this.#time + elapsed;
        if (next > this.#range.end) {
            if (this.#options.loop) next = this.#range.start + ((next - this.#range.start) % Math.max(1, this.#range.end - this.#range.start));
            else {
                next = this.#range.end;
                this.pause();
            }
        }
        this.#metronome(before, next);
        this.#time = next;
        if (this.#visible && !document.hidden) this.#draw();
        if (this.#playing) this.#frame = requestAnimationFrame((t) => this.#tick(t));
    }

    /** Clicks on every beat crossed since the last frame, for the timing lessons. */
    #metronome(from, to) {
        const m = this.#options.metronome;
        if (!m?.bpm || to < from) return;
        const beat = 60000 / m.bpm;
        const offset = m.offset ?? 0;
        const first = Math.ceil((from - offset) / beat);
        const last = Math.floor((to - offset) / beat);
        for (let i = first; i <= last; i++) this.#click(i % 4 === 0);
    }

    #click(downbeat) {
        try {
            this.#audio ??= new AudioContext();
            const ctx = this.#audio;
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.frequency.value = downbeat ? 1320 : 880;
            gain.gain.setValueAtTime(0.18, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.06);
            osc.connect(gain).connect(ctx.destination);
            osc.start();
            osc.stop(ctx.currentTime + 0.07);
        } catch {
            // Audio is a nice extra; ignore browsers that refuse it.
        }
    }

    #draw() {
        this.#renderer.draw(this.#time);
        const e = this.#elements;
        e.clock.textContent = `${formatTime(this.#time)} / ${formatTime(this.#range.end)}`;
        if (document.activeElement !== e.seek) e.seek.value = String(Math.round(this.#time));
    }

    destroy() {
        this.pause();
        for (const o of this.#observers) o.disconnect();
        this.#audio?.close();
        this.#root.innerHTML = '';
    }
}
