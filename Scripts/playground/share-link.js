// Playground links carry the code in the URL fragment (#osb=... or #js=...), as URL-safe base64 of
// its UTF-8 bytes, so any text survives, Japanese titles included. Used by the lesson "Open in
// playground" buttons and the playground's Share button.

export const toBase64Url = (s) => btoa(String.fromCharCode(...new TextEncoder().encode(s))).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

export const fromBase64Url = (s) => new TextDecoder().decode(Uint8Array.from(atob(s.replace(/-/g, '+').replace(/_/g, '/')), (c) => c.charCodeAt(0)));
