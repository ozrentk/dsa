$(document).ready(function () {

    $("#roles-table tr td button.permission").click(function (event) {
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

    $("button.delete").click(function (event) {
        var confirmation = confirm("Please confirm");
        if (confirmation) {
            var $this = $(this);
            var $form = $this.closest("form");
            var $flag = $form.children("input[name$='IsDelete']");
            $flag.val("True");
            $form.submit();
        }
    });
});