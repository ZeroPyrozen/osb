// A browser page for tests of the site's page scripts, made with jsdom. The scripts use browser
// globals (document, window, localStorage...), so open() puts the new page's on globalThis.
// Page scripts run their setup when imported, so import them with fresh(): each call loads a new
// copy of the module for the page that's open now.

import { JSDOM } from 'jsdom';

const GLOBALS = ['window', 'document', 'localStorage', 'location', 'history', 'navigator', 'Event', 'CustomEvent', 'MouseEvent', 'HTMLElement', 'Element', 'Node'];

let copies = 0;

export function open(body, { reducedMotion = true, userId } = {}) {
    const dom = new JSDOM(`<!DOCTYPE html><html><body${userId ? ` data-user-id="${userId}"` : ''}>${body}</body></html>`, {
        url: 'https://osb.test/learn',
        pretendToBeVisual: true,
    });
    const { window } = dom;
    // jsdom has no layout, media queries or scrolling; these stand-ins are enough for the scripts.
    window.matchMedia = (query) => ({ media: query, matches: query.includes('prefers-reduced-motion') && reducedMotion });
    window.Element.prototype.scrollIntoView = function () { this.dataset.scrolledIntoView = 'true'; };
    window.Element.prototype.scrollTo = function ({ left }) { this.scrollLeft = left; };
    for (const name of GLOBALS) Object.defineProperty(globalThis, name, { value: name === 'window' ? window : window[name], configurable: true, writable: true });
    return window;
}

/** Imports a page script as a new module, so its setup runs again for the current page. */
export const fresh = (path) => import(`${new URL(path, import.meta.url).href}?copy=${++copies}`);

/** Waits for promises the page script started (fetches, awaited completions) to finish. */
export const settle = () => new Promise((resolve) => setTimeout(resolve, 10));
