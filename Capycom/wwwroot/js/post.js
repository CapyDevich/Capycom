'use strict'

const subscribeButton = document.getElementById('subscribeButton');
function unfollow() {
    const userID = document.getElementById('userID').innerHTML;
    let dataToSend = {
        CpcmUserId: userID,
    };
    $.ajax({
        url: '/User/Unfollow',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            if (response['status']) {
                console.log("response was successful");
                subscribeButton.innerHTML = 'Подписаться'
                subscribeButton.onclick = follow;
            }
            else {
                console.log("response was not successful");
            }

        },
        error: function (obj) {
            if (obj.status == 401)
                window.location.replace("/UserLogIn");
        }
    });
}
function follow() {
    const userID = document.getElementById('userID').innerHTML;
    let dataToSend = {
        CpcmUserId: userID,
    };
    $.ajax({
        url: '/User/Follow',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            if (response['status']) {
                console.log("response was successful");
                subscribeButton.innerHTML = 'Отписаться'
                subscribeButton.onclick = unfollow;
            }
            else {
                console.log("response was not successful");
            }

        },
        error: function (obj) {
            if (obj.status == 401)
                window.location.replace("/UserLogIn");
        }
    });
}

const friendButton = document.getElementById('friendButton');
function sendFriendRequest() {
    const userID = document.getElementById('userID').innerHTML;
    const button = document.getElementById("friendButton");
    let dataToSend = {
        CpcmUserId: userID,
    };
    $.ajax({
        url: '/User/CreateFriendRequest',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            if (response['status']) {
                button.innerHTML = 'Отозвать заявку в друзья';
                button.onclick = deleteFriendRequest;
                button.classList.remove('btn-outline-primary');
                button.classList.add('btn-primary');
            }
            else {
                console.log("response was not successful");
            }

        },
        error: function (obj) {
            if (obj.status == 401)
                window.location.replace("/UserLogIn");
        }
    });
}
function deleteFriendRequest() {
    const userID = document.getElementById('userID').innerHTML;
    const button = document.getElementById("friendButton");
    let dataToSend = {
        CpcmUserId: userID,
    };
    $.ajax({
        url: '/User/DeleteFriendRequests',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            if (response['status']) {
                button.innerHTML = 'Добавить в друзья';
                button.onclick = sendFriendRequest;
                button.classList.remove('btn-primary');
                button.classList.add('btn-outline-primary');
            }
            else {
                console.log("response was not successful");
            }

        },
        error: function (obj) {
            if (obj.status == 401)
                window.location.replace("/UserLogIn");
        }
    });
}
function asnwerFriendRequest() {
    const userID = document.getElementById('userID').innerHTML;
    const button = document.getElementById("friendButton");
    let dataToSend = {
        CpcmUserId: userID,
        status: true
    };
    $.ajax({
        url: '/User/AnswerToFriendRequests',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            if (response['status']) {
                button.innerHTML = 'Удалить из друзей';
                button.onclick = deleteFriendRequest;
            }
            else {
                console.log("response was not successful");
            }
        },
        error: function (obj) {
            if (obj.status == 401)
                window.location.replace("/UserLogIn");
        }
    });
}

let lastIDToDelete = ''
function askDelete(postID) {
    lastIDToDelete = postID;
}
function deletePost() {
    let dataToSend = {
        postGuid: lastIDToDelete
    };
    if (lastIDToDelete != '')
        $.ajax({
            url: '/User/DeletePost',
            type: 'POST',
            data: dataToSend,
            success: function (response) {
                if (response['status']) {
                    $(`#${lastIDToDelete}`).remove();
                    lastIDToDelete = ''
                }
                else {
                    alert('Удалить пост не вышло :(');
                    lastIDToDelete = ''
                }
            },
            error: function (obj) {
                if (obj.status == 401)
                    window.location.replace("/UserLogIn");
                else
                    alert(`Удалить пост не вышло, что-то пошло не так :(\nСтатус: ${obj.status}`);
                lastIDToDelete = ''
            }
        });

}