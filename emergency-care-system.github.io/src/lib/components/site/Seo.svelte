<script lang="ts">
	// Shared <svelte:head> block for every route: title, description, and
	// canonical link. `path` is the route's own trailing-slash path (e.g.
	// "/features/"), used to build the canonical URL.
	import { SITE_URL, SITE_NAME } from '#lib/site.js';

	let {
		title,
		description,
		path
	}: {
		title: string;
		description: string;
		path: string;
	} = $props();

	let url = $derived(`${SITE_URL}${path}`);
</script>

<svelte:head>
	<title>{title}</title>
	<meta name="description" content={description} />
	<link rel="canonical" href={url} />

	<meta property="og:type" content="website" />
	<meta property="og:site_name" content={SITE_NAME} />
	<meta property="og:title" content={title} />
	<meta property="og:description" content={description} />
	<meta property="og:url" content={url} />
</svelte:head>
