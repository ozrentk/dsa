$(document).ready(function () {

    // Datetime pickers
    $(function () {
        var dtpOptions =
            {
                format: 'MM/DD/YYYY LT',
                extraFormats: ['DD.M.YYYY. LTS'],
                showClose: true,
                showTodayButton: true
            };
        $('#ClientTimeFrom').datetimepicker(dtpOptions)
        $('#ClientTimeTo').datetimepicker(dtpOptions);
    });

    function toggleTimeEntryType() {
        var rowTimeInDays = $("label[for=TimeInDays]").closest(".input-group");
        var rowTimeFromTo = $("label[for=ClientTimeFrom]").closest(".input-group");

        if(rowTimeInDays.hasClass('hidden')) {
            rowTimeInDays.removeClass("hidden");
            rowTimeFromTo.addClass("hidden");
            $("input[name=TimeEntryType]").val("0");
        } else {
            rowTimeFromTo.removeClass("hidden");
            rowTimeInDays.addClass("hidden");
            $("input[name=TimeEntryType]").val("1");
        }
    }

    // SWITCH TIME SELECTION (DAYS <-> FROM/TO)
    $("label[for=TimeInDays]").click(toggleTimeEntryType);
    $("label[for=ClientTimeFrom]").click(toggleTimeEntryType);

    // REMOVE COMPARISON
    $("a.compare-item").click(function (event) {
        $(this).addClass("disabled");

        var $form = $(this).closest("form")
        var compareItemNumber = $(this).data("compareitem-number");
        $form.find("input[name=ActionIsRemove]").val(true);
        $form.find("input[name=ActionItemNumber]").val(compareItemNumber);
        $form.submit();
    });

    // ADD BUSINESS
    $(".business-dropdown ul.dropdown-menu li a").click(function (event) {
        var $form = $(this).closest("form")
        var $divBizDrd = $(this).closest(".business-dropdown");

        var $divBizLnk = $divBizDrd.find("a.dropdown-toggle");
        $divBizLnk.addClass('disabled');
        $divBizLnk.html("Business: " + $(this).text() + " <span class='caret'></span>");

        var businessId = $(this).data("business-id");
        $form.find("input[name=ActionIsAdd]").val(true);
        $form.find("input[name=ActionBusinessId]").val(businessId);

        $form.submit();
    });

    // TOGGLE LINE
    $(".line-dropdown ul.dropdown-menu li a").click(function (event) {
        var $form = $(this).closest("form")
        var $divLnDrd = $(this).closest(".line-dropdown");

        var $divLnBtn = $divLnDrd.find("a.dropdown-toggle");
        $divLnBtn.addClass('disabled');
        $divLnBtn.html($(this).text() + " <span class='caret'></span>");

        var compareItemNumber = $(this).data("compareitem-number");
        var lineId = $(this).data("line-id");
        $form.find("input[name=ActionIsToggle]").val(true);
        $form.find("input[name=ActionItemNumber]").val(compareItemNumber);
        $form.find("input[name=ActionLineId]").val(lineId);

        $form.submit();
    });

    // ADD EMPLOYEE
    $(".employee-dropdown ul.dropdown-menu li a").click(function (event) {
        var $form = $(this).closest("form")
        var $divEmpDrd = $(this).closest(".employee-dropdown");

        var $divEmpBtn = $divEmpDrd.find("a.dropdown-toggle");
        $divEmpBtn.addClass('disabled');
        $divEmpBtn.html($(this).text() + " <span class='caret'></span>");

        var compareItemNumber = $(this).data("compareitem-number");
        var employeeId = $(this).data("employee-id");
        $form.find("input[name=ActionIsToggle]").val(true);
        $form.find("input[name=ActionItemNumber]").val(compareItemNumber);
        $form.find("input[name=ActionEmployeeId]").val(employeeId);

        $form.submit();
    });

});