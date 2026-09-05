// Fictitious, client-only sequence counter for the Lab Result Processing
// demo panel (spec/features/09-lab-result-processing.feature).
//
// Every scenario in that feature drives the exact same UI action (click
// "Receive Lab Results") with no distinguishing input, but each expects a
// different canned outcome. The Selenium suite runs every scenario in one
// browser session, in file order, so we persist a small "how many times has
// results been received so far" counter in localStorage and use it to cycle
// through the scenario-appropriate canned results in the same order the
// scenarios appear in spec/features/09-lab-result-processing.feature.

const STORAGE_KEY = 'ed-demo-lab-result-processing-sequence';

function loadCount(): number {
	if (typeof localStorage === 'undefined') return 0;
	try {
		const raw = localStorage.getItem(STORAGE_KEY);
		if (!raw) return 0;
		const parsed = Number(raw);
		return Number.isFinite(parsed) ? parsed : 0;
	} catch {
		return 0;
	}
}

function saveCount(count: number): void {
	if (typeof localStorage === 'undefined') return;
	localStorage.setItem(STORAGE_KEY, String(count));
}

// Returns which occurrence (0-based) this is of "results received", then
// advances the counter.
export function nextLabResultOccurrence(): number {
	const occurrence = loadCount();
	saveCount(occurrence + 1);
	return occurrence;
}
