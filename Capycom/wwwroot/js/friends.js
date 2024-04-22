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

		$.urlParam = function (name) {
			var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
			if (results == null) {
				return null;
			}
			return decodeURI(results[1]) || 0;
		}

		if ($.urlParam('NickName'))
			$(this).append(`<input type="hidden" name="NickName" value="${$.urlParam('NickName')}">`);
		else if ($.urlParam('UserId'))
			$(this).append(`<input type="hidden" name="UserId" value="${$.urlParam('UserId')}">`);
			
		this.submit();
	});
});