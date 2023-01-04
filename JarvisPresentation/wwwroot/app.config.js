(function () {
    'use strict';

    angular
        .module('jarvis')
        .constant('APP_CONFIG', {
            THEME: 'skin-green',
            VISUALIZER: false,
            THEME: 'skin-blue',
            APP_NAME: 'JARVIS',
            BASE_UI_PATH: '/app/',
            NAMESPACE_STORAGE: 'jarvis-dev',
            DEFAULT_URL: '/login',
            DASHBOARD_URL: '/dashboard',
            LOGIN_URL: '/login',
            //Customize template of component
            TEMPLATE_URLS: {
                // 'uiCore': '/customize/core/core.template.html',
                // 'uiNavbar': '/customize/shared/components/navbar/navbar.template.html',
                // 'uiTopbar': '/customize/shared/components/topbar/topbar.template.html',
                // 'uiFooter': '/app/shared/components/footer/footer.template.html',
                // 'uiSidebar': '/customize/shared/components/sidebar/sidebar.template.html',
                // 'uiUserInfo': '/app/shared/components/user-info/user-info.template.html',
                // 'uiIdentity': '/customize/identity/identity.template.html',
                'uiLogin': '/app/core/identity/login/login.admin2.template.html',
                // 'uiUserRead': '/customize/identity/users/user-read.template.html',
                // 'uiTenantInfo': '/customize/core/tenant-info/tenant-info.template.html'
            },
            //Customize controller of component
            CONTROLLER_URLS: {
                // 'tenantInfoController': '/customize/core/tenant-info/tenant-info.controller.js'
            },
            //Mapping nagication code from BE to URL FE
            NAVIGATION_CODE_MAPPING_TO_STATE: {
                //Core
                'system': null,
                'users': 'identity.backend.user',
                'roles': 'identity.backend.role',
                'labels': 'core.label',
                'settings': 'core.settings',
                'tenant-info': 'core.tenant-info',
                'tenants': 'core.tenant',
                'tenant': 'core.tenant',

                // 'dashboard': 'vnis.dashboard',
                // 'email': 'catalog.email',
            },
            claimOfTenantAdmins: [
                'User_Read', 'User_Create', 'User_Update', 'User_Delete', 'User_Lock', 'User_Reset_Password',
                'Tenant_Create', 'Tenant_Read', 'Tenant_Update', 'Tenant_Delete',
                'Role_Create', 'Role_Delete', 'Role_Read', 'Role_Update',
                'Setting_Read', 'Setting_Update',
                'OrganizationUnit_Create', 'OrganizationUnit_Delete', 'OrganizationUnit_Read', 'OrganizationUnit_Roles', 'OrganizationUnit_Update', 'OrganizationUnit_Users',
                'Label_Create', 'Label_Delete', 'Label_Read', 'Label_Update',
                'License_Update'
            ],
            // Thêm thông báo vào cặp dấu ''
            MESSAGE: ''
        })
        .constant('CONST_PERMISSION', {
            // hệ thống
            User_Read: 'User_Read',
            User_Create: 'User_Create',
            User_Update: 'User_Update',
            User_Delete: 'User_Delete',
            User_Lock: 'User_Lock',
            User_Reset_Password: 'User_Reset_Password',
            Role_Read: 'Role_Read',
            Role_Create: 'Role_Create',
            Role_Update: 'Role_Update',
            Role_Delete: 'Role_Delete',
            Tenant_Read: 'Tenant_Read',
            Tenant_Create: 'Tenant_Create',
            Tenant_Update: 'Tenant_Update',
            Tenant_Delete: 'Tenant_Delete',
            Label_Read: 'Label_Read',
            Label_Create: 'Label_Create',
            Label_Update: 'Label_Update',
            Label_Delete: 'Label_Delete',
            Setting_Read: 'Setting_Read',
            Setting_Update: 'Setting_Update',
            License_Update: 'License_Update',
            OrganizationUnit_Read: 'OrganizationUnit_Read',
            OrganizationUnit_Create: 'OrganizationUnit_Create',
            OrganizationUnit_Update: 'OrganizationUnit_Update',
            OrganizationUnit_Delete: 'OrganizationUnit_Delete',
            OrganizationUnit_Users: 'OrganizationUnit_Users',
            OrganizationUnit_Roles: 'OrganizationUnit_Roles',
        });
})();