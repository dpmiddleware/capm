(function (app) {
    document.addEventListener('DOMContentLoaded', function () {
        ng.platformBrowserDynamic.bootstrap(app.TestComponent);
    });
})(window.app || (window.app = {}));