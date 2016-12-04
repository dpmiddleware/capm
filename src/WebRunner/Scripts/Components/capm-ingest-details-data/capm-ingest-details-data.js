angular.module('CaPM').component('capmIngestDetailsData', {
    templateUrl: 'Scripts/Components/capm-ingest-details-data/capm-ingest-details-data.html',
    controller: function (ingestEventService, $routeParams) {
        var self = this;
        self.id = $routeParams.id;
        self.events = [];
        ingestEventService.onNewIngestEvents(evt => evt.ingestId === self.id, evt => {
            //TODO: In case events come out of order we should ensure we add them to the array in order, but we're not doing that yet
            self.events.push(evt);
        });

        self.getText = function (event) {
            return JSON.stringify(event, null, 2);
        };
    }
});