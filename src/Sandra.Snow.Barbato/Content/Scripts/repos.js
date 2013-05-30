(function (app) {
    'use strict';

    app.controller(
        'ReposController', ['$scope', 'data', function ($scope, data) {
            $scope.items = data;
        }
        ]);

})(app);