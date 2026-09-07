<script lang="ts">
	// Panel for tests-with-given-when-then-features/05-bed-assignment.feature.
	//
	// Fictitious, client-only bed assignment recommendation engine. Each click
	// of "Request bed assignment recommendations" advances through a scripted
	// sequence of scenarios (one per Gherkin scenario in the feature file), so
	// repeated clicks across the Selenium test file's scenarios each reveal the
	// next scripted recommendation. The sequence position is persisted to
	// localStorage so it survives the full-page reload each Mocha `it` causes
	// via the login flow, but stays scoped to this feature via a unique key.
	import { Button } from 'lily-design-system-svelte-headless';

	const STORAGE_KEY = 'ed-demo-bed-assignment-scene-index';

	function loadSceneIndex(): number {
		if (typeof localStorage === 'undefined') return -1;
		const raw = localStorage.getItem(STORAGE_KEY);
		if (!raw) return -1;
		const parsed = Number(raw);
		return Number.isFinite(parsed) ? parsed : -1;
	}

	function saveSceneIndex(index: number): void {
		if (typeof localStorage === 'undefined') return;
		localStorage.setItem(STORAGE_KEY, String(index));
	}

	const SCENE_COUNT = 7;

	let sceneIndex = $state(loadSceneIndex());
	let rejected = $state(false);
	let assignmentConfirmed = $state(false);

	function requestRecommendations() {
		sceneIndex = Math.min(sceneIndex + 1, SCENE_COUNT - 1);
		saveSceneIndex(sceneIndex);
		rejected = false;
	}

	function rejectRecommendation() {
		rejected = true;
	}

	function confirmBedAssignment() {
		assignmentConfirmed = true;
	}
</script>

<div class="card" data-testid="bed-assignment-panel">
	<h1 class="panel-heading">Bed Assignment</h1>
	<p class="panel-subtitle">
		Recommend the next patient for an available bed based on acuity, room type, and privacy
		needs.
	</p>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Waiting queue</h2>
		<p>Multiple patients are waiting for beds across standard, isolation, pediatric, and trauma rooms.</p>
		<Button
			type="button"
			class="btn"
			data-testid="request-recommendations-button"
			onclick={requestRecommendations}
		>
			Request bed assignment recommendations
		</Button>
	</div>

	{#if sceneIndex === 0}
		<div class="card" data-type="info" role="status" data-testid="top-recommendation">
			Maria Gonzalez
		</div>
		<p>Recommended Patient: <strong data-testid="recommended-patient">Maria Gonzalez</strong></p>
		<p>ESI Level: <strong data-testid="esi-level">2</strong></p>
		<p>Wait Time: <strong data-testid="wait-time">45 minutes</strong></p>
		<p>Room Match: <strong data-testid="room-match">Standard room suitable</strong></p>
		<p>Rationale: <strong data-testid="rationale">Highest acuity patient requiring standard bed</strong></p>
		<h3 class="panel-heading" style="font-size: 0.95rem;">Updated wait times for remaining patients</h3>
		<ul>
			{#each ['Peter Kim — Next for standard (30-45 minutes)', 'James Brown — 2nd in queue (45-60 minutes)', 'Susan Davis — 3rd in queue (90-120 minutes)'] as row (row)}
				<li data-testid="wait-time-update-row">{row}</li>
			{/each}
		</ul>
	{:else if sceneIndex === 1}
		<div class="card" data-type="info" role="status" data-testid="top-recommendation">
			Alex Johnson
		</div>
		<p>Recommended Patient: <strong data-testid="recommended-patient">Alex Johnson</strong></p>
		<p>Room Type: <strong data-testid="room-type">Isolation room</strong></p>
		<p>Rationale: <strong data-testid="rationale">Patient requires isolation precautions</strong></p>
		<p>Infection Control: <strong data-testid="infection-control">Airborne precautions needed</strong></p>
		<p role="status" data-type="warning" data-testid="room-type-restriction-notice">
			Other waiting patients cannot use this isolation room.
		</p>
		<p>Standard room wait time change: <strong data-testid="standard-room-wait-time-change">Unchanged</strong></p>
	{:else if sceneIndex === 2}
		<div class="card" data-type="info" role="status" data-testid="top-recommendation">
			Sarah Mitchell
		</div>
		<p>Recommended Patient: <strong data-testid="recommended-patient">Sarah Mitchell</strong></p>
		<p>Age: <strong data-testid="age">4 years old</strong></p>
		<p>ESI Level: <strong data-testid="esi-level">2</strong></p>
		<p>Room Type: <strong data-testid="room-type">Pediatric room</strong></p>
		<p>Rationale: <strong data-testid="rationale">Highest acuity pediatric patient</strong></p>
		<h3 class="panel-heading" style="font-size: 0.95rem;">Updated wait times for remaining pediatric patients</h3>
		<ul>
			{#each ['Tommy Anderson — Next for pediatric room (20-30 minutes)', 'Emily Foster — 2nd in pediatric queue (60-90 minutes)'] as row (row)}
				<li data-testid="wait-time-update-row">{row}</li>
			{/each}
		</ul>
		<p data-testid="david-chen-queue-status">David Chen remains in the adult standard room queue</p>
	{:else if sceneIndex === 3}
		<div class="card" data-type="warning" role="alert" data-testid="no-suitable-patients-message">
			No suitable patients for trauma room
		</div>
		<p>Alternative Use: <strong data-testid="alternative-use">Consider using for high acuity standard patients</strong></p>
		<p>Room Conversion: <strong data-testid="room-conversion">Can be downgraded to standard room if needed</strong></p>
		<p>Hold for Emergency: <strong data-testid="hold-for-emergency">Keep available for incoming trauma cases</strong></p>
		<p>Trauma Room Status: <strong data-testid="trauma-room-status">Available</strong></p>
	{:else if sceneIndex === 4}
		<div class="card" data-type="info" role="status" data-testid="top-recommendation">
			Carol Davis
		</div>
		<h3 class="panel-heading" style="font-size: 0.95rem;">Multiple recommendations</h3>
		<table>
			<thead>
				<tr><th>Room Number</th><th>Recommended Patient</th><th>ESI Level</th><th>Rationale</th></tr>
			</thead>
			<tbody>
				{#each [['ED-TRAUMA-2', 'Carol Davis', '1', 'Critical patient, trauma room'], ['ED-ISO-1', 'Helen Garcia', '2', 'Isolation requirements'], ['ED-15', 'Frank Miller', '2', 'Highest acuity for standard']] as row (row[0])}
					<tr data-testid="bed-recommendation-row">
						<td>{row[0]}</td><td>{row[1]}</td><td>{row[2]}</td><td>{row[3]}</td>
					</tr>
				{/each}
			</tbody>
		</table>
		<p role="status" data-type="info" data-testid="wait-times-updated-notice">
			Wait times updated for all remaining patients.
		</p>
		<p>Ranking: <strong data-testid="recommendation-ranking-order">Ranked by patient acuity priority</strong></p>
	{:else if sceneIndex === 5}
		<div class="card" data-type="info" role="status" data-testid="top-recommendation">
			Anthony Clark
		</div>
		<p>Recommended Patient: <strong data-testid="recommended-patient">Anthony Clark</strong></p>
		<p>Privacy Consideration: <strong data-testid="privacy-consideration">Same gender as adjacent patient</strong></p>
		<p>Alternative Option: <strong data-testid="alternative-option">Michelle Lee (if privacy not a concern)</strong></p>
		<Button type="button" class="btn secondary" data-testid="override-gender-consideration">
			Override gender consideration
		</Button>
	{:else if sceneIndex === 6}
		<div class="card" data-type="info" role="status" data-testid="top-recommendation">
			Crisis Patient A
		</div>
		<p>High Volume Alert: <strong data-testid="high-volume-alert">ED at capacity - emergency protocols active</strong></p>
		<p>Room Flex Option: <strong data-testid="room-flex-option">Standard room can accommodate ESI Level 1</strong></p>
		<p>Resource Alert: <strong data-testid="resource-alert">Additional equipment may be needed</strong></p>
		<p role="status" data-type="warning" data-testid="trauma-room-availability-suggestion">
			The system suggests moving lower acuity patients to make trauma rooms available.
		</p>
	{/if}

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Recommendation review</h2>
		<p>The system recommends John Smith for room ED-10 (ESI Level 2, chest pain).</p>
		<Button
			type="button"
			class="btn secondary"
			data-testid="reject-recommendation-button"
			onclick={rejectRecommendation}
		>
			Reject recommendation
		</Button>
		{#if rejected}
			<h3 class="panel-heading" style="font-size: 0.95rem;">Alternative recommendations</h3>
			<ul>
				{#each [['Lisa Brown', '2', 'Next highest acuity, standard care'], ['Mike Johnson', '3', 'Suitable for standard room']] as row (row[0])}
					<li data-testid="alternative-recommendation-row">{row[0]} — ESI {row[1]} — {row[2]}</li>
				{/each}
			</ul>
			<h3 class="panel-heading" style="font-size: 0.95rem;">Alternative rooms for John Smith</h3>
			<ul>
				{#each [['ED-CCU-1', 'Cardiac Monitor', 'Available now'], ['ED-CCU-2', 'Cardiac Monitor', 'Available 15min']] as row (row[0])}
					<li data-testid="alternative-room-row">{row[0]} — {row[1]} — {row[2]}</li>
				{/each}
			</ul>
			<Button type="button" class="btn" data-testid="select-alternative-patient">
				Select alternative patient
			</Button>
		{/if}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Confirm bed assignment</h2>
		<p>Patient A (ESI Level 2, current wait 30 minutes) is ready to be assigned to the available room.</p>
		<Button
			type="button"
			class="btn"
			data-testid="confirm-bed-assignment-button"
			onclick={confirmBedAssignment}
		>
			Confirm bed assignment
		</Button>
		{#if assignmentConfirmed}
			<h3 class="panel-heading" style="font-size: 0.95rem;">Recalculated wait times</h3>
			<ul>
				{#each ['Patient B — 45 minutes (reduced 15 min)', 'Patient C — 60 minutes (reduced 15 min)', 'Patient D — 105 minutes (reduced 15 min)'] as row (row)}
					<li data-testid="wait-time-update-row">{row}</li>
				{/each}
			</ul>
			<p role="status" data-type="info" data-testid="patient-tracking-board">
				Updated wait times are displayed on the patient tracking board.
			</p>
			<p data-testid="family-notification-status">
				Family members notified of updated wait time estimates via the patient portal.
			</p>
		{/if}
	</div>
</div>
