'use strict'
function banComment(url, banID) {
    let dataToSend = {
        commentId: banID
    };
    $.ajax({
        url: url,
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            if (response['status']) {
                let banIco = $(`#${banID} .ban-icon`);
                if (banIco.css('color') == 'rgb(25, 135, 84)') { // ~ green
                    banIco.css('color', '#dc3545'); // ~ red
                    $(`#${banID} .comment-text`).first().html('<i class="m-1 pb-1">Комментарий был разбанен.<br>Чтобы увидеть его содержимое, перезагрузите страницу.</i>')
                }
                else {
                    banIco.css('color', '#198754'); // ~ green
                    $(`#${banID} .comment-text`).first().html('<i class="m-1 pb-1">Комментарий забаннен :(</i>')
                }
            }
            else {
                alert('Забанить не вышло :(');
            }
        },
        error: function (obj) {
            if (obj.status == 401)
                window.location.replace("/UserLogIn");
            else
                alert(`Забанить не вышло, что-то пошло не так :(\nСтатус: ${obj.status}`);
        }
    });
}