///<reference path="../typings/jquery/jquery.d.ts" />
///<reference path="../typings/knockout/knockout.d.ts" />
///<reference path="../TypeLite.Net4.d.ts" />

class LoginRequestViewModel {
    Username: KnockoutObservable<string>
    Password: KnockoutObservable<string>

    constructor(viewModel: DSA.WEB.Models.LoginRequest) {
        this.Username = ko.observable(viewModel.Username);
        this.Password = ko.observable(viewModel.Password);
    }

    loginRequest() {
        $.ajax({
            type: "POST",
            url: "/api/authenticate/login",
            data: ko.toJSON(this),
            contentType: "application/json"
        }).done(this.loginSuccess).fail(this.loginFail);

        return false; // don't submit anything
    }

    loginSuccess(data: DSA.WEB.Models.LoginResponse, status: string) {
        console.log(status, data.Jwt);
    }

    loginFail(istrazi: any) {
        console.log(istrazi);
    }
}

ko.applyBindings(new LoginRequestViewModel({ Username: "", Password: "" }));

