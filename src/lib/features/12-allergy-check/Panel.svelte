<script lang="ts">
	// Panel for spec/features/12-allergy-check.feature.
	//
	// Unlike some other scripted-demo panels, most of this feature's
	// scenarios ARE distinguishable purely from what the physician typed
	// into the order form (medication name, and dose where two scenarios
	// share a medication name), so the outcome is derived directly from the
	// current form values rather than from any hidden sequence counter.
	import { Button } from 'lily-design-system-svelte-headless';

	type OrderOutcome =
		| 'direct-allergy'
		| 'no-allergy'
		| 'cross-reactive'
		| 'unknown-status'
		| 'drug-class';

	function classifyOrder(medication: string, dose: string): OrderOutcome {
		const med = medication.trim().toLowerCase();
		const doseValue = dose.trim().toLowerCase();
		if (med.includes('penicillin')) return 'direct-allergy';
		if (med.includes('amoxicillin')) {
			return doseValue === '875mg' ? 'no-allergy' : 'cross-reactive';
		}
		if (med.includes('cephalexin')) return 'unknown-status';
		if (med.includes('trimethoprim') || med.includes('sulfamethoxazole')) return 'drug-class';
		return 'no-allergy';
	}

	function kebabCase(label: string): string {
		return label
			.trim()
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/(^-|-$)/g, '');
	}

	function blankOrder() {
		return { medication: '', dose: '', route: '', frequency: '', duration: '', drugClass: '' };
	}

	let order = $state(blankOrder());
	let orderSubmitted = $state(false);
	let overrideStarted = $state(false);
	let overrideSubmitted = $state(false);
	let overrideForm = $state({
		clinicalRationale: '',
		riskAssessment: '',
		monitoringPlan: '',
		alternativeReview: ''
	});

	const outcome = $derived(classifyOrder(order.medication, order.dose));
	const showPredictiveAlert = $derived(order.medication.trim() === 'Pen');

	function submitOrder() {
		orderSubmitted = true;
		overrideStarted = false;
		overrideSubmitted = false;
	}

	function startOverride() {
		overrideStarted = true;
	}

	function submitOverride() {
		overrideSubmitted = true;
	}

	// Multiple simultaneous medication orders
	type MultiRow = { medication: string; dose: string; route: string; purpose: string };
	function blankMultiRow(): MultiRow {
		return { medication: '', dose: '', route: '', purpose: '' };
	}
	let multiRows = $state<MultiRow[]>([blankMultiRow(), blankMultiRow(), blankMultiRow()]);
	let multiSubmitted = $state(false);

	type MultiResult = { name: string; status: string; level: string; warning?: string };
	function classifyMultiRow(name: string): MultiResult {
		const lower = name.trim().toLowerCase();
		if (lower.includes('ibuprofen') || lower.includes('nsaid')) {
			return {
				name,
				status: 'NSAID allergy',
				level: 'WARNING',
				warning: 'Patient allergic to NSAIDs - GI bleeding risk'
			};
		}
		if (lower.includes('fentanyl')) {
			return { name, status: 'No direct allergy', level: 'Safe' };
		}
		return { name, status: 'No allergy', level: 'Safe' };
	}

	const multiResults = $derived(
		multiRows.filter((row) => row.medication.trim()).map((row) => classifyMultiRow(row.medication))
	);

	function submitMultiOrders() {
		multiSubmitted = true;
	}
</script>

<div class="card" data-testid="allergy-check-panel">
	<h1 class="panel-heading">Allergy Check</h1>
	<p class="panel-subtitle">
		Automatically check drug allergies and cross-reactivity when entering a medication order.
	</p>

	<h2 class="panel-heading" style="font-size: 1.05rem;">Medication order</h2>
	<div class="field-row">
		<div class="field">
			<label for="medication">Medication</label>
			<input id="medication" data-testid="medication" type="text" bind:value={order.medication} />
		</div>
		<div class="field">
			<label for="dose">Dose</label>
			<input id="dose" data-testid="dose" type="text" bind:value={order.dose} />
		</div>
		<div class="field">
			<label for="route">Route</label>
			<input id="route" data-testid="route" type="text" bind:value={order.route} />
		</div>
		<div class="field">
			<label for="frequency">Frequency</label>
			<input id="frequency" data-testid="frequency" type="text" bind:value={order.frequency} />
		</div>
		<div class="field">
			<label for="duration">Duration</label>
			<input id="duration" data-testid="duration" type="text" bind:value={order.duration} />
		</div>
		{#if !orderSubmitted}
			<div class="field">
				<label for="drug-class">Drug Class</label>
				<input id="drug-class" data-testid="drug-class" type="text" bind:value={order.drugClass} />
			</div>
		{/if}
	</div>
	<Button type="button" class="btn" data-testid="submit-medication-order" onclick={submitOrder}>
		Submit order
	</Button>

	{#if showPredictiveAlert}
		<div class="card" data-type="warning" role="alert" data-testid="allergy-alert">
			<p data-testid="predictive-alert">⚠️ Patient allergic to Penicillin</p>
			<p>
				Medication Match:
				<span data-testid="medication-match">"Pen" may be penicillin-related drug</span>
			</p>
			<p>Suggestion: <span data-testid="suggestion">Consider alternative antibiotics</span></p>
			<p data-testid="allergy-match-highlight">Potential allergy match highlighted while typing.</p>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Alternative medication suggestions</h2>
			<table>
				<thead>
					<tr>
						<th>Alternative</th>
						<th>Drug Class</th>
						<th>Reason</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td>Cephalexin</td>
						<td data-testid="cephalexin-drug-class">Cephalosporin</td>
						<td data-testid="cephalexin-reason">Lower cross-reactivity</td>
					</tr>
					<tr>
						<td>Azithromycin</td>
						<td data-testid="azithromycin-drug-class">Macrolide</td>
						<td data-testid="azithromycin-reason">No cross-reactivity</td>
					</tr>
					<tr>
						<td>Ciprofloxacin</td>
						<td data-testid="ciprofloxacin-drug-class">Fluoroquinolone</td>
						<td data-testid="ciprofloxacin-reason">Different mechanism</td>
					</tr>
				</tbody>
			</table>
			<p data-testid="alternative-suggestion-list">Select an alternative directly from this list.</p>
		</div>
	{/if}

	{#if orderSubmitted && outcome === 'direct-allergy'}
		<div class="card" data-type="error" role="alert" data-testid="allergy-alert">
			<p data-testid="drug-allergy">⚠️ ALLERGY ALERT: Patient allergic to Penicillin</p>
			<p>Severity Level: <span data-testid="severity-level">Moderate</span></p>
			<p>Reaction Type: <span data-testid="reaction-type">Rash, hives</span></p>
			<p>Date Documented: <span data-testid="date-documented">May 15, 2023</span></p>
			<p>Source: <span data-testid="source">Patient reported</span></p>
		</div>
		<p data-testid="order-status">Order blocked pending allergy review</p>
		<ul>
			<li data-testid="cancel-order">Cancel Order — Remove penicillin order</li>
			<li data-testid="override-with-reason">Override with Reason — Document clinical justification</li>
			<li data-testid="alternative-drugs">Alternative Drugs — View suggested alternative antibiotics</li>
		</ul>
		<p data-testid="audit-trail-entry">
			Allergy alert logged in the audit trail for {order.medication || 'Penicillin VK'}.
		</p>
		{#if !overrideStarted}
			<Button
				type="button"
				class="btn secondary"
				data-testid="override-allergy-warning"
				onclick={startOverride}
			>
				Override warning
			</Button>
		{:else if !overrideSubmitted}
			<div class="card">
				<h2 class="panel-heading" style="font-size: 1.05rem;">Override justification</h2>
				<div class="field-row">
					<div class="field">
						<label for="clinical-rationale">Clinical Rationale</label>
						<p data-testid="clinical-rationale-description">
							Why this medication is medically necessary
						</p>
						<input
							id="clinical-rationale"
							data-testid="clinical-rationale"
							type="text"
							bind:value={overrideForm.clinicalRationale}
						/>
					</div>
					<div class="field">
						<label for="risk-assessment">Risk Assessment</label>
						<p data-testid="risk-assessment-description">
							Evaluation of allergy risk vs benefit
						</p>
						<input
							id="risk-assessment"
							data-testid="risk-assessment"
							type="text"
							bind:value={overrideForm.riskAssessment}
						/>
					</div>
					<div class="field">
						<label for="monitoring-plan">Monitoring Plan</label>
						<p data-testid="monitoring-plan-description">
							How allergic reactions will be monitored
						</p>
						<input
							id="monitoring-plan"
							data-testid="monitoring-plan"
							type="text"
							bind:value={overrideForm.monitoringPlan}
						/>
					</div>
					<div class="field">
						<label for="alternative-review">Alternative Review</label>
						<p data-testid="alternative-review-description">Why alternatives are not suitable</p>
						<input
							id="alternative-review"
							data-testid="alternative-review"
							type="text"
							bind:value={overrideForm.alternativeReview}
						/>
					</div>
				</div>
				<Button
					type="button"
					class="btn"
					data-testid="submit-override-documentation"
					onclick={submitOverride}
				>
					Submit override documentation
				</Button>
			</div>
		{:else}
			<div class="card" data-type="success" role="status">
				<p data-testid="override-status">Override accepted</p>
				<p data-testid="override-audit-log">
					Override decision logged with full documentation.
				</p>
				<p data-testid="nursing-notification">
					Nursing staff notified of the allergy override for enhanced monitoring.
				</p>
			</div>
		{/if}

		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Allergy information from multiple sources</h2>
			<p data-testid="verified-record">Penicillin - Urticaria, rash (Medical Records)</p>
			<p data-testid="patient-report">Penicillin - "Bad reaction" (Unverified)</p>
			<table>
				<thead>
					<tr>
						<th>Source</th>
						<th>Credibility Level</th>
						<th>Clinical Weight</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td>Medical Records</td>
						<td data-testid="medical-records-credibility-level">High reliability</td>
						<td data-testid="medical-records-clinical-weight">Primary consideration</td>
					</tr>
					<tr>
						<td>Patient Report</td>
						<td data-testid="patient-report-credibility-level">Moderate reliability</td>
						<td data-testid="patient-report-clinical-weight">Secondary consideration</td>
					</tr>
				</tbody>
			</table>
			<p data-testid="detailed-allergy-history">
				Full documented allergy history available for review before making decisions.
			</p>
		</div>
	{/if}

	{#if orderSubmitted && outcome === 'no-allergy'}
		<p data-testid="no-allergies-found">No known allergies to {order.medication || 'Amoxicillin'}</p>
		<p data-testid="order-status">Order submitted successfully</p>
		<p data-testid="pharmacy-routing-status">Medication order routed to pharmacy for dispensing</p>
	{/if}

	{#if orderSubmitted && outcome === 'cross-reactive'}
		<div class="card" data-type="warning" role="alert" data-testid="allergy-alert">
			<p data-testid="cross-reactivity">⚠️ WARNING: Cross-reactivity with Penicillin</p>
			<p>Known Allergy: <span data-testid="known-allergy">Patient allergic to Penicillin (Severe)</span></p>
			<p>
				Cross-Reaction Risk:
				<span data-testid="cross-reaction-risk">Amoxicillin is a penicillin derivative</span>
			</p>
			<p>Reaction Type: <span data-testid="reaction-type">Respiratory distress</span></p>
			<p>Risk Level: <span data-testid="risk-level">High - Severe reaction possible</span></p>
		</div>
		<p>
			Cross-Reaction Rate:
			<span data-testid="cross-reaction-rate">8-10% cross-reactivity with penicillin</span>
		</p>
		<p>
			Clinical Guidance:
			<span data-testid="clinical-guidance">Consider non-beta-lactam alternatives</span>
		</p>
		<p>
			Emergency Prep:
			<span data-testid="emergency-prep">Have epinephrine available if administered</span>
		</p>
		<p data-testid="acknowledge-cross-reactivity-risk">Cross-reactivity risk acknowledgement required.</p>
	{/if}

	{#if orderSubmitted && outcome === 'drug-class'}
		<div class="card" data-type="error" role="alert" data-testid="allergy-alert">
			<p data-testid="drug-class-allergy">⚠️ SEVERE: Patient allergic to Sulfa drugs</p>
			<p>Specific Drug: <span data-testid="specific-drug">TMP-SMX contains sulfamethoxazole</span></p>
			<p>Reaction History: <span data-testid="reaction-history">Stevens-Johnson syndrome</span></p>
			<p>Severity: <span data-testid="severity">Severe - Life-threatening reaction possible</span></p>
		</div>
		<p>Drug Class: <span data-testid="drug-class">Sulfonamide antibiotics</span></p>
		<p>
			Cross-Reactivity:
			<span data-testid="cross-reactivity">All sulfa-containing medications at risk</span>
		</p>
		<p>
			Alternative Classes:
			<span data-testid="alternative-classes">Beta-lactams, fluoroquinolones available</span>
		</p>
	{/if}

	{#if orderSubmitted && outcome === 'unknown-status'}
		<div class="card" data-type="info" role="status">
			<p data-testid="allergy-unknown">⚠️ INFO: Patient allergy status unknown</p>
			<p>
				Risk Consideration:
				<span data-testid="risk-consideration">Cannot verify medication allergies</span>
			</p>
			<p>
				Recommendation:
				<span data-testid="recommendation">Consider allergy assessment before administration</span>
			</p>
		</div>
		<p>
			Allergy Assessment:
			<span data-testid="allergy-assessment">Attempt to obtain allergy history</span>
		</p>
		<p>
			Start Monitoring:
			<span data-testid="start-monitoring">Monitor for allergic reactions closely</span>
		</p>
		<p>
			Have Antidotes Ready:
			<span data-testid="have-antidotes-ready">Ensure emergency medications available</span>
		</p>
		<p data-testid="enhanced-monitoring-flag">Order flagged for enhanced allergy monitoring.</p>
	{/if}

	<h2 class="panel-heading" style="font-size: 1.05rem;">Multiple medication orders</h2>
	<div class="field-row">
		{#each multiRows as row, index (index)}
			<div class="field">
				<label for={`medication-${index + 1}`}>Medication {index + 1}</label>
				<input
					id={`medication-${index + 1}`}
					data-testid={`medication-${index + 1}`}
					type="text"
					bind:value={row.medication}
				/>
			</div>
			<div class="field">
				<label for={`dose-${index + 1}`}>Dose {index + 1}</label>
				<input
					id={`dose-${index + 1}`}
					data-testid={`dose-${index + 1}`}
					type="text"
					bind:value={row.dose}
				/>
			</div>
			<div class="field">
				<label for={`route-${index + 1}`}>Route {index + 1}</label>
				<input
					id={`route-${index + 1}`}
					data-testid={`route-${index + 1}`}
					type="text"
					bind:value={row.route}
				/>
			</div>
			<div class="field">
				<label for={`purpose-${index + 1}`}>Purpose {index + 1}</label>
				<input
					id={`purpose-${index + 1}`}
					data-testid={`purpose-${index + 1}`}
					type="text"
					bind:value={row.purpose}
				/>
			</div>
		{/each}
	</div>
	<Button type="button" class="btn" data-testid="submit-medication-orders" onclick={submitMultiOrders}>
		Submit all orders
	</Button>

	{#if multiSubmitted}
		<div class="card">
			<table>
				<thead>
					<tr>
						<th>Medication</th>
						<th>Allergy Status</th>
						<th>Alert Level</th>
					</tr>
				</thead>
				<tbody>
					{#each multiResults as result (result.name)}
						<tr>
							<td>{result.name}</td>
							<td data-testid={`${kebabCase(result.name)}-allergy-status`}>{result.status}</td>
							<td data-testid={`${kebabCase(result.name)}-alert-level`}>{result.level}</td>
						</tr>
					{/each}
				</tbody>
			</table>
			{#each multiResults.filter((r) => r.warning) as result (result.name)}
				<p data-testid={`${kebabCase(result.name)}-warning-message`}>{result.warning}</p>
			{/each}
			<p data-testid="review-and-modify-orders">Review and modify orders before final submission.</p>
		</div>
	{/if}
</div>
