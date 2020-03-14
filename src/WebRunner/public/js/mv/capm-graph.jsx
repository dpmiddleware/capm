import { ingestEventService } from "./IngestEventService.js";
import React from 'react';
import { debounce } from "./debounce.js";
import { CapmGraphStep } from "./capm-graph-step.jsx";

export const CapmGraph = ({ ingestId }) => {
    const [events, setEvents] = React.useState(ingestEventService.getIngestProcessDetails(ingestId));
    React.useEffect(() => {
        const debouncedRefresh = debounce(() => setEvents(ingestEventService.getIngestProcessDetails(ingestId)), 100);
        ingestEventService.addIngestProcessEventReceivedListener(ingestId, () => {
            debouncedRefresh();
        });
    }, []);

    const gatherEvents = (gatherFunc, eventType, componentExecutionIdOrPredicate) => {
        if (!componentExecutionIdOrPredicate) { componentExecutionIdOrPredicate = () => true; }
        var predicate = typeof componentExecutionIdOrPredicate === 'string' ? e => e.componentExecutionId === componentExecutionIdOrPredicate : componentExecutionIdOrPredicate;
        var regexp = new RegExp(eventType, 'i');
        return gatherFunc.call(events, e => regexp.test(e.eventType) && predicate(e));
    };
    const getEvents = (eventType, componentExecutionIdOrPredicate) => {
        return gatherEvents(events.filter, eventType, componentExecutionIdOrPredicate);
    };
    const getEvent = (eventType, componentExecutionIdOrPredicate) => {
        return gatherEvents(events.find, eventType, componentExecutionIdOrPredicate);
    };
    const readPlans = () => {
        const plans = getEvents('IngestPlanSet');
        //The first plan (given that the order is correct, which we should be able to assume) should be the ingest plan
        const ingestPlanEvent = plans.length > 0 ? plans[0] : null;
        //The second plan, if any, should be the compensation plan
        const compensatingPlanEvent = plans.length > 1 ? plans[1] : null;
        function createStepViewModels(plan) {
            if (!plan) {
                return { steps: [] };
            }
            return {
                steps: (plan.ingestPlan || []).map(function (step) {
                    const startEvent = getEvent('IngestComponentWorkStartRequested|IngestComponentCompensationStartRequested', e => e.commandSent.componentExecutionId === step.componentExecutionId);
                    return {
                        id: step.componentExecutionId,
                        title: /^Component\./i.test(step.componentCode) ? step.componentCode.substring('Component.'.length) : step.componentCode,
                        startedAt: startEvent ? startEvent.timestamp : null,
                        wasSuccessful: getEvent('IngestComponentWorkCompleted', step.componentExecutionId) != null,
                        wasFailed: getEvent('IngestComponentWorkFailed', step.componentExecutionId) != null,
                        wasCompensationStep: step.isCompensatingComponent
                    };
                })
            };
        }
        const ingestPlan = createStepViewModels(ingestPlanEvent);
        const compensationPlan = createStepViewModels(compensatingPlanEvent);
        const hasIngestFailed = Boolean(getEvent('IngestComponentWorkFailed'));

        if (ingestPlan.steps.length > 0 || compensationPlan.steps.length > 0) {
            const completionEvent = getEvent('IngestCompleted');
            const completionStep = {
                id: "completionStepId",
                title: 'Completed',
                startedAt: completionEvent ? completionEvent.timestamp : null,
                wasSuccessful: Boolean(completionEvent),
                wasFailed: false,
                wasCompensationStep: hasIngestFailed
            };
            if (hasIngestFailed) {
                compensationPlan.steps.push(completionStep);
            }
            else {
                ingestPlan.steps.push(completionStep);
            }
        }
        compensationPlan.steps.reverse();

        const startedEvent = getEvent('IngestStarted');
        const startStep = {
            id: "startstepid",
            title: 'Started',
            startedAt: startedEvent ? startedEvent.timestamp : null,
            wasSuccessful: Boolean(startedEvent),
            wasFailed: false,
            wasCompensationStep: false
        };
        ingestPlan.steps.splice(0, 0, startStep);

        const numberOfSuccessfulComponents = ingestPlan ? ingestPlan.steps.reduce((acc, step) => step.wasSuccessful ? acc + 1 : acc, 0) : 0;

        return {
            ingestPlan,
            compensationPlan,
            numberOfSuccessfulComponents
        }
    };
    const plans = readPlans();
    return (
        <table className="ingest-details-table">
            <tbody>
                <tr>
                    {
                        plans.ingestPlan.steps.map((step, index) => (
                            <React.Fragment key={step.id}>
                                <td>
                                    <CapmGraphStep step={step} />
                                </td>
                                {index < plans.ingestPlan.steps.length - 1 ?
                                    <td>
                                        <span className={step.wasSuccessful ? 'arrow-right' : 'arrow-right inactive-path'}>&gt;</span>
                                    </td> : null
                                }
                            </React.Fragment>
                        ))
                    }
                </tr>
                {
                    plans.compensationPlan && plans.compensationPlan.steps && plans.compensationPlan.steps.length ? <>

                        <tr>
                            <td colSpan={plans.numberOfSuccessfulComponents * 2}>
                                &nbsp;
                            </td>
                            <td>
                                <span className="arrow-down">&gt;</span>
                            </td>
                        </tr>

                        <tr>
                            {
                                plans.compensationPlan.steps.map((step, index) => (
                                    <React.Fragment key={step.id}>
                                        <td>
                                            <CapmGraphStep step={step} />
                                        </td>
                                        {index < plans.compensationPlan.steps.length - 1 ?
                                            <td>
                                                <span className={step.wasSuccessful ? 'arrow-left' : 'arrow-left inactive-path'}>&gt;</span>
                                            </td> : null
                                        }
                                    </React.Fragment>
                                ))
                            }
                        </tr>
                    </> : null
                }
            </tbody>
        </table>
    );
};