import signalR from '@microsoft/signalr';

function IngestEventService() {
    const ingestProcesses = {};
    const ingestStartListeners = [];
    const ingestProcessListeners = {};

    this.getIngestProcessIds = () => Object.keys(ingestProcesses)
        .sort((ingest1, ingest2) => {
            return ingestProcesses[ingest2][0].timestamp.localeCompare(ingestProcesses[ingest1][0].timestamp);
        });
    this.getIngestProcessDetails = (ingestId) =>
        ingestId in ingestProcesses ?
            ingestProcesses[ingestId]
                .slice()
                .sort((event1, event2) => {
                    return event1.timestamp.localeCompare(event2.timestamp);
                })
            : [];
    this.addIngestStartedListener = (callback) => {
        ingestStartListeners.push(callback);
        return () => {
            if (ingestStartListeners.indexOf(callback) != -1) {
                ingestStartListeners.splice(ingestStartListeners.indexOf(callback), 1)
            };
        };
    };
    this.addIngestProcessEventReceivedListener = (ingestId, callback) => {
        if (ingestProcessListeners[ingestId] === undefined) {
            ingestProcessListeners[ingestId] = [callback]
        }
        else {
            ingestProcessListeners[ingestId].push(callback);
        }
        return () => {
            if (ingestProcessListeners[ingestId] !== undefined) {
                const index = ingestProcessListeners[ingestId].indexOf(callback);
                if (index !== -1) {
                    ingestProcessListeners[ingestId].splice(index, 1);
                    if (ingestProcessListeners[ingestId].length === 0) {
                        delete (ingestProcessListeners[ingestId]);
                    }
                }
            }
        };
    }

    function receivedIngestEvent(event) {
        const ingestId = event.ingestId;
        if (ingestProcesses[ingestId] === undefined) {
            ingestProcesses[ingestId] = [event];
            ingestStartListeners.forEach(listener => listener(ingestId, event));
        }
        else {
            ingestProcesses[ingestId].push(event);
            if (ingestProcessListeners[ingestId] !== undefined) {
                ingestProcessListeners[ingestId].forEach(listener => listener(ingestId, ingestProcesses[ingestId]));
            }
        }
    }
    const hub = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/ingest")
        .configureLogging(signalR.LogLevel.Information)
        .build();
    hub.on('onNewEvent', event => {
        console.log('Received ingest event', event);
        receivedIngestEvent(event);
    });
    hub.start().then(a => {
        console.log('Connected to ingest hub');
    }).catch(err => console.error("Failed connecting to ingest hub", err));

    //We can miss a lot of events if they occur between when we get the response and when we connect to the hub.
    //To fix this we should send some ID of the last event we received and ensure that the hub pushed the events
    //which have occured since that event before sending new incoming events.
    //We could also get the events in the wrong order if events are ongoing while we are pushing these.
    //However, in this case we don't care, since it's just a refresh of the browser away to get that data correct
    //and this view is only used for demo purposes.
    fetch('/api/ingestions')
        .then(resp => resp.json())
        .then(data => {
            for (let i = 0; i < data.length; i++) {
                receivedIngestEvent(data[i], true);
            }
        })
        .catch(function (response) {
            console.error('Failed retreiving existing ingest events', response);
        });
}

export const ingestEventService = new IngestEventService();