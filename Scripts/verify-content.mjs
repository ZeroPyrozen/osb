// Checks the learn content as the site serves it: every exercise's starter must fail its checks and its
// solution must pass them, every .osb example in a unit must parse cleanly and only use images from the
// playground library, and every script example with a "Run in playground" button must run and do the
// same. It also warns, without failing, about units whose facts were last checked more than three months
// ago. Run it against a running site:
//
//   npm run verify:content                    (http://localhost:5000, the dotnet run default)
//   npm run verify:content -- http://127.0.0.1:5094

import { parse } from './storyboard/parse.js';
import { runChecks } from './storyboard/checks.js';
import { runScript, isRunnableScript } from './storyboard/script.js';
import { Assets, framePath } from './storyboard/assets.js';

const base = (process.argv[2] ?? 'http://localhost:5000').replace(/\/$/, '');
const library = new Assets();
const problems = [];
let exercises = 0;
let examples = 0;
let scripts = 0;
let dated = 0;
const warnings = [];

// Units dated with reviewed: get their facts re-checked every quarter. An older date is a warning, not a failure.
const reviewDue = (() => {
    const date = new Date();
    date.setMonth(date.getMonth() - 3);
    return date.toISOString().slice(0, 10);
})();

const unescape = (html) => html
    .replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&#39;/g, "'").replace(/&amp;/g, '&');

async function page(path) {
    const response = await fetch(base + path);
    if (!response.ok) throw new Error(`GET ${path} returned ${response.status}`);
    return response.text();
}

function jsonScript(html, attribute) {
    const match = new RegExp(`<script type="application/json" ${attribute}>([\\s\\S]*?)</script>`).exec(html);
    return match ? JSON.parse(match[1]) : null;
}

/** Errors and warnings in a script, plus images that the playground can't show. */
function lint(text) {
    const model = parse(text);
    const found = model.diagnostics
        .filter((d) => d.severity !== 'info')
        .map((d) => `line ${d.line}: ${d.severity}: ${d.message}`);
    for (const object of model.objects) {
        const paths = object.kind === 'animation'
            ? Array.from({ length: object.frameCount }, (_, i) => framePath(object.path, i))
            : [object.path];
        for (const path of paths)
            if (!library.has(path)) found.push(`line ${object.line}: "${path}" isn't in the playground library`);
    }
    return found;
}

function toOsb(config, which) {
    const text = config[which] ?? '';
    if (config.mode !== 'script') return text;
    try {
        return runScript(text).osb;
    } catch (error) {
        if (which === 'starter') return '';
        throw new Error(`the ${which} script throws: ${error.message}`);
    }
}

function verifyExercise(unit, config) {
    exercises++;
    const solution = toOsb(config, 'solution');
    const starter = toOsb(config, 'starter');
    const report = (message) => problems.push(`${unit.url}: ${message}`);

    for (const issue of lint(solution)) report(`solution ${issue}`);

    const solved = runChecks(config.checks, solution, { solutionText: solution });
    for (const result of solved.results.filter((r) => !r.pass))
        report(`the solution fails "${result.label}"${result.message ? ` (${result.message})` : ''}`);

    const started = runChecks(config.checks, starter, { solutionText: solution });
    if (started.passed) report('the starter already passes every check');

    if (config.hints?.length === 0) report('the hints list is empty');
}

const catalogPage = await page('/learn');
const catalog = jsonScript(catalogPage, 'id="learn-catalog"');
if (!catalog) throw new Error('No learn catalog on /learn. Is this the osb site?');

for (const unit of catalog.units) {
    let html;
    try {
        html = await page(unit.url);
    } catch (error) {
        problems.push(`${unit.url}: ${error.message}`);
        continue;
    }

    if (unit.type === 'exercise') {
        const config = jsonScript(html, 'data-exercise-config');
        if (!config) problems.push(`${unit.url}: no exercise configuration on the page`);
        else {
            try {
                verifyExercise(unit, config);
            } catch (error) {
                problems.push(`${unit.url}: ${error.message}`);
            }
        }
    }

    for (const match of html.matchAll(/<code class="language-osb(?:-static)?">([\s\S]*?)<\/code>/g)) {
        const text = unescape(match[1]);
        if (!/^(Sprite|Animation|Sample)\b/m.test(text)) continue; // a syntax fragment, not a storyboard
        examples++;
        for (const issue of lint(text)) problems.push(`${unit.url}: example "${text.split('\n')[0].slice(0, 50)}" ${issue}`);
    }

    for (const match of html.matchAll(/<code class="language-js">([\s\S]*?)<\/code>/g)) {
        const text = unescape(match[1]);
        if (!isRunnableScript(text)) continue; // no "Run in playground" button: a fragment, not a whole script
        scripts++;
        const where = `${unit.url}: script "${text.split('\n')[0].slice(0, 50)}"`;
        try {
            for (const issue of lint(runScript(text).osb)) problems.push(`${where} ${issue}`);
        } catch (error) {
            problems.push(`${where} throws: ${error.message}`);
        }
    }

    const reviewed = /data-reviewed="(\d{4}-\d{2}-\d{2})"/.exec(html)?.[1];
    if (reviewed) {
        dated++;
        if (reviewed < reviewDue)
            warnings.push(`${unit.url}: facts last checked on ${reviewed}, more than three months ago. Check them against the current ranking criteria and osu!(lazer), then update reviewed:.`);
    }
}

const plural = (n, word) => `${n} ${word}${n === 1 ? '' : 's'}`;
console.log(`Checked ${catalog.units.length} units (${dated} with a review date): ${exercises} exercises, ${examples} .osb examples and ${scripts} script examples.`);
if (warnings.length) {
    console.log(`\n${plural(warnings.length, 'warning')}:`);
    for (const warning of warnings) console.log(`  - ${warning}`);
}
if (problems.length) {
    console.log(`\n${plural(problems.length, 'problem')}:`);
    for (const problem of problems) console.log(`  - ${problem}`);
    process.exitCode = 1;
} else {
    console.log(warnings.length ? '\nNo problems. The warnings above don\'t fail the check.' : 'Everything checks out.');
}
