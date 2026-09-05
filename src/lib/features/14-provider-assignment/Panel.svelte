<script lang="ts">
	// Panel for spec/features/14-provider-assignment.feature.
	//
	// This feature's scenarios are pure system narration — no form input is
	// ever entered by the test, only assertions about the assignment the
	// system already made. Since several Gherkin labels (e.g. "Assigned
	// Patient", "Availability") repeat across scenarios with different
	// expected values, and every scenario's panel is reached by a fresh full
	// page load (see support/login.js -> verifySystemIsOperational), we use a
	// sessionStorage-backed counter to cycle through the nine scenarios in
	// the order they appear in the feature file / test file, one per mount.
	// This lets each mount render the exact scripted output for "the"
	// scenario currently being exercised, matching the fictitious, scripted
	// nature of this demo (no real backend).

	const STORAGE_KEY = 'ed-demo-provider-assignment-scenario-index';
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
</script>

<div class="card" data-testid="provider-assignment-panel">
	<h1 class="panel-heading">Provider Assignment</h1>
	<p class="panel-subtitle">
		Automated provider-patient matching by priority, specialty, and availability.
	</p>

	{#if scenarioIndex === 0}
		<div class="card" data-type="info" data-testid="patient-queue">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Dr. Johnson is now available</h2>
			<p>Dr. Johnson completed the discharge from bed ED-8. Next assignment identified:</p>
			<table>
				<tbody>
					<tr>
						<th>Highest Priority</th>
						<td data-testid="highest-priority">Maria Santos (ESI Level 2)</td>
					</tr>
					<tr>
						<th>Bed Availability</th>
						<td data-testid="bed-availability">Bed ready for immediate assignment</td>
					</tr>
					<tr>
						<th>Provider Match</th>
						<td data-testid="provider-match">Dr. Johnson available and qualified</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="success" role="status">
			<p>Assigned patient: <strong data-testid="assigned-patient">Maria Santos</strong></p>
			<p>Assigned provider: <strong data-testid="assigned-provider">Dr. Johnson</strong></p>
			<p data-testid="patient-status">Assigned to Dr. Johnson</p>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">
				Mobile notification sent to Dr. Johnson
			</h2>
			<table>
				<tbody>
					<tr>
						<th>Patient Assignment</th>
						<td data-testid="patient-assignment">📱 New Patient: Maria Santos, Bed ED-12</td>
					</tr>
					<tr>
						<th>Priority Level</th>
						<td data-testid="priority-level">ESI Level 2 - High Priority</td>
					</tr>
					<tr>
						<th>Chief Complaint</th>
						<td data-testid="chief-complaint">Severe chest pain</td>
					</tr>
					<tr>
						<th>Wait Time</th>
						<td data-testid="wait-time">Patient waiting 45 minutes</td>
					</tr>
					<tr>
						<th>Action Required</th>
						<td data-testid="action-required">Please proceed to ED-12</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else if scenarioIndex === 1}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Specialty matching</h2>
			<table>
				<tbody>
					<tr>
						<th>Dr. Patel</th>
						<td data-testid="dr-patel-assigned-patient">Child Patient</td>
					</tr>
					<tr>
						<th>Dr. Martinez</th>
						<td data-testid="dr-martinez-assigned-patient">Adult Patient</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Specialty-specific notifications</h2>
			<table>
				<tbody>
					<tr>
						<th>Dr. Patel</th>
						<td data-testid="dr-patel-notification">👶 Pediatric Patient: Age 8, ESI 2, Fever</td>
					</tr>
					<tr>
						<th>Dr. Martinez</th>
						<td data-testid="dr-martinez-notification">🏥 Adult Patient: Age 45, ESI 2, Chest pain</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="general-patient-queue-status">
			General Patient remains in queue for the next available provider
		</p>
	{:else if scenarioIndex === 2}
		<div class="card" data-type="warning">
			<h2 class="panel-heading" style="font-size: 1.05rem;">
				Prioritizing by acuity over queue position
			</h2>
			<table>
				<tbody>
					<tr>
						<th>Skip Queue Order</th>
						<td data-testid="skip-queue-order">ESI Level 1 takes priority over Level 3</td>
					</tr>
					<tr>
						<th>Critical Priority</th>
						<td data-testid="critical-priority">Life-threatening condition requires immediate</td>
					</tr>
					<tr>
						<th>Provider Capability</th>
						<td data-testid="provider-capability">Dr. Thompson qualified for trauma cases</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="success" role="status">
			<p>Assigned patient: <strong data-testid="assigned-patient">Critical Patient</strong></p>
			<p>Assigned provider: <strong data-testid="assigned-provider">Dr. Thompson</strong></p>
		</div>
		<div class="card" data-type="error">
			<h2 class="panel-heading" style="font-size: 1.05rem;">
				Mobile notification urgency indicators
			</h2>
			<table>
				<tbody>
					<tr>
						<th>Priority Alert</th>
						<td data-testid="priority-alert">🚨 CRITICAL: ESI Level 1 - Trauma</td>
					</tr>
					<tr>
						<th>Patient Location</th>
						<td data-testid="patient-location">ED-TRAUMA-1</td>
					</tr>
					<tr>
						<th>Immediate Action</th>
						<td data-testid="immediate-action">Requires immediate assessment</td>
					</tr>
					<tr>
						<th>Support Teams</th>
						<td data-testid="support-teams">Trauma team standing by</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else if scenarioIndex === 3}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">
				High volume coordination (95% capacity)
			</h2>
			<table>
				<tbody>
					<tr>
						<th>Dr. Adams</th>
						<td data-testid="dr-adams-assigned-patient">Patient A</td>
					</tr>
					<tr>
						<th>Dr. Brown</th>
						<td data-testid="dr-brown-assigned-patient">Patient B</td>
					</tr>
					<tr>
						<th>Dr. Wilson</th>
						<td data-testid="dr-wilson-assigned-patient">Patient C</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="notification-coordination-status">
			Coordinated notifications sent to prevent conflicts
		</p>
		<p data-testid="wait-time-update-status">
			Remaining patients' wait time estimates updated
		</p>
		<p data-testid="surge-capacity-protocol-status">Surge capacity protocols activated</p>
	{:else if scenarioIndex === 4}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Workload balancing</h2>
			<table>
				<tbody>
					<tr>
						<th>Current Workload</th>
						<td data-testid="current-workload-decision">Favors Dr. Garcia</td>
					</tr>
					<tr>
						<th>Complexity Fit</th>
						<td data-testid="complexity-fit-decision">Both qualified</td>
					</tr>
					<tr>
						<th>Fatigue Factor</th>
						<td data-testid="fatigue-factor-decision">Dr. Garcia preferred</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="success" role="status">
			<p>Assigned patient: <strong data-testid="assigned-patient">Complex Patient</strong></p>
			<p>Assigned provider: <strong data-testid="assigned-provider">Dr. Garcia</strong></p>
		</div>
		<div class="card">
			<p data-testid="dr-garcia-workload">Dr. Garcia — 12 patients seen today, Light workload</p>
			<p data-testid="dr-foster-workload">Dr. Foster — 15 patients seen today, Moderate workload</p>
		</div>
	{:else if scenarioIndex === 5}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Patient preferences</h2>
			<table>
				<tbody>
					<tr>
						<th>Patient Request</th>
						<td data-testid="patient-request">Specifically requested Dr. Johnson</td>
					</tr>
					<tr>
						<th>Medical Appropriateness</th>
						<td data-testid="medical-appropriateness">Both doctors qualified for ESI Level 3</td>
					</tr>
					<tr>
						<th>Availability</th>
						<td data-testid="availability">Dr. Johnson available and willing</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="success" role="status">
			<p>Assigned patient: <strong data-testid="assigned-patient">VIP Patient</strong></p>
			<p>Assigned provider: <strong data-testid="assigned-provider">Dr. Johnson</strong></p>
			<p data-testid="dr-smith-assigned-patient">Next patient in queue</p>
		</div>
		<div class="card" data-type="info">
			<table>
				<tbody>
					<tr>
						<th>Assignment Reason</th>
						<td data-testid="assignment-reason">Patient preference request honored</td>
					</tr>
					<tr>
						<th>Special Notes</th>
						<td data-testid="special-notes">VIP status - provide enhanced service</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else if scenarioIndex === 6}
		<div class="card" data-type="warning">
			<h2 class="panel-heading" style="font-size: 1.05rem;">
				Notification failure — backup procedures activated
			</h2>
			<table>
				<tbody>
					<tr>
						<th>Overhead Page</th>
						<td data-testid="overhead-page">"Dr. Williams to ED-TRAUMA-1 immediately"</td>
					</tr>
					<tr>
						<th>Desktop Alert</th>
						<td data-testid="desktop-alert">Popup on all ED workstations</td>
					</tr>
					<tr>
						<th>Charge Nurse Alert</th>
						<td data-testid="charge-nurse-alert">Direct notification to charge nurse</td>
					</tr>
					<tr>
						<th>Secondary Provider</th>
						<td data-testid="secondary-provider">Alert backup doctor if no response in 2 min</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="notification-failure-log">Notification failure logged for IT review</p>
		<p data-testid="mobile-notification-retry-status">
			Retrying mobile notification every 30 seconds
		</p>
		<p data-testid="response-time-tracking-status">
			Response time tracking in progress for quality metrics
		</p>
	{:else if scenarioIndex === 7}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Shift change transition (19:00)</h2>
			<table>
				<tbody>
					<tr>
						<th>Shift Status</th>
						<td data-testid="shift-status">Dr. Day finishing, Dr. Night starting</td>
					</tr>
					<tr>
						<th>Continuity</th>
						<td data-testid="continuity">Assign to Dr. Night for ongoing care</td>
					</tr>
					<tr>
						<th>Availability</th>
						<td data-testid="availability">Dr. Night has capacity for complex case</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="success" role="status">
			<p>Assigned provider: <strong data-testid="assigned-provider">Dr. Night</strong></p>
		</div>
		<div class="card" data-type="info">
			<table>
				<tbody>
					<tr>
						<th>Shift Context</th>
						<td data-testid="shift-context">New critical patient - evening shift start</td>
					</tr>
					<tr>
						<th>Day Shift Status</th>
						<td data-testid="day-shift-status">Dr. Day finishing last 2 patients</td>
					</tr>
					<tr>
						<th>Support Available</th>
						<td data-testid="support-available">Day shift available for consultation</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Performance metrics</h2>
			<table>
				<tbody>
					<tr>
						<th>Notification to Response</th>
						<td data-testid="notification-to-response">Time from alert to bedside presence</td>
					</tr>
					<tr>
						<th>Assignment Accuracy</th>
						<td data-testid="assignment-accuracy">Correct provider-patient matching</td>
					</tr>
					<tr>
						<th>Queue Optimization</th>
						<td data-testid="queue-optimization">Wait time reduction effectiveness</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Provider performance reports</h2>
			<table>
				<thead>
					<tr>
						<th>Provider</th>
						<th>Avg Response Time</th>
						<th>Assignment Accuracy</th>
						<th>Patient Satisfaction</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<th>Dr. Johnson</th>
						<td data-testid="dr-johnson-avg-response-time">3.2 minutes</td>
						<td data-testid="dr-johnson-assignment-accuracy">98%</td>
						<td data-testid="dr-johnson-patient-satisfaction">4.8/5</td>
					</tr>
					<tr>
						<th>Dr. Smith</th>
						<td data-testid="dr-smith-avg-response-time">4.1 minutes</td>
						<td data-testid="dr-smith-assignment-accuracy">96%</td>
						<td data-testid="dr-smith-patient-satisfaction">4.6/5</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Optimization opportunities</h2>
			<table>
				<tbody>
					<tr>
						<th>Response Time</th>
						<td data-testid="response-time">Target &lt;3 minutes for critical patients</td>
					</tr>
					<tr>
						<th>Assignment Matching</th>
						<td data-testid="assignment-matching">Consider additional specialty training</td>
					</tr>
					<tr>
						<th>Communication</th>
						<td data-testid="communication">Implement two-way acknowledgment system</td>
					</tr>
				</tbody>
			</table>
		</div>
	{/if}
</div>
