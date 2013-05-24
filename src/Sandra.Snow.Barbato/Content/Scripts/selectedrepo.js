(function () {
    'use strict';

    var app = angular.module('barbato');

    app.controller(
        'SelectedRepoController', function ($scope, $routeParams, repoService) {
            $scope.item = repoService.getItem($routeParams.selectedRepo);
            $scope.item.userName = $routeParams.githubUser;
            $scope.item.deploymentType = 'azure';
        }
        );

})();