'use strict'
function filltheFields() {
    $.urlParam = function (name) {
        let results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
        if (results == null) {
            return null;
        }
        return decodeURI(results[1]) || 0;
    }

    if ($.urlParam('FirstName') != 0)
        $('*[name="FirstName"]')[0].value = $.urlParam('FirstName');
    if ($.urlParam('SecondName') != 0)
        $('*[name="SecondName"]')[0].value = $.urlParam('SecondName');
    if ($.urlParam('AdditionalName') != 0)
        $('*[name="AdditionalName"]')[0].value = $.urlParam('AdditionalName');
    if ($.urlParam('CityId') != null)
        document.getElementById('city').querySelector(`option[value="${$.urlParam('CityId')}"]`).selected = true;
    if ($.urlParam('UniversityId') != null)
        document.getElementById('university').querySelector(`option[value="${$.urlParam('UniversityId')}"]`).selected = true;
    if ($.urlParam('SchoolId') != null)
        document.getElementById('school').querySelector(`option[value="${$.urlParam('SchoolId')}"]`).selected = true;
    if ($.urlParam('UserRole') != null)
        document.getElementById('role').querySelector(`option[value="${$.urlParam('UserRole')}"]`).selected = true;

    let selectpickers = $('.selectpicker');
    selectpickers.selectpicker('destroy');
    selectpickers.addClass('selectpicker');
    selectpickers.selectpicker('render');
}
$(document).ready(filltheFields);