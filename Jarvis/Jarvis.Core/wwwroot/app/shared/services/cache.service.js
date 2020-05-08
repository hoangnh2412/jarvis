(function () {
    'use strict';

    function cacheService(appStore) {
        this.all = function () {
            return appStore.inMemoryCache;
        };

        this.get = function (name) {
            return appStore.get(name);
        };

        this.set = function (name, value) {
            appStore.set(name, value);
        };

        this.remove = function (name) {
            appStore.remove(name);
        };

        this.clean = function () {
            appStore.clear();
        };
    };

    angular
        .module('jarvis')
        .service('cacheService', cacheService)
        .factory('appStore', ['APP_CONFIG', 'store', function (APP_CONFIG, store) {
            return store.getNamespacedStore(APP_CONFIG.NAMESPACE_STORAGE.toLowerCase());
        }]);
    cacheService.$inject = ['appStore'];
}());