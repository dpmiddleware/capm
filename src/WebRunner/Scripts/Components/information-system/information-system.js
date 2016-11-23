angular.module('CaPM').component('informationSystem', {
    templateUrl: 'Scripts/Components/information-system/information-system.html',
    bindings: {
        submissionAgreementId: '='
    },
    controller: function informationSystemController($http) {
        this.submit = function () {
            $http({
                method: 'POST', url: '/api/ingestions', contentType: 'application/json', data: {
                    SubmissionAgreementId: this.submissionAgreementId
                }
            }).then(function successCallback(response) {
                console.log('Ingestion sent');
            }).catch(function (response) {
                console.error(response);
            });
        };
    }
});