angular.module('CaPM').config(function ($routeProvider) {

    $routeProvider
        .when('/', {
            template: '<capm-ingest-list></capm-ingest-list>'
        })
        .when('/:id', {
            template: '<capm-ingest-details ingestId="id"></capm-ingest-details>'
        })
        .otherwise('list');
});