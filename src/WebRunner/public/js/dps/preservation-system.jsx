import React from 'react';
import { preservationService } from './PreservationService.js';

export const PreservationSystem = () => {
    const [aips, setAips] = React.useState([]);
    React.useEffect(() =>
        preservationService.addNewAipListener(aip => {
            aips.splice(0, 0, aip);
            const newArray = aips.slice();
            setAips(newArray);
        }),
        []);

    return (
        <preservation-system>
            <h1>Preservation System</h1>

            <ul>
                {aips.map((aip, index) => (
                    <li key={aips.length - index}>
                        <img width="130" src={aip.url} />
                    </li>
                ))}
            </ul>
        </preservation-system>
    );
};