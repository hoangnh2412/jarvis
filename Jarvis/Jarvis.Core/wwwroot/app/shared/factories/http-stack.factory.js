// https://plnkr.co/edit/Tmjw0MCfSbBSgWRhFvcg?preview
// https://stackoverflow.com/questions/14464945/add-queueing-to-angulars-http-service
// httpStackService({ method: '...', url: '...' }).then(function (result) {
// ...
// });

(function () {
    'use strict';

    var httpStackService = function ($q, $http) {
        var queue = [];

        var next = function () {
            var task = queue[queue.length - 1];
            $http(task.config).then(function (response) {
                queue.pop();
                task.delegate.resolve(response);

                if (queue.length > 0)
                    next();
            }, function (err) {
                queue.pop();
                task.delegate.reject(err);

                if (queue.length > 0)
                    next();
            });
        };

        return function (config) {
            var delegate = $q.defer();
            queue.push({
                config: config,
                delegate: delegate
            });

            if (queue.length === 1)
                next();
            return delegate.promise;
        };
    };

    angular
        .module('jarvis')
        .factory('httpStackService', httpStackService);
}());