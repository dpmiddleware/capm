(function (app) {
    app.TestComponent =
      ng.core.Component({
          selector: 'test-component',
          template: '<h1>Test Component</h1>'
      })
      .Class({
          constructor: function () { }
      });
})(window.app || (window.app = {}));