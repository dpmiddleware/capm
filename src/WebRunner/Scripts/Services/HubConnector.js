angular.module('CaPM').service('hubConnector', function (ingestEventService, preservationService) {
    function HubConnector() {
        this.connect = function(){
            return $.connection.hub.start()
            .done(function () {
                console.log('Connected to hubs');
            })
            .fail(function (e) {
                console.error('Could not connect hubs', e);
            });
        }
    }

    return new HubConnector();
});