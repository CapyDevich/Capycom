function answerComment(commentID, button) {
    $(button).hide();
    $(`#${commentID} .commentCard`).last().show();
}