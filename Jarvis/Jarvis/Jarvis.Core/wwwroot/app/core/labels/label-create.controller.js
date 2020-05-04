(function () {
    'use strict';

    var labelCreateController = function ($state, sweetAlert, labelService) {
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
                    ctrl.label = response.data;
                }
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            labelService.post(ctrl.label).then(function (response) {
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
        .controller('labelCreateController', labelCreateController);

    labelCreateController.$inject = ['$state', 'sweetAlert', 'labelService'];
}());