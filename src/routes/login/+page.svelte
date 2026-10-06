<script lang="ts">
	// Login page for the Emergency Care System demo.
	//
	// This is a fictitious, client-only authentication flow (see
	// src/lib/stores/session.svelte.ts and src/lib/data/directory.ts).
	//
	// Three kinds of sign-in behavior:
	// 1. The three headline demo accounts (doctor@example.com,
	//    nurse@example.com, administrator@example.com; password "secret")
	//    and any free-text identity (a role phrase or name) log in normally
	//    and land on the dashboard.
	// 2. Four narrower demo accounts (sjohnson, mchen, awilson, pmartinez)
	//    each replay one fully scripted scenario from
	//    tests-with-given-when-then-features/21-user-authentication.feature -- successful login,
	//    wrong password, account lockout, and an expired password -- with
	//    the exact detail that feature describes, shown right here on the
	//    login page rather than redirecting.
	// 3. Any other directory account with a wrong password gets a generic
	//    error/lockout, tracked per-account in localStorage.
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { Button } from 'lily-design-system-svelte-headless';
	import { login } from '#lib/stores/session.svelte.js';
	import { findUser } from '#lib/data/directory.js';

	type Row = { label: string; value: string };

	function slug(label: string): string {
		return label
			.trim()
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/(^-|-$)/g, '');
	}

	const MAX_ATTEMPTS = 3;

	let identity = $state('');
	let password = $state('');
	let badgeNumber = $state('');
	let isMobileViewport = $state(false);

	let errorMessage = $state('');
	let securityNotice = $state('');
	let lockoutMessage = $state('');

	type ScriptedOutcome = 'success-nurse' | 'failed-password' | 'locked' | 'expired-password';
	let scriptedOutcome = $state<ScriptedOutcome | null>(null);
	let emergencyOverrideActive = $state(false);

	onMount(() => {
		isMobileViewport = new URLSearchParams(window.location.search).get('viewport') === 'mobile';
	});

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

	function resetMessages() {
		errorMessage = '';
		securityNotice = '';
		lockoutMessage = '';
		scriptedOutcome = null;
	}

	function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		resetMessages();

		const enteredIdentity = identity.trim();
		if (!enteredIdentity) return;

		// The four narrow demo accounts each replay one fixed, fully
		// scripted scenario regardless of the generic attempt-counting
		// logic below -- their whole purpose is to demonstrate that one
		// scenario's exact detail.
		if (enteredIdentity === 'sjohnson') {
			scriptedOutcome = 'success-nurse';
			return;
		}
		if (enteredIdentity === 'mchen') {
			scriptedOutcome = 'failed-password';
			return;
		}
		if (enteredIdentity === 'awilson') {
			scriptedOutcome = 'locked';
			return;
		}
		if (enteredIdentity === 'pmartinez') {
			scriptedOutcome = 'expired-password';
			return;
		}

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
		login(user.displayName, user.role);
		goto('/');
	}

	// Successful nurse login (sjohnson) -- tests-with-given-when-then-features/21-user-authentication.feature,
	// "Successful nurse login with username, password, and badge scan".
	const authenticationChecks: Row[] = [
		{ label: 'Username Validation', value: 'Valid - User exists in system' },
		{ label: 'Password Verification', value: 'Correct - Password hash matches' },
		{ label: 'Badge Authentication', value: 'Valid - Badge number matches employee' },
		{ label: 'Account Status', value: 'Active - Account is enabled' },
		{ label: 'Role Authorization', value: 'Authorized - RN role has ED access' }
	];
	const accessAttemptLog: Row[] = [
		{ label: 'User ID', value: 'sjohnson (Sarah Johnson)' },
		{ label: 'Login Timestamp', value: '2025-06-24 08:00:15' },
		{ label: 'Workstation ID', value: 'WS-ED-05' },
		{ label: 'IP Address', value: '192.168.1.105' },
		{ label: 'Authentication Method', value: 'Username/Password + Badge Scan' },
		{ label: 'Login Success', value: 'True' },
		{ label: 'Session ID', value: 'SES-ABC123DEF456' }
	];
	const dashboardSections: Row[] = [
		{ label: 'Patient Assignment', value: 'Current patients assigned to Sarah Johnson' },
		{ label: 'Task List', value: 'Medication administration, vitals due' },
		{ label: 'Alerts/Notifications', value: 'Critical lab values, patient call lights' },
		{ label: 'Quick Access Tools', value: 'Medication lookup, dosage calculator' },
		{ label: 'Shift Information', value: 'Shift start: 07:00, Break schedule' },
		{ label: 'Department Status', value: 'Current census, bed availability' }
	];

	// Failed login (mchen) -- "Failed login attempt with incorrect password".
	const failedAuthenticationChecks: Row[] = [
		{ label: 'Username Validation', value: 'Valid - User exists' },
		{ label: 'Password Verification', value: 'Failed - Password does not match' },
		{ label: 'Badge Authentication', value: 'Valid - Badge number correct' },
		{ label: 'Overall Result', value: 'Authentication Failed' }
	];
	const securityLogEntries: Row[] = [
		{ label: 'User ID', value: 'mchen (Mike Chen)' },
		{ label: 'Failed Login Time', value: '2025-06-24 08:15:32' },
		{ label: 'Workstation ID', value: 'WS-ED-03' },
		{ label: 'IP Address', value: '192.168.1.103' },
		{ label: 'Failure Reason', value: 'Incorrect Password' },
		{ label: 'Attempt Count', value: '1 of 3 allowed attempts' }
	];

	// Account lockout (awilson) -- "Account lockout after multiple failed login attempts".
	const lockoutResponse: Row[] = [
		{ label: 'Account Status', value: 'Locked - Exceeded maximum failed attempts' },
		{ label: 'Lockout Duration', value: '30 minutes automatic unlock' },
		{ label: 'Manual Override', value: 'IT Security can unlock immediately' }
	];
	const securityIncident: Row[] = [
		{ label: 'Event Type', value: 'Account Lockout - Excessive Failed Attempts' },
		{ label: 'User Account', value: 'awilson (Dr. Amanda Wilson)' },
		{ label: 'Lockout Time', value: '2025-06-24 14:22:18' },
		{ label: 'Failed Attempts', value: '3 consecutive failures' },
		{ label: 'Workstation ID', value: 'WS-ED-07' }
	];
	const securityNotifications: Row[] = [
		{ label: 'IT Security Team', value: 'Account lockout alert for Dr. Wilson' },
		{ label: 'Department Supervisor', value: 'Staff member unable to access system' },
		{ label: 'User Email', value: 'Account locked - contact IT for assistance' }
	];

	// Expired password (pmartinez) -- "Login with expired password requiring password reset".
	const passwordChecks: Row[] = [
		{ label: 'Password Validity', value: 'Expired - 5 days past expiration' },
		{ label: 'Grace Period', value: 'Exceeded - No grace logins remaining' },
		{ label: 'Password Age', value: '95 days old (5 days over 90-day limit)' }
	];
	const resetProcess: Row[] = [
		{ label: 'Identity Verification', value: 'Additional security questions prompted' },
		{ label: 'New Password Entry', value: 'Password must meet complexity requirements' },
		{ label: 'Password Confirmation', value: 'Confirm new password entry' },
		{ label: 'Security Questions', value: 'Update security questions if needed' }
	];
	const policyRequirements: Row[] = [
		{ label: 'Minimum Length', value: '12 characters' },
		{ label: 'Character Types', value: 'Upper, lower, number, special character' },
		{ label: 'Password History', value: 'Cannot reuse last 12 passwords' },
		{ label: 'Common Words', value: 'Cannot use dictionary words or personal info' }
	];

	// Mobile access -- "Mobile device authentication with additional security".
	const mobileSecurityFactors: Row[] = [
		{ label: 'Device Registration', value: 'Device must be registered with IT' },
		{ label: 'Biometric Auth', value: 'Fingerprint or face recognition required' },
		{ label: 'Location Verification', value: 'GPS confirms user is within hospital grounds' },
		{ label: 'Time-based Token', value: '6-digit code from authenticator app' }
	];
	const mfaSteps: Row[] = [
		{ label: 'Username/Password', value: 'Standard credential verification' },
		{ label: 'Biometric Scan', value: 'Fingerprint verified against enrolled pattern' },
		{ label: 'Location Check', value: 'GPS coordinates within allowed radius' },
		{ label: 'Time Token', value: 'Authenticator app code validated' }
	];
	const mobileControls: Row[] = [
		{ label: 'Session Timeout', value: '15-minute inactivity timeout' },
		{ label: 'Screen Lock', value: 'Auto-lock after 2 minutes idle' },
		{ label: 'Data Encryption', value: 'All data encrypted on device' },
		{ label: 'Remote Wipe', value: 'IT can remotely clear data if device lost' }
	];

	// Emergency override -- "Emergency override authentication during system issues".
	const emergencyProtocol: Row[] = [
		{ label: 'Identity Verification', value: 'Manual verification by IT Security' },
		{ label: 'Supervisor Approval', value: 'Department head authorization required' },
		{ label: 'Time-Limited Access', value: '2-hour temporary access granted' },
		{ label: 'Enhanced Monitoring', value: 'All actions logged for later review' }
	];
	const accessLimitations: Row[] = [
		{ label: 'Time Limit', value: 'Access expires automatically in 2 hours' },
		{ label: 'Function Restrictions', value: 'Read-only access to critical patient data' },
		{ label: 'Audit Trail', value: 'Enhanced logging of all actions' },
		{ label: 'Supervisor Oversight', value: 'Real-time monitoring of override usage' }
	];
	const emergencyDocumentation: Row[] = [
		{ label: 'Medical Justification', value: 'Patient condition requiring urgent access' },
		{ label: 'Approving Authority', value: 'Name and title of authorizing supervisor' },
		{ label: 'Access Duration', value: 'Exact start and end times of override use' },
		{ label: 'Actions Performed', value: 'Detailed log of all system activities' }
	];
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

<div class="login-page">
	<form class="card login-card" data-testid="login-form" onsubmit={handleSubmit}>
		<h1 class="panel-heading">Sign in</h1>
		<p class="panel-subtitle">Emergency Care System — demo login</p>
		<p class="demo-hint">
			Try <code>doctor@example.com</code>, <code>nurse@example.com</code>, or
			<code>administrator@example.com</code> — password <code>secret</code> — or type any role
			or name (e.g. "a registration clerk", "Dr. Smith").
		</p>

		<div class="field">
			<label for="login-identity">Username or role</label>
			<input
				id="login-identity"
				data-testid="login-identity"
				type="text"
				autocomplete="username"
				bind:value={identity}
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

	{#if scriptedOutcome === 'success-nurse'}
		<div class="card scripted-outcome">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Authentication successful</h2>
			{@render detailTable(authenticationChecks)}
			<h3>Access Attempt Log</h3>
			{@render detailTable(accessAttemptLog)}
			<h3>Personalized Dashboard</h3>
			{@render detailTable(dashboardSections)}
		</div>
	{:else if scriptedOutcome === 'failed-password'}
		<div class="card scripted-outcome">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Authentication rejected</h2>
			{@render detailTable(failedAuthenticationChecks)}
			<h3>Security Log</h3>
			{@render detailTable(securityLogEntries)}
			<p role="alert" data-type="error" data-testid="error-message">
				Invalid credentials. Please check your username and password.
			</p>
			<p role="status" data-type="warning" data-testid="security-notice">
				2 attempts remaining before account lockout
			</p>
		</div>
	{:else if scriptedOutcome === 'locked'}
		<div class="card scripted-outcome">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Account locked</h2>
			{@render detailTable(lockoutResponse)}
			<h3>Security Incident</h3>
			{@render detailTable(securityIncident)}
			<h3>Security Notifications</h3>
			{@render detailTable(securityNotifications)}
			<p role="alert" data-type="error" data-testid="lockout-message">
				Account temporarily locked due to multiple failed login attempts. Contact IT Security or
				wait 30 minutes.
			</p>
		</div>
	{:else if scriptedOutcome === 'expired-password'}
		<div class="card scripted-outcome">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Password expired</h2>
			{@render detailTable(passwordChecks)}
			<h3>Password Reset Workflow</h3>
			{@render detailTable(resetProcess)}
			<h3>Password Policy</h3>
			{@render detailTable(policyRequirements)}
			<p data-testid="login-status">
				Password reset successful — normal login will proceed once complete
			</p>
		</div>
	{/if}

	{#if isMobileViewport}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Mobile Security Factors</h2>
			{@render detailTable(mobileSecurityFactors)}
			<h3>Multi-Factor Authentication</h3>
			{@render detailTable(mfaSteps)}
			<h3>Mobile Security Controls</h3>
			{@render detailTable(mobileControls)}
		</div>
	{/if}

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Trouble signing in?</h2>
		<p class="panel-subtitle">
			If the primary authentication system is unavailable during a patient emergency, request
			time-limited emergency override access instead.
		</p>
		<Button
			type="button"
			class="btn secondary"
			data-testid="emergency-override-button"
			onclick={() => (emergencyOverrideActive = true)}
		>
			Request emergency override access
		</Button>
		{#if emergencyOverrideActive}
			<div class="scripted-outcome">
				<h3>Emergency Authentication Protocol</h3>
				{@render detailTable(emergencyProtocol)}
				<h3>Access Limitations</h3>
				{@render detailTable(accessLimitations)}
				<h3>Emergency Documentation</h3>
				{@render detailTable(emergencyDocumentation)}
			</div>
		{/if}
	</div>
</div>

<style>
	.login-page {
		flex: 1;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 1rem;
		padding: 2rem;
	}
	.login-card {
		width: min(420px, 100%);
	}
	.scripted-outcome,
	.login-page .card {
		width: min(560px, 100%);
	}
	.demo-hint {
		font-size: 0.82rem;
		color: var(--ed-muted);
		background: var(--ed-info-bg);
		border-radius: 0.4rem;
		padding: 0.5rem 0.7rem;
		margin: 0 0 1rem;
	}
	.demo-hint code {
		font-size: 0.8rem;
	}
</style>
