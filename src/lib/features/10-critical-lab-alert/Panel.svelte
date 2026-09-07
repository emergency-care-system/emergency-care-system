<script lang="ts">
	// Panel for tests-with-given-when-then-features/10-critical-lab-alert.feature.
	//
	// This is a fictitious, client-only critical value alert board. Unlike the
	// Lab Result Processing panel, each scenario here has its own distinct
	// button (and, for two scenarios, its own distinct field), so the active
	// scenario is chosen directly from which control was used — no persisted
	// occurrence counter is needed.
	import { Button } from 'lily-design-system-svelte-headless';

	type ScenarioKind =
		| 'standard'
		| 'acknowledgment'
		| 'multiple'
		| 'shift-change'
		| 'cardiac'
		| 'false-positive'
		| 'transfer'
		| 'validation'
		| null;

	let scenario = $state<ScenarioKind>(null);

	let testName = $state('');
	let result = $state('');
	let referenceRange = $state('');
	let units = $state('');
	let criticalThreshold = $state('');
	let status = $state('');
	let troponinResult = $state('');
	let sampleErrorReported = $state(false);
	let correctedTroponinResult = $state('');
	let testTroponinResult = $state('');

	function receiveLabResult() {
		// Scenario "shift change" fills the Troponin Result field before
		// clicking this button; the standard scenario fills Test Name/Result/
		// etc. instead and leaves Troponin Result empty.
		scenario = troponinResult.trim() ? 'shift-change' : 'standard';
	}

	function triggerCriticalAlert() {
		scenario = 'acknowledgment';
	}

	function receiveCriticalResults() {
		scenario = 'multiple';
	}

	function receiveCardiacMarkerResults() {
		scenario = 'cardiac';
	}

	function reportSampleError() {
		sampleErrorReported = true;
	}

	function submitCorrection() {
		scenario = 'false-positive';
	}

	function processCriticalAlert() {
		scenario = 'transfer';
	}

	function processTestResult() {
		scenario = 'validation';
	}
</script>

<div class="card" data-testid="critical-lab-alert-panel">
	<h1 class="panel-heading">Critical Lab Alert</h1>
	<p class="panel-subtitle">
		Immediate multi-channel alerts when a critical lab value is received, with acknowledgment
		tracking and escalation.
	</p>

	<div class="field-row">
		<div class="field">
			<label for="test-name">Test Name</label>
			<input id="test-name" data-testid="test-name" type="text" bind:value={testName} />
		</div>
		<div class="field">
			<label for="result">Result</label>
			<input id="result" data-testid="result" type="text" bind:value={result} />
		</div>
		<div class="field">
			<label for="reference-range">Reference Range</label>
			<input id="reference-range" data-testid="reference-range" type="text" bind:value={referenceRange} />
		</div>
		<div class="field">
			<label for="units">Units</label>
			<input id="units" data-testid="units" type="text" bind:value={units} />
		</div>
		<div class="field">
			<label for="critical-threshold">Critical Threshold</label>
			<input id="critical-threshold" data-testid="critical-threshold" type="text" bind:value={criticalThreshold} />
		</div>
		<div class="field">
			<label for="status">Status</label>
			<input id="status" data-testid="status" type="text" bind:value={status} />
		</div>
		<div class="field">
			<label for="troponin-result">Troponin Result</label>
			<input id="troponin-result" data-testid="troponin-result" type="text" bind:value={troponinResult} />
		</div>
		<div class="field">
			<label for="test-troponin-result">Test Troponin Result</label>
			<input
				id="test-troponin-result"
				data-testid="test-troponin-result"
				type="text"
				bind:value={testTroponinResult}
			/>
		</div>
	</div>

	<div class="field-row">
		<Button type="button" class="btn" data-testid="receive-lab-result-button" onclick={receiveLabResult}>
			Receive lab result
		</Button>
		<Button type="button" class="btn secondary" data-testid="trigger-critical-alert-button" onclick={triggerCriticalAlert}>
			Trigger critical alert
		</Button>
		<Button type="button" class="btn secondary" data-testid="receive-critical-results-button" onclick={receiveCriticalResults}>
			Receive multiple critical results
		</Button>
		<Button type="button" class="btn secondary" data-testid="receive-lab-results-button" onclick={receiveCardiacMarkerResults}>
			Receive cardiac marker results
		</Button>
		<Button type="button" class="btn secondary" data-testid="report-sample-error-button" onclick={reportSampleError}>
			Report sample error
		</Button>
		<Button type="button" class="btn secondary" data-testid="process-critical-alert-button" onclick={processCriticalAlert}>
			Process critical alert (transfer)
		</Button>
		<Button type="button" class="btn secondary" data-testid="process-test-result-button" onclick={processTestResult}>
			Process test result
		</Button>
	</div>

	{#if sampleErrorReported}
		<div class="field">
			<label for="corrected-troponin-result">Corrected Troponin Result</label>
			<input
				id="corrected-troponin-result"
				data-testid="corrected-troponin-result"
				type="text"
				bind:value={correctedTroponinResult}
			/>
		</div>
		<Button type="button" class="btn" data-testid="submit-correction-button" onclick={submitCorrection}>
			Submit correction
		</Button>
	{/if}

	{#if scenario === 'standard'}
		<div class="card" data-type="error" role="alert">
			<p data-testid="critical-alert-status">Critical alert triggered immediately.</p>
			<h3>Dr. Johnson notifications</h3>
			<p data-testid="mobile-push-alert">🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7</p>
			<p data-testid="sms-alert">CRITICAL LAB: R.Martinez ED-7 Troponin 5.8</p>
			<p data-testid="popup-alert">CRITICAL VALUE - Requires Acknowledgment</p>
			<h3>Nurse Williams notifications</h3>
			<p data-testid="desktop-alert">CRITICAL: Troponin 5.8 - Bed ED-7</p>
			<p data-testid="overhead-page">Critical lab value bed ED-7</p>
			<p data-testid="mobile-alert">Critical troponin result requires attention</p>
			<h3>Red flags on patient displays</h3>
			<ul>
				<li data-testid="red-flag-indicator">Patient Monitor — 🔴 CRITICAL LAB flashing red banner</li>
				<li data-testid="red-flag-indicator">Bedside Workstation — Red alert icon next to patient name</li>
				<li data-testid="red-flag-indicator">Main ED Dashboard — Red flag on bed ED-7 status</li>
				<li data-testid="red-flag-indicator">Mobile Devices — Red notification badge on patient chart</li>
				<li data-testid="red-flag-indicator">Nursing Station — Critical value alert on patient board</li>
			</ul>
		</div>
	{:else if scenario === 'acknowledgment'}
		<div class="card" data-type="warning" role="alert">
			<p data-testid="initial-alert">Must acknowledge receipt within 15 minutes</p>
			<p data-testid="clinical-review">Must document result review</p>
			<p data-testid="action-plan">Must indicate next steps taken</p>
			<h3>Escalation if unacknowledged</h3>
			<p data-testid="secondary-alert">Alert sent to backup physician</p>
			<p data-testid="charge-nurse-alert">Escalation notice to charge nurse</p>
			<p data-testid="supervisor-alert">Department supervisor notified</p>
			<ul>
				<li data-testid="acknowledgment-status-entry">Alert Sent — 14:30:15 — System — Initial notification</li>
				<li data-testid="acknowledgment-status-entry">Acknowledged — 14:32:45 — Dr. Lee — Acknowledged receipt</li>
				<li data-testid="acknowledgment-status-entry">Reviewed — 14:35:20 — Dr. Lee — Documented review</li>
				<li data-testid="acknowledgment-status-entry">Action Taken — 14:40:10 — Dr. Lee — Treatment initiated</li>
			</ul>
		</div>
	{:else if scenario === 'multiple'}
		<div class="card" data-type="error" role="alert">
			<p data-testid="priority-1-patient">Lisa Johnson</p>
			<p data-testid="priority-1-alert-level">Critical</p>
			<p data-testid="priority-2-patient">Mike Davis</p>
			<p data-testid="priority-2-alert-level">High</p>
			<p data-testid="priority-3-patient">John Williams</p>
			<p data-testid="priority-3-alert-level">High</p>
			<p data-testid="dr-brown-alert-summary">CRITICAL: Lisa Johnson Trop 8.9 - IMMEDIATE</p>
			<p data-testid="dr-adams-alert-summary">HIGH: 2 patients with elevated troponin</p>
			<p data-testid="mass-alert">3 critical troponin results requiring attention</p>
			<p data-testid="priority-list">Lisa Johnson (Critical), others (High)</p>
			<ul>
				<li data-testid="color-coded-flag">Lisa Johnson — Red (Critical)</li>
				<li data-testid="color-coded-flag">Mike Davis — Orange (High)</li>
				<li data-testid="color-coded-flag">John Williams — Orange (High)</li>
			</ul>
		</div>
	{:else if scenario === 'shift-change'}
		<div class="card" data-type="error" role="alert">
			<p data-testid="dr-taylor-alert">CRITICAL Troponin 6.1 - Your patient</p>
			<p data-testid="dr-wilson-alert">FYI: Critical result on your order</p>
			<p data-testid="shift-context">Critical result during physician handoff</p>
			<p data-testid="current-md">Dr. Taylor (assuming care)</p>
			<p data-testid="ordering-md">Dr. Wilson (ordered test)</p>
			<div data-testid="handoff-documentation">
				Handoff documentation updated: Catherine Brown, critical troponin 6.1 ng/mL.
			</div>
			<ul>
				<li data-testid="red-flag-indicator">Main ED Dashboard — Red flag with shift change context</li>
			</ul>
		</div>
	{:else if scenario === 'cardiac'}
		<div class="card" data-type="error" role="alert">
			<p data-testid="cardiac-panel">Multiple critical cardiac markers</p>
			<p data-testid="primary-alert">Troponin I: 4.7 ng/mL (CRITICAL)</p>
			<p data-testid="secondary-alert">CK-MB: 45 ng/mL (CRITICAL)</p>
			<p data-testid="supporting-data">Myoglobin: 280 ng/mL (Elevated)</p>
			<p data-testid="clinical-indication">Acute myocardial infarction likely</p>
			<p data-testid="recommended-actions">Cardiology consult, STEMI protocol</p>
			<p data-testid="time-sensitivity">Treatment within 90 minutes critical</p>
			<p data-testid="stemi-protocol-alert">STEMI protocol automatically triggered.</p>
		</div>
	{:else if scenario === 'false-positive'}
		<div class="card" data-type="success" role="status">
			<p data-testid="cancel-alert">Original critical alert is cancelled</p>
			<p data-testid="send-correction">Corrected value sent to all recipients</p>
			<p data-testid="document-error">Lab error documented in audit trail</p>
			<p data-testid="alert-cancellation">CANCELLED: Previous critical troponin alert</p>
			<p data-testid="corrected-value">Troponin corrected to 0.03 ng/mL (Normal)</p>
			<p data-testid="error-explanation">Laboratory sample contamination identified</p>
			<div data-testid="quality-assurance-log">
				QA log: Nancy Rodriguez troponin corrected from 5.1 ng/mL to 0.03 ng/mL.
			</div>
		</div>
	{:else if scenario === 'transfer'}
		<div class="card" data-type="warning" role="alert">
			<p data-testid="transfer-required">Patient needs immediate cardiac unit transfer</p>
			<p data-testid="bed-availability">CCU bed 302 available</p>
			<p data-testid="transport-time">Transport team ETA 10 minutes</p>
			<p data-testid="ccu-alert">Incoming transfer - Critical troponin 9.3</p>
			<p data-testid="cardiology-alert">Urgent consult needed - STEMI protocol</p>
			<div data-testid="transfer-documentation">
				Transfer documentation initiated for Timothy Chang to CCU bed 302.
			</div>
			<p data-testid="patient-alert-handoff">Critical alerts follow Timothy Chang to the receiving unit.</p>
		</div>
	{:else if scenario === 'validation'}
		<div class="card" data-type="info" role="status">
			<p data-testid="physician-mobile">Test alert delivered successfully</p>
			<p data-testid="charge-nurse">Test alert delivered successfully</p>
			<p data-testid="patient-displays">Red flags displayed correctly</p>
			<p data-testid="audit-trail">Test alert logged with timestamp</p>
			<p data-testid="test-alert-marking">SYSTEM TEST</p>
			<p data-testid="alert-latency">&lt;30 seconds from result to notification</p>
			<p data-testid="delivery-success">100% successful delivery to all recipients</p>
			<p data-testid="display-update">&lt;5 seconds to update all patient displays</p>
		</div>
	{/if}
</div>
