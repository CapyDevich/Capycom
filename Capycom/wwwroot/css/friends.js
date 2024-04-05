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