(function () {
    'use strict';

    var app = angular.module('barbato', []).
           config(function ($routeProvider) {
               $routeProvider.
                   when('/', {
                       controller: 'ReposController',
                       templateUrl: '/Content/templates/repos.html'
                   }).
                   otherwise({ redirectTo: '/' });
           });

    app.factory('repoService', function ($http) {

        return {
            getItems: function () {
                $http.get('http://localhost:12008/getrepodata/jchannon').then(function (response) {
                    console.log(response.data);
                    return response.data;
                });
            },
        };
    });

})();