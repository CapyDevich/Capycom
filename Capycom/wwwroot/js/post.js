"use strict";
function shortenNumber(number) {
	if (number < 1_000) {
		return number;
	}
	else if (number < 1_000_000) {
		number = Math.floor(number / 1_000);
		return number + "K";
	}
	else if (number < 1_000_000_000) {
		number = Math.floor(number / 1_000_000);
		return number + "M";
	}
	else
		return number;
}

function likePost(button, postId) {
	let icon = button.querySelectorAll('img')[0];
	let likeCount = button.querySelectorAll('.likes-count')[0];
	$.ajax({
		url: '/PostComment/AddRemoveLike',
		type: 'POST',
		data: { postID: postId },
		success: function (response) {
			if (response['status']) {
				if (icon.src.match('tangerine_black')) {
					icon.src = icon.src.replace('black', 'colored');
					likeCount.innerText = Number(likeCount.innerText) + 1;
					renderPostButtons();
				}
				else if (icon.src.match('tangerine_colored')) {
					icon.src = icon.src.replace('colored', 'black');
					likeCount.innerText = Number(likeCount.innerText) - 1;
					renderPostButtons();
				}
			}
			else {
				console.log("response was not successful");
			}

		},
		error: function (obj) {
			if (obj.status == 401)
				window.location.replace("/UserLogIn");
		}
	});
}

function renderPostButtons() {
	let classesNames = ['likes-count', 'comments-count', 'repost-count'];
	for (let i = 0; i < classesNames.length; i++) {
		let elements = document.getElementsByClassName(classesNames[i]);
		let toShowElements = document.getElementsByClassName(classesNames[i] + "-show");
		for (let j = 0; j < elements.length; j++)
			toShowElements[j].innerText = shortenNumber(elements[j].innerText);
	}
}

renderPostButtons();