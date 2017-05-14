require.config({
    baseUrl: "/Scripts",
    paths: {
        jquery: "jquery-3.1.1",
        bootstrap: "bootstrap",
        knockout: "knockout-3.4.2",
        moment: "moment",
        dot: "doT",
        modernizr: "modernizr-2.6.2",
        signalr: "jquery.signalR-2.2.1",
        "signalr.hubs": "/signalr/hubs?"
    },
    shim: {
        bootstrap: ["jquery"/*, "respond"*/],
        jquery: { exports: "$" },
        signalr: { deps: ["jquery"], exports: "$.connection" },
        "signalr.hubs": { deps: ["signalr"] }
    }
});

console.log("main configured");