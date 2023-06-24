(function () {
    'use strict';

    function emailTemplateService(httpService) {
        var api = '/emails/templates';

        this.get = function (code) {
            return httpService.get(api + '/' + code);
        };

        this.pagination = function (paging) {
            return httpService.get(api, { params: paging });
        };

        this.post = function (emailTemplate) {
            return httpService.post(api, emailTemplate);
        };

        this.put = function (emailTemplate) {
            return httpService.put(api + '/' + emailTemplate.key, emailTemplate);
        };

        this.delete = function (code) {
            return httpService.delete(api + '/' + code);
        };
    };

    angular
        .module('app')
        .service('emailTemplateService', emailTemplateService);
    emailTemplateService.$inject = ['httpService'];
}());