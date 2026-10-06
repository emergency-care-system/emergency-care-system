<script lang="ts">
	// Panel for tests-with-given-when-then-features/09-lab-result-processing.feature.
	//
	// This is a fictitious, client-only HL7 result inbox. Every scenario in the
	// feature file clicks a single "Receive Lab Results" button with no
	// distinguishing input, so a persisted (localStorage) occurrence counter
	// from $lib/data/lab-result-processing picks which canned scenario outcome
	// to render, in the same order the scenarios appear in the feature file.
	import { Button } from 'lily-design-system-svelte-headless';
	import { nextLabResultOccurrence } from '#lib/data/lab-result-processing.js';

	type OutcomeKind =
		| 'normal'
		| 'critical'
		| 'corrections'
		| 'pediatric'
		| 'handoff'
		| 'technical-failure'
		| 'batch'
		| 'interpretation';

	const SEQUENCE: OutcomeKind[] = [
		'normal',
		'critical',
		'corrections',
		'pediatric',
		'handoff',
		'technical-failure',
		'batch',
		'interpretation'
	];

	let outcomeKind = $state<OutcomeKind | null>(null);

	function receiveLabResults() {
		const occurrence = nextLabResultOccurrence();
		outcomeKind = SEQUENCE[occurrence % SEQUENCE.length];
	}
</script>

<div class="card" data-testid="lab-result-processing-panel">
	<h1 class="panel-heading">Lab Result Processing</h1>
	<p class="panel-subtitle">
		Results arriving over the HL7 interface are automatically processed, flagged for critical
		values, and distributed to the appropriate providers.
	</p>

	<Button type="button" class="btn" data-testid="receive-lab-results-button" onclick={receiveLabResults}>
		Receive lab results (HL7)
	</Button>

	{#if outcomeKind === 'normal'}
		<div class="card" data-type="success" role="status">
			<p data-testid="patient-record-update-status">Patient record updated with all results.</p>
			<p data-testid="result-status">Normal</p>
			<h3>Notification to Dr. Smith</h3>
			<p data-testid="lab-results">Normal CBC and BMP available for review</p>
			<p data-testid="patient">Jennifer Lopez, Bed ED-8</p>
			<p data-testid="timestamp">14:30</p>
			<p data-testid="priority">Standard</p>
			<ul>
				<li data-testid="timeline-result-entry">White Blood Cells 7.2 K/uL — Normal</li>
				<li data-testid="timeline-result-entry">Hemoglobin 13.5 g/dL — Normal</li>
				<li data-testid="timeline-result-entry">Sodium 140 mmol/L — Normal</li>
				<li data-testid="timeline-result-entry">Potassium 4.1 mmol/L — Normal</li>
				<li data-testid="timeline-result-entry">Creatinine 1.0 mg/dL — Normal</li>
			</ul>
			<p data-testid="nursing-results-notification">
				Nursing staff notified that Jennifer Lopez's results are available for review.
			</p>
		</div>
	{:else if outcomeKind === 'critical'}
		<div class="card" data-type="error" role="alert">
			<p data-testid="troponin-i-critical-flag">CRITICAL HIGH</p>
			<p data-testid="troponin-i-severity-level">Severe</p>
			<p data-testid="bnp-critical-flag">CRITICAL HIGH</p>
			<p data-testid="bnp-severity-level">High</p>
			<p data-testid="critical-alert">🔴 CRITICAL: Troponin I = 8.5 ng/mL</p>
			<p data-testid="high-alert">🟠 HIGH: BNP = 1200 pg/mL</p>
			<p data-testid="patient-info">Michael Davis, Bed ED-12</p>
			<h3>Notification to Dr. Johnson</h3>
			<p data-testid="mobile-push">CRITICAL LAB: Troponin 8.5 - Michael Davis</p>
			<p data-testid="sms-alert">ED-12 CRITICAL Troponin I: 8.5 ng/mL</p>
			<p data-testid="in-app-alert">High priority popup requiring acknowledgment</p>
			<p data-testid="charge-nurse-notification">Charge nurse notified of critical troponin and BNP values.</p>
			<div data-testid="critical-result-highlight">Troponin I highlighted red</div>
			<div data-testid="critical-result-highlight">BNP highlighted red</div>
			<div data-testid="critical-value-audit-trail">
				Audit trail: critical value communication logged for Michael Davis.
			</div>
		</div>
	{:else if outcomeKind === 'corrections'}
		<div class="card" data-type="warning" role="alert">
			<p data-testid="corrected">Replace previous value, maintain history</p>
			<p data-testid="final">Add new result to patient record</p>
			<p data-testid="pending">Update status, maintain order tracking</p>
			<p data-testid="correction-alert">Lab value corrected: Hgb 11.2 → 9.2 g/dL</p>
			<p data-testid="reason">Sample hemolysis detected</p>
			<p data-testid="clinical-impact">Anemia now more severe than initially reported</p>
			<p data-testid="physician-correction-notification">
				Dr. Martinez notified of the corrected hemoglobin value.
			</p>
			<div data-testid="original-result-audit-entry">Original result: Hemoglobin 11.2 g/dL (preserved)</div>
			<p data-testid="anemia-protocol-alert">Anemia protocol alert triggered.</p>
		</div>
	{:else if outcomeKind === 'pediatric'}
		<div class="card" data-type="success" role="status">
			<p data-testid="white-blood-cells-interpretation">Normal for age 6</p>
			<p data-testid="white-blood-cells-flag">Normal</p>
			<p data-testid="hemoglobin-interpretation">Normal for age 6</p>
			<p data-testid="hemoglobin-flag">Normal</p>
			<p data-testid="alkaline-phosphatase-interpretation">Normal for age 6</p>
			<p data-testid="alkaline-phosphatase-flag">Normal</p>
			<p data-testid="pediatric-attending-notification">
				Dr. Chen notified with age-specific reference range context for Emma Foster (age 6).
			</p>
			<p data-testid="adult-reference-range">Adult range: WBC 4.0-10.0, Hgb 12.0-16.0, ALP 44-147</p>
			<p data-testid="pediatric-reference-range">
				Pediatric (age 6) range: WBC 5.0-14.5, Hgb 11.5-13.5, ALP 156-369
			</p>
		</div>
	{:else if outcomeKind === 'handoff'}
		<div class="card" data-type="error" role="alert">
			<p data-testid="primary-notification-target">Dr. Brown</p>
			<p data-testid="primary-notification-rationale">Current attending physician</p>
			<p data-testid="secondary-notification-target">Dr. Adams</p>
			<p data-testid="secondary-notification-rationale">Ordered the tests, may need notification</p>
			<p data-testid="dr-brown-notification">CRITICAL: Lipase 350 - Patient from Dr. Adams</p>
			<p data-testid="dr-adams-notification">FYI: Your lipase order critical - Now Dr. Brown</p>
			<div data-testid="handoff-log">
				Handoff log: Robert Kim, Lipase 350 (critical), ordered by Dr. Adams, current Dr. Brown.
			</div>
			<p data-testid="charge-nurse-shift-change-notification">
				Charge nurse notified of the critical lipase value during shift change.
			</p>
		</div>
	{:else if outcomeKind === 'technical-failure'}
		<div class="card" data-type="warning" role="alert">
			<p data-testid="delayed-result-processing-status">Delayed results processed.</p>
			<p data-testid="delay-notice">Results delayed due to technical issues</p>
			<p data-testid="original-time">Results ready at 20:10</p>
			<p data-testid="received-time">Results received at 20:15</p>
			<p data-testid="physician-delay-notification">
				Attending physician notified of both the troponin result and the transmission delay.
			</p>
			<p data-testid="system-administrator-alert">
				System administrators alerted to the HL7 interface failure.
			</p>
			<div data-testid="interface-audit-log">
				Interface audit log: transmission failed, retried after 5 minutes, succeeded at 20:15.
			</div>
		</div>
	{:else if outcomeKind === 'batch'}
		<div class="card" data-type="info" role="status">
			<p data-testid="batch-processing-status">All results processed simultaneously.</p>
			<p data-testid="priority-1-patient">Carol Martinez</p>
			<p data-testid="priority-1-notification-type">Critical value alert</p>
			<p data-testid="priority-2-patient">Bob Thompson</p>
			<p data-testid="priority-2-notification-type">Abnormal value alert</p>
			<p data-testid="priority-3-patient">Alice Johnson</p>
			<p data-testid="priority-3-notification-type">Normal results</p>
			<p data-testid="consolidated-notification">
				Dr. Smith received a consolidated notification for Alice Johnson and Bob Thompson.
			</p>
			<p data-testid="system-performance-metrics">Batch processing completed within performance targets.</p>
		</div>
	{:else if outcomeKind === 'interpretation'}
		<div class="card" data-type="info" role="status">
			<ul>
				<li data-testid="interpretation-comment">Blood Smear — Moderate anisocytosis noted</li>
				<li data-testid="interpretation-comment">Hemoglobin A1C — Consistent with poor DM control</li>
				<li data-testid="interpretation-comment">Thyroid Function — Pattern suggests hyperthyroidism</li>
			</ul>
			<p data-testid="raw-results">Numeric values and reference ranges</p>
			<p data-testid="interpretation">Pathologist comments and clinical significance</p>
			<p data-testid="recommendations">Suggested follow-up or additional testing</p>
			<ul>
				<li data-testid="highlighted-interpretation-comment">Hemoglobin A1C 9.2% — highlighted</li>
			</ul>
			<p data-testid="physician-review-flag">Flagged for physician review and acknowledgment.</p>
		</div>
	{/if}
</div>
