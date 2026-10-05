// One entry per tests-with-given-when-then-features/*.feature file in the source repository --
// kept in sync with that directory by hand. If a feature file is added,
// renamed, or removed there, update this list in the same change.
export interface Feature {
	slug: string;
	title: string;
	asA: string;
	iWant: string;
	soThat: string;
}

export const FEATURES: Feature[] = [
	{
		slug: '01-walk-in-patient-registration',
		title: 'Walk-in Patient Registration',
		asA: 'a registration clerk',
		iWant: 'to register new patients who arrive without prior registration',
		soThat: 'they can be properly identified and queued for triage'
	},
	{
		slug: '02-ambulance-arrival-registration',
		title: 'Ambulance Arrival Registration',
		asA: 'a registration staff member',
		iWant: 'to register patients who arrive by ambulance without identification',
		soThat: 'they can receive immediate medical care while maintaining proper documentation'
	},
	{
		slug: '03-initial-triage-assessment',
		title: 'Initial Triage Assessment',
		asA: 'a triage nurse',
		iWant: 'to perform initial assessments on registered patients',
		soThat: 'they are properly prioritized based on their medical acuity'
	},
	{
		slug: '04-triage-re-assessment',
		title: 'Triage Re-assessment',
		asA: 'a triage nurse',
		iWant: 'to reassess patients who have been waiting in the queue',
		soThat: 'their priority can be adjusted if their condition has changed'
	},
	{
		slug: '05-bed-assignment',
		title: 'Bed Assignment',
		asA: 'a charge nurse',
		iWant: 'to receive bed assignment recommendations based on patient acuity and room type',
		soThat: 'I can optimize patient flow and ensure appropriate care placement'
	},
	{
		slug: '06-bed-status-updates',
		title: 'Bed Status Updates',
		asA: 'a nurse',
		iWant: 'to update bed statuses during patient flow transitions',
		soThat: 'housekeeping is notified and bed availability is accurately tracked'
	},
	{
		slug: '07-physician-assessment',
		title: 'Physician Assessment',
		asA: 'a physician',
		iWant: 'to access comprehensive patient information on my mobile device',
		soThat: 'I can make informed clinical decisions based on current patient data'
	},
	{
		slug: '08-order-entry',
		title: 'Order Entry',
		asA: 'a physician',
		iWant: 'to enter diagnostic and treatment orders electronically',
		soThat: 'they are automatically routed to appropriate departments and nursing workflow'
	},
	{
		slug: '09-lab-result-processing',
		title: 'Lab Result Processing',
		asA: 'a healthcare provider',
		iWant: 'laboratory results to be automatically processed and distributed',
		soThat:
			'critical values are immediately communicated and patient records are updated in real-time'
	},
	{
		slug: '10-critical-lab-alert',
		title: 'Critical Lab Alert',
		asA: 'a healthcare provider',
		iWant: 'immediate alerts when critical lab values are received',
		soThat: 'life-threatening conditions can be identified and treated without delay'
	},
	{
		slug: '11-medication-administration',
		title: 'Medication Administration',
		asA: 'a nurse',
		iWant: 'to safely administer medications using barcode verification',
		soThat:
			'I can ensure the five rights of medication administration and maintain accurate documentation'
	},
	{
		slug: '12-allergy-check',
		title: 'Allergy Check',
		asA: 'a physician',
		iWant: 'the system to automatically check for drug allergies when prescribing medications',
		soThat: 'I can prevent allergic reactions and ensure patient safety'
	},
	{
		slug: '13-dynamic-queue-updates',
		title: 'Dynamic Queue Updates',
		asA: 'a charge nurse',
		iWant: 'the patient queue to automatically adjust when new high-acuity patients arrive',
		soThat: 'critical patients receive immediate priority and wait times remain accurate'
	},
	{
		slug: '14-provider-assignment',
		title: 'Provider Assignment',
		asA: 'a charge nurse',
		iWant: 'the system to automatically assign the next appropriate patient when providers become available',
		soThat: 'patient flow is optimized and wait times are minimized'
	},
	{
		slug: '15-patient-discharge',
		title: 'Patient Discharge',
		asA: 'a physician',
		iWant: 'to efficiently process patient discharges with automated documentation and workflow',
		soThat: 'patients receive proper instructions and departmental processes are streamlined'
	},
	{
		slug: '16-discharge-follow-up',
		title: 'Discharge Follow-up',
		asA: 'a patient care coordinator',
		iWant: 'automated follow-up scheduling and patient communication after discharge',
		soThat: 'patients receive proper continuity of care and adhere to treatment plans'
	},
	{
		slug: '17-real-time-dashboard',
		title: 'Real-time Dashboard',
		asA: 'a charge nurse',
		iWant: 'to monitor real-time department status through a comprehensive dashboard',
		soThat: 'I can make informed decisions about patient flow, staffing, and resource allocation'
	},
	{
		slug: '18-performance-metrics',
		title: 'Performance Metrics',
		asA: 'an ED manager',
		iWant: 'to generate comprehensive monthly performance reports',
		soThat:
			'I can evaluate departmental efficiency, quality of care, and identify improvement opportunities'
	},
	{
		slug: '19-mass-casualty-activation',
		title: 'Mass Casualty Activation',
		asA: 'a charge nurse',
		iWant: 'to rapidly activate emergency protocols during mass casualty incidents',
		soThat:
			'the ED can efficiently manage multiple critically injured patients and maximize survival outcomes'
	},
	{
		slug: '20-code-blue-response',
		title: 'Code Blue Response',
		asA: 'a nurse',
		iWant: 'to rapidly activate emergency response for cardiac arrest patients',
		soThat: 'the code team can respond immediately with proper coordination and documentation'
	},
	{
		slug: '21-user-authentication',
		title: 'User Authentication',
		asA: 'a healthcare provider',
		iWant: 'to securely log into the emergency care system',
		soThat: 'I can access patient information and perform my clinical duties with proper authorization'
	},
	{
		slug: '22-audit-trail',
		title: 'Audit Trail',
		asA: 'a compliance officer',
		iWant: 'to review comprehensive access logs for patient records',
		soThat:
			'I can ensure HIPAA compliance and investigate any unauthorized access to protected health information'
	}
];
