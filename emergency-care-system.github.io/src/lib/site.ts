// Shared site constants -- kept in one place so Seo.svelte, llms.txt/llms.json,
// and sitemap.xml/+server.ts never drift from each other.
export const SITE_URL = 'https://emergency-care-system.github.io';
export const SITE_NAME = 'Emergency Care System';
export const REPO_URL = 'https://github.com/emergency-care-system/emergency-care-system';
export const REPO_SLUG = 'emergency-care-system/emergency-care-system';
// GitHub code search scoped to the source repository (used by the header's search picker).
export const repoSearchUrl = (query: string) =>
	`https://github.com/search?type=code&q=${encodeURIComponent(`repo:${REPO_SLUG} ${query}`)}`;
// There is no hosted deployment of the demo app itself (it's a local-only
// SvelteKit dev server, per the source repo's README) -- don't invent one.
