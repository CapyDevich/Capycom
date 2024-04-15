﻿'use strict'
$(document).ready(function () {
    $('#commentForm').submit(function (e) {
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
            error: function (error) {
                console.error('Ошибка при отправке данных:', error);
            }
        });
    });
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
