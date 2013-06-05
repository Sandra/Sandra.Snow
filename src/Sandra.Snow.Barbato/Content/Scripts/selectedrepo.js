(function (app) {
    'use strict';

    app.controller(
        'SelectedRepoController', function ($scope, $routeParams, repoService, $http, $location) {
            $scope.item = repoService.getItem($routeParams.selectedRepo);
            $scope.item.userName = $routeParams.githubUser;
            $scope.item.deploymentType = 'azure';
            $scope.item.ftpserver = '';
            $scope.item.ftppath = '';
            $scope.item.ftpusername = '';
            $scope.item.ftppassword = '';
            $scope.item.azurerepo = '';
            $scope.item.serversidevalid = true;
            $scope.item.deploying = false;

            $scope.saveDeployment = function () {

                var data = {
                    azureDeployment: $scope.item.deploymentType === 'azure',
                    repo: $scope.item.deploymentType === 'azure' ? $scope.item.azurerepo : $scope.item.ftpserver,
                    username: $scope.item.userName
                };
                
                $http.post('http://localhost:12008/alreadyregistered', data).success(function (data) {
                    if (data.isValid) {
                        $scope.item.serversidevalid = true;
                        $scope.item.deploying = true;
                        
                        $http.post('http://localhost:12008/initializedeployment', data).success(function() {
                            $scope.item.deploying = false;
                            $location.path($location.path() + "/complete");
                            console.log('deployed');
                        });                        
                    } else {
                        $scope.item.serversidevalid = false;
                        for (var i = 0; i < data.keys.length; i++) {
                            $scope.myForm[data.keys[0]].$setValidity(data.keys[0], false);
                        }
                    }
                });
            };

            $scope.test = function() {
                $scope.myForm.azurerepo.$setValidity(true);
                $scope.$apply();
            };

            $scope.azureCallback = function () {

                //var xsrf = "repo=" + $scope.item.azurerepo + "&username=" + $scope.item.userName;

                //$http.post('http://localhost:12008/alreadyregistered', xsrf,
                //{
                //    headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
                //})
                //.success(function (data, status, headers, config) {
                //    $scope.item.azurerepotaken = data;
                //})
                //.error(function (data, status, headers, config) {
                //    $scope.item.azurerepotaken = false;
                //});

                alreadyRegistered(true);
            };

            $scope.ftpCallback = function () {

                //var xsrf = "repo=" + $scope.item.ftpserver + "&username=" + $scope.item.userName;

                //$http.post('http://localhost:12008/alreadyregistered', xsrf,
                //{
                //    headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
                //})
                //.success(function (data, status, headers, config) {
                //    $scope.item.azurerepotaken = data;
                //})
                //.error(function (data, status, headers, config) {
                //    $scope.item.azurerepotaken = false;
                //});

                alreadyRegistered(false);
            };
            
            $scope.checkValidity = function (fieldName, fieldValue, callback) {
                var data = {
                    azureDeployment: fieldName === 'azurerepo',
                    repo: fieldName === 'azurerepo' ? $scope.item.azurerepo : $scope.item.ftpserver,
                    username: $scope.item.userName
                };
                
                $http.post('http://localhost:12008/alreadyregistered', data).success(function (res) {
                    return callback(res);
                });
            };

            function alreadyRegistered(azure) {

                var data = {
                    azureDeployment: azure,
                    repo: azure ? $scope.item.azurerepo : $scope.item.ftpserver,
                    username: $scope.item.userName
                };

                $http.post('http://localhost:12008/alreadyregistered', data)
                .success(function (data, status, headers, config) {
                    if (data === 'true') {
                        $scope.myForm.azurerepo.$setValidity('azurerepo', false);
                       
                    } else {
                        $scope.myForm.azurerepo.$setValidity(true);
                    }

                })
                .error(function (data, status, headers, config) {
                    $scope.item.azurerepotaken = false;
                });
            }
        }
        );

})(app);