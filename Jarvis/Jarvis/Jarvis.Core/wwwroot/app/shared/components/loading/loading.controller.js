(function () {
    'use strict';

    function loadingController() {
        var ctrl = this;
        
        ctrl.$onInit = () => {
        };
    };

    angular
        .module('app')
        .controller('loadingController', loadingController);

    loadingController.$inject = [];
}());