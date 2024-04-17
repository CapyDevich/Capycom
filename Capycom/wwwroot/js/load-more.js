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
            let postBody = document.getElementsByClassName('col-lg-8')[0];
            if (response != '') {
                postBody.innerHTML += response;
                button.innerHTML = 'Загрузить ещё';
            }
            else {
                button.remove();
                postBody.innerHTML += '<p class="text-center">У наших капибар закончились новости!</p>'
            } 
        },
        error: function (obj) {
            if (obj.status == 401)
                window.location.replace("/UserLogIn");
            else
                alert(`:(\nСтатус: ${obj.status}`);
        }
    });

}