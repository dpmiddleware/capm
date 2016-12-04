angular.module('CaPM').component('preservationSystem', {
    templateUrl: 'Scripts/Components/preservation-system/preservation-system.html',
    controller: function ($http) {
        var self = this;
        self.aips = [];
        function refresh() {
            $http({ method: 'GET', url: '/api/aips', contentType: 'application/json' })
                .then(function successCallback(response) {
                    for (var i = 0; i < response.data.length; i++) {
                        var url = '/api/aips/' + response.data[i];
                        if (!self.aips.find(aip => aip.url === url)) {
                            self.aips.push({
                                url: url
                            });
                        }
                    }
                }).catch(function (response) {
                    console.error(response);
                });
        }
        refresh();
        setInterval(refresh, 2500);
    }
});