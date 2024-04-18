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
function loadFollowers(button) {
    if ($('.follower').length > 0) {
        button.innerHTML = '<img src="../images/loading.svg" />';
        $.urlParam = function (name) {
            let results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
            if (results == null) {
                return null;
            }
            return decodeURI(results[1]) || 0;
        }
        let dataToSend = {
            FirstName: $.urlParam('FirstName'),
            SecondName: $.urlParam('SecondName'),
            AdditionalName: $.urlParam('AdditionalName') == 0 ? null : $.urlParam('AdditionalName'),
            CityId: $.urlParam('CityId'),
            UniversityId: $.urlParam('UniversityId'),
            SchoolId: $.urlParam('SchoolId'),
            NickName: $.urlParam('NickName'),
            lastId: $('.follower').last()[0].id
        };
        console.log(dataToSend);
        $.ajax({
            url: '/User/GetNextFollowers',
            type: 'POST',
            data: dataToSend,
            success: function (response) {
                let feedBody = document.getElementById('followers-container')[0];
                if (response != '') {
                    feedBody.innerHTML += response;
                    button.innerHTML = 'Загрузить ещё';
                }
                else {
                    button.remove();
                    document.getElementById('followers-footer').innerHTML += '<p class="text-center">Больше за этим пользователем никто не наблюдает<br/><span class="text-muted">(или нет?🤔)</span></p>'
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
}