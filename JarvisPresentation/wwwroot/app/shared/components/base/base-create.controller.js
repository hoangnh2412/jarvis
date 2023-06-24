(function () {
    'use strict';

    var baseCreateController = function ($state, sweetAlert, baseService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.item = {
            name: null,
            description: null,
            color: '#3c8dbc'
        };

        ctrl.validationOptions = {
            rules: {
                name: {
                    required: true
                },
                color: {
                    required: true
                }
            }
        };

        ctrl.get = function (id) {
            baseService.getById(id).then(function (response) {
                if (response.status === 200) {
                    ctrl.item = response.data;
                }
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            baseService.post(ctrl.item).then(function (response) {
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã tạo nhãn thành công!",
                        type: "success",
                    });
                    $state.go('core.label.read');
                }
            });
        };

        ctrl.$onInit = function () {
        };
    };

    angular
        .module('core')
        .controller('baseCreateController', baseCreateController);

    baseCreateController.$inject = ['$state', 'sweetAlert', 'baseService'];
}());