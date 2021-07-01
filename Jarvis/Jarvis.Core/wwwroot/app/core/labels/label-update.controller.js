(function () {
    'use strict';

    var labelUpdateController = function ($state, $stateParams, sweetAlert, labelService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.label = {
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

        ctrl.changeColor = function (color) {
            ctrl.label.color = color;
        };

        ctrl.getLabel = function (id) {
            labelService.getById(id).then(function (response) {
                if (response.status === 200) {
                    ctrl.label = response.data.data;
                }
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            labelService.put(ctrl.label).then(function (response) {
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã sửa nhãn thành công!",
                        type: "success",
                    });
                    $state.go('core.label.read');
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.getLabel($stateParams.id);
        };
    };

    angular
        .module('core')
        .controller('labelUpdateController', labelUpdateController);

    labelUpdateController.$inject = ['$state', '$stateParams', 'sweetAlert', 'labelService'];
}());