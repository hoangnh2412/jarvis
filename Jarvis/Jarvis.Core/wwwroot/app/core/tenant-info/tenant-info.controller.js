(function () {
    'use strict';

    function tenantInfoController(sweetAlert, tenantInfoService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.tenant = {};

        ctrl.$onInit = function () {
            ctrl.getTenantInfo();
        };

        ctrl.getTenantInfo = function () {
            tenantInfoService.get().then(function (response) {
                if (response.status !== 200) {
                    return;
                }

                ctrl.tenant = response.data;
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            tenantInfoService.put(ctrl.tenant).then(function (response) {
                if (response.status !== 200) {
                    return;
                }

                sweetAlert.success('Thành công', 'Bạn đã cập nhật thông tin doanh nghiệp thành công');
            });
        };
    };

    angular
        .module('core')
        .controller('tenantInfoController', tenantInfoController);

    tenantInfoController.$inject = ['sweetAlert', 'tenantInfoService'];
}());