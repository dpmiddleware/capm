angular.module('CaPM').component('capmIngestDetails', {
    templateUrl: 'Scripts/Components/capm-ingest-details/capm-ingest-details.html',
    bindings: {
        id: '='
    },
    controller: function (ingestEventService) {
        var self = this;
        self.ingestPlan = [];
        self.compensationPlan = [];
        var hasEventsChangedSinceLastPlanRead = false;
        var events = [];
        function gatherEvents(gatherFunc, eventType, componentExecutionIdOrPredicate){
            if (!componentExecutionIdOrPredicate){componentExecutionIdOrPredicate = () => true;}
            var predicate = typeof (componentExecutionIdOrPredicate) === 'string' ? e => e.componentExecutionId === componentExecutionIdOrPredicate : componentExecutionIdOrPredicate;
            var regexp = new RegExp(eventType, 'i');
            return gatherFunc.call(events, e => regexp.test(e.$type) && predicate(e));
        }
        function getEvents(eventType, componentExecutionIdOrPredicate) {
            return gatherEvents(events.filter, eventType, componentExecutionIdOrPredicate);
        }
        function getEvent(eventType, componentExecutionIdOrPredicate) {
            return gatherEvents(events.find, eventType, componentExecutionIdOrPredicate);
        }
        self.hasFailed = function () {
            return Boolean(getEvent('IngestComponentWorkFailed'));
        };
        function readPlans() {
            var plans = getEvents('IngestPlanSet');
            //The first plan (given that the order is correct, which we should be able to assume) should be the ingest plan
            var ingestPlan = plans.length > 0 ? plans[0] : null;
            //The second plan, if any, should be the compensation plan
            var compensatingPlan = plans.length > 1 ? plans[1] : null;
            function createStepViewModels(plan) {
                if (!plan) {
                    return { steps: [] };
                }
                return {
                    steps: (plan.ingestPlan || []).map(function (step) {
                        var startEvent = getEvent('IngestComponentWorkStartRequested|IngestComponentCompensationStartRequested', function (e) { return e.commandSent.componentExecutionId === step.componentExecutionId; });
                        return {
                            title: /^Component\./i.test(step.componentCode) ? step.componentCode.substring('Component.'.length) : step.componentCode,
                            startedAt: startEvent ? startEvent.timestamp : null,
                            wasSuccessful: getEvent('IngestComponentWorkCompleted', step.componentExecutionId) != null,
                            wasFailed: getEvent('IngestComponentWorkFailed', step.componentExecutionId) != null,
                            wasCompensationStep: step.isCompensatingComponent
                        };
                    })
                };
            }
            self.ingestPlan = createStepViewModels(ingestPlan);
            self.compensationPlan = createStepViewModels(compensatingPlan);

            if (self.ingestPlan.steps.length > 0 || self.compensationPlan.steps.length > 0) {
                var completionEvent = getEvent('IngestCompleted');
                var completionStep = {
                    title: 'Completed',
                    startedAt: completionEvent ? completionEvent.timestamp : null,
                    wasSuccessful: Boolean(completionEvent),
                    wasFailed: false,
                    wasCompensationStep: self.hasCompensationPlan
                };
                if (self.hasFailed()) {
                    self.compensationPlan.steps.push(completionStep);
                }
                else {
                    self.ingestPlan.steps.push(completionStep);
                }
            }
            self.compensationPlan.steps.reverse();

            var startedEvent = getEvent('IngestStarted');
            var startStep = {
                title: 'Started',
                startedAt: startedEvent ? startedEvent.timestamp : null,
                wasSuccessful: Boolean(startedEvent),
                wasFailed: false,
                wasCompensationStep: false
            };
            self.ingestPlan.steps.splice(0, 0, startStep);
            hasEventsChangedSinceLastPlanRead = false;
        }
        ingestEventService.onNewIngestEvents(evt => evt.ingestId === self.id, evt => {
            //TODO: In case events come out of order we should ensure we add them to the array in order, but we're not doing that yet
            events.push(evt);
            hasEventsChangedSinceLastPlanRead = true;
        });

        self.getIngestPlan = function () {
            if (hasEventsChangedSinceLastPlanRead) {
                readPlans();
            }
            return self.ingestPlan;
        }

        self.getCompensationPlan = function () {
            if (hasEventsChangedSinceLastPlanRead) {
                readPlans();
            }
            return self.compensationPlan;
        }
    }
});