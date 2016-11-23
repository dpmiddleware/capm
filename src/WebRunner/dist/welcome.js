define(["require", "exports"], function (require, exports) {
    "use strict";
    class Welcome {
        constructor() {
            this.heading = 'Welcome to the Aurelia Navigation App!';
            this.firstName = 'John';
            this.lastName = 'Doe';
            this.previousValue = this.fullName;
        }
        get fullName() {
            return `${this.firstName} ${this.lastName}`;
        }
        submit() {
            this.previousValue = this.fullName;
            alert(`Welcome, ${this.fullName}!`);
        }
        canDeactivate() {
            if (this.fullName !== this.previousValue) {
                return confirm('Are you sure you want to leave?');
            }
        }
    }
    exports.Welcome = Welcome;
    class UpperValueConverter {
        toView(value) {
            return value && value.toUpperCase();
        }
    }
    exports.UpperValueConverter = UpperValueConverter;
});

//# sourceMappingURL=welcome.js.map
