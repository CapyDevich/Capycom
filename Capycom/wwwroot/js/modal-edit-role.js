function clearSelect(selectID) {
	$(`#${selectID}`).val('');
	$(`#${selectID}`).selectpicker("destroy");
	$(`#${selectID}`).addClass('selectpicker').selectpicker("render");
}
$(document).ready(function () {
    $('#editUserRole').on('submit', function (event) {
        event.preventDefault();
        let dataToSend = {
            userId: this.querySelector('[name=userId]').value,
            roleId: this.querySelector('[name=roleId]').value
        };
        $.ajax({
            type: 'POST',
            url: '/Roles/EditUserRole',
            data: dataToSend,
            success: function (response) {
                if (response['status']) {
                    bootstrap.Modal.getInstance(document.getElementById('editRoleModal')).hide();
                    $('#editRoleModalAlert').hide();
                    document.getElementById('editUserRole').querySelector('[name=userId]').value = '';
                    clearSelect('role');
                }
                else {
                    $('#editRoleModalAlert').text('Ошибка. ' + response['message']);
                    $('#editRoleModalAlert').show();
                }
            },
            error: function (obj) {
                if (obj.status == 401)
                    window.location.replace("/UserLogIn");
                else {
                    console.log(obj);
                    console.log(obj.status);
                    console.log(obj.responseJSON['message']);
                    $('#editRoleModalAlert').text(obj.responseJSON['message']);
                    $('#editRoleModalAlert').show();
                }
                lastIDToDelete = ''
            }
        });
    });
});