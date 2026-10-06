<script lang="ts">
	// Panel for tests-with-given-when-then-features/08-order-entry.feature.
	//
	// This is a fictitious, client-only order entry module. Several scenarios
	// in the feature file click "Enter Order" then "Submit Orders" with no
	// distinguishing input, so the exact number of prior "Enter Order" clicks,
	// plus a persisted (localStorage) occurrence counter from $lib/data/order-entry,
	// picks which canned scenario outcome to render — see that module for why.
	import { Button } from 'lily-design-system-svelte-headless';
	import { nextOneClickOccurrence, nextTwoClickOccurrence } from '#lib/data/order-entry.js';

	type OutcomeKind =
		| 'chest-pain'
		| 'pediatric'
		| 'allergy'
		| 'insurance'
		| 'system-failure'
		| 'stat'
		| null;

	const ONE_CLICK_SEQUENCE: OutcomeKind[] = ['allergy', 'insurance', 'system-failure'];
	const TWO_CLICK_SEQUENCE: OutcomeKind[] = ['chest-pain', 'pediatric'];

	let moduleOpened = $state(false);
	let orderMarking = $state('');
	let orderSet = $state('');
	let enteredCount = $state(0);
	let timingRequirementsAdded = $state(false);
	let outcomeKind = $state<OutcomeKind>(null);

	// Order modification scenario (existing orders / cancel / modify / add)
	type ExistingOrder = {
		id: string;
		type: string;
		description: string;
		status: string;
		enteredTime: string;
	};

	const existingOrders: ExistingOrder[] = [
		{ id: 'ORD-101', type: 'Laboratory', description: 'CBC', status: 'In Progress', enteredTime: '10:30' },
		{ id: 'ORD-102', type: 'Radiology', description: 'Chest X-ray', status: 'Pending', enteredTime: '10:30' },
		{ id: 'ORD-103', type: 'Medication', description: 'Morphine 2mg', status: 'Pending', enteredTime: '10:30' }
	];

	let orderIdInput = $state('');
	let cancellationReason = $state('');
	let modification = $state('');
	let newOrderDescription = $state('');
	let orderChanges = $state<{ id: string; text: string }[]>([]);
	let departmentNotifications = $state<{ id: string; text: string }[]>([]);

	function openOrderEntryModule() {
		moduleOpened = true;
	}

	function enterOrder() {
		enteredCount += 1;
	}

	function submitOrders() {
		if (orderMarking.trim() === 'Emergency - Life threatening') {
			outcomeKind = 'stat';
			return;
		}
		if (enteredCount === 2) {
			const occurrence = nextTwoClickOccurrence();
			outcomeKind = TWO_CLICK_SEQUENCE[occurrence % TWO_CLICK_SEQUENCE.length];
			return;
		}
		const occurrence = nextOneClickOccurrence();
		outcomeKind = ONE_CLICK_SEQUENCE[occurrence % ONE_CLICK_SEQUENCE.length];
	}

	function cancelOrder() {
		orderChanges = [
			...orderChanges,
			{
				id: `change-${orderChanges.length}`,
				text: `Cancelled — ${orderIdInput || 'ORD-103'} — Cancelled — ${cancellationReason || 'Morphine allergy discovered'}`
			}
		];
		departmentNotifications = [
			...departmentNotifications,
			{ id: `notif-${departmentNotifications.length}`, text: 'Pharmacy: Morphine order cancelled - allergy' }
		];
	}

	function modifyOrder() {
		orderChanges = [
			...orderChanges,
			{
				id: `change-${orderChanges.length}`,
				text: `Modified — ${orderIdInput || 'ORD-102'} — Updated — Changed to portable CXR (${modification || 'Add portable'})`
			}
		];
		departmentNotifications = [
			...departmentNotifications,
			{ id: `notif-${departmentNotifications.length}`, text: 'Radiology: CXR modified to portable study' }
		];
	}

	function addOrder() {
		orderChanges = [
			...orderChanges,
			{
				id: `change-${orderChanges.length}`,
				text: `New Order — ORD-104 — Pending — ${newOrderDescription || 'Fentanyl 50mcg IV push'}`
			}
		];
		departmentNotifications = [
			...departmentNotifications,
			{ id: `notif-${departmentNotifications.length}`, text: 'Nursing: New pain medication order available' }
		];
	}

	function addTimingRequirement() {
		timingRequirementsAdded = true;
	}

	const strokeProtocolOrders = [
		{ category: 'Laboratory', description: 'CBC, BMP, PT/INR, PTT', priority: 'STAT', defaultState: 'Selected' },
		{ category: 'Laboratory', description: 'Troponin, Lipid panel', priority: 'STAT', defaultState: 'Selected' },
		{ category: 'Radiology', description: 'CT Head without contrast', priority: 'STAT', defaultState: 'Selected' },
		{ category: 'Radiology', description: 'CT Angiogram head/neck', priority: 'STAT', defaultState: 'Optional' },
		{ category: 'Medication', description: 'Aspirin 325mg', priority: 'STAT', defaultState: 'Selected' },
		{ category: 'Consults', description: 'Neurology consult', priority: 'STAT', defaultState: 'Selected' }
	];
</script>

<div class="card" data-testid="order-entry-panel">
	<h1 class="panel-heading">Order Entry</h1>
	<p class="panel-subtitle">
		Enter diagnostic and treatment orders for electronic routing to the lab, radiology, and
		pharmacy, and to the nursing workflow.
	</p>

	<h2 class="panel-heading" style="font-size: 1.05rem;">Existing orders</h2>
	<table>
		<thead>
			<tr>
				<th>Order ID</th>
				<th>Type</th>
				<th>Description</th>
				<th>Status</th>
				<th>Entered</th>
			</tr>
		</thead>
		<tbody>
			{#each existingOrders as order (order.id)}
				<tr>
					<td>{order.id}</td>
					<td>{order.type}</td>
					<td>{order.description}</td>
					<td>{order.status}</td>
					<td>{order.enteredTime}</td>
				</tr>
			{/each}
		</tbody>
	</table>

	<h2 class="panel-heading" style="font-size: 1.05rem;">New order entry</h2>
	<div class="field-row">
		<div class="field">
			<label for="order-marking">Order Marking</label>
			<input id="order-marking" data-testid="order-marking" type="text" bind:value={orderMarking} />
		</div>
		<div class="field">
			<label for="order-set">Order Set</label>
			<input
				id="order-set"
				data-testid="order-set"
				type="text"
				placeholder="e.g. Acute Stroke Protocol"
				bind:value={orderSet}
			/>
		</div>
	</div>

	<div class="field-row">
		<Button type="button" class="btn secondary" data-testid="open-order-entry-module-button" onclick={openOrderEntryModule}>
			Open order entry module
		</Button>
		<Button type="button" class="btn secondary" data-testid="enter-order-button" onclick={enterOrder}>
			Enter order
		</Button>
		<Button type="button" class="btn" data-testid="submit-orders-button" onclick={submitOrders}>
			Submit orders
		</Button>
	</div>

	{#if moduleOpened}
		<p role="status" data-type="info">Order entry module opened. {enteredCount} order(s) entered.</p>
	{/if}

	{#if orderSet.trim() === 'Acute Stroke Protocol'}
		<div class="card" data-type="info" role="status">
			<h3>Acute Stroke Protocol order set</h3>
			<ul>
				{#each strokeProtocolOrders as order, index (index)}
					<li data-testid="stroke-protocol-order">
						{order.category} — {order.description} — {order.priority} — {order.defaultState}
						<Button type="button" class="btn secondary" data-testid="modify-order" onclick={() => {}}>
							Modify Order
						</Button>
						<Button type="button" class="btn secondary" data-testid="remove-order" onclick={() => {}}>
							Remove Order
						</Button>
					</li>
				{/each}
			</ul>
			<Button type="button" class="btn" data-testid="add-timing-requirement-button" onclick={addTimingRequirement}>
				Add stroke-specific timing requirements
			</Button>
			{#if timingRequirementsAdded}
				<p data-testid="protocol-compliance-tracking-status">
					The system tracks compliance with stroke protocol timing requirements.
				</p>
				<ul>
					<li data-testid="automatic-reminder">CT Head — Within 25 minutes of arrival</li>
					<li data-testid="automatic-reminder">Lab results — Within 45 minutes of arrival</li>
					<li data-testid="automatic-reminder">Neurology — Consult within 15 minutes</li>
				</ul>
			{/if}
		</div>
	{/if}

	{#if outcomeKind === 'chest-pain'}
		<div class="card" data-type="success" role="status">
			<h3>Orders transmitted</h3>
			<ul>
				<li data-testid="laboratory-order-sent">LAB-001 — CBC with diff — John Martinez, ED-5 — Routine — Current</li>
				<li data-testid="laboratory-order-sent">LAB-002 — Troponin I — John Martinez, ED-5 — STAT — Current</li>
				<li data-testid="laboratory-order-sent">LAB-003 — Basic Metabolic — John Martinez, ED-5 — Routine — Current</li>
				<li data-testid="radiology-order-sent">RAD-001 — Chest X-ray PA/LAT — John Martinez, ED-5 — STAT — Current</li>
			</ul>
			<h3>Specimen labels</h3>
			<ul>
				<li data-testid="specimen-label">Blood Draw — John Martinez, DOB: 1975-08-15, ED-5</li>
				<li data-testid="specimen-label">Test Codes — CBC, Troponin, BMP</li>
				<li data-testid="specimen-label">Collection Time — STAT - Collect immediately</li>
				<li data-testid="specimen-label">Barcode — Patient and order identifiers</li>
			</ul>
			<h3>Nursing workflow</h3>
			<ul>
				<li data-testid="nursing-task">Blood Collection — Draw CBC, Troponin, BMP — STAT — Immediate</li>
				<li data-testid="nursing-task">Patient Transport — Transport to X-ray — STAT — After labs</li>
				<li data-testid="nursing-task">Monitor Results — Watch for critical values — High — Ongoing</li>
			</ul>
		</div>
	{:else if outcomeKind === 'pediatric'}
		<div class="card" data-type="success" role="status">
			<p data-testid="dosing-validation-status">Weight-based dosing calculations validated.</p>
			<h3>Pediatric-specific protocols</h3>
			<ul>
				<li data-testid="pediatric-protocol">Collection Volume — Minimum blood volume for pediatric labs</li>
				<li data-testid="pediatric-protocol">Dosing Alerts — Maximum safe dose verified</li>
				<li data-testid="pediatric-protocol">Administration — Child-friendly instructions</li>
			</ul>
			<h3>Nursing tasks</h3>
			<ul>
				<li data-testid="pediatric-nursing-task">Medication Admin — Use pediatric dosing chart</li>
				<li data-testid="pediatric-nursing-task">Blood Collection — Minimize collection volume</li>
				<li data-testid="pediatric-nursing-task">Comfort Measures — Parent/caregiver involvement</li>
			</ul>
			<p data-testid="pharmacy-dosing-notification">
				Pharmacy has received weight-verified dosing information for Tommy Chen (18kg).
			</p>
		</div>
	{:else if outcomeKind === 'allergy'}
		<div class="card" data-type="warning" role="alert">
			<p data-testid="drug-allergy">WARNING: Patient allergic to Penicillin</p>
			<p data-testid="severity">Moderate - Rash, hives</p>
			<p data-testid="cross-reaction">Amoxicillin contains penicillin</p>
			<p data-testid="recommendation">Consider alternative antibiotic</p>
			<p data-testid="order-status">Order held pending confirmation</p>
			<ul>
				<li data-testid="allergy-action-option">Override with reason — Document clinical justification</li>
				<li data-testid="allergy-action-option">Cancel order — Remove the problematic medication</li>
				<li data-testid="allergy-action-option">Select alternative — Choose non-penicillin antibiotic</li>
			</ul>
			<p data-testid="allergy-alert-log-entry">
				Allergy alert logged in patient record: Amoxicillin vs. Penicillin allergy.
			</p>
		</div>
	{:else if outcomeKind === 'insurance'}
		<div class="card" data-type="info" role="status">
			<p data-testid="coverage-verification">CT covered with prior authorization</p>
			<p data-testid="authorization-status">Prior auth required</p>
			<p data-testid="alternative-options">Ultrasound covered without pre-auth</p>
			<ul>
				<li data-testid="insurance-option">Submit for auth — Send for insurance approval (delay)</li>
				<li data-testid="insurance-option">Order alternative — Consider ultrasound instead</li>
				<li data-testid="insurance-option">Emergency override — Document medical necessity</li>
			</ul>
			<p data-testid="order-status">Pending Authorization</p>
			<p data-testid="financial-counselor-notification">
				Patient financial counselor notified regarding CT Abdomen authorization.
			</p>
		</div>
	{:else if outcomeKind === 'system-failure'}
		<div class="card" data-type="warning" role="alert">
			<p data-testid="warning-message">Lab system offline - orders will be queued</p>
			<p data-testid="order-status">Queued for transmission</p>
			<p data-testid="nursing-coordination-notification">
				Nursing notified to manually coordinate laboratory orders with lab staff.
			</p>
			<p data-testid="connectivity-restored-notification">
				You will be notified when lab system connectivity is restored.
			</p>
			<p data-testid="queued-order-transmission-status">
				Queued orders will be automatically transmitted once system connectivity is restored.
			</p>
			<p data-testid="manual-backup-procedure-documentation">
				Manual backup procedures documented for critical orders while lab system is offline.
			</p>
		</div>
	{:else if outcomeKind === 'stat'}
		<div class="card" data-type="error" role="alert">
			<p data-testid="order-transmission-status">
				All orders are immediately transmitted with highest priority.
			</p>
			<p data-testid="laboratory-order-marking">CRITICAL - TRAUMA</p>
			<p data-testid="blood-bank-notification">
				Blood bank has been notified to prepare emergency release protocol.
			</p>
			<p data-testid="radiology-alert">Radiology has been alerted for trauma CT protocol.</p>
			<ul>
				<li data-testid="nursing-action-item">Blood Draw — Collect trauma labs — 5 minutes</li>
				<li data-testid="nursing-action-item">IV Access — Large bore IV x2 — Immediate</li>
				<li data-testid="nursing-action-item">Patient Prep — Prepare for CT transport — 10 minutes</li>
			</ul>
			<p data-testid="department-status-update">
				All departments have received automatic status updates.
			</p>
		</div>
	{/if}

	<h2 class="panel-heading" style="font-size: 1.05rem;">Modify or cancel an order</h2>
	<div class="field-row">
		<div class="field">
			<label for="order-id">Order ID</label>
			<input id="order-id" data-testid="order-id" type="text" bind:value={orderIdInput} />
		</div>
		<div class="field">
			<label for="cancellation-reason">Cancellation Reason</label>
			<input id="cancellation-reason" data-testid="cancellation-reason" type="text" bind:value={cancellationReason} />
		</div>
		<div class="field">
			<label for="modification">Modification</label>
			<input id="modification" data-testid="modification" type="text" bind:value={modification} />
		</div>
		<div class="field">
			<label for="new-order-description">New Order Description</label>
			<input
				id="new-order-description"
				data-testid="new-order-description"
				type="text"
				bind:value={newOrderDescription}
			/>
		</div>
	</div>
	<div class="field-row">
		<Button type="button" class="btn secondary" data-testid="cancel-order-button" onclick={cancelOrder}>
			Cancel order
		</Button>
		<Button type="button" class="btn secondary" data-testid="modify-order-button" onclick={modifyOrder}>
			Modify order
		</Button>
		<Button type="button" class="btn secondary" data-testid="add-order-button" onclick={addOrder}>
			Add order
		</Button>
	</div>

	{#if orderChanges.length > 0}
		<ul>
			{#each orderChanges as change (change.id)}
				<li data-testid="order-change-entry">{change.text}</li>
			{/each}
		</ul>
		<ul>
			{#each departmentNotifications as notification (notification.id)}
				<li data-testid="department-notification">{notification.text}</li>
			{/each}
		</ul>
	{/if}

	<div class="card" data-testid="order-change-audit-trail">
		{#if orderChanges.length > 0}
			Audit trail: {orderChanges.length} order change(s) recorded.
		{:else}
			No order changes recorded yet.
		{/if}
	</div>
</div>
