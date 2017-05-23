$(document).ready(function () {

    var businessId;
    var lineId;

    // Datetime pickers
    $(function () {
        var dtpOptions =
            {
                format: 'MM/DD/YYYY LT',
                extraFormats: ['DD.M.YYYY. LTS'],
                showClose: true,
                showTodayButton: true
            };
        $('#dtpFrom').datetimepicker(dtpOptions)
        $('#dtpTo').datetimepicker(dtpOptions);
    });

    function toggleTimeEntryType() {
        var rowTimeInDays = $("label[for=Days]").closest(".row");
        var rowTimeFromTo = $("label[for=From]").closest(".row");

        if (rowTimeInDays.hasClass('hidden')) {
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
    $("label[for=Days]").click(toggleTimeEntryType);
    $("label[for=From]").click(toggleTimeEntryType);

    $("#drdBusiness ul.dropdown-menu li a").click(function (event) {
        //console.log("drdBusiness click");

        var $divBiz = $("#drdBusiness");
        var $divBizBtn = $divBiz.find("button");

        $divBizBtn.html("Business: " + $(this).text() + " <span class='caret'></span>");
        //$divBiz.removeClass("open");

        var $btnOpenExcel = $("#btnOpenExcel");
        $btnOpenExcel.attr("disabled", "disabled");

        var $divLine = $("#drdLine");
        var $divLineBtn = $divLine.find("button");
        var $divLineUl = $divLine.find("ul.dropdown-menu");

        var $divEmployee = $("#drdEmployee");
        var $divEmployeeBtn = $divEmployee.find("button");
        var $divEmployeeUl = $divEmployee.find("ul.dropdown-menu");

        businessId = $(this).data("business-id");
        $("#BusinessId").val(businessId);
        
        $.ajax({
            url: "/Excel/Lines", 
            data: { businessId: businessId },
            success: function(markup) {
                $divLineBtn.html("Select line <span class='caret'></span>");
                $divLineBtn.removeAttr("disabled");
                $divLineUl.html("");
                $divLineUl.append(markup);
            }
        });

        $.ajax({
            url: "/Excel/Employees",
            data: { businessId: businessId },
            success: function (markup) {
                $divEmployeeBtn.html("Select employee <span class='caret'></span>");
                $divEmployeeBtn.removeAttr("disabled");
                $divEmployeeUl.html("");
                $divEmployeeUl.append(markup);
            }
        });

        //event.preventDefault();
        //return false;
    });

    $("body").delegate("#drdLine ul.dropdown-menu li a", "click", function () {
        var $divLine = $("#drdLine");
        var $divLineBtn = $divLine.find("button");
        lineId = $(this).data("line-id");
        $("#LineId").val(lineId);
        $divLineBtn.html("Line: " + $(this).text() + " <span class='caret'></span>");

        var $btnOpenExcel = $("#btnOpenExcel");
        $btnOpenExcel.removeAttr("disabled");
    });

    $("body").delegate("#drdEmployee ul.dropdown-menu li a", "click", function () {
        var $divEmployee = $("#drdEmployee");
        var $divEmployeeBtn = $divEmployee.find("button");
        employeeId = $(this).data("employee-id");
        $("#EmployeeId").val(employeeId);
        $divEmployeeBtn.html("Employee: " + $(this).text() + " <span class='caret'></span>");

        var $btnOpenExcel = $("#btnOpenExcel");
        $btnOpenExcel.removeAttr("disabled");
    });
});