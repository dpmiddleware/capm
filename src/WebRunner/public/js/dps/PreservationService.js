function PreservationService() {
    const listeners = [];
    this.addNewAipListener = function (callback) {
        listeners.push(callback);
    };
    const hub = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/dps")
        .configureLogging(signalR.LogLevel.Information)
        .build();
    hub.on('onNewAip', aip => {
        console.log('Received aip', aip);
        for (let i = 0; i < listeners.length; i++) {
            listeners[i]({ url: '/api/aips/' + aip });
        }
    });
    hub.start().then(a => {
        console.log('Connected to dps hub');
    }).catch(err => console.error("Failed connecting to dps hub", err));
}

export const preservationService = new PreservationService();