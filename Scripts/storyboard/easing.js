// The 35 storyboard easings, numbered as in .osb files (see the osu! wiki, Storyboard scripting commands).
// Formulas match osu!framework's DefaultEasingFunction (MIT licence, ppy Pty Ltd), so curves look
// the same as in the game.

const elasticConst = (2 * Math.PI) / 0.3;
const elasticConst2 = 0.3 / 4;
const backConst = 1.70158;
const backConst2 = backConst * 1.525;
const bounceConst = 1 / 2.75;
const expoOffset = Math.pow(2, -10);
const elasticOffsetFull = Math.pow(2, -11);
const elasticOffsetHalf = Math.pow(2, -10) * Math.sin((0.5 - elasticConst2) * elasticConst);
const elasticOffsetQuarter = Math.pow(2, -10) * Math.sin((0.25 - elasticConst2) * elasticConst);
const inOutElasticOffset = Math.pow(2, -10) * Math.sin((1 - elasticConst2 * 1.5) * elasticConst / 1.5);

function outBounce(t) {
    if (t < bounceConst) return 7.5625 * t * t;
    if (t < 2 * bounceConst) return 7.5625 * (t -= 1.5 * bounceConst) * t + 0.75;
    if (t < 2.5 * bounceConst) return 7.5625 * (t -= 2.25 * bounceConst) * t + 0.9375;
    return 7.5625 * (t -= 2.625 * bounceConst) * t + 0.984375;
}

/** [name, function] by easing number. */
export const easings = [
    ['Linear', (t) => t],
    ['Out', (t) => t * (2 - t)],
    ['In', (t) => t * t],
    ['InQuad', (t) => t * t],
    ['OutQuad', (t) => t * (2 - t)],
    ['InOutQuad', (t) => (t < 0.5 ? t * t * 2 : --t * t * -2 + 1)],
    ['InCubic', (t) => t * t * t],
    ['OutCubic', (t) => --t * t * t + 1],
    ['InOutCubic', (t) => (t < 0.5 ? t * t * t * 4 : --t * t * t * 4 + 1)],
    ['InQuart', (t) => t * t * t * t],
    ['OutQuart', (t) => 1 - --t * t * t * t],
    ['InOutQuart', (t) => (t < 0.5 ? t * t * t * t * 8 : --t * t * t * t * -8 + 1)],
    ['InQuint', (t) => t * t * t * t * t],
    ['OutQuint', (t) => --t * t * t * t * t + 1],
    ['InOutQuint', (t) => (t < 0.5 ? t * t * t * t * t * 16 : --t * t * t * t * t * 16 + 1)],
    ['InSine', (t) => 1 - Math.cos(t * Math.PI * 0.5)],
    ['OutSine', (t) => Math.sin(t * Math.PI * 0.5)],
    ['InOutSine', (t) => 0.5 - 0.5 * Math.cos(Math.PI * t)],
    ['InExpo', (t) => Math.pow(2, 10 * (t - 1)) + expoOffset * (t - 1)],
    ['OutExpo', (t) => -Math.pow(2, -10 * t) + 1 + expoOffset * t],
    ['InOutExpo', (t) => (t < 0.5
        ? 0.5 * (Math.pow(2, 20 * t - 10) + expoOffset * (2 * t - 1))
        : 1 - 0.5 * (Math.pow(2, -20 * t + 10) + expoOffset * (-2 * t + 1)))],
    ['InCirc', (t) => 1 - Math.sqrt(1 - t * t)],
    ['OutCirc', (t) => Math.sqrt(1 - --t * t)],
    ['InOutCirc', (t) => ((t *= 2) < 1 ? 0.5 - 0.5 * Math.sqrt(1 - t * t) : 0.5 * Math.sqrt(1 - (t -= 2) * t) + 0.5)],
    ['InElastic', (t) => -Math.pow(2, -10 + 10 * t) * Math.sin((1 - elasticConst2 - t) * elasticConst) + elasticOffsetFull * (1 - t)],
    ['OutElastic', (t) => Math.pow(2, -10 * t) * Math.sin((t - elasticConst2) * elasticConst) + 1 - elasticOffsetFull * t],
    ['OutElasticHalf', (t) => Math.pow(2, -10 * t) * Math.sin((0.5 * t - elasticConst2) * elasticConst) + 1 - elasticOffsetHalf * t],
    ['OutElasticQuarter', (t) => Math.pow(2, -10 * t) * Math.sin((0.25 * t - elasticConst2) * elasticConst) + 1 - elasticOffsetQuarter * t],
    ['InOutElastic', (t) => ((t *= 2) < 1
        ? -0.5 * (Math.pow(2, -10 + 10 * t) * Math.sin((1 - elasticConst2 * 1.5 - t) * elasticConst / 1.5) - inOutElasticOffset * (1 - t))
        : 0.5 * (Math.pow(2, -10 * --t) * Math.sin((t - elasticConst2 * 1.5) * elasticConst / 1.5) - inOutElasticOffset * t) + 1)],
    ['InBack', (t) => t * t * ((backConst + 1) * t - backConst)],
    ['OutBack', (t) => --t * t * ((backConst + 1) * t + backConst) + 1],
    ['InOutBack', (t) => ((t *= 2) < 1
        ? 0.5 * t * t * ((backConst2 + 1) * t - backConst2)
        : 0.5 * ((t -= 2) * t * ((backConst2 + 1) * t + backConst2) + 2))],
    ['InBounce', (t) => 1 - outBounce(1 - t)],
    ['OutBounce', outBounce],
    ['InOutBounce', (t) => (t < 0.5 ? 0.5 - 0.5 * outBounce(1 - t * 2) : outBounce((t - 0.5) * 2) * 0.5 + 0.5)],
];

/** Display names as the osu! wiki lists them, e.g. "Quad Out". */
export const easingLabels = [
    'Linear', 'Easing Out', 'Easing In', 'Quad In', 'Quad Out', 'Quad In/Out', 'Cubic In', 'Cubic Out', 'Cubic In/Out',
    'Quart In', 'Quart Out', 'Quart In/Out', 'Quint In', 'Quint Out', 'Quint In/Out', 'Sine In', 'Sine Out', 'Sine In/Out',
    'Expo In', 'Expo Out', 'Expo In/Out', 'Circ In', 'Circ Out', 'Circ In/Out', 'Elastic In', 'Elastic Out',
    'ElasticHalf Out', 'ElasticQuarter Out', 'Elastic In/Out', 'Back In', 'Back Out', 'Back In/Out', 'Bounce In',
    'Bounce Out', 'Bounce In/Out',
];

/** Applies easing number `easing` (0-34) to progress `t` (0-1). Unknown numbers fall back to linear. */
export function ease(easing, t) {
    const entry = easings[easing];
    return entry ? entry[1](t) : t;
}

/** Easing number for a name such as "OutQuad" or "out-quad" (case-insensitive), or -1. */
export function easingByName(name) {
    const wanted = String(name).toLowerCase().replace(/[^a-z]/g, '');
    if (wanted === 'none') return 0; // storybrew's name for linear (OsbEasing.None)
    return easings.findIndex(([n]) => n.toLowerCase() === wanted);
}
