// Site-wide behaviour: nav bar, popover menus, the home slideshow, video facades and the
// community role filter. Everything here is progressive enhancement: pages work without it.

const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

// The Future design's floating nav bar becomes full width once the page scrolls.
const nav = document.querySelector('[data-nav]');
if (nav) {
    const update = () => nav.toggleAttribute('data-scrolled', window.scrollY > 12);
    update();
    window.addEventListener('scroll', update, { passive: true });
}

// Popover menus open in the top layer; place each one under the button that opened it.
for (const popover of document.querySelectorAll('[popover].menu')) {
    popover.addEventListener('beforetoggle', (event) => {
        if (event.newState !== 'open') return;
        const invoker = document.querySelector(`[popovertarget="${popover.id}"]`);
        if (!invoker) return;
        const rect = invoker.getBoundingClientRect();
        popover.style.top = `${rect.bottom + 8}px`;
        popover.style.right = `${Math.max(8, window.innerWidth - rect.right)}px`;
    });
}

// Slideshow: a CSS scroll-snap track, so swiping works natively; buttons, dots and autoplay on top.
for (const carousel of document.querySelectorAll('[data-carousel]')) {
    const track = carousel.querySelector('[data-carousel-track]');
    const slides = [...carousel.querySelectorAll('[data-carousel-slide]')];
    const dots = [...carousel.querySelectorAll('[data-carousel-dot]')];
    if (!track || slides.length === 0) continue;

    let index = 0;
    const markCurrent = () => dots.forEach((dot, i) => dot.setAttribute('aria-current', String(i === index)));
    const go = (i) => {
        index = (i + slides.length) % slides.length;
        track.scrollTo({ left: index * track.clientWidth, behavior: reduceMotion ? 'auto' : 'smooth' });
        markCurrent();
    };

    // Keep the dots right when people swipe instead of using the buttons.
    track.addEventListener('scroll', () => {
        const visible = Math.round(track.scrollLeft / track.clientWidth);
        if (visible !== index && visible >= 0 && visible < slides.length) {
            index = visible;
            markCurrent();
        }
    }, { passive: true });

    carousel.querySelector('[data-carousel-prev]')?.addEventListener('click', () => go(index - 1));
    carousel.querySelector('[data-carousel-next]')?.addEventListener('click', () => go(index + 1));
    dots.forEach((dot, i) => dot.addEventListener('click', () => go(i)));
    markCurrent();

    // Autoplay every 7 seconds, paused while hovered or focused, and never with reduced motion.
    if (!reduceMotion && slides.length > 1) {
        let paused = false;
        carousel.addEventListener('pointerenter', () => { paused = true; });
        carousel.addEventListener('pointerleave', () => { paused = false; });
        carousel.addEventListener('focusin', () => { paused = true; });
        carousel.addEventListener('focusout', () => { paused = false; });
        setInterval(() => { if (!paused && !document.hidden) go(index + 1); }, 7000);
    }
}

// Video facades: swap the thumbnail for the privacy-enhanced YouTube player on click.
document.addEventListener('click', (event) => {
    const button = event.target.closest('[data-youtube]');
    if (!button) return;
    const iframe = document.createElement('iframe');
    iframe.src = `https://www.youtube-nocookie.com/embed/${encodeURIComponent(button.dataset.youtube)}?autoplay=1&rel=0`;
    iframe.title = button.dataset.title || 'YouTube video';
    iframe.allow = 'accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; fullscreen';
    iframe.allowFullscreen = true;
    iframe.className = 'absolute inset-0 size-full';
    button.replaceWith(iframe);
    iframe.focus();
});

// Community page: show only members with the chosen role.
const roleFilters = document.querySelector('[data-role-filters]');
const members = document.querySelector('[data-members]');
if (roleFilters && members) {
    roleFilters.addEventListener('click', (event) => {
        const chip = event.target.closest('[data-role-filter]');
        if (!chip) return;
        const role = chip.dataset.roleFilter;
        for (const other of roleFilters.querySelectorAll('[data-role-filter]'))
            other.setAttribute('aria-pressed', String(other === chip));
        for (const item of members.children)
            item.hidden = role !== '' && !item.dataset.roles.split('|').includes(role);
    });
}
