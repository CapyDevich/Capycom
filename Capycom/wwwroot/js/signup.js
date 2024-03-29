'use strict'
function addOptionToSelectpicker(selectElement, value, text) {
	$(selectElement).selectpicker("destroy");
	$(selectElement).append(`<option value="${value}">${text}</option>`);
	$(selectElement).addClass('selectpicker').selectpicker("render");
};

function sendNewCityName() {
	$.ajax({
		url: '/UserSignUp/AddCity',
		type: 'POST',
		data: { newCity: $('#addingCityInput').val() },
		success: function (response) {
			if (response['success']) {
				addOptionToSelectpicker('#city', response['id'], $('#addingCityInput').val());
				$('#addingCityModal').modal('hide');
				$('#addingCityModalAlert').hide();
			}
			else {
				$('#addingSchoolModalAlert').empty();
				$('#addingCityModalAlert').append(response['message']);
				$('#addingCityModalAlert').show();
			}

		},
		error: function (obj) {
			$('#addingCityModalAlert').empty();
			$('#addingCityModalAlert').append('Неизвестная ошибка. Попробуйте позже.');
			$('#addingCityModalAlert').show();
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
				addOptionToSelectpicker('#university', response['id'], $('#addingUniversityInput').val());
				$('#addingUniversityModal').modal('hide');
				$('#addingUniversityModalAlert').hide();
			}
			else {
				$('#addingSchoolModalAlert').empty();
				$('#addingUniversityModalAlert').append(response['message']);
				$('#addingUniversityModalAlert').show();
			}

		},
		error: function (obj) {
			$('#addingUniversityModalAlert').empty();
			$('#addingUniversityModalAlert').append('Неизвестная ошибка. Попробуйте позже.');
			$('#addingUniversityModalAlert').show();
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
				addOptionToSelectpicker('#school', response['id'], $('#addingSchoolInput').val());
				$('#addingSchoolModal').modal('hide');
				$('#addingSchoolModalAlert').hide();
			}
			else {
				$('#addingSchoolModalAlert').empty();
				$('#addingSchoolModalAlert').append(response['message']);
				$('#addingSchoolModalAlert').show();
			}

		},
		error: function (obj) {
			$('#addingSchoolModalAlert').empty();
			$('#addingSchoolModalAlert').append('Неизвестная ошибка. Попробуйте позже.');
			$('#addingSchoolModalAlert').show();
		}
	});
}