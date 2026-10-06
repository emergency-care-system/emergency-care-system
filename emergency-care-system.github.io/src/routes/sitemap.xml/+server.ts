import { SITE_URL } from '#lib/site.js';

// The sitemap is generated from the route tree: every
// src/routes/**/+page.svelte becomes one <url>. Adding a page needs no
// change here.
//
// Routes that must NOT be listed go in UNLISTED (exact paths, with the
// trailing slash). Dynamic routes ([param]) can't be enumerated from
// file names alone, so they are skipped; if one is added, list its
// concrete paths in EXTRA_ROUTES.
const UNLISTED: string[] = [];
const EXTRA_ROUTES: string[] = [];

export const prerender = true;

// import.meta.glob only needs the file paths, so don't load the modules.
const pages = Object.keys(import.meta.glob('/src/routes/**/+page.svelte'));

function routeFor(file: string): string | null {
	const segments = file
		.replace('/src/routes', '')
		.replace(/\/\+page\.svelte$/, '')
		.split('/')
		.filter(Boolean)
		// Route groups like (marketing) don't appear in the URL.
		.filter((segment) => !/^\(.+\)$/.test(segment));
	if (segments.some((segment) => segment.includes('['))) return null;
	return segments.length ? `/${segments.join('/')}/` : '/';
}

const routes: string[] = [
	...new Set([...pages.map(routeFor).filter((r): r is string => r !== null), ...EXTRA_ROUTES])
]
	.filter((route) => !UNLISTED.includes(route))
	.sort((a, b) => (a === '/' ? -1 : b === '/' ? 1 : a.localeCompare(b)));

export function GET() {
	const body = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
${routes.map((route) => `  <url><loc>${SITE_URL}${route}</loc></url>`).join('\n')}
</urlset>
`;

	return new Response(body, {
		headers: { 'Content-Type': 'application/xml' }
	});
}
