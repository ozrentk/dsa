(function () {
    require(["/Scripts/require-main.js"], app);

    function app() {
        require(["jquery", "bootstrap", "knockout", "dot", "moment", "modernizr", "signalr.hubs"], function ($, bs, ko, dot, mmt, mzr, slr) {
            console.log("jquery", $);
            //console.log("bootstrap", bs);
            console.log("knockout", ko);
            console.log("dot", dot);
            console.log("moment", mmt);
            //console.log("modernizr", mzr);

            var testTxt = "Pljugicaaaaa!"

            // Test jQuery
            $('h1').html(testTxt);

            // Test moment
            console.log(mmt().format('dddd'));

            // Test doT
            var tmpl = $("#headertmpl").get(0).innerHTML;
            var tmplfn = dot.template(tmpl);

            var el = tmplfn({
                title: testTxt
            });
            $("#dot-test").html(el);

            // Test knockout
            ko.applyBindings({ test: testTxt });

            // Test bootstrap
            $('#testmodal').modal('show');

            // Test modernizr
            if (Modernizr.history) {
                console.log('DA', testTxt);
            } else {
                console.log('NE', testTxt);
            }

            // Test SignalR
            var chat = $.connection.chatHub;
            // Create a function that the hub can call back to display messages.
            chat.client.addNewMessageToPage = function (name, message) {
                console.log(name, message);
            };
            // Start the connection.
            //$.connection.hub.start().done(function () {
            //    $('#sendmessage').click(function () {
            //        // Call the Send method on the hub. 
            //        chat.server.send($('#displayname').val(), $('#message').val());
            //        // Clear text box and reset focus for next comment. 
            //        $('#message').val('').focus();
            //    });
            //});
        })
    }
})();