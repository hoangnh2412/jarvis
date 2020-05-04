(function () {
    'use strict';

    var userUpdateController = function ($state, $stateParams, sweetAlert, userService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.user = {
            infos: {}
        };
        ctrl.roles = [];

        ctrl.validationOptions = {
            rules: {
                fullName: {
                    required: true
                },
                email: {
                    multipleEmails: true
                },
                userName: {
                    required: true
                }
            }
        };

        ctrl.getUser = function (id) {
            userService.getById(id).then(function (response) {
                if (response.status === 200) {
                    ctrl.user = response.data;
                    ctrl.getRoles();
                }
            });
        };

        ctrl.getRoles = function () {
            userService.getRoles({ page: 1, size: 99999 }).then(function (response) {
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

            userService.put(ctrl.user).then(function (response) {
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã sửa TÀI KHOẢN thành công!",
                        type: "success",
                    });
                    $state.go('identity.backend.user.read');
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.getUser($stateParams.id);
        };
    };

    angular
        .module('identity')
        .controller('userUpdateController', userUpdateController);

    userUpdateController.$inject = ['$state', '$stateParams', 'sweetAlert', 'userService'];
}());