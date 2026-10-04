// The playground's sprite library: the images exercises and examples can use, drawn on canvases at
// runtime (white shapes tint well with the Colour command). Learners can add their own files too.

const white = '#ffffff';

function canvas(width, height, draw) {
    const c = document.createElement('canvas');
    c.width = width;
    c.height = height;
    const ctx = c.getContext('2d');
    draw(ctx, width, height);
    return c;
}

function circle(ctx, w, h, inset = 1) {
    ctx.fillStyle = white;
    ctx.beginPath();
    ctx.arc(w / 2, h / 2, w / 2 - inset, 0, Math.PI * 2);
    ctx.fill();
}

function starPath(ctx, cx, cy, outer, inner, points) {
    ctx.beginPath();
    for (let i = 0; i < points * 2; i++) {
        const r = i % 2 === 0 ? outer : inner;
        const a = -Math.PI / 2 + (i * Math.PI) / points;
        ctx.lineTo(cx + Math.cos(a) * r, cy + Math.sin(a) * r);
    }
    ctx.closePath();
}

function glow(ctx, w, h, hardness) {
    const g = ctx.createRadialGradient(w / 2, h / 2, 0, w / 2, h / 2, w / 2);
    g.addColorStop(0, 'rgba(255,255,255,1)');
    g.addColorStop(hardness, 'rgba(255,255,255,0.6)');
    g.addColorStop(1, 'rgba(255,255,255,0)');
    ctx.fillStyle = g;
    ctx.fillRect(0, 0, w, h);
}

function text(word, size = 56) {
    const font = `700 ${size}px "Exo 2 Variable", "Open Sans Variable", sans-serif`;
    const measure = document.createElement('canvas').getContext('2d');
    measure.font = font;
    const width = Math.ceil(measure.measureText(word).width) + 8;
    return canvas(width, Math.ceil(size * 1.3), (ctx, w, h) => {
        ctx.font = font;
        ctx.fillStyle = white;
        ctx.textBaseline = 'middle';
        ctx.fillText(word, 4, h / 2);
    });
}

/** Library entries: path, a short description for the library panel, and how to make the image. */
const LIBRARY = [
    { path: 'sb/dot.png', size: '32×32', description: 'small white circle', make: () => canvas(32, 32, (c, w, h) => circle(c, w, h)) },
    { path: 'sb/circle.png', size: '128×128', description: 'white circle', make: () => canvas(128, 128, (c, w, h) => circle(c, w, h)) },
    {
        path: 'sb/ring.png', size: '128×128', description: 'white ring',
        make: () => canvas(128, 128, (c, w) => { c.strokeStyle = white; c.lineWidth = 10; c.beginPath(); c.arc(w / 2, w / 2, w / 2 - 6, 0, Math.PI * 2); c.stroke(); }),
    },
    { path: 'sb/square.png', size: '100×100', description: 'white square', make: () => canvas(100, 100, (c) => { c.fillStyle = white; c.fillRect(1, 1, 98, 98); }) },
    { path: 'sb/pixel.png', size: '1×1', description: 'one white pixel: scale it into bars and boxes', make: () => canvas(1, 1, (c) => { c.fillStyle = white; c.fillRect(0, 0, 1, 1); }) },
    { path: 'sb/bar.png', size: '12×120', description: 'white bar', make: () => canvas(12, 120, (c) => { c.fillStyle = white; c.fillRect(1, 1, 10, 118); }) },
    { path: 'sb/glow.png', size: '128×128', description: 'soft white glow', make: () => canvas(128, 128, (c, w, h) => glow(c, w, h, 0.35)) },
    { path: 'sb/particle.png', size: '16×16', description: 'tiny soft dot', make: () => canvas(16, 16, (c, w, h) => glow(c, w, h, 0.5)) },
    {
        path: 'sb/star.png', size: '64×64', description: 'white star',
        make: () => canvas(64, 64, (c) => { c.fillStyle = white; starPath(c, 32, 33, 30, 12, 5); c.fill(); }),
    },
    {
        path: 'sb/triangle.png', size: '100×88', description: 'white triangle',
        make: () => canvas(100, 88, (c) => { c.fillStyle = white; c.beginPath(); c.moveTo(50, 1); c.lineTo(99, 87); c.lineTo(1, 87); c.closePath(); c.fill(); }),
    },
    {
        path: 'sb/heart.png', size: '64×58', description: 'white heart',
        make: () => canvas(64, 58, (c) => {
            c.fillStyle = white;
            c.beginPath();
            c.moveTo(32, 56);
            c.bezierCurveTo(-8, 28, 8, -6, 32, 14);
            c.bezierCurveTo(56, -6, 72, 28, 32, 56);
            c.fill();
        }),
    },
    {
        path: 'sb/note.png', size: '48×64', description: 'music note',
        make: () => canvas(48, 64, (c) => {
            c.fillStyle = white;
            c.beginPath(); c.ellipse(16, 50, 13, 10, -0.4, 0, Math.PI * 2); c.fill();
            c.fillRect(26, 6, 5, 44);
            c.beginPath(); c.moveTo(31, 6); c.quadraticCurveTo(46, 14, 44, 30); c.quadraticCurveTo(40, 20, 31, 18); c.fill();
        }),
    },
    { path: 'sb/text/hello.png', size: 'text', description: '"Hello, osu!" in white', make: () => text('Hello, osu!') },
    ...['Feel', 'the', 'beat', 'tonight'].map((word, i) => ({
        path: `sb/lyrics/${i}.png`, size: 'text', description: `lyric word "${word}"`, make: () => text(word, 48),
    })),
    {
        path: 'bg.jpg', size: '854×480', description: 'night-sky background (covers widescreen)',
        make: () => canvas(854, 480, (c, w, h) => {
            const g = c.createLinearGradient(0, 0, 0, h);
            g.addColorStop(0, '#0f2027');
            g.addColorStop(0.55, '#203a43');
            g.addColorStop(1, '#2c5364');
            c.fillStyle = g;
            c.fillRect(0, 0, w, h);
            let seed = 9;
            const rand = () => ((seed = (seed * 16807) % 2147483647) / 2147483647);
            for (let i = 0; i < 140; i++) {
                c.fillStyle = `rgba(255,255,255,${0.2 + rand() * 0.6})`;
                c.beginPath();
                c.arc(rand() * w, rand() * h * 0.7, rand() * 1.4 + 0.3, 0, Math.PI * 2);
                c.fill();
            }
        }),
    },
];

/** A 6-frame sparkle for Animation lessons: sb/spark0.png ... sb/spark5.png. */
const SPARK_FRAMES = 6;
for (let frame = 0; frame < SPARK_FRAMES; frame++) {
    LIBRARY.push({
        path: `sb/spark${frame}.png`, size: '64×64', description: frame === 0 ? 'sparkle animation, frames 0-5 (use "sb/spark.png", 6 frames)' : null,
        make: () => canvas(64, 64, (c) => {
            const t = Math.sin((frame / (SPARK_FRAMES - 1)) * Math.PI); // grows, then shrinks
            c.fillStyle = white;
            c.translate(32, 32);
            c.rotate((frame * Math.PI) / 12);
            starPath(c, 0, 0, 8 + 22 * t, 3 + 3 * t, 4);
            c.fill();
        }),
    });
}

/** Images loaded from the site, for a friendlier preview. */
const REMOTE = [
    { path: 'sb/hifumi.png', size: '624×447', description: 'Hifumi, the osb mascot', url: '/images/mascot/hifumi-wave.webp' },
    { path: 'sb/osb.png', size: '256×256', description: 'the osb! logo', url: '/images/osb.svg', width: 256, height: 256 },
];

function loadImage(url, width, height) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
            if (!width) return resolve(img);
            // Rasterise SVGs at a fixed size so they behave like a PNG of that size.
            resolve(canvas(width, height, (c) => c.drawImage(img, 0, 0, width, height)));
        };
        img.onerror = () => reject(new Error(`Couldn't load ${url}`));
        img.src = url;
    });
}

const normalise = (path) => path.replace(/\\/g, '/').replace(/^\.?\//, '').toLowerCase();

/** Resolves storyboard paths to drawable images. */
export class Assets {
    #images = new Map();   // normalised path -> canvas or image
    #loading = new Map();  // normalised path -> promise
    #userFiles = new Set();

    constructor() {
        for (const entry of LIBRARY) this.#loading.set(normalise(entry.path), null);
        for (const entry of REMOTE) this.#loading.set(normalise(entry.path), null);
    }

    /** The library as shown in the playground's sprite panel. */
    static get catalog() {
        return [...LIBRARY, ...REMOTE].filter((e) => e.description).map(({ path, size, description }) => ({ path, size, description }));
    }

    has(path) {
        const key = normalise(path);
        return this.#images.has(key) || this.#loading.has(key);
    }

    /** The image for a path, or undefined if it isn't ready (yet). Starts loading on first request. */
    get(path) {
        const key = normalise(path);
        const ready = this.#images.get(key);
        if (ready) return ready;
        if (!this.#loading.has(key)) return undefined;
        if (this.#loading.get(key) === null) this.#loading.set(key, this.#create(key));
        return undefined;
    }

    /** Resolves once every path in the list is drawable (or known to be missing). */
    async ready(paths) {
        await Promise.all(paths.map(async (path) => {
            const key = normalise(path);
            this.get(path);
            const pending = this.#loading.get(key);
            if (pending) await pending.catch(() => {});
        }));
    }

    async #create(key) {
        const local = LIBRARY.find((e) => normalise(e.path) === key);
        if (local) {
            const image = local.make();
            this.#images.set(key, image);
            return image;
        }
        const remote = REMOTE.find((e) => normalise(e.path) === key);
        const image = await loadImage(remote.url, remote.width, remote.height);
        this.#images.set(key, image);
        return image;
    }

    /** Adds a learner's own image file under its file name (and sb/<name>). */
    async addFile(file) {
        const url = URL.createObjectURL(file);
        const image = await loadImage(url);
        for (const path of [file.name, `sb/${file.name}`]) {
            this.#images.set(normalise(path), image);
            this.#userFiles.add(path);
        }
        return file.name;
    }

    get userFiles() {
        return [...this.#userFiles].filter((p) => !p.startsWith('sb/'));
    }
}

/** The path of an animation frame: "sb/spark.png", frame 2 -> "sb/spark2.png". */
export function framePath(path, frame) {
    const dot = path.lastIndexOf('.');
    return dot < 0 ? `${path}${frame}` : `${path.slice(0, dot)}${frame}${path.slice(dot)}`;
}
