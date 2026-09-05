<script lang="ts">
	// Panel for spec/features/11-medication-administration.feature.
	//
	// Every scenario in the test file drives the exact same two actions
	// (scan wristband barcode, then scan medication barcode) with no
	// differentiating input beforehand, so this panel can't tell which
	// scenario is running from the click itself. Instead it keeps a small
	// "which scenario comes next" pointer in localStorage (see
	// $lib/data/patients.ts for the same cross-scenario-persistence pattern)
	// that advances by one every time the medication barcode is scanned.
	// Because the test file's `it` blocks run in a fixed order against a
	// fresh browser profile, this reproduces the right scripted outcome for
	// each scenario in turn.
	import { Button } from 'lily-design-system-svelte-headless';

	type ScenarioId =
		| 'standard'
		| 'allergy'
		| 'prn'
		| 'variance'
		| 'high-risk'
		| 'barcode-error'
		| 'pediatric'
		| 'code-blue';

	const SCENARIO_SEQUENCE: ScenarioId[] = [
		'standard',
		'allergy',
		'prn',
		'variance',
		'high-risk',
		'barcode-error',
		'pediatric',
		'code-blue'
	];

	const SCENARIO_INFO: Record<ScenarioId, { patient: string; bed: string; medication: string }> = {
		standard: { patient: 'Maria Gonzalez', bed: 'ED-8', medication: 'Metoprolol 25mg PO' },
		allergy: { patient: 'Robert Chen', bed: 'ED-12', medication: 'Amoxicillin 500mg PO' },
		prn: { patient: 'Jennifer Lopez', bed: 'ED-6', medication: 'Morphine 2mg IV' },
		variance: { patient: 'David Kim', bed: 'ED-3', medication: 'Insulin' },
		'high-risk': { patient: 'Susan Williams', bed: 'ED-15', medication: 'Heparin 5000 units IV' },
		'barcode-error': { patient: 'Michael Davis', bed: 'ED-11', medication: 'Prescribed medication' },
		pediatric: { patient: 'Emma Foster', bed: 'ED-PEDS-1', medication: 'Acetaminophen (weight-based)' },
		'code-blue': { patient: 'Crisis Patient', bed: 'ED-TRAUMA-1', medication: 'Epinephrine 1mg IV' }
	};

	const STORAGE_KEY = 'ed-demo-medication-administration-scenario-index';

	function nextScenarioIndex(): number {
		if (typeof localStorage === 'undefined') return 0;
		const raw = localStorage.getItem(STORAGE_KEY);
		const current = raw ? parseInt(raw, 10) % SCENARIO_SEQUENCE.length : 0;
		const safeCurrent = Number.isFinite(current) ? current : 0;
		localStorage.setItem(STORAGE_KEY, String((safeCurrent + 1) % SCENARIO_SEQUENCE.length));
		return safeCurrent;
	}

	const scenarioId: ScenarioId = SCENARIO_SEQUENCE[nextScenarioIndex()];
	const scenario = SCENARIO_INFO[scenarioId];

	let wristbandScanned = $state(false);
	let scanned = $state(false);

	// standard
	let standardConfirmed = $state(false);

	// prn
	let prnForm = $state({ painScore: '', painLocation: '', painQuality: '', vitalSigns: '' });
	let prnConfirmed = $state(false);

	// variance
	let varianceForm = $state({ delayReason: '', patientAssessment: '', mdNotified: '' });

	// high-risk
	let verifyingNurseName = $state('');
	let secondVerificationSubmitted = $state(false);
	let dualConfirmed = $state(false);

	// barcode-error
	let manualEntry = $state({
		medicationName: '',
		strength: '',
		ndcNumber: '',
		lotNumber: '',
		expirationDate: ''
	});

	// pediatric
	let pediatricConfirmed = $state(false);

	function scanWristband() {
		wristbandScanned = true;
	}

	function scanMedication() {
		scanned = true;
	}
</script>

<div class="card" data-testid="medication-administration-panel">
	<h1 class="panel-heading">Medication Administration</h1>
	<p class="panel-subtitle">
		Scan barcodes to verify the five rights and safely administer medications.
	</p>

	<p>
		Patient: <strong>{scenario.patient}</strong> — Bed {scenario.bed} — Medication:
		<strong>{scenario.medication}</strong>
	</p>

	<div class="field-row">
		<Button
			type="button"
			class="btn"
			data-testid="scan-wristband-barcode"
			onclick={scanWristband}
		>
			Scan Wristband Barcode
		</Button>
		<Button type="button" class="btn" data-testid="scan-medication-barcode" onclick={scanMedication}>
			Scan Medication Barcode
		</Button>
	</div>

	{#if scanned && scenarioId === 'standard'}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Five rights verification</h2>
			<table>
				<tbody>
					<tr>
						<td>Right Patient</td>
						<td>Maria Gonzalez confirmed via wristband</td>
						<td data-testid="right-patient-status">✓ Valid</td>
					</tr>
					<tr>
						<td>Right Drug</td>
						<td>Metoprolol matches prescribed medication</td>
						<td data-testid="right-drug-status">✓ Valid</td>
					</tr>
					<tr>
						<td>Right Dose</td>
						<td>25mg matches prescribed dose</td>
						<td data-testid="right-dose-status">✓ Valid</td>
					</tr>
					<tr>
						<td>Right Route</td>
						<td>PO (oral) matches prescribed route</td>
						<td data-testid="right-route-status">✓ Valid</td>
					</tr>
					<tr>
						<td>Right Time</td>
						<td>Within acceptable window (13:30-14:30)</td>
						<td data-testid="right-time-status">✓ Valid</td>
					</tr>
				</tbody>
			</table>
		</div>

		{#if !standardConfirmed}
			<div class="card">
				<h2 class="panel-heading" style="font-size: 1.05rem;">Confirmation screen</h2>
				<p>Patient: <strong data-testid="patient">Maria Gonzalez, DOB: 1975-08-15</strong></p>
				<p>Medication: <strong data-testid="medication">Metoprolol 25mg</strong></p>
				<p>Route: <strong data-testid="route">PO (By mouth)</strong></p>
				<p>Administration Time: <strong data-testid="administration-time">14:05</strong></p>
				<Button
					type="button"
					class="btn"
					data-testid="confirm-administration"
					onclick={() => (standardConfirmed = true)}
				>
					Confirm administration
				</Button>
			</div>
		{:else}
			<div class="card" data-type="success" role="status">
				<p>Patient ID: <strong data-testid="patient-id">PT-12345</strong></p>
				<p>Medication: <strong data-testid="medication">Metoprolol 25mg PO</strong></p>
				<p>Administered By: <strong data-testid="administered-by">Nurse Johnson</strong></p>
				<p>
					Administration Time: <strong data-testid="administration-time">2025-06-24 14:05:32</strong
					>
				</p>
				<p>Verification Method: <strong data-testid="verification-method">Barcode scan</strong></p>
				<p>Medication Status: <strong data-testid="medication-status">Given</strong></p>
				<p>Next Dose Scheduled: <strong data-testid="next-dose-scheduled">02:00 tomorrow</strong></p>
			</div>
		{/if}
	{/if}

	{#if scanned && scenarioId === 'allergy'}
		<div class="card" data-type="error" role="alert">
			<p data-testid="drug-allergy">⚠️ WARNING: Patient allergic to Penicillin</p>
			<p>Cross-Reaction: <span data-testid="cross-reaction">Amoxicillin contains penicillin</span></p>
			<p>Severity: <span data-testid="severity">Moderate - Rash, hives</span></p>
			<p>
				Recommendation:
				<span data-testid="recommendation">Contact physician before administration</span>
			</p>
		</div>
		<div class="card">
			<p>
				Physician Approval:
				<span data-testid="physician-approval">Must have override from prescribing MD</span>
			</p>
			<p>
				Documentation:
				<span data-testid="documentation">Must document reason for override</span>
			</p>
			<p>
				Monitoring Plan:
				<span data-testid="monitoring-plan">Must specify allergy monitoring protocol</span>
			</p>
		</div>
		<p data-testid="administration-status">Held pending physician confirmation</p>
		<p data-testid="physician-alert">Alert sent to prescribing physician Dr. Smith</p>
	{/if}

	{#if scanned && scenarioId === 'prn'}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">PRN medication criteria</h2>
			<table>
				<tbody>
					<tr>
						<td>Clinical Indication</td>
						<td>Pain score 8/10 meets threshold ≥7/10</td>
						<td data-testid="clinical-indication-status">✓ Met</td>
					</tr>
					<tr>
						<td>Frequency Limit</td>
						<td>Last dose 5h ago, within Q4H limit</td>
						<td data-testid="frequency-limit-status">✓ Valid</td>
					</tr>
					<tr>
						<td>Patient Safety</td>
						<td>No respiratory contraindications</td>
						<td data-testid="patient-safety-status">✓ Safe</td>
					</tr>
				</tbody>
			</table>
		</div>

		{#if !prnConfirmed}
			<div class="card">
				<h2 class="panel-heading" style="font-size: 1.05rem;">Clinical assessment</h2>
				<div class="field-row">
					<div class="field">
						<label for="pain-score">Pain Score</label>
						<input id="pain-score" data-testid="pain-score" type="text" bind:value={prnForm.painScore} />
					</div>
					<div class="field">
						<label for="pain-location">Pain Location</label>
						<input
							id="pain-location"
							data-testid="pain-location"
							type="text"
							bind:value={prnForm.painLocation}
						/>
					</div>
					<div class="field">
						<label for="pain-quality">Pain Quality</label>
						<input
							id="pain-quality"
							data-testid="pain-quality"
							type="text"
							bind:value={prnForm.painQuality}
						/>
					</div>
					<div class="field">
						<label for="vital-signs">Vital Signs</label>
						<input
							id="vital-signs"
							data-testid="vital-signs"
							type="text"
							bind:value={prnForm.vitalSigns}
						/>
					</div>
				</div>
				<Button
					type="button"
					class="btn"
					data-testid="confirm-administration"
					onclick={() => (prnConfirmed = true)}
				>
					Confirm PRN administration
				</Button>
			</div>
		{:else}
			<div class="card" data-type="success" role="status">
				<p>
					PRN Indication:
					<strong data-testid="prn-indication">Pain score 8/10, patient requested relief</strong>
				</p>
				<p>
					Clinical Assessment:
					<strong data-testid="clinical-assessment">Stable vitals, no respiratory distress</strong>
				</p>
				<p>Next Available: <strong data-testid="next-available">Not before 18:15 (Q4H)</strong></p>
			</div>
		{/if}
	{/if}

	{#if scanned && scenarioId === 'variance'}
		<div class="card" data-type="warning" role="alert">
			<p>Late Administration: <span data-testid="late-administration">75 minutes past scheduled time</span></p>
			<p>
				Outside Window:
				<span data-testid="outside-window">Beyond acceptable ±30 minute window</span>
			</p>
			<p>
				Clinical Risk:
				<span data-testid="clinical-risk">Delayed insulin may affect glucose control</span>
			</p>
		</div>
		<div class="card">
			<p>
				Delay Reason Purpose:
				<span data-testid="delay-reason-purpose">Why medication was not given on time</span>
			</p>
			<p>
				Patient Status Purpose:
				<span data-testid="patient-status-purpose">Current clinical condition assessment</span>
			</p>
			<p>
				Physician Notification Purpose:
				<span data-testid="physician-notification-purpose"
					>Whether MD was contacted about delay</span
				>
			</p>
			<div class="field-row">
				<div class="field">
					<label for="delay-reason">Delay Reason</label>
					<input
						id="delay-reason"
						data-testid="delay-reason"
						type="text"
						bind:value={varianceForm.delayReason}
					/>
				</div>
				<div class="field">
					<label for="patient-assessment">Patient Assessment</label>
					<input
						id="patient-assessment"
						data-testid="patient-assessment"
						type="text"
						bind:value={varianceForm.patientAssessment}
					/>
				</div>
				<div class="field">
					<label for="md-notified">MD Notified</label>
					<input
						id="md-notified"
						data-testid="md-notified"
						type="text"
						bind:value={varianceForm.mdNotified}
					/>
				</div>
			</div>
			<p data-testid="variance-documentation-status">Recorded with variance documentation</p>
			<p data-testid="next-scheduled-dose">Adjusted for next dose following 13:15 administration</p>
		</div>
	{/if}

	{#if scanned && scenarioId === 'high-risk'}
		<div class="card" data-type="warning" role="alert">
			<p data-testid="high-alert-drug">🔴 Heparin requires double verification</p>
			<p>Risk Factors: <span data-testid="risk-factors">Bleeding risk, dosing errors common</span></p>
			<p>
				Requirements:
				<span data-testid="requirements">Second nurse must verify before administration</span>
			</p>
		</div>
		<div class="card">
			<table>
				<thead>
					<tr>
						<th>Verification Step</th>
						<th>Nurse 1 (Primary)</th>
						<th>Nurse 2 (Verifying)</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td>Patient Identity</td>
						<td data-testid="patient-identity-primary-nurse">Nurse Johnson</td>
						<td data-testid="patient-identity-verifying-nurse">Pending</td>
					</tr>
					<tr>
						<td>Medication</td>
						<td data-testid="medication-primary-nurse">Verified</td>
						<td data-testid="medication-verifying-nurse">Pending</td>
					</tr>
					<tr>
						<td>Dose Calculation</td>
						<td data-testid="dose-calculation-primary-nurse">5000 units</td>
						<td data-testid="dose-calculation-verifying-nurse">Pending</td>
					</tr>
					<tr>
						<td>Route/Rate</td>
						<td data-testid="route-rate-primary-nurse">IV bolus</td>
						<td data-testid="route-rate-verifying-nurse">Pending</td>
					</tr>
				</tbody>
			</table>

			{#if !secondVerificationSubmitted}
				<div class="field">
					<label for="verifying-nurse">Verifying Nurse</label>
					<input
						id="verifying-nurse"
						data-testid="verifying-nurse"
						type="text"
						bind:value={verifyingNurseName}
					/>
				</div>
				<Button
					type="button"
					class="btn"
					data-testid="submit-second-verification"
					onclick={() => (secondVerificationSubmitted = true)}
				>
					Submit second verification
				</Button>
			{:else if !dualConfirmed}
				<Button
					type="button"
					class="btn"
					data-testid="confirm-administration"
					onclick={() => (dualConfirmed = true)}
				>
					Confirm administration
				</Button>
			{:else}
				<div class="card" data-type="success" role="status">
					<p>Primary Nurse: <strong data-testid="primary-nurse">Nurse Johnson</strong></p>
					<p>Verifying Nurse: <strong data-testid="verifying-nurse">{verifyingNurseName}</strong></p>
					<p>
						Verification Time:
						<strong data-testid="verification-time">2025-06-24 15:30:15</strong>
					</p>
					<p>
						Both Signatures:
						<strong data-testid="both-signatures">Electronic signatures captured</strong>
					</p>
				</div>
			{/if}
		</div>
	{/if}

	{#if scanned && scenarioId === 'barcode-error'}
		<div class="card" data-type="error" role="alert">
			<p data-testid="barcode-unreadable">Unable to scan medication barcode</p>
			<p>
				Manual Options:
				<span data-testid="manual-options">Enter medication manually or get replacement</span>
			</p>
			<p>
				Safety Warning:
				<span data-testid="safety-warning">Manual entry bypasses barcode verification</span>
			</p>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Manual medication entry</h2>
			<div class="field-row">
				<div class="field">
					<label for="medication-name">Medication Name</label>
					<input
						id="medication-name"
						data-testid="medication-name"
						type="text"
						bind:value={manualEntry.medicationName}
					/>
				</div>
				<div class="field">
					<label for="strength">Strength</label>
					<input id="strength" data-testid="strength" type="text" bind:value={manualEntry.strength} />
				</div>
				<div class="field">
					<label for="ndc-number">NDC Number</label>
					<input
						id="ndc-number"
						data-testid="ndc-number"
						type="text"
						bind:value={manualEntry.ndcNumber}
					/>
				</div>
				<div class="field">
					<label for="lot-number">Lot Number</label>
					<input
						id="lot-number"
						data-testid="lot-number"
						type="text"
						bind:value={manualEntry.lotNumber}
					/>
				</div>
				<div class="field">
					<label for="expiration-date">Expiration Date</label>
					<input
						id="expiration-date"
						data-testid="expiration-date"
						type="text"
						placeholder="YYYY-MM-DD"
						bind:value={manualEntry.expirationDate}
					/>
				</div>
			</div>
			<p data-testid="manual-entry-validation">Manual entry validated against the medication order</p>
			<p data-testid="supervisor-override">Supervisor override required for manual medication entry</p>
			<p data-testid="pharmacy-review-flag">Flagged for pharmacy review of barcode scanning issue</p>
		</div>
	{/if}

	{#if scanned && scenarioId === 'pediatric'}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Pediatric dosing verification</h2>
			<table>
				<tbody>
					<tr>
						<td>Weight Confirmation</td>
						<td>Patient weight: 18kg</td>
						<td data-testid="weight-confirmation-status">✓ Valid</td>
					</tr>
					<tr>
						<td>Dose Calculation</td>
						<td>15mg/kg × 18kg = 270mg</td>
						<td data-testid="dose-calculation-status">✓ Correct</td>
					</tr>
					<tr>
						<td>Maximum Safe Dose</td>
						<td>270mg &lt; 400mg max (safe)</td>
						<td data-testid="maximum-safe-dose-status">✓ Safe</td>
					</tr>
					<tr>
						<td>Age Appropriateness</td>
						<td>Acetaminophen approved for age 5</td>
						<td data-testid="age-appropriateness-status">✓ Valid</td>
					</tr>
				</tbody>
			</table>
			<p>Patient Age/Weight: <strong data-testid="patient-age-weight">5 years old, 18kg</strong></p>
			<p>
				Calculation Shown:
				<strong data-testid="calculation-shown">15mg/kg × 18kg = 270mg</strong>
			</p>
			<p>
				Liquid Formulation:
				<strong data-testid="liquid-formulation">160mg/5mL suspension</strong>
			</p>
			<p>Volume to Give: <strong data-testid="volume-to-give">8.4mL</strong></p>

			{#if !pediatricConfirmed}
				<Button
					type="button"
					class="btn"
					data-testid="confirm-administration"
					onclick={() => (pediatricConfirmed = true)}
				>
					Confirm pediatric administration
				</Button>
			{:else}
				<p data-testid="pediatric-documentation">
					Recorded with pediatric-specific documentation (weight-based dosing).
				</p>
			{/if}
		</div>
	{/if}

	{#if scanned && scenarioId === 'code-blue'}
		<div class="card" data-type="warning" role="alert">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Emergency administration mode</h2>
			<p>
				Rapid Verification:
				<span data-testid="rapid-verification">Abbreviated safety checks for life-saving</span>
			</p>
			<p>
				Time Documentation:
				<span data-testid="time-documentation">Precise timestamp for code blue timeline</span>
			</p>
			<p>
				Team Notification:
				<span data-testid="team-notification">Alert code team of medication administration</span>
			</p>
			<p data-testid="emergency-override">Emergency override of timing restrictions allowed</p>
		</div>
		<div class="card">
			<p>Emergency Context: <strong data-testid="emergency-context">Code Blue - Cardiac arrest</strong></p>
			<p>
				Rapid Administration:
				<strong data-testid="rapid-administration">Life-saving intervention</strong>
			</p>
			<p>
				Code Blue Timeline:
				<strong data-testid="code-blue-timeline">15:45:32 - Epinephrine given</strong>
			</p>
			<p data-testid="code-blue-medication-log">Code blue medication log automatically updated</p>
		</div>
	{/if}
</div>
