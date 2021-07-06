(function () {
    'use strict';

    function httpService($http, $location, APP_CONFIG, cacheService, VNIS_CONFIG) {
        //Build API endpoint
        //Nếu có cấu hình API_URL (debug mode) => Sử dụng luôn API_URL
        //Nếu ko cấu hình API_URL (live mode) => Lấy theo domain
        var domain = "";
        if (APP_CONFIG.API_URL) {
            domain = APP_CONFIG.API_URL;
        } else {
            var protocol = $location.protocol();
            domain = $location.host();
            var port = $location.port();
            if (port !== 80 && port !== 443) {
                domain = domain + ':' + port;
            }
            domain = protocol + '://' + domain;
        }

        //Chuyển tenant
        var getCurrentTenantCode = function () {
            var tenantCode = null;
            var currentTenant = cacheService.get('currentTenant');

            var context = cacheService.get('context');
            if (!context) {
                return tenantCode;
            }

            if (VNIS_CONFIG.DOMAIN_SEARCH_TENANT && VNIS_CONFIG.DOMAIN_SEARCH_TENANT.includes($location.host()))
                return currentTenant.code;

            if (context.tenantInfo.code !== currentTenant.code) {
                tenantCode = currentTenant.code;
            }
            return tenantCode;
        };

        this.get = function (url, config) {
            var tenantCode = getCurrentTenantCode();
            if (tenantCode !== null) {
                if (!config) {
                    config = {};
                }

                if (!config.params) {
                    config.params = {};
                }
                config.params.tenantCode = tenantCode;
            }
            return $http.get(domain + url, config);
        };

        this.post = function (url, data, config) {
            var tenantCode = getCurrentTenantCode();
            if (tenantCode !== null) {
                if (!config) {
                    config = {};
                }

                if (!config.params) {
                    config.params = {};
                }
                config.params.tenantCode = tenantCode;
            }
            return $http.post(domain + url, data, config);
        };

        this.put = function (url, data, config) {
            var tenantCode = getCurrentTenantCode();
            if (tenantCode !== null) {
                if (!config) {
                    config = {};
                }

                if (!config.params) {
                    config.params = {};
                }
                config.params.tenantCode = tenantCode;
            }
            return $http.put(domain + url, data, config);
        };

        this.patch = function (url, data, config) {
            var tenantCode = getCurrentTenantCode();
            if (tenantCode !== null) {
                if (!config) {
                    config = {};
                }

                if (!config.params) {
                    config.params = {};
                }
                config.params.tenantCode = tenantCode;
            }
            return $http.patch(domain + url, data, config);
        };

        this.delete = function (url, config) {
            var tenantCode = getCurrentTenantCode();
            if (tenantCode !== null) {
                if (!config) {
                    config = {};
                }

                if (!config.params) {
                    config.params = {};
                }
                config.params.tenantCode = tenantCode;
            }
            return $http.delete(domain + url, config);
        };
    };

    angular
        .module('jarvis')
        .service('httpService', httpService);
    httpService.$inject = ['$http', '$location', 'APP_CONFIG', 'cacheService', 'VNIS_CONFIG'];
}());