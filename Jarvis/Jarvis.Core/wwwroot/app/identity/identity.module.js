(function () {
    'use strict';

    angular
        .module('identity', [])
        .factory('httpInterceptor', ['$state', 'APP_CONFIG', 'sweetAlert', 'cacheService', function ($state, APP_CONFIG, sweetAlert, cacheService) {
            var getStateNameByUrl = function (url) {
                var states = $state.get();
                for (var i = 0; i < states.length; i++) {
                    var element = states[i];
                    if (element.url === url) {
                        return element.name;
                    }
                }
            };

            return {
                requestError: function (config) {
                    return config;
                },

                response: function (res) {
                    return res;
                },

                responseError: function (response) {
                    if (response.status === -1) {
                        sweetAlert.error('Lỗi', 'Không thể kết nối đến server');
                        return response;
                    }

                    if (response.status === 403) {
                        sweetAlert.warning('Cảnh báo', 'Bạn không có quyền sử dụng tính năng này');
                        $state.go(getStateNameByUrl(APP_CONFIG.DASHBOARD_URL));
                        return response;
                    }

                    if (response.status === 401) {
                        sweetAlert.warning('Cảnh báo', 'Phiên làm việc của bạn đã hết hạn, vui lòng đăng nhập lại');
                        cacheService.clean();
                        $state.go(getStateNameByUrl(APP_CONFIG.LOGIN_URL));
                        return response;
                    }

                    if (response.status === 404) {
                        sweetAlert.error("Lỗi", "Không tìm thấy dữ liệu");
                        return response;
                    }

                    if (response.status === 500 || response.status === 400) {
                        if (response.config.responseType && response.config.responseType === 'arraybuffer') {
                            var stringError = new TextDecoder().decode(response.data);
                            try {
                                var responseParse = JSON.parse(stringError);
                                if (responseParse.errors && Object.keys(responseParse.errors).length > 0) {
                                    var err = '';
                                    Object.keys(responseParse.errors).forEach(function (e) {
                                        for (var i = 0; i < responseParse.errors[e].length; i++) {
                                            err += responseParse.errors[e][i] + '</br>';
                                        };
                                    });
                                    swal.fire({
                                        title: "Lỗi",
                                        html: err,
                                        type: "error"
                                    });
                                    stringError = undefined;
                                }
                            }
                            catch (e) { }
                            finally {
                                if (stringError)
                                    sweetAlert.error("Lỗi", stringError);
                            }
                        }
                        else if (response.data.errors) {
                            if (Object.keys(response.data.errors).length > 0) {
                                var err = '';
                                Object.keys(response.data.errors).forEach(function (e) {
                                    for (var i = 0; i < response.data.errors[e].length; i++) {
                                        err += response.data.errors[e][i] + '</br>';
                                    };
                                });
                                swal.fire({
                                    title: "Lỗi",
                                    html: err,
                                    type: "error"
                                });
                            }
                        }
                        else
                            sweetAlert.error("Lỗi", response.data);
                        return response;
                    }


                    //if (response.status === 200) {
                    //    if (response.data.succeeded === false) {
                    //        if (response.data.code === 1006 || response.data.code === 2) {
                    //            sweetAlert.error(response.data.message || response.statusText);
                    //            $state.go('auth.login');
                    //        } else {
                    //            if (response.data.errors) {
                    //                sweetAlert.error(response.data.errors.map(function (x) { return x.description; }).join('<br/>'));
                    //            }
                    //            else {
                    //                sweetAlert.error(response.data.message || response.statusText);
                    //            }
                    //        }
                    //    }
                    //} else {
                    //    if (response.data.code === 20) {
                    //        sweetAlert.error(response.data.message + "<br/>" + response.data.errors.map(function (x) { return x.description; }).join('<br/>'));
                    //    }
                    //    else {
                    //        if (response.data) {
                    //            sweetAlert.error(response.data.message || response.statusText);
                    //        }
                    //    }
                    //}

                    return response;
                },
                request: function (config) {
                    var ignores = ["/login", "/register", ".html"];
                    var hasIgnore = ignores.filter(function (x) { return config.url.endsWith(x) });
                    if (hasIgnore.length > 0)
                        return config;

                    if (!config.headers['Content-Type']) {
                        config.headers['Content-Type'] = 'application/json';
                    }

                    if (config.headers['Content-Type'] === 'multipart/form-data') {
                        config.headers['Content-Type'] = undefined;
                    }

                    var token = cacheService.get('token');
                    if (!token)
                        return config;

                    config.headers.Authorization = 'Bearer ' + token.accessToken;

                    var context = cacheService.get('context');
                    if (context && context.connectionId)
                        config.headers['Connection-Id'] = context.connectionId;

                    return config;
                }
            };
        }])
        .factory('permissionService', ['cacheService', 'APP_CONFIG', function (cacheService, APP_CONFIG) {
            return {
                hasClaim: function (claim) {
                    var context = cacheService.get('context');

                    if (context)
                        for (var prop in context.claims) {
                            if (prop === 'Special_DoEnything') {
                                return true;
                            }

                            if (prop === 'Special_TenantAdmin' && APP_CONFIG.claimOfTenantAdmins.includes(claim)) {
                                return true;
                            }

                            if (prop === claim) {
                                return true;
                            }
                        }
                    return false;
                },
                hasClaims: function (claims) {
                    var context = cacheService.get('context');

                    if (context)
                        for (var prop in context.claims) {
                            if (prop === 'Special_DoEnything') {
                                return true;
                            }

                            for (var i = 0; i < claims.length; i++) {
                                if (prop === 'Special_TenantAdmin' && APP_CONFIG.claimOfTenantAdmins.includes(claims[i])) {
                                    return true;
                                }

                                if (prop === claims[i]) {
                                    return true;
                                }
                            }
                        }
                    return false;
                }
            };
        }])
        .config(['$httpProvider', function ($httpProvider) {
            //Custom request (ex: Authentication, ContentType)
            $httpProvider.interceptors.push('httpInterceptor');
        }])
        .component('uiIdentityBackend', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiIdentityBackend', '/app/identity/identity.backend.template.html');
            }],
            bindings: {
                context: '='
            }
        })
        .component('uiIdentityFrontend', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiIdentityFrontend', '/app/identity/identity.frontend.template.html');
            }],
            bindings: {
                context: '='
            }
        })
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('identity', {
                abstract: true,
                template: '<ui-view context="$ctrl.context"></ui-view>',
                resolve: {
                    validate: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleValidate');
                    }],
                    autofocus: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleAutofocus');
                    }],
                    tooltip: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleTooltip');
                    }],
                    uiBootstrap: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleUiBootstrap');
                    }]
                }
            });

            $stateProvider.state('identity.backend', {
                abstract: true,
                //templateUrl: '/system/app/identity/identity.backend.template.html'
                component: 'uiIdentityBackend'
            });

            $stateProvider.state('identity.frontend', {
                abstract: true,
                //templateUrl: '/system/app/identity/identity.frontend.template.html'
                component: 'uiIdentityFrontend'
            });
        }])
        .config(['$ocLazyLoadProvider', function ($ocLazyLoadProvider) {
            $ocLazyLoadProvider.config({
                modules: [
                    {
                        name: 'moduleImgCrop',
                        serie: true,
                        files: ['libs/angular/ng-img-crop/ng-img-crop.css', 'libs/angular/ng-img-crop/ng-img-crop.js']
                    },
                    {
                        name: 'moduleUiBootstrap',
                        serie: true,
                        files: ['libs/jquery/moment.js', 'libs/jquery/moment-with-locales.js', 'libs/angular/ui-bootstrap-tpls-2-5-0.js']
                    }
                ]
            });
        }])
        .run(['$transitions', 'APP_CONFIG', '$state', 'cacheService', 'permissionService', function ($transitions, APP_CONFIG, $state, cacheService, permissionService) {
            var getStateNameByUrl = function (url) {
                var states = $state.get();
                for (var i = 0; i < states.length; i++) {
                    var element = states[i];
                    if (element.url === url) {
                        return element.name;
                    }
                }
            };

            //On any page, if has data.authorize then check authorization
            $transitions.onBefore({
                to: function (state) {
                    //No authentication required
                    if (!state.data) {
                        return false;
                    }

                    //No authentication required
                    if (!state.data.authorize) {
                        return false;
                    }

                    //Require authentication
                    var token = cacheService.get('token');

                    //Reuire authentication but not token
                    if (!token) {
                        return true;
                    }

                    //Reuire authentication but token has expired
                    if (new Date(token.expireAt) < new Date()) {
                        cacheService.remove('token');
                        return true;
                    }

                    //Ignore role, claim
                    if (state.data.authorize.length === 0) {
                        return false;
                    }

                    var hasPermission = permissionService.hasClaims(state.data.authorize);
                    return !hasPermission;
                }
            }, function () {
                return $state.target(getStateNameByUrl(APP_CONFIG.LOGIN_URL));
            });

            //On login page, if has token then redirect to dashboard
            $transitions.onBefore({
                to: function (state) {
                    return state.url.pattern === APP_CONFIG.LOGIN_URL;
                }
            }, function () {
                var token = cacheService.get('token');

                //Not token => not authenticate
                if (!token) {
                    return;
                }

                //Token expired => not authenticate
                if (new Date(token.expireAt) < new Date()) {
                    cacheService.remove('token');
                    return;
                }

                return $state.target(getStateNameByUrl(APP_CONFIG.DASHBOARD_URL));
            });
        }]);
}());