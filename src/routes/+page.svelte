<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import type { Component } from 'svelte';
	import { sessionState, restoreSession } from '$lib/stores/session.svelte';
	import { features } from '$lib/features/registry';

	type Row = { label: string; value: string };

	function slug(label: string): string {
		return label
			.trim()
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/(^-|-$)/g, '');
	}

	// Role-personalized Overview content -- spec/features/21-user-authentication.feature,
	// "Role-based dashboard customization after successful login".
	const physicianDashboard: Row[] = [
		{ label: 'Patient Queue', value: 'Patients waiting to be seen by priority' },
		{ label: 'Active Orders', value: 'Lab results, imaging pending review' },
		{ label: 'Critical Alerts', value: 'Abnormal vitals, critical lab values' },
		{ label: 'Decision Support', value: 'Clinical guidelines, drug interactions' },
		{ label: 'Documentation Tools', value: 'Templates for common conditions' }
	];
	const chargeNurseDashboard: Row[] = [
		{ label: 'Department Overview', value: 'Bed status, staff assignments, census' },
		{ label: 'Resource Management', value: 'Equipment status, supply levels' },
		{ label: 'Staff Coordination', value: 'Break schedules, assignments, coverage' },
		{ label: 'Quality Metrics', value: 'Wait times, patient satisfaction, safety' },
		{ label: 'Administrative Tasks', value: 'Reporting, scheduling, policy updates' }
	];
	const technicalDashboard: Row[] = [
		{ label: 'System Status', value: 'Server health, network connectivity' },
		{ label: 'User Management', value: 'Account status, permission changes' },
		{ label: 'Audit Logs', value: 'System access, security events' },
		{ label: 'Maintenance Tools', value: 'Backup status, system updates' }
	];

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

{#snippet detailTable(rows: Row[])}
	<table>
		<tbody>
			{#each rows as row (row.label)}
				<tr>
					<th>{row.label}</th>
					<td data-testid={slug(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
{/snippet}

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
					This is a demonstration emergency care system. All data shown
					throughout the app is fictitious and exists only in this browser session.
				</p>

				{#if sessionState.current.role === 'Physician'}
					<h2 class="panel-heading" style="font-size: 1.05rem;">Physician Dashboard</h2>
					{@render detailTable(physicianDashboard)}
				{:else if sessionState.current.role === 'Charge Nurse'}
					<h2 class="panel-heading" style="font-size: 1.05rem;">Charge Nurse Dashboard</h2>
					{@render detailTable(chargeNurseDashboard)}
				{:else if sessionState.current.role === 'Technical Support'}
					<h2 class="panel-heading" style="font-size: 1.05rem;">Technical Dashboard</h2>
					{@render detailTable(technicalDashboard)}
				{/if}
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
