(function () {
    'use strict';

    var app = angular.module('barbato');

    app.controller(
        'ReposController',
        function ($scope, repoService) {
            $scope.items = repoService.getItems();
        }
    );

})();