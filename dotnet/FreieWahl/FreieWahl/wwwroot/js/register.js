function initRegistrations(votingId, registrationId, startDateRaw, endDateRaw) {
    const origVal = $("#sigRequest").val();
    const base64Encoded = window.btoa(votingId);
    const newVal = origVal.replace("TOKEN_PLACEHOLDER", base64Encoded);
    $("#sigRequest").val(newVal);
    $("#buergerkarteLoader").submit();

    const startDate = formatDateTimeSeconds(startDateRaw, false);
    const endDate = formatDateTimeSeconds(endDateRaw, false);
    $('#fwDateInfo').html(`Start: ${startDate}<br />Ende: ${endDate}`);

    $(document).ready(function() {
        $('#btnSelectIdentityProofMethod').click(function() {
            const selectedId = $("#inputGroupSelectIdType option:selected").val();
            if (selectedId === "1") {
                $("#inputGroupSelectIdTypeattr").attr('disabled', true);
                $("#fwRegFormHandySig").show();
            } else if (selectedId === "2") {
                $("#inputGroupSelectIdTypeattr").attr('disabled', true);
                $("#fwRegFormSms").show();
            }

        });

        $('#smsRegSendButton').click(function() {
            $('#smsRegInputName').prop('disabled', true);
            $('#smsRegInputPhone').prop('disabled', true);
            $('#smsRegSendButton').addClass('d-none');
            $.ajax({
                url: '../Registration/SendSmsChallenge',
                data: {
                    votingId: votingId,
                    registrationId: registrationId,
                    name: $('#smsRegInputName').val(),
                    phone: $('#smsRegInputPhone').val()
                },
                type: 'POST',
                success: function (data) {
                    $('#smsRegSendButton').show();
                    $('#smsRegVerificationCodeEntry').removeClass('d-none');
                    $('#smsRegVerifyButton').show();
                },
                error: function(data) {
                }
            });
        });

        $('#smsRegVerifyButton').click(function () {
            $('#smsRegVerificationCode').prop('disabled', true);
            $('#smsRegVerifyButton').prop('disabled', true);

            $.ajax({
                url: '../Registration/VerifySmsChallenge',
                data: {
                    challengeResponse: $('#smsRegVerificationCode').val(),
                    registrationId: registrationId
                },
                type: 'POST',
                success: function (data) {
                    window.location.replace(`../Registration/RegistrationDetails?regUid=${registrationId}`);
                },
                error: function (data) {
                    window.location.replace(`../Registration/RegistrationError?votingId=${votingId}&reason=smsVerificationFailed`);
                }
            });
        });
    });
}