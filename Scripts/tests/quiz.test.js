// Tests for knowledge checks (Scripts/learn/quiz.js), on markup shaped like Views/Learn/Unit.cshtml.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { open, settle } from './dom.js';
import { mountQuiz } from '../learn/quiz.js';

const question = (n) => `
    <fieldset data-question>
        <div>
            <label><input type="radio" name="q${n}" value="0"> First</label>
            <label><input type="radio" name="q${n}" value="1"> Second</label>
            <label><input type="radio" name="q${n}" value="2"> Third</label>
        </div>
        <div data-explanation hidden><p>Correct or not.</p><div data-explanation-text>Why ${n}.</div></div>
    </fieldset>`;

/** Keeps the authored order: Fisher–Yates with random() = 0.999 swaps each choice with itself. */
const keepOrder = () => 0.999;

/**
 * Two questions; the answers are "Second" (1), then "First" (0). Choices keep their authored order
 * unless a random function is given.
 */
function page({ pass = 2, done = [], random = keepOrder, firstSaved } = {}) {
    open(`
        <form data-quiz data-unit-id="m1/check" data-pass="${pass}">
            <script type="application/json" data-quiz-key>[1,0]</script>
            ${question(0)}${question(1)}
            <button type="submit">Check answers</button>
            <button type="button" data-quiz-retry hidden>Try again</button>
            <div data-quiz-result></div>
        </form>`);
    // Each page has its own storage, like a visit on another day after the earlier one was saved.
    if (firstSaved != null) localStorage.setItem('osb.quiz.m1/check', String(firstSaved));
    const form = document.querySelector('form');
    const progress = {
        completed: [],
        done: new Set(done),
        isDone(unitId) { return this.done.has(unitId); },
        async complete(unitId, result) { this.completed.push({ unitId, ...result }); this.done.add(unitId); return { xpGained: 25 }; },
    };
    mountQuiz(form, progress, { random });
    const answer = (q, value) => { form.querySelectorAll('[data-question]')[q].querySelector(`input[value="${value}"]`).checked = true; };
    const submit = async () => { form.requestSubmit(); await settle(); };
    return { form, progress, answer, submit, result: () => form.querySelector('[data-quiz-result]').textContent.replace(/\s+/g, ' ').trim() };
}

const retryButton = (form) => form.querySelector('[data-quiz-retry]');
const shownAnswers = (form) => [...form.querySelectorAll('label[data-answer]')].map((l) => l.textContent.trim());
const choiceOrder = (form, q) => [...form.querySelectorAll('[data-question]')[q].querySelectorAll('label')].map((l) => l.textContent.trim());
const explanationShown = (form, q) => {
    const fieldset = form.querySelectorAll('[data-question]')[q];
    return !fieldset.querySelector('[data-explanation]').hidden && !fieldset.querySelector('[data-explanation-text]').hidden;
};

test('unanswered questions are pointed out, and nothing is recorded', async () => {
    const { form, progress, answer, submit, result } = page();
    answer(0, 1);

    await submit();

    assert.equal(result(), 'Answer every question first.');
    assert.equal(form.querySelectorAll('[data-question]')[1].dataset.scrolledIntoView, 'true');
    assert.deepEqual(progress.completed, []);
});

test('choices are shuffled on every attempt, and graded by the choice rather than its position', async () => {
    // Fisher–Yates with random() = 0 moves the first choice to the end each time.
    const { form, progress, answer, submit } = page({ random: () => 0 });
    assert.deepEqual(choiceOrder(form, 0), ['Second', 'Third', 'First']);
    assert.ok(form.querySelector('[data-question]').lastElementChild.hasAttribute('data-explanation'), 'the explanation stays after the choices');

    answer(0, 0);
    answer(1, 1);
    await submit();
    retryButton(form).click();
    assert.deepEqual(choiceOrder(form, 0), ['Third', 'First', 'Second'], 'a retry shuffles again');

    answer(0, 1);
    answer(1, 0);
    await submit();
    assert.equal(progress.completed[0].score, 2);
});

test('a failed check marks each question but keeps the answers hidden', async () => {
    const { form, progress, answer, submit, result } = page();
    answer(0, 0);
    answer(1, 0);

    await submit();

    const [first, second] = form.querySelectorAll('[data-question]');
    assert.equal(first.dataset.state, 'wrong');
    assert.equal(second.dataset.state, 'correct');
    assert.deepEqual(shownAnswers(form), []);
    assert.equal(first.querySelector('[data-explanation]').hidden, false, '"Not quite" shows');
    assert.equal(explanationShown(form, 0), false, "the wrong answer's explanation would give the answer away");
    assert.equal(explanationShown(form, 1), true);
    assert.ok([...form.querySelectorAll('input')].every((i) => i.disabled));
    assert.equal(retryButton(form).hidden, false);
    assert.match(result(), /^1 of 2 correct You need 2 to pass\..*The answers are shown once you pass\.$/);
    assert.deepEqual(progress.completed, []);
});

/** A retry used to leave the right answers highlighted, so the second try gave them away. */
test('a retry hides the marks and explanations again', async () => {
    const { form, answer, submit, result } = page({ pass: 1 });
    answer(0, 0);
    answer(1, 0);
    await submit();
    assert.deepEqual(shownAnswers(form), ['Second', 'First'], 'a pass shows the answers');

    retryButton(form).click();

    assert.deepEqual(shownAnswers(form), []);
    assert.ok([...form.querySelectorAll('[data-question]')].every((q) => !q.dataset.state));
    assert.ok([...form.querySelectorAll('input')].every((i) => !i.checked && !i.disabled));
    assert.ok([...form.querySelectorAll('[data-explanation]')].every((e) => e.hidden));
    assert.equal(form.querySelector('[type="submit"]').hidden, false);
    assert.equal(retryButton(form).hidden, true);
    assert.equal(result(), '');
});

test('a perfect first attempt records the unit, announces the XP and needs no retry', async () => {
    const { form, progress, answer, submit, result } = page();
    let completedEvent = false;
    document.addEventListener('unit-complete', () => { completedEvent = true; });
    answer(0, 1);
    answer(1, 0);

    await submit();

    assert.deepEqual(progress.completed, [{ unitId: 'm1/check', score: 2, max: 2, first: 2 }]);
    assert.match(result(), /^2 of 2 correct \(perfect!\) You earned 25 XP\.$/);
    assert.deepEqual(shownAnswers(form), ['Second', 'First']);
    assert.ok([0, 1].every((q) => explanationShown(form, q)));
    assert.equal(retryButton(form).hidden, true);
    assert.equal(completedEvent, true);
});

test('passing without a perfect score keeps the retry, for a better score', async () => {
    const { form, progress, answer, submit, result } = page({ pass: 1 });
    answer(0, 1);
    answer(1, 1);

    await submit();

    assert.deepEqual(progress.completed, [{ unitId: 'm1/check', score: 1, max: 2, first: 1 }]);
    assert.match(result(), /Your best score is kept if you try again\.$/);
    assert.ok([0, 1].every((q) => explanationShown(form, q)), 'a pass explains every question');
    assert.equal(retryButton(form).hidden, false);
});

test('after a failed attempt, a perfect retry passes but is not a first attempt', async () => {
    const { form, progress, answer, submit, result } = page();
    answer(0, 0);
    answer(1, 0);
    await submit();
    retryButton(form).click();

    answer(0, 1);
    answer(1, 0);
    await submit();

    assert.deepEqual(progress.completed, [{ unitId: 'm1/check', score: 2, max: 2, first: 1 }]);
    assert.match(result(), /Bonus XP is only for a perfect first attempt\.$/);
});

test('a failed first attempt is saved, so coming back later is not a first attempt', async () => {
    const before = page();
    before.answer(0, 0);
    before.answer(1, 0);
    await before.submit();
    assert.equal(localStorage.getItem('osb.quiz.m1/check'), '1');

    const revisit = page({ firstSaved: 1 });
    revisit.answer(0, 1);
    revisit.answer(1, 0);
    await revisit.submit();

    assert.deepEqual(revisit.progress.completed, [{ unitId: 'm1/check', score: 2, max: 2, first: 1 }]);
    assert.match(revisit.result(), /Bonus XP is only for a perfect first attempt\.$/);
});

test('retaking a finished check leaves its first attempt to the saved completion', async () => {
    const { progress, answer, submit } = page({ done: ['m1/check'] });
    answer(0, 1);
    answer(1, 0);

    await submit();

    assert.deepEqual(progress.completed, [{ unitId: 'm1/check', score: 2, max: 2, first: undefined }]);
    assert.equal(localStorage.getItem('osb.quiz.m1/check'), null);
});
