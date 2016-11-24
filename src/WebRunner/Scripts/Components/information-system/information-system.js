angular.module('CaPM').component('informationSystem', {
    templateUrl: 'Scripts/Components/information-system/information-system.html',
    bindings: {
        submissionAgreementId: '='
    },
    controller: function informationSystemController($http) {
        var self = this;
        self.downloadUrl = 'http://images4.fanpop.com/image/photos/16000000/Cute-Kitten-Wallpaper-kittens-16094684-1280-800.jpg';
        self.submit = function () {
            $http({
                method: 'POST', url: '/api/ingestions', contentType: 'application/json', data: {
                    submissionAgreementId: self.submissionAgreementId,
                    ingestParameters: self.downloadUrl
                }
            }).then(function successCallback(response) {
                console.log('Ingestion sent');
            }).catch(function (response) {
                console.error(response);
            });
        };
    }
});