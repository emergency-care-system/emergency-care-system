<script lang="ts">
	// Panel for tests-with-given-when-then-features/17-real-time-dashboard.feature.
	// Fictitious, client-only real-time department dashboard. All figures
	// below are scripted demo data (not live-computed) so the panel reads
	// like a snapshot a charge nurse would see at 14:30 on a busy Tuesday.
	import { Button } from 'lily-design-system-svelte-headless';

	type Row = { label: string; value: string };

	function kebab(label: string): string {
		return label
			.trim()
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/(^-|-$)/g, '');
	}

	const censusRows: Row[] = [
		{ label: 'Total Patients', value: '18 (90% capacity) - Yellow indicator' },
		{ label: 'Admitted Patients', value: '14 currently in beds' },
		{ label: 'Waiting Patients', value: '12 in triage queue - Red alert' },
		{ label: 'Discharged Today', value: '23 patients processed' }
	];

	const waitTimeRows: Row[] = [
		{ label: 'Triage to Bed', value: '45 minutes' },
		{ label: 'Bed to Provider', value: '25 minutes' },
		{ label: 'Provider to Discharge', value: '120 minutes' },
		{ label: 'Total ED Length of Stay', value: '190 minutes' }
	];

	const bedStatusRows: Row[] = [
		{ label: 'Available Clean', value: '2' },
		{ label: 'Needs Cleaning', value: '2' },
		{ label: 'Occupied', value: '16' },
		{ label: 'Out of Service', value: '0' }
	];

	const staffAssignmentRows: Row[] = [
		{ label: 'Dr. Smith', value: '3 patients' },
		{ label: 'Dr. Johnson', value: '4 patients' },
		{ label: 'Nurse Martinez', value: '5 patients' },
		{ label: 'Nurse Chen', value: '4 patients' }
	];

	const updatedMetricRows: Row[] = [
		{ label: 'Patient Count', value: '19 (95% capacity) - Red indicator' },
		{ label: 'Bed Availability', value: '1 available - Critical level alert' },
		{ label: 'Wait Time Alert', value: 'Triage wait exceeds 45 min threshold' },
		{ label: 'Staff Status', value: 'Dr. Smith now unavailable' }
	];

	const visualUpdateRows: Row[] = [
		{ label: 'Capacity Indicator', value: 'Yellow → Red (approaching full capacity)' },
		{ label: 'Bed Status Alert', value: 'New warning for low bed availability' },
		{ label: 'Provider Icon', value: 'Dr. Smith icon changes to "busy" status' }
	];

	const alertPriorityRows: Row[] = [
		{ label: 'CRITICAL', value: '🚨 Red flashing banner at top of screen' },
		{ label: 'HIGH', value: '🟠 Orange banner with urgent icon' },
		{ label: 'MEDIUM', value: '🟡 Yellow notification strip' },
		{ label: 'INFO', value: '🔵 Blue informational indicator' }
	];

	const alertDetailRows: Row[] = [
		{ label: 'Capacity Critical', value: 'ED at 95% capacity - Consider diversion' },
		{ label: 'Wait Time Excessive', value: 'Triage wait: 65 min - Expedite process' },
		{ label: 'ESI 1 Patient', value: 'Critical patient waiting - Bed needed' },
		{ label: 'Staffing Concern', value: 'Provider ratio 1:5 - Additional coverage needed' }
	];

	const alertHistory = [
		'14:32 — CRITICAL: ED at 95% capacity - Consider diversion',
		'14:15 — HIGH: Triage wait 65 min - Expedite process',
		'13:58 — MEDIUM: Provider ratio 1:5 - Additional coverage needed'
	];

	const flowMetricRows: Row[] = [
		{ label: 'Patients per Hour', value: '3.2 arrivals' },
		{ label: 'Discharge Rate', value: '2.1 per hour' },
		{ label: 'Bed Turnover Time', value: '35 minutes' },
		{ label: 'Left Without Being Seen', value: '2 patients' }
	];

	const trendGraphRows: Row[] = [
		{ label: 'Census Trend', value: '24 hours' },
		{ label: 'Wait Time Trend', value: '12 hours' },
		{ label: 'Capacity Utilization', value: '7 days' },
		{ label: 'Provider Productivity', value: 'Shift' }
	];

	const predictiveIndicatorRows: Row[] = [
		{ label: 'Peak Time Prediction', value: 'Expected surge at 18:00-20:00' },
		{ label: 'Capacity Projection', value: 'Will reach 100% capacity in 90 minutes' },
		{ label: 'Staffing Needs', value: 'Additional provider needed by 16:00' }
	];

	const dashboardSectionRows: Row[] = [
		{ label: 'Resource Management', value: 'Bed status, staffing levels, equipment' },
		{ label: 'Quality Metrics', value: 'Wait times, satisfaction scores, safety' },
		{ label: 'Operational Alerts', value: 'Capacity issues, workflow bottlenecks' },
		{ label: 'Staff Coordination', value: 'Break schedules, assignments, coverage' }
	];

	const widgetConfigurationRows: Row[] = [
		{ label: 'Bed Management Grid', value: 'Color-coded bed status with room numbers' },
		{ label: 'Provider Status Board', value: 'Real-time availability and patient load' },
		{ label: 'Queue Management', value: 'ESI-sorted patient list with wait times' },
		{ label: 'Performance KPIs', value: 'Key metrics with targets and trends' }
	];

	const crisisDisplayRows: Row[] = [
		{ label: 'Mode Banner', value: 'Red "CRISIS MODE ACTIVE" across top' },
		{ label: 'Protocol Status', value: 'Active emergency protocols listed' },
		{ label: 'Resource Allocation', value: 'Special staffing and bed assignments' },
		{ label: 'Communication Center', value: 'Emergency contact information prominent' }
	];

	const crisisMetricRows: Row[] = [
		{ label: 'Response Teams', value: 'Available emergency response personnel' },
		{ label: 'Special Equipment', value: 'Crisis supplies and equipment status' },
		{ label: 'External Coordination', value: 'Communication with EMS, other hospitals' },
		{ label: 'Surge Capacity', value: 'Additional beds and overflow areas' }
	];

	const mobileFeatureRows: Row[] = [
		{ label: 'Summary Cards', value: 'Key metrics in swipeable card format' },
		{ label: 'Alert Notifications', value: 'Push notifications for critical alerts' },
		{ label: 'Quick Actions', value: 'Rapid access to common charge nurse tasks' },
		{ label: 'Touch Interface', value: 'Optimized for touch navigation' }
	];

	const priorityDisplayRows: Row[] = [
		{ label: 'Critical Alerts', value: 'Top of screen with prominent notification' },
		{ label: 'Bed Status Summary', value: 'Visual grid with color-coded indicators' },
		{ label: 'Staff Availability', value: 'Provider status with quick contact options' },
		{ label: 'Key Metrics', value: 'Current census, wait times, alerts' }
	];

	const comparisonTypeRows: Row[] = [
		{ label: 'Same Day Last Week', value: 'Tuesday to Tuesday comparison' },
		{ label: 'Monthly Average', value: 'Current day vs monthly average' },
		{ label: 'Seasonal Patterns', value: 'Year-over-year seasonal comparison' },
		{ label: 'Shift Comparisons', value: 'Day vs evening vs night shift metrics' }
	];

	const benchmarkRows: Row[] = [
		{ label: 'Internal Targets', value: 'Hospital-specific performance goals' },
		{ label: 'Industry Standards', value: 'National ED performance benchmarks' },
		{ label: 'Peer Comparison', value: 'Similar-sized ED performance data' }
	];

	const varianceAlertRows: Row[] = [
		{ label: 'Significant Increase', value: '>20% above normal pattern' },
		{ label: 'Significant Decrease', value: '>15% below expected performance' },
		{ label: 'Unusual Pattern', value: 'Unexpected trends or anomalies' }
	];

	const performanceRecommendations = [
		'Expedite triage-to-bed process to bring wait times under the 30-minute target',
		'Add a provider during the predicted 18:00-20:00 surge window',
		'Review bed turnover workflow with housekeeping to reduce cleaning delays'
	];

	let activeSection = $state('main');
</script>

<div class="card" data-testid="real-time-dashboard-panel">
	<h1 class="panel-heading">Real-time Dashboard</h1>
	<p class="panel-subtitle">
		Live departmental status — census, wait times, beds, staffing, and alerts — refreshed every
		30 seconds.
	</p>

	<div class="field-row" style="margin-bottom: 1rem;">
		<Button type="button" class="btn secondary" data-testid="main-dashboard-nav" onclick={() => (activeSection = 'main')}>
			Main Dashboard
		</Button>
		<Button type="button" class="btn secondary" data-testid="detailed-metrics-nav" onclick={() => (activeSection = 'detailed')}>
			Detailed Metrics
		</Button>
		<Button type="button" class="btn secondary" data-testid="personalized-dashboard-nav" onclick={() => (activeSection = 'personalized')}>
			Personalized View
		</Button>
		<Button type="button" class="btn secondary" data-testid="trend-analysis-nav" onclick={() => (activeSection = 'trend')}>
			Trend Analysis
		</Button>
	</div>

	<p data-testid="last-update-timestamp">Updated: 15:00:30</p>
</div>

<div class="card" data-testid="patient-census-display">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Patient Census</h2>
	<table>
		<tbody>
			{#each censusRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Average Wait Times</h2>
	<table>
		<tbody>
			{#each waitTimeRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Bed Availability</h2>
	<table>
		<tbody>
			{#each bedStatusRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Staff Assignments</h2>
	<table>
		<tbody>
			{#each staffAssignmentRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Real-time Update</h2>
	<table>
		<tbody>
			{#each updatedMetricRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
			{#each visualUpdateRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<div class="card" data-type="error" role="alert" data-testid="critical-alert-indicator">
		🚨 Critical alert — flashing indicator active
	</div>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Priority Alerts</h2>
	<table>
		<tbody>
			{#each alertPriorityRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
			{#each alertDetailRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<p data-type="warning" role="alert" data-testid="alert-acknowledgment-required">
		Acknowledgment required for all CRITICAL and HIGH priority alerts.
	</p>
	<ul>
		{#each alertHistory as entry, index (index)}
			<li data-testid="alert-history-entry">{entry}</li>
		{/each}
	</ul>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Detailed Patient Flow Metrics</h2>
	<table>
		<tbody>
			{#each flowMetricRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Trending Graphs</h3>
	<table>
		<tbody>
			{#each trendGraphRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Predictive Indicators</h3>
	<table>
		<tbody>
			{#each predictiveIndicatorRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Personalized Dashboard (Charge Nurse)</h2>
	<table>
		<tbody>
			{#each dashboardSectionRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Customizable Widgets</h3>
	<table>
		<tbody>
			{#each widgetConfigurationRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<div data-testid="layout-editor">
		Drag widgets to rearrange the layout and set priority order.
	</div>
	<p role="status" data-type="success" data-testid="preferences-saved-status">
		Preferences saved for future sessions.
	</p>
</div>

<div class="card" data-type="warning">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Crisis Mode Indicators</h2>
	<table>
		<tbody>
			{#each crisisDisplayRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<table>
		<tbody>
			{#each crisisMetricRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<div data-testid="crisis-specific-data">
		Normal operations metrics are supplemented with crisis-specific data during an active event.
	</div>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Mobile View</h2>
	<table>
		<tbody>
			{#each mobileFeatureRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
			{#each priorityDisplayRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<p data-testid="sync-status">Synchronized in real-time between desktop and mobile views.</p>
	<div data-testid="offline-status-indicator">
		Offline mode: last-known status will be preserved if connectivity is lost.
	</div>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Historical Comparison &amp; Trend Analysis</h2>
	<table>
		<tbody>
			{#each comparisonTypeRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Performance Benchmarks</h3>
	<table>
		<tbody>
			{#each benchmarkRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Variance Analysis</h3>
	<table>
		<tbody>
			{#each varianceAlertRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Recommendations</h3>
	<ul>
		{#each performanceRecommendations as recommendation, index (index)}
			<li data-testid="performance-recommendation">{recommendation}</li>
		{/each}
	</ul>
</div>
