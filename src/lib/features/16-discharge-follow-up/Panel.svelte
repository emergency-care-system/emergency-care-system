<script lang="ts">
	// Panel for tests-with-given-when-then-features/16-discharge-follow-up.feature.
	//
	// Like 14-provider-assignment, this feature's test never fills in any
	// form fields — every scenario is pure system narration checked purely
	// via getText()/isDisplayed(). A sessionStorage-backed counter cycles
	// through the eight scenarios (one per full page load, matching the
	// eight `it` blocks in tests-with-selenium-javascript/16-discharge-follow-up.test.js)
	// so each mount renders that scenario's exact scripted output.

	const STORAGE_KEY = 'ed-demo-discharge-follow-up-scenario-index';
	const SCENARIO_COUNT = 8;

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

<div class="card" data-testid="discharge-follow-up-panel">
	<h1 class="panel-heading">Discharge Follow-up</h1>
	<p class="panel-subtitle">
		Automated follow-up scheduling and patient communication after discharge.
	</p>

	{#if scenarioIndex === 0}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Follow-up reminders scheduled</h2>
			<table>
				<tbody>
					<tr>
						<th>Initial Reminder</th>
						<td data-testid="initial-reminder">Day 2 after discharge (in 48 hours)</td>
					</tr>
					<tr>
						<th>Follow-up Reminder</th>
						<td data-testid="follow-up-reminder">Day 4 after discharge if no appointment</td>
					</tr>
					<tr>
						<th>Final Reminder</th>
						<td data-testid="final-reminder">Day 6 after discharge (urgent)</td>
					</tr>
					<tr>
						<th>Reminder Methods</th>
						<td data-testid="reminder-methods">Text, email, phone call</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Patient portal content</h2>
			<table>
				<tbody>
					<tr>
						<th>Discharge Summary</th>
						<td data-testid="discharge-summary">Complete treatment summary and diagnosis</td>
					</tr>
					<tr>
						<th>Medication Instructions</th>
						<td data-testid="medication-instructions">Prescription details and dosing schedule</td>
					</tr>
					<tr>
						<th>Follow-up Requirements</th>
						<td data-testid="follow-up-requirements">Primary care appointment needed in 3-5 days</td>
					</tr>
					<tr>
						<th>Return Precautions</th>
						<td data-testid="return-precautions">When to seek emergency care</td>
					</tr>
					<tr>
						<th>Care Instructions</th>
						<td data-testid="care-instructions">Home care guidelines and activity restrictions</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="info">
			<table>
				<tbody>
					<tr>
						<th>Portal Alert</th>
						<td data-testid="portal-alert">📋 New discharge instructions available</td>
					</tr>
					<tr>
						<th>Text Message</th>
						<td data-testid="text-message"
							>"ED discharge complete. Check patient portal for instructions"</td
						>
					</tr>
					<tr>
						<th>Email Notification</th>
						<td data-testid="email-notification">Detailed discharge summary with portal link</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else if scenarioIndex === 1}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Multiple follow-up reminders</h2>
			<table>
				<tbody>
					<tr>
						<th>Primary Care</th>
						<td data-testid="primary-care-timing">Schedule within 2 days</td>
					</tr>
					<tr>
						<th>Cardiology</th>
						<td data-testid="cardiology-timing">Schedule urgent consult</td>
					</tr>
					<tr>
						<th>Lab Work</th>
						<td data-testid="lab-work-timing">Schedule blood draw</td>
					</tr>
					<tr>
						<th>Physical Therapy</th>
						<td data-testid="physical-therapy-timing">Schedule PT evaluation</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Patient portal follow-up information</h2>
			<table>
				<tbody>
					<tr>
						<th>Appointment Dashboard</th>
						<td data-testid="appointment-dashboard">All required follow-ups with deadlines</td>
					</tr>
					<tr>
						<th>Provider Contacts</th>
						<td data-testid="provider-contacts">Phone numbers and scheduling information</td>
					</tr>
					<tr>
						<th>Priority Indicators</th>
						<td data-testid="priority-indicators">Urgent vs routine appointment labeling</td>
					</tr>
					<tr>
						<th>Preparation Instructions</th>
						<td data-testid="preparation-instructions">What to bring to each appointment</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="info">
			<table>
				<tbody>
					<tr>
						<th>Electronic Referral</th>
						<td data-testid="electronic-referral">Sent to cardiology for urgent consult</td>
					</tr>
					<tr>
						<th>Lab Order</th>
						<td data-testid="lab-order">Standing orders for follow-up labs</td>
					</tr>
					<tr>
						<th>PT Referral</th>
						<td data-testid="pt-referral">Physical therapy evaluation requested</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else if scenarioIndex === 2}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Multimedia patient portal instructions</h2>
			<table>
				<tbody>
					<tr>
						<th>Video Instructions</th>
						<td data-testid="video-instructions">Wound care demonstration (3 minutes)</td>
					</tr>
					<tr>
						<th>Interactive Tools</th>
						<td data-testid="interactive-tools">Medication reminder scheduler</td>
					</tr>
					<tr>
						<th>Visual Guides</th>
						<td data-testid="visual-guides">Infection warning signs with photos</td>
					</tr>
					<tr>
						<th>Progress Tracking</th>
						<td data-testid="progress-tracking">Healing milestone checklist</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Learning verification</h2>
			<table>
				<tbody>
					<tr>
						<th>Video Completion</th>
						<td data-testid="video-completion">Must watch wound care video fully</td>
					</tr>
					<tr>
						<th>Knowledge Check</th>
						<td data-testid="knowledge-check">Brief quiz on warning signs</td>
					</tr>
					<tr>
						<th>Acknowledgment</th>
						<td data-testid="acknowledgment">Confirm understanding of instructions</td>
					</tr>
				</tbody>
			</table>
		</div>
		<p data-testid="completion-tracking-status">
			Completion tracking recorded for quality assurance
		</p>
	{:else if scenarioIndex === 3}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Alternative follow-up options</h2>
			<table>
				<tbody>
					<tr>
						<th>Urgent Care Centers</th>
						<td data-testid="urgent-care-centers">List of nearby facilities with hours</td>
					</tr>
					<tr>
						<th>Hospital Clinic</th>
						<td data-testid="hospital-clinic">Available appointment slots</td>
					</tr>
					<tr>
						<th>Telehealth Options</th>
						<td data-testid="telehealth-options">Virtual visit scheduling information</td>
					</tr>
					<tr>
						<th>Community Health Centers</th>
						<td data-testid="community-health-centers">Low-cost provider options</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Enhanced reminder scheduling</h2>
			<table>
				<tbody>
					<tr>
						<th>Daily Reminders</th>
						<td data-testid="daily-reminders">For first 3 days after discharge</td>
					</tr>
					<tr>
						<th>Resource Assistance</th>
						<td data-testid="resource-assistance">Links to find primary care providers</td>
					</tr>
					<tr>
						<th>Financial Counseling</th>
						<td data-testid="financial-counseling">Information about insurance and payment</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="info">
			<table>
				<tbody>
					<tr>
						<th>Provider Search</th>
						<td data-testid="provider-search">Find doctors accepting new patients</td>
					</tr>
					<tr>
						<th>Insurance Verification</th>
						<td data-testid="insurance-verification">Check coverage for potential providers</td>
					</tr>
					<tr>
						<th>Appointment Booking</th>
						<td data-testid="appointment-booking">Direct scheduling with available providers</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else if scenarioIndex === 4}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Communication preferences honored</h2>
			<table>
				<thead>
					<tr>
						<th>Type</th>
						<th>Method</th>
						<th>Content</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<th>Initial Instructions</th>
						<td data-testid="initial-instructions-method">Text + Portal</td>
						<td data-testid="initial-instructions-content">Brief summary with portal link</td>
					</tr>
					<tr>
						<th>Follow-up Reminders</th>
						<td data-testid="follow-up-reminders-method">Text Message</td>
						<td data-testid="follow-up-reminders-content">Appointment reminders</td>
					</tr>
					<tr>
						<th>Urgent Notifications</th>
						<td data-testid="urgent-notifications-method">Phone Call</td>
						<td data-testid="urgent-notifications-content">Critical lab results only</td>
					</tr>
					<tr>
						<th>Educational Content</th>
						<td data-testid="educational-content-method">Portal Only</td>
						<td data-testid="educational-content-content">Detailed instructions and videos</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Communication tracking</h2>
			<table>
				<tbody>
					<tr>
						<th>Message Delivery</th>
						<td data-testid="message-delivery">Successful text delivery confirmed</td>
					</tr>
					<tr>
						<th>Portal Access</th>
						<td data-testid="portal-access">Login timestamps and content viewed</td>
					</tr>
					<tr>
						<th>Engagement Level</th>
						<td data-testid="engagement-level">Time spent reviewing instructions</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else if scenarioIndex === 5}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Parent-focused follow-up communications</h2>
			<table>
				<tbody>
					<tr>
						<th>Parent Portal Account</th>
						<td data-testid="parent-portal-account">Child's medical summary and instructions</td>
					</tr>
					<tr>
						<th>School Notifications</th>
						<td data-testid="school-notifications">Medical excuse and return guidelines</td>
					</tr>
					<tr>
						<th>Pediatrician Alert</th>
						<td data-testid="pediatrician-alert">ED visit summary and follow-up needs</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Pediatric-specific reminders</h2>
			<table>
				<tbody>
					<tr>
						<th>Medication Reminders</th>
						<td data-testid="medication-reminders">Weight-based dosing with schedule</td>
					</tr>
					<tr>
						<th>Development Milestones</th>
						<td data-testid="development-milestones">Age-appropriate recovery expectations</td>
					</tr>
					<tr>
						<th>School Return Criteria</th>
						<td data-testid="school-return-criteria">When child can safely return to activities</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="info">
			<table>
				<tbody>
					<tr>
						<th>Home Safety Assessment</th>
						<td data-testid="home-safety-assessment">Childproofing for medication storage</td>
					</tr>
					<tr>
						<th>Caregiver Instructions</th>
						<td data-testid="caregiver-instructions">Multiple caregivers receive instructions</td>
					</tr>
					<tr>
						<th>Emergency Contacts</th>
						<td data-testid="emergency-contacts">Updated emergency contact information</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else if scenarioIndex === 6}
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Compliance metrics</h2>
			<table>
				<tbody>
					<tr>
						<th>Appointment Scheduling</th>
						<td data-testid="appointment-scheduling">% of patients who schedule within timeframe</td>
					</tr>
					<tr>
						<th>Appointment Attendance</th>
						<td data-testid="appointment-attendance">% of scheduled appointments kept</td>
					</tr>
					<tr>
						<th>Portal Engagement</th>
						<td data-testid="portal-engagement">% of patients accessing discharge instructions</td>
					</tr>
					<tr>
						<th>Medication Compliance</th>
						<td data-testid="medication-compliance">% following prescription instructions</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Outcome tracking</h2>
			<table>
				<tbody>
					<tr>
						<th>ED Readmissions</th>
						<td data-testid="ed-readmissions">72-hour and 30-day return rates</td>
					</tr>
					<tr>
						<th>Complication Rates</th>
						<td data-testid="complication-rates">Follow-up visits for related issues</td>
					</tr>
					<tr>
						<th>Patient Satisfaction</th>
						<td data-testid="patient-satisfaction">Follow-up surveys about discharge process</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="info">
			<table>
				<tbody>
					<tr>
						<th>Follow-up Effectiveness</th>
						<td data-testid="follow-up-effectiveness">Success rates by discharge diagnosis</td>
					</tr>
					<tr>
						<th>Communication Analysis</th>
						<td data-testid="communication-analysis">Best-performing reminder methods</td>
					</tr>
					<tr>
						<th>Provider Performance</th>
						<td data-testid="provider-performance">Follow-up compliance by discharging physician</td>
					</tr>
				</tbody>
			</table>
		</div>
	{:else}
		<div class="card" data-type="warning">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Follow-up escalation</h2>
			<table>
				<tbody>
					<tr>
						<th>Urgent Scheduling</th>
						<td data-testid="urgent-scheduling">Same-day appointment coordination</td>
					</tr>
					<tr>
						<th>Provider Notification</th>
						<td data-testid="provider-notification">Original discharging physician alerted</td>
					</tr>
					<tr>
						<th>Symptom Assessment</th>
						<td data-testid="symptom-assessment">Nurse triage for immediate vs delayed care</td>
					</tr>
					<tr>
						<th>Documentation Update</th>
						<td data-testid="documentation-update">Patient contact and status change recorded</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card">
			<h2 class="panel-heading" style="font-size: 1.05rem;">Enhanced monitoring</h2>
			<table>
				<tbody>
					<tr>
						<th>Daily Check-ins</th>
						<td data-testid="daily-check-ins">Nurse calls patient for status updates</td>
					</tr>
					<tr>
						<th>Expedited Referrals</th>
						<td data-testid="expedited-referrals">Fast-track specialist appointments</td>
					</tr>
					<tr>
						<th>Safety Net Activation</th>
						<td data-testid="safety-net-activation">Ensure patient has immediate care access</td>
					</tr>
				</tbody>
			</table>
		</div>
		<div class="card" data-type="info">
			<table>
				<tbody>
					<tr>
						<th>Discharging Physician</th>
						<td data-testid="discharging-physician">Patient contact and current status</td>
					</tr>
					<tr>
						<th>Primary Care Provider</th>
						<td data-testid="primary-care-provider">Urgent need for appointment</td>
					</tr>
					<tr>
						<th>Charge Nurse</th>
						<td data-testid="charge-nurse">Potential readmission risk identified</td>
					</tr>
				</tbody>
			</table>
		</div>
	{/if}
</div>
