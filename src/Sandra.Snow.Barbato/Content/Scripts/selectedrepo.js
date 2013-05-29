(function () {
    'use strict';

    var app = angular.module('barbato');

    app.controller(
        'SelectedRepoController', function ($scope, $routeParams, repoService) {
            $scope.item = repoService.getItem($routeParams.selectedRepo);
            $scope.item.userName = $routeParams.githubUser;
            $scope.item.deploymentType = 'azure';
            $scope.item.ftpserver = '';
            $scope.item.ftppath = '';
            $scope.item.ftpusername = '';
            $scope.item.ftppassword = '';
            $scope.item.azurerepo = '';
            
            $scope.saveDeployment = function() {
                repoService.initializeDeployment($scope.item);
            };
        }
        );

})();