// Registry of the 22 ED features, one per spec/features/*.feature file.
//
// `slug` (without the leading number) is used for this feature's nav link
// (data-testid="nav-<slug>") and drives the dynamic import path below.
// Components are loaded lazily so the dashboard can list every feature
// before every Panel.svelte has been written.
import type { Component } from 'svelte';

export type FeatureEntry = {
	number: string;
	slug: string;
	title: string;
	loadPanel: () => Promise<{ default: Component }>;
};

export const features: FeatureEntry[] = [
	{
		number: '01',
		slug: 'walk-in-patient-registration',
		title: 'Walk-in Patient Registration',
		loadPanel: () => import('./01-walk-in-patient-registration/Panel.svelte')
	},
	{
		number: '02',
		slug: 'ambulance-arrival-registration',
		title: 'Ambulance Arrival Registration',
		loadPanel: () => import('./02-ambulance-arrival-registration/Panel.svelte')
	},
	{
		number: '03',
		slug: 'initial-triage-assessment',
		title: 'Initial Triage Assessment',
		loadPanel: () => import('./03-initial-triage-assessment/Panel.svelte')
	},
	{
		number: '04',
		slug: 'triage-re-assessment',
		title: 'Triage Re-assessment',
		loadPanel: () => import('./04-triage-re-assessment/Panel.svelte')
	},
	{
		number: '05',
		slug: 'bed-assignment',
		title: 'Bed Assignment',
		loadPanel: () => import('./05-bed-assignment/Panel.svelte')
	},
	{
		number: '06',
		slug: 'bed-status-updates',
		title: 'Bed Status Updates',
		loadPanel: () => import('./06-bed-status-updates/Panel.svelte')
	},
	{
		number: '07',
		slug: 'physician-assessment',
		title: 'Physician Assessment',
		loadPanel: () => import('./07-physician-assessment/Panel.svelte')
	},
	{
		number: '08',
		slug: 'order-entry',
		title: 'Order Entry',
		loadPanel: () => import('./08-order-entry/Panel.svelte')
	},
	{
		number: '09',
		slug: 'lab-result-processing',
		title: 'Lab Result Processing',
		loadPanel: () => import('./09-lab-result-processing/Panel.svelte')
	},
	{
		number: '10',
		slug: 'critical-lab-alert',
		title: 'Critical Lab Alert',
		loadPanel: () => import('./10-critical-lab-alert/Panel.svelte')
	},
	{
		number: '11',
		slug: 'medication-administration',
		title: 'Medication Administration',
		loadPanel: () => import('./11-medication-administration/Panel.svelte')
	},
	{
		number: '12',
		slug: 'allergy-check',
		title: 'Allergy Check',
		loadPanel: () => import('./12-allergy-check/Panel.svelte')
	},
	{
		number: '13',
		slug: 'dynamic-queue-updates',
		title: 'Dynamic Queue Updates',
		loadPanel: () => import('./13-dynamic-queue-updates/Panel.svelte')
	},
	{
		number: '14',
		slug: 'provider-assignment',
		title: 'Provider Assignment',
		loadPanel: () => import('./14-provider-assignment/Panel.svelte')
	},
	{
		number: '15',
		slug: 'patient-discharge',
		title: 'Patient Discharge',
		loadPanel: () => import('./15-patient-discharge/Panel.svelte')
	},
	{
		number: '16',
		slug: 'discharge-follow-up',
		title: 'Discharge Follow-up',
		loadPanel: () => import('./16-discharge-follow-up/Panel.svelte')
	},
	{
		number: '17',
		slug: 'real-time-dashboard',
		title: 'Real-time Dashboard',
		loadPanel: () => import('./17-real-time-dashboard/Panel.svelte')
	},
	{
		number: '18',
		slug: 'performance-metrics',
		title: 'Performance Metrics',
		loadPanel: () => import('./18-performance-metrics/Panel.svelte')
	},
	{
		number: '19',
		slug: 'mass-casualty-activation',
		title: 'Mass Casualty Activation',
		loadPanel: () => import('./19-mass-casualty-activation/Panel.svelte')
	},
	{
		number: '20',
		slug: 'code-blue-response',
		title: 'Code Blue Response',
		loadPanel: () => import('./20-code-blue-response/Panel.svelte')
	},
	{
		number: '21',
		slug: 'user-authentication',
		title: 'User Authentication',
		loadPanel: () => import('./21-user-authentication/Panel.svelte')
	},
	{
		number: '22',
		slug: 'audit-trail',
		title: 'Audit Trail',
		loadPanel: () => import('./22-audit-trail/Panel.svelte')
	}
];
