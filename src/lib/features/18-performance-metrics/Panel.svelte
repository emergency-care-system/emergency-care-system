<script lang="ts">
	// Panel for tests-with-given-when-then-features/18-performance-metrics.feature.
	// Fictitious, client-only monthly performance report. All figures below
	// are scripted demo data representing "May 2025" so the panel reads like
	// a report an ED manager would generate for the previous month.
	import { Button } from 'lily-design-system-svelte-headless';

	type Row = { label: string; value: string };

	function kebab(label: string): string {
		return label
			.trim()
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/(^-|-$)/g, '');
	}

	let reportMonth = $state('');

	const coreMetricCategoryRows: Row[] = [
		{ label: 'Length of Stay', value: 'Average, median, 95th percentile by ESI' },
		{ label: 'LWBS Rates', value: 'Left without being seen percentages' },
		{ label: 'Patient Satisfaction', value: 'Overall scores and domain-specific ratings' },
		{ label: 'Throughput Metrics', value: 'Door-to-provider, bed turnaround times' },
		{ label: 'Quality Indicators', value: 'Safety events, readmission rates' }
	];

	const averageLosRows: Row[] = [
		{ label: 'ESI 1 (Critical)', value: '180 minutes' },
		{ label: 'ESI 2 (High Priority)', value: '165 minutes' },
		{ label: 'ESI 3 (Urgent)', value: '145 minutes' },
		{ label: 'ESI 4 (Less Urgent)', value: '95 minutes' },
		{ label: 'ESI 5 (Non-urgent)', value: '75 minutes' },
		{ label: 'Overall Average', value: '132 minutes' }
	];

	const lwbsRateRows: Row[] = [
		{ label: 'Overall LWBS Rate', value: '3.2%' },
		{ label: 'ESI 3 LWBS Rate', value: '4.1%' },
		{ label: 'ESI 4 LWBS Rate', value: '5.8%' },
		{ label: 'ESI 5 LWBS Rate', value: '8.2%' },
		{ label: 'Peak Hours LWBS', value: '6.1%' }
	];

	const satisfactionScoreRows: Row[] = [
		{ label: 'Overall Satisfaction', value: '87.3%' },
		{ label: 'Communication', value: '89.1%' },
		{ label: 'Pain Management', value: '84.7%' },
		{ label: 'Staff Responsiveness', value: '88.9%' },
		{ label: 'Cleanliness', value: '92.4%' },
		{ label: 'Discharge Process', value: '86.2%' }
	];

	const monthOverMonthRows: Row[] = [
		{ label: 'Average Length of Stay Comparison', value: '132 min' },
		{ label: 'LWBS Rate Comparison', value: '3.2%' },
		{ label: 'Patient Satisfaction Comparison', value: '87.3%' },
		{ label: 'Door-to-Provider Comparison', value: '35 min' },
		{ label: 'Bed Turnaround Comparison', value: '28 min' }
	];

	const yearOverYearRows: Row[] = [
		{ label: 'Average LOS YoY', value: '132 min' },
		{ label: 'LWBS Rate YoY', value: '3.2%' },
		{ label: 'Patient Volume YoY', value: '2,847 pts' },
		{ label: 'Satisfaction YoY', value: '87.3%' }
	];

	const statisticalSignificanceRows: Row[] = [
		{ label: 'Length of Stay Significance', value: '0.003' },
		{ label: 'LWBS Rate Significance', value: '0.012' },
		{ label: 'Satisfaction Significance', value: '0.001' }
	];

	const lwbsBreakdownRows: Row[] = [
		{ label: 'Time-based Patterns', value: 'LWBS rates by hour, day of week, shift' },
		{ label: 'Acuity Distribution', value: 'LWBS percentage by ESI level' },
		{ label: 'Wait Time Correlation', value: 'LWBS rates vs wait time thresholds' },
		{ label: 'Seasonal Factors', value: 'Weather, holidays, local events impact' }
	];

	const rootCauseRows: Row[] = [
		{ label: 'Extended Wait Times', value: 'High' },
		{ label: 'Staffing Shortages', value: 'High' },
		{ label: 'Bed Availability', value: 'Medium' },
		{ label: 'Triage Delays', value: 'Medium' }
	];

	const preventionRecommendationRows: Row[] = [
		{ label: 'Fast Track Protocol', value: 'High' },
		{ label: 'Provider Scheduling', value: 'High' },
		{ label: 'Patient Communication', value: 'Medium' },
		{ label: 'Comfort Amenities', value: 'Low' }
	];

	const demographicSatisfactionRows: Row[] = [
		{ label: 'Age 18-35', value: '85.1%' },
		{ label: 'Age 36-55', value: '88.7%' },
		{ label: 'Age 56-75', value: '89.2%' },
		{ label: 'Age 75+', value: '86.8%' },
		{ label: 'Male Patients', value: '86.9%' },
		{ label: 'Female Patients', value: '87.7%' }
	];

	const patientTypeOverallRows: Row[] = [
		{ label: 'Trauma Patients Overall', value: '82.0%' },
		{ label: 'Chest Pain Overall', value: '90.1%' },
		{ label: 'Abdominal Pain Overall', value: '84.4%' },
		{ label: 'Minor Injuries Overall', value: '89.6%' }
	];

	const improvementOpportunityRows: Row[] = [
		{ label: 'Trauma Communication', value: '82.3%' },
		{ label: 'Pain Management Improvement Opportunity', value: '79.1%' },
		{ label: 'Young Adult Experience', value: '85.1%' }
	];

	const qualityIndicatorRows: Row[] = [
		{ label: 'Medication Errors', value: '0.12%' },
		{ label: 'Patient Falls', value: '0 events' },
		{ label: 'Hospital Readmissions', value: '2.1%' },
		{ label: 'Infection Control', value: '99.8%' },
		{ label: 'Adverse Events', value: '3 events' }
	];

	const safetyEventRows: Row[] = [
		{ label: 'Medication Error Count', value: '2 events' },
		{ label: 'Diagnostic Delay Count', value: '1 event' },
		{ label: 'Equipment Failure Count', value: '0 events' }
	];

	const complianceStatusRows: Row[] = [
		{ label: 'Joint Commission', value: 'Compliant' },
		{ label: 'CMS Core Measures', value: 'Compliant' },
		{ label: 'State Regulations', value: 'Compliant' }
	];

	const efficiencyIndicatorRows: Row[] = [
		{ label: 'Cost per Patient', value: '$847' },
		{ label: 'Revenue per Patient', value: '$1,245' },
		{ label: 'Staff Productivity', value: '2.8 pts/hr' },
		{ label: 'Bed Utilization', value: '87.3%' },
		{ label: 'Equipment Uptime', value: '98.7%' }
	];

	const staffingMetricRows: Row[] = [
		{ label: 'RN Hours per Patient', value: '4.2 hours' },
		{ label: 'Physician Coverage', value: '1:12 ratio' },
		{ label: 'Overtime Hours', value: '3.2%' },
		{ label: 'Agency Staff Usage', value: '1.8%' }
	];

	const shiftProductivityRows: Row[] = [
		{ label: 'Day (7a-7p)', value: '3.2' },
		{ label: 'Evening (7p-11p)', value: '2.8' },
		{ label: 'Night (11p-7a)', value: '1.9' }
	];

	const executiveSummaryRows: Row[] = [
		{ label: 'Overall Performance', value: '87% of targets met, significant improvements' },
		{ label: 'Major Achievements', value: 'LOS reduction, LWBS improvement, satisfaction up' },
		{ label: 'Areas of Concern', value: 'Night shift efficiency, young adult satisfaction' },
		{ label: 'Financial Impact', value: '$2.1M revenue, $53 cost reduction per patient' }
	];

	const actionableRecommendationRows: Row[] = [
		{ label: 'Implement fast-track for ESI 4-5', value: '15% LOS reduction' },
		{ label: 'Night shift staffing optimization', value: '10% efficiency ↗' },
		{ label: 'Young adult communication program', value: '3% satisfaction ↗' },
		{ label: 'Comfort amenity upgrades', value: '2% satisfaction ↗' }
	];

	const roiAnalysisRows: Row[] = [
		{ label: '$125K (Fast Track)', value: '$340K' },
		{ label: '$85K (Staffing)', value: '$220K' },
		{ label: '$45K (Communication)', value: '$95K' }
	];

	const nextMonthFocusAreas = [
		'Night shift staffing optimization',
		'Young adult patient experience',
		'Sustaining LWBS improvements'
	];
</script>

<div class="card" data-testid="performance-metrics-panel">
	<h1 class="panel-heading">Performance Metrics</h1>
	<p class="panel-subtitle">
		Comprehensive monthly performance report — length of stay, LWBS, satisfaction, quality, and
		operational efficiency.
	</p>

	<div class="field-row">
		<div class="field">
			<label for="report-month">Report Month</label>
			<input
				id="report-month"
				data-testid="report-month"
				type="text"
				placeholder="e.g. May 2025"
				bind:value={reportMonth}
			/>
		</div>
	</div>

	<div class="field-row" style="margin-bottom: 0.5rem;">
		<Button type="button" class="btn" data-testid="generate-monthly-report">Generate monthly report</Button>
		<Button type="button" class="btn secondary" data-testid="generate-comparative-report">Generate comparative report</Button>
		<Button type="button" class="btn secondary" data-testid="request-lwbs-analysis">Request LWBS analysis</Button>
		<Button type="button" class="btn secondary" data-testid="generate-satisfaction-analysis">Generate satisfaction analysis</Button>
		<Button type="button" class="btn secondary" data-testid="include-quality-metrics">Include quality metrics</Button>
		<Button type="button" class="btn secondary" data-testid="request-operational-metrics">Request operational metrics</Button>
		<Button type="button" class="btn secondary" data-testid="request-executive-summary">Request executive summary</Button>
	</div>
</div>

<div class="card" data-testid="core-metric-category-report">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Core Metric Categories</h2>
	<table>
		<tbody>
			{#each coreMetricCategoryRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Average Length of Stay by ESI Level</h2>
	<table>
		<tbody>
			{#each averageLosRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Left Without Being Seen (LWBS) Rates</h2>
	<table>
		<tbody>
			{#each lwbsRateRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Patient Satisfaction Scores</h2>
	<table>
		<tbody>
			{#each satisfactionScoreRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Month-over-Month Comparison</h2>
	<table>
		<tbody>
			{#each monthOverMonthRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Year-over-Year Comparison</h3>
	<table>
		<tbody>
			{#each yearOverYearRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Statistical Significance</h3>
	<table>
		<tbody>
			{#each statisticalSignificanceRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">LWBS Root Cause Analysis</h2>
	<table>
		<tbody>
			{#each lwbsBreakdownRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Contributing Factors</h3>
	<table>
		<tbody>
			{#each rootCauseRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Prevention Recommendations</h3>
	<table>
		<tbody>
			{#each preventionRecommendationRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Satisfaction Demographic Breakdown</h2>
	<table>
		<tbody>
			{#each demographicSatisfactionRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Satisfaction by Patient Type</h3>
	<table>
		<tbody>
			{#each patientTypeOverallRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Improvement Opportunities</h3>
	<table>
		<tbody>
			{#each improvementOpportunityRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Quality &amp; Safety Metrics</h2>
	<table>
		<tbody>
			{#each qualityIndicatorRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Safety Event Analysis</h3>
	<table>
		<tbody>
			{#each safetyEventRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Regulatory Compliance</h3>
	<table>
		<tbody>
			{#each complianceStatusRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Financial &amp; Operational Efficiency</h2>
	<table>
		<tbody>
			{#each efficiencyIndicatorRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Staffing Metrics</h3>
	<table>
		<tbody>
			{#each staffingMetricRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Productivity by Shift</h3>
	<table>
		<tbody>
			{#each shiftProductivityRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<div class="card">
	<h2 class="panel-heading" style="font-size: 1.05rem;">Executive Summary</h2>
	<table>
		<tbody>
			{#each executiveSummaryRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Actionable Recommendations</h3>
	<table>
		<tbody>
			{#each actionableRecommendationRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">ROI Analysis</h3>
	<table>
		<tbody>
			{#each roiAnalysisRows as row (row.label)}
				<tr>
					<td>{row.label}</td>
					<td data-testid={kebab(row.label)}>{row.value}</td>
				</tr>
			{/each}
		</tbody>
	</table>
	<h3 class="panel-heading" style="font-size: 0.95rem;">Next Month's Focus Areas</h3>
	<ul>
		{#each nextMonthFocusAreas as area, index (index)}
			<li data-testid="next-month-focus-area">{area}</li>
		{/each}
	</ul>
</div>
