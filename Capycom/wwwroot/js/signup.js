'use strict'
function sendNewCityName() {
	$.ajax({
		url: '/UserSignUp/AddCity',
		type: 'POST',
		data: { newCity: $('#addingCityInput').val() },
		success: function (response) {
			if (response["success"]) {
				console.log('Успешно: ', response);
			}
			else {
				console.log('NO: ', response);
			}

		},
		error: function (obj) {
			console.log("Unknown error");
		}
	});
}
function sendNewUniversityName() {
	$.ajax({
		url: '/UserSignUp/AddUniversities',
		type: 'POST',
		data: { newUni: $('#addingUniversityInput').val() },
		success: function (response) {
			if (response["success"]) {
				console.log('Успешно: ', response);
			}
			else {
				console.log('NO: ', response);
			}

		},
		error: function (obj) {
			console.log("Unknown error");
		}
	});
}
function sendNewSchoolName() {
	$.ajax({
		url: '/UserSignUp/AddSchool',
		type: 'POST',
		data: { newSchool: $('#addingSchoolInput').val() },
		success: function (response) {
			if (response["success"]) {
				console.log('Успешно: ', response);
			}
			else {
				console.log('NO: ', response);
			}

		},
		error: function (obj) {
			console.log("Unknown error");
		}
	});
}