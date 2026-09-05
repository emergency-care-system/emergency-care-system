<script lang="ts">
	import './layout.css';
	import { onMount } from 'svelte';
	import { Button } from 'lily-design-system-svelte-headless';
	import { sessionState, logout, restoreSession } from '$lib/stores/session.svelte';
	import { goto } from '$app/navigation';

	let { children } = $props();

	onMount(() => {
		restoreSession();
	});

	function handleLogout() {
		logout();
		goto('/login');
	}
</script>

<svelte:head>
	<title>ED Command Center</title>
</svelte:head>

<div data-testid="app-root" class="app-shell">
	<header class="top-bar">
		<a href="/" class="brand">🏥 ED Command Center</a>
		{#if sessionState.current}
			<div class="session-info">
				<span data-testid="current-identity">{sessionState.current.identity}</span>
				<span class="role-badge" data-testid="current-role">{sessionState.current.role}</span>
				<Button type="button" data-testid="logout-button" onclick={handleLogout}>Log out</Button>
			</div>
		{/if}
	</header>
	<main class="app-main">
		{@render children()}
	</main>
</div>
