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
		return "";
}

let classesNames = ['likes-count', 'comments-count', 'repost-count'];
for (let i = 0; i < classesNames.length; i++) {
	let elements = document.getElementsByClassName(classesNames[i]);
	for (let j = 0; j < elements.length; j++)
		elements[j].innerText = shortenNumber(elements[j].innerText);
}