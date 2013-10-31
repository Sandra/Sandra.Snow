(function (app) {
    'use strict';

    app.controller(
        'SelectedRepoController', function ($scope, $routeParams, repoService, $http, $location) {
            $scope.item = repoService.getItem($routeParams.selectedRepo);
            $scope.item.userName = $routeParams.githubUser;
            $scope.item.deploymentType = 'git';
            $scope.item.ftpserver = '';
            $scope.item.ftppath = '';
            $scope.item.ftpusername = '';
            $scope.item.ftppassword = '';
            $scope.item.gitrepo = '';
            $scope.item.serversidevalid = true;
            $scope.item.deploying = false;
            $scope.item.deploysuccess = false;
            $scope.item.deployfailure = false;
            $scope.item.deploymessage = '';
            
            $scope.saveDeployment = function () {

                var data = {
                    azureDeployment: $scope.item.deploymentType === 'git',
                    repo: $scope.item.deploymentType === 'git' ? $scope.item.gitrepo : $scope.item.ftpserver,
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
                $scope.myForm.gitrepo.$setValidity(true);
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
                    azureDeployment: fieldName === 'gitrepo',
                    repo: fieldName === 'gitrepo' ? $scope.item.gitrepo : $scope.item.ftpserver,
                    username: $scope.item.userName
                };

                $http.post('http://localhost:12008/alreadyregistered', data).success(function (res) {
                    return callback(res);
                });
            };

            function alreadyRegistered(git) {

                var data = {
                    gitDeployment: git,
                    repo: git ? $scope.item.gitrepo : $scope.item.ftpserver,
                    username: $scope.item.userName
                };

                $http.post('http://localhost:12008/alreadyregistered', data)
                .success(function (data, status, headers, config) {
                    if (data === 'true') {
                        $scope.myForm.gitrepo.$setValidity('gitrepo', false);

                    } else {
                        $scope.myForm.gitrepo.$setValidity(true);
                    }

                })
                .error(function (data, status, headers, config) {
                    $scope.item.gitrepotaken = false;
                });
            }
        }
        );

})(app);