$(function () {
    // initialize progress bar
    $("#progressbar").progressbar({ value: 0 });

    // initialize the connection to the server
    var progressNotifier = $.connection.progressHub;

    // client-side sendMessage function that will be called from the server-side
    progressNotifier.client.sendMessage = function (message) {
        // update progress
        UpdateProgress(message);
    }
    // establish the connection to the server and start server-side operation
    $.connection.hub.start().done(function () {
        // call the method CallLongOperation defined in the Hub
        progressNotifier.server.getStarted();
    });

progressNotifier.client.reportProgress = function (workfile, message, pct) {
    var result = $("#result");
    var resultb = $("#resultb");
    result.html(message);
    resultb = (workfile);
    $("#progressbar").progressbar("option", "value", pct);
}
progressNotifier.client.disableProgress = function () {
    $(".progressbar").progressbar("option", "disabled", true);
}
});

function UpdateProgress(message) {
    // get result div
    var result = $("#result");
    // set message
    result.html(message);
    // get progress bar - why now?
    var value = $("#progressbar").progressbar("option", "value");
    // update progress bar
    $("#progressbar").progressbar("option", "value");
}
