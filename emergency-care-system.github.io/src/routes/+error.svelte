<script lang="ts">
	import { page } from '$app/state';
	import Seo from '#lib/components/site/Seo.svelte';
</script>

<Seo
	title="Page not found — Emergency Care System"
	description="This page doesn't exist."
	path={page.url.pathname}
/>

<section class="page-hero">
	<h1>{page.status}: {page.error?.message ?? 'Page not found'}</h1>
	<p><a href="/">Back to the home page</a>.</p>
</section>
