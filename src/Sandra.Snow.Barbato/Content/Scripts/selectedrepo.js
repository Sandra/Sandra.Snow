(function (app) {
    'use strict';

    app.controller(
        'SelectedRepoController', function ($scope, $routeParams, repoService, $http) {
            $scope.item = repoService.getItem($routeParams.selectedRepo);
            $scope.item.userName = $routeParams.githubUser;
            $scope.item.deploymentType = 'azure';
            $scope.item.ftpserver = '';
            $scope.item.ftppath = '';
            $scope.item.ftpusername = '';
            $scope.item.ftppassword = '';
            $scope.item.azurerepo = '';
            $scope.item.azurerepotaken = false;

            $scope.saveDeployment = function () {
                repoService.initializeDeployment($scope.item);
            };

            $scope.blurCallback = function () {

                var xsrf = "repo=" + $scope.item.azurerepo;

                $http.post('http://localhost:12008/alreadyregistered', xsrf,
                {
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
                })
                .success(function (data, status, headers, config) {
                    $scope.item.azurerepotaken = data;
                })
                .error(function (data, status, headers, config) {
                    $scope.item.azurerepotaken = false;
                });
            };
        }
        );

})(app);