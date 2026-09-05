<script lang="ts">
	// Panel for spec/features/22-audit-trail.feature.
	//
	// Fictitious, client-only audit trail demo for a compliance officer. A
	// small seed access log is shown on load. Searching for a patient and
	// generating a report renders the scripted output for that scenario's
	// exact patient (Jennifer Rodriguez -> standard access log, Robert
	// Thompson -> suspicious-access investigation); the other report buttons
	// each drive their own scripted scenario output.
	import { Button } from 'lily-design-system-svelte-headless';

	type Row = { label: string; value: string };

	function slug(label: string): string {
		return label
			.trim()
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/(^-|-$)/g, '');
	}

	type AccessLogUser = {
		access: number;
		name: string;
		role: string;
		department: string;
		purpose: string;
		loginTime: string;
		logoutTime: string;
		duration: string;
		dataElements: string;
		actions: string;
	};

	// Scenario: Review comprehensive access log for frequently accessed patient record
	const accessLogUsers: AccessLogUser[] = [
		{
			access: 1,
			name: 'Dr. Sarah Kim',
			role: 'Emergency Physician',
			department: 'Emergency Dept',
			purpose: 'Direct patient care',
			loginTime: '2025-06-20 14:15:22',
			logoutTime: '2025-06-20 14:45:10',
			duration: '29 min 48 sec',
			dataElements: 'Demographics, Chief complaint, Vital signs, Assessment, Orders',
			actions: 'View, Edit, Create'
		},
		{
			access: 2,
			name: 'Nurse Johnson',
			role: 'Registered Nurse',
			department: 'Emergency Dept',
			purpose: 'Direct patient care',
			loginTime: '2025-06-20 14:20:15',
			logoutTime: '2025-06-20 16:30:22',
			duration: '2 hr 10 min 7 sec',
			dataElements: 'Vital signs, Medications, Allergies, Care plans',
			actions: 'View, Edit, Document'
		},
		{
			access: 3,
			name: 'Tech Martinez',
			role: 'Lab Technician',
			department: 'Laboratory',
			purpose: 'Lab result entry',
			loginTime: '2025-06-20 15:22:45',
			logoutTime: '2025-06-20 15:25:12',
			duration: '2 min 27 sec',
			dataElements: 'Lab orders, Lab results',
			actions: 'View, Enter results'
		},
		{
			access: 4,
			name: 'Dr. Chen',
			role: 'Radiologist',
			department: 'Radiology',
			purpose: 'Image interpretation',
			loginTime: '2025-06-20 16:10:33',
			logoutTime: '2025-06-20 16:18:45',
			duration: '8 min 12 sec',
			dataElements: 'Imaging orders, Radiology reports',
			actions: 'View, Create report'
		},
		{
			access: 5,
			name: 'Billing Clerk Adams',
			role: 'Billing Specialist',
			department: 'Patient Financial',
			purpose: 'Billing/coding',
			loginTime: '2025-06-21 09:15:20',
			logoutTime: '2025-06-21 09:22:15',
			duration: '6 min 55 sec',
			dataElements: 'Diagnosis codes, Procedures, Insurance info',
			actions: 'View only'
		},
		{
			access: 6,
			name: 'Case Mgr Wilson',
			role: 'Case Manager',
			department: 'Social Services',
			purpose: 'Discharge planning',
			loginTime: '2025-06-21 11:30:10',
			logoutTime: '2025-06-21 11:45:33',
			duration: '15 min 23 sec',
			dataElements: 'Discharge plans, Insurance, Social history',
			actions: 'View, Edit'
		},
		{
			access: 7,
			name: 'Dr. Patel',
			role: 'Cardiologist',
			department: 'Cardiology',
			purpose: 'Consultation',
			loginTime: '2025-06-22 10:22:18',
			logoutTime: '2025-06-22 10:35:45',
			duration: '13 min 27 sec',
			dataElements: 'Cardiac tests, Consultation notes',
			actions: 'View, Create notes'
		}
	];

	// Scenario: Investigate suspicious access pattern for patient record
	const suspiciousActivity: Row[] = [
		{ label: 'Unauthorized User', value: 'Dr. Williams (Orthopedics) - No treatment relationship' },
		{ label: 'Unusual Timing', value: 'Access at 11:45 PM on June 16 (outside normal hours)' },
		{ label: 'Excessive Duration', value: '45-minute session for patient not under care' },
		{ label: 'Data Mining Pattern', value: 'Accessed multiple unrelated patient records same night' }
	];

	const forensicData: Row[] = [
		{ label: 'IP Address', value: "192.168.1.205 (Dr. Williams' office computer)" },
		{ label: 'Workstation ID', value: 'WS-ORTHO-03' },
		{ label: 'Access Method', value: 'Valid credentials, no badge scan' },
		{ label: 'Previous Pattern', value: 'First time accessing this patient' },
		{ label: 'Concurrent Activity', value: 'Accessed 8 other unrelated patients same session' }
	];

	const violationIndicators: Row[] = [
		{ label: 'No Treatment Relationship', value: 'No medical necessity for access' },
		{ label: 'Excessive Access', value: 'Viewed entire medical history unnecessarily' },
		{ label: 'Pattern of Behavior', value: 'Multiple inappropriate accesses detected' },
		{ label: 'Time-based Concern', value: 'Access outside normal work hours' }
	];

	const securityAlerts: Row[] = [
		{ label: 'Privacy Officer', value: 'Immediate alert sent for investigation' },
		{ label: 'Department Head', value: 'Orthopedics supervisor notified' },
		{ label: 'IT Security', value: 'Account flagged for enhanced monitoring' },
		{ label: 'Risk Management', value: 'Potential HIPAA violation logged' }
	];

	// Scenario: Generate audit report for break-the-glass emergency access
	const breakTheGlassDocumentation: Row[] = [
		{ label: 'Access Type', value: 'Break-the-glass emergency override' },
		{ label: 'Medical Justification', value: 'Patient unconscious, life-threatening condition' },
		{ label: 'Authorizing Physician', value: 'Dr. Emergency Chief (Emergency Department Head)' },
		{ label: 'Access Duration', value: '2 hours during critical care period' },
		{ label: 'Override Reason', value: 'Unable to obtain consent, medical emergency' }
	];

	const emergencyAccessActivities: Row[] = [
		{ label: 'Records Accessed', value: 'Previous ED visits, medication allergies, medical history' },
		{ label: 'Users Involved', value: 'Dr. Sarah Kim, Nurse Johnson, Pharmacist Lee' },
		{ label: 'Data Viewed', value: 'Allergies, medications, past procedures' },
		{ label: 'Clinical Decisions', value: 'Medication choices based on allergy history' },
		{ label: 'Patient Outcome', value: 'Successful treatment, patient stabilized' }
	];

	const reviewRequirements: Row[] = [
		{ label: 'Medical Necessity', value: 'Clinical justification documented' },
		{ label: 'Minimum Necessary', value: 'Only essential health information accessed' },
		{ label: 'Patient Notification', value: 'Patient to be informed of emergency access when able' },
		{ label: 'Quality Review', value: 'Emergency access appropriateness reviewed' }
	];

	// Scenario: Audit trail for patient who requested access log of their own record
	const accessSummary: Row[] = [
		{ label: 'Healthcare Providers', value: 'Names and roles of providers who accessed record' },
		{ label: 'Treatment Dates', value: 'Dates when records were accessed for care' },
		{ label: 'Purpose Categories', value: 'Treatment, payment, healthcare operations' },
		{ label: 'Administrative Access', value: 'Billing, quality assurance, regulatory compliance' }
	];

	const informationIncluded: Row[] = [
		{ label: 'Provider Names', value: 'Dr. Sarah Kim (Emergency Medicine)' },
		{ label: 'Access Dates', value: 'June 20, 2025 for emergency treatment' },
		{ label: 'General Purpose', value: 'Direct patient care and treatment' },
		{ label: 'Department', value: 'Emergency Department' }
	];

	const informationExcluded: Row[] = [
		{ label: 'IP Addresses', value: 'Technical data not relevant to patient' },
		{ label: 'Workstation IDs', value: 'Internal system identifiers' },
		{ label: 'Session Details', value: 'Technical access information' },
		{ label: 'Investigation Data', value: 'Law enforcement sensitive information' }
	];

	const patientRights: Row[] = [
		{ label: 'Right to Restrict', value: 'How to request access restrictions' },
		{ label: 'Right to Complain', value: 'How to file privacy complaints' },
		{ label: 'Contact Information', value: 'Privacy officer contact details' }
	];

	// Scenario: Monthly compliance audit report for department oversight
	const accessStatistics: Row[] = [
		{ label: 'Total Record Access', value: '15,847 patient record accesses' },
		{ label: 'Unique Users', value: '156 healthcare providers' },
		{ label: 'Average Session', value: '12 minutes 34 seconds' },
		{ label: 'After-hours Access', value: '892 accesses (5.6% of total)' },
		{ label: 'Emergency Override', value: '12 break-the-glass accesses' }
	];

	const complianceIndicators: Row[] = [
		{ label: 'Appropriate Access', value: '99.2% of accesses had documented treatment relationship' },
		{ label: 'Minimum Necessary Compliance', value: '98.7% accessed only required data elements' },
		{ label: 'Timely Documentation', value: '99.8% of access properly documented within 24 hours' },
		{ label: 'Unauthorized Access', value: '0.3% flagged for investigation (47 instances)' }
	];

	const trendsAndPatterns: Row[] = [
		{ label: 'Access Volume', value: '15% increase from May (normal seasonal pattern)' },
		{ label: 'User Compliance', value: '2 users require additional HIPAA training' },
		{ label: 'System Performance', value: 'No audit logging failures detected' },
		{ label: 'Policy Adherence', value: '99.1% compliance with access policies' }
	];

	const improvementRecommendations: Row[] = [
		{ label: 'User Training', value: 'Schedule refresher training for 2 staff members' },
		{ label: 'Policy Updates', value: 'Review after-hours access procedures' },
		{ label: 'System Enhancement', value: 'Consider additional automated monitoring' },
		{ label: 'Process Improvement', value: 'Streamline emergency access documentation' }
	];

	// Scenario: Investigate potential data breach with forensic audit trail
	const forensicElements: Row[] = [
		{ label: 'User Activity', value: 'Detailed timeline of all user actions' },
		{ label: 'Data Accessed', value: 'Specific patient information viewed/modified' },
		{ label: 'System Interactions', value: 'Every click, search, and data retrieval' },
		{ label: 'Network Activity', value: 'IP addresses, network connections, file transfers' },
		{ label: 'Concurrent Sessions', value: 'Multiple simultaneous access attempts' }
	];

	const securityIndicators: Row[] = [
		{ label: 'Unusual Patterns', value: '15 users accessed >100 records in one day' },
		{ label: 'Off-site Access', value: '23 connections from non-hospital IP addresses' },
		{ label: 'Data Export Activity', value: '5 instances of bulk data downloads' },
		{ label: 'Failed Login Attempts', value: '247 failed logins from external IPs' }
	];

	const evidencePreservation: Row[] = [
		{ label: 'Audit Logs', value: 'Tamper-proof digital signature applied' },
		{ label: 'System Snapshots', value: 'Full system state captured and archived' },
		{ label: 'User Account Data', value: 'Complete account history preserved' },
		{ label: 'Network Logs', value: 'Network traffic logs secured for analysis' }
	];

	const legalCompliance: Row[] = [
		{ label: 'Chain of Custody', value: 'Documented evidence handling procedures' },
		{ label: 'Data Integrity', value: 'Cryptographic verification of audit data' },
		{ label: 'Discovery Response', value: 'Legal hold procedures activated' },
		{ label: 'Regulatory Reporting', value: 'Breach notification procedures initiated' }
	];

	type ReportView = 'none' | 'standard' | 'suspicious' | 'emergency' | 'patient' | 'monthly' | 'forensic';

	let patientSearch = $state('');
	let startDate = $state('');
	let endDate = $state('');
	let reportingPeriod = $state('');
	let reportView = $state<ReportView>('none');

	function generateAuditReport() {
		if (patientSearch.includes('Robert Thompson') || patientSearch.includes('MRN-456789')) {
			reportView = 'suspicious';
		} else {
			reportView = 'standard';
		}
	}

	function reviewEmergencyAccess() {
		reportView = 'emergency';
	}

	function generatePatientAccessReport() {
		reportView = 'patient';
	}

	function runMonthlyAuditAnalysis() {
		reportView = 'monthly';
	}

	function conductForensicAudit() {
		reportView = 'forensic';
	}
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

<div class="card" data-testid="audit-trail-panel">
	<h1 class="panel-heading">Audit Trail</h1>
	<p class="panel-subtitle">
		Review comprehensive access logs for patient records to ensure HIPAA compliance and
		investigate unauthorized access.
	</p>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Recent Patient Record Access</h2>
		<table>
			<thead>
				<tr>
					<th>User</th>
					<th>Role</th>
					<th>Department</th>
					<th>Purpose</th>
				</tr>
			</thead>
			<tbody>
				{#each accessLogUsers as user (user.access)}
					<tr data-testid="access-log-summary-row">
						<td>{user.name}</td>
						<td>{user.role}</td>
						<td>{user.department}</td>
						<td>{user.purpose}</td>
					</tr>
				{/each}
			</tbody>
		</table>
	</div>

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Search Access Logs</h2>
		<div class="field-row">
			<div class="field">
				<label for="patient-search">Patient Search</label>
				<input
					id="patient-search"
					data-testid="patient-search"
					type="text"
					placeholder="e.g. Jennifer Rodriguez (MRN-789456)"
					bind:value={patientSearch}
				/>
			</div>
			<div class="field">
				<label for="start-date">Start Date</label>
				<input
					id="start-date"
					data-testid="start-date"
					type="text"
					placeholder="YYYY-MM-DD"
					bind:value={startDate}
				/>
			</div>
			<div class="field">
				<label for="end-date">End Date</label>
				<input
					id="end-date"
					data-testid="end-date"
					type="text"
					placeholder="YYYY-MM-DD"
					bind:value={endDate}
				/>
			</div>
			<div class="field">
				<label for="reporting-period">Reporting Period</label>
				<input
					id="reporting-period"
					data-testid="reporting-period"
					type="text"
					placeholder="e.g. 12 months"
					bind:value={reportingPeriod}
				/>
			</div>
		</div>

		<Button type="button" class="btn" data-testid="generate-audit-report-button" onclick={generateAuditReport}>
			Generate audit report
		</Button>
		<Button
			type="button"
			class="btn secondary"
			data-testid="generate-patient-access-report-button"
			onclick={generatePatientAccessReport}
		>
			Generate patient access report
		</Button>
		<Button
			type="button"
			class="btn secondary"
			data-testid="review-emergency-access-button"
			onclick={reviewEmergencyAccess}
		>
			Review emergency access
		</Button>
		<Button
			type="button"
			class="btn secondary"
			data-testid="run-monthly-audit-analysis-button"
			onclick={runMonthlyAuditAnalysis}
		>
			Run monthly audit analysis
		</Button>
		<Button
			type="button"
			class="btn secondary"
			data-testid="conduct-forensic-audit-button"
			onclick={conductForensicAudit}
		>
			Conduct forensic audit
		</Button>
	</div>

	{#if reportView === 'standard'}
		<div class="card" data-type="info" role="status" data-testid="audit-report-standard">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Comprehensive Access Log</h2>
			<table>
				<thead>
					<tr>
						<th>Access #</th>
						<th>User Name</th>
						<th>User Role</th>
						<th>Department</th>
						<th>Access Purpose</th>
					</tr>
				</thead>
				<tbody>
					{#each accessLogUsers as user (user.access)}
						<tr data-testid="audit-log-entry">
							<td>{user.access}</td>
							<td>{user.name}</td>
							<td>{user.role}</td>
							<td>{user.department}</td>
							<td>{user.purpose}</td>
						</tr>
					{/each}
				</tbody>
			</table>

			<h3>Access Timestamps</h3>
			<table>
				<thead>
					<tr>
						<th>User Name</th>
						<th>Login Time</th>
						<th>Logout Time</th>
						<th>Session Duration</th>
					</tr>
				</thead>
				<tbody>
					{#each accessLogUsers as user (user.access)}
						<tr data-testid="audit-log-timestamp-entry">
							<td>{user.name}</td>
							<td>{user.loginTime}</td>
							<td>{user.logoutTime}</td>
							<td>{user.duration}</td>
						</tr>
					{/each}
				</tbody>
			</table>

			<h3>Data Elements Accessed</h3>
			<table>
				<thead>
					<tr>
						<th>User Name</th>
						<th>Data Elements Accessed</th>
						<th>Actions Performed</th>
					</tr>
				</thead>
				<tbody>
					{#each accessLogUsers as user (user.access)}
						<tr data-testid="audit-log-data-element-entry">
							<td>{user.name}</td>
							<td>{user.dataElements}</td>
							<td>{user.actions}</td>
						</tr>
					{/each}
				</tbody>
			</table>
		</div>
	{:else if reportView === 'suspicious'}
		<div class="card" data-type="warning" role="alert" data-testid="audit-report-suspicious">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Suspicious Access Investigation</h2>
			{@render detailTable(suspiciousActivity)}
			<h3>Forensic Information</h3>
			{@render detailTable(forensicData)}
			<h3>Compliance Violation Indicators</h3>
			{@render detailTable(violationIndicators)}
			<h3>Automatic Security Alerts</h3>
			{@render detailTable(securityAlerts)}
		</div>
	{:else if reportView === 'emergency'}
		<div class="card" data-type="info" role="status" data-testid="audit-report-emergency">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Break-the-Glass Emergency Access</h2>
			{@render detailTable(breakTheGlassDocumentation)}
			<h3>Emergency Access Activities</h3>
			{@render detailTable(emergencyAccessActivities)}
			<h3>Post-Emergency Review Requirements</h3>
			{@render detailTable(reviewRequirements)}
		</div>
	{:else if reportView === 'patient'}
		<div class="card" data-type="info" role="status" data-testid="audit-report-patient">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Patient-Facing Access Report</h2>
			{@render detailTable(accessSummary)}
			<h3>Information Included</h3>
			{@render detailTable(informationIncluded)}
			<h3>Information Excluded</h3>
			{@render detailTable(informationExcluded)}
			<h3>Patient Rights</h3>
			{@render detailTable(patientRights)}
		</div>
	{:else if reportView === 'monthly'}
		<div class="card" data-type="info" role="status" data-testid="audit-report-monthly">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Monthly Compliance Report</h2>
			{@render detailTable(accessStatistics)}
			<h3>Compliance Indicators</h3>
			{@render detailTable(complianceIndicators)}
			<h3>Trends and Patterns</h3>
			{@render detailTable(trendsAndPatterns)}
			<h3>Recommendations for Improvement</h3>
			{@render detailTable(improvementRecommendations)}
		</div>
	{:else if reportView === 'forensic'}
		<div class="card" data-type="warning" role="alert" data-testid="audit-report-forensic">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Forensic Audit Investigation</h2>
			{@render detailTable(forensicElements)}
			<h3>Security Indicators</h3>
			{@render detailTable(securityIndicators)}
			<h3>Evidence Preservation</h3>
			{@render detailTable(evidencePreservation)}
			<h3>Legal Compliance</h3>
			{@render detailTable(legalCompliance)}
		</div>
	{/if}
</div>
