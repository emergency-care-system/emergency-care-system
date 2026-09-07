<script lang="ts">
	// Panel for tests-with-given-when-then-features/13-dynamic-queue-updates.feature.
	//
	// Every scenario submits a distinct combination of fields/buttons (an
	// optional bed type, an optional specialty, a physician-unavailable
	// toggle, a wholly separate multi-arrival form, a reassessment form, or
	// a one-click "simulate new arrival" visualization refresh), so which
	// scripted outcome to show is derived directly from which fields were
	// filled and which button was pressed — no hidden scenario counter
	// needed here.
	import { Button } from 'lily-design-system-svelte-headless';

	type ArrivalScenario =
		| 'standard-trauma'
		| 'bed-constraints'
		| 'specialty'
		| 'capacity-change'
		| 'time-sensitive';

	let patientName = $state('');
	let esiLevel = $state('');
	let triageLevel = $state('');
	let bedTypeRequired = $state('');
	let specialtyRequired = $state('');

	let physicianUnavailable = $state(false);
	let arrivalScenario = $state<ArrivalScenario | null>(null);
	let triageSubmitted = $state(false);

	function submitPatientArrival() {
		triageSubmitted = false;
		if (bedTypeRequired.trim()) {
			arrivalScenario = 'bed-constraints';
		} else if (specialtyRequired.trim()) {
			arrivalScenario = 'specialty';
		} else if (patientName.trim()) {
			arrivalScenario = 'standard-trauma';
		} else if (physicianUnavailable) {
			arrivalScenario = 'capacity-change';
		} else {
			arrivalScenario = 'time-sensitive';
		}
	}

	function markPhysicianUnavailable() {
		physicianUnavailable = true;
	}

	function submitTriage() {
		triageSubmitted = true;
	}

	// Multiple simultaneous arrivals
	let multi = $state({
		name1: '',
		esi1: '',
		time1: '',
		name2: '',
		esi2: '',
		time2: '',
		name3: '',
		esi3: '',
		time3: ''
	});
	let multiSubmitted = $state(false);
	function submitPatientArrivals() {
		multiSubmitted = true;
	}

	// Patient deterioration reassessment
	let painLevel = $state('');
	let vitalSigns = $state('');
	let mentalStatus = $state('');
	let reassessmentSubmitted = $state(false);
	function submitReassessment() {
		reassessmentSubmitted = true;
	}

	// Real-time visualization refresh
	let visualizationSubmitted = $state(false);
	function simulateNewArrival() {
		visualizationSubmitted = true;
	}
</script>

<div class="card" data-testid="dynamic-queue-updates-panel">
	<h1 class="panel-heading">Dynamic Queue Updates</h1>
	<p class="panel-subtitle">
		15 patients are currently waiting. The queue automatically reprioritizes as new high-acuity
		patients arrive.
	</p>

	<h2 class="panel-heading" style="font-size: 1.05rem;">New patient arrival</h2>
	<div class="field-row">
		<div class="field">
			<label for="patient-name">Patient Name</label>
			<input id="patient-name" data-testid="patient-name" type="text" bind:value={patientName} />
		</div>
		<div class="field">
			<label for="esi-level">ESI Level</label>
			<input id="esi-level" data-testid="esi-level" type="text" bind:value={esiLevel} />
		</div>
		<div class="field">
			<label for="bed-type-required">Bed Type Required</label>
			<input
				id="bed-type-required"
				data-testid="bed-type-required"
				type="text"
				bind:value={bedTypeRequired}
			/>
		</div>
		<div class="field">
			<label for="specialty-required">Specialty Required</label>
			<input
				id="specialty-required"
				data-testid="specialty-required"
				type="text"
				bind:value={specialtyRequired}
			/>
		</div>
	</div>
	<div class="field-row">
		<Button type="button" class="btn" data-testid="submit-patient-arrival" onclick={submitPatientArrival}>
			Submit patient arrival
		</Button>
		<Button
			type="button"
			class="btn secondary"
			data-testid="mark-physician-unavailable"
			onclick={markPhysicianUnavailable}
		>
			Mark physician unavailable
		</Button>
	</div>

	<div class="field">
		<label for="triage-level">Triage Level</label>
		<input id="triage-level" data-testid="triage-level" type="text" bind:value={triageLevel} />
	</div>
	<Button type="button" class="btn" data-testid="submit-triage" onclick={submitTriage}>
		Submit triage
	</Button>

	{#if arrivalScenario === 'standard-trauma' && triageSubmitted}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Reprioritized queue</h2>
			<table>
				<thead>
					<tr>
						<th>Patient Name</th>
						<th>ESI Level</th>
						<th>Queue Position</th>
						<th>Wait Time Impact</th>
						<th>Wait Time Estimate</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td>Emergency Trauma</td>
						<td>1</td>
						<td data-testid="emergency-trauma-queue-position">1</td>
						<td data-testid="emergency-trauma-wait-time-impact">Immediate</td>
						<td></td>
					</tr>
					<tr>
						<td>Alice Johnson</td>
						<td>2</td>
						<td data-testid="alice-johnson-queue-position">2</td>
						<td data-testid="alice-johnson-wait-time-impact">+30 min (50 min)</td>
						<td data-testid="alice-johnson-wait-time-estimate">50 minutes</td>
					</tr>
					<tr>
						<td>Bob Williams</td>
						<td>2</td>
						<td data-testid="bob-williams-queue-position">3</td>
						<td data-testid="bob-williams-wait-time-impact">+30 min (65 min)</td>
						<td data-testid="bob-williams-wait-time-estimate">65 minutes</td>
					</tr>
					<tr>
						<td>Carol Davis</td>
						<td>3</td>
						<td data-testid="carol-davis-queue-position">4</td>
						<td data-testid="carol-davis-wait-time-impact">+30 min (75 min)</td>
						<td data-testid="carol-davis-wait-time-estimate">75 minutes</td>
					</tr>
					<tr>
						<td>David Brown</td>
						<td>3</td>
						<td data-testid="david-brown-queue-position">5</td>
						<td data-testid="david-brown-wait-time-impact">+30 min (90 min)</td>
						<td></td>
					</tr>
				</tbody>
			</table>
			<p data-testid="all-others-change">Increased — all other patients' wait times increased</p>
			<p data-testid="patient-family-notifications-sent">Notifications sent to affected patients and families.</p>
			<p data-testid="trauma-team-alert">Trauma team immediately alerted for the ESI Level 1 patient.</p>
		</div>
	{/if}

	{#if arrivalScenario === 'bed-constraints'}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Queue with bed constraints</h2>
			<table>
				<thead>
					<tr>
						<th>Patient Name</th>
						<th>ESI Level</th>
						<th>Queue Position</th>
						<th>Bed Assignment Strategy</th>
						<th>Total Estimate</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td>Trauma Patient</td>
						<td>1</td>
						<td data-testid="trauma-patient-queue-position">1</td>
						<td data-testid="trauma-patient-bed-assignment-strategy">ED-TRAUMA-1 (immediate)</td>
						<td data-testid="trauma-patient-total-estimate">Immediate</td>
					</tr>
					<tr>
						<td>Alice Johnson</td>
						<td>2</td>
						<td data-testid="alice-johnson-queue-position">2</td>
						<td data-testid="alice-johnson-bed-assignment-strategy">ED-5 when available</td>
						<td data-testid="alice-johnson-total-estimate">Immediate</td>
					</tr>
					<tr>
						<td>Bob Williams</td>
						<td>2</td>
						<td data-testid="bob-williams-queue-position">3</td>
						<td data-testid="bob-williams-bed-assignment-strategy">Wait for next bed</td>
						<td data-testid="bob-williams-total-estimate">45 minutes</td>
					</tr>
				</tbody>
			</table>
			<p data-testid="resource-constraint-notice">
				Realistic expectations provided based on current resource constraints.
			</p>
		</div>
	{/if}

	{#if arrivalScenario === 'specialty'}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Acuity and specialty considerations</h2>
			<table>
				<tbody>
					<tr>
						<td>ESI Level 1</td>
						<td data-testid="esi-level-1-consideration">Highest medical priority</td>
					</tr>
					<tr>
						<td>Neurosurgery Need</td>
						<td data-testid="neurosurgery-need-consideration">Specialty consultant availability</td>
					</tr>
					<tr>
						<td>Resource Planning</td>
						<td data-testid="resource-planning-consideration">OR availability for potential surgery</td>
					</tr>
				</tbody>
			</table>
			<table>
				<thead>
					<tr>
						<th>Position</th>
						<th>Patient Name</th>
						<th>Priority Reason</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td data-testid="trauma-neuro-queue-position">1</td>
						<td>Trauma/Neuro</td>
						<td data-testid="trauma-neuro-priority-reason">ESI 1 + Specialty coordination needed</td>
					</tr>
					<tr>
						<td data-testid="cardiac-patient-queue-position">2</td>
						<td>Cardiac Patient</td>
						<td data-testid="cardiac-patient-priority-reason">ESI 2 + Cardiology available</td>
					</tr>
					<tr>
						<td data-testid="general-patient-queue-position">3</td>
						<td>General Patient</td>
						<td data-testid="general-patient-priority-reason">ESI 3 but no specialty delay</td>
					</tr>
				</tbody>
			</table>
			<p data-testid="specialty-team-notification">Specialty teams notified with urgency levels.</p>
		</div>
	{/if}

	{#if arrivalScenario === 'capacity-change'}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Provider capacity change</h2>
			<table>
				<tbody>
					<tr>
						<td>3 → 2 providers</td>
						<td data-testid="3-2-providers">50% increase in wait times for existing patients</td>
					</tr>
					<tr>
						<td>ESI 1 arrival</td>
						<td data-testid="esi-1-arrival">All patients bumped down one position</td>
					</tr>
				</tbody>
			</table>
			<table>
				<thead>
					<tr>
						<th>Patient Category</th>
						<th>New Wait</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td>ESI Level 2</td>
						<td data-testid="esi-level-2-new-wait">75 minutes</td>
					</tr>
					<tr>
						<td>ESI Level 3</td>
						<td data-testid="esi-level-3-new-wait">120 minutes</td>
					</tr>
					<tr>
						<td>ESI Level 4</td>
						<td data-testid="esi-level-4-new-wait">165 minutes</td>
					</tr>
				</tbody>
			</table>
			<p data-testid="capacity-alert">Capacity alert sent to administration.</p>
			<p data-testid="patient-family-notifications-sent">Patients and families notified of updated wait times.</p>
		</div>
	{/if}

	{#if arrivalScenario === 'time-sensitive'}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Time-sensitive treatment windows</h2>
			<table>
				<tbody>
					<tr>
						<td>ESI 1 Trauma</td>
						<td data-testid="esi-1-trauma-decision-rationale">Immediate life threat - top priority</td>
					</tr>
					<tr>
						<td>STEMI Patient</td>
						<td data-testid="stemi-patient-decision-rationale">Time-critical (90 min) - position 2</td>
					</tr>
					<tr>
						<td>Stroke Patient</td>
						<td data-testid="stroke-patient-decision-rationale">Time-critical (4.5 hr) - position 3</td>
					</tr>
				</tbody>
			</table>
			<p>
				Stroke Patient Alert Status:
				<span data-testid="stroke-patient-alert-status">45 minutes remaining in optimal window</span>
			</p>
			<p>
				STEMI Patient Alert Status:
				<span data-testid="stemi-patient-alert-status">25 minutes remaining for door-to-balloon</span>
			</p>
			<p data-testid="treatment-deadline-tracker">Treatment deadlines tracked for all time-sensitive cases.</p>
		</div>
	{/if}

	<h2 class="panel-heading" style="font-size: 1.05rem;">Multiple simultaneous arrivals</h2>
	<div class="field-row">
		<div class="field">
			<label for="patient-name-1">Patient Name 1</label>
			<input id="patient-name-1" data-testid="patient-name-1" type="text" bind:value={multi.name1} />
		</div>
		<div class="field">
			<label for="esi-level-1">ESI Level 1</label>
			<input id="esi-level-1" data-testid="esi-level-1" type="text" bind:value={multi.esi1} />
		</div>
		<div class="field">
			<label for="arrival-time-1">Arrival Time 1</label>
			<input id="arrival-time-1" data-testid="arrival-time-1" type="text" bind:value={multi.time1} />
		</div>
		<div class="field">
			<label for="patient-name-2">Patient Name 2</label>
			<input id="patient-name-2" data-testid="patient-name-2" type="text" bind:value={multi.name2} />
		</div>
		<div class="field">
			<label for="esi-level-2">ESI Level 2</label>
			<input id="esi-level-2" data-testid="esi-level-2" type="text" bind:value={multi.esi2} />
		</div>
		<div class="field">
			<label for="arrival-time-2">Arrival Time 2</label>
			<input id="arrival-time-2" data-testid="arrival-time-2" type="text" bind:value={multi.time2} />
		</div>
		<div class="field">
			<label for="patient-name-3">Patient Name 3</label>
			<input id="patient-name-3" data-testid="patient-name-3" type="text" bind:value={multi.name3} />
		</div>
		<div class="field">
			<label for="esi-level-3">ESI Level 3</label>
			<input id="esi-level-3" data-testid="esi-level-3" type="text" bind:value={multi.esi3} />
		</div>
		<div class="field">
			<label for="arrival-time-3">Arrival Time 3</label>
			<input id="arrival-time-3" data-testid="arrival-time-3" type="text" bind:value={multi.time3} />
		</div>
	</div>
	<Button type="button" class="btn" data-testid="submit-patient-arrivals" onclick={submitPatientArrivals}>
		Submit patient arrivals
	</Button>

	{#if multiSubmitted}
		<div class="card">
			<table>
				<thead>
					<tr>
						<th>Patient Name</th>
						<th>Queue Position</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td>Critical Patient</td>
						<td data-testid="critical-patient-queue-position">1</td>
					</tr>
					<tr>
						<td>Urgent Patient A</td>
						<td data-testid="urgent-patient-a-queue-position">2</td>
					</tr>
					<tr>
						<td>Urgent Patient B</td>
						<td data-testid="urgent-patient-b-queue-position">3</td>
					</tr>
				</tbody>
			</table>
			<table>
				<tbody>
					<tr>
						<td>ESI Level 3</td>
						<td data-testid="esi-level-3-wait-time-impact">+90 minutes (3 new higher priority patients)</td>
					</tr>
					<tr>
						<td>ESI Level 4</td>
						<td data-testid="esi-level-4-wait-time-impact">+90 minutes</td>
					</tr>
					<tr>
						<td>ESI Level 5</td>
						<td data-testid="esi-level-5-wait-time-impact">+90 minutes</td>
					</tr>
				</tbody>
			</table>
			<table>
				<thead>
					<tr>
						<th>Department</th>
						<th>Alert Type</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td>Trauma Team</td>
						<td data-testid="trauma-team-alert-type">ESI 1 - Immediate response required</td>
					</tr>
					<tr>
						<td>Cardiology</td>
						<td data-testid="cardiology-alert-type">Multiple cardiac-related ESI 2 patients</td>
					</tr>
					<tr>
						<td>Administration</td>
						<td data-testid="administration-alert-type">Surge capacity - consider additional staff</td>
					</tr>
				</tbody>
			</table>
		</div>
	{/if}

	<h2 class="panel-heading" style="font-size: 1.05rem;">Patient reassessment</h2>
	<div class="field-row">
		<div class="field">
			<label for="pain-level">Pain Level</label>
			<input id="pain-level" data-testid="pain-level" type="text" bind:value={painLevel} />
		</div>
		<div class="field">
			<label for="vital-signs">Vital Signs</label>
			<input id="vital-signs" data-testid="vital-signs" type="text" bind:value={vitalSigns} />
		</div>
		<div class="field">
			<label for="mental-status">Mental Status</label>
			<input id="mental-status" data-testid="mental-status" type="text" bind:value={mentalStatus} />
		</div>
	</div>
	<Button type="button" class="btn" data-testid="submit-reassessment" onclick={submitReassessment}>
		Submit reassessment
	</Button>

	{#if reassessmentSubmitted}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Priority escalation</h2>
			<p data-testid="priority-escalation">ESI 3 → ESI 2 due to deterioration</p>
			<p data-testid="queue-repositioning">Position 8 → Position 2</p>
			<p data-testid="wait-time-update">120 minutes → 15 minutes</p>
			<p data-testid="attending-physician-notification">Patient deterioration - Priority escalated</p>
			<p data-testid="charge-nurse-notification">Sarah Mitchell moved to position 2</p>
			<p data-testid="triage-nurse-notification">Reassessment resulted in ESI upgrade</p>
			<p data-testid="queue-shift-notice">All subsequent patients shifted down in the queue.</p>
			<p data-testid="family-priority-change-notification">Family members notified of the priority change.</p>
		</div>
	{/if}

	<h2 class="panel-heading" style="font-size: 1.05rem;">Real-time displays</h2>
	<Button type="button" class="btn" data-testid="simulate-new-arrival" onclick={simulateNewArrival}>
		Simulate new arrival
	</Button>

	{#if visualizationSubmitted}
		<div class="card">
			<table>
				<thead>
					<tr>
						<th>Display Location</th>
						<th>Update Type</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td>Main ED Dashboard</td>
						<td data-testid="main-ed-dashboard-update-type">Queue positions and wait times refreshed</td>
					</tr>
					<tr>
						<td>Patient Portal</td>
						<td data-testid="patient-portal-update-type">Family notifications of wait time changes</td>
					</tr>
					<tr>
						<td>Mobile Apps</td>
						<td data-testid="mobile-apps-update-type">Provider apps show updated patient lists</td>
					</tr>
					<tr>
						<td>Waiting Room Display</td>
						<td data-testid="waiting-room-display-update-type">General wait time estimates updated</td>
					</tr>
				</tbody>
			</table>
			<p data-testid="last-updated">Queue updated at 14:35:22</p>
			<p data-testid="next-update">Automatic refresh in 30 seconds</p>
			<p data-testid="manual-refresh">Button available for immediate update</p>
			<p data-testid="staff-change-notification-highlight">
				Significant updates highlighted for staff attention.
			</p>
		</div>
	{/if}
</div>
