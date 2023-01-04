(function () {
    'use strict';

    function tenantLogoController($state, $uibModal, $uibModalInstance, httpService, sweetAlert, code) {
        var ctrl = this;
        ctrl.files = null;

        ctrl.$onInit = function () {
            // console.log(code);
        };

        ctrl.ok = function () {
            var fd = new FormData();
            for (let i = 0; i < ctrl.files.length; i++) {
                fd.append('file', ctrl.files[i]);
            }

            httpService.post('/tenants/' + code + '/logo', fd, {
                transformRequest: angular.identity,
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            }).then(function (response) {
                if (response.status !== 200) {
                    return;
                }

                $uibModalInstance.close();
                sweetAlert.success('Thành công', 'Bạn đã cập nhật logo thành công!');
            });
        };

        ctrl.cancel = function () {
            // $uibModalInstance.dismiss('cancel');
            $uibModalInstance.close();
        };
    };

    angular
        .module('core')
        .controller('tenantLogoController', tenantLogoController);

    tenantLogoController.$inject = ['$state', '$uibModal', '$uibModalInstance', 'httpService', 'sweetAlert', 'code'];
}());