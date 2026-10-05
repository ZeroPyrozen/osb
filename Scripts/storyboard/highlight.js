// Syntax highlighting for code blocks in lessons. Produces <span class="tok-..."> markup; the colours
// live in Styles/app.css. .osb gets a proper tokenizer; C#, JavaScript and Python a light keyword pass.

import { LAYERS, ORIGINS } from './parse.js';

const escape = (s) => s.replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]);
const span = (cls, s) => `<span class="tok-${cls}">${escape(s)}</span>`;
const NAMES = new Set([...LAYERS, ...ORIGINS, 'LoopForever', 'LoopOnce', 'Passing', 'Failing']);

function osbLine(line) {
    if (/^\s*\/\//.test(line)) return span('comment', line);
    if (/^\s*\[\w+\]\s*$/.test(line)) return span('section', line);
    if (/^\s*\$[\w]+=/.test(line)) {
        const eq = line.indexOf('=');
        return span('variable', line.slice(0, eq)) + escape('=') + osbValues(line.slice(eq + 1));
    }
    const indent = /^[ _]*/.exec(line)[0];
    const rest = line.slice(indent.length);
    const head = /^([A-Za-z]+)(?=,|$)/.exec(rest);
    if (!head) return escape(indent) + osbValues(rest);
    const word = head[1];
    const cls = indent.length === 0 ? 'object' : 'event';
    const isTrigger = word === 'T';
    return escape(indent) + span(cls, word) + osbValues(rest.slice(word.length), isTrigger);
}

function osbValues(text, triggerName = false) {
    let out = '';
    const re = /("[^"]*"|\$\w+|-?\d+(?:\.\d+)?(?:e-?\d+)?|[A-Za-z][\w/.\\]*|,|\s+|.)/g;
    let match;
    let field = 0;
    while ((match = re.exec(text))) {
        const t = match[0];
        if (t === ',') { out += escape(t); field++; continue; }
        if (t.startsWith('"')) out += span('string', t);
        else if (t.startsWith('$')) out += span('variable', t);
        else if (/^-?\d/.test(t)) out += span('number', t);
        else if (NAMES.has(t) || (triggerName && field === 1) || /^HitSound/.test(t)) out += span('keyword', t);
        else if (/[./\\]/.test(t)) out += span('string', t);
        else out += escape(t);
    }
    return out;
}

const C_KEYWORDS = /\b(var|let|const|for|foreach|in|of|if|else|while|return|new|class|public|private|override|void|int|float|double|string|bool|true|false|null|using|namespace|function|this|import|from|def|range|and|or|not|None|True|False|static|readonly|await|async)\b/;

function genericLine(line, commentToken) {
    const commentAt = line.indexOf(commentToken);
    const code = commentAt >= 0 ? line.slice(0, commentAt) : line;
    const comment = commentAt >= 0 ? line.slice(commentAt) : '';
    let out = '';
    const re = /("(?:[^"\\]|\\.)*"|'(?:[^'\\]|\\.)*'|`[^`]*`|\b\d+(?:\.\d+)?f?\b|\b[A-Za-z_]\w*\b|\s+|.)/g;
    let match;
    while ((match = re.exec(code))) {
        const t = match[0];
        if (/^["'`]/.test(t)) out += span('string', t);
        else if (/^\d/.test(t)) out += span('number', t);
        else if (C_KEYWORDS.test(t) && t.match(C_KEYWORDS)[0] === t) out += span('keyword', t);
        else if (/^[A-Z]\w*$/.test(t)) out += span('object', t);
        else out += escape(t);
    }
    return out + (comment ? span('comment', comment) : '');
}

/** Highlights code in a language ("osb", "csharp", "js", "python"); unknown languages are just escaped. */
export function highlight(code, language) {
    const lines = code.replace(/\n$/, '').split('\n');
    switch (language) {
        case 'osb': return lines.map(osbLine).join('\n');
        case 'csharp': case 'cs': case 'js': case 'javascript': return lines.map((l) => genericLine(l, '//')).join('\n');
        case 'python': case 'py': return lines.map((l) => genericLine(l, '#')).join('\n');
        default: return lines.map(escape).join('\n');
    }
}
