(function () {
    'use strict';

    angular
        .module('jarvis')
        .constant('APP_CONFIG', {
            VISUALIZER: false,
            BASE_UI_PATH: '/app/',
            BASE_API_PATH: '/api/v4',
            APP_NAME: 'JARVIS',
            NAMESPACE_STORAGE: 'jarvis',
            DEFAULT_URL: '/login',
            DASHBOARD_URL: '/dashboard',
            LOGIN_URL: '/login',
            PRESENTATION: {
                THEME_NAME: 'adminlte',
                SKIN_NAME: 'skin-blue',
                //Customize template of component
                TEMPLATE_URLS: {
                    // 'uiCore': '/app/core/core.template.html',
                    'uiLabelRead': '/app/core/labels/label-read.template.html',
                    'uiLabelCreate': '/app/core/labels/label-create.template.html',
                    'uiLabelUpdate': '/app/core/labels/label-update.template.html',
                    'uiOrganizations': '/app/core/organizations/organizations.template.html',
                    'uiSettings': '/app/core/settings/settings.template.html',
                    'uiTenantInfo': '/app/core/tenant-info/tenant-info.template.html',
                    'uiTenantRead': '/app/core/tenants/tenant-read.template.html',
                    'uiTenantCreate': '/app/core/tenants/tenant-create.template.html',
                    'uiTenantUpdate': '/app/core/tenants/tenant-update.template.html',
                    'uiTenantLogo': '/app/core/tenants/tenant-logo.template.html',

                    // 'uiIdentityManagement': '/app/identity/identity-management.template.html',
                    'uiIdentityAuth': '/app/identity/identity-auth.template.html',
                    'uiUserInfo': '/app/shared/components/user-info/user-info.template.html',
                    'uiChangePassword': '/app/identity/change-password/change-password.template.html',
                    'uiForgotPassword': '/app/identity/forgot-password/forgot-password.template.html',
                    // 'uiLogin': '/app/identity/login/login.template.html',
                    'uiProfile': '/app/identity/profile/profile.template.html',
                    'uiRoleRead': '/app/identity/roles/role-read.template.html',
                    'uiRoleCreate': '/app/identity/roles/role-create.template.html',
                    'uiRoleUpdate': '/app/identity/roles/role-update.template.html',
                    'uiUserRead': '/app/identity/users/user-read.template.html',
                    'uiUserCreate': '/app/identity/users/user-create.template.html',
                    'uiUserUpdate': '/app/identity/users/user-update.template.html',

                    'uiEmailTemplateList': '/app/core/emails/email-template-list.template.html',
                    'uiEmailTemplateDetail': '/app/core/emails/email-template-detail.template.html',
                    'uiEmailHistoryList': '/app/core/emails/email-history-list.template.html',

                    'uiAlert': '/app/shared/components/alert/alert.template.html',
                    'uiFooter': '/app/shared/components/footer/footer.template.html',
                    'uiLoading': '/app/shared/components/loading/loading.template.html',
                    // 'uiNavbar': '/app/shared/components/navbar/navbar.template.html',
                    'uiSidebar': '/app/shared/components/sidebar/sidebar.adminlte.template.html',
                    // 'uiTopbar': '/app/shared/components/topbar/topbar.template.html',
                    'uiTopbarToggle': '/app/shared/components/topbar-toggle/topbar-toggle.adminlte.template.html'
                },
                //Customize controller of component
                CONTROLLER_URLS: {
                    'uiLabelRead': '/app/core/labels/label-read.controller.js',
                    'uiLabelCreate': '/app/core/labels/label-create.controller.js',
                    'uiLabelUpdate': '/app/core/labels/label-update.controller.js',
                    'uiOrganizations': '/app/core/organizations/organizations.controller.js',
                    'uiOrganizationUnit': '/app/core/organizations/organization-unit.controller.js',
                    'uiOrganizationUser': '/app/core/organizations/organization-user.controller.js',
                    'uiSettings': '/app/core/settings/settings.controller.js',
                    'uiTenantInfo': '/app/core/tenant-info/tenant-info.controller.js',
                    'uiTenantRead': '/app/core/tenants/tenant-read.controller.js',
                    'uiTenantCreate': '/app/core/tenants/tenant-create.controller.js',
                    'uiTenantUpdate': '/app/core/tenants/tenant-update.controller.js',
                    'uiTenantLogo': '/app/core/tenants/tenant-logo.controller.js',

                    // 'uiUserInfo': '/app/shared/components/user-info/user-info.controller.js',
                    // 'uiIdentityManagement': '/app/identity/user-info.controller.js',
                    // 'uiChangePassword': '/app/identity/change-password/change-password.controller.js',
                    // 'uiForgotPassword': '/app/identity/forgot-password/forgot-password.controller.js',
                    // 'uiLogin': '/app/identity/login/login.controller.js',
                    // 'uiProfile': '/app/identity/profile/profile.controller.js',
                    'uiRoleRead': '/app/identity/roles/role-read.controller.js',
                    'uiRoleCreate': '/app/identity/roles/role-create.controller.js',
                    'uiRoleUpdate': '/app/identity/roles/role-update.controller.js',
                    'uiUserRead': '/app/identity/users/user-read.controller.js',
                    'uiUserCreate': '/app/identity/users/user-create.controller.js',
                    'uiUserUpdate': '/app/identity/users/user-update.controller.js',

                    'uiEmailTemplateList': '/app/core/emails/email-template-list.controller.js',
                    'uiEmailTemplateDetail': '/app/core/emails/email-template-detail.controller.js',
                    'uiEmailHistoryList': '/app/core/emails/email-history-list.controller.js'

                    // 'uiAlert': '/app/shared/components/alert/alert.controller.js',
                    // 'uiFooter': '/app/shared/components/footer/footer.controller.js',
                    // 'uiLoading': '/app/shared/components/loading/loading.controller.js',
                    // 'uiNavbar': '/app/shared/components/navbar/navbar.controller.js',
                    // 'uiSidebar': '/app/shared/components/sidebar/sidebar.controller.js',
                    // 'uiTopbar': '/app/shared/components/topbar/topbar.controller.js',
                    // 'uiTopbarToggle': '/app/shared/components/topbar-toggle/topbar-toggle.controller.js'
                }
            },
            //Mapping nagication code from BE to URL FE
            NAVIGATION_CODE_MAPPING_TO_STATE: {
                //Core
                'tenant-info': 'core.tenant-info',
                'users': 'identity.management.user',
                'roles': 'identity.management.role',
                'labels': 'core.label',
                'settings': 'core.settings',
                'tenants': 'core.tenant',
                'tenant': 'core.tenant',
                'licenses': 'core.license',
                'organization': 'core.organization',
                'email-template': 'core.email-template',
                'email-history': 'core.email-history',

                'dashboard': 'admin.dashboard({ from: "' + moment().subtract(7, 'days').format('YYYY-MM-DD') + '", to: "' + moment().format('YYYY-MM-DD') + '" })',
                'word-statistic': 'admin.catalog.word-statistic',
                'word-agent': 'admin.catalog.word-agent',
                'word-customer': 'admin.catalog.word-customer',
                'word-phonetic': 'admin.catalog.word-phonetic',
                'intent': 'admin.catalog.intent',
                'criteria': 'admin.catalog.criteria',
                'script': 'admin.catalog.script',
                'call': 'admin.business.call',
                'caller': 'admin.business.caller',
            },
            NAVIGATION: [],
            // Thêm thông báo vào cặp dấu ''
            MESSAGE: ''
        });
})();