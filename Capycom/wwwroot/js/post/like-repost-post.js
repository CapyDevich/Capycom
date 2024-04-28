'use strict'
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