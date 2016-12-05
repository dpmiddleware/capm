angular.module('CaPM').service('preservationService', function () {
    function PreservationService() {
        var hub = $.connection.preservationSystemHub;
        var listeners = [];
        this.addNewAipListener = function(callback){
            listeners.push(callback);
        };
        hub.client.onNewAip = function (aip) {
            for(var i = 0; i < listeners.length; i++){
                listeners[i](aip);
            }
        };
    }

    return new PreservationService();
});