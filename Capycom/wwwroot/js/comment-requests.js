'use strict'
$(document).ready(function () {
    $('#commentForm').submit(function (e) {
        e.preventDefault();

        let formData = new FormData(this);
        console.log(formData);
        $.ajax({
            type: 'POST',
            url: '/PostComment/AddComment',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                console.log('Данные успешно отправлены!');
                console.log(response);
            },
            error: function (error) {
                console.error('Ошибка при отправке данных:', error);
            }
        });
    });
});