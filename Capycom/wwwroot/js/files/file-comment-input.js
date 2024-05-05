'use strict'
function cancelInput(message, fileInput, fileList) {
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
function succesInput(fileInput, fileList) {
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
function fileSizeIsMore(fileByteSize, fileInput) {
    for (let i = 0; i < fileInput.files.length; i++) {
        if (fileInput.files[i].size > fileByteSize) {
            return false;
        }
    }
    return true;
}
function checkFileTypes(fileTypesArray, fileInput) {
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

const commentTextareas = document.getElementsByClassName('comment-text');
for (let i = 0; i < commentTextareas.length; i++) {
    commentTextareas[i].addEventListener('input', function () {

        if (this.scrollHeight > 200)
            this.style.overflow = 'scroll';
        else {
            this.style.overflow = 'hidden';
            this.style.height = 'auto';
            this.style.height = (this.scrollHeight) + 'px';
        }
    });
}

function fileLoad(inputF) {
    const fileList = inputF.form.getElementsByClassName('file-list')[0];
    fileList.innerHTML = '';
    if (inputF.files.length + $('.existing-img').length - $('.delete-img').length <= 2) {
        if (checkFileTypes(['image/png', 'image/gif', 'image/jpeg'], inputF)) {
            if (fileSizeIsMore(8_388_608, inputF)) { // 8 Мб
                succesInput(inputF, fileList);
            }
            else {
                cancelInput('Размер файла не должен превышать 8 Мб.', inputF, fileList);
            }
        }
        else {
            cancelInput('Загружаемые файлы должны быть изображениями.', inputF, fileList);
        }
    }
    else {
        cancelInput('Можно загрузить до 2 файлов', inputF, fileList);
    }
};