angular.module('CaPM').component('capmIngestDetails', {
    templateUrl: 'Scripts/Components/capm-ingest-details/capm-ingest-details.html',
    controller: function (ingestEventService, $routeParams) {
        var self = this;
        self.id = $routeParams.id;
        self.events = [];
        self.couldNotGetDetails = false;
        self.stringify = function (event) { return JSON.stringify(event, null, 2); };
        ingestEventService.getIngestEvents(this.id).then(function (data) {
            self.events = data;
        }).catch(function (error) {
            console.error(error);
            self.couldNotGetDetails = true;
        });
    }
});