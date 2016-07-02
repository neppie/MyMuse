WL.Event.subscribe("auth.login", onLogin);
WL.init({
    client_id: "0000000040129C43",
    redirect_uri: "https://neppie.mymuse.net:44300/signin-microsoft",
    scope: "wl.photos",
    response_type: "token"
});
WL.ui({
    name: "signin",
    element: "signin"
});
function onLogin(session) {
    if (!session.error) {
        WL.api({
            path: "me",
            method: "GET"
        }).then(
            function (response) {
                document.getElementById("info").innerText =
                    "Hello, " + response.first_name + " " + response.last_name + "!";
            },
            function (responseFailed) {
                document.getElementById("info").innerText =
                    "Error calling API: " + responseFailed.error.message;
            }
        );
    }
    else {
        document.getElementById("info").innerText =
            "Error signing in: " + session.error_description;
    }
}
//WL.ui({
//    name: "skydrivepicker",
//    element: "downloadFile_div",
//    mode: "open",
//    select: "multi",
//    onselected: onDownloadFileCompleted,
//    onerror: onDownloadFileError
//});

WL.ui({
    name: "skydrivepicker",
    element: "uploadFile_div",
    mode: "save",
    onselected: onUploadFileCompleted,
    onerror: onUploadFileError
});

function onUploadFileCompleted(response) {
    WL.upload({
        path: response.data.folders[0].id,
        element: "file",
        overwrite: "rename"
    }).then(
        function (response) {
            document.getElementById("info").innerText =
                "File uploaded.";
        },
        function (responseFailed) {
            document.getElementById("info").innerText =
                "Error uploading file: " + responseFailed.error.message;
        }
    );
};

function onUploadFileError(response) {
    document.getElementById("info").innerText =
        "Error getting folder info: " + response.error.message;
}


//function onDownloadFileCompleted(response) {
//    var msg = "";
//    // For each folder selected...
//    if (response.data.folders.length > 0) {
//        for (folder = 0; folder < response.data.folders.length; folder++) {
//            // Use folder IDs to iterate through child folders and files as needed.
//            msg += "\n" + response.data.folders[folder].id;
//        }
//    }
//    // For each file selected...
//    if (response.data.files.length > 0) {
//        for (file = 0; file < response.data.files.length; file++) {
//            // Use file IDs to iterate through files as needed.
//            msg += "\n" + response.data.files[file].id;
//        }
//    }
//    document.getElementById("info").innerText =
//        "Selected folders/files:" + msg;
//};

//function onDownloadFileError(responseFailed) {
//    document.getElementById("info").innerText =
//        "Error getting folder/file info: " + responseFailed.error.message;
//}

function uploadFile_fileDialog() {
    WL.fileDialog({
        mode: "save"
    }).then(
        function (response) {
            WL.upload({
                path: response.data.folders[0].id,
                element: "file",
                overwrite: "rename"
            }).then(
                function (response) {
                    document.getElementById("info").innerText =
                        "File uploaded.";
                },
                function (responseFailed) {
                    document.getElementById("info").innerText =
                        "Error uploading file: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("info").innerText =
                "Error getting folder info: " + responseFailed.error.message;
        }
    );
}

//function downloadFile_fileDialog() {
//    WL.fileDialog({
//        mode: "open",
//        select: "multi"
//    }).then(
//        function (response) {
//            var msg = "";
//            // For each folder selected...
//            if (response.data.folders.length > 0) {
//                for (folder = 0; folder < response.data.folders.length; folder++) {
//                    // Use folder IDs to iterate through child folders and files as needed.
//                    msg += "\n" + response.data.folders[folder].id;
//                }
//            }
//            // For each file selected...
//            if (response.data.files.length > 0) {
//                for (file = 0; file < response.data.files.length; file++) {
//                    // Use file IDs to iterate through files as needed.
//                    msg += "\n" + response.data.files[file].id;
//                }
//            }
//            document.getElementById("info").innerText =
//                "Selected folders/files:" + msg;
//        },
//        function (responseFailed) {
//            document.getElementById("info").innerText =
//                "Error getting folder/file info: " + responseFailed.error.message;
//        }
//    );
//}
function uploadFile() {
    WL.login({
        scope: "wl.skydrive_update"
    }).then(
        function (response) {
            WL.upload({
                path: "folder.a6b2a7e8f2515e5e.A6B2A7E8F2515E5E!170",
                element: "file",
                overwrite: "rename"
            }).then(
                function (response) {
                    document.getElementById("info").innerText =
                        "File uploaded.";
                },
                function (responseFailed) {
                    document.getElementById("info").innerText =
                        "Error uploading file: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("info").innerText =
                "Error signing in: " + responseFailed.error.message;
        }
    );
}