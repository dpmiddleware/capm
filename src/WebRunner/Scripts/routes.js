angular.module('CaPM').config(function ($routeProvider) {

    $routeProvider
        .when('/', {
            template: '<capm-ingest-list></capm-ingest-list>'
        })
        .when('/:id', {
            template: '<capm-ingest-details-data></capm-ingest-details-data>'
        })
        .otherwise('/');
});