angular.module('CaPM').component('preservationSystem', {
    templateUrl: 'Scripts/Components/preservation-system/preservation-system.html',
    controller: function ($http) {
        var self = this;
        self.aips = [];
        var highestAipIdIncluded = -1;
        function refresh() {
            $http({ method: 'GET', url: '/api/aips', contentType: 'application/json' })
                .then(function successCallback(response) {
                    for (var i = highestAipIdIncluded + 1; i < response.data.length; i++) {
                        var aipId = response.data[i];
                        self.aips.push({
                            url: '/api/aips/' + aipId
                        });
                        highestAipIdIncluded = aipId;
                    }
                }).catch(function (response) {
                    console.error(response);
                });
        }
        refresh();
        setInterval(refresh, 5000);
    }
});