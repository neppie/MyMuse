$(function () {

         // Declare a proxy to reference the hub.
         var chatHub = $.connection.chatHub;
         registerClientMethods(chatHub);

         // Start Hub
         $.connection.hub.start().done(function () {
             registerEvents(chatHub)
         });

     });

function registerEvents(chatHub) {

    //$("#btnStartChat").click(function () {
    $(function () {
        var name = $("#txtNickName").val();
        if (name.length > 0) {
            chatHub.server.connect(name);
        }
        //else {
        //    alert("Please enter name");
        //}
    });


    $('#btnSendMsg').click(function () {

        var msg = $("#txtMessage").val();
        if (msg.length > 0) {
            var chatName = $('#hdChatName').val();
            chatHub.server.sendMessageToAll(chatName, msg);
            $("#txtMessage").val('');
        }
    });

    //$("#txtNickName").keypress(function (e) {6
    //    if (e.which == 13) {
    //        $("#btnStartChat").click();
    //    }
    //});

    $("#txtMessage").keypress(function (e) {
        if (e.which == 13) {
            $('#btnSendMsg').click();
        }
    });
}

function registerClientMethods(chatHub) {

    // Calls when user successfully logged in
    chatHub.client.onConnected = function (id, chatName, allUsers, messages) {

        $('#hdId').val(id);
        $('#hdChatName').val(chatName);
        $('#spanUser').html(chatName);

        // Add All Users
        for (i = 0; i < allUsers.length; i++) {
            AddUser(chatHub, allUsers[i].ConnectionId, allUsers[i].ChatName);
        }

        // Add Existing Messages
        for (i = 0; i < messages.length; i++) {
            AddMessage(messages[i].ChatName, messages[i].Message);
        }
    }

    // On New User Connected
    chatHub.client.onNewUserConnected = function (id, name) {
        AddUser(chatHub, id, name);
    }

    // On User Disconnected
    chatHub.client.onUserDisconnected = function (id, chatName) {
        $('#' + id).remove();

        var disc = $('<div class="disconnect">"' + chatName + '" logged off.</div>');

        $(disc).hide();
        $('#divusers').prepend(disc);
        $(disc).fadeIn(200).delay(2000).fadeOut(200);
    }

    chatHub.client.messageReceived = function (chatName, message) {
        AddMessage(chatName, message);
    }
}

function AddUser(chatHub, id, name) {
    var userId = $('#hdId').val();
    var code = "";
    if (userId == id) {
        code = $('<div class="loginUser">' + name + "</div>");
    }
    else {
        code = $('<a id="' + id + '" class="user" >' + name + '<a>');
        $(code).dblclick(function () {
            var id = $(this).attr('id');
            if (userId != id)
                OpenPrivateChatWindow(chatHub, id, name);
        });
    }
    $('#divChatUsers').append(code);
    var height = $('#divChatUsers')[0].scrollHeight;
    $('#divChatUsers').scrollTop(height);
}

function AddMessage(chatName, message) {
    $('#divChatWindow').append('<div class="message"><span class="chatName">' + chatName + '</span>: ' + message + '</div>');

    var height = $('#divChatWindow')[0].scrollHeight;
    $('#divChatWindow').scrollTop(height);
}
