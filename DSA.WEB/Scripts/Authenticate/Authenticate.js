///<reference path="../typings/jquery/jquery.d.ts" />
///<reference path="../typings/knockout/knockout.d.ts" />
///<reference path="../TypeLite.Net4.d.ts" />
var LoginRequestViewModel = (function () {
    function LoginRequestViewModel(viewModel) {
        this.Username = ko.observable(viewModel.Username);
        this.Password = ko.observable(viewModel.Password);
    }
    LoginRequestViewModel.prototype.loginRequest = function () {
        $.ajax({
            type: "POST",
            url: "/api/authenticate/login",
            data: ko.toJSON(this),
            contentType: "application/json"
        }).done(this.loginSuccess).fail(this.loginFail);
        return false; // don't submit anything
    };
    LoginRequestViewModel.prototype.loginSuccess = function (data, status) {
        console.log(status, data.Jwt);
    };
    LoginRequestViewModel.prototype.loginFail = function (istrazi) {
        console.log(istrazi);
    };
    return LoginRequestViewModel;
}());
ko.applyBindings(new LoginRequestViewModel({ Username: "", Password: "" }));
//# sourceMappingURL=Authenticate.js.map