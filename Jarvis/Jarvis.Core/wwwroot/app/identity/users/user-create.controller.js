(function () {
    'use strict';

    var userCreateController = function ($state, sweetAlert, userService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.user = {
            isAutoPassword: true,
            infos: {}
        };
        ctrl.roles = [];
        ctrl.claims = [];

        ctrl.validationOptions = {
            rules: {
                fullName: {
                    required: true,
                    whiteSpace: true
                },
                email: {
                    singleEmail: true,
                    maxlength: 256
                },
                userName: {
                    required: true,
                    maxlength: 256,
                    regex: /^[a-zA-Z0-9][\w-\/]{0,}[a-zA-Z0-9]$|^[a-zA-Z0-9]$/
                }
            },
            messages: {
                userName: {
                    regex: "Tài khoản đăng nhập viết không dấu, được phép chứa các ký tự đặc biệt -, _, / và không đặt ở đầu hoặc cuối"
                }
            }
        };

        ctrl.getUser = function (id) {
            ctrl.loading = true;
            userService.getById(id).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    ctrl.user = response.data;
                    ctrl.getRoles();
                }
            });
        };

        ctrl.getRoles = function () {
            ctrl.loading = true;
            userService.getRoles({ page: 1, size: 99999 }).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    ctrl.roles = response.data.data;

                    if (ctrl.user.id) {
                        for (var i = 0; i < ctrl.roles.length; i++) {
                            for (var j = 0; j < ctrl.user.idRoles.length; j++) {
                                if (ctrl.user.idRoles[j] === ctrl.roles[i].id) {
                                    ctrl.roles[i].select = true;
                                }
                            }
                        }
                    }
                }
            });
        };

        ctrl.getClaims = function () {
            userService.getClaims().then(function (response) {
                if (response.status === 200) {
                    ctrl.claims = response.data;
                }
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            ctrl.user.idRoles = [];
            var roles = ctrl.roles.filter(function (role) { return role.select === true; });
            for (var i = 0; i < roles.length; i++) {
                if (roles[i].select) {
                    ctrl.user.idRoles.push(roles[i].id);
                }
            }

            ctrl.user.claims = [];
            var claims = ctrl.claims.filter(function (claim) { return claim.select === true; });
            for (let i = 0; i < claims.length; i++) {
                const element = claims[i];
                ctrl.user.claims.push(element.key);
            }

            ctrl.loading = true;
            userService.post(ctrl.user).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã tạo TÀI KHOẢN thành công!",
                        type: "success",
                    });
                    $state.go('identity.management.user.read');
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.getRoles();
            // ctrl.getClaims();
        };
    };

    angular
        .module('identity')
        .controller('userCreateController', userCreateController);

    userCreateController.$inject = ['$state', 'sweetAlert', 'userService'];
}());