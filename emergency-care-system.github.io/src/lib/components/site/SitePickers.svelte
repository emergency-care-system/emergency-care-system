<script lang="ts">
	// SitePickers -- the header's row of controls, built from Lily's
	// PickerBar (search, theme, locale, text size, and share in one
	// component) with this site's own configuration.
	//
	// * Search: this site is static and has no search index, so the search
	//   picker sends the query to GitHub code search on the source
	//   repository, where the features and test suites actually live.
	// * Theme: the five stylesheets in static/themes/ (see themes.ts), not
	//   Lily's 45-theme catalog. The picker manages the
	//   <link data-lily-theme-picker="theme"> declared in app.html and
	//   persists the choice under "ecs-theme", which app.html's inline
	//   script reads before first paint.
	// * Locale: the site is English only, so theme.css hides the locale
	//   picker; PickerBar always renders one.
	// * Text size: Lily's default seven-step scale; the picker sets
	//   data-text-size on <html> and theme.css maps each slug to
	//   --user-font-scale. Persists under "ecs-text-size".
	// * Share: native share sheet where the platform has one, otherwise
	//   the targets below plus copy-to-clipboard. The package ships no
	//   social-network URLs by design, so every destination is listed here.
	import PickerBar from '@lilydesignsystem/svelte-picker-bar';
	import type { ShareTarget } from '@lilydesignsystem/svelte-share-picker';
	import { themes, DEFAULT_THEME_ID } from '#lib/data/themes.js';
	import { SITE_NAME, repoSearchUrl } from '#lib/site.js';

	// The search picker hands over "/?<encoded query>"; send the query to
	// GitHub code search instead of a page on this static site.
	function searchRepository(href: string) {
		window.location.assign(repoSearchUrl(decodeURIComponent(href.replace(/^[^?]*\?/, ''))));
	}

	const themeSlugs = themes.map((t) => t.id);
	const themeLabels = Object.fromEntries(themes.map((t) => [t.id, t.label]));

	const shareTargets: ShareTarget[] = [
		{
			id: 'email',
			label: 'Share on Email',
			href: (url, title) =>
				`mailto:?subject=${encodeURIComponent(title)}&body=${encodeURIComponent(url)}`,
			newTab: false
		},
		{
			id: 'linkedin',
			label: 'Share on LinkedIn',
			href: (url) => `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(url)}`
		},
		{
			id: 'bluesky',
			label: 'Share on Bluesky',
			href: (url, title) =>
				`https://bsky.app/intent/compose?text=${encodeURIComponent(`${title} ${url}`)}`
		},
		{
			id: 'reddit',
			label: 'Share on Reddit',
			href: (url, title) =>
				`https://www.reddit.com/submit?url=${encodeURIComponent(url)}&title=${encodeURIComponent(title)}`
		},
		{
			id: 'mastodon',
			label: 'Share on Mastodon',
			href: (url, title) =>
				`https://mastodonshare.com/?text=${encodeURIComponent(title)}&url=${encodeURIComponent(url)}`
		}
	];
</script>

<PickerBar
	labels={{
		search: 'Search the source repository on GitHub',
		searchInput: 'Search terms',
		searchSubmit: 'Search',
		theme: 'Colour theme',
		locale: 'Language',
		textSize: 'Text size',
		share: 'Share'
	}}
	searchProps={{
		placeholder: 'Search the source…',
		navigate: searchRepository
	}}
	themesUrl="/themes/"
	themes={themeSlugs}
	themeProps={{ themeLabels, storageKey: 'ecs-theme', defaultValue: DEFAULT_THEME_ID, name: 'theme' }}
	locales={['en']}
	textSizeProps={{ storageKey: 'ecs-text-size', name: 'text-size' }}
	{shareTargets}
	shareProps={{
		title: SITE_NAME,
		copyLabel: 'Copy link',
		copiedLabel: 'Link copied to clipboard',
		copyFailedLabel: 'Could not copy — copy it from the address bar'
	}}
/>
