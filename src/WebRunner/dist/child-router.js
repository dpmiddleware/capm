define(["require", "exports"], function (require, exports) {
    "use strict";
    class ChildRouter {
        constructor() {
            this.heading = 'Child Router';
        }
        configureRouter(config, router) {
            config.map([
                { route: ['', 'welcome'], name: 'welcome', moduleId: 'welcome', nav: true, title: 'Welcome' },
                { route: 'users', name: 'users', moduleId: 'users', nav: true, title: 'Github Users' },
                { route: 'child-router', name: 'child-router', moduleId: 'child-router', nav: true, title: 'Child Router' }
            ]);
            this.router = router;
        }
    }
    exports.ChildRouter = ChildRouter;
});

//# sourceMappingURL=child-router.js.map
