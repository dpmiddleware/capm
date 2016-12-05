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

        function receivedIngestEvent(event, isInitialLoad) {
            ingestEvents.push(event);
            for (var i = 0; i < ingestEventListeners.length; i++) {
                if (!ingestEventListeners[i].filter || ingestEventListeners[i].filter(event)) {
                    ingestEventListeners[i].callback(event, Boolean(isInitialLoad));
                }
            }
        }
        var hub = $.connection.ingestEventsHub;
        hub.client.onNewEvent = function (event) {
            receivedIngestEvent(event);
        };

        //We can miss a lot of events if they occur between when we get the response and when we connect to the hub.
        //To fix this we should send some ID of the last event we received and ensure that the hub pushed the events
        //which have occured since that event before sending new incoming events.
        //We could also get the events in the wrong order if events are ongoing while we are pushing these.
        //However, in this case we don't care, since it's just a refresh of the browser away to get that data correct
        //and this view is only used for demo purposes.
        $http({ method: 'GET', url: '/api/ingestions' })
            .then(function (response) {
                for (var i = 0; i < response.data.length; i++) {
                    receivedIngestEvent(response.data[i], true);
                }
            })
            .catch(function (response){
                console.error('Failed retreiving existing ingest events', response);
            });        
    }

    return new IngestEventService();
});