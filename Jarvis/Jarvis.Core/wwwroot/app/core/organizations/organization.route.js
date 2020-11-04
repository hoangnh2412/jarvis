(function () {
    'use strict';
    angular
        .module('core')
        .component('uiOrganizations', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiOrganizations', '/app/core/organizations/organizations.template.html');
            }],
            controller: 'organizationsController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('core.organization', {
                component: 'uiOrganizations',
                url: '/organizations',
                resolve: {
                    tree: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleTree');
                    }],
                    organizationsController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiOrganizations', '/app/core/organizations/organizations.controller.js'));
                    }],
                    organizationService: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.replace('/app/core/organizations/organization.service.js'));
                    }],
                }
            });
        });
}());