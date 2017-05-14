$(document).ready(function () {

    $("#lnkLineIsDelete").click(function (event) {
        console.log("lnkLineIsDelete");
        $("input[name='IsDelete']")
            .val("true")
            .closest("form")
            .submit();
    });

    $("#Guid").click(function (event) {
        $(this).select();
    });


});