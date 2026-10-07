// Tests for knowledge checks (Scripts/learn/quiz.js), on markup shaped like Views/Learn/Unit.cshtml.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { open, settle } from './dom.js';
import { mountQuiz } from '../learn/quiz.js';

const question = (n) => `
    <fieldset data-question>
        <label><input type="radio" name="q${n}" value="0"> First</label>
        <label><input type="radio" name="q${n}" value="1"> Second</label>
        <div data-explanation hidden>Why.</div>
    </fieldset>`;

/** Two questions; the answers are "Second" (1), then "First" (0). */
function page({ pass = 2 } = {}) {
    open(`
        <form data-quiz data-unit-id="m1/check" data-pass="${pass}">
            <script type="application/json" data-quiz-key>[1,0]</script>
            ${question(0)}${question(1)}
            <button type="submit">Check answers</button>
            <button type="button" data-quiz-retry hidden>Try again</button>
            <div data-quiz-result></div>
        </form>`);
    const form = document.querySelector('form');
    const progress = { completed: [], async complete(unitId, result) { this.completed.push({ unitId, ...result }); return { xpGained: 25 }; } };
    mountQuiz(form, progress);
    const answer = (q, value) => { form.querySelectorAll('[data-question]')[q].querySelector(`input[value="${value}"]`).checked = true; };
    const submit = async () => { form.requestSubmit(); await settle(); };
    return { form, progress, answer, submit, result: () => form.querySelector('[data-quiz-result]').textContent.replace(/\s+/g, ' ').trim() };
}

const retryButton = (form) => form.querySelector('[data-quiz-retry]');
const shownAnswers = (form) => [...form.querySelectorAll('label[data-answer]')].map((l) => l.textContent.trim());

test('unanswered questions are pointed out, and nothing is recorded', async () => {
    const { form, progress, answer, submit, result } = page();
    answer(0, 1);

    await submit();

    assert.equal(result(), 'Answer every question first.');
    assert.equal(form.querySelectorAll('[data-question]')[1].dataset.scrolledIntoView, 'true');
    assert.deepEqual(progress.completed, []);
});

test('a failed check marks each question, shows the right answers and offers a retry', async () => {
    const { form, progress, answer, submit, result } = page();
    answer(0, 0);
    answer(1, 0);

    await submit();

    const [first, second] = form.querySelectorAll('[data-question]');
    assert.equal(first.dataset.state, 'wrong');
    assert.equal(second.dataset.state, 'correct');
    assert.deepEqual(shownAnswers(form), ['Second', 'First']);
    assert.ok([...form.querySelectorAll('[data-explanation]')].every((e) => !e.hidden));
    assert.ok([...form.querySelectorAll('input')].every((i) => i.disabled));
    assert.equal(retryButton(form).hidden, false);
    assert.match(result(), /^1 of 2 correct You need 2 to pass/);
    assert.deepEqual(progress.completed, []);
});

/** A retry used to leave the right answers highlighted, so the second try gave them away. */
test('a retry hides the right answers again', async () => {
    const { form, answer, submit, result } = page();
    answer(0, 0);
    answer(1, 0);
    await submit();

    retryButton(form).click();

    assert.deepEqual(shownAnswers(form), []);
    assert.ok([...form.querySelectorAll('[data-question]')].every((q) => !q.dataset.state));
    assert.ok([...form.querySelectorAll('input')].every((i) => !i.checked && !i.disabled));
    assert.ok([...form.querySelectorAll('[data-explanation]')].every((e) => e.hidden));
    assert.equal(form.querySelector('[type="submit"]').hidden, false);
    assert.equal(retryButton(form).hidden, true);
    assert.equal(result(), '');
});

test('a perfect score records the unit, announces the XP and needs no retry', async () => {
    const { form, progress, answer, submit, result } = page();
    let completedEvent = false;
    document.addEventListener('unit-complete', () => { completedEvent = true; });
    answer(0, 1);
    answer(1, 0);

    await submit();

    assert.deepEqual(progress.completed, [{ unitId: 'm1/check', score: 2, max: 2 }]);
    assert.match(result(), /^2 of 2 correct \(perfect!\) You earned 25 XP\.$/);
    assert.equal(retryButton(form).hidden, true);
    assert.equal(completedEvent, true);
});

test('passing without a perfect score keeps the retry, for the bonus XP', async () => {
    const { form, progress, answer, submit, result } = page({ pass: 1 });
    answer(0, 1);
    answer(1, 1);

    await submit();

    assert.deepEqual(progress.completed, [{ unitId: 'm1/check', score: 1, max: 2 }]);
    assert.match(result(), /Get every answer right for bonus XP\.$/);
    assert.equal(retryButton(form).hidden, false);
});
