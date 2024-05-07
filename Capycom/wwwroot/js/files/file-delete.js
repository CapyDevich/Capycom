'use strict'

const textarea = document.getElementById('commentText');
textarea.addEventListener('input', function () {

    if (this.scrollHeight > 200)
        this.style.overflow = 'scroll';
    else {
        this.style.overflow = 'hidden';
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    }
});

function darkenImage(image) {
    const img = $(image);
    if (!img.hasClass('darken-image'))
        $("#EditPost").append(`<input type="hidden" class="delete-img" name="FilesToDelete[]" id="to-send-${image.id}" value="${image.id}">`);
    else {
        $(`#to-send-${image.id}`).remove();
        if (fileInput.files.length + $('.existing-img').length - $('.delete-img').length > 4) {
            fileList.innerHTML = '';
            fileInput.value = null;

            const listItem = document.createElement('div');
            listItem.classList.add('d-flex', 'justify-content-between', 'text-danger', 'ps-1', 'mb-1', 'rounded');
            listItem.innerHTML = "Можно загрузить до 4 файлов";
            const cancelBtn = document.createElement('button');
            cancelBtn.classList.add('btn', 'btn-outline-danger', 'btn-sm', 'border-0');
            cancelBtn.innerHTML = '&times;';
            cancelBtn.addEventListener('click', function () {
                listItem.remove();
            });
            fileList.appendChild(listItem);
            listItem.appendChild(cancelBtn);
        }
    }
    img.toggleClass('darken-image');
}