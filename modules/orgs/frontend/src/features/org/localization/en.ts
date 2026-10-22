export const enMessages = {
  form: {
    createTitle: 'Create organization',
    editTitle: 'Update organization',
    viewTitle: 'Organization details',
    code: 'Code',
    name: 'Name',
    codePlaceholder: 'acme',
    namePlaceholder: 'Acme Corp',
    saveSuccess: 'Organization saved',
    activateSuccess: 'Organization activated',
    deactivateSuccess: 'Organization deactivated',
    saveError: 'Could not save the organization. Please try again.',
    loadError: 'Could not load the organization',
  },
  list: {
    description:
      'End-customer catalog for the working tenant. Records are deactivated, not deleted.',
    scopeHint:
      'The API requires X-Org-Id of an existing organization. Set VITE_ORG_ID or the orgId prop.',
    searchPlaceholder: 'Search by code or name…',
    statusAll: 'Any status',
    statusActive: 'Active',
    statusInactive: 'Inactive',
    create: 'Add organization',
    empty: 'No organizations yet',
    loading: 'Loading…',
  },
  table: {
    code: 'Code',
    name: 'Name',
    status: 'Status',
    actions: 'Actions',
    view: 'View',
    edit: 'Edit',
    activate: 'Activate',
    deactivate: 'Deactivate',
  },
  confirm: {
    activateTitle: 'Activate this organization?',
    deactivateTitle: 'Deactivate this organization?',
    activate: 'Activate',
    deactivate: 'Deactivate',
  },
  detail: {
    code: 'Code',
    name: 'Name',
    status: 'Status',
    tenant: 'Tenant',
    active: 'Active',
    inactive: 'Inactive',
  },
  routes: {
    list: 'Organizations',
  },
  menu: {
    orgs: 'Organizations',
  },
}
