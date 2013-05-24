(function () {
    'use strict';

    var app = angular.module('barbato');

    app.controller(
        'ReposController', ['$scope', 'data', function ($scope, data) {
            $scope.items = data;
        }
        ]);

})();