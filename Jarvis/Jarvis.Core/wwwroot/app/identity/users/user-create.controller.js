﻿(function () {
    'use strict';

    var userCreateController = function ($state, sweetAlert, userService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.user = {
            isAutoPassword: true,
            infos: {}
        };
        ctrl.roles = [];

        ctrl.validationOptions = {
            rules: {
                fullName: {
                    required: true,
                    whiteSpace: true,
                    maxlength: 250
                },
                email: {
                    singleEmail: true,
                    maxlength: 256
                },
                phoneNumber: {
                    maxlength: 50
                },
                userName: {
                    required: true,
                    maxlength: 256,
                    regex: /^[a-zA-Z0-9][\w-\/.\@]{0,}[a-zA-Z0-9]$|^[a-zA-Z0-9]$/
                },
                password: {
                    required: true,
                    minlength: 6,
                    maxlength: 100,
                    regex: /^\S[^\t\n\r]+[\S]$/
                }
            },
            messages: {
                userName: {
                    regex: "Mã khách hàng viết không dấu, được phép chứa các ký tự đặc biệt ., -, _, /, @ và không đặt ở đầu hoặc cuối mã."
                },
                password: {
                    regex: "Mật khẩu không chứa ký tự tab và không bắt đầu hoặc kết thúc bằng khoảng trắng"
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

            ctrl.loading = true;
            userService.post(ctrl.user).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã tạo TÀI KHOẢN thành công!",
                        type: "success",
                    });
                    $state.go('identity.backend.user.read');
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.getRoles();
        };
    };

    angular
        .module('identity')
        .controller('userCreateController', userCreateController);

    userCreateController.$inject = ['$state', 'sweetAlert', 'userService'];
}());