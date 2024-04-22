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
                alert(response['status']);
                let banIco = document.getElementById('banID');
                if (banIco.style['color'] == '#198754') // ~ green
                    banIco.style['color'] == '#dc3545' // ~ red
                else
                    banIco.style['color'] == '#198754'
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