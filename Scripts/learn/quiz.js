// Knowledge checks: single-choice questions graded in the browser. Choices are shuffled on every
// attempt and graded by the choice picked, not its position. A failed attempt marks which questions
// were wrong without giving the answers away; a pass shows every answer and explanation.
// Passing records the unit with the best score and the score of the learner's first attempt, the one
// made before any answers were shown: only a perfect first attempt earns the bonus XP.

const firstKey = (unitId) => `osb.quiz.${unitId}`;

function loadFirst(unitId) {
    try {
        const saved = localStorage.getItem(firstKey(unitId));
        return saved == null ? null : Number(saved);
    } catch { return null; }
}

function saveFirst(unitId, score) {
    try { localStorage.setItem(firstKey(unitId), String(score)); } catch { /* private mode: remembered for this page only */ }
}

/** Puts a question's choices in a new random order, in the same place in the page. */
function shuffleChoices(question, random) {
    const labels = [...question.querySelectorAll('label')];
    if (labels.length < 2) return;
    const parent = labels[0].parentNode;
    const after = labels.at(-1).nextSibling;
    for (let i = labels.length - 1; i > 0; i--) {
        const j = Math.floor(random() * (i + 1));
        [labels[i], labels[j]] = [labels[j], labels[i]];
    }
    for (const label of labels) parent.insertBefore(label, after);
}

export function mountQuiz(form, progress, { random = Math.random } = {}) {
    const unitId = form.dataset.unitId;
    const key = JSON.parse(form.querySelector('script[data-quiz-key]').textContent);
    const pass = Number(form.dataset.pass);
    const questions = [...form.querySelectorAll('[data-question]')];
    const result = form.querySelector('[data-quiz-result]');
    const submit = form.querySelector('[type="submit"]');
    const retry = form.querySelector('[data-quiz-retry]');

    // Remembered between visits, so leaving the page after a failed attempt doesn't make the next one
    // a "first" attempt again. Once the unit is done, its completion keeps the first score instead.
    let first = loadFirst(unitId);

    const shuffle = () => { for (const q of questions) shuffleChoices(q, random); };
    shuffle();

    const reset = () => {
        for (const q of questions) {
            q.removeAttribute('data-state');
            q.querySelector('[data-explanation]').hidden = true;
            q.querySelector('[data-explanation-text]').hidden = false;
            for (const input of q.querySelectorAll('input')) {
                input.checked = false;
                input.disabled = false;
                // Hide the correct answer again, so the retry doesn't give it away.
                input.closest('label').removeAttribute('data-answer');
            }
        }
        shuffle();
        result.innerHTML = '';
        submit.hidden = false;
        retry.hidden = true;
        questions[0]?.querySelector('input')?.focus();
    };

    retry.addEventListener('click', reset);

    form.addEventListener('submit', async (event) => {
        event.preventDefault();
        const answers = questions.map((q) => q.querySelector('input:checked')?.value);
        const missing = answers.findIndex((a) => a == null);
        if (missing >= 0) {
            result.innerHTML = '<p class="text-gold-200">Answer every question first.</p>';
            questions[missing].scrollIntoView({ behavior: 'smooth', block: 'center' });
            return;
        }

        const right = answers.map((a, i) => Number(a) === key[i]);
        const score = right.filter(Boolean).length;
        const passed = score >= pass;
        const firstAttempt = first == null && !progress.isDone(unitId);
        if (firstAttempt) {
            first = score;
            saveFirst(unitId, score);
        }

        questions.forEach((q, i) => {
            q.dataset.state = right[i] ? 'correct' : 'wrong';
            q.querySelector('[data-explanation]').hidden = false;
            // A wrong answer's explanation gives the right one away, so it waits for a pass.
            q.querySelector('[data-explanation-text]').hidden = !passed && !right[i];
            for (const input of q.querySelectorAll('input')) {
                input.disabled = true;
                if (passed && Number(input.value) === key[i]) input.closest('label').dataset.answer = 'true';
            }
        });

        submit.hidden = true;
        retry.hidden = score === questions.length;
        if (!passed) {
            result.innerHTML = `<p class="text-lg font-bold text-mint-50">${score} of ${questions.length} correct</p>
                <p class="text-sm text-mint-200">You need ${pass} to pass. Go back over the lessons for the questions marked "Not quite", then try again. The answers are shown once you pass.</p>`;
            return;
        }
        const outcome = await progress.complete(unitId, { score, max: questions.length, first: first ?? undefined });
        const perfect = score === questions.length;
        const note = perfect
            ? (firstAttempt ? '' : ' Bonus XP is only for a perfect first attempt.')
            : ' Your best score is kept if you try again.';
        result.innerHTML = `<p class="text-lg font-bold text-mint-50">${score} of ${questions.length} correct${perfect ? ' (perfect!)' : ''}</p>
            <p class="text-sm text-mint-200">${outcome.xpGained > 0 ? `You earned <strong class="text-gold-300">${outcome.xpGained} XP</strong>.` : 'Passed.'}${note}</p>`;
        form.dispatchEvent(new CustomEvent('unit-complete', { bubbles: true }));
    });
}
