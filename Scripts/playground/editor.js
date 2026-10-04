// The code editor for exercises and the playground, built on CodeMirror 6. .osb scripts get syntax
// colours and live diagnostics from the storyboard parser; script mode uses JavaScript highlighting.

import { EditorState, Compartment } from '@codemirror/state';
import { EditorView, keymap, lineNumbers, highlightActiveLine, highlightActiveLineGutter, drawSelection, placeholder } from '@codemirror/view';
import { defaultKeymap, history, historyKeymap, indentWithTab } from '@codemirror/commands';
import { StreamLanguage, HighlightStyle, syntaxHighlighting, bracketMatching, indentUnit } from '@codemirror/language';
import { linter, lintGutter } from '@codemirror/lint';
import { javascript } from '@codemirror/lang-javascript';
import { tags } from '@lezer/highlight';
import { parse, LAYERS, ORIGINS } from '../storyboard/parse.js';

const NAMES = new Set([...LAYERS, ...ORIGINS, 'LoopForever', 'LoopOnce', 'Passing', 'Failing']);

/** A small stream tokenizer for .osb: sections, comments, objects, commands, names, numbers, paths. */
const osbLanguage = StreamLanguage.define({
    name: 'osb',
    startState: () => ({ head: true }),
    token(stream, state) {
        if (stream.sol()) {
            state.head = true;
            if (stream.match(/^\s*\/\/.*/)) return 'comment';
            if (stream.match(/^\s*\[\w+\]\s*$/)) return 'heading';
            if (stream.match(/^\$\w+(?==)/)) return 'variableName';
            if (stream.match(/^[ _]+/)) return 'meta';
        }
        if (state.head) {
            state.head = false;
            if (stream.match(/^(Sprite|Animation|Sample)\b/)) return 'keyword';
            if (stream.match(/^(MX|MY|F|M|S|V|R|C|P|L|T)(?=,|$)/)) return 'typeName';
        }
        if (stream.match(/^"[^"]*"?/)) return 'string';
        if (stream.match(/^\$\w+/)) return 'variableName';
        if (stream.match(/^-?\d+(\.\d+)?(e-?\d+)?/)) return 'number';
        if (stream.match(/^[A-Za-z][\w/.\\]*/)) {
            const word = stream.current();
            if (NAMES.has(word) || /^HitSound/.test(word)) return 'atom';
            return /[./\\]/.test(word) ? 'string' : null;
        }
        if (stream.eat(',')) return 'punctuation';
        stream.next();
        return null;
    },
});

const highlightStyle = HighlightStyle.define([
    { tag: tags.comment, color: '#7d8f8a', fontStyle: 'italic' },
    { tag: tags.heading, color: '#8ab4f8', fontWeight: '600' },
    { tag: tags.keyword, color: '#8fcfbf', fontWeight: '600' },
    { tag: [tags.typeName, tags.className], color: '#ffe08a', fontWeight: '600' },
    { tag: tags.atom, color: '#c5a3ff' },
    { tag: [tags.number, tags.bool], color: '#f8b38a' },
    { tag: tags.string, color: '#9fd98b' },
    { tag: [tags.variableName, tags.special(tags.variableName)], color: '#e7f1ee' },
    { tag: [tags.definition(tags.variableName), tags.function(tags.variableName)], color: '#8fcfbf' },
    { tag: tags.propertyName, color: '#b8e1d7' },
    { tag: [tags.operator, tags.punctuation], color: '#90a9a3' },
    { tag: tags.meta, color: '#4f5b58' },
]);

const theme = EditorView.theme({
    '&': { backgroundColor: 'var(--color-charcoal-950)', color: 'var(--color-mint-100)', fontSize: '13.5px', height: '100%' },
    '.cm-content': { fontFamily: 'var(--font-mono)', caretColor: 'var(--color-mint-300)', padding: '10px 0' },
    '.cm-scroller': { fontFamily: 'var(--font-mono)', lineHeight: '1.6' },
    '.cm-gutters': { backgroundColor: 'var(--color-charcoal-900)', color: 'var(--color-sage)', border: 'none' },
    '.cm-activeLine': { backgroundColor: 'rgb(97 188 166 / 0.07)' },
    '.cm-activeLineGutter': { backgroundColor: 'rgb(97 188 166 / 0.12)', color: 'var(--color-mint-200)' },
    '&.cm-focused .cm-cursor': { borderLeftColor: 'var(--color-mint-300)' },
    '&.cm-focused .cm-selectionBackground, .cm-selectionBackground, ::selection': { backgroundColor: 'rgb(97 188 166 / 0.3) !important' },
    '.cm-tooltip': { backgroundColor: 'var(--color-charcoal-800)', border: '1px solid var(--color-charcoal-600)', color: 'var(--color-mint-100)' },
    '.cm-diagnostic': { fontFamily: 'var(--font-sans)', fontSize: '13px' },
    '.cm-placeholder': { color: 'var(--color-sage)' },
}, { dark: true });

/** Parser diagnostics as CodeMirror lint markers, so mistakes are underlined while typing. */
function osbLinter() {
    return linter((view) => {
        const doc = view.state.doc;
        return parse(doc.toString()).diagnostics
            .filter((d) => d.line >= 1 && d.line <= doc.lines)
            .map((d) => {
                const line = doc.line(d.line);
                return { from: line.from, to: Math.max(line.from, line.to), severity: d.severity, message: d.message };
            });
    }, { delay: 250 });
}

/**
 * Creates an editor in `parent`.
 * @param {object} options  doc, mode ('osb' | 'script'), onChange(text), label
 */
export function createEditor(parent, { doc = '', mode = 'osb', onChange, label = 'Storyboard script' } = {}) {
    const language = new Compartment();
    const languageFor = (m) => (m === 'script' ? [javascript()] : [osbLanguage, osbLinter(), lintGutter()]);

    const view = new EditorView({
        parent,
        state: EditorState.create({
            doc,
            extensions: [
                lineNumbers(),
                highlightActiveLineGutter(),
                highlightActiveLine(),
                drawSelection(),
                history(),
                bracketMatching(),
                indentUnit.of(' '),
                keymap.of([indentWithTab, ...defaultKeymap, ...historyKeymap]),
                syntaxHighlighting(highlightStyle),
                theme,
                EditorView.lineWrapping,
                placeholder(mode === 'script' ? '// Write JavaScript that builds a storyboard' : '// Write your storyboard here'),
                EditorView.contentAttributes.of({ 'aria-label': label }),
                language.of(languageFor(mode)),
                EditorView.updateListener.of((update) => {
                    if (update.docChanged) onChange?.(update.state.doc.toString());
                }),
            ],
        }),
    });

    return {
        view,
        get text() { return view.state.doc.toString(); },
        set text(value) {
            view.dispatch({ changes: { from: 0, to: view.state.doc.length, insert: value } });
        },
        setMode(m) { view.dispatch({ effects: language.reconfigure(languageFor(m)) }); },
        insertLine(line) {
            const end = view.state.doc.length;
            const prefix = end > 0 && !view.state.doc.toString().endsWith('\n') ? '\n' : '';
            view.dispatch({ changes: { from: end, insert: prefix + line + '\n' }, selection: { anchor: end + prefix.length + line.length + 1 } });
            view.focus();
        },
        /** Scrolls to and selects a line (used to jump to script errors). */
        revealLine(n) {
            if (n < 1 || n > view.state.doc.lines) return;
            const line = view.state.doc.line(n);
            view.dispatch({ selection: { anchor: line.from, head: line.to }, scrollIntoView: true });
            view.focus();
        },
        focus() { view.focus(); },
    };
}
