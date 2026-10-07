// Tests for Scripts/site.js: the nav bar, popover menus, the home slideshow, video facades and the
// community role filter.

import { test, mock } from 'node:test';
import assert from 'node:assert/strict';
import { open, fresh } from './dom.js';

const slideshow = `
    <section data-carousel>
        <div data-carousel-track>
            <article data-carousel-slide></article><article data-carousel-slide></article><article data-carousel-slide></article>
        </div>
        <button data-carousel-prev></button>
        <button data-carousel-dot="0"></button><button data-carousel-dot="1"></button><button data-carousel-dot="2"></button>
        <button data-carousel-next></button>
    </section>`;

/** Index of the slide whose dot is marked current. */
const currentDot = () => [...document.querySelectorAll('[data-carousel-dot]')].findIndex((d) => d.getAttribute('aria-current') === 'true');
const click = (selector) => document.querySelector(selector).click();

async function home(options) {
    open(slideshow, options);
    Object.defineProperty(document.querySelector('[data-carousel-track]'), 'clientWidth', { value: 400 });
    await fresh('../site.js');
}

test('the slideshow buttons and dots move between slides, wrapping around', async () => {
    await home();
    assert.equal(currentDot(), 0);

    click('[data-carousel-next]');
    assert.equal(currentDot(), 1);
    assert.equal(document.querySelector('[data-carousel-track]').scrollLeft, 400);

    click('[data-carousel-prev]');
    click('[data-carousel-prev]');
    assert.equal(currentDot(), 2, 'back from the first slide goes to the last');

    click('[data-carousel-dot="0"]');
    assert.equal(currentDot(), 0);
});

test('the dots follow a swipe', async () => {
    await home();
    const track = document.querySelector('[data-carousel-track]');

    track.scrollLeft = 800;
    track.dispatchEvent(new Event('scroll'));

    assert.equal(currentDot(), 2);
});

test('the slideshow moves on by itself every 7 seconds, except while hovered', async (t) => {
    t.after(() => mock.timers.reset());
    mock.timers.enable({ apis: ['setInterval'] });
    await home({ reducedMotion: false });
    const carousel = document.querySelector('[data-carousel]');

    mock.timers.tick(7000);
    assert.equal(currentDot(), 1);

    carousel.dispatchEvent(new Event('pointerenter'));
    mock.timers.tick(7000);
    assert.equal(currentDot(), 1);

    carousel.dispatchEvent(new Event('pointerleave'));
    mock.timers.tick(7000);
    assert.equal(currentDot(), 2);
});

test('with reduced motion there is no autoplay', async (t) => {
    t.after(() => mock.timers.reset());
    mock.timers.enable({ apis: ['setInterval'] });
    await home({ reducedMotion: true });

    mock.timers.tick(21000);

    assert.equal(currentDot(), 0);
});

test('a video thumbnail turns into the privacy-friendly YouTube player when clicked', async () => {
    open(`<div><button data-youtube="abc_DEF-123" data-title="xi - Blue Zenith">Play</button></div>
          <div><button data-youtube="x?list=1&amp;v=2">Play</button></div>`);
    await fresh('../site.js');

    const [first, second] = document.querySelectorAll('[data-youtube]');
    first.click();
    second.click();

    const [player, other] = document.querySelectorAll('iframe');
    assert.equal(player.src, 'https://www.youtube-nocookie.com/embed/abc_DEF-123?autoplay=1&rel=0');
    assert.equal(player.title, 'xi - Blue Zenith');
    assert.equal(other.src, 'https://www.youtube-nocookie.com/embed/x%3Flist%3D1%26v%3D2?autoplay=1&rel=0');
    assert.equal(other.title, 'YouTube video');
    assert.equal(document.querySelectorAll('[data-youtube]').length, 0);
});

test('the community role filter shows only members with the chosen role', async () => {
    open(`
        <div data-role-filters>
            <button data-role-filter="" aria-pressed="true">Everyone</button>
            <button data-role-filter="Mentor" aria-pressed="false">Mentor</button>
        </div>
        <ul data-members>
            <li data-roles="Mentor|Verified">Alice</li><li data-roles="Storyboarder">Bob</li><li data-roles="">Carol</li>
        </ul>`);
    await fresh('../site.js');
    const visible = () => [...document.querySelectorAll('[data-members] li')].filter((li) => !li.hidden).map((li) => li.textContent);

    click('[data-role-filter="Mentor"]');
    assert.deepEqual(visible(), ['Alice']);
    assert.equal(document.querySelector('[data-role-filter="Mentor"]').getAttribute('aria-pressed'), 'true');
    assert.equal(document.querySelector('[data-role-filter=""]').getAttribute('aria-pressed'), 'false');

    click('[data-role-filter=""]');
    assert.deepEqual(visible(), ['Alice', 'Bob', 'Carol']);
});

test('the nav bar goes full width once the page scrolls', async () => {
    open('<header data-nav></header>');
    await fresh('../site.js');
    const nav = document.querySelector('[data-nav]');
    assert.ok(!nav.hasAttribute('data-scrolled'));

    Object.defineProperty(window, 'scrollY', { value: 100, configurable: true });
    window.dispatchEvent(new Event('scroll'));

    assert.ok(nav.hasAttribute('data-scrolled'));
});

test('a menu opens right under the button that opened it', async () => {
    open('<button popovertarget="user-menu">Me</button><div id="user-menu" popover class="menu"></div>');
    document.querySelector('[popovertarget]').getBoundingClientRect = () => ({ bottom: 50, right: 900 });
    await fresh('../site.js');
    const menu = document.getElementById('user-menu');

    menu.dispatchEvent(Object.assign(new Event('beforetoggle'), { newState: 'open' }));

    assert.equal(menu.style.top, '58px');
    assert.equal(menu.style.right, `${window.innerWidth - 900}px`);
});
