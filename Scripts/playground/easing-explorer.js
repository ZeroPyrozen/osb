// The easing explorer used in the easing lessons: pick an easing, see its curve, and watch a sprite
// travel with it next to a linear one.

import { ease, easingLabels } from '../storyboard/easing.js';

const DURATION = 1600;
const PAUSE = 500;

export function mountEasingExplorer(root) {
    const initial = Number(root.dataset.easing ?? 1);
    root.className = 'not-prose my-6 grid gap-4 rounded-card border border-charcoal-600 bg-charcoal-900 p-4 sm:grid-cols-[minmax(0,15rem)_minmax(0,1fr)]';
    root.innerHTML = `
        <div class="flex flex-col gap-3">
            <label class="flex flex-col gap-1 text-sm font-semibold text-mint-200">Easing
                <select data-pick class="input py-1.5 text-sm">
                    ${easingLabels.map((label, i) => `<option value="${i}" ${i === initial ? 'selected' : ''}>${i}: ${label}</option>`).join('')}
                </select>
            </label>
            <canvas data-curve width="240" height="240" class="aspect-square w-full rounded-md bg-charcoal-950" aria-label="Curve of the selected easing"></canvas>
        </div>
        <div class="flex min-w-0 flex-col justify-center gap-6">
            <div>
                <p class="mb-1 text-xs font-semibold text-sage">Linear (0)</p>
                <div class="relative h-8 rounded-full bg-charcoal-950"><span data-linear class="absolute top-1 size-6 rounded-full bg-sage"></span></div>
            </div>
            <div>
                <p data-name class="mb-1 text-xs font-semibold text-mint-200"></p>
                <div class="relative h-8 rounded-full bg-charcoal-950"><span data-eased class="absolute top-1 size-6 rounded-full bg-mint-400"></span></div>
            </div>
            <p class="text-xs text-sage">Both dots take the same time. Only how they spend it differs.</p>
        </div>`;

    const pick = root.querySelector('[data-pick]');
    const curve = root.querySelector('[data-curve]');
    const linear = root.querySelector('[data-linear]');
    const eased = root.querySelector('[data-eased]');
    const name = root.querySelector('[data-name]');
    const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;

    const drawCurve = () => {
        const n = Number(pick.value);
        name.textContent = `${easingLabels[n]} (${n})`;
        const ctx = curve.getContext('2d');
        const w = curve.width;
        const h = curve.height;
        const pad = 30;
        const yOf = (v) => h - pad - v * (h - 2 * pad);
        ctx.clearRect(0, 0, w, h);
        ctx.strokeStyle = 'rgba(144,169,163,0.35)';
        ctx.lineWidth = 1;
        ctx.setLineDash([4, 4]);
        for (const v of [0, 1]) { ctx.beginPath(); ctx.moveTo(pad, yOf(v)); ctx.lineTo(w - pad, yOf(v)); ctx.stroke(); }
        ctx.beginPath(); ctx.moveTo(pad, yOf(0)); ctx.lineTo(w - pad, yOf(1)); ctx.stroke();
        ctx.setLineDash([]);
        ctx.strokeStyle = '#61bca6';
        ctx.lineWidth = 3;
        ctx.beginPath();
        for (let i = 0; i <= 120; i++) {
            const t = i / 120;
            const x = pad + t * (w - 2 * pad);
            i === 0 ? ctx.moveTo(x, yOf(ease(n, t))) : ctx.lineTo(x, yOf(ease(n, t)));
        }
        ctx.stroke();
        ctx.fillStyle = 'rgba(184,225,215,0.8)';
        ctx.font = '12px "Open Sans Variable", sans-serif';
        ctx.fillText('start value', 4, yOf(0) + 16);
        ctx.fillText('end value', 4, yOf(1) - 8);
        ctx.fillText('time →', w - 52, h - 8);
    };

    // Back and Elastic easings overshoot, so allow the dot a little past either end.
    const place = (dot, p) => {
        dot.style.left = `calc((100% - 1.75rem) * ${Math.max(-0.15, Math.min(1.15, p))} + 0.125rem)`;
    };

    let startTime = performance.now();
    const frame = (now) => {
        const cycle = (now - startTime) % (DURATION + PAUSE);
        const t = Math.min(1, cycle / DURATION);
        place(linear, t);
        place(eased, ease(Number(pick.value), t));
        if (!reduceMotion) requestAnimationFrame(frame);
    };

    pick.addEventListener('change', () => {
        drawCurve();
        startTime = performance.now();
        if (reduceMotion) { place(linear, 1); place(eased, 1); }
    });
    drawCurve();
    if (reduceMotion) { place(linear, 1); place(eased, 1); } else requestAnimationFrame(frame);
}
