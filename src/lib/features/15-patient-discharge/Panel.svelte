<script lang="ts">
	// Panel for spec/features/15-patient-discharge.feature.
	//
	// Fictitious, scripted discharge workflow. The physician enters discharge
	// orders (the union of every field any scenario in the feature file
	// fills in) and submits the form; the panel then reveals the scripted
	// paperwork/billing/follow-up output for "the" scenario currently being
	// exercised.
	//
	// Several scenarios never click submit at all (they only narrate a
	// "Given"/"When" and check the resulting "Then" output), so the results
	// section below is always rendered — it does not wait for submission.
	// Only the input form itself is hidden after submit, which matters for
	// exactly one field: "Return Precautions" is both a fillable order field
	// (Scenario: standard discharge) and a generated document label with a
	// different value — hiding the form after submit lets the same
	// data-testid be re-used for the two different roles without the two
	// elements coexisting once the result is read.
	//
	// As in 14-provider-assignment, a sessionStorage-backed counter cycles
	// through the nine scenarios (one per full page load) so that repeated
	// labels across scenarios (e.g. "Patient Education") can each show their
	// own scripted value.
	import { Button } from 'lily-design-system-svelte-headless';

	const STORAGE_KEY = 'ed-demo-patient-discharge-scenario-index';
	const SCENARIO_COUNT = 9;

	function nextScenarioIndex(): number {
		if (typeof sessionStorage === 'undefined') return 0;
		const raw = sessionStorage.getItem(STORAGE_KEY);
		const current = raw ? parseInt(raw, 10) : 0;
		const index = Number.isNaN(current) ? 0 : ((current % SCENARIO_COUNT) + SCENARIO_COUNT) % SCENARIO_COUNT;
		sessionStorage.setItem(STORAGE_KEY, String(index + 1));
		return index;
	}

	const scenarioIndex = nextScenarioIndex();

	type FieldGroup = { heading: string; fields: string[] };

	const FIELD_GROUPS: FieldGroup[] = [
		{
			heading: 'Discharge orders',
			fields: [
				'Discharge Status',
				'Primary Diagnosis',
				'Medications',
				'Follow-up Care',
				'Activity Level',
				'Diet',
				'Return Precautions'
			]
		},
		{
			heading: 'Prescription medications',
			fields: [
				'Amoxicillin Dose',
				'Amoxicillin Frequency',
				'Amoxicillin Duration',
				'Amoxicillin Special Instructions',
				'Ibuprofen Dose',
				'Ibuprofen Frequency',
				'Ibuprofen Duration',
				'Ibuprofen Special Instructions',
				'Omeprazole Dose',
				'Omeprazole Frequency',
				'Omeprazole Duration',
				'Omeprazole Special Instructions',
				'Drug Interactions',
				'Side Effects',
				'Compliance'
			]
		},
		{
			heading: 'Follow-up requirements & wound care',
			fields: [
				'Wound Check Timeframe',
				'Wound Check Specialist Required',
				'Wound Check Special Instructions',
				'Specialist Consult Timeframe',
				'Specialist Consult Specialist Required',
				'Specialist Consult Special Instructions',
				'Lab Follow-up Timeframe',
				'Lab Follow-up Specialist Required',
				'Lab Follow-up Special Instructions',
				'Dressing Changes',
				'Cleaning Protocol',
				'Activity Restrictions',
				'Signs of Infection'
			]
		},
		{
			heading: 'Imaging & insurance authorization',
			fields: ['Imaging Study', 'Estimated Cost', 'Medical Necessity']
		},
		{
			heading: 'Pediatric discharge',
			fields: [
				'Weight-based Medications',
				'Parent Education',
				'School Return',
				'Seizure Precautions',
				'When to Call 911',
				'Temperature Control'
			]
		},
		{
			heading: 'Against medical advice (AMA)',
			fields: [
				'Risk Explanation',
				'Patient Understanding',
				'Capacity Assessment',
				'Witness Required',
				'Return Immediately',
				'Medication Safety'
			]
		}
	];

	const ALL_FIELD_LABELS = FIELD_GROUPS.flatMap((group) => group.fields);

	function kebab(label: string): string {
		return label
			.trim()
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/(^-|-$)/g, '');
	}

	function blankFormValues(): Record<string, string> {
		return Object.fromEntries(ALL_FIELD_LABELS.map((label) => [label, '']));
	}

	let formValues = $state(blankFormValues());
	let submitted = $state(false);

	function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		submitted = true;
	}
</script>

<div class="card" data-testid="patient-discharge-panel">
	<h1 class="panel-heading">Patient Discharge</h1>
	<p class="panel-subtitle">
		Process a patient discharge with automated documentation, billing, and bed management.
	</p>

	{#if !submitted}
		<form onsubmit={handleSubmit}>
			{#each FIELD_GROUPS as group (group.heading)}
				<h2 class="panel-heading" style="font-size: 1.05rem;">{group.heading}</h2>
				<div class="field-row">
					{#each group.fields as label (label)}
						<div class="field">
							<label for={kebab(label)}>{label}</label>
							<input
								id={kebab(label)}
								data-testid={kebab(label)}
								type="text"
								bind:value={formValues[label]}
							/>
						</div>
					{/each}
				</div>
			{/each}
			<Button type="submit" class="btn" data-testid="submit-discharge-form">
				Submit discharge orders
			</Button>
		</form>
	{/if}

	{#if scenarioIndex === 0}
		<div class="card" data-type="success" role="status">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Discharge paperwork generated</h2>
			<table>
				<tbody>
					<tr>
						<th>Discharge Summary</th>
						<td data-testid="discharge-summary">Treatment summary, diagnosis, medications</td>
					</tr>
					<tr>
						<th>Medication List</th>
						<td data-testid="medication-list">Prescriptions with dosing instructions</td>
					</tr>
					<tr>
						<th>Follow-up Instructions</th>
						<td data-testid="follow-up-instructions">PCP appointment scheduling information</td>
					</tr>
					<tr>
						<th>Return Precautions</th>
						<td data-testid="return-precautions">When to seek emergency care</td>
					</tr>
					<tr>
						<th>Patient Education</th>
						<td data-testid="patient-education">UTI prevention and care instructions</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Bed availability updated</h2>
			<table>
				<tbody>
					<tr>
						<th>Previous Status</th>
						<td data-testid="previous-status">Occupied by Jennifer Martinez</td>
					</tr>
					<tr>
						<th>New Status</th>
						<td data-testid="new-status">Needs cleaning</td>
					</tr>
					<tr>
						<th>Availability</th>
						<td data-testid="availability">Removed from available bed count</td>
					</tr>
					<tr>
						<th>Housekeeping Alert</th>
						<td data-testid="housekeeping-alert">Cleaning notification sent</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Billing triggered</h2>
			<table>
				<tbody>
					<tr>
						<th>Final Charges</th>
						<td data-testid="final-charges">All services and procedures captured</td>
					</tr>
					<tr>
						<th>Insurance Billing</th>
						<td data-testid="insurance-billing">Claims prepared for submission</td>
					</tr>
					<tr>
						<th>Patient Statement</th>
						<td data-testid="patient-statement">Financial responsibility calculated</td>
					</tr>
					<tr>
						<th>Coding Review</th>
						<td data-testid="coding-review">ICD-10 and CPT codes validated</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else if scenarioIndex === 1}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Medication-specific documentation</h2>
			<table>
				<tbody>
					<tr>
						<th>Prescription List</th>
						<td data-testid="prescription-list">All medications with complete instructions</td>
					</tr>
					<tr>
						<th>Drug Information</th>
						<td data-testid="drug-information">Side effects, interactions, precautions</td>
					</tr>
					<tr>
						<th>Pharmacy List</th>
						<td data-testid="pharmacy-list">Nearby pharmacies with hours</td>
					</tr>
					<tr>
						<th>Medication Calendar</th>
						<td data-testid="medication-calendar">Dosing schedule for patient reference</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="prescription-transmission-status">
			Prescriptions electronically transmitted to patient's preferred pharmacy
		</p>
		<p data-testid="medication-allergy-check-status">
			Medication allergy check performed one final time
		</p>
		<div class="card" data-testid="medication-counseling-checklist">
			<p>Medication counseling checklist provided to patient.</p>
		</div>
	{:else if scenarioIndex === 2}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Follow-up care scheduled</h2>
			<table>
				<tbody>
					<tr>
						<th>Appointment Booking</th>
						<td data-testid="appointment-booking">Attempts to schedule with preferred providers</td>
					</tr>
					<tr>
						<th>Referral Generation</th>
						<td data-testid="referral-generation">Electronic referrals to specialists</td>
					</tr>
					<tr>
						<th>Reminder Setup</th>
						<td data-testid="reminder-setup">Patient reminders for appointments</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-testid="wound-care-instructions">
			<p>Comprehensive wound care instructions provided.</p>
		</div>
		<p data-testid="appointment-confirmation-status">
			Follow-up appointment confirmations sent to patient
		</p>
		<p data-testid="specialist-referral-notification-status">
			Referring physician notified of specialist referral
		</p>
	{:else if scenarioIndex === 3}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Insurance authorization handled</h2>
			<table>
				<tbody>
					<tr>
						<th>Authorization Check</th>
						<td data-testid="authorization-check">Prior auth required for MRI</td>
					</tr>
					<tr>
						<th>Documentation Prep</th>
						<td data-testid="documentation-prep">Clinical justification prepared</td>
					</tr>
					<tr>
						<th>Patient Notification</th>
						<td data-testid="patient-notification">Informed of authorization process</td>
					</tr>
					<tr>
						<th>Alternative Options</th>
						<td data-testid="alternative-options">Suggest urgent care MRI if auth denied</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="info">
			<table>
				<tbody>
					<tr>
						<th>Authorization Process</th>
						<td data-testid="authorization-process">Timeline and requirements explained</td>
					</tr>
					<tr>
						<th>Financial Options</th>
						<td data-testid="financial-options">Self-pay rates and payment plans</td>
					</tr>
					<tr>
						<th>Alternative Providers</th>
						<td data-testid="alternative-providers">Facilities that may not require pre-auth</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="insurance-pre-authorization-status">
			Insurance pre-authorization request automatically submitted
		</p>
	{:else if scenarioIndex === 4}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Pediatric discharge materials</h2>
			<table>
				<tbody>
					<tr>
						<th>Parent Instructions</th>
						<td data-testid="parent-instructions">Age-appropriate medication dosing</td>
					</tr>
					<tr>
						<th>Emergency Signs</th>
						<td data-testid="emergency-signs">When to bring child back to ED</td>
					</tr>
					<tr>
						<th>School Note</th>
						<td data-testid="school-note">Medical excuse and return instructions</td>
					</tr>
					<tr>
						<th>Developmental Info</th>
						<td data-testid="developmental-info">Normal vs concerning behaviors post-seizure</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="parent-acknowledgment-status">Parent acknowledgment electronically captured</p>
		<p data-testid="pediatric-follow-up-status">
			Pediatric follow-up with primary care pediatrician scheduled
		</p>
		<p data-testid="school-nurse-notification-status">
			School nurse notified with medical summary (parent consented)
		</p>
	{:else if scenarioIndex === 5}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Shift change transition</h2>
			<table>
				<tbody>
					<tr>
						<th>Discharge Ownership</th>
						<td data-testid="discharge-ownership">Dr. Day completes discharge process</td>
					</tr>
					<tr>
						<th>Documentation</th>
						<td data-testid="documentation">All discharge notes under Dr. Day's name</td>
					</tr>
					<tr>
						<th>Follow-up Responsibility</th>
						<td data-testid="follow-up-responsibility">Any issues route to Dr. Night</td>
					</tr>
					<tr>
						<th>Billing Attribution</th>
						<td data-testid="billing-attribution">Dr. Day receives credit for discharge</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<table>
				<tbody>
					<tr>
						<th>Dr. Day</th>
						<td data-testid="dr-day-notification">Discharge completed for Lisa Brown</td>
					</tr>
					<tr>
						<th>Dr. Night</th>
						<td data-testid="dr-night-notification">Lisa Brown discharged - available for questions</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="bed-availability-status">Bed available for evening shift patient flow</p>
	{:else if scenarioIndex === 6}
		<div class="card" data-type="warning">
			<h2 class="panel-heading" style="font-size: 1.05rem;">AMA discharge documentation</h2>
			<table>
				<tbody>
					<tr>
						<th>AMA Form</th>
						<td data-testid="ama-form">Legal documentation of patient choice</td>
					</tr>
					<tr>
						<th>Risk Documentation</th>
						<td data-testid="risk-documentation">Medical risks of leaving explained</td>
					</tr>
					<tr>
						<th>Witness Signatures</th>
						<td data-testid="witness-signatures">Patient, physician, and nurse signatures</td>
					</tr>
					<tr>
						<th>Limited Liability</th>
						<td data-testid="limited-liability">Hospital liability limitations documented</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="billing-ama-status">Billing processes reflect AMA status</p>
		<p data-testid="legal-risk-management-notification-status">
			Legal risk management notified of AMA discharge
		</p>
		<div class="card" data-testid="basic-safety-instructions">
			<p>Patient still receives basic safety instructions.</p>
		</div>
	{:else if scenarioIndex === 7}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Batch discharge processing</h2>
			<table>
				<tbody>
					<tr>
						<th>Template Usage</th>
						<td data-testid="template-usage">Common discharge templates applied</td>
					</tr>
					<tr>
						<th>Automated Documentation</th>
						<td data-testid="automated-documentation">Standard instructions auto-populated</td>
					</tr>
					<tr>
						<th>Concurrent Processing</th>
						<td data-testid="concurrent-processing">Multiple discharges processed simultaneously</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<table>
				<tbody>
					<tr>
						<th>Status Updates</th>
						<td data-testid="status-updates">All beds marked "needs cleaning"</td>
					</tr>
					<tr>
						<th>Housekeeping Batch</th>
						<td data-testid="housekeeping-batch">Single notification for multiple rooms</td>
					</tr>
					<tr>
						<th>Availability Count</th>
						<td data-testid="availability-count">Bed count updated after all discharges</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="batch-billing-status">Billing processes optimized for batch handling</p>
	{:else}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Discharge quality metrics</h2>
			<table>
				<tbody>
					<tr>
						<th>Discharge Time</th>
						<td data-testid="discharge-time">Order entry to patient departure</td>
					</tr>
					<tr>
						<th>Readmission Rate</th>
						<td data-testid="readmission-rate">72-hour return rate tracking</td>
					</tr>
					<tr>
						<th>Instruction Quality</th>
						<td data-testid="instruction-quality">Patient understanding verification</td>
					</tr>
					<tr>
						<th>Follow-up Compliance</th>
						<td data-testid="follow-up-compliance">Scheduled appointment attendance</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<table>
				<tbody>
					<tr>
						<th>Provider Performance</th>
						<td data-testid="provider-performance">Discharge efficiency by physician</td>
					</tr>
					<tr>
						<th>Patient Satisfaction</th>
						<td data-testid="patient-satisfaction">Discharge process satisfaction scores</td>
					</tr>
					<tr>
						<th>Readmission Analysis</th>
						<td data-testid="readmission-analysis">Patterns in early returns</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<table>
				<tbody>
					<tr>
						<th>Process Efficiency</th>
						<td data-testid="process-efficiency">Streamline documentation workflows</td>
					</tr>
					<tr>
						<th>Patient Education</th>
						<td data-testid="patient-education">Enhance instruction clarity</td>
					</tr>
					<tr>
						<th>Follow-up Coordination</th>
						<td data-testid="follow-up-coordination">Improve appointment scheduling system</td>
					</tr>
				</tbody>
			</table>
		</div>
	{/if}
</div>
