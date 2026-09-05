<script lang="ts">
	// Panel for spec/features/20-code-blue-response.feature.
	//
	// Fictitious, client-only code blue response demo. Pressing the code blue
	// button reveals the instant activation details for the scripted scenario
	// (bed ED-5, Nurse Johnson, Robert Martinez). The remaining scenarios in
	// the feature file describe system state that is assumed to already be in
	// progress (equipment coordination, real-time documentation, ROSC,
	// end-of-life transition, family presence, quality metrics, and false
	// alarm handling), so those sections are shown as standing reference
	// content below the activation panel.
	import { Button } from 'lily-design-system-svelte-headless';

	type Row = { label: string; value: string };

	function slug(label: string): string {
		return label
			.trim()
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/(^-|-$)/g, '');
	}

	let codeBlueActivated = $state(false);

	function activateCodeBlue() {
		codeBlueActivated = true;
	}

	// Scenario: Activate code blue for cardiac arrest in bed 5
	const alertActivationDetails: Row[] = [
		{ label: 'Alert Timestamp', value: '14:35:22 - Precise time recorded' },
		{ label: 'Location', value: 'ED-5 clearly identified' },
		{ label: 'Initiating Staff', value: 'Nurse Johnson' },
		{ label: 'Patient Identity', value: 'Robert Martinez (if available)' },
		{ label: 'Alert Type', value: 'Code Blue - Cardiac Arrest' }
	];

	const codeTeamAlerts: { member: string; method: string; responseTime: string }[] = [
		{ member: 'Emergency Physician', method: 'Overhead page, mobile', responseTime: 'Immediate' },
		{ member: 'Cardiologist', method: 'Mobile alert, pager', responseTime: 'Within 2 minutes' },
		{
			member: 'Anesthesiologist',
			method: 'Overhead page, mobile',
			responseTime: 'Within 3 minutes'
		},
		{ member: 'ICU Nurse', method: 'Mobile alert, pager', responseTime: 'Within 2 minutes' },
		{
			member: 'Respiratory Therapist',
			method: 'Overhead page, mobile',
			responseTime: 'Within 2 minutes'
		},
		{ member: 'Pharmacist', method: 'Mobile alert', responseTime: 'Within 3 minutes' },
		{ member: 'Chaplain', method: 'Silent alert', responseTime: 'Within 5 minutes' }
	];

	const patientLocationDisplays: Row[] = [
		{ label: 'ED Dashboard', value: '🚨 CODE BLUE - BED ED-5 flashing red' },
		{ label: 'Mobile Devices', value: 'Push notification with location' },
		{ label: 'Overhead Displays', value: '"CODE BLUE BED ED-5" prominently shown' },
		{ label: 'Pager System', value: '"CODE BLUE ED-5" message' },
		{ label: 'Hospital Information', value: 'Alert on all connected terminals' }
	];

	const documentationTemplateSections: Row[] = [
		{ label: 'Event Details', value: 'Time, location, discoverer, initial rhythm' },
		{ label: 'Timeline Tracker', value: 'Medication times, defibrillation, procedures' },
		{ label: 'Team Members', value: 'Roles and arrival times' },
		{ label: 'Vital Signs', value: 'Real-time monitoring integration' },
		{ label: 'Interventions', value: 'CPR quality, airway management, IV access' }
	];

	// Scenario: Code blue response with automatic equipment alerts
	const equipmentAlerts: Row[] = [
		{ label: 'Crash Cart', value: 'Crash cart dispatch to ED-5' },
		{ label: 'Defibrillator', value: 'AED/Manual defibrillator to ED-5' },
		{ label: 'Airway Equipment', value: 'Intubation kit and ventilator to ED-5' },
		{ label: 'Emergency Medications', value: 'Code blue medication box to ED-5' },
		{ label: 'IV Access Supplies', value: 'Central line kit and fluids to ED-5' }
	];

	const equipmentTracking: Row[] = [
		{ label: 'Crash Cart Location', value: 'GPS tracking to bed ED-5' },
		{ label: 'Defibrillator Readiness', value: 'Battery level and functionality check' },
		{ label: 'Medication Expiration', value: 'Code blue drugs expiration verification' },
		{ label: 'Equipment Arrival', value: 'Timestamp when equipment reaches bedside' }
	];

	const backupEquipment: Row[] = [
		{ label: 'Secondary Crash Cart', value: 'Made ready for potential second code' },
		{ label: 'Additional Ventilator', value: 'Checked and moved closer to ED' },
		{ label: 'Blood Bank Alert', value: 'Emergency blood products prepared' },
		{ label: 'OR Notification', value: 'Operating room placed on standby' }
	];

	// Scenario: Real-time code blue documentation during resuscitation
	const documentedActivities: Row[] = [
		{ label: 'CPR Administration', value: 'Start time, compression quality, provider' },
		{ label: 'Medication Given', value: 'Drug name, dose, route, time, provider' },
		{ label: 'Defibrillation', value: 'Joules delivered, rhythm before/after' },
		{ label: 'Airway Management', value: 'Type of airway, success, provider' },
		{ label: 'IV Access', value: 'Location, size, number of attempts' }
	];

	const timelineEntries: Row[] = [
		{ label: 'Event Start', value: '14:35:22 - Code blue activated' },
		{ label: 'CPR Initiated', value: '14:35:45 - CPR started by Nurse Johnson' },
		{ label: 'Team Leader Arrival', value: '14:36:15 - Dr. Smith assumes leadership' },
		{ label: 'First Medication', value: '14:37:30 - Epinephrine 1mg IV push' },
		{ label: 'Defibrillation Timeline', value: '14:38:45 - 200J biphasic shock delivered' }
	];

	const qualityMetrics: Row[] = [
		{ label: 'Compression Depth', value: 'CPR feedback device integration' },
		{ label: 'Compression Rate', value: 'Metronome guidance and measurement' },
		{ label: 'No-flow Time', value: 'Automatic calculation of interruptions' },
		{ label: 'Medication Timing', value: 'Alert for time-critical drug intervals' }
	];

	// Scenario: Code blue with return of spontaneous circulation (ROSC)
	const codeStatusUpdates: Row[] = [
		{ label: 'ROSC Achievement', value: 'Time: 14:43:15 - ROSC achieved' },
		{ label: 'Vital Signs', value: 'BP: 110/70, HR: 85, documented' },
		{ label: 'Rhythm Change', value: 'Normal sinus rhythm confirmed' },
		{ label: 'Intervention Pause', value: 'CPR discontinued, monitoring intensified' }
	];

	const postRoscProtocols: Row[] = [
		{ label: 'ICU Transfer', value: 'ICU bed request and transport coordination' },
		{ label: 'Cardiology Consult', value: 'Urgent cardiology evaluation requested' },
		{ label: 'Temperature Management', value: 'Therapeutic hypothermia consideration' },
		{ label: 'Neurological Assessment', value: 'Baseline neuro checks ordered' }
	];

	const familyNotificationProcedures: Row[] = [
		{ label: 'Contact Attempt', value: 'Emergency contact called by social worker' },
		{ label: 'Status Update', value: '"Patient being treated, stable condition"' },
		{
			label: 'Visitation Arrangement',
			value: 'Family arrival and bedside visit coordination'
		},
		{ label: 'Chaplain Services', value: 'Spiritual care offered to family' }
	];

	// Scenario: Unsuccessful code blue with transition to end-of-life care
	const endOfLifeDocumentation: Row[] = [
		{ label: 'Time of Death', value: '15:00:15 - Officially recorded' },
		{ label: 'Resuscitation Duration', value: '24 minutes 53 seconds total time' },
		{ label: 'Interventions Summary', value: 'Complete list of all attempted treatments' },
		{ label: 'Team Members Present', value: 'All providers involved in resuscitation' }
	];

	const familySupportProcedures: Row[] = [
		{ label: 'Immediate Contact', value: 'Personal notification by physician' },
		{ label: 'Bereavement Support', value: 'Chaplain and social worker assigned' },
		{ label: 'Viewing Arrangement', value: 'Private room prepared for family viewing' },
		{ label: 'Organ Donation', value: 'Coordinator contacted per protocol' }
	];

	const administrativeProcesses: Row[] = [
		{ label: 'Medical Examiner', value: 'Contact if death meets criteria' },
		{ label: 'Autopsy Consent', value: 'Family discussion and documentation' },
		{ label: 'Death Certificate', value: 'Physician completion requirements' },
		{ label: 'Quality Review', value: 'Case review scheduled within 24 hours' }
	];

	// Scenario: Code blue during visitor hours with family present
	const familyManagementProtocols: Row[] = [
		{ label: 'Family Escort', value: 'Security escorts family to private area' },
		{ label: 'Communication', value: 'Social worker provides immediate support' },
		{
			label: 'Information Updates',
			value: 'Regular updates provided during resuscitation'
		},
		{ label: 'Chaplain Services', value: 'Spiritual care offered immediately' }
	];

	const visitorAreaManagement: Row[] = [
		{ label: 'Area Clearance', value: 'Non-family visitors moved from immediate area' },
		{ label: 'Privacy Protection', value: 'Screens and barriers deployed' },
		{ label: 'Crowd Control', value: 'Security manages visitor flow' },
		{ label: 'Other Patient Care', value: 'Continued care for nearby patients' }
	];

	const familyPreferenceAccommodation: Row[] = [
		{ label: 'Bedside Presence', value: 'Option to remain during resuscitation' },
		{ label: 'Waiting Area', value: 'Comfortable private space with updates' },
		{ label: 'Family Spokesperson', value: 'Designated family member for communication' },
		{ label: 'Support Person', value: 'Additional family/friend notification' }
	];

	// Scenario: Code blue team performance metrics and quality improvement
	const performanceMetrics: Row[] = [
		{ label: 'Response Time', value: '1 minute 23 seconds from alert to arrival' },
		{ label: 'No-flow Time', value: '15 seconds total interruption time' },
		{ label: 'First Shock Time', value: '3 minutes 45 seconds from arrest' },
		{ label: 'Medication Timing', value: 'All drugs given within target windows' },
		{ label: 'Team Coordination', value: 'Communication effectiveness score' }
	];

	const qiDataElements: Row[] = [
		{ label: 'Protocol Adherence', value: '95% compliance with ACLS guidelines' },
		{ label: 'Equipment Function', value: 'All equipment functioned properly' },
		{ label: 'Team Performance', value: 'Effective leadership and role clarity' },
		{ label: 'Communication Quality', value: 'Clear, concise, and timely communication' }
	];

	const improvementOpportunities: Row[] = [
		{ label: 'Response Time Recommendation', value: 'Consider additional code cart placement' },
		{ label: 'Team Training', value: 'Schedule quarterly simulation training' },
		{ label: 'Equipment Maintenance', value: 'Review defibrillator calibration schedule' },
		{ label: 'Documentation', value: 'Streamline real-time entry process' }
	];

	// Scenario: Code blue false alarm with appropriate system response
	const falseAlarmProtocol: Row[] = [
		{ label: 'Alert Cancellation', value: '"Code blue canceled - false alarm" announcement' },
		{ label: 'Team Stand-down', value: 'Code team notified to return to normal duties' },
		{ label: 'Equipment Check', value: 'Investigate and repair malfunctioning equipment' },
		{ label: 'Documentation', value: 'Document false alarm and cause' }
	];

	const systemImprovements: Row[] = [
		{ label: 'Equipment Maintenance', value: 'Immediate repair of faulty equipment' },
		{ label: 'Staff Education', value: 'Review proper code blue activation' },
		{ label: 'System Calibration', value: 'Adjust sensitivity to prevent false alarms' },
		{ label: 'Audit Trail', value: 'Record incident for system improvement' }
	];
</script>

{#snippet detailTable(rows: Row[])}
	<table>
		<tbody>
			{#each rows as row (row.label)}
				<tr>
					<th>{row.label}</th>
					<td data-testid={slug(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
{/snippet}

<div class="card" data-testid="code-blue-response-panel">
	<h1 class="panel-heading">Code Blue Response</h1>
	<p class="panel-subtitle">
		Rapidly activate emergency response for cardiac arrest patients, with coordinated team
		notification and documentation.
	</p>

	<Button type="button" class="btn" data-testid="code-blue-button" onclick={activateCodeBlue}>
		Activate Code Blue
	</Button>

	{#if codeBlueActivated}
		<div class="card" data-type="error" role="alert" data-testid="code-blue-active-alert">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Code Blue Activated - Bed ED-5</h2>
			{@render detailTable(alertActivationDetails)}

			<h3>Code Team Alerted</h3>
			<ul>
				{#each codeTeamAlerts as alert (alert.member)}
					<li data-testid="code-team-alert-entry">
						{alert.member} — {alert.method} — {alert.responseTime}
					</li>
				{/each}
			</ul>

			<h3>Patient Location Displayed on All Devices</h3>
			{@render detailTable(patientLocationDisplays)}

			<h3>Resuscitation Documentation Template</h3>
			{@render detailTable(documentationTemplateSections)}
		</div>
	{/if}

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Automatic Equipment Coordination</h2>
		{@render detailTable(equipmentAlerts)}
		{@render detailTable(equipmentTracking)}
		{@render detailTable(backupEquipment)}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">
			Real-time Resuscitation Documentation
		</h2>
		{@render detailTable(documentedActivities)}
		{@render detailTable(timelineEntries)}
		{@render detailTable(qualityMetrics)}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">
			Return of Spontaneous Circulation (ROSC)
		</h2>
		{@render detailTable(codeStatusUpdates)}
		{@render detailTable(postRoscProtocols)}
		{@render detailTable(familyNotificationProcedures)}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">End-of-Life Care Transition</h2>
		{@render detailTable(endOfLifeDocumentation)}
		{@render detailTable(familySupportProcedures)}
		{@render detailTable(administrativeProcesses)}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Family Presence During Code Events</h2>
		{@render detailTable(familyManagementProtocols)}
		{@render detailTable(visitorAreaManagement)}
		{@render detailTable(familyPreferenceAccommodation)}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">
			Quality Improvement &amp; Performance Metrics
		</h2>
		{@render detailTable(performanceMetrics)}
		{@render detailTable(qiDataElements)}
		{@render detailTable(improvementOpportunities)}
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">False Alarm Handling</h2>
		{@render detailTable(falseAlarmProtocol)}
		{@render detailTable(systemImprovements)}
		<p role="status" data-type="success" data-testid="system-operations-status">
			Normal operations resumed; lessons learned integrated into protocols.
		</p>
	</div>
</div>
