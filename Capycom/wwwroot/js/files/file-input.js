'use strict'

const fileInput = document.getElementById('fileInput');
const fileList = document.querySelector('.file-list');

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

function succesInput() {
    Array.from(fileInput.files).forEach(file => {
        const listItem = document.createElement('div');
        listItem.classList.add('d-flex', 'justify-content-between', 'align-items-center');
        const fileName = document.createTextNode(file.name);
        listItem.appendChild(fileName);
        const cancelBtn = document.createElement('button');
        cancelBtn.classList.add('btn', 'btn-outline-danger', 'btn-sm', 'border-0');
        cancelBtn.innerHTML = '&times;';
        cancelBtn.addEventListener('click', function () {
            listItem.remove();
            fileInput.value = null;
        });
        listItem.appendChild(cancelBtn);
        fileList.appendChild(listItem);
    });
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
    if (fileInput.files.length + $('.existing-img').length - $('.delete-img').length <= 4) {
        if (checkFileTypes(['image/png', 'image/gif', 'image/jpeg'])) {
            if (fileSizeIsMore(8_388_608)) { // 8 Мб
                succesInput();
            }
            else {
                cancelInput('Размер файла не должен превышать 8 Мб.');
            }
        }
        else {
            cancelInput('Загружаемые файлы должны быть изображениями.');
        }
    }
    else {
        cancelInput('Можно загрузить до 4 файлов');
    }
});