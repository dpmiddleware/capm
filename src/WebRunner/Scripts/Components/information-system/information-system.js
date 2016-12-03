angular.module('CaPM').component('informationSystem', {
    templateUrl: 'Scripts/Components/information-system/information-system.html',
    controller: function informationSystemController($http) {
        var self = this;
        self.submissionAgreements = [];
        self.selectedSubmissionAgreement = null;
        self.downloadUrl = 'http://images4.fanpop.com/image/photos/16000000/Cute-Kitten-Wallpaper-kittens-16094684-1280-800.jpg';
        self.submit = function () {
            if (self.selectedSubmissionAgreement) {
                $http({
                    method: 'POST', url: '/api/ingestions', contentType: 'application/json', data: {
                        submissionAgreementId: self.selectedSubmissionAgreement,
                        ingestParameters: self.downloadUrl
                    }
                }).then(function successCallback(response) {
                    console.log('Ingestion sent');
                }).catch(function (response) {
                    console.error(response);
                });
            }
        };

        $http({ method: 'GET', url: '/SubmissionAgreements.json' })
            .then(function successCallback(response) {
                self.submissionAgreements = response.data;
                self.selectedSubmissionAgreement = self.submissionAgreements.length > 0 ? self.submissionAgreements[0].SubmissionAgreementId : null;
            }).catch(function errorHandler(response) {
                console.error(response);
            });
    }
});