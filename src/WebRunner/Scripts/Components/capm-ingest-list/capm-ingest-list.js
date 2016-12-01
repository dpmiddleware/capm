angular.module('CaPM').component('capmIngestList', {
    templateUrl: 'Scripts/Components/capm-ingest-list/capm-ingest-list.html',
    controller: function (ingestEventService) {
        var self = this;
        self.ingests = [];
        function refreshList() {
            ingestEventService.getIngests().then(function (data) {
                for (let i = 0; i < data.length; i++) {
                    if (!self.ingests.find(id => id === data[i].key)) {
                        self.ingests.splice(0, 0, data[i].key);
                    }
                }
            }).catch(function (response) {
                console.error(response);
            });
        }
        setInterval(refreshList, 2500);
        refreshList();
    }
});