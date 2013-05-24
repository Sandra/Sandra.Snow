(function () {
    'use strict';

    var app = angular.module('barbato', []).
           config(function ($routeProvider) {
               $routeProvider.
                   when('/:githubUser', {
                       controller: 'ReposController',
                       templateUrl: '/Content/templates/repos.html',
                       resolve: {
                           data: function (repoService, $route) {
                               return repoService.getItems($route.current.params.githubUser);
                           }
                       }
                   }).
                   when('/:githubUser/:selectedRepo', {
                       controller: 'SelectedRepoController',
                       templateUrl: '/Content/templates/repoDetail.html'
                   }).
                   otherwise({ redirectTo: '/' });
           });

    app.factory('repoService', ['$http', '$q', function ($http, $q) {
        var repoList = [];
        return {
            getItems: function (githubUser) {
                var deferred = $q.defer();

                console.log('json requested');
                $http.get('http://localhost:12008/getrepodata/' + githubUser).success(function (data) {
                    deferred.resolve(data);
                    repoList = data;
                }).error(function () {
                    deferred.reject();
                });
                return deferred.promise;

            },

            getItem: function (repoName) {
                return repoList.Repos.filter(function (x) { return x.Name === repoName })[0];
            }
        };

    }]);

})();