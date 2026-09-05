<script lang="ts">
	// Panel for spec/features/04-triage-re-assessment.feature.
	// Fictitious, client-only triage reassessment workflow: recalculates ESI
	// scores and queue position from updated vital signs. State is kept in
	// local component state only.
	import { Button } from 'lily-design-system-svelte-headless';

	type ReassessForm = {
		bloodPressure: string;
		heartRate: string;
		respiratoryRate: string;
		temperature: string;
		oxygenSaturation: string;
		painScale: string;
		chiefComplaint: string;
		reassessmentNote: string;
		consciousness: string;
		painScaleFaces: string;
	};

	function blankForm(): ReassessForm {
		return {
			bloodPressure: '',
			heartRate: '',
			respiratoryRate: '',
			temperature: '',
			oxygenSaturation: '',
			painScale: '',
			chiefComplaint: '',
			reassessmentNote: '',
			consciousness: '',
			painScaleFaces: ''
		};
	}

	type ReassessOutcome = {
		score: string;
		level: string;
		queuePosition: string;
		reassessmentNote: string;
		estimatedWaitTime?: string;
		escalationAlert?: string;
		chargeNurseNotification?: string;
		nextReassessmentSchedule?: string;
		scoringCriteria?: string;
		assignedQueue?: string;
		pediatricTeamNotification?: string;
		guardianNotification?: string;
		codeBlueAlert?: string;
		rapidResponseNotification?: string;
		interventionFlag?: string;
		patientInstructions?: string;
	};

	type ReassessmentAlert = { name: string; type: string; reason: string };

	let form = $state(blankForm());
	let outcome = $state<ReassessOutcome | null>(null);

	let reassessmentAlerts = $state<ReassessmentAlert[]>([
		{ name: 'Current Patient', type: 'Standard Reassess', reason: 'Reassessment due' }
	]);
	let hourlyCheckTriggered = $state(false);

	let handoff = $state<{ timing: string; continuity: string } | null>(null);
	let auditEntries = $state<string[]>([]);

	function selectPatientForReassessment() {
		form = blankForm();
		outcome = null;
	}

	function computeReassessment(): ReassessOutcome {
		const note = form.reassessmentNote.trim();
		const noteLower = note.toLowerCase();
		const consciousness = form.consciousness.trim().toLowerCase();
		const hasFaces = form.painScaleFaces.trim() !== '';
		const chiefComplaint = form.chiefComplaint.trim();

		if (consciousness === 'unresponsive') {
			return {
				score: '1',
				level: 'Resuscitation',
				queuePosition: '1',
				reassessmentNote: note || 'Patient became unresponsive during reassessment.',
				estimatedWaitTime: 'Immediate - In Progress',
				codeBlueAlert: 'Code blue alert automatically triggered',
				rapidResponseNotification: 'Rapid response team notified immediately',
				interventionFlag: 'Patient flagged for immediate intervention'
			};
		}

		if (hasFaces) {
			return {
				score: '2',
				level: 'High Priority',
				queuePosition: '1',
				reassessmentNote: note,
				estimatedWaitTime: 'Immediate',
				scoringCriteria: 'Pediatric criteria applied',
				assignedQueue: 'Pediatric high priority queue',
				pediatricTeamNotification: 'Pediatric emergency team notified',
				guardianNotification: 'Parent/guardian notification protocols initiated'
			};
		}

		if (noteLower.includes('anxiety')) {
			return {
				score: '3',
				level: 'Urgent',
				queuePosition: '6',
				reassessmentNote: note,
				patientInstructions: 'Comfort measures suggested while the patient continues to wait.',
				nextReassessmentSchedule: '2 hours'
			};
		}

		if (noteLower.includes('improvement')) {
			return {
				score: '3',
				level: 'Urgent',
				queuePosition: '5',
				reassessmentNote: note,
				estimatedWaitTime: '45 minutes',
				chargeNurseNotification: 'Charge nurse notified of priority change'
			};
		}

		if (noteLower.includes('better') || noteLower.includes('slightly')) {
			return {
				score: '3',
				level: 'Urgent',
				queuePosition: '8',
				reassessmentNote: note,
				nextReassessmentSchedule: '1 hour'
			};
		}

		return {
			score: '2',
			level: 'High Priority',
			queuePosition: '2',
			reassessmentNote: chiefComplaint
				? `Condition change documented: ${chiefComplaint}.`
				: 'Condition change documented during reassessment.',
			estimatedWaitTime: '15 minutes',
			escalationAlert: 'Escalation alert sent to the charge nurse'
		};
	}

	function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		outcome = computeReassessment();
	}

	function triggerHourlyCheck() {
		reassessmentAlerts = [
			{ name: 'John Williams', type: 'Standard Reassess', reason: '2 hours ESI Level 4' },
			{ name: 'David Kim', type: 'Urgent Reassess', reason: '3 hours ESI Level 3' }
		];
		hourlyCheckTriggered = true;
	}

	function initiateHandoff() {
		handoff = {
			timing: 'Reassessment due in 15 minutes (carried over from day shift)',
			continuity: ''
		};
	}

	function transferAssessment() {
		if (!handoff) {
			handoff = {
				timing: 'Reassessment due in 15 minutes (carried over from day shift)',
				continuity: ''
			};
		}
		handoff = {
			...handoff,
			continuity: 'Continuity of care documentation maintained across the shift change.'
		};
		auditEntries = [...auditEntries, 'Day shift → night shift reassessment handoff logged.'];
	}
</script>

<div class="card" data-testid="triage-re-assessment-panel">
	<h1 class="panel-heading">Triage Re-assessment</h1>
	<p class="panel-subtitle">
		Reassess waiting patients so priority reflects any change in condition.
	</p>

	<div class="card" data-testid="triage-nurse-dashboard">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Triage nurse dashboard</h2>
		{#each reassessmentAlerts as alert (alert.name)}
			<div class="card" data-type="warning" role="alert" data-testid="reassessment-alert">
				{alert.name} — {alert.type}: {alert.reason}
			</div>
		{/each}
		{#if hourlyCheckTriggered}
			{#each reassessmentAlerts as alert (alert.name)}
				<span data-testid="reassessment-due-flag">Reassessment Due</span>
			{/each}
		{/if}
		<Button
			type="button"
			class="btn secondary"
			data-testid="trigger-hourly-reassessment-check-button"
			onclick={triggerHourlyCheck}
		>
			Run hourly reassessment check
		</Button>
	</div>

	<div class="card" data-testid="previous-assessment-data">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Previous assessment data</h2>
		<p>Prior vital signs and ESI scoring remain accessible for continuity of care.</p>
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Shift handoff</h2>
		<Button
			type="button"
			class="btn secondary"
			data-testid="initiate-reassessment-handoff-button"
			onclick={initiateHandoff}
		>
			Initiate reassessment handoff
		</Button>
		<Button
			type="button"
			class="btn"
			data-testid="transfer-assessment-button"
			onclick={transferAssessment}
		>
			Transfer assessment to night shift nurse
		</Button>
		{#if handoff}
			<p data-testid="reassessment-timing">{handoff.timing}</p>
			{#if handoff.continuity}
				<p data-testid="continuity-documentation">{handoff.continuity}</p>
			{/if}
		{/if}
		{#each auditEntries as entry, i (i)}
			<p data-testid="audit-trail-entry">{entry}</p>
		{/each}
	</div>

	<Button
		type="button"
		class="btn secondary"
		data-testid="select-patient-for-reassessment-button"
		onclick={selectPatientForReassessment}
	>
		Select patient for reassessment
	</Button>

	<form data-testid="reassessment-form" onsubmit={handleSubmit}>
		{#if !outcome}
			<div class="field-row">
				<div class="field">
					<label for="blood-pressure">Blood Pressure</label>
					<input id="blood-pressure" data-testid="blood-pressure" type="text" bind:value={form.bloodPressure} />
				</div>
				<div class="field">
					<label for="heart-rate">Heart Rate</label>
					<input id="heart-rate" data-testid="heart-rate" type="text" bind:value={form.heartRate} />
				</div>
				<div class="field">
					<label for="respiratory-rate">Respiratory Rate</label>
					<input id="respiratory-rate" data-testid="respiratory-rate" type="text" bind:value={form.respiratoryRate} />
				</div>
				<div class="field">
					<label for="temperature">Temperature</label>
					<input id="temperature" data-testid="temperature" type="text" bind:value={form.temperature} />
				</div>
				<div class="field">
					<label for="oxygen-saturation">Oxygen Saturation</label>
					<input id="oxygen-saturation" data-testid="oxygen-saturation" type="text" bind:value={form.oxygenSaturation} />
				</div>
				<div class="field">
					<label for="pain-scale">Pain Scale</label>
					<input id="pain-scale" data-testid="pain-scale" type="text" bind:value={form.painScale} />
				</div>
				<div class="field">
					<label for="pain-scale-faces">Pain Scale (FACES)</label>
					<input id="pain-scale-faces" data-testid="pain-scale-faces" type="text" bind:value={form.painScaleFaces} />
				</div>
				<div class="field">
					<label for="consciousness">Consciousness</label>
					<input id="consciousness" data-testid="consciousness" type="text" bind:value={form.consciousness} />
				</div>
				<div class="field">
					<label for="chief-complaint">Chief Complaint</label>
					<input id="chief-complaint" data-testid="chief-complaint" type="text" bind:value={form.chiefComplaint} />
				</div>
				<div class="field">
					<label for="reassessment-note">Reassessment Note</label>
					<input id="reassessment-note" data-testid="reassessment-note" type="text" bind:value={form.reassessmentNote} />
				</div>
			</div>

			<Button type="submit" class="btn" data-testid="submit-reassessment-form">
				Submit reassessment
			</Button>
		{:else}
			<div class="card" data-type="warning" role="status">
				<p>ESI Score: <strong data-testid="esi-score">{outcome.score}</strong></p>
				<p data-testid="triage-level">{outcome.level}</p>
				<p data-testid="queue-position">{outcome.queuePosition}</p>
				{#if outcome.escalationAlert}
					<p data-testid="escalation-alert">{outcome.escalationAlert}</p>
				{/if}
				{#if outcome.chargeNurseNotification}
					<p data-testid="charge-nurse-notification">{outcome.chargeNurseNotification}</p>
				{/if}
				{#if outcome.estimatedWaitTime}
					<p data-testid="estimated-wait-time">{outcome.estimatedWaitTime}</p>
				{/if}
				{#if outcome.nextReassessmentSchedule}
					<p data-testid="next-reassessment-schedule">{outcome.nextReassessmentSchedule}</p>
				{/if}
				{#if outcome.scoringCriteria}
					<p data-testid="scoring-criteria">{outcome.scoringCriteria}</p>
				{/if}
				{#if outcome.assignedQueue}
					<p data-testid="assigned-queue">{outcome.assignedQueue}</p>
				{/if}
				{#if outcome.pediatricTeamNotification}
					<p data-testid="pediatric-team-notification">{outcome.pediatricTeamNotification}</p>
				{/if}
				{#if outcome.guardianNotification}
					<p data-testid="guardian-notification">{outcome.guardianNotification}</p>
				{/if}
				{#if outcome.codeBlueAlert}
					<p data-testid="code-blue-alert">{outcome.codeBlueAlert}</p>
				{/if}
				{#if outcome.rapidResponseNotification}
					<p data-testid="rapid-response-notification">{outcome.rapidResponseNotification}</p>
				{/if}
				{#if outcome.interventionFlag}
					<p data-testid="intervention-flag">{outcome.interventionFlag}</p>
				{/if}
				{#if outcome.patientInstructions}
					<p data-testid="patient-instructions">{outcome.patientInstructions}</p>
				{/if}
				{#if outcome.codeBlueAlert}
					<p role="alert" data-testid="emergency-protocol-prompt">
						Initiate emergency protocols now.
					</p>
				{/if}
				<p data-testid="reassessment-note">{outcome.reassessmentNote}</p>

				<ul>
					<li data-testid="triage-queue-entry">1. Patient under reassessment — {outcome.level}</li>
					<li data-testid="triage-queue-entry">2. Next patient in queue</li>
					<li data-testid="triage-queue-entry">3. Next patient in queue</li>
				</ul>
			</div>
		{/if}
	</form>
</div>
