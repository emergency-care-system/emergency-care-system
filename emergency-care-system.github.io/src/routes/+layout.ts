// Fully static site: every route is prerendered at build time for GitHub Pages.
export const prerender = true;

// Directory + index.html per route (e.g. features/index.html), so links
// work the same whether or not a trailing slash is typed.
export const trailingSlash = 'always';
