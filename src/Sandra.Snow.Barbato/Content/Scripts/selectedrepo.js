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
            $scope.item.deploysuccess = false;
            $scope.item.deployfailure = false;
            $scope.item.deploymessage = '';
            
            $scope.saveDeployment = function () {

                var data = {
                    azureDeployment: $scope.item.deploymentType === 'azure',
                    repo: $scope.item.deploymentType === 'azure' ? $scope.item.azurerepo : $scope.item.ftpserver,
                    username: $scope.item.userName
                };

                $http.post('/alreadyregistered', data).success(function (responsedata) {
                    if (responsedata.isValid) {
                        $scope.item.serversidevalid = true;
                        $scope.item.deploying = true;

                      $http.post('/initializedeployment', $scope.item)
                            .success(function () {
                                $scope.item.deploying = false;
                                //$location.path($location.path() + "/complete");
                                $scope.item.deploysuccess = true;
                                $scope.item.deploymessage = 'Successfully deployed your blog';
                                console.log('deployed');
                            })
                            .error(function () {
                                $scope.item.deploying = false;
                                $scope.item.deployfailure = true;
                                $scope.item.deploymessage = 'Failed to deploy your blog!';
                                console.log('error');
                            });
                    }
                    else {
                        $scope.item.serversidevalid = false;
                        for (var i = 0; i < responsedata.keys.length; i++) {
                            $scope.myForm[responsedata.keys[0]].$setValidity(responsedata.keys[0], false);
                        }
                    }
                });
            };

            $scope.test = function () {
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