var app;

(function () {
    'use strict';

    app = angular.module('barbato', ['ui.utils']).
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
                   when('/:githubUser/:selectedRepo/complete', {
                       controller: 'CompleteController',
                       templateUrl: '/Content/templates/complete.html'
                   }).
                   otherwise({ redirectTo: '/' });
           });



    app.factory('repoService', ['$http', '$q', function ($http, $q) {
        var repoList = [];
        return {
            getItems: function (githubUser) {
                var deferred = $q.defer();

                console.log('json requested');
                $http.get('/getrepodata/' + githubUser).success(function (data) {
                    deferred.resolve(data);
                    repoList = data;
                }).error(function () {
                    deferred.reject();
                });
                return deferred.promise;
            },

            getItem: function (repoName) {
                return repoList.Repos.filter(function (x) { return x.Name === repoName })[0];
            },

            initializeDeployment: function (data) {
                $http.post('http://localhost:12008/initializedeployment', data).success(function () { console.log('deployed') });
            }
        };

    }]);

    app.directive('checker', function () {
        return {
            restrict: 'A',
            scope: {
                checkValidity: '=checkValidity' // isolate directive's scope and inherit only checking function from parent's one
            },
            require: 'ngModel', // controller to be passed into directive linking function
            link: function (scope, elem, attr, ctrl) {
                var yourFieldName = elem.attr('name');

                // check validity on field blur
                elem.bind('blur', function () {
                    scope.checkValidity(yourFieldName, elem.val(), function (res) {
                        if (res.valid) {
                            ctrl.$setValidity(yourFieldName, true);

                        } else {
                            ctrl.$setValidity(yourFieldName, false);

                        }
                    });
                });

                // set "valid" by default on typing
                elem.bind('keyup', function () {
                    ctrl.$setValidity(yourFieldName, true);
                    scope.$apply();
                });
            }
        };
    });

})();