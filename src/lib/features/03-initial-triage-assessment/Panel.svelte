<script lang="ts">
	// Panel for tests-with-given-when-then-features/03-initial-triage-assessment.feature.
	// Fictitious, client-only ESI (Emergency Severity Index) triage assessment
	// workflow. State is kept in local component state only.
	import { Button } from 'lily-design-system-svelte-headless';

	type VitalsForm = {
		bloodPressure: string;
		heartRate: string;
		respiratoryRate: string;
		temperature: string;
		oxygenSaturation: string;
		chiefComplaint: string;
		painScale: string;
		onset: string;
		interventionFlag: string;
	};

	function blankVitalsForm(): VitalsForm {
		return {
			bloodPressure: '',
			heartRate: '',
			respiratoryRate: '',
			temperature: '',
			oxygenSaturation: '',
			chiefComplaint: '',
			painScale: '',
			onset: '',
			interventionFlag: ''
		};
	}

	type ReassessForm = {
		bloodPressure: string;
		heartRate: string;
		respiratoryRate: string;
		temperature: string;
		oxygenSaturation: string;
		chiefComplaint: string;
		painScale: string;
	};

	function blankReassessForm(): ReassessForm {
		return {
			bloodPressure: '',
			heartRate: '',
			respiratoryRate: '',
			temperature: '',
			oxygenSaturation: '',
			chiefComplaint: '',
			painScale: ''
		};
	}

	type Mode = 'idle' | 'triage' | 'reassessment';

	type EsiResult = {
		score: '1' | '2' | '3' | '4';
		level: string;
		pediatric: boolean;
	};

	type TriageOutcome = {
		score: string;
		level: string;
		queuePosition?: string;
		physicianAlert?: string;
		codeAlert?: string;
		traumaTeamNotification?: string;
		scoringCriteria?: string;
		assignedQueue?: string;
		pediatricTeamNotification?: string;
		estimatedWaitTime: string;
	};

	type ReassessOutcome = {
		score: string;
		level: string;
		queuePosition: string;
		escalationAlert: string;
		conditionChangeNote: string;
	};

	type BulkEntry = { name: string; esi: string; level: string };

	let mode = $state<Mode>('idle');
	let vitalsForm = $state(blankVitalsForm());
	let reassessForm = $state(blankReassessForm());
	let vitalsErrors = $state<Record<string, string>>({});
	let assessmentStatus = $state('');
	let triageOutcome = $state<TriageOutcome | null>(null);
	let reassessOutcome = $state<ReassessOutcome | null>(null);
	let bulkQueue = $state<BulkEntry[]>([]);
	let waitTimeStatus = $state('');

	const REQUIRED_VITALS: { key: keyof VitalsForm; label: string; message: string }[] = [
		{ key: 'bloodPressure', label: 'blood-pressure', message: 'Blood pressure is required' },
		{ key: 'heartRate', label: 'heart-rate', message: 'Heart rate is required' },
		{
			key: 'respiratoryRate',
			label: 'respiratory-rate',
			message: 'Respiratory rate is required'
		},
		{ key: 'temperature', label: 'temperature', message: 'Temperature is required' },
		{
			key: 'oxygenSaturation',
			label: 'oxygen-saturation',
			message: 'Oxygen saturation is required'
		}
	];

	function calculateEsi(chiefComplaint: string): EsiResult {
		const complaint = chiefComplaint.trim().toLowerCase();
		if (complaint.includes('unresponsive')) {
			return { score: '1', level: 'Resuscitation', pediatric: false };
		}
		if (complaint.includes('chest pain')) {
			return { score: '2', level: 'High Priority', pediatric: false };
		}
		if (complaint.includes('abdominal pain')) {
			return { score: '2', level: 'High Priority', pediatric: false };
		}
		if (complaint.includes('fever')) {
			return { score: '3', level: 'Urgent', pediatric: true };
		}
		if (complaint.includes('sprain') || complaint.includes('ankle')) {
			return { score: '4', level: 'Less Urgent', pediatric: false };
		}
		return { score: '4', level: 'Less Urgent', pediatric: false };
	}

	function buildTriageOutcome(esi: EsiResult): TriageOutcome {
		switch (esi.score) {
			case '1':
				return {
					score: '1',
					level: 'Resuscitation',
					queuePosition: '1',
					codeAlert: 'Code alert automatically triggered',
					traumaTeamNotification: 'Trauma team notified immediately',
					estimatedWaitTime: 'Immediate - In Progress'
				};
			case '2':
				return {
					score: '2',
					level: 'High Priority',
					queuePosition: '1',
					physicianAlert: 'Alert sent to the attending physician',
					estimatedWaitTime: 'Immediate'
				};
			case '3':
				return {
					score: '3',
					level: 'Urgent',
					scoringCriteria: esi.pediatric
						? 'Pediatric criteria applied'
						: 'Standard adult criteria applied',
					assignedQueue: esi.pediatric ? 'Urgent pediatric queue' : 'Urgent queue',
					pediatricTeamNotification: esi.pediatric ? 'Pediatric team notified' : undefined,
					estimatedWaitTime: '30-45 minutes'
				};
			default:
				return {
					score: '4',
					level: 'Less Urgent',
					assignedQueue: 'Less urgent queue',
					estimatedWaitTime: '60-90 minutes'
				};
		}
	}

	function selectPatientForTriage() {
		mode = 'triage';
		vitalsForm = blankVitalsForm();
		vitalsErrors = {};
		assessmentStatus = '';
		triageOutcome = null;
	}

	function selectPatientForReassessment() {
		mode = 'reassessment';
		reassessForm = blankReassessForm();
		reassessOutcome = null;
	}

	function submitTriageAssessment(event: SubmitEvent) {
		event.preventDefault();
		const newErrors: Record<string, string> = {};
		for (const field of REQUIRED_VITALS) {
			if (!vitalsForm[field.key].trim()) {
				newErrors[field.label] = field.message;
			}
		}
		vitalsErrors = newErrors;

		if (Object.keys(newErrors).length > 0) {
			assessmentStatus = 'Incomplete';
			triageOutcome = null;
			return;
		}

		assessmentStatus = 'Complete';
		const esi = calculateEsi(vitalsForm.chiefComplaint);
		triageOutcome = buildTriageOutcome(esi);
	}

	function submitReassessment(event: SubmitEvent) {
		event.preventDefault();
		const esi = calculateEsi(reassessForm.chiefComplaint);
		reassessOutcome = {
			score: esi.score,
			level: esi.level,
			queuePosition: '1',
			escalationAlert: 'Escalation alert sent to the charge nurse',
			conditionChangeNote: `Condition change documented: ${
				reassessForm.chiefComplaint.trim() || 'updated vital signs'
			}.`
		};
	}

	function completeAllAssessments() {
		bulkQueue = [
			{ name: 'Carol Davis', esi: '2', level: 'High Priority' },
			{ name: 'Alice Brown', esi: '3', level: 'Urgent' },
			{ name: 'Bob Wilson', esi: '4', level: 'Less Urgent' }
		];
		waitTimeStatus = 'Wait times calculated based on queue position and available resources.';
	}
</script>

<div class="card" data-testid="initial-triage-assessment-panel">
	<h1 class="panel-heading">Initial Triage Assessment</h1>
	<p class="panel-subtitle">
		Perform ESI-based triage assessments on registered patients waiting to be seen.
	</p>

	<div class="card" data-testid="triage-dashboard">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Triage dashboard</h2>
		<p>Patients waiting for triage: John Doe, Jane Smith, Emergency Patient, Tommy Jones (age 5), Mary Johnson.</p>
		<Button type="button" class="btn secondary" data-testid="select-patient-for-triage-button" onclick={selectPatientForTriage}>
			Select next patient for triage
		</Button>
		<Button type="button" class="btn secondary" data-testid="select-patient-for-reassessment-button" onclick={selectPatientForReassessment}>
			Select patient for reassessment
		</Button>
		<Button type="button" class="btn" data-testid="complete-all-triage-assessments-button" onclick={completeAllAssessments}>
			Complete all pending triage assessments
		</Button>
	</div>

	{#if mode === 'triage'}
		<form data-testid="triage-assessment-form" onsubmit={submitTriageAssessment}>
			<div class="field-row">
				<div class="field">
					<label for="blood-pressure">Blood Pressure</label>
					<input id="blood-pressure" data-testid="blood-pressure" type="text" bind:value={vitalsForm.bloodPressure} />
					{#if vitalsErrors['blood-pressure']}
						<p role="alert" data-testid="blood-pressure-error">{vitalsErrors['blood-pressure']}</p>
					{/if}
				</div>
				<div class="field">
					<label for="heart-rate">Heart Rate</label>
					<input id="heart-rate" data-testid="heart-rate" type="text" bind:value={vitalsForm.heartRate} />
					{#if vitalsErrors['heart-rate']}
						<p role="alert" data-testid="heart-rate-error">{vitalsErrors['heart-rate']}</p>
					{/if}
				</div>
				<div class="field">
					<label for="respiratory-rate">Respiratory Rate</label>
					<input id="respiratory-rate" data-testid="respiratory-rate" type="text" bind:value={vitalsForm.respiratoryRate} />
					{#if vitalsErrors['respiratory-rate']}
						<p role="alert" data-testid="respiratory-rate-error">{vitalsErrors['respiratory-rate']}</p>
					{/if}
				</div>
				<div class="field">
					<label for="temperature">Temperature</label>
					<input id="temperature" data-testid="temperature" type="text" bind:value={vitalsForm.temperature} />
					{#if vitalsErrors['temperature']}
						<p role="alert" data-testid="temperature-error">{vitalsErrors['temperature']}</p>
					{/if}
				</div>
				<div class="field">
					<label for="oxygen-saturation">Oxygen Saturation</label>
					<input id="oxygen-saturation" data-testid="oxygen-saturation" type="text" bind:value={vitalsForm.oxygenSaturation} />
					{#if vitalsErrors['oxygen-saturation']}
						<p role="alert" data-testid="oxygen-saturation-error">{vitalsErrors['oxygen-saturation']}</p>
					{/if}
				</div>
				<div class="field">
					<label for="chief-complaint">Chief Complaint</label>
					<input id="chief-complaint" data-testid="chief-complaint" type="text" bind:value={vitalsForm.chiefComplaint} />
				</div>
				<div class="field">
					<label for="pain-scale">Pain Scale</label>
					<input id="pain-scale" data-testid="pain-scale" type="text" bind:value={vitalsForm.painScale} />
				</div>
				<div class="field">
					<label for="onset">Onset</label>
					<input id="onset" data-testid="onset" type="text" bind:value={vitalsForm.onset} />
				</div>
				<div class="field">
					<label for="intervention-flag">Intervention Flag</label>
					<input id="intervention-flag" data-testid="intervention-flag" type="text" bind:value={vitalsForm.interventionFlag} />
				</div>
			</div>

			<Button type="submit" class="btn" data-testid="submit-triage-assessment-form">
				Submit triage assessment
			</Button>
		</form>

		{#if assessmentStatus}
			<p data-testid="assessment-status">{assessmentStatus}</p>
		{/if}

		{#if triageOutcome}
			<div class="card" data-type="info" role="status">
				<p>ESI Score: <strong data-testid="esi-score">{triageOutcome.score}</strong></p>
				<p data-testid="triage-level">{triageOutcome.level}</p>
				{#if triageOutcome.queuePosition}
					<p data-testid="queue-position">{triageOutcome.queuePosition}</p>
				{/if}
				{#if triageOutcome.physicianAlert}
					<p data-testid="physician-alert">{triageOutcome.physicianAlert}</p>
				{/if}
				{#if triageOutcome.codeAlert}
					<p data-testid="code-alert">{triageOutcome.codeAlert}</p>
				{/if}
				{#if triageOutcome.traumaTeamNotification}
					<p data-testid="trauma-team-notification">{triageOutcome.traumaTeamNotification}</p>
				{/if}
				{#if triageOutcome.scoringCriteria}
					<p data-testid="scoring-criteria">{triageOutcome.scoringCriteria}</p>
				{/if}
				{#if triageOutcome.assignedQueue}
					<p data-testid="assigned-queue">{triageOutcome.assignedQueue}</p>
				{/if}
				{#if triageOutcome.pediatricTeamNotification}
					<p data-testid="pediatric-team-notification">{triageOutcome.pediatricTeamNotification}</p>
				{/if}
				<p data-testid="estimated-wait-time">{triageOutcome.estimatedWaitTime}</p>
			</div>
		{/if}
	{:else if mode === 'reassessment'}
		<form onsubmit={submitReassessment}>
			<div class="field-row">
				<div class="field">
					<label for="reassess-blood-pressure">Blood Pressure</label>
					<input id="reassess-blood-pressure" data-testid="blood-pressure" type="text" bind:value={reassessForm.bloodPressure} />
				</div>
				<div class="field">
					<label for="reassess-heart-rate">Heart Rate</label>
					<input id="reassess-heart-rate" data-testid="heart-rate" type="text" bind:value={reassessForm.heartRate} />
				</div>
				<div class="field">
					<label for="reassess-respiratory-rate">Respiratory Rate</label>
					<input id="reassess-respiratory-rate" data-testid="respiratory-rate" type="text" bind:value={reassessForm.respiratoryRate} />
				</div>
				<div class="field">
					<label for="reassess-temperature">Temperature</label>
					<input id="reassess-temperature" data-testid="temperature" type="text" bind:value={reassessForm.temperature} />
				</div>
				<div class="field">
					<label for="reassess-oxygen-saturation">Oxygen Saturation</label>
					<input id="reassess-oxygen-saturation" data-testid="oxygen-saturation" type="text" bind:value={reassessForm.oxygenSaturation} />
				</div>
				<div class="field">
					<label for="reassess-chief-complaint">Chief Complaint</label>
					<input id="reassess-chief-complaint" data-testid="chief-complaint" type="text" bind:value={reassessForm.chiefComplaint} />
				</div>
				<div class="field">
					<label for="reassess-pain-scale">Pain Scale</label>
					<input id="reassess-pain-scale" data-testid="pain-scale" type="text" bind:value={reassessForm.painScale} />
				</div>
			</div>

			<Button type="submit" class="btn" data-testid="submit-reassessment-form">
				Submit reassessment
			</Button>
		</form>

		{#if reassessOutcome}
			<div class="card" data-type="warning" role="status">
				<p>ESI Score: <strong data-testid="esi-score">{reassessOutcome.score}</strong></p>
				<p data-testid="triage-level">{reassessOutcome.level}</p>
				<p data-testid="queue-position">{reassessOutcome.queuePosition}</p>
				<p data-testid="escalation-alert">{reassessOutcome.escalationAlert}</p>
				<p data-testid="condition-change-note">{reassessOutcome.conditionChangeNote}</p>
			</div>
		{/if}
	{/if}

	{#if bulkQueue.length > 0}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Triage queue</h2>
			<ul>
				{#each bulkQueue as entry, i (entry.name)}
					<li data-testid="triage-queue-entry">{i + 1}. {entry.name} — {entry.level} (ESI {entry.esi})</li>
				{/each}
			</ul>
			<p data-testid="wait-time-calculation-status">{waitTimeStatus}</p>
		</div>
	{/if}
</div>
