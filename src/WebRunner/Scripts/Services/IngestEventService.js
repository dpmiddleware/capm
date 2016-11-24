angular.module('CaPM').service('ingestEventService', function ($http) {
    function IngestEventService() {
        this.getIngests = function () {
            return $http({
                method: 'GET', url: '/api/ingestions'
            }).then(function successCallback(response) {
                return response.data;
            });
        };

        this.getIngestEvents = function (ingestId) {
            return $http({
                method: 'GET', url: '/api/ingestions/' + ingestId
            }).then(function successCallback(response) {
                return response.data;
            });
        };
    }

    return new IngestEventService();
});