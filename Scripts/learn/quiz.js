// Knowledge checks: single-choice questions graded in the browser. Passing records the unit with
// the score; retries keep the best score (a perfect score earns bonus XP).

export function mountQuiz(form, progress) {
    const unitId = form.dataset.unitId;
    const key = JSON.parse(form.querySelector('script[data-quiz-key]').textContent);
    const pass = Number(form.dataset.pass);
    const questions = [...form.querySelectorAll('[data-question]')];
    const result = form.querySelector('[data-quiz-result]');
    const submit = form.querySelector('[type="submit"]');
    const retry = form.querySelector('[data-quiz-retry]');

    const reset = () => {
        for (const q of questions) {
            q.removeAttribute('data-state');
            q.querySelector('[data-explanation]').hidden = true;
            for (const input of q.querySelectorAll('input')) {
                input.checked = false;
                input.disabled = false;
                // Hide the correct answer again, so the retry doesn't give it away.
                input.closest('label').removeAttribute('data-answer');
            }
        }
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

        let score = 0;
        questions.forEach((q, i) => {
            const correct = Number(answers[i]) === key[i];
            if (correct) score++;
            q.dataset.state = correct ? 'correct' : 'wrong';
            q.querySelector('[data-explanation]').hidden = false;
            for (const input of q.querySelectorAll('input')) {
                input.disabled = true;
                if (Number(input.value) === key[i]) input.closest('label').dataset.answer = 'true';
            }
        });

        submit.hidden = true;
        retry.hidden = score === questions.length;
        const passed = score >= pass;
        if (!passed) {
            result.innerHTML = `<p class="text-lg font-bold text-mint-50">${score} of ${questions.length} correct</p>
                <p class="text-sm text-mint-200">You need ${pass} to pass. Read the explanations, then try again.</p>`;
            return;
        }
        const outcome = await progress.complete(unitId, { score, max: questions.length });
        const perfect = score === questions.length;
        result.innerHTML = `<p class="text-lg font-bold text-mint-50">${score} of ${questions.length} correct${perfect ? ' (perfect!)' : ''}</p>
            <p class="text-sm text-mint-200">${outcome.xpGained > 0 ? `You earned <strong class="text-gold-300">${outcome.xpGained} XP</strong>.` : 'Passed. Your best score is kept.'}
            ${perfect ? '' : ' Get every answer right for bonus XP.'}</p>`;
        form.dispatchEvent(new CustomEvent('unit-complete', { bubbles: true }));
    });
}
