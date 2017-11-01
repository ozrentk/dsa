$(document).ready(function () {

    $("#user-table tr td button.role").click(function (event) {
        var $this = $(this);
        var $flag = $this.siblings("input[name$='IsSelected']");
        if ($flag.val() === "True") {
            $this.removeClass("btn-primary");
            $this.addClass("btn-default");
            $flag.val("False");
        } else {
            $flag.val("True");
            $this.removeClass("btn-default");
            $this.addClass("btn-primary");
        }
    });
});