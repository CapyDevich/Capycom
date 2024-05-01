'use strict'
$(document).ready(function () {
    let commentForms = $('.commentForm');
    for (let i = 0; i < commentForms.length; i++) {
        commentForms[i].addEventListener('submit', function (e) {
            e.preventDefault();

            let formData = new FormData(this);
            $.ajax({
                type: 'POST',
                url: '/PostComment/AddComment',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    location.reload();
                },
                error: function (obj) {
                    if (obj.status == 401)
                        window.location.replace("/UserLogIn");
                    else
                        console.error('Ошибка при отправке данных:', obj.status);
                }
            });
        });
    }
});

let lastIDToDelete = ''
function askCommentDelete(commentID) {
    lastIDToDelete = commentID;
}
function deleteComment() {
    let dataToSend = {
        commentId: lastIDToDelete
    };
    if (lastIDToDelete != '')
        $.ajax({
            url: '/PostComment/DeleteComment',
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
                    alert(`Удалить комментарий не вышло, что-то пошло не так :(\nСтатус: ${obj.status}`);
                lastIDToDelete = ''
            }
        });

}

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
                    window.location.replace("/User");
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
