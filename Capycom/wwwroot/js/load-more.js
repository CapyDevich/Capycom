'use strict'
function loadNews(button) {
    button.innerHTML = '<img src="../images/loading.svg" />';
    if ($(`.PostId`).length != 0) {
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
    else {
        button.remove();
        document.getElementsByClassName('feed')[0].innerHTML += '<p class="text-center">У наших капибар нет новостей!</p>'
    }
}
function loadProfilePost(button) {
    if ($(`.PostId`).length != 0) {
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
    else {
        button.remove();
        document.getElementsByClassName('feed')[0].innerHTML += '<p class="text-center">У наших капибар нет новостей!</p>'
    }
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
            FirstName: $.urlParam('FirstName') == 0 ? null : $.urlParam('FirstName'),
            SecondName: $.urlParam('SecondName') == 0 ? null : $.urlParam('SecondName'),
            AdditionalName: $.urlParam('AdditionalName') == 0 ? null : $.urlParam('AdditionalName'),
            CityId: $.urlParam('CityId'),
            UniversityId: $.urlParam('UniversityId'),
            SchoolId: $.urlParam('SchoolId'),
            NickName: $.urlParam('NickName'),
            UserId: $.urlParam('UserId'),
            lastId: $('.follower').last()[0].id
        };
        $.ajax({
            url: '/User/GetNextFollowers',
            type: 'POST',
            data: dataToSend,
            success: function (response) {
                let followersContainer = document.getElementById('followers-container');
                if (response != '') {
                    followersContainer.innerHTML += response;
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
function loadFriends(button) {
    if ($('.friend').length > 0) {
        button.innerHTML = '<img src="../images/loading.svg" />';
        $.urlParam = function (name) {
            let results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
            if (results == null) {
                return null;
            }
            return decodeURI(results[1]) || 0;
        }
        let dataToSend = {
            FirstName: $.urlParam('FirstName') == 0 ? null : $.urlParam('FirstName'),
            SecondName: $.urlParam('SecondName') == 0 ? null : $.urlParam('SecondName'),
            AdditionalName: $.urlParam('AdditionalName') == 0 ? null : $.urlParam('AdditionalName'),
            CityId: $.urlParam('CityId'),
            UniversityId: $.urlParam('UniversityId'),
            SchoolId: $.urlParam('SchoolId'),
            NickName: $.urlParam('NickName'),
            UserId: $.urlParam('UserId'),
            lastId: $('.friend').last()[0].id
        };
        $.ajax({
            url: '/User/GetNextFriends',
            type: 'POST',
            data: dataToSend,
            success: function (response) {
                let friendsContainer = document.getElementById('friends-container');
                if (response != '') {
                    friendsContainer.innerHTML += response;
                    button.innerHTML = 'Загрузить ещё';
                }
                else {
                    button.remove();
                    document.getElementById('friends-footer').innerHTML += '<p class="text-center">Больше c этим пользователем никто не дружит<br/><span class="text-muted">(ну, кроме капибар, конечно)</span></p>'
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
function loadNextUser(button) {
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
            FirstName: $.urlParam('FirstName') == 0 ? null : $.urlParam('FirstName'),
            SecondName: $.urlParam('SecondName') == 0 ? null : $.urlParam('SecondName'),
            AdditionalName: $.urlParam('AdditionalName') == 0 ? null : $.urlParam('AdditionalName'),
            CityId: $.urlParam('CityId'),
            UniversityId: $.urlParam('UniversityId'),
            SchoolId: $.urlParam('SchoolId'),
            NickName: $.urlParam('NickName'),
            lastId: $('.follower').last()[0].id
        };
        $.ajax({
            url: '/User/FindNextUser',
            type: 'GET',
            data: dataToSend,
            success: function (response) {
                let friendsContainer = document.getElementById('cpcm-users-container');
                if (response != '') {
                    friendsContainer.innerHTML += response;
                    button.innerHTML = 'Загрузить ещё';
                }
                else {
                    button.remove();
                    document.getElementById('cpcm-users-footer').innerHTML += '<p class="text-center">Больше никого не нашлось<br/><span class="text-muted">(ну, кроме капибар, конечно)</span></p>'
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
function loadNextFriendRequest(button) {
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
            FirstName: $.urlParam('FirstName') == 0 ? null : $.urlParam('FirstName'),
            SecondName: $.urlParam('SecondName') == 0 ? null : $.urlParam('SecondName'),
            AdditionalName: $.urlParam('AdditionalName') == 0 ? null : $.urlParam('AdditionalName'),
            CityId: $.urlParam('CityId'),
            UniversityId: $.urlParam('UniversityId'),
            SchoolId: $.urlParam('SchoolId'),
            NickName: $.urlParam('NickName'),
            lastId: $('.follower').last()[0].id
        };
        $.ajax({
            url: '/User/GetNextFriendRequests',
            type: 'GET',
            data: dataToSend,
            success: function (response) {
                let friendsContainer = document.getElementById('cpcm-users-container');
                if (response != '') {
                    friendsContainer.innerHTML += response;
                    button.innerHTML = 'Загрузить ещё';
                }
                else {
                    button.remove();
                    document.getElementById('cpcm-users-footer').innerHTML += '<p class="text-center">Больше никого не нашлось<br/><span class="text-muted">(ну, кроме капибар, конечно)</span></p>'
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
    else {
        button.remove();
        document.getElementById('cpcm-users-footer').innerHTML += '<p class="text-center">У вас нет заявок в друзья!<br/><span class="text-muted">(не расстраивайтесь, капибары всегда с вами!)</span></p>'
    }
}
function loadComments(button) {
    button.innerHTML = '<img src="../images/loading.svg" />';
    const id = $('.comments-body .comment').filter(function () {
        return $(this).parents('.comment').length === 0;
    }).last()[0].attributes['id'].value;

    let dataToSend = {
        postId: $(`.PostId`)[0].innerText,
        lastCommentId: id
    };
    $.ajax({
        url: '/PostComment/GetNextComments',
        type: 'POST',
        data: dataToSend,
        success: function (response) {
            let feedBody = $('.comments-body')[0];
            if (response != '') {
                feedBody.innerHTML += response;
                button.innerHTML = 'Загрузить ещё';
            }
            else {
                button.remove();
                feedBody.innerHTML += '<p class="text-center">У остальных капибар без комментариев!</p>'
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