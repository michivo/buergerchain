﻿function isEmpty(str) {
    return (!str || 0 === str.length || str === '0');
}

function deleteQuestion(votingId, questionNumber) {
    $.ajax({
        url: 'DeleteVotingQuestion',
        data: { "id": votingId, "qid": questionNumber },
        type: 'POST',
        datatype: 'json',
        success: function (data) {
            updateQuestions(votingId);
        } // TODO error
    });
}

function editQuestion(question) {
    $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(question.votingId, question.index); });
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

    let maxNumAnswers = parseInt($('#maxNoAnswers').val());
    if (isNaN(maxNumAnswers)) {
        maxNumAnswers = 1;
    }


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
            "minNumAnswers": 1,
            "maxNumAnswers": maxNumAnswers
        },
        success: function (data) { // todo
            $('#newQuestionModal').modal('hide');
            resetNewQuestionModal();
            updateQuestions(vid);
        }
        // error: todo
    });
}

function formatDateTimeSeconds(secondsSinceEpoch) {
    var date = new Date(0);
    date.setUTCSeconds(secondsSinceEpoch);
    return `${date.getDate()}.${(date.getMonth() + 1).toString().padStart(2, '0')}.${date.getFullYear()} ${date.getHours()}:${date.getMinutes().toString().padStart(2, '0')}`;
}

function updateQuestions(votingId) {
    $('#questionList').load(`QuestionList?id=${votingId}`);
}

function resetNewQuestionModal() {
    $('#newQuestionTitle').val('');
    $('#newQuestionDescription').val('');
    $('#maxNoAnswers').val('1');
    $("#answersTable .answerOption").not('.hide').each(function () {
        $(this).detach();
    });
    $('#modalQuestionOk').text('OK');
    $('#modalQuestionOk').removeClass('disabled');
}

function grantRegistration(regIds, votingId) {
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
            updateRegistrations(votingId);
        }
        // error: todo
    });
}

function denyRegistration(regIds, votingId) {
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
}

function lockQuestion(votingId, questionIndex) {
    $.ajax({
        url: 'LockQuestion',
        data: { "votingId": votingId, 'questionIndex': questionIndex },
        type: 'POST',
        datatype: 'json',
        success: function (data) {
            updateQuestions(votingId);
        } // TODO error
    });
}

function loadQuestionResults(votingId, questionIndex) {
    $(`#result-dropdown-${questionIndex}`).load(`../Voting/GetResults?votingId=${votingId}&questionIndex=${questionIndex}`);
}

function showQuestionResults(votingId, questionIndex, resultData) {

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
    $(".openRegistrationItem").detach();
    for (let i = 0; i < registrations.length; i++) {
        const registration = registrations[i];
        const item =
            `<div class="d-flex mx-3 my-0 border-bottom openRegistrationItem" id="openreg-${registration.registrationId}"><div style="flex:1;margin-right:1rem">${registration.voterName}</div>\n` +
            `<div class="fw-registration-item-button" onclick="grantRegistration(['${registration.registrationId}'], '${votingId}')"><i class="material-icons">add_circle_outline</i></div>\n` +
            `<div class="fw-registration-item-button" onclick="denyRegistration(['${registration.registrationId}'], '${votingId}')"><i class="material-icons mx-1">remove_circle_outline</i></div>\n` +
            `<div class="fw-registration-item-button"><i class="material-icons">info</i></div></div>`;
        $(item).insertBefore('#openRegistrationsDivider');
    }

    $('#openRegistrationsBadge').text(registrations.length);
}

function grantAllRegistrations(votingId) {
    const ids = $('.openRegistrationItem').map(function () {
        return this.id.substr(8);
    });
    grantRegistration(ids.toArray(), votingId);
}

function denyAllRegistrations(votingId) {
    const ids = $('.openRegistrationItem').map(function () {
        return this.id.substr(8);
    });
    denyRegistration(ids.toArray(), votingId);
}


function sendInvitations(votingId) {
    const recipients = $('#mailRecipients').val().split(/[\n\r,;]+/);
    $.post({
        url: 'SendInvitationMail',
        data: { "votingId": votingId, "addresses": recipients },
        success: function (data) {
            $('#inviteVotersModal').modal('hide');
        }
        // error: todo
    });
}

function setupEditScreen(votingId) {
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

    updateResultCounts(votingId);
    window.setInterval(function () {
        updateResultCounts(votingId);
    },
        60000);
}

function updateResultCounts(votingId) {
    const questionIndices = [];
    $(".questionCardInactive").each(function () {
        const id = $(this).attr('data-questionid');
        questionIndices.push(parseInt(id));
    });

    if (questionIndices.length === 0)
        return;

    $.ajax({
        url: '../Voting/GetNumberOfAnswers',
        data: { "votingId": votingId, 'questionIndices': questionIndices },
        type: 'POST',
        datatype: 'json',
        success: function (data) {
            showResultCounts(data);
        } // TODO error
    });
}

function showResultCounts(data) {
    $.each(data,
        function (index, value) {
            const progressBar = $(`.questionProgress[data-questionid=${value.index}]`).first();
            if (progressBar && progressBar.length > 0) {
                progressBar.removeClass('bg-white');
                const bar = progressBar.children('div').first();
                bar.css("width", `${5 * value.count}%`);
                bar.attr("aria-valuenow", `${5 * value.count}`);
                bar.text(`${value.count} Wahlberechtigte haben bereits abgestimmt`);
            }
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
    $('#shareLinkText').popover('hide');
    $('#shareLinkButton').popover('hide');
    $('#shareLinkText').popover('dispose');
    $('#shareLinkButton').popover('dispose');
    $('#newQuestionModal').modal();
    $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(votingId, 0); });
}

function showInviteModal() {
    $('#shareLinkText').popover('hide');
    $('#shareLinkButton').popover('hide');
    $('#shareLinkText').popover('dispose');
    $('#shareLinkButton').popover('dispose');
    $('#mailRecipients').val('');
    $('#inviteVotersModal').modal();
}

//----------------------------------- OVERVIEW FUNCTIONS

function setupOverview() {
    initApp();

    $('document').ready(function () {
        $('#fw-user-img-input').change(function () { previewAndUploadFile('user'); });
        $('#fw-voting-img-input').change(function () { previewAndUploadFile('voting'); });
        $('#fwStartDate').change(function () { fwValidate('#fwStartDate', '#fwStartDateValidation', /^[0-3]?\d\.[0,1]?\d\.20\d{2}$/); });
        $('#fwEndDate').change(function () { fwValidate('#fwEndDate', '#fwEndDateValidation', /^[0-3]?\d\.[0,1]?\d\.20\d{2}$/); });
        $('#fwStartTime').change(function () { fwValidate('#fwStartTime', '#fwStartTimeValidation', /^[0-2]?\d:[0-5]\d$/); });
        $('#fwEndTime').change(function () { fwValidate('#fwEndTime', '#fwEndTimeValidation', /^[0-2]?\d:[0-5]\d$/); });
        $('#fwNewVotingName').change(function () { fwValidate('#fwNewVotingName', '#fwNewVotingNameValidation', /^(?!\s*$).+/); });

        showOverview();
        $('#fw-user-img-content').hover(
            function () { highlightImgSelector('user', 'rgba(255, 255, 255, 0.5)'); },
            function () { resetImgSelector('user', 'rgba(255, 255, 255, 0.5)'); }
        );

        $('#fw-voting-img-content').hover(
            function () { highlightImgSelector('voting', 'rgba(101, 127, 140, 0.5)'); },
            function () { resetImgSelector('voting', 'rgba(101, 127, 140, 0.5)'); }
        );

        $('.form-group.date').datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "de",
            orientation: "top left"
        });
    });
}

function highlightImgSelector(type, color) {
    $("#fw-" + type + "-img-selector").css("border", "1px solid " + color);
    $("#fw-" + type + "-img-selector").css({
        backgroundColor: 'rgba(0,0,0,.5)'
    });
    $("#fw-" + type + "-img-selector").css("box-shadow", "1px 1px 1px rgba(0, 0, 0, 0.5)");
    $("#fw-" + type + "-img-icon").animate(
        { fontSize: "1.5rem", color: "white" });
    $("#fw-" + type + "-img-text").animate({
        opacity: 1
    });
}

function resetImgSelector(type, color) {
    $("#fw-" + type + "-img-selector").css("border", "0px solid " + color);
    $("#fw-" + type + "-img-selector").css({
        backgroundColor: 'rgba(0,0,0,0)',
    });
    $("#fw-" + type + "-img-icon").animate(
        { fontSize: "2rem" });
    $("#fw-" + type + "-img-text").animate({
        opacity: 0
    });
    $("#fw-" + type + "-img-selector").css("box-shadow", "0px 0px 0px");
    $('#fw-' + type + '-img-upload').css('display', 'none');
}

function highlightVotingCard() {
    console.log(this);
}

function showFileSelector(elementId) {
    $(elementId).css("display", "inline");
}

function highlightAddVoting(jqueryPath) {
    $(jqueryPath).addClass('shadow-pulse');
}

function resetAddVoting(jqueryPath) {
    $(jqueryPath).removeClass('shadow-pulse');
}

function createVoting() {
    $("#newVotingModal").modal();
}

function previewAndUploadFile(type) {
    var file = $('#fw-' + type + '-img-input')[0].files[0];
    if (!file.type.match(/image.*/)) {
        return; // TODO: inform user to upload actual image
    };
    var img = new Image();
    var reader = new FileReader();
    reader.onload = function (e) {
        img.src = e.target.result;
    }

    img.onload = function () {
        var canvas = document.getElementById('fw-' + type + '-img-real');
        var context = canvas.getContext('2d');
        var MAX_WIDTH = 300;
        var MAX_HEIGHT = 300;
        var width = img.width;
        var height = img.height;

        if (width > height) {
            if (width > MAX_WIDTH) {
                height *= MAX_WIDTH / width;
                width = MAX_WIDTH;
            }
        } else {
            if (height > MAX_HEIGHT) {
                width *= MAX_HEIGHT / height;
                height = MAX_HEIGHT;
            }
        }
        canvas.width = width;
        canvas.height = height;
        context.drawImage(img, 0, 0, width, height);
        $('#fw-' + type + '-img-dummy').hide();
        $('#fw-' + type + '-img-real').show();
        if (type === 'user')
            uploadFile();
    }
    reader.readAsDataURL(file);
}

function uploadFile() {
    var canvas = document.getElementById('fw-user-img-real');
    var dataURL = canvas.toDataURL();
    $.ajax({
        url: 'UpdateUserImage',
        type: 'POST',
        data: {
            imageData: dataURL
        },
        success: updateImage,
        error: function (data) {
            // TODO
        }
    });
}

function updateImage(data) {
}

function fwValidate(elementId, validationId, regex) {
    const startDate = $(elementId).val();
    if (!startDate.match(regex)) {
        $(elementId).addClass('is-invalid');
        $(validationId).addClass('d-block');
        return false;
    }

    $(elementId).removeClass('is-invalid');
    $(validationId).removeClass('d-block');
    return true;
}

function saveVoting() {
    var title = $("#fwNewVotingName").val();
    var desc = $("#fwNewVotingDescription").val();
    var imageData = '';
    var validationResult = fwValidate('#fwStartDate', '#fwStartDateValidation', /^[0-3]?\d\.[0,1]?\d\.20\d{2}$/);
    validationResult = fwValidate('#fwEndDate', '#fwEndDateValidation', /^[0-3]?\d\.[0,1]?\d\.20\d{2}$/) && validationResult;
    validationResult = fwValidate('#fwStartTime', '#fwStartTimeValidation', /^[0-2]?\d:[0-5]\d$/) && validationResult;
    validationResult = fwValidate('#fwEndTime', '#fwEndTimeValidation', /^[0-2]?\d:[0-5]\d$/) && validationResult;
    validationResult = fwValidate('#fwNewVotingName', '#fwNewVotingNameValidation', /^(?!\s*$).+/) && validationResult;
    if (!validationResult)
        return;

    if ($('#fw-voting-img-real').is(':visible')) {
        var canvas = document.getElementById('fw-voting-img-real');
        imageData = canvas.toDataURL();
    }

    const startDate = parseDateTime($("#fwStartDate").val(), $("#fwStartTime").val());
    const endDate = parseDateTime($("#fwEndDate").val(), $("#fwEndTime").val());

    $.post({
        url: 'UpdateVoting',
        data: {
            "title": title,
            "desc": desc,
            "id": null,
            "imageData": imageData,
            "startDate": startDate.toISOString(),
            "endDate": endDate.toISOString(),
        },
        success: function (data) {
            $('#newVotingModal').modal('hide');
            window.location.replace(`Edit?id=${data}&isNew=true`);
        },
        error: function (err) {
            alert(err);
            // TODO
        }
    });
}

function parseDateTime(date, time) {
    const dateParts = date.split('.');
    const timeParts = time.split(':');
    const day = parseInt(dateParts[0]);
    const month = parseInt(dateParts[1]);
    const year = parseInt(dateParts[2]);
    const hour = parseInt(timeParts[0]);
    const minute = parseInt(timeParts[1]);
    return new Date(year, month - 1, day, hour, minute);
}

function showOverview() {
    $(".fw-open-voting").remove();
    $("#fw-load-votings-card").show();

    $.ajax({
        url: 'GetVotingsForUser',
        type: 'GET',
        datatype: 'json',
        success: showOverviewData,
        error: function (data) {
            console.log('oops, error');
            $("<div/>",
                {
                    "class": "bc-error-message TODO TODO error handling",
                    html: "An error occurred getting your votings: " + data
                }).appendTo("#votingOverviewListContent");
            componentHandler.upgradeDom();

            $("#fw-load-votings-card").hide();
        }
    });
}

function showOverviewData(data) {
    var items = [];
    $("#fw-active-voting-count").text(data.length);
    for (var i = 0; i < data.length; i++) {
        const voting = data[i];
        let item = '<div class="col-xl-4 col-md-6 col-12 fw-open-voting"><div class="card fw-overview-card">' +
            `<a href="Edit?id=${voting.id}">`;
        if (voting.imageData) {
            item += `<img class="card-img-top" style="max-height:12rem;" src="${voting.imageData}">`;
        } else {
            item +=
                '<i class="material-icons fw-voting-img" style="text-align:center;width:100%">how_to_vote</i>';
        }

        item += `</a><div class="card-body d-flex flex-column justify-content-between" style="height:12rem"><h5 class="card-title">${voting.title}</h5>` +
            `<p class="card-text">${truncate(voting.description, 50)}</p>\r\n` +
            `<div><a class="fw-card-link-icon float-left p-2 border" href="javascript:void(0);" onclick="deleteVoting('${voting.id}')"><i class="material-icons">delete</i></a>\r\n` +
            `<a class="fw-card-link-icon bg-primary float-right p-2" href="Edit?id=${voting.id}"><i class="material-icons text-white">edit</i></a></div></div></div></div>`;
        items.push(item);
    }
    $(items.join("\n")).insertAfter("#fw-new-voting-onboarding");

    $("#fw-load-votings-card").hide();
    if (data.length === 0) {
        $("#fw-new-voting-onboarding").removeClass("d-none").addClass("d-flex");
        $("#fw-new-voting-button").addClass("d-none");
    } else {
        $("#fw-new-voting-onboarding").removeClass("d-flex").addClass("d-none");
        $("#fw-new-voting-button").removeClass("d-none");
    }

    componentHandler.upgradeDom();

    $(".fw-overview-card").mouseover(function () {
        $(this).removeClass('fw-overview-card-inactive');
        $(this).addClass('fw-overview-card-active');
    });


    $(".fw-overview-card").mouseout(function () {
        $(this).removeClass('fw-overview-card-active');
        $(this).addClass('fw-overview-card-inactive');
    });
}


function truncate(val, maxLength) {
    if (val.length < maxLength)
        return val;

    return val.substr(0, maxLength) + '...';
}

function deleteVoting(id) {
    if (confirm("Sind Sie sicher, dass Sie diese Abstimmung unwiderruflich löschen wollen?")) {

        $.post({
            url: 'DeleteVoting',
            data: { "id": id },
            success: function (data) {
                showOverview(firebase.auth().currentUser);
            }
            // error: todo
        });
    }
}

function unlockQuestion(votingId, questionIndex) {
    $.post({
        url: 'UnlockQuestion',
        data: { "votingId": votingId, "questionIndex": questionIndex },
        success: function (data) {
            updateQuestions(votingId);
        }
        // error: todo
    });
}