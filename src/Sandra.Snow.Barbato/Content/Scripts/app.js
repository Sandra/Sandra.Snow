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
            },

            initializeDeployment: function (data) {
                $http.post('http://localhost:12008/initializedeployment', data).success(function () { console.log('deployed') });
            }
        };

    }]);


    

    app.directive('repoAvailable', function ($http, $timeout) { // available
        return {
            require: 'ngModel',

            link: function (scope, elem, attr, ctrl) {
                console.log(ctrl);
                scope.$watch('item.azurerepotaken', function (newValue, oldValue) {
                    console.log("new:" + newValue + "old:" + oldValue);
                    ctrl.$setValidity('checkingRepo', newValue);
                });
                //ctrl.$parsers.push(function (viewValue) {
                //    // set it to true here, otherwise it will not 
                //    // clear out when previous validators fail.
                //    ctrl.$setValidity('repoAvailable', true);
                //    if (ctrl.$valid) {
                //        // set it to false here, because if we need to check 
                //        // the validity of the email, it's invalid until the 
                //        // AJAX responds.
                //        ctrl.$setValidity('checkingRepo', false);

                //        // now do your thing, chicken wing.
                //        if (viewValue !== "" && typeof viewValue !== "undefined") {
                //            var xsrf = "repo=" + viewValue;
                //            $http.post('http://localhost:12008/alreadyregistered', xsrf,
                //            {
                //                 headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
                //            }) //set to 'Test.json' for it to return true.
                //                .success(function (data, status, headers, config) {
                //                    ctrl.$setValidity('repoAvailable', true);
                //                    ctrl.$setValidity('checkingRepo', true);
                //                })
                //                .error(function (data, status, headers, config) {
                //                    ctrl.$setValidity('repoAvailable', false);
                //                    ctrl.$setValidity('checkingRepo', true);
                //                });
                //        } else {
                //            ctrl.$setValidity('repoAvailable', false);
                //            ctrl.$setValidity('checkingRepo', true);
                //        }
                //    }
                //    return viewValue;
                //});

            }
        };
    });

})();