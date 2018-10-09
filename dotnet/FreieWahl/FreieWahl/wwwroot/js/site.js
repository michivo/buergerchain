function isEmpty(str) {
    return (!str || 0 === str.length || str === '0');
}

function deleteQuestion(votingId, questionNumber) {
    $.ajax({
        url: 'DeleteVotingQuestion',
        data: { "id": votingId, "qid": questionNumber },
        type: 'POST',
        datatype: 'json',
        success: function (data) {
            location.reload();
        } // TODO error
    });
}

function editQuestion(question) {
    $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(question.votingId, question.index); });
    $('#newQuestionTitle').val(question.text);
    $('#newQuestionDescription').val(question.description);
    $('#questionType').val(question.type);
    if (question.type !== '1') {
        $('#minNoAnswers').val(question.minNumAnswers);
        $('#maxNoAnswers').val(question.maxNumAnswers);
        $('#minNoAnswers').parent().show();
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

function saveQuestion(vid, idx) {
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
    const minNumAnswers = parseInt($('#minNoAnswers').val());
    const maxNumAnswers = parseInt($('#maxNoAnswers').val());

    $.post({
        url: 'UpdateVotingQuestion',
        data: {
            "id": vid,
            "qid": idx,
            "title": title,
            "desc": description,
            "type": type,
            "answers": answers,
            "answerDescriptions": answerDescriptions,
            "minNumAnswers": minNumAnswers,
            "maxNumAnswers": maxNumAnswers
        },
        success: function (data) { // todo
            $('#newQuestionModal').modal('hide');
        }
        // error: todo
    });
}

function grantRegistration(regId, votingId) {
    $.post({
        url: '../Registration/GrantRegistration',
        data: { "rid": regId },
        success: function (data) { // todo
            updateRegistrations(votingId);
        }
        // error: todo
    });
}

function denyRegistration(regId, votingId) {
    $.post({
        url: '../Registration/DenyRegistration',
        data: { "rid": regId },
        success: function (data) { // todo
            updateRegistrations(votingId);
        }
        // error: todo
    });
}

function showCompletedRegistrations(registrations) {
    const grantedList = $("#grantedRegistrationsList");
    grantedList.empty();
    const deniedList = $("#deniedRegistrationsList");
    deniedList.empty();
    let grantedCount = 0;
    let deniedCount = 0;
    for (var i = 0; i < registrations.length; i++) {
        const registration = registrations[i];
        const item = `<div class="m-0 px-3 border-bottom">@{registration.voterName}</div>`;
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
}

function updateRegistrations(votingId) {
    $.ajax({
        url: '../Registration/GetRegistrations',
        data: { "votingId": votingId },
        type: 'POST',
        datatype: 'json',
        success: function (data) {
            showRegistrations(data, votingId);
        } // TODO error
    });

    $.ajax({
        url: '../Registration/GetCompletedRegistrations',
        data: { "votingId": votingId },
        type: 'POST',
        datatype: 'json',
        success: function (data) {
            showCompletedRegistrations(data);
        } // TODO error
    });
}

function showRegistrations(registrations, votingId) {
    $("#openRegistrationsList").removeClass('openRegistrationItem');
    for (let i = 0; i < registrations.length; i++) {
        const registration = registrations[i];
        const item =
            `<div class="d-flex mx-3 my-0 border-bottom openRegistrationItem"><div style="flex:1;margin-right:1rem">${registration.voterName}</div>\n` +
            `<div style="align-self: flex-end;cursor:pointer;color:#657f8C" onclick="grantRegistration('${registration.registrationId}', '${votingId}')"><i class="material-icons">add_circle_outline</i></div>\n` +
            `<div style="align-self: flex-end;cursor:pointer;color:#657f8C" onclick="denyRegistration('${registration.registrationId}', '${votingId}')"><i class="material-icons mx-1">remove_circle_outline</i></div>\n` +
            `<div style="align-self: flex-end;cursor:pointer;color:#657f8C"><i class="material-icons">info</i></div></div>`;
        $(item).insertBefore('#openRegistrationsDivider');
    }

    $('#openRegistrationsBadge').text(registrations.length);
}


function sendInvitations(votingId) {
    const recipients = $('#mailRecipients').val().split(/[\n\r,;]+/);
    $.post({
        url: 'SendInvitationMail',
        data: { "votingId": votingId, "addresses": recipients },
        success: function(data) {
            $('#inviteVotersModal').modal('hide');
        }
        // error: todo
    });
}

function setupOverview(votingId) {
    $(document).on('click',
        '.dropdown-menu',
        function (e) {
            e.stopPropagation();
        });

    updateRegistrations(votingId);
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
            $('#minNoAnswers').parent().hide();
            $('#maxNoAnswers').parent().hide();
        } else {
            $('#minNoAnswers').parent().show();
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


function blinkOnScroll() {
    const winHeightPadded = $(window).height() * 1.1;
    const scrolled = $(window).scrollTop();
    $(".blinkOnScroll:not(.animated)").each(function () {
        var $this = $(this),
            offsetTop = $this.offset().top;

        if (scrolled + winHeightPadded > offsetTop) {
            if ($this.data('timeout')) {
                window.setTimeout(function () {
                    $this.addClass('animated blinky');
                },
                    parseInt($this.data('timeout'), 10));
            } else {
                $this.addClass('animated blinky');
            }
        }
    });
    $(".blinkOnScroll.animated").each(function (index) {
        const $this = $(this),
            offsetTop = $this.offset().top;
        if (scrolled + winHeightPadded < offsetTop) {
            $(this).removeClass('animated blinky');
        }
    });
}

function createQuestion(votingId) {
    $('#newQuestionTitle').val('');
    $('#newQuestionDescription').val('');
    $('#newQuestionModal').modal();
    $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(votingId, 0); });
}

function showInviteModal() {
    $('#mailRecipients').val('');
    $('#inviteVotersModal').modal();
}