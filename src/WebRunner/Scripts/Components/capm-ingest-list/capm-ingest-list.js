angular.module('CaPM').component('capmIngestList', {
    templateUrl: 'Scripts/Components/capm-ingest-list/capm-ingest-list.html',
    controller: function (ingestEventService) {
        var self = this;
        self.ingests = [];
        function refreshList() {
            ingestEventService.getIngests().then(function (data) {
                self.ingests = data;
            }).catch(function (response) {
                console.error(response);
            });
        }
        setInterval(refreshList, 2500);
        refreshList();
    }
});