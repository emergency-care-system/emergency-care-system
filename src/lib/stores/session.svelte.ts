// Session store: who is currently "logged in" to this demo app.
//
// This is a fictitious, client-only session for demo purposes — there is no
// real backend or authentication service. Any non-empty identity typed on
// the login page is accepted (see src/lib/data/directory.ts for the small
// set of usernames with mock password/lockout behavior).

const STORAGE_KEY = 'ed-demo-session';

export type Session = {
	identity: string;
	role: string;
};

export const sessionState = $state<{ current: Session | null }>({ current: null });

const ROLE_PATTERNS: Array<[RegExp, string]> = [
	[/\bdr\.?\b|physician/i, 'Physician'],
	[/charge nurse/i, 'Charge Nurse'],
	[/nurse/i, 'Nurse'],
	[/registration/i, 'Registration Staff'],
	[/manager/i, 'ED Manager'],
	[/compliance officer/i, 'Compliance Officer'],
	[/officer/i, 'Officer'],
	[/coordinator/i, 'Coordinator'],
	[/technician/i, 'Technician'],
	[/team leader|code blue/i, 'Code Blue Team Leader'],
	[/clerk/i, 'Registration Clerk'],
	[/staff/i, 'Staff'],
	[/tech support/i, 'Technical Support']
];

export function inferRole(identity: string): string {
	for (const [pattern, role] of ROLE_PATTERNS) {
		if (pattern.test(identity)) return role;
	}
	return 'Staff Member';
}

export function login(identity: string, role: string = inferRole(identity)): void {
	sessionState.current = { identity, role };
	if (typeof localStorage !== 'undefined') {
		localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionState.current));
	}
}

export function logout(): void {
	sessionState.current = null;
	if (typeof localStorage !== 'undefined') {
		localStorage.removeItem(STORAGE_KEY);
	}
}

export function restoreSession(): void {
	if (typeof localStorage === 'undefined') return;
	const raw = localStorage.getItem(STORAGE_KEY);
	if (!raw) return;
	try {
		sessionState.current = JSON.parse(raw);
	} catch {
		localStorage.removeItem(STORAGE_KEY);
	}
}
