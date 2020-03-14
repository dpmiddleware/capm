export const CapmGraphStep = ({ step }) => {
    return (
        <div className={'ingest-step ' + (step.wasSuccessful ? 'succeeded' : step.wasFailed ? 'failed' : step.startedAt ? 'processing' : 'not-started')}>
            <span>{step.title}</span>

            {step.wasCompensationStep ? <em>Compensating</em> : null}

            {step.startedAt ?
                <span style={{ "fontSize": "x-small" }}>{step.startedAt.substring(0, 19).replace('T', ' ')}</span>
                : null
            }
        </div>
    );
};