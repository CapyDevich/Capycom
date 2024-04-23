'use strict'
function banPost(url, banID) {
    let dataToSend = {
        id: banID
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
                    $(`#${banID}`).children().first().children().eq(1).html('<i>Пост был разбанен.<br/>Чтобы увидеть его содержимое, перезагрузите страницу.</i>')
                }
                else {
                    banIco.css('color', '#198754'); // ~ green
                    $(`#${banID}`).children().first().children().eq(1).html('<i>Пост был забаннен :(</i>')
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
                    //banIco.css('color', '#dc3545'); // ~ red
                    $(`#${banID}`).html(`<div class="d-flex"><i class="m-1 pb-1">Комментарий был разбанен.<br>Чтобы увидеть его содержимое, перезагрузите страницу.</i><div class="ms-auto mb-auto me-1 d-flex" style="flex-direction:column"><a class="ban-icon-link" onclick="banComment(&quot;/PostComment/BanUnbanComment&quot;,&quot;${banID}&quot;)"><svg class="ban-icon" style="color:#dc3545" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-ban" viewBox="0 0 16 16"><path d="M15 8a6.97 6.97 0 0 0-1.71-4.584l-9.874 9.875A7 7 0 0 0 15 8M2.71 12.584l9.874-9.875a7 7 0 0 0-9.874 9.874ZM16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0"/></svg></a></div></div>`)
                }
                else {
                    //banIco.css('color', '#198754'); // ~ green
                    $(`#${banID}`).html(`<div class="d-flex"><i class="m-1 pb-1">Комментарий забаннен :(</i><div class="ms-auto mb-auto me-1 d-flex" style="flex-direction:column"><a class="ban-icon-link" onclick="banComment(&quot;/PostComment/BanUnbanComment&quot;,&quot;${banID}&quot;)"><svg class="ban-icon" style="color:#dc3545" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-ban" viewBox="0 0 16 16"><path d="M15 8a6.97 6.97 0 0 0-1.71-4.584l-9.874 9.875A7 7 0 0 0 15 8M2.71 12.584l9.874-9.875a7 7 0 0 0-9.874 9.874ZM16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0"/></svg></a></div></div>`)
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