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
        console.log("drdBusiness click");

        var $divBiz = $("#drdBusiness");
        var $divBizBtn = $divBiz.find("button");

        $divBizBtn.html("Business: " + $(this).text() + " <span class='caret'></span>");
        //$divBiz.removeClass("open");

        var $btnOpenExcel = $("#btnOpenExcel");
        $btnOpenExcel.attr("disabled", "disabled");

        var $divLine = $("#drdLine");
        var $divLineBtn = $divLine.find("button");
        var $divLineUl = $divLine.find("ul.dropdown-menu");

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

        //event.preventDefault();
        //return false;
    });

    $("body").delegate("#drdLine ul.dropdown-menu li a", "click", function () {
        console.log("drdLine click");

        var $divLine = $("#drdLine");
        var $divLineBtn = $divLine.find("button");

        lineId = $(this).data("line-id");
        $("#LineId").val(lineId);

        $divLineBtn.html("Line: " + $(this).text() + " <span class='caret'></span>");

        var $btnOpenExcel = $("#btnOpenExcel");
        $btnOpenExcel.removeAttr("disabled");

        //$divBiz.removeClass("open");

        //event.preventDefault();
        //return false;
    });

    //$("#btnOpenExcel").click(function (event) {
    //    console.log("btnOpenExcel click");

    //    window.location.href = "/Excel/ReportFile?lineId=" + lineId + "&businessId=" + businessId + "&days=" + $("#txtDays").val();
    //});

});