import React from 'react';
import { ingestEventService } from './IngestEventService.js';
import { debounce } from './debounce.js';

export const CapmIngestDetails = ({ ingestId }) => {
    const [events, setEvents] = React.useState(ingestEventService.getIngestProcessDetails(ingestId));
    React.useEffect(() => {
        const debouncedRefresh = debounce(() => setEvents(ingestEventService.getIngestProcessDetails(ingestId)), 100);
        ingestEventService.addIngestProcessEventReceivedListener(ingestId, () => {
            debouncedRefresh();
        });
    }, []);

    return (
        <ol>
            {
                events.map(e => <pre key={e.ingestId + "/" + e.order}>{JSON.stringify(e, null, 2)}</pre>)
            }
        </ol>
    );
};