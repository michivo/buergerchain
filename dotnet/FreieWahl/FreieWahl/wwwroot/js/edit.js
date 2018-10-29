﻿//-----------------------------------
// Module Edit
//-----------------------------------
const Edit = (function () {
    let mVotingId;
    let mGrantedRegistrationCount;
    let mQuestions;

    var setQuestions = function(questions) {
        mQuestions = questions;
    }

    var updateQuestions = function () {
        $('#questionList').load(`QuestionList?id=${mVotingId}`);
    }

    var resetNewQuestionModal = function () {
        $('#newQuestionTitle').val('');
        $('#newQuestionDescription').val('');
        $('#maxNoAnswers').val('1');
        $("#answersTable .answerOption").not('.hide').each(function () {
            $(this).detach();
        });
        $('#modalQuestionOk').text('OK');
        $('#modalQuestionOk').removeClass('disabled');
    }

    var deleteQuestion = function (questionNumber) {
        $.ajax({
            url: 'DeleteVotingQuestion',
            data: { "id": mVotingId, "qid": questionNumber },
            type: 'POST',
            datatype: 'json',
            success: function (data) {
                updateQuestions(mVotingId);
            } // TODO error
        });
    }

    var saveQuestion = function (idx) {
        $('#modalQuestionOk').text('Speichere...');
        $('#modalQuestionOk').addClass('disabled');
        const description = $('#newQuestionDescription').val();
        const title = $('#newQuestionTitle').val();
        const type = parseInt($('#questionType').val());
        const answers = [];
        const answerDescriptions = [];
        $("#answersTable .answerOption").not('.hide').each(function () {
            answers.push($(this).find('td.answerOptionText').text());
            answerDescriptions.push($(this).find('td.answerOptionDescription').text());
        });

        let maxNumAnswers = parseInt($('#maxNoAnswers').val());
        if (isNaN(maxNumAnswers)) {
            maxNumAnswers = 1;
        }


        $.post({
            url: 'UpdateVotingQuestion',
            data: {
                "id": mVotingId,
                "qid": idx,
                "title": title,
                "desc": description,
                "type": type,
                "answers": answers,
                "answerDescriptions": answerDescriptions,
                "minNumAnswers": 1,
                "maxNumAnswers": maxNumAnswers
            },
            success: function (data) { // todo
                $('#newQuestionModal').modal('hide');
                resetNewQuestionModal();
                updateQuestions();
            }
            // error: todo
        });
    }


    var editQuestion = function (question) {
        $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(question.index); });
        $('#newQuestionTitle').val(question.text);
        $('#newQuestionDescription').val(question.description);
        $('#questionType').val(question.type);
        if (question.type !== '1') {
            $('#maxNoAnswers').val(question.maxNumAnswers);
            $('#maxNoAnswers').parent().show();
        }

        $("#answersTable .answerOption").not('.hide').each(function () {
            $(this).detach();
        });


        $.each(question.answerOptions,
            function (idx, answer) {
                const $clone = $('#answersTable').find('tr.hide').clone(true).removeClass('hide');
                $clone.addClass('answer-line');
                $clone.children('td.answerOptionText').text(answer.answer);
                $clone.children('td.answerOptionDescription').text(answer.description);
                $('#answersTable').find('table').append($clone);
            });

        $('#newQuestionModal').modal();
    }


    var lockQuestion = function (questionIndex) {
        $.ajax({
            url: 'LockQuestion',
            data: { "votingId": mVotingId, 'questionIndex': questionIndex },
            type: 'POST',
            datatype: 'json',
            success: function (data) {
                updateQuestions(mVotingId);
            } // TODO error
        });
    }

    var loadQuestionResults = function (questionIndex) {
        $(`#result-dropdown-${questionIndex}`).load(`../Voting/GetResults?votingId=${mVotingId}&questionIndex=${questionIndex}`);
    }

    var showResultCounts = function (data) {
        $.each(data,
            function (index, value) {
                const progressBar = $(`.questionProgress[data-questionid=${value.index}]`).first();
                const percentage = (100 * value.count) / mGrantedRegistrationCount;

                if (progressBar && progressBar.length > 0) {
                    progressBar.removeClass('d-none');
                    const bar = progressBar.children('div').first();
                    bar.css("width", `${percentage}%`);
                    bar.attr("aria-valuenow", `${percentage}`);
                    bar.text(`${value.count} Wahlberechtigte haben bereits abgestimmt`);
                }
            });
    }

    var updateResultCounts = function () {
        if (!mGrantedRegistrationCount) {
            return;
        }

        const questionIndices = [];
        $(".questionCardInactive").each(function () {
            const id = $(this).attr('data-questionid');
            questionIndices.push(parseInt(id));
        });

        if (questionIndices.length === 0)
            return;

        $.ajax({
            url: '../Voting/GetNumberOfAnswers',
            data: { "votingId": mVotingId, 'questionIndices': questionIndices },
            type: 'POST',
            datatype: 'json',
            success: function (data) {
                showResultCounts(data);
            } // TODO error
        });
    }


    var createQuestion = function () {
        $('#shareLinkText').popover('hide');
        $('#fwBtnShareLink').popover('hide');
        $('#shareLinkText').popover('dispose');
        $('#fwBtnShareLink').popover('dispose');
        $('#newQuestionModal').modal();
        $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(0); });
    }

    var setGrantedRegistrationCount = function (count) {
        const wasCountSet = mGrantedRegistrationCount;
        mGrantedRegistrationCount = count;
        if (!wasCountSet) {
            updateResultCounts();
        }
    }

    var init = function (votingId) {
        mVotingId = votingId;
        $('#questionList').on('click', ".fwBtnEditQuestion", function() {
             editQuestion(parseInt($(this).attr("data-questionid")));
        });
        $('#questionList').on('click', ".fwBtnDeleteQuestion", function () {
            deleteQuestion(parseInt($(this).attr("data-questionid")));
        });
        $('#questionList').on('click', ".fwBtnUnlockQuestion", function () {
            unlockQuestion(parseInt($(this).attr("data-questionid")));
        });
        $('#questionList').on('click', ".fwBtnLockQuestion", function () {
            lockQuestion(parseInt($(this).attr("data-questionid")));
        });

        $('#questionList').on('click', "#fw-new-question-button", createQuestion);
        $('#questionList').on('click', "#fw-new-question-onboarding", createQuestion);

        updateResultCounts();
        window.setInterval(updateResultCounts, 60000);
    }

    return {
        init: init,
        setGrantedRegistrationCount: setGrantedRegistrationCount,
        setQuestions: setQuestions
    }
})();


//-----------------------------------
// Module Registrations
//-----------------------------------
const Registration = (function () {
    let mVotingId;
    let grantRegistration;
    let denyRegistration;

    var showCompletedRegistrations = function (registrations) {
        const grantedList = $("#grantedRegistrationsList");
        grantedList.empty();
        const deniedList = $("#deniedRegistrationsList");
        deniedList.empty();
        let grantedCount = 0;
        let deniedCount = 0;
        for (var i = 0; i < registrations.length; i++) {
            const registration = registrations[i];
            const item = `<div class="m-0 px-3 border-bottom">${registration.voterName}</div>`;
            if (registration.decision === 1) {
                grantedList.append(item);
                grantedCount++;
            } else {
                deniedList.append(item);
                deniedCount++;
            }
        }

        $('#grantedRegistrationsBadge').text(grantedCount);
        $('#deniedRegistrationsBadge').text(deniedCount);
        Edit.setGrantedRegistrationCount(grantedCount);
    }

    var showRegistrations = function (registrations) {
        $(".openRegistrationItem").detach();
        for (let i = 0; i < registrations.length; i++) {
            const registration = registrations[i];
            const item =
                `<div class="d-flex mx-3 my-0 border-bottom openRegistrationItem" id="openreg-${registration.registrationId}"><div style="flex:1;margin-right:1rem">${registration.voterName}</div>\n` +
                `<div class="fw-registration-item-button fwBtnGrantRegistration" data-registration-id="${registration.registrationId}"><i class="material-icons">check_circle_outline</i></div>\n` +
                `<div class="fw-registration-item-button fwBtnDenyRegistration" data-registration-id="${registration.registrationId}"><i class="material-icons mx-1">block</i></div>\n` +
                `<div class="fw-registration-item-button"><i class="material-icons">info</i></div></div>`;

            $(item).insertBefore('#openRegistrationsDivider');
        }

        $('#openRegistrationsBadge').text(registrations.length);
    }

    var updateRegistrations = function () {
        $.ajax({
            url: '../Registration/GetRegistrations',
            data: { "votingId": mVotingId },
            type: 'POST',
            datatype: 'json',
            success: function (data) {
                showRegistrations(data);
            } // TODO error
        });

        $.ajax({
            url: '../Registration/GetCompletedRegistrations',
            data: { "votingId": mVotingId },
            type: 'POST',
            datatype: 'json',
            success: function (data) {
                showCompletedRegistrations(data);
            } // TODO error
        });
    }

    grantRegistration = function (regIds) {
        $('#grantedRegistrationsBadge').text('.');
        $('#openRegistrationsBadge').text('.');
        $.each(regIds,
            function (index, value) {
                $(`#openreg-${value}`).slideUp(250, function () {
                    $(`#openreg-${value}`).detach();
                });
            });

        $.post({
            url: "../Registration/GrantRegistration",
            data: { 'registrationIds': regIds, 'utcOffsetMinutes': new Date().getTimezoneOffset(), 'timezoneName': Intl.DateTimeFormat().resolvedOptions().timeZone },
            success: function (data) { // todo
                updateRegistrations();
            }
            // error: todo
        });
    };

    denyRegistration = function (regIds) {
        $('#deniedRegistrationsBadge').text('.');
        $('#openRegistrationsBadge').text('.');
        $.each(regIds,
            function (index, value) {
                $(`#openreg-${value}`).slideUp(250, function () {
                    $(`#openreg-${value}`).detach();
                });
            });

        $.post({
            url: '../Registration/DenyRegistration',
            data: { "registrationIds": regIds },
            success: function (data) { // todo
                updateRegistrations();
            }
            // error: todo
        });
    };

    var grantAllRegistrations = function () {
        const ids = $('.openRegistrationItem').map(function () {
            return this.id.substr(8);
        });
        grantRegistration(ids.toArray());
    }

    var denyAllRegistrations = function () {
        const ids = $('.openRegistrationItem').map(function () {
            return this.id.substr(8);
        });
        denyRegistration(ids.toArray());
    }

    var sendInvitations = function () {
        const recipients = $('#mailRecipients').val().split(/[\n\r,;]+/);
        $.post({
            url: 'SendInvitationMail',
            data: { "votingId": mVotingId, "addresses": recipients },
            success: function (data) {
                $('#inviteVotersModal').modal('hide');
            }
            // error: todo
        });
    }

    var showInviteModal = function () {
        $('#shareLinkText').popover('hide');
        $('#fwBtnShareLink').popover('hide');
        $('#shareLinkText').popover('dispose');
        $('#fwBtnShareLink').popover('dispose');
        $('#mailRecipients').val('');
        $('#inviteVotersModal').modal();
    }

    var init = function (votingId) {
        mVotingId = votingId;
        $('#fwBtnShareLink').click(showInviteModal);
        $('#fwBtnGrantAll').click(grantAllRegistrations);
        $('#fwBtnDenyAll').click(denyAllRegistrations);
        $('#fwBtnSendInvitations').click(sendInvitations);
        $("#fwInviteMailText").text(function () {
            const date = parseInt($(this).attr("data-utcoffset"));
            return $(this).text().replace("$datum$", formatDateTimeSecondsForMail(date));
        });

        $('#openRegistrationsList').on('click', '.fwBtnGrantRegistration', function () {
            grantRegistration([$(this).attr('data-registration-id')]);
        });
        $('#denyRegistrationsList').on('click', '.fwBtnDenyRegistration', function () {
            grantRegistration([$(this).attr('data-registration-id')]);
        });

        updateRegistrations();
    }

    return {
        init: init
    }
})();

//-----------------------------------
// Setup edit screen
//-----------------------------------
function setupEditScreen(votingId) {
    Registration.init(votingId);
    Edit.init(votingId);

    $(document).on('click',
        '.dropdown-menu',
        function (e) {
            e.stopPropagation();
        });

    $('.answerOptionText').click(function () {
        if ($(this).text() === '...') {
            document.execCommand('selectAll', false, null);
        }
    });

    $('.answerOptionDescription').click(function () {
        if ($(this).text() === '...') {
            document.execCommand('selectAll', false, null);
        }
    });
    $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(votingId, 0); });

    $(window).on('scroll', blinkOnScroll);

    $('#questionType').change(function () {
        if ($('#questionType').val() === '1') {
            $('#maxNoAnswers').parent().hide();
        } else {
            $('#maxNoAnswers').parent().show();
        }
    });

    $('.table-add').click(function () {
        const $clone = $('#answersTable').find('tr.hide').clone(true).removeClass('hide table-line');
        $clone.addClass('answer-line');
        $('#answersTable').find('table').append($clone);
    });

    $('.table-remove').click(function () {
        $(this).parents('tr.answer-line').detach();
    });

    $('.table-up').click(function () {
        const $row = $(this).parents('tr');
        if ($row.index() === 1) return; // Don't go above the header
        $row.prev().before($row.get(0));
    });

    $('.table-down').click(function () {
        const $row = $(this).parents('tr');
        $row.next().after($row.get(0));
    });
}