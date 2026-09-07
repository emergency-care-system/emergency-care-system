// A small fictitious user directory for the login page's mock authentication.
//
// Two kinds of accounts live here:
//
// - The three "headline" demo accounts (doctor@example.com,
//   nurse@example.com, administrator@example.com, all password "secret")
//   -- easy to remember, meant for a person trying the app to actually sign
//   in with. They log in normally: src/routes/login/+page.svelte checks
//   them like any other directory user, then redirects to the dashboard.
// - Four narrower accounts (sjohnson, mchen, awilson, pmartinez) that each
//   exist to demonstrate one specific authentication scenario from
//   tests-with-given-when-then-features/21-user-authentication.feature (successful login, wrong
//   password, account lockout, expired password) with the exact scripted
//   detail that feature describes.
//
// Any identity NOT in this directory (e.g. a bare role like "a registration
// clerk", or a free-text name like "Dr. Smith") is accepted by the login
// page unconditionally -- see src/routes/login/+page.svelte.

export type DirectoryUser = {
	username: string;
	password: string;
	badgeNumber?: string;
	displayName: string;
	role: string;
	/** True if this account's password should be treated as expired. */
	passwordExpired?: boolean;
};

export const userDirectory: DirectoryUser[] = [
	{
		username: 'doctor@example.com',
		password: 'secret',
		displayName: 'Dr. Taylor Morgan',
		role: 'Physician'
	},
	{
		username: 'nurse@example.com',
		password: 'secret',
		displayName: 'Nurse Jordan Casey',
		role: 'Nurse'
	},
	{
		username: 'administrator@example.com',
		password: 'secret',
		displayName: 'Alex Rivera',
		role: 'ED Administrator'
	},
	{
		username: 'sjohnson',
		password: 'CorrectPassword123!',
		badgeNumber: 'BADGE-67890',
		displayName: 'Nurse Sarah Johnson',
		role: 'Registered Nurse'
	},
	{
		username: 'mchen',
		password: 'MikeChenPassword123!',
		badgeNumber: 'BADGE-54321',
		displayName: 'Nurse Mike Chen',
		role: 'Registered Nurse'
	},
	{
		username: 'awilson',
		password: 'AmandaWilsonPassword123!',
		badgeNumber: 'BADGE-11111',
		displayName: 'Dr. Amanda Wilson',
		role: 'Physician'
	},
	{
		username: 'pmartinez',
		password: 'ExpiredPassword123!',
		badgeNumber: 'BADGE-24680',
		displayName: 'Nurse Patricia Martinez',
		role: 'Registered Nurse',
		passwordExpired: true
	}
];

export function findUser(username: string): DirectoryUser | undefined {
	return userDirectory.find((user) => user.username === username);
}
