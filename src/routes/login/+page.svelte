<script lang="ts">
	// Login page for the ED Command Center demo.
	//
	// This is a fictitious, client-only authentication flow (see
	// src/lib/stores/session.svelte.ts and src/lib/data/directory.ts). Any
	// non-empty identity is accepted unless it matches a directory username
	// with a wrong password, which demonstrates a real (mock) wrong-password
	// and account-lockout flow per spec/features/21-user-authentication.feature.
	import { goto } from '$app/navigation';
	import { Button } from 'lily-design-system-svelte-headless';
	import { login } from '$lib/stores/session.svelte';
	import { findUser } from '$lib/data/directory';

	const MAX_ATTEMPTS = 3;

	let identity = $state('');
	let password = $state('');
	let badgeNumber = $state('');

	let errorMessage = $state('');
	let securityNotice = $state('');
	let lockoutMessage = $state('');

	function attemptsKey(username: string) {
		return `ed-demo-failed-attempts:${username}`;
	}

	function getAttempts(username: string): number {
		if (typeof localStorage === 'undefined') return 0;
		return Number(localStorage.getItem(attemptsKey(username)) ?? '0');
	}

	function setAttempts(username: string, count: number) {
		if (typeof localStorage === 'undefined') return;
		localStorage.setItem(attemptsKey(username), String(count));
	}

	function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		errorMessage = '';
		securityNotice = '';
		lockoutMessage = '';

		const enteredIdentity = identity.trim();
		if (!enteredIdentity) return;

		const user = findUser(enteredIdentity);

		// Free-text identity (a role phrase like "a registration clerk", or a
		// name like "Dr. Smith") is not in the mock directory, so it always
		// succeeds -- most Background steps across the feature files use
		// free-text identities, not a specific directory username.
		if (!user) {
			login(enteredIdentity);
			goto('/');
			return;
		}

		const attempts = getAttempts(user.username);
		if (attempts >= MAX_ATTEMPTS) {
			lockoutMessage =
				'Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes.';
			return;
		}

		if (password && password !== user.password) {
			const nextAttempts = attempts + 1;
			setAttempts(user.username, nextAttempts);
			if (nextAttempts >= MAX_ATTEMPTS) {
				lockoutMessage =
					'Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes.';
			} else {
				errorMessage = 'Invalid credentials. Please check your username and password.';
				securityNotice = `${MAX_ATTEMPTS - nextAttempts} attempts remaining before account lockout`;
			}
			return;
		}

		setAttempts(user.username, 0);
		login(user.displayName);
		goto('/');
	}
</script>

<div class="login-page">
	<form class="card login-card" data-testid="login-form" onsubmit={handleSubmit}>
		<h1 class="panel-heading">Sign in</h1>
		<p class="panel-subtitle">Emergency Department Management System — demo login</p>

		<div class="field">
			<label for="login-identity">Username or role</label>
			<input
				id="login-identity"
				data-testid="login-identity"
				type="text"
				autocomplete="username"
				bind:value={identity}
				placeholder="e.g. a registration clerk, Dr. Smith, sjohnson"
			/>
		</div>

		<div class="field">
			<label for="login-password">Password</label>
			<input
				id="login-password"
				data-testid="password"
				type="password"
				autocomplete="current-password"
				bind:value={password}
			/>
		</div>

		<div class="field">
			<label for="login-badge-number">Badge number</label>
			<input
				id="login-badge-number"
				data-testid="badge-number"
				type="text"
				bind:value={badgeNumber}
			/>
		</div>

		{#if errorMessage}
			<p role="alert" data-type="error" data-testid="error-message">{errorMessage}</p>
		{/if}
		{#if securityNotice}
			<p role="status" data-type="warning" data-testid="security-notice">{securityNotice}</p>
		{/if}
		{#if lockoutMessage}
			<p role="alert" data-type="error" data-testid="lockout-message">{lockoutMessage}</p>
		{/if}

		<Button type="submit" class="btn" data-testid="login-submit">Log in</Button>
	</form>
</div>

<style>
	.login-page {
		flex: 1;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 2rem;
	}
	.login-card {
		width: min(420px, 100%);
	}
</style>
