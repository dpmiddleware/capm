(function () {
    var applyTimeoutHandle = null;
    angular.module('CaPM').component('capmIngestList', {
        templateUrl: 'Scripts/Components/capm-ingest-list/capm-ingest-list.html',
        controller: function (ingestEventService, $scope) {
            var self = this;
            self.ingests = [];
            ingestEventService.onNewIngestEvents(null, function (event, isInitialLoading) {
                if (!self.ingests.find(id => id === event.ingestId)) {
                    self.ingests.splice(0, 0, event.ingestId);
                }
                if (applyTimeoutHandle) {
                    clearTimeout(applyTimeoutHandle);
                }
                //Delay digest for as few milliseconds, in case this is the initial loading since
                //in that case we will get loads of these calls within short period
                applyTimeoutHandle = setTimeout(function () { $scope.$apply() }, 50);
            });
        }
    });
})();