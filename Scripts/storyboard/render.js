// Draws a compiled storyboard onto a canvas, the way osu! composes it: 640×480 storyboard space
// (854 wide in widescreen), layers stacked Background, Fail or Pass, Foreground, Overlay, and objects
// in the order they were declared. This is a learning preview, close to the game but not pixel-exact.

import { LAYERS, ORIGIN_POINTS } from './parse.js';
import { stateAt, frameAt } from './timeline.js';
import { framePath } from './assets.js';

const PLAY_AREA = { x: 60, y: 55, width: 510, height: 385 };
const TINT_CACHE_SIZE = 400;

export class StoryboardRenderer {
    #canvas;
    #ctx;
    #assets;
    #objects = [];
    #tints = new Map();
    missing = new Set();

    options = { widescreen: true, passing: true, guides: false };

    constructor(canvas, assets) {
        this.#canvas = canvas;
        this.#ctx = canvas.getContext('2d');
        this.#assets = assets;
    }

    /** Uses compiled objects from timeline.compile(). Starts loading the images they reference. */
    setObjects(objects) {
        this.#objects = objects;
        this.missing.clear();
        const paths = new Set();
        for (const o of objects) {
            const src = o.source;
            if (src.kind === 'animation') for (let f = 0; f < src.frameCount; f++) paths.add(framePath(src.path, f));
            else paths.add(src.path);
        }
        for (const path of paths) if (!this.#assets.has(path)) this.missing.add(path);
        return this.#assets.ready([...paths]);
    }

    /** Size the canvas backing store for the element's current size and the device's pixel ratio. */
    resize() {
        const rect = this.#canvas.getBoundingClientRect();
        const dpr = Math.min(window.devicePixelRatio || 1, 2);
        const width = Math.max(1, Math.round(rect.width * dpr));
        const height = Math.max(1, Math.round(rect.height * dpr));
        if (this.#canvas.width !== width || this.#canvas.height !== height) {
            this.#canvas.width = width;
            this.#canvas.height = height;
        }
    }

    /** Storyboard coordinates for a point on the canvas element (for the coordinate readout). */
    toStoryboard(clientX, clientY) {
        const rect = this.#canvas.getBoundingClientRect();
        const scale = rect.height / 480;
        const offsetX = (rect.width - 640 * scale) / 2;
        return { x: (clientX - rect.left - offsetX) / scale, y: (clientY - rect.top) / scale };
    }

    draw(time) {
        const ctx = this.#ctx;
        const { width, height } = this.#canvas;
        const scale = height / 480;
        const offsetX = (width - 640 * scale) / 2;

        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.globalAlpha = 1;
        ctx.globalCompositeOperation = 'source-over';
        ctx.fillStyle = '#000';
        ctx.fillRect(0, 0, width, height);

        const visible = new Set(['Background', this.options.passing ? 'Pass' : 'Fail', 'Foreground', 'Overlay']);
        for (const layer of LAYERS) {
            if (!visible.has(layer)) continue;
            for (const object of this.#objects) {
                if (object.source.layer !== layer) continue;
                const state = stateAt(object, time);
                if (state && state.opacity > 0.001) this.#drawObject(object, state, time, scale, offsetX);
            }
        }

        if (this.options.guides) this.#drawGuides(scale, offsetX);
    }

    #drawObject(object, state, time, scale, offsetX) {
        const src = object.source;
        const path = src.kind === 'animation' ? framePath(src.path, frameAt(object, time)) : src.path;
        const image = this.#assets.get(path) ?? placeholder();
        const w = image.width;
        const h = image.height;
        const [fx, fy] = ORIGIN_POINTS[src.origin];
        const ox = fx * w;
        const oy = fy * h;

        const ctx = this.#ctx;
        ctx.save();
        ctx.globalAlpha = Math.min(1, state.opacity);
        ctx.globalCompositeOperation = state.additive ? 'lighter' : 'source-over';
        ctx.translate(offsetX + state.x * scale, state.y * scale);
        ctx.scale(scale, scale);
        ctx.rotate(state.rotation);
        ctx.scale(Math.abs(state.scaleX), Math.abs(state.scaleY));
        // Flips (and negative scales) mirror the image in place, as osu! does, rather than around the origin.
        if (state.flipH !== state.scaleX < 0) {
            ctx.translate(w - 2 * ox, 0);
            ctx.scale(-1, 1);
        }
        if (state.flipV !== state.scaleY < 0) {
            ctx.translate(0, h - 2 * oy);
            ctx.scale(1, -1);
        }
        const [r, g, b] = state.color;
        const tinted = r >= 254.5 && g >= 254.5 && b >= 254.5 ? image : this.#tint(path, image, r, g, b);
        ctx.drawImage(tinted, -ox, -oy, w, h);
        ctx.restore();
    }

    /** The Colour command multiplies the image by a colour. Canvas can't do that per draw, so cache tinted copies. */
    #tint(path, image, r, g, b) {
        const key = `${path}|${Math.round(r)},${Math.round(g)},${Math.round(b)}|${image.width}`;
        let tinted = this.#tints.get(key);
        if (tinted) {
            this.#tints.delete(key); // move to the end: least recently used goes first
            this.#tints.set(key, tinted);
            return tinted;
        }
        tinted = document.createElement('canvas');
        tinted.width = image.width;
        tinted.height = image.height;
        const c = tinted.getContext('2d');
        c.drawImage(image, 0, 0);
        c.globalCompositeOperation = 'multiply';
        c.fillStyle = `rgb(${r},${g},${b})`;
        c.fillRect(0, 0, tinted.width, tinted.height);
        c.globalCompositeOperation = 'destination-in';
        c.drawImage(image, 0, 0);
        this.#tints.set(key, tinted);
        if (this.#tints.size > TINT_CACHE_SIZE) this.#tints.delete(this.#tints.keys().next().value);
        return tinted;
    }

    #drawGuides(scale, offsetX) {
        const ctx = this.#ctx;
        ctx.save();
        ctx.setTransform(scale, 0, 0, scale, offsetX, 0);
        ctx.lineWidth = 1 / scale;

        ctx.strokeStyle = 'rgba(184, 225, 215, 0.12)';
        ctx.beginPath();
        for (let x = 0; x <= 640; x += 80) { ctx.moveTo(x, 0); ctx.lineTo(x, 480); }
        for (let y = 0; y <= 480; y += 80) { ctx.moveTo(0, y); ctx.lineTo(640, y); }
        ctx.stroke();

        ctx.setLineDash([6 / scale, 4 / scale]);
        ctx.strokeStyle = 'rgba(97, 188, 166, 0.8)';
        ctx.strokeRect(PLAY_AREA.x, PLAY_AREA.y, PLAY_AREA.width, PLAY_AREA.height);
        if (this.options.widescreen) {
            ctx.strokeStyle = 'rgba(245, 196, 81, 0.7)';
            ctx.strokeRect(0, 0, 640, 480);
        }
        ctx.setLineDash([]);

        ctx.fillStyle = 'rgba(184, 225, 215, 0.85)';
        ctx.font = `${11 / scale}px "Open Sans Variable", sans-serif`;
        ctx.fillText('play area', PLAY_AREA.x + 4 / scale, PLAY_AREA.y + 13 / scale);
        if (this.options.widescreen) {
            ctx.fillStyle = 'rgba(245, 196, 81, 0.85)';
            ctx.fillText('4:3 (0-640)', 4 / scale, 470);
        }
        ctx.restore();
    }
}

let placeholderImage;
/** Shown for images that aren't in the library: a magenta and black checkerboard, like a missing texture. */
function placeholder() {
    if (!placeholderImage) {
        placeholderImage = document.createElement('canvas');
        placeholderImage.width = 64;
        placeholderImage.height = 64;
        const c = placeholderImage.getContext('2d');
        for (let y = 0; y < 4; y++) {
            for (let x = 0; x < 4; x++) {
                c.fillStyle = (x + y) % 2 ? '#000' : '#ff00ff';
                c.fillRect(x * 16, y * 16, 16, 16);
            }
        }
    }
    return placeholderImage;
}
