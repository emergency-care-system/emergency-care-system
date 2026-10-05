<script lang="ts">
	import '../lib/styles/theme.css';
	import SiteNav from '#lib/components/site/SiteNav.svelte';
	import FooterNav from '#lib/components/site/FooterNav.svelte';

	let { children } = $props();
</script>

<SiteNav />

<main class="site-shell">
	{@render children()}
</main>

<FooterNav />
