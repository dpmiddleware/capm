angular.module('CaPM').config(function ($routeProvider) {

    $routeProvider
        .when('/', {
            template: '<capm-ingest-list></capm-ingest-list>'
        })
        .otherwise('list');
});