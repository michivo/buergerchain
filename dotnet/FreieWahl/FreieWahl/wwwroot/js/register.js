function init() {
    const origVal = $("#sigRequest").val();
    const base64Encoded = window.btoa('@ViewData["VotingId"]');
    const newVal = origVal.replace("TOKEN_PLACEHOLDER", base64Encoded);
    $("#sigRequest").val(newVal);
    $("#buergerkarteLoader").submit();

    const startDate = formatDateTimeSeconds(@ViewData["StartDate"], false);
    const endDate = formatDateTimeSeconds(@ViewData["EndDate"], false);
    $('#fwDateInfo').html(`Start: ${startDate}
        <br />Ende: ${endDate}`);
}

init();

$(document).ready(function () {
    $('#btnSelectIdentityProofMethod').onclick(function () {
        const selectedId = ("#inputGroupSelectIdType option:selected").val();
        if (selectedId === "1") {
            $("#inputGroupSelectIdTypeattr").attr('disabled', true);
            $("#fwRegFormHandySig").show();
        }
        else if (selectedId === "2") {
            $("#inputGroupSelectIdTypeattr").attr('disabled', true);
            $("#fwRegFormSms").show();
        }

    });

    var $form = $('#fwSendSmsChallenge');
    $form.submit(function () {

        $.ajax({
            url: 'SendChallenge',
            data: {

            },
            type: 'POST',
            success: function (data) {
            },
            error: function (data) {
            }
        });
    });
});