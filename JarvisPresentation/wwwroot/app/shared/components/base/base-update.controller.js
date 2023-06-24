(function () {
    'use strict';

    var baseUpdateController = function ($state, $stateParams, sweetAlert, baseService) {
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
            baseService.get(id).then(function (response) {
                if (response.status === 200) {
                    ctrl.item = response.data;
                }
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            baseService.put(ctrl.item).then(function (response) {
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã sửa nhãn thành công!",
                        type: "success",
                    });
                    // $state.go('core.label.read');
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.get($stateParams.id);
        };
    };

    angular
        .module('core')
        .controller('baseUpdateController', baseUpdateController);

    baseUpdateController.$inject = ['$state', '$stateParams', 'sweetAlert', 'baseService'];
}());