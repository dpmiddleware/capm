angular.module('CaPM').component('ingestStep', {
    templateUrl: 'Scripts/Components/ingest-step/ingest-step.html',
    bindings: {
        step: '='
    },
    controller: function () {
        var self = this;

        self.isProcessing = Boolean(this.step.startedAt && !this.step.wasSuccessful && !this.step.wasFailed);
    }
});