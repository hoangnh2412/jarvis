(function () {
    'use strict';

    var organizationUserController = function ($state, sweetAlert, organizationService, $uibModal, $document) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.users = [];

        ctrl.$onInit = function () {
            ctrl.getUsers();
        };

        ctrl.getUsers = function () {

        };
    };

    angular
        .module('core')
        .controller('organizationUserController', organizationUserController);

    organizationUserController.$inject = ['$state', 'sweetAlert', 'organizationService', '$uibModal', '$document'];
}());