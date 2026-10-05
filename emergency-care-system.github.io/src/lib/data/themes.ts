// Themes available through ThemePicker. Each theme is a standalone
// stylesheet (static/themes/<id>.css) redefining this site's own small
// set of CSS custom properties (see theme.css) -- not a port of Lily's
// full 45-theme, 492-class-hook catalog, which styles a much larger
// component surface than this four-page site uses. See ThemePicker.svelte,
// which lazy-loads exactly one of these per selection.
export interface ThemeOption {
	id: string;
	label: string;
}

export const DEFAULT_THEME_ID = 'light';

export const themes: ThemeOption[] = [
	{ id: 'light', label: 'Light' },
	{ id: 'dark', label: 'Dark' },
	{ id: 'high-contrast', label: 'High contrast' },
	{ id: 'ambulance', label: 'Ambulance' },
	{ id: 'midnight', label: 'Midnight' }
];
