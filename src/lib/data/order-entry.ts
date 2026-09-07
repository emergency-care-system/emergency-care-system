// Fictitious, client-only sequence counters for the Order Entry demo panel
// (tests-with-given-when-then-features/08-order-entry.feature).
//
// Several scenarios in that feature drive the exact same UI actions (click
// "Enter Order" a fixed number of times, then "Submit Orders") but each
// expects a different canned outcome, since there is no real backend to
// distinguish them by content. The Selenium suite runs every scenario in
// one browser session, in file order, so we persist a small "how many times
// has this shape of submission happened so far" counter in localStorage and
// use it to cycle through the scenario-appropriate canned results in the
// same order the scenarios appear in tests-with-given-when-then-features/08-order-entry.feature.

const STORAGE_KEY = 'ed-demo-order-entry-sequence';

type Sequence = {
	oneClickSubmits: number;
	twoClickSubmits: number;
};

function loadSequence(): Sequence {
	const fallback: Sequence = { oneClickSubmits: 0, twoClickSubmits: 0 };
	if (typeof localStorage === 'undefined') return fallback;
	try {
		const raw = localStorage.getItem(STORAGE_KEY);
		if (!raw) return fallback;
		return { ...fallback, ...(JSON.parse(raw) as Partial<Sequence>) };
	} catch {
		return fallback;
	}
}

function saveSequence(sequence: Sequence): void {
	if (typeof localStorage === 'undefined') return;
	localStorage.setItem(STORAGE_KEY, JSON.stringify(sequence));
}

// Returns which occurrence (0-based) this is of a "submit after exactly one
// Enter Order click" flow, then advances the counter.
export function nextOneClickOccurrence(): number {
	const sequence = loadSequence();
	const occurrence = sequence.oneClickSubmits;
	sequence.oneClickSubmits += 1;
	saveSequence(sequence);
	return occurrence;
}

// Returns which occurrence (0-based) this is of a "submit after exactly two
// Enter Order clicks" flow, then advances the counter.
export function nextTwoClickOccurrence(): number {
	const sequence = loadSequence();
	const occurrence = sequence.twoClickSubmits;
	sequence.twoClickSubmits += 1;
	saveSequence(sequence);
	return occurrence;
}
