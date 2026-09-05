// Fictitious, client-only "patient database" for the demo app. Persisted to
// localStorage so it behaves like a real (if tiny) EHR within one browser:
// patients registered earlier in the session are found by later duplicate
// checks, exactly as a real system would.

const STORAGE_KEY = 'ed-demo-patients';

export type Patient = {
	id: string;
	givenName: string;
	familyName: string;
	dateOfBirth: string;
	phoneNumber: string;
	address: string;
	city: string;
	state: string;
	zipCode: string;
	insuranceType: string;
	policyNumber: string;
	groupNumber: string;
	medicalRecordNumber: string;
	triageStatus: string;
	registeredAt: string;
};

// A small fictitious starting roster, unrelated to any demographic values
// used in spec/features/*.feature scenarios, so it never collides with a
// scenario's own duplicate-detection input.
const SEED_PATIENTS: Patient[] = [
	{
		id: 'seed-1',
		givenName: 'Maria',
		familyName: 'Garcia',
		dateOfBirth: '1978-11-02',
		phoneNumber: '555-201-3344',
		address: '88 Elm St',
		city: 'Springfield',
		state: 'IL',
		zipCode: '62701',
		insuranceType: 'Aetna',
		policyNumber: 'AET998877',
		groupNumber: 'GRP450',
		medicalRecordNumber: 'MRN-100001',
		triageStatus: 'Discharged',
		registeredAt: '2026-08-30T09:12:00.000Z'
	},
	{
		id: 'seed-2',
		givenName: 'Kevin',
		familyName: "O'Brien",
		dateOfBirth: '1962-04-19',
		phoneNumber: '555-778-2210',
		address: '14 River Rd',
		city: 'Springfield',
		state: 'IL',
		zipCode: '62702',
		insuranceType: 'Medicare',
		policyNumber: 'MCR112233',
		groupNumber: 'GRP001',
		medicalRecordNumber: 'MRN-100002',
		triageStatus: 'Admitted',
		registeredAt: '2026-09-01T14:40:00.000Z'
	}
];

let nextSequence = 100003;

export function loadPatients(): Patient[] {
	if (typeof localStorage === 'undefined') return SEED_PATIENTS;
	const raw = localStorage.getItem(STORAGE_KEY);
	if (!raw) {
		localStorage.setItem(STORAGE_KEY, JSON.stringify(SEED_PATIENTS));
		return SEED_PATIENTS;
	}
	try {
		return JSON.parse(raw) as Patient[];
	} catch {
		return SEED_PATIENTS;
	}
}

export function savePatients(patients: Patient[]): void {
	if (typeof localStorage === 'undefined') return;
	localStorage.setItem(STORAGE_KEY, JSON.stringify(patients));
}

export function findDuplicates(
	patients: Patient[],
	givenName: string,
	familyName: string,
	dateOfBirth: string
): Patient[] {
	const normalize = (value: string) => value.trim().toLowerCase();
	return patients.filter(
		(patient) =>
			normalize(patient.givenName) === normalize(givenName) &&
			normalize(patient.familyName) === normalize(familyName) &&
			patient.dateOfBirth === dateOfBirth
	);
}

export function generateMedicalRecordNumber(): string {
	const patients = loadPatients();
	const highest = patients.reduce((max, patient) => {
		const match = /MRN-(\d+)/.exec(patient.medicalRecordNumber);
		return match ? Math.max(max, Number(match[1])) : max;
	}, nextSequence - 1);
	nextSequence = highest + 1;
	return `MRN-${String(nextSequence).padStart(6, '0')}`;
}
