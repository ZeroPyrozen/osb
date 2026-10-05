// Shares the learner's progress between the learn page script and the lazily loaded playground code.

let provide;
/** Resolves with the Progress instance once learn.js has loaded it. */
export const learnReady = new Promise((resolve) => { provide = resolve; });

export function provideLearn(progress) {
    provide(progress);
}
