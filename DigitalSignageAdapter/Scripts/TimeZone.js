function sendOffset(returnUrl) 
{
    var d = new Date();
    var offset = -d.getTimezoneOffset() / 60;

    $.ajax({
        url: "/TimeZone/RefreshOffset",
        method: "POST",
        data: { Offset: offset, ReturnUrl: returnUrl },
        success: function () {
            window.location.href = returnUrl;
        }
    });
}