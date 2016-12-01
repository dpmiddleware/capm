angular.module('CaPM').component('capmIngestList', {
    templateUrl: 'Scripts/Components/capm-ingest-list/capm-ingest-list.html',
    controller: function (ingestEventService) {
        var self = this;
        self.ingests = [];
        ingestEventService.onNewIngestEvents(null, function (event) {
            if (!self.ingests.find(id => id === event.ingestId)) {
                self.ingests.splice(0, 0, event.ingestId);
            }
        });
    }
});