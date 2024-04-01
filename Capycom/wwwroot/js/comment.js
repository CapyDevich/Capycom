const fileInput = document.getElementById('fileInput');
const fileList = document.querySelector('.file-list');

fileInput.addEventListener('change', function () {
    fileList.innerHTML = '';
    if (fileInput.files.length <= 2) {
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
    else {
        const listItem = document.createElement('div');
        listItem.classList.add('d-flex', 'justify-content-between', 'text-danger', 'ps-1', 'mb-1', 'rounded');
        listItem.innerHTML = "Можно загрузить до 2 файлов"
        const cancelBtn = document.createElement('button');
        cancelBtn.classList.add('btn', 'btn-outline-danger', 'btn-sm', 'border-0');
        cancelBtn.innerHTML = '&times;';
        cancelBtn.addEventListener('click', function () {
            listItem.remove();
        });
        fileList.appendChild(listItem);
        listItem.appendChild(cancelBtn);
    }
});

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