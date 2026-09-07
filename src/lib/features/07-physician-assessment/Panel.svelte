<script lang="ts">
	// Panel for tests-with-given-when-then-features/07-physician-assessment.feature.
	//
	// Fictitious, client-only mobile patient chart viewer for physicians. Each
	// click of "Open patient chart" advances through a scripted sequence of
	// patients (one per Gherkin scenario in the feature file), persisted to
	// localStorage so the sequence survives the full-page reload each Mocha
	// `it` causes via the login flow.
	import { Button } from 'lily-design-system-svelte-headless';

	const STORAGE_KEY = 'ed-demo-physician-assessment-scene-index';

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

	const SCENE_COUNT = 8;

	let sceneIndex = $state(loadSceneIndex());

	function openPatientChart() {
		sceneIndex = Math.min(sceneIndex + 1, SCENE_COUNT - 1);
		saveSceneIndex(sceneIndex);
	}
</script>

<div class="card" data-testid="physician-assessment-panel">
	<h1 class="panel-heading">Physician Assessment</h1>
	<p class="panel-subtitle">
		Access comprehensive patient information on a mobile device to make informed clinical
		decisions based on current patient data.
	</p>

	<Button type="button" class="btn" data-testid="open-patient-chart-button" onclick={openPatientChart}>
		Open patient chart
	</Button>

	{#if sceneIndex === 0}
		<!-- Jennifer Martinez, ED-8 — complete nursing assessment -->
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Patient summary</h2>
			<p>Patient Identity: <strong data-testid="patient-identity">Jennifer Martinez, DOB: 1975-03-15</strong></p>
			<p>Bed Assignment: <strong data-testid="bed-assignment">ED-8</strong></p>
			<p>Arrival Time: <strong data-testid="arrival-time">14:00</strong></p>
			<p>Triage Notes: <strong data-testid="triage-notes">ESI Level 2 - Severe chest pain, onset 2h ago</strong></p>
		</div>
		<div class="card">
			<h3 class="panel-heading" style="font-size: 0.95rem;">Vital signs</h3>
			<table>
				<thead><tr><th>Vital Sign</th><th>Value</th><th>Timestamp</th><th>Trend</th></tr></thead>
				<tbody>
					<tr><td>Blood Pressure</td><td data-testid="blood-pressure">160/95</td><td>14:20</td><td data-testid="blood-pressure-trend">High</td></tr>
					<tr><td>Heart Rate</td><td data-testid="heart-rate">110</td><td>14:20</td><td data-testid="heart-rate-trend">Elevated</td></tr>
					<tr><td>Respiratory Rate</td><td data-testid="respiratory-rate">22</td><td>14:20</td><td data-testid="respiratory-rate-trend">Elevated</td></tr>
					<tr><td>Temperature</td><td data-testid="temperature">98.6°F</td><td>14:20</td><td data-testid="temperature-trend">Normal</td></tr>
					<tr><td>Oxygen Saturation</td><td data-testid="oxygen-saturation">94%</td><td>14:20</td><td data-testid="oxygen-saturation-trend">Low</td></tr>
					<tr><td>Pain Score</td><td data-testid="pain-score">8/10</td><td>14:20</td><td data-testid="pain-score-trend">Severe</td></tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h3 class="panel-heading" style="font-size: 0.95rem;">Allergies</h3>
			<table>
				<thead><tr><th>Allergy</th><th>Reaction Type</th><th>Severity</th></tr></thead>
				<tbody>
					<tr><td>Penicillin</td><td data-testid="penicillin-reaction">Rash</td><td data-testid="penicillin-severity">Moderate</td></tr>
					<tr><td>Shellfish</td><td data-testid="shellfish-reaction">Unknown</td><td data-testid="shellfish-severity">Unknown</td></tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h3 class="panel-heading" style="font-size: 0.95rem;">Current medications</h3>
			<table>
				<thead><tr><th>Medication</th><th>Dosage</th><th>Frequency</th><th>Status</th></tr></thead>
				<tbody>
					<tr><td>Metoprolol</td><td data-testid="metoprolol-dosage">50mg</td><td>BID</td><td data-testid="metoprolol-status">Active</td></tr>
					<tr><td>Aspirin</td><td data-testid="aspirin-dosage">81mg</td><td>Daily</td><td data-testid="aspirin-status">Active</td></tr>
				</tbody>
			</table>
		</div>
	{:else if sceneIndex === 1}
		<!-- Michael Chen, ED-3 — active treatment -->
		<div class="card">
			<p>Current Status: <strong data-testid="current-status">Active treatment in progress</strong></p>
			<p>Most Recent Vitals: <strong data-testid="most-recent-vitals">BP: 130/80, HR: 88, T: 100.2°F (14:15)</strong></p>
			<p>Active Orders: <strong data-testid="active-orders">Lab work in progress</strong></p>
			<p>Triage Summary: <strong data-testid="triage-summary">ESI 3 - Abd pain, onset 6h ago</strong></p>
			<p data-testid="data-freshness-timestamp">All data timestamped — last updated 14:15</p>
			<p data-type="error" data-testid="critical-value-highlight">Critical value: Temperature 100.2°F</p>
			<p>Lab Result Status: <strong data-testid="lab-result-status">In Progress</strong></p>
		</div>
	{:else if sceneIndex === 2}
		<!-- Robert Johnson, ED-12 — allergies and interactions -->
		<div class="card">
			<p data-type="error" role="alert">
				Critical Alert: <strong data-testid="critical-alert">SEVERE ALLERGIES: Morphine, NSAIDs</strong>
			</p>
			<p data-type="warning" role="alert">
				Warning: <strong data-testid="warning">Moderate allergy: Codeine</strong>
			</p>
		</div>
		<div class="card">
			<table>
				<thead><tr><th>Medication</th><th>Status</th><th>Interaction Alerts</th></tr></thead>
				<tbody>
					<tr><td>Warfarin</td><td data-testid="warfarin-status">Active</td><td data-testid="warfarin-interaction-alerts">Monitor for bleeding risk</td></tr>
					<tr><td>Metformin</td><td data-testid="metformin-status">Active</td><td data-testid="metformin-interaction-alerts">No interactions detected</td></tr>
				</tbody>
			</table>
			<p data-testid="allergy-checking-notice">Any new medication orders will trigger allergy checking.</p>
			<p data-testid="interaction-warning">Interaction warnings are displayed for contraindicated drugs.</p>
		</div>
	{:else if sceneIndex === 3}
		<!-- Emma Foster, ED-PEDS-2, age 7 — pediatric chart -->
		<div class="card">
			<p>Patient Age/Weight: <strong data-testid="patient-age-weight">7 years old, 22 kg</strong></p>
			<p>Pediatric Vital Ranges: <strong data-testid="pediatric-vital-ranges">All vitals with age-appropriate norms</strong></p>
			<p>Growth Percentiles: <strong data-testid="growth-percentiles">Weight: 50th percentile</strong></p>
			<p>Guardian Information: <strong data-testid="guardian-information">Sarah Foster (mother) - present</strong></p>
		</div>
		<div class="card">
			<table>
				<thead><tr><th>Vital Sign</th><th>Value</th><th>Normal Range (Age 7)</th><th>Status</th></tr></thead>
				<tbody>
					<tr><td>Blood Pressure</td><td data-testid="blood-pressure">95/60</td><td>90-110/55-70</td><td data-testid="blood-pressure-status">Normal</td></tr>
					<tr><td>Heart Rate</td><td data-testid="heart-rate">110</td><td>80-120</td><td data-testid="heart-rate-status">Normal</td></tr>
					<tr><td>Respiratory</td><td data-testid="respiratory">24</td><td>18-25</td><td data-testid="respiratory-status">Normal</td></tr>
					<tr><td>Temperature</td><td data-testid="temperature">102.8°F</td><td>98.6°F ±1°F</td><td data-testid="temperature-status">Elevated</td></tr>
				</tbody>
			</table>
			<p data-testid="weight-based-dosing">Medication dosing shows weight-based calculations.</p>
			<p data-testid="parental-consent-status">Parental consent status: on file — Sarah Foster (mother).</p>
		</div>
	{:else if sceneIndex === 4}
		<!-- David Wilson, ED-6 — incomplete nursing assessment -->
		<div class="card">
			<p>Completed Data: <strong data-testid="completed-data">Triage notes, initial vitals available</strong></p>
			<ul>
				<li data-testid="missing-data-item">Allergies: Assessment in progress</li>
				<li data-testid="missing-data-item">Medications: History pending</li>
				<li data-testid="missing-data-item">Pain scale: Not yet assessed</li>
			</ul>
			<p data-type="warning">
				Yellow Warning: <strong data-testid="yellow-warning">Assessment in progress</strong>
			</p>
			<p>Refresh Timer: <strong data-testid="refresh-timer">Auto-refresh every 30 seconds</strong></p>
			<p>Notification: <strong data-testid="notification">"Assessment updating - refresh for latest"</strong></p>
			<Button type="button" class="btn secondary" data-testid="request-priority-completion-button">
				Request priority completion
			</Button>
		</div>
	{:else if sceneIndex === 5}
		<!-- Lisa Brown, ED-9 — shift change handoff -->
		<div class="card">
			<p>Clinical Summary: <strong data-testid="clinical-summary">Stable condition, pain controlled</strong></p>
			<p>Pending Tasks: <strong data-testid="pending-tasks">Orthopedic consult ordered - pending</strong></p>
			<p>Communication Log: <strong data-testid="communication-log">Family contact: Son updated 18:30</strong></p>
			<p>Special Needs: <strong data-testid="special-needs">Patient preference: Female staff</strong></p>
			<p data-testid="handoff-notes-timestamp">Handoff notes recorded at 18:45.</p>
			<Button type="button" class="btn secondary" data-testid="add-physician-handoff-notes">
				Add physician handoff notes
			</Button>
			<p data-testid="combined-handoff-information">
				Both nursing and physician handoff information are visible to the evening team.
			</p>
		</div>
	{:else if sceneIndex === 6}
		<!-- Thomas Anderson, ED-4 — network connectivity issues -->
		<div class="card">
			<table>
				<thead><tr><th>Data Type</th><th>Availability</th></tr></thead>
				<tbody>
					<tr><td>Basic Demographics</td><td data-testid="basic-demographics">Available (cached)</td></tr>
					<tr><td>Last Known Vitals</td><td data-testid="last-known-vitals">Available - last sync 16:45</td></tr>
					<tr><td>Medication Data</td><td data-testid="medication-data">Available (cached)</td></tr>
					<tr><td>Recent Lab Results</td><td data-testid="recent-lab-results">May not be current - sync pending</td></tr>
				</tbody>
			</table>
			<p data-type="warning" role="alert" data-testid="connectivity-warning">
				Limited connectivity - data may not be current
			</p>
			<p data-testid="automatic-sync-status">The app attempts automatic sync when connection is restored.</p>
			<p data-testid="sync-priority-notice">Critical data is prioritized for sync when connectivity returns.</p>
			<Button type="button" class="btn secondary" data-testid="manual-refresh-button">
				Refresh now
			</Button>
		</div>
	{:else if sceneIndex === 7}
		<!-- Karen White, ED-7 — time-sensitive alerts -->
		<div class="card">
			<table>
				<thead><tr><th>Alert Priority</th><th>Alert Details</th></tr></thead>
				<tbody>
					<tr><td>CRITICAL</td><td data-testid="critical">🔴 Troponin 0.8 - Possible MI (17:15)</td></tr>
					<tr><td>WARNING</td><td data-testid="warning">🟡 Medication due - Metoprolol (17:30)</td></tr>
					<tr><td>INFO</td><td data-testid="info">🔵 Pain reassessment overdue (17:25)</td></tr>
				</tbody>
			</table>
			<Button type="button" class="btn secondary" data-testid="alert-acknowledgment">
				Acknowledge critical alert
			</Button>
			<p data-testid="alert-timestamp">Alerts generated at 17:15, 17:25, and 17:30.</p>
			<Button type="button" class="btn secondary" data-testid="alert-action-button">
				Take action
			</Button>
			<p data-testid="alert-resolution-tracking">Alert resolution is tracked and timestamped.</p>
		</div>
	{/if}
</div>
