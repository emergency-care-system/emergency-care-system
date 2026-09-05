<script lang="ts">
	// Panel for spec/features/02-ambulance-arrival-registration.feature.
	// Fictitious, client-only "unknown patient" registration workflow. State is
	// kept in local component state only (no localStorage) since none of this
	// feature's scenarios require data to persist across separate test files.
	import { Button } from 'lily-design-system-svelte-headless';

	type RegistrationForm = {
		registrationType: string;
		estimatedAge: string;
		gender: string;
		chiefComplaint: string;
		vitalSigns: string;
		incidentLocation: string;
		emsUnit: string;
		arrivalTime: string;
		foundName: string;
		partialPhone: string;
		identityStatus: string;
		incidentType: string;
		totalPatients: string;
	};

	function blankRegistrationForm(): RegistrationForm {
		return {
			registrationType: '',
			estimatedAge: '',
			gender: '',
			chiefComplaint: '',
			vitalSigns: '',
			incidentLocation: '',
			emsUnit: '',
			arrivalTime: '',
			foundName: '',
			partialPhone: '',
			identityStatus: '',
			incidentType: '',
			totalPatients: ''
		};
	}

	type VerificationForm = {
		fullName: string;
		dateOfBirth: string;
		phoneNumber: string;
		socialSecurity: string;
	};

	function blankVerificationForm(): VerificationForm {
		return { fullName: '', dateOfBirth: '', phoneNumber: '', socialSecurity: '' };
	}

	type TempRecord = { id: string; placeholderId: string };

	type SingleOutcome = {
		kind: 'single';
		records: TempRecord[];
		identityStatusValue: string;
	};

	type MassOutcome = {
		kind: 'mass';
		records: TempRecord[];
		incidentNumber: string;
	};

	let form = $state(blankRegistrationForm());
	let verificationForm = $state(blankVerificationForm());
	let errors = $state<{ estimatedAgeRange?: string; gender?: string; chiefComplaint?: string }>(
		{}
	);
	let warningMessage = $state('');
	let outcome = $state<SingleOutcome | MassOutcome | null>(null);
	let verificationResult = $state<{
		medicalRecordNumber: string;
		patientName: string;
	} | null>(null);

	let unkCounter = 1;
	let mrnCounter = 500001;

	function pad3(n: number): string {
		return String(n).padStart(3, '0');
	}

	function nextPlaceholderId(): TempRecord {
		const placeholderId = `UNK-${pad3(unkCounter)}`;
		const record = { id: placeholderId, placeholderId };
		unkCounter += 1;
		return record;
	}

	function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		errors = {};
		warningMessage = '';

		const isMassCasualty = form.registrationType.trim().toLowerCase().includes('mass casualty');

		if (isMassCasualty) {
			const count = Math.max(1, parseInt(form.totalPatients, 10) || 1);
			const records: TempRecord[] = [];
			for (let i = 0; i < count; i++) {
				records.push(nextPlaceholderId());
			}
			outcome = {
				kind: 'mass',
				records,
				incidentNumber: `INC-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${pad3(1)}`
			};
			return;
		}

		if (!form.chiefComplaint.trim() || !form.estimatedAge.trim()) {
			warningMessage = 'Insufficient information for registration';
			errors = {
				estimatedAgeRange: 'Must be provided',
				gender: 'Must be Male, Female, or Unknown',
				chiefComplaint: 'Must be provided'
			};
			return;
		}

		outcome = {
			kind: 'single',
			records: [nextPlaceholderId()],
			identityStatusValue: form.identityStatus.trim()
		};
	}

	function initiateIdentityVerification() {
		verificationResult = null;
	}

	function verifyIdentification() {
		const medicalRecordNumber = `MRN-${mrnCounter}`;
		mrnCounter += 1;
		verificationResult = {
			medicalRecordNumber,
			patientName: verificationForm.fullName.trim() || 'Unknown Patient'
		};
	}
</script>

<div class="card" data-testid="ambulance-arrival-registration-panel">
	<h1 class="panel-heading">Ambulance Arrival Registration</h1>
	<p class="panel-subtitle">
		Register a patient who arrived by ambulance without identification.
	</p>

	{#if !outcome}
		<form onsubmit={handleSubmit}>
			<div class="field-row">
				<div class="field">
					<label for="registration-type">Registration Type</label>
					<input
						id="registration-type"
						data-testid="registration-type"
						type="text"
						placeholder="Unknown Patient"
						bind:value={form.registrationType}
					/>
				</div>
				<div class="field">
					<label for="estimated-age">Estimated Age</label>
					<input
						id="estimated-age"
						data-testid="estimated-age"
						type="text"
						bind:value={form.estimatedAge}
					/>
				</div>
				<div class="field">
					<label for="gender">Gender</label>
					<input id="gender" data-testid="gender" type="text" bind:value={form.gender} />
				</div>
				<div class="field">
					<label for="chief-complaint">Chief Complaint</label>
					<input
						id="chief-complaint"
						data-testid="chief-complaint"
						type="text"
						bind:value={form.chiefComplaint}
					/>
				</div>
				<div class="field">
					<label for="vital-signs">Vital Signs</label>
					<input
						id="vital-signs"
						data-testid="vital-signs"
						type="text"
						bind:value={form.vitalSigns}
					/>
				</div>
				<div class="field">
					<label for="incident-location">Incident Location</label>
					<input
						id="incident-location"
						data-testid="incident-location"
						type="text"
						bind:value={form.incidentLocation}
					/>
				</div>
				<div class="field">
					<label for="ems-unit">EMS Unit</label>
					<input id="ems-unit" data-testid="ems-unit" type="text" bind:value={form.emsUnit} />
				</div>
				<div class="field">
					<label for="arrival-time">Arrival Time</label>
					<input
						id="arrival-time"
						data-testid="arrival-time"
						type="text"
						bind:value={form.arrivalTime}
					/>
				</div>
				<div class="field">
					<label for="found-name">Found Name</label>
					<input
						id="found-name"
						data-testid="found-name"
						type="text"
						bind:value={form.foundName}
					/>
				</div>
				<div class="field">
					<label for="partial-phone">Partial Phone</label>
					<input
						id="partial-phone"
						data-testid="partial-phone"
						type="text"
						bind:value={form.partialPhone}
					/>
				</div>
				<div class="field">
					<label for="identity-status">Identity Status</label>
					<input
						id="identity-status"
						data-testid="identity-status"
						type="text"
						placeholder="Unverified"
						bind:value={form.identityStatus}
					/>
				</div>
				<div class="field">
					<label for="incident-type">Incident Type</label>
					<input
						id="incident-type"
						data-testid="incident-type"
						type="text"
						bind:value={form.incidentType}
					/>
				</div>
				<div class="field">
					<label for="total-patients">Total Patients</label>
					<input
						id="total-patients"
						data-testid="total-patients"
						type="text"
						bind:value={form.totalPatients}
					/>
				</div>
			</div>

			{#if warningMessage}
				<div class="card" data-type="warning" role="alert" data-testid="warning-message">
					{warningMessage}
				</div>
				<p role="alert" data-testid="estimated-age-range-error">{errors.estimatedAgeRange}</p>
				<p role="alert" data-testid="gender-error">{errors.gender}</p>
				<p role="alert" data-testid="chief-complaint-error">{errors.chiefComplaint}</p>
			{/if}

			<Button type="submit" class="btn" data-testid="submit-registration-form">
				Submit registration
			</Button>
		</form>
	{/if}

	{#if outcome?.kind === 'single'}
		<div class="card" data-type="success" role="status" data-testid="temporary-patient-record">
			<p>
				Placeholder ID: <strong data-testid="placeholder-id"
					>{outcome.records[0].placeholderId}</strong
				>
			</p>
			<p data-testid="identity-verification-flag">Identity Verification Required</p>
			<p data-testid="triage-queue-status">Patient queued for triage</p>
			<p data-testid="charge-nurse-notification">
				Charge nurse notified about unknown patient arrival
			</p>
			<p data-testid="record-status">Temporary - Pending Identification</p>
			{#if outcome.identityStatusValue}
				<p data-testid="identity-status">{outcome.identityStatusValue}</p>
			{/if}
			<p data-testid="social-services-task">
				Social services task created to assist with identification
			</p>
		</div>
	{:else if outcome?.kind === 'mass'}
		{#each outcome.records as record (record.id)}
			<div class="card" data-type="success" role="status" data-testid="temporary-patient-record">
				<p>
					Placeholder ID: <strong data-testid="placeholder-id">{record.placeholderId}</strong>
				</p>
			</div>
		{/each}
		<p data-testid="incident-number">{outcome.incidentNumber}</p>
		<p data-testid="mass-casualty-protocol-status">Activated</p>
		<p data-testid="notification-recipients">
			Notifications sent to administration and social services
		</p>
	{/if}

	<div class="card">
		<h2 class="panel-heading" style="font-size: 1.05rem;">Identity verification</h2>
		<p class="panel-subtitle">
			Convert a temporary unknown-patient record once identification becomes available.
		</p>
		<div class="field-row">
			<div class="field">
				<label for="full-name">Full Name</label>
				<input
					id="full-name"
					data-testid="full-name"
					type="text"
					bind:value={verificationForm.fullName}
				/>
			</div>
			<div class="field">
				<label for="date-of-birth">Date of Birth</label>
				<input
					id="date-of-birth"
					data-testid="date-of-birth"
					type="text"
					placeholder="YYYY-MM-DD"
					bind:value={verificationForm.dateOfBirth}
				/>
			</div>
			<div class="field">
				<label for="phone-number">Phone Number</label>
				<input
					id="phone-number"
					data-testid="phone-number"
					type="text"
					bind:value={verificationForm.phoneNumber}
				/>
			</div>
			<div class="field">
				<label for="social-security">Social Security</label>
				<input
					id="social-security"
					data-testid="social-security"
					type="text"
					bind:value={verificationForm.socialSecurity}
				/>
			</div>
		</div>
		<Button
			type="button"
			class="btn secondary"
			data-testid="initiate-identity-verification-button"
			onclick={initiateIdentityVerification}
		>
			Initiate identity verification
		</Button>
		<Button
			type="button"
			class="btn"
			data-testid="verify-identification-button"
			onclick={verifyIdentification}
		>
			Verify identification
		</Button>

		{#if verificationResult}
			<div class="card" data-type="success" role="status">
				<p>
					Medical record number: <strong data-testid="medical-record-number"
						>{verificationResult.medicalRecordNumber}</strong
					>
				</p>
				<p data-testid="patient-name">{verificationResult.patientName}</p>
				<p data-testid="identification-note">
					Identification confirmed; demographic details updated and documented.
				</p>
				<p data-testid="record-merge-status">
					Temporary record merged with verified information.
				</p>
				<p data-testid="clinical-documentation-status">
					All clinical documentation preserved under the verified record.
				</p>
				<p data-testid="billing-status">Billing information updated with verified patient details.</p>
			</div>
		{/if}
	</div>
</div>
