import { SITE_URL } from '#lib/site.js';

// Every route belongs in this list, full stop -- it's about search-engine
// discovery, not on-site navigation. Add a new route here in the same
// change that adds it under src/routes/.
const routes = ['/', '/about/', '/features/', '/testing/'];

export const prerender = true;

export function GET() {
	const body = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
${routes.map((route) => `  <url><loc>${SITE_URL}${route}</loc></url>`).join('\n')}
</urlset>`;

	return new Response(body, {
		headers: { 'Content-Type': 'application/xml' }
	});
}
