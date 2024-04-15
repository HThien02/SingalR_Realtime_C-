// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(() => {
    var connection = new signalR.HubConnectionBuilder().withUrl("/server").build();
    connection.start();

    connection.on("LoadPostsPage", function () {
        LoadPostsPageData();
    });

    connection.on("LoadUserPage", function () {
        LoadUsers();
    });

    function LoadPostsPageData() {
        $.ajax({
            url: "/Posts/LoadPostsByPage",
            type: "GET",
            data: {
                page: document.getElementById("currentPage").value
            },
            success: function (result) {
                document.getElementById("postContainer").innerHTML = result;
            },
            error: function (result) {
                console.log(result);
            }
        });
    }

    function LoadUsers() {
        $.ajax({
            url: "/AppUsers/LoadUsers",
            type: "GET",
            success: function (result) {
                document.getElementById("userContainer").innerHTML = result;
            },
            error: function (result) {
                console.log(result);
            }
        });
    }
})
