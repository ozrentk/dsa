$(document).ready(function () {

    $("#btnAddLineBusiness").click(function (event) {
        console.log("AddLineBusiness");
        $("#AddLineBusiness").val("true");
    });

    $("#btnRemoveLineBusinesses").click(function (event) {
        console.log("RemoveLineBusinesses");
        $("#RemoveLineBusinesses").val("true");
    });

    $("#btnSaveConfig").click(function (event) {
        console.log("SaveConfig");
        $("#SaveConfig").val("true");
    });

});