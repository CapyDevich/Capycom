'use strict'
function shortenNumber(number) {
    if (number < 1_000) {
        return number;
    }
    else if (number < 1_000_000) {
        number = Math.floor(number / 1_000);
        return number + "K";
    }
    else if (number < 1_000_000_000) {
        number = Math.floor(number / 1_000_000);
        return number + "M";
    }
    else
        return number;
}

function likePost(button, postId) {
    let icon = button.querySelectorAll('img')[0];
    let likeCount = button.querySelectorAll('.likes-count')[0];
    $.ajax({
        url: '/PostComment/AddRemoveLike',
        type: 'POST',
        data: { postID: postId },
        success: function (response) {
            if (response['status']) {
                if (icon.src.match('tangerine_black')) {
                    icon.src = icon.src.replace('black', 'colored');
                    likeCount.innerText = Number(likeCount.innerText) + 1;
                    renderPostButtons();
                }
                else if (icon.src.match('tangerine_colored')) {
                    icon.src = icon.src.replace('colored', 'black');
                    likeCount.innerText = Number(likeCount.innerText) - 1;
                    renderPostButtons();
                }
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

let repostCountElement;
let postID;
function repost(button, postId) {
    console.log("repost");
    repostCountElement = button.querySelectorAll('.repost-count')[0];
    postID = postId;
    $('#repostSend').click(function () {
        let inputText = $('#repostInput').val();

        let dataToSend = {
            PostFatherId: postId,
            Text: inputText
        };
        $.ajax({
            url: '/User/CreatePostP',
            type: 'POST',
            data: dataToSend,
            success: function (response) {
                if (response['status']) {
                    repostCountElement.innerText = Number(repostCountElement.innerText) + 1;
                    renderPostButtons();
                    postID = "";
                    inputText = "";
                    document.getElementById('repostInput').value = "";
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
    });
}

function renderPostButtons() {
    let classesNames = ['likes-count', 'repost-count'];
    for (let i = 0; i < classesNames.length; i++) {
        let elements = document.getElementsByClassName(classesNames[i]);
        let toShowElements = document.getElementsByClassName(classesNames[i] + "-show");
        for (let j = 0; j < elements.length; j++)
            toShowElements[j].innerText = shortenNumber(elements[j].innerText);
    }
}

renderPostButtons();

const textarea = document.getElementById('repostInput');
textarea.addEventListener('input', function () {

    if (this.scrollHeight > 200)
        this.style.overflow = 'scroll';
    else {
        this.style.overflow = 'hidden';
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    }
});

const cancelButton = document.getElementById('cancelButton');
cancelButton.addEventListener('click', function () {
    textarea.value = "";
});

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
    let dataToSend = {
        CpcmUserId: userID,
    };
    $.ajax({
        url: '/User/CreateFriendRequest',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            if (response['status']) {
                console.log("response was successful");
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
    let dataToSend = {
        CpcmUserId: userID,
    };
    $.ajax({
        url: '/User/DeleteFriendRequests',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            if (response['status']) {
                console.log("response was successful");
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
function asnwerFriendRequest(accepted) {
    const userID = document.getElementById('userID').innerHTML;
    let dataToSend = {
        CpcmUserId: userID,
        status: accepted
    };
    $.ajax({
        url: '/User/AnswerToFriendRequests',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            if (response['status']) {
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