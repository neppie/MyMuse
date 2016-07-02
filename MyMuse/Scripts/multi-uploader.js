$(document).ready(function () {
          $("#fupload1").click(function (evt) {
              var files = $("#file1").get(0).files;
              if (files.length > 0) {
                  var data = new FormData();
                  for (i = 0; i < files.length; i++) {
                      data.append("file" + i, files[i]);
                  }
                  $.ajax({
                      type: "POST",
                      url: "/api/fileupload",
                      contentType: false,
                      processData: false,
                      data: data,
                      success: function (messages) {
                          for (i = 0; i < messages.length; i++) {
                              alert(messages[i]);
                          }
                      },
                      error: function () {
                          alert("Error while provoking the Web API");
                      }
                  });
              }
          });
      });