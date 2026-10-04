// Toasts for XP, level-ups and badges.

const escape = (s) => String(s).replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]);
const sparkles = '<svg class="size-5 shrink-0 text-gold-300" viewBox="0 0 24 24" aria-hidden="true"><path d="M10 3.5c.6 3.6 2.4 5.4 6 6-3.6.6-5.4 2.4-6 6-.6-3.6-2.4-5.4-6-6 3.6-.6 5.4-2.4 6-6ZM17.5 14c.3 1.8 1.2 2.7 3 3-1.8.3-2.7 1.2-3 3-.3-1.8-1.2-2.7-3-3 1.8-.3 2.7-1.2 3-3Z" fill="currentColor"/></svg>';
const trophy = '<svg class="size-5 shrink-0 text-gold-300" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 4.5h8v5a4 4 0 0 1-8 0v-5ZM8 6.5H5a3 3 0 0 0 3 3.5M16 6.5h3a3 3 0 0 1-3 3.5M12 13.5V17m-3.5 2.5h7M9.5 17h5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/></svg>';

/** Shows a short message at the bottom of the screen. `html` must already be escaped. */
export function toast(html, { duration = 4500, big = false } = {}) {
    const host = document.getElementById('toasts');
    if (!host) return;
    const el = document.createElement('div');
    el.className = `pointer-events-auto flex animate-pop items-center gap-3 rounded-card border border-gold-400/40 bg-charcoal-800 px-4 py-3 text-sm text-mint-50 shadow-card ${big ? 'max-w-md' : 'max-w-sm'}`;
    el.setAttribute('role', 'status');
    el.innerHTML = html;
    host.append(el);
    setTimeout(() => {
        el.style.transition = 'opacity 0.4s, transform 0.4s';
        el.style.opacity = '0';
        el.style.transform = 'translateY(8px)';
        setTimeout(() => el.remove(), 450);
    }, duration);
}

/** Announces what a completion earned. */
export function celebrate(diff) {
    if (diff.xpGained > 0) toast(`${sparkles}<span><strong class="text-gold-300">+${diff.xpGained} XP</strong></span>`, { duration: 3000 });
    if (diff.levelUp) {
        toast(`<img src="/images/mascot/hifumi-shy.webp" alt="" class="h-14 w-auto">
            <span><span class="block text-xs font-bold tracking-wider text-gold-300 uppercase">Level up!</span>
            <span class="block font-display text-lg font-bold">Level ${diff.levelUp.level}: ${escape(diff.levelUp.title)}</span></span>`, { duration: 6000, big: true });
    }
    for (const badge of diff.newBadges ?? []) {
        const label = badge.kind === 'trophy' ? 'Trophy earned' : badge.kind === 'module' ? 'Badge earned' : 'Achievement unlocked';
        toast(`${trophy}<span><span class="block text-xs font-bold tracking-wider text-gold-300 uppercase">${label}</span><span class="font-semibold">${escape(badge.name)}</span></span>`, { duration: 6000 });
    }
}
