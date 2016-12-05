angular.module('CaPM').component('preservationSystem', {
    templateUrl: 'Scripts/Components/preservation-system/preservation-system.html',
    controller: function (preservationService, $scope) {
        var self = this;
        self.aips = [];
        preservationService.addNewAipListener(aipId => {
            $scope.$apply(function () {
                self.aips.splice(0, 0, {
                    url: '/api/aips/' + aipId
                });
            });
        });
    }
});