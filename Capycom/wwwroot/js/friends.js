'use strict'
function asnwerFriendRequest(userID, accepted) {
	let dataToSend = {
		CpcmUserId: userID,
		status: accepted
	};
	$.ajax({
		url: '/User/AnswerToFriendRequests',
		type: 'POST',
		data: dataToSend,
		success: function (response) {
			if (response['status']) {
				console.log("response was successful");
				document.getElementById(userID).remove();
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
$(document).ready(function () {
	$('#searchUser').submit(function (event) {
		event.preventDefault();

		let dataToSend = new FormData(this);
		for (let pair of dataToSend.entries()) {
			console.log(pair[0] + ', ' + pair[1]);
		}

		$.urlParam = function (name) {
			var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
			if (results == null) {
				return null;
			}
			return decodeURI(results[1]) || 0;
		}

		if ($.urlParam('NickName'))
			dataToSend.append('NickName', $.urlParam('NickName'));
		else if ($.urlParam('UserId'))
			dataToSend.append('UserId', $.urlParam('UserId'));
			
		dataToSend.append('hello', 'привет');

		$.ajax({
			url: '/User/Followers',
			type: 'POST',
			data: dataToSend,
			processData: false,
			contentType: false,
			success: function (response) {
				console.log(response);

			},
			error: function (obj) {
				if (obj.status == 401)
					window.location.replace("/UserLogIn");
			}
		});
	});
});