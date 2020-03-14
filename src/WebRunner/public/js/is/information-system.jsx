import React from 'react';

export const InformationSystem = () => {
    const [hasLoaded, setHasLoaded] = React.useState(false);
    const [submissionAgreements, setSubmissionAgreements] = React.useState([]);
    React.useEffect(() => {
        fetch('/api/submissionagreements')
            .then(resp => resp.json())
            .then(submissionAgreementData => {
                setSubmissionAgreements(submissionAgreementData);
                setHasLoaded(true);
            });
    }, []);
    const performIngest = ({ submissionAgreementId, ingestParameter }) => {
        fetch('/api/ingestions', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                SubmissionAgreementId: submissionAgreementId,
                ingestParameters: ingestParameter
            })
        })
            .then(() => {
                console.log('Ingestion sent');

            })
            .catch(console.error);
    };
    return (
        <information-system>
            <h1>Information System</h1>
            {!hasLoaded ?
                <p>
                    Loading submission agreements, please wait...
                </p>
                :
                <form onSubmit={(event) => {
                    event.preventDefault();
                    performIngest({
                        submissionAgreementId: event.target.agreementId.value,
                        ingestParameter: event.target.data.value
                    });
                }}>
                    <label htmlFor="agreementId">
                        Submission Agreement:
                        <select name="agreementId" defaultValue={submissionAgreements[0]}>
                            {submissionAgreements.map(sa => (
                                <option key={sa} value={sa}>{sa}</option>
                            ))}
                        </select>
                    </label>
                    <label htmlFor="data">
                        Image Url to submit:
                        <input name="data" type="text" defaultValue="https://upload.wikimedia.org/wikipedia/commons/thumb/a/a5/Red_Kitten_01.jpg/120px-Red_Kitten_01.jpg" />
                    </label>
                    <button>Submit</button>
                </form>
            }
        </information-system>
    );
};