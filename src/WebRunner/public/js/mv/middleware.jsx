import { ingestEventService } from "./IngestEventService.js";
import { CapmGraph } from "./capm-graph.jsx";
import React from "react";
import { debounce } from "./debounce.js";
import { CapmIngestDetails } from "./capm-ingest-details.jsx";

const ingestDetailsRegex = /#\/ingests\/([^\/]+)/;
export const Middleware = () => {
    const [processIds, setProcessIds] = React.useState(ingestEventService.getIngestProcessIds());
    const [locationHash, setLocationHash] = React.useState(window.location.hash);
    React.useEffect(() => {
        const debouncedRefresh = debounce(() => setProcessIds(ingestEventService.getIngestProcessIds()), 100);
        ingestEventService.addIngestStartedListener(() => {
            debouncedRefresh();
        });
        window.addEventListener('hashchange', function () {
            setLocationHash(window.location.hash);
        }, false);
    }, []);
    if (ingestDetailsRegex.test(locationHash)) {
        const ingestId = ingestDetailsRegex.exec(locationHash)[1];
        if (processIds.indexOf(ingestId) !== -1) {
            return (
                <div className="capm-system">
                    <h1>{ingestId}</h1>
                    <a href="#/">Back to ingest list</a>
                    <CapmIngestDetails ingestId={ingestId} />
                    <a href="#/">Back to ingest list</a>
                </div>
            );
        }
    }
    return (
        <div className="capm-system">
            <h1>capm-ingest-list</h1>
            <ol>
                {processIds.map(ingestId => (
                    <li key={ingestId}>
                        <a href={'#/ingests/' + ingestId}><h2>{ingestId}</h2></a>
                        <CapmGraph ingestId={ingestId} />
                    </li>
                ))}
            </ol>
        </div>
    );
};