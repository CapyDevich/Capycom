'use strict'

const fileInput = document.getElementById('formFile');
const fileList = document.querySelector('.errorFile');

function cancelInput(message) {
    fileInput.value = null;
    const listItem = document.createElement('div');
    listItem.classList.add('d-flex', 'justify-content-between', 'text-danger', 'ps-1', 'mb-1', 'rounded');
    listItem.innerHTML = message;
    const cancelBtn = document.createElement('button');
    cancelBtn.classList.add('btn', 'btn-outline-danger', 'btn-sm', 'border-0');
    cancelBtn.innerHTML = '&times;';
    cancelBtn.addEventListener('click', function () {
        listItem.remove();
    });
    fileList.appendChild(listItem);
    listItem.appendChild(cancelBtn);
}
function fileSizeIsMore(fileByteSize) {
    for (let i = 0; i < fileInput.files.length; i++) {
        if (fileInput.files[i].size > fileByteSize) {
            return false;
        }
    }
    return true;
}
function checkFileTypes(fileTypesArray) {
    for (let i = 0; i < fileInput.files.length; i++) {
        let isNeededType = false;
        for (let j = 0; j < fileTypesArray.length; j++) {
            if (fileInput.files[i].type == fileTypesArray[j]) {
                isNeededType = true;
                break;
            }
        }
        if (!isNeededType)
            return false;
    }
    return true;
}
fileInput.addEventListener('change', function () {
    fileList.innerHTML = '';
    if (fileInput.files.length + $('.existing-img').length - $('.delete-img').length == 1) {
        if (checkFileTypes(['image/png', 'image/gif', 'image/jpeg'])) {
            if (!fileSizeIsMore(8_388_608)) { // 8 Мб
                cancelInput('Размер файла не должен превышать 8 Мб.');
            }
        }
        else {
            cancelInput('Загружаемые файлы должны быть изображениями.');
        }
    }
    else {
        cancelInput('Можно только 1 файл.');
    }
});