'use strict'
function ban(url, banID) {
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
                    $(`#${banID}`).children().first().children().eq(1).html('<div class="card mb-2"><div class="card-body"><div class="d-flex align-items-center">Пост был разбанен.<br/>Чтобы увидеть его содержимое, перезагрузите страницу.</div></div></div>')
                }
                else {
                    banIco.css('color', '#198754'); // ~ green
                    $(`#${banID}`).children().first().children().eq(1).html('<div class="card mb-2"><div class="card-body"><div class="d-flex align-items-center">Пост был забаннен :(</div></div></div>')
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