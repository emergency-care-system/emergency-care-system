<script lang="ts">
	// Panel for tests-with-given-when-then-features/21-user-authentication.feature ("User Authentication").
	//
	// This feature's own subject is the login flow itself (see
	// src/routes/login/+page.svelte and its Selenium test, which drives
	// /login directly for almost every scenario). This panel is a simple
	// "Security & Access" summary shown once a session already exists: it
	// reports who is currently signed in and gives a short, honest
	// description of this demo's mock authentication model.
	import { sessionState } from '#lib/stores/session.svelte.js';
	import { userDirectory } from '#lib/data/directory.js';
</script>

<div class="card" data-testid="user-authentication-panel">
	<h1 class="panel-heading">User Authentication</h1>
	<p class="panel-subtitle">
		Security and access summary for the currently signed-in session.
	</p>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Current Session</h2>
		{#if sessionState.current}
			<p>
				Signed in as <strong data-testid="current-identity">{sessionState.current.identity}</strong>
			</p>
			<p>
				Role: <strong data-testid="current-role">{sessionState.current.role}</strong>
			</p>
		{:else}
			<p>No active session.</p>
		{/if}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">How sign-in works in this demo</h2>
		<p>
			This is a fictitious, client-only authentication flow — there is no real backend. The
			sign-in page accepts any free-text identity or role phrase (such as "a registration
			clerk" or "Dr. Smith") unconditionally, which is how most feature Backgrounds across
			this app log in.
		</p>
		<p>
			A small mock user directory demonstrates real password-check and account-lockout
			behavior: entering a wrong password for a directory user shows
			"Invalid credentials. Please check your username and password." along with a
			countdown of attempts remaining, and a third consecutive failure locks the account with
			the message "Account temporarily locked due to multiple failed login attempts. Contact
			IT Security or wait 30 minutes."
		</p>
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Mock User Directory</h2>
		<table>
			<thead>
				<tr>
					<th>Username</th>
					<th>Display Name</th>
					<th>Badge Number</th>
				</tr>
			</thead>
			<tbody>
				{#each userDirectory as user (user.username)}
					<tr data-testid="directory-user-row">
						<td>{user.username}</td>
						<td>{user.displayName}</td>
						<td>{user.badgeNumber}</td>
					</tr>
				{/each}
			</tbody>
		</table>
	</div>
</div>
