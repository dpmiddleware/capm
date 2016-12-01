angular.module('CaPM').service('ingestEventService', function ($http) {
    function IngestEventService() {

        var ingestEvents = [];
        let ingestEventListeners = [];
        this.onNewIngestEvents = function (filter, callback) {
            ingestEventListeners.push({
                filter: filter,
                callback: callback
            });
            for (var i = 0; i < ingestEvents.length; i++) {
                if (!filter || filter(ingestEvents[i])) {
                    callback(ingestEvents[i]);
                }
            }
        };

        var hub = $.connection.ingestEventsHub;
        hub.client.onNewEvent = function (event) {
            ingestEvents.push(event);
            for (var i = 0; i < ingestEventListeners.length; i++) {
                if (!ingestEventListeners[i].filter || ingestEventListeners[i].filter(event)) {
                    ingestEventListeners[i].callback(event);
                }
            }
        };
        $.connection.hub.start().done(function () {
            hub.server.getExistingEvents();
        });
    }

    return new IngestEventService();
});