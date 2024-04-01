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