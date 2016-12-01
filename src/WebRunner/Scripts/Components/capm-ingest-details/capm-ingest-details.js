angular.module('CaPM').component('capmIngestDetails', {
    templateUrl: 'Scripts/Components/capm-ingest-details/capm-ingest-details.html',
    bindings: {
        id: '='
    },
    controller: function (ingestEventService, $routeParams) {
        var self = this;
        self.plans = [];
        self.couldNotGetDetails = false;
        ingestEventService.getIngestEvents(this.id).then(function (data) {
            function getEvent(eventType, componentExecutionIdOrPredicate){
                var predicate = typeof(componentExecutionIdOrPredicate) === 'string' ? e => e.componentExecutionId === componentExecutionIdOrPredicate : componentExecutionIdOrPredicate;
                return data.find(e => new RegExp(eventType, 'i').test(e.$type) && predicate(e));
            }
            self.plans = data.map(function (event) {
                if (/IngestPlanSet/i.test(event.$type)) {
                    return {
                        steps: event.ingestPlan.map(function (step) {
                            var startEvent = getEvent('IngestComponentWorkStartRequested|IngestComponentCompensationStartRequested', function (e) { return e.commandSent.componentExecutionId === step.componentExecutionId; });
                            return {
                                title: /^Component\./i.test(step.componentCode) ? step.componentCode.substring('Component.'.length) : step.componentCode,
                                startedAt: startEvent ? startEvent.timestamp: null,
                                wasSuccessful: getEvent('IngestComponentWorkCompleted', step.componentExecutionId) != null,
                                wasFailed: getEvent('IngestComponentWorkFailed', step.componentExecutionId) != null,
                                wasCompensationStep: step.isCompensatingComponent
                            };
                        })
                    };
                }
            }).filter(function (plan) { return plan; });
            if (self.plans.length > 0) {
                self.plans[0].isCompensationPlan = false;
            }
            if (self.plans.length > 1) {
                self.plans[1].isCompensationPlan = true;
            }
            self.hasCompensationPlan = self.plans.length > 1;
            var completionEvent = data.find(event => /IngestCompleted/i.test(event.$type));
            self.completionStep = {
                title: 'Completed',
                startedAt: completionEvent ? completionEvent.timestamp : null,
                wasSuccessful: Boolean(completionEvent),
                wasFailed: false,
                wasCompensationStep: self.hasCompensationPlan
            };
            var startedEvent = data.find(event => /IngestStarted/i.test(event.$type));
            self.startStep = {
                title: 'Started',
                startedAt: startedEvent ? startedEvent.timestamp : null,
                wasSuccessful: Boolean(startedEvent),
                wasFailed: false,
                wasCompensationStep: false
            };
        }).catch(function (error) {
            console.error(error);
            self.couldNotGetDetails = true;
        });
    }
});