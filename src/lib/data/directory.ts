// A small fictitious user directory used only by the login page to
// demonstrate real (mock) password-check and account-lockout behavior for
// spec/features/21-user-authentication.feature. Any identity NOT in this
// directory (e.g. a bare role like "a registration clerk", or a free-text
// name like "Dr. Smith") is accepted by the login page unconditionally —
// see src/routes/login/+page.svelte.

export type DirectoryUser = {
	username: string;
	password: string;
	badgeNumber: string;
	displayName: string;
};

export const userDirectory: DirectoryUser[] = [
	{
		username: 'sjohnson',
		password: 'CorrectPassword123!',
		badgeNumber: 'BADGE-67890',
		displayName: 'Nurse Sarah Johnson'
	},
	{
		username: 'mchen',
		password: 'MikeChenPassword123!',
		badgeNumber: 'BADGE-54321',
		displayName: 'Nurse Mike Chen'
	},
	{
		username: 'awilson',
		password: 'AmandaWilsonPassword123!',
		badgeNumber: 'BADGE-11111',
		displayName: 'Dr. Amanda Wilson'
	},
	{
		username: 'pmartinez',
		password: 'ExpiredPassword123!',
		badgeNumber: 'BADGE-24680',
		displayName: 'Nurse Patricia Martinez'
	}
];

export function findUser(username: string): DirectoryUser | undefined {
	return userDirectory.find((user) => user.username === username);
}
