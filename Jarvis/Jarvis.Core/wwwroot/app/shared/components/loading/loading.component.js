(function () {
    'use strict';

    angular
        .module('jarvis')
        .component('uiLoading', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiLoading', '/app/shared/components/loading/loading.template.html');
            }],
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('jarvis.shared.loading', {
                component: 'uiLoading'
            });
        });

}());