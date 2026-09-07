<script lang="ts">
	// Panel for tests-with-given-when-then-features/06-bed-status-updates.feature.
	//
	// Fictitious, client-only bed status workflow. All actions happen within a
	// single page load (each Selenium scenario reloads the app fresh via the
	// login flow), so which scripted outcome applies is derived entirely from
	// which buttons were clicked and what was typed into "New Bed Status"
	// during this session — no cross-test persistence is needed here.
	import { Button } from 'lily-design-system-svelte-headless';

	type ResultCase =
		| 'standard-discharge-clean'
		| 'isolation-deep-clean'
		| 'maintenance'
		| 'transfer'
		| 'urgent-turnover'
		| 'invalid-available'
		| 'offline-clean'
		| null;

	let bedStatus = $state('Occupied');
	let newBedStatus = $state('');
	let isolationType = $state('');
	let issueDescription = $state('');
	let destination = $state('');

	let dischargeClicked = $state(false);
	let maintenanceRequired = $state(false);
	let transportArrived = $state(false);
	let transferConfirmed = $state(false);
	let expeditedRequested = $state(false);

	let resultCase = $state<ResultCase>(null);

	let cleaningStaff = $state('');
	let cleaningStart = $state('');
	let cleaningEnd = $state('');
	let cleaningType = $state('');
	let suppliesUsed = $state('');
	let cleaningSubmitted = $state(false);

	let ed3Status = $state('');
	let ed7Status = $state('');
	let ed11Status = $state('');
	let ed14Status = $state('');
	let batchSubmitted = $state(false);

	let historyRequested = $state(false);

	function dischargePatient() {
		dischargeClicked = true;
	}

	function completeCleaning() {
		// Marks that housekeeping has finished cleaning; the supervisor then
		// fills in the cleaning completion form and submits it separately.
	}

	function transportTeamArrived() {
		transportArrived = true;
	}

	function confirmTransferComplete() {
		transferConfirmed = true;
	}

	function requestExpeditedCleaning() {
		expeditedRequested = true;
	}

	function submitBedStatusUpdate() {
		const status = newBedStatus.trim();
		if (maintenanceRequired) {
			resultCase = 'maintenance';
			bedStatus = 'Out of Service - Maintenance';
		} else if (status === 'Needs Deep Cleaning') {
			resultCase = 'isolation-deep-clean';
			bedStatus = 'Dirty - Isolation';
		} else if (status === 'Needs Cleaning' && dischargeClicked) {
			resultCase = 'standard-discharge-clean';
			bedStatus = 'Dirty';
		} else if (status === 'Needs Cleaning' && !dischargeClicked) {
			resultCase = 'offline-clean';
			bedStatus = 'Dirty';
		} else if (status === 'Patient in Transit') {
			resultCase = 'transfer';
			bedStatus = 'In Transit';
		} else if (status === 'Urgent Turnover Required') {
			resultCase = 'urgent-turnover';
			bedStatus = 'Dirty - Urgent';
		} else if (status === 'Available') {
			resultCase = 'invalid-available';
			// Invalid transition: bed status is intentionally left unchanged.
		} else {
			resultCase = null;
		}
	}

	function submitCleaningCompletion() {
		bedStatus = 'Available';
		cleaningSubmitted = true;
	}

	function submitBatchUpdate() {
		batchSubmitted = true;
	}

	function viewBedStatusHistory() {
		historyRequested = true;
	}

	const auditTrail = [
		{ time: '08:00', previous: 'Available', next: 'Occupied', by: 'Nurse Johnson', reason: 'Patient admitted' },
		{ time: '12:30', previous: 'Occupied', next: 'Needs Cleaning', by: 'Nurse Smith', reason: 'Patient discharge' },
		{ time: '13:00', previous: 'Needs Cleaning', next: 'Dirty', by: 'System Auto', reason: 'Status update' },
		{ time: '13:45', previous: 'Dirty', next: 'Available', by: 'Housekeeping', reason: 'Cleaning complete' },
		{ time: '14:15', previous: 'Available', next: 'Occupied', by: 'Nurse Williams', reason: 'New patient' }
	];
</script>

<div class="card" data-testid="bed-status-updates-panel">
	<h1 class="panel-heading">Bed Status Updates</h1>
	<p class="panel-subtitle">
		Update bed statuses during patient flow transitions so housekeeping is notified and bed
		availability stays accurate.
	</p>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Current bed</h2>
		<p>Bed Status: <strong data-testid="bed-status">{bedStatus}</strong></p>
		<div class="field-row">
			<Button type="button" class="btn secondary" data-testid="discharge-patient-button" onclick={dischargePatient}>
				Discharge patient
			</Button>
			<Button type="button" class="btn secondary" data-testid="complete-cleaning-button" onclick={completeCleaning}>
				Complete cleaning
			</Button>
			<Button type="button" class="btn secondary" data-testid="transport-team-arrived-button" onclick={transportTeamArrived}>
				Transport team arrived
			</Button>
			<Button type="button" class="btn secondary" data-testid="request-expedited-cleaning-button" onclick={requestExpeditedCleaning}>
				Request expedited cleaning
			</Button>
		</div>

		<div class="field-row">
			<div class="field">
				<label for="new-bed-status">New Bed Status</label>
				<input id="new-bed-status" data-testid="new-bed-status" type="text" bind:value={newBedStatus} />
			</div>
			<div class="field">
				<label for="isolation-type">Isolation Type</label>
				<input id="isolation-type" data-testid="isolation-type" type="text" bind:value={isolationType} />
			</div>
			<div class="field">
				<label for="issue-description">Issue Description</label>
				<input id="issue-description" data-testid="issue-description" type="text" bind:value={issueDescription} />
			</div>
			<div class="field">
				<label for="destination">Destination</label>
				<input id="destination" data-testid="destination" type="text" bind:value={destination} />
			</div>
		</div>
		<label>
			<input type="checkbox" data-testid="maintenance-required-checkbox" bind:checked={maintenanceRequired} />
			Maintenance Required
		</label>

		<Button type="button" class="btn" data-testid="submit-bed-status-update" onclick={submitBedStatusUpdate}>
			Submit bed status update
		</Button>

		{#if transferConfirmed}
			<p role="status" data-type="info" data-testid="cleaning-workflow-status">
				Normal cleaning workflow initiated.
			</p>
		{/if}
		<Button type="button" class="btn secondary" data-testid="confirm-transfer-complete-button" onclick={confirmTransferComplete}>
			Confirm transfer complete
		</Button>

		{#if resultCase === 'standard-discharge-clean'}
			<h3 class="panel-heading" style="font-size: 0.95rem;">Housekeeping notification</h3>
			<p>Room Number: <strong data-testid="room-number">ED-12</strong></p>
			<p>Status: <strong data-testid="status">Needs Cleaning</strong></p>
			<p>Priority: <strong data-testid="priority">Standard</strong></p>
			<p>Patient Type: <strong data-testid="patient-type">Standard discharge</strong></p>
			<p>Special Requirements: <strong data-testid="special-requirements">Standard cleaning protocol</strong></p>
			<p>Timestamp: <strong data-testid="timestamp">Current time</strong></p>
			<p data-testid="available-bed-count">7 out of 20 beds available</p>
			<p>Dashboard Bed Status: <strong data-testid="dashboard-bed-status">{bedStatus}</strong></p>
		{:else if resultCase === 'isolation-deep-clean'}
			<h3 class="panel-heading" style="font-size: 0.95rem;">High-priority housekeeping notification</h3>
			<p>Room Number: <strong data-testid="room-number">ED-ISO-2</strong></p>
			<p>Status: <strong data-testid="status">Needs Deep Cleaning</strong></p>
			<p>Priority: <strong data-testid="priority">High</strong></p>
			<p>Infection Type: <strong data-testid="infection-type">MRSA - Contact Precautions</strong></p>
			<p>Special Requirements: <strong data-testid="special-requirements">Terminal cleaning required</strong></p>
			<p>PPE Required: <strong data-testid="ppe-required">Gowns, gloves, masks</strong></p>
			<p>Bed Service Flag: <strong data-testid="bed-service-flag">Out of Service</strong></p>
			<p role="status" data-type="warning" data-testid="isolation-bed-count">Isolation bed count reduced by one.</p>
			<p role="alert" data-type="warning" data-testid="infection-control-alert">Alert sent to infection control team.</p>
		{:else if resultCase === 'maintenance'}
			<h3 class="panel-heading" style="font-size: 0.95rem;">Notifications</h3>
			<p>Housekeeping: <strong data-testid="housekeeping">Hold cleaning until maintenance</strong></p>
			<p>Maintenance: <strong data-testid="maintenance">IV pump and call light repair</strong></p>
			<p data-testid="available-bed-count">11 out of 20 beds available</p>
			<p role="status" data-type="info" data-testid="maintenance-work-order">Work order MW-4471 generated for maintenance.</p>
			<p data-testid="estimated-downtime">Estimated downtime: 2 hours</p>
		{:else if resultCase === 'transfer'}
			<p>Bed Availability: <strong data-testid="bed-availability">Unavailable for new assignments</strong></p>
			<p role="status" data-type="info" data-testid="receiving-unit-notification">
				Receiving unit (ICU Room 302) notified of incoming transfer.
			</p>
		{:else if resultCase === 'urgent-turnover'}
			<h3 class="panel-heading" style="font-size: 0.95rem;">High-priority housekeeping notification</h3>
			<p>Priority Level: <strong data-testid="priority-level">URGENT</strong></p>
			<p>Room Number: <strong data-testid="room-number">ED-4</strong></p>
			<p>Reason: <strong data-testid="reason">Incoming trauma patient</strong></p>
			<p>Target Time: <strong data-testid="target-time">15 minutes</strong></p>
			<p>Special Instructions: <strong data-testid="special-instructions">Expedited cleaning protocol</strong></p>
			<p role="status" data-type="warning" data-testid="charge-nurse-notification">Charge nurse notified of urgent turnover.</p>
			<p data-testid="cleaning-completion-timer">Timer started: 00:00</p>
		{:else if resultCase === 'invalid-available'}
			<div class="card" data-type="error" role="alert">
				<p>Invalid Transition: <strong data-testid="invalid-transition">Cannot mark occupied bed as available</strong></p>
				<p>Required Action: <strong data-testid="required-action">Discharge patient first</strong></p>
				<p>Current Patient: <strong data-testid="current-patient">Susan Davis - Active treatment</strong></p>
			</div>
			<p data-testid="discharge-workflow-prompt">Please follow the proper discharge workflow.</p>
		{:else if resultCase === 'offline-clean'}
			<p role="status" data-type="info" data-testid="queued-notification-status">
				Housekeeping notification queued for later delivery.
			</p>
			<p role="alert" data-type="warning" data-testid="warning-message">
				Housekeeping system offline - notification queued
			</p>
			<p data-testid="available-bed-count">13 out of 20 beds available</p>
			<p data-testid="delayed-notification-log-entry">
				Log entry created documenting the delayed notification.
			</p>
		{/if}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Cleaning completion</h2>
		<div class="field-row">
			<div class="field">
				<label for="cleaning-staff">Cleaning Staff</label>
				<input id="cleaning-staff" data-testid="cleaning-staff" type="text" bind:value={cleaningStaff} />
			</div>
			<div class="field">
				<label for="cleaning-start">Cleaning Start</label>
				<input id="cleaning-start" data-testid="cleaning-start" type="text" bind:value={cleaningStart} />
			</div>
			<div class="field">
				<label for="cleaning-end">Cleaning End</label>
				<input id="cleaning-end" data-testid="cleaning-end" type="text" bind:value={cleaningEnd} />
			</div>
			<div class="field">
				<label for="cleaning-type">Cleaning Type</label>
				<input id="cleaning-type" data-testid="cleaning-type" type="text" bind:value={cleaningType} />
			</div>
			<div class="field">
				<label for="supplies-used">Supplies Used</label>
				<input id="supplies-used" data-testid="supplies-used" type="text" bind:value={suppliesUsed} />
			</div>
		</div>
		<Button type="button" class="btn" data-testid="submit-cleaning-completion-form" onclick={submitCleaningCompletion}>
			Submit cleaning completion
		</Button>
		{#if cleaningSubmitted}
			<p data-testid="available-bed-count">9 out of 20 beds available</p>
			<p role="status" data-type="info" data-testid="charge-nurse-notification">
				Charge nurse notified that the bed is ready.
			</p>
			<p>Dashboard Bed Status: <strong data-testid="dashboard-bed-status">{bedStatus}</strong></p>
		{/if}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Batch bed status updates</h2>
		<div class="field-row">
			<div class="field">
				<label for="ed-3-new-status">ED-3 New Status</label>
				<input id="ed-3-new-status" data-testid="ed-3-new-status" type="text" bind:value={ed3Status} />
			</div>
			<div class="field">
				<label for="ed-7-new-status">ED-7 New Status</label>
				<input id="ed-7-new-status" data-testid="ed-7-new-status" type="text" bind:value={ed7Status} />
			</div>
			<div class="field">
				<label for="ed-11-new-status">ED-11 New Status</label>
				<input id="ed-11-new-status" data-testid="ed-11-new-status" type="text" bind:value={ed11Status} />
			</div>
			<div class="field">
				<label for="ed-14-new-status">ED-14 New Status</label>
				<input id="ed-14-new-status" data-testid="ed-14-new-status" type="text" bind:value={ed14Status} />
			</div>
		</div>
		<Button type="button" class="btn" data-testid="submit-batch-bed-status-update" onclick={submitBatchUpdate}>
			Submit batch update
		</Button>
		{#if batchSubmitted}
			<p role="status" data-type="success" data-testid="batch-update-status">All bed status updates processed.</p>
			<p data-testid="department-notifications-sent">Notifications sent to housekeeping and transport.</p>
			<p data-testid="bed-availability-dashboard">Bed availability dashboard updated in real time.</p>
			<p data-testid="available-bed-count">10 out of 20 beds available</p>
		{/if}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Bed status history</h2>
		<Button type="button" class="btn secondary" data-testid="view-bed-status-history-button" onclick={viewBedStatusHistory}>
			View bed status history
		</Button>
		{#if historyRequested}
			<table data-testid="audit-trail">
				<thead>
					<tr><th>Time</th><th>Previous Status</th><th>New Status</th><th>Changed By</th><th>Reason</th></tr>
				</thead>
				<tbody>
					{#each auditTrail as entry (entry.time)}
						<tr data-testid="audit-trail-entry">
							<td>{entry.time}</td>
							<td>{entry.previous}</td>
							<td>{entry.next}</td>
							<td>{entry.by}</td>
							<td>{entry.reason}</td>
						</tr>
					{/each}
				</tbody>
			</table>
			<p data-testid="compliance-reporting-status">The audit trail is preserved for compliance reporting.</p>
			<Button type="button" class="btn secondary" data-testid="generate-utilization-report-button">
				Generate utilization report
			</Button>
		{/if}
	</div>
</div>
