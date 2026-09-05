<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import type { Component } from 'svelte';
	import { sessionState, restoreSession } from '$lib/stores/session.svelte';
	import { features } from '$lib/features/registry';

	let activeSlug = $state<string | null>(null);
	let ActivePanel = $state<Component | null>(null);
	let loading = $state(false);
	let restored = $state(false);

	onMount(() => {
		restoreSession();
		restored = true;
	});

	$effect(() => {
		if (restored && !sessionState.current) {
			goto('/login');
		}
	});

	async function selectFeature(slug: string) {
		activeSlug = slug;
		loading = true;
		const feature = features.find((entry) => entry.slug === slug);
		if (feature) {
			const module = await feature.loadPanel();
			ActivePanel = module.default;
		}
		loading = false;
	}
</script>

{#if sessionState.current}
	<nav class="sidebar" data-testid="feature-nav" aria-label="ED features">
		<button
			type="button"
			class="nav-item"
			class:active={activeSlug === null}
			data-testid="nav-dashboard-overview"
			onclick={() => {
				activeSlug = null;
				ActivePanel = null;
			}}
		>
			Overview
		</button>
		{#each features as feature (feature.slug)}
			<button
				type="button"
				class="nav-item"
				class:active={activeSlug === feature.slug}
				data-testid={`nav-${feature.slug}`}
				onclick={() => selectFeature(feature.slug)}
			>
				{feature.number}. {feature.title}
			</button>
		{/each}
	</nav>

	<section class="content" data-testid="feature-content">
		{#if activeSlug === null}
			<div class="card" data-testid="dashboard-overview">
				<h1 class="panel-heading">Welcome back, {sessionState.current.identity}</h1>
				<p class="panel-subtitle">
					Role: {sessionState.current.role} — pick a feature from the left to get started.
				</p>
				<p>
					This is a demonstration Emergency Department management system. All data shown
					throughout the app is fictitious and exists only in this browser session.
				</p>
			</div>
		{:else if loading}
			<p>Loading…</p>
		{:else if ActivePanel}
			<ActivePanel />
		{/if}
	</section>
{/if}

<style>
	.sidebar {
		width: 280px;
		flex-shrink: 0;
		background: var(--ed-surface);
		border-right: 1px solid var(--ed-border);
		display: flex;
		flex-direction: column;
		padding: 0.75rem;
		gap: 0.15rem;
		overflow-y: auto;
	}
	.nav-item {
		text-align: left;
		background: transparent;
		border: none;
		border-radius: 0.4rem;
		padding: 0.55rem 0.7rem;
		font-size: 0.88rem;
		color: var(--ed-text);
	}
	.nav-item:hover {
		background: var(--ed-info-bg);
	}
	.nav-item.active {
		background: var(--ed-primary);
		color: white;
		font-weight: 600;
	}
	.content {
		flex: 1;
		padding: 1.5rem;
		overflow-y: auto;
	}
</style>
