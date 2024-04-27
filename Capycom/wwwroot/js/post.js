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

        if (inputText != '') {
            if (inputText.length <= 300) {
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
                            document.getElementById('repostAlert').style.display = 'none';
                            repostCountElement.innerText = Number(repostCountElement.innerText) + 1;
                            renderPostButtons();
                            postID = "";
                            inputText = "";
                            document.getElementById('repostInput').value = "";
                            bootstrap.Modal.getInstance(document.getElementById('repostModal')).hide();
                        }
                        else {
                            let alertUserField = document.getElementById('repostAlert');
                            alertUserField.innerHTML = 'Не получилось создать репост.';
                            alertUserField.style.display = '';
                        }

                    },
                    error: function (obj) {
                        if (obj.status == 401)
                            window.location.replace("/UserLogIn");
                        else {
                            alert('Случилось, что случилось...\nСтатус ответа: ' + obj.status);
                        }
                    }
                });
            }
            else {
                let alertUserField = document.getElementById('repostAlert');
                alertUserField.innerHTML = 'Репост должен содержать не более 300 символов.<br/>Кол-во символов у вас: ' + inputText.length;
                alertUserField.style.display = '';
            }
        }
        else {
            let alertUserField = document.getElementById('repostAlert');
            alertUserField.innerHTML = 'Нельзя создать репост без текста.';
            alertUserField.style.display = '';
        }
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