<script lang="ts">
	// Panel for spec/features/01-walk-in-patient-registration.feature.
	// Fictitious, client-only patient registration backed by localStorage
	// (see $lib/data/patients.ts) so duplicate detection works realistically
	// across scenarios within the same browser session.
	import { Button } from 'lily-design-system-svelte-headless';
	import {
		loadPatients,
		savePatients,
		findDuplicates,
		generateMedicalRecordNumber,
		type Patient
	} from '$lib/data/patients';

	type FormState = {
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
	};

	function blankForm(): FormState {
		return {
			givenName: '',
			familyName: '',
			dateOfBirth: '',
			phoneNumber: '',
			address: '',
			city: '',
			state: '',
			zipCode: '',
			insuranceType: '',
			policyNumber: '',
			groupNumber: ''
		};
	}

	let form = $state(blankForm());
	let errors = $state<{ familyName?: string; dateOfBirth?: string }>({});
	let duplicateMatches = $state<Patient[]>([]);
	let confirmation = $state<{ medicalRecordNumber: string; insuranceStatus: string } | null>(
		null
	);

	const DATE_RE = /^\d{4}-\d{2}-\d{2}$/;

	function resetOutcome() {
		errors = {};
		duplicateMatches = [];
		confirmation = null;
	}

	function createPatient(): { medicalRecordNumber: string; insuranceStatus: string } {
		const medicalRecordNumber = generateMedicalRecordNumber();
		const insuranceStatus = form.insuranceType.trim() || 'Self-Pay';
		const patient: Patient = {
			id: `patient-${medicalRecordNumber}`,
			...form,
			insuranceType: insuranceStatus,
			medicalRecordNumber,
			triageStatus: 'Queued for triage',
			registeredAt: new Date().toISOString()
		};
		savePatients([...loadPatients(), patient]);
		return { medicalRecordNumber, insuranceStatus };
	}

	function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		resetOutcome();

		const newErrors: typeof errors = {};
		if (!form.familyName.trim()) newErrors.familyName = 'Family name is required';
		if (form.dateOfBirth && !DATE_RE.test(form.dateOfBirth)) {
			newErrors.dateOfBirth = 'Invalid date format';
		}
		if (Object.keys(newErrors).length > 0) {
			errors = newErrors;
			return;
		}

		const matches = findDuplicates(
			loadPatients(),
			form.givenName,
			form.familyName,
			form.dateOfBirth
		);
		if (matches.length > 0) {
			duplicateMatches = matches;
			return;
		}

		confirmation = createPatient();
		form = blankForm();
	}

	function linkToExistingRecord() {
		duplicateMatches = [];
	}

	function createNewRecordAnyway() {
		confirmation = createPatient();
		duplicateMatches = [];
		form = blankForm();
	}
</script>

<div class="card" data-testid="patient-registration-form">
	<h1 class="panel-heading">Walk-in Patient Registration</h1>
	<p class="panel-subtitle">
		Register a new patient who has arrived at the ED without prior registration.
	</p>

	<form onsubmit={handleSubmit}>
		<div class="field-row">
			<div class="field">
				<label for="given-name">Given Name</label>
				<input id="given-name" data-testid="given-name" type="text" bind:value={form.givenName} />
			</div>
			<div class="field">
				<label for="family-name">Family Name</label>
				<input
					id="family-name"
					data-testid="family-name"
					type="text"
					bind:value={form.familyName}
				/>
				{#if errors.familyName}
					<p role="alert" data-testid="family-name-error">{errors.familyName}</p>
				{/if}
			</div>
			<div class="field">
				<label for="date-of-birth">Date of Birth</label>
				<input
					id="date-of-birth"
					data-testid="date-of-birth"
					type="text"
					placeholder="YYYY-MM-DD"
					bind:value={form.dateOfBirth}
				/>
				{#if errors.dateOfBirth}
					<p role="alert" data-testid="date-of-birth-error">{errors.dateOfBirth}</p>
				{/if}
			</div>
			<div class="field">
				<label for="phone-number">Phone Number</label>
				<input
					id="phone-number"
					data-testid="phone-number"
					type="text"
					bind:value={form.phoneNumber}
				/>
			</div>
			<div class="field">
				<label for="address">Address</label>
				<input id="address" data-testid="address" type="text" bind:value={form.address} />
			</div>
			<div class="field">
				<label for="city">City</label>
				<input id="city" data-testid="city" type="text" bind:value={form.city} />
			</div>
			<div class="field">
				<label for="state">State</label>
				<input id="state" data-testid="state" type="text" bind:value={form.state} />
			</div>
			<div class="field">
				<label for="zip-code">Zip Code</label>
				<input id="zip-code" data-testid="zip-code" type="text" bind:value={form.zipCode} />
			</div>
		</div>

		<h2 class="panel-heading" style="font-size: 1.05rem;">Insurance details</h2>
		<div class="field-row">
			<div class="field">
				<label for="insurance-type">Insurance Type</label>
				<input
					id="insurance-type"
					data-testid="insurance-type"
					type="text"
					placeholder="e.g. Blue Cross, Self-Pay"
					bind:value={form.insuranceType}
				/>
			</div>
			<div class="field">
				<label for="policy-number">Policy Number</label>
				<input
					id="policy-number"
					data-testid="policy-number"
					type="text"
					bind:value={form.policyNumber}
				/>
			</div>
			<div class="field">
				<label for="group-number">Group Number</label>
				<input
					id="group-number"
					data-testid="group-number"
					type="text"
					bind:value={form.groupNumber}
				/>
			</div>
		</div>

		<Button type="submit" class="btn" data-testid="submit-registration-form">
			Submit registration
		</Button>
	</form>

	{#if duplicateMatches.length > 0}
		<div class="card" data-type="warning" role="alert" data-testid="duplicate-patient-warning">
			Potential duplicate patient found
		</div>
		<ul>
			{#each duplicateMatches as match (match.id)}
				<li data-testid="existing-patient-record">
					{match.givenName} {match.familyName} — DOB {match.dateOfBirth} — {match.medicalRecordNumber}
				</li>
			{/each}
		</ul>
		<Button type="button" class="btn secondary" data-testid="link-to-existing-record" onclick={linkToExistingRecord}>
			Link to existing record
		</Button>
		<Button type="button" class="btn" data-testid="create-new-record" onclick={createNewRecordAnyway}>
			Create new record
		</Button>
	{/if}

	{#if confirmation}
		<div class="card" data-type="success" role="status" data-testid="confirmation-message">
			Patient successfully registered
		</div>
		<p>
			Medical record number: <strong data-testid="medical-record-number"
				>{confirmation.medicalRecordNumber}</strong
			>
		</p>
		<p data-testid="triage-queue-status">Queued for triage</p>
		<p>
			Insurance status: <strong data-testid="insurance-status">{confirmation.insuranceStatus}</strong>
		</p>
	{/if}
</div>
