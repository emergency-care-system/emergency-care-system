<script lang="ts">
	// Panel for tests-with-given-when-then-features/19-mass-casualty-activation.feature.
	// Fictitious, client-only mass casualty incident (MCI) activation panel.
	// All figures below are scripted demo data for the "multi-vehicle
	// accident" scenario family described in the feature file.
	import { Button } from 'lily-design-system-svelte-headless';

	type Row = { label: string; value: string };

	function kebab(label: string): string {
		return label
			.trim()
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/(^-|-$)/g, '');
	}

	const systemChangeRows: Row[] = [
		{ label: 'Mode Indicator', value: '"MASS CASUALTY ACTIVE" banner displayed' },
		{ label: 'Interface Switch', value: 'MCI-specific workflows activated' },
		{ label: 'Normal Operations', value: 'Routine tasks suspended/deprioritized' },
		{ label: 'Resource Allocation', value: 'Emergency resource management enabled' },
		{ label: 'Communication Mode', value: 'Critical alerts and notifications active' }
	];

	const staffAlertRows: Row[] = [
		{ label: 'Off-duty Physicians', value: 'SMS, Phone call' },
		{ label: 'Off-duty Nurses', value: 'SMS, Phone call' },
		{ label: 'Surgical Team', value: 'Overhead page, SMS' },
		{ label: 'Lab/Radiology', value: 'System alert, Phone' },
		{ label: 'Administration', value: 'Phone call, SMS' },
		{ label: 'Security', value: 'Radio, Overhead page' }
	];

	const registrationFeatureRows: Row[] = [
		{ label: 'Patient Identification', value: 'Sequential numbering: MCI-001, MCI-002' },
		{ label: 'Triage Tags', value: 'Color-coded electronic tags' },
		{ label: 'Minimal Data Entry', value: 'Name, age, chief complaint only' },
		{ label: 'Family Notification', value: 'Automated family alert system' },
		{ label: 'Tracking Board', value: 'Real-time patient status dashboard' }
	];

	const surgeAreaRows: Row[] = [
		{ label: 'Hallway Beds', value: '+6 treatment spaces' },
		{ label: 'Procedure Rooms', value: '+3 converted spaces' },
		{ label: 'Observation Area', value: '+8 holding spaces' },
		{ label: 'Waiting Room Triage', value: '+4 assessment areas' },
		{ label: 'Ambulatory Care', value: '+10 walking wounded' }
	];

	const resourceAllocationRows: Row[] = [
		{ label: 'Trauma Bays', value: 'All 4 activated' },
		{ label: 'Operating Rooms', value: '3 rooms on standby' },
		{ label: 'Ventilators', value: '8 total (5 from ICU)' },
		{ label: 'Blood Products', value: 'Massive transfusion protocol' },
		{ label: 'Medication Carts', value: '5 carts deployed' }
	];

	const staffingRatioRows: Row[] = [
		{ label: 'Physicians', value: '1:12 patients' },
		{ label: 'Nurses', value: '1:6 patients' },
		{ label: 'Support Staff', value: 'Double coverage' }
	];

	const registrationStepRows: Row[] = [
		{ label: 'Patient Arrival', value: 'Immediate tag assignment: MCI-001, 002, 003' },
		{ label: 'Triage Assessment', value: 'START triage protocol applied' },
		{ label: 'Electronic Tagging', value: 'Color-coded digital tags assigned' },
		{ label: 'Minimal Documentation', value: 'Name, estimated age, mechanism of injury' },
		{ label: 'Bed Assignment', value: 'Automatic assignment by acuity' }
	];

	const triageColorRows: Row[] = [
		{ label: 'Red (Immediate)', value: 'MCI-001 → Trauma Bay 1' },
		{ label: 'Yellow (Delayed)', value: 'MCI-002 → Surge Bed 3' },
		{ label: 'Green (Minor)', value: 'MCI-003 → Ambulatory Area' },
		{ label: 'Black (Deceased)', value: 'Morgue coordination' }
	];

	const notificationProcessRows: Row[] = [
		{ label: 'Emergency Contacts', value: 'Auto-dial from patient personal effects' },
		{ label: 'Public Information', value: 'Hospital hotline number broadcasted' },
		{ label: 'Media Coordination', value: 'Incident command liaison activated' },
		{ label: 'Social Services', value: 'Family support team mobilized' }
	];

	const communicationChannelRows: Row[] = [
		{ label: 'EMS Command Center', value: 'Patient distribution and transport updates' },
		{ label: 'Other Area Hospitals', value: 'Bed availability and transfer coordination' },
		{ label: 'Air Medical Services', value: 'Helicopter transport for critical patients' },
		{ label: 'Regional Trauma Centers', value: 'Specialty care transfer arrangements' }
	];

	const distributionStrategyRows: Row[] = [
		{ label: 'Load Balancing', value: 'Distribute patients across regional facilities' },
		{ label: 'Specialty Matching', value: 'Route patients to appropriate specialty care' },
		{ label: 'Capacity Monitoring', value: 'Real-time bed availability tracking' },
		{ label: 'Transport Coordination', value: 'Ambulance and helicopter scheduling' }
	];

	const integrationElementRows: Row[] = [
		{ label: 'Incident Command', value: 'Hospital EOC links with regional ICS' },
		{ label: 'Resource Sharing', value: 'Equipment and staff sharing protocols' },
		{ label: 'Information Sharing', value: 'Patient status updates to command center' },
		{ label: 'Media Management', value: 'Coordinated public information releases' }
	];

	const familySupportSystemRows: Row[] = [
		{ label: 'Information Hotline', value: 'Dedicated phone line with trained staff' },
		{ label: 'Family Reunification', value: 'Secure area for family waiting and updates' },
		{ label: 'Patient Tracking', value: 'Real-time status board for authorized viewers' },
		{ label: 'Privacy Protection', value: 'HIPAA-compliant information sharing' }
	];

	const notificationMethodRows: Row[] = [
		{ label: 'SMS Updates', value: 'Your family member is being treated safely' },
		{ label: 'Phone Calls', value: 'Personal calls for critical status changes' },
		{ label: 'Information Boards', value: 'General incident updates (no patient names)' },
		{ label: 'Social Workers', value: 'One-on-one family support and counseling' }
	];

	const trackingElementRows: Row[] = [
		{ label: 'Current Location', value: 'Treatment area, OR, transferred, etc.' },
		{ label: 'Medical Status', value: 'Stable, critical, treated and released' },
		{ label: 'Next of Kin Contact', value: 'Verification and notification status' },
		{ label: 'Discharge Planning', value: 'Expected timeline and care needs' }
	];

	const deactivationStepRows: Row[] = [
		{ label: 'Incident Assessment', value: 'Review of patient outcomes and resources used' },
		{ label: 'Staff Debriefing', value: 'Immediate hot wash and formal debriefing' },
		{ label: 'Resource Restoration', value: 'Return equipment and supplies to normal areas' },
		{ label: 'Documentation', value: 'Complete incident documentation and reports' }
	];

	const restorationPhaseRows: Row[] = [
		{ label: 'Immediate (0-30 min)', value: 'Secure scene' },
		{ label: 'Short-term (30-60 min)', value: 'Resource cleanup' },
		{ label: 'Medium-term (1-4 hrs)', value: 'Staff rotation' },
		{ label: 'Long-term (4-24 hrs)', value: 'Full restoration' }
	];

	const qiActivityRows: Row[] = [
		{ label: 'After Action Review', value: 'Identify strengths and improvement areas' },
		{ label: 'Performance Metrics', value: 'Analyze response times and patient outcomes' },
		{ label: 'Protocol Updates', value: 'Revise procedures based on lessons learned' },
		{ label: 'Training Needs', value: 'Identify staff training and education needs' }
	];

	const staffingStrategyRows: Row[] = [
		{ label: 'Shift Hold', value: 'Day shift staff remain for incident response' },
		{ label: 'Double Coverage', value: 'Both shifts work together during surge' },
		{ label: 'Incident Command Continuity', value: 'Clear leadership chain established' },
		{ label: 'Communication', value: 'All staff briefed on roles and responsibilities' }
	];

	const modifiedProtocolRows: Row[] = [
		{ label: 'Handoff Procedures', value: 'Suspended until incident resolution' },
		{ label: 'Staffing Ratios', value: 'Enhanced coverage with both shifts' },
		{ label: 'Leadership Structure', value: 'Incident commander takes operational control' },
		{ label: 'Modified Documentation', value: 'Emergency documentation procedures active' }
	];

	const drillFeatureRows: Row[] = [
		{ label: 'Test Mode Indicator', value: '"DRILL - NOT REAL EMERGENCY" displayed' },
		{ label: 'Simulated Patients', value: 'Test patient records created' },
		{ label: 'Staff Participation', value: 'All roles and responsibilities tested' },
		{ label: 'Resource Tracking', value: 'Equipment and supplies tracked but not used' }
	];

	const drillPerformanceMetricRows: Row[] = [
		{ label: 'Activation Time', value: 'Time from alert to full surge capacity' },
		{ label: 'Staff Response Time', value: 'Time for staff to report and assume roles' },
		{ label: 'Communication Speed', value: 'Time for all notifications to be completed' },
		{ label: 'Resource Deployment', value: 'Time to set up surge areas and equipment' }
	];

	const evaluationComponentRows: Row[] = [
		{ label: 'Protocol Effectiveness', value: 'How well procedures worked in practice' },
		{ label: 'Staff Preparedness', value: 'Knowledge and skill gaps identified' },
		{ label: 'System Performance', value: 'Technology and workflow efficiency' },
		{ label: 'Improvement Plans', value: 'Action items for enhancing response capabilities' }
	];

	let activated = $state(false);
	let transitioned = $state(false);
	let drillActive = $state(false);
</script>

<div class="card" data-testid="mass-casualty-activation-panel">
	<h1 class="panel-heading">Mass Casualty Activation</h1>
	<p class="panel-subtitle">
		Rapidly activate emergency protocols during mass casualty incidents to manage multiple
		critically injured patients.
	</p>

	<div class="field-row">
		<Button
			type="button"
			class="btn"
			data-testid="activate-disaster-protocol"
			onclick={() => (activated = true)}
		>
			Activate disaster protocol
		</Button>
		<Button
			type="button"
			class="btn secondary"
			data-testid="initiate-transition-to-normal"
			onclick={() => (transitioned = true)}
		>
			Initiate transition to normal operations
		</Button>
		<Button
			type="button"
			class="btn secondary"
			data-testid="activate-test-mci-protocol"
			onclick={() => (drillActive = true)}
		>
			Activate test MCI protocol (drill)
		</Button>
	</div>
</div>

<div class="card" data-type="error">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Surge Capacity Mode — System Changes</h2>
	<table>
		<tbody>
			{#each systemChangeRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Staff Alerting</h2>
	<table>
		<tbody>
			{#each staffAlertRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Rapid Registration Workflow</h2>
	<table>
		<tbody>
			{#each registrationFeatureRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Surge Treatment Areas</h2>
	<table>
		<tbody>
			{#each surgeAreaRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Resource Allocation</h3>
	<table>
		<tbody>
			{#each resourceAllocationRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Staffing Ratios</h3>
	<table>
		<tbody>
			{#each staffingRatioRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Rapid Patient Registration &amp; Triage</h2>
	<table>
		<tbody>
			{#each registrationStepRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">START Triage Color Coding</h3>
	<table>
		<tbody>
			{#each triageColorRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Family Notification</h3>
	<table>
		<tbody>
			{#each notificationProcessRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">External Coordination</h2>
	<table>
		<tbody>
			{#each communicationChannelRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Patient Distribution</h3>
	<table>
		<tbody>
			{#each distributionStrategyRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Regional Integration</h3>
	<table>
		<tbody>
			{#each integrationElementRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Family Reunification &amp; Information Center</h2>
	<table>
		<tbody>
			{#each familySupportSystemRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Family Notification Methods</h3>
	<table>
		<tbody>
			{#each notificationMethodRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Patient Status Tracking</h3>
	<table>
		<tbody>
			{#each trackingElementRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Transition Back to Normal Operations</h2>
	<table>
		<tbody>
			{#each deactivationStepRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Restoration Timeline</h3>
	<table>
		<tbody>
			{#each restorationPhaseRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Quality Improvement</h3>
	<table>
		<tbody>
			{#each qiActivityRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Shift Change Management</h2>
	<table>
		<tbody>
			{#each staffingStrategyRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Modified Protocols</h3>
	<table>
		<tbody>
			{#each modifiedProtocolRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card" data-type="info">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Quarterly Drill Mode</h2>
	<table>
		<tbody>
			{#each drillFeatureRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Drill Performance Metrics</h3>
	<table>
		<tbody>
			{#each drillPerformanceMetricRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Drill Evaluation</h3>
	<table>
		<tbody>
			{#each evaluationComponentRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>
