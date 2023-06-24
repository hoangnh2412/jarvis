(function () {
    'use strict';

    function emailHistoryService(httpService) {
        var api = '/emails/history';

        this.get = function (code) {
            return httpService.get(api + '/' + code);
        };

        this.pagination = function (paging) {
            return httpService.get(api, { params: paging });
        };

        this.post = function (emailHistory) {
            return httpService.post(api, emailHistory);
        };

        this.put = function (emailHistory) {
            return httpService.put(api + '/' + emailHistory.code, emailHistory);
        };

        this.delete = function (code) {
            return httpService.delete(api + '/' + code);
        };
    };

    angular
        .module('app')
        .service('emailHistoryService', emailHistoryService);
    emailHistoryService.$inject = ['httpService'];
}());