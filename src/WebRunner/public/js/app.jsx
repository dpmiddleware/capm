import React from 'react';
import ReactDOM from 'react-dom';
import { InformationSystem } from './is/information-system.jsx';
import { Middleware } from './mv/middleware.jsx';
import { PreservationSystem } from './dps/preservation-system.jsx';

(() => {
    const root = document.getElementById('app');
    ReactDOM.render((<>
        <InformationSystem />

        <Middleware />

        <PreservationSystem />
    </>
    ), root);
})();