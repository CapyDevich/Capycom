'use strict'
function loadNews(button) {
    button.innerHTML = '<img src="../images/loading.svg" />';
    let dataToSend = {
        lastPostId: $(`.PostId`).last()[0].innerText
    };
    $.ajax({
        url: '/News/GetNextPosts',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            let feedBody = document.getElementsByClassName('feed')[0];
            if (response != '') {
                feedBody.innerHTML += response;
                button.innerHTML = 'Загрузить ещё';
            }
            else {
                button.remove();
                feedBody.innerHTML += '<p class="text-center">У наших капибар закончились новости!</p>'
            } 
        },
        error: function (obj) {
            if (obj.status == 401)
                window.location.replace("/UserLogIn");
            else
                alert(`:(\nСтатус: ${obj.status}`);
            button.innerHTML = 'Загрузить ещё';
        }
    });

}
function loadProfilePost(button) {
    button.innerHTML = '<img src="../images/loading.svg" />';
    let dataToSend = {
        userId: $(`#userID`)[0].innerText,
        lastPostId: $(`.PostId`).last()[0].innerText
    };
    $.ajax({
        url: '/User/GetNextPosts',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            let feedBody = document.getElementsByClassName('feed')[0];
            if (response != '') {
                feedBody.innerHTML += response;
                button.innerHTML = 'Загрузить ещё';
            }
            else {
                button.remove();
                feedBody.innerHTML += '<p class="text-center">У наших капибар закончились новости!</p>'
            }
        },
        error: function (obj) {
            if (obj.status == 401)
                window.location.replace("/UserLogIn");
            else
                alert(`:(\nСтатус: ${obj.status}`);
            button.innerHTML = 'Загрузить ещё';
        }
    });

}