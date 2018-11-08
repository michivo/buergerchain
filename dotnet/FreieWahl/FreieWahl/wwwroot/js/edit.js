//-----------------------------------
// Module Edit
//-----------------------------------
const Edit = (function () {
    let mVotingId;
    let mGrantedRegistrationCount;
    let mCurrentVotingData;
    let mQuestions;
    let mProgressBars;
    let mProgressBarConfig;
    let mChartsReady;
    let mCharts;

    var setQuestions = function (questions) {
        mQuestions = questions;
    }

    var findAnswer = function (key, answerOptions) {
        const answerOption = answerOptions.find(function (answer) {
            return answer.id === key;
        });

        return answerOption;
    };

    var addChart = function (questionIndex, chart) {
        let chartIndex = 0;
        for (; chartIndex < mCharts.length; chartIndex++) {
            if (mCharts[chartIndex].index === questionIndex) {
                mCharts[chartIndex].chart = chart;
                return;
            }
        }

        mCharts.push({ index: questionIndex, chart: chart });
    }

    var setChartType = function (type, questionIndex) {
        let chartIndex = 0;
        for (; chartIndex < mCharts.length; chartIndex++) {
            if (mCharts[chartIndex].index === questionIndex) {
                const chart = mCharts[chartIndex].chart;
                chart.setChartType(type);
                chart.draw();
                $(`.fw-chart-buttons[data-questionid="${questionIndex}"] > i[data-charttype="${type}"]`)
                    .removeClass("fw-chart-button-inactive").addClass('fw-chart-button-active');
                $(`.fw-chart-buttons[data-questionid="${questionIndex}"] > i`).not(`[data-charttype="${type}"]`)
                    .removeClass("fw-chart-button-active").addClass('fw-chart-button-inactive');
                return;
            }
        }
    }

    var renderDecisionQuestionResults = function (question) {

        const keys = [];
        const vals = [];
        let totalNumVotes = 0;
        question.votes.forEach(function (vote) {
            if (vote.length > 1) {
                console.log('Invalid vote is ignored!');
                return;
            }
            let key;
            if (vote.length === 0) {
                key = 'Enthaltungen';
            } else {
                key = vote[0];
            }
            totalNumVotes += 1;
            const keyIndex = keys.indexOf(key);
            if (keyIndex === -1) {
                keys.push(key);
                vals.push(1);
            } else {
                vals[keyIndex] = vals[keyIndex] + 1;
            }
        });

        const seriesData = [];
        let idx = 0;
        for (; idx < vals.length; idx++) {
            if (keys[idx] === 'Enthaltungen') {
                seriesData.push(['Enthaltungen', vals[idx]]); //, `Enthaltungen\r\n ${vals[idx]}/${totalNumVotes}`
            } else {
                const answer = findAnswer(keys[idx], question.answerOptions);
                seriesData.push([answer.answer, vals[idx]]); //, `${answer.answer}\r\n ${vals[idx]}/${totalNumVotes}`
            }
        }

        seriesData.sort(function (a, b) { return b[1] - a[1]; });

        var dataTable = new google.visualization.DataTable();
        dataTable.addColumn('string', 'Option');
        dataTable.addColumn('number', 'Stimmen');
        dataTable.addRows(seriesData);

        var options = {
            colors: ['#657f8d', '#232f19', '#8a6476', '#7e3237', '#224e7f', '#798233', '#443848', '#c47a58'],
            fontName: 'Roboto',
            fontSize: 14,
            chartArea: { width: '85%', height: '85%' }
        };


        var chart = new google.visualization.ChartWrapper({
            containerId: `fw-question-result-${question.index}`,
            dataTable: dataTable,
            options: options,
            chartType: 'PieChart'
        });

        chart.draw();

        addChart(question.index, chart);
    }

    var renderMultipleChoiceQuestionResults = function (question) {
        const keys = [];
        const vals = [];
        let totalNumVotes = 0;
        question.votes.forEach(function (vote) {
            if (vote.length === 0) {
                keys.push('Enthaltungen');
                vals.push(1);
                totalNumVotes++;
                return;
            }

            vote.forEach(function (key) {
                totalNumVotes += 1;
                const keyIndex = keys.indexOf(key);
                if (keyIndex === -1) {
                    keys.push(key);
                    vals.push(1);
                } else {
                    vals[keyIndex] = vals[keyIndex] + 1;
                }
            });
        });

        const seriesData = [];
        let idx = 0;
        for (; idx < vals.length; idx++) {
            if (keys[idx] === 'Enthaltungen') {
                seriesData.push(['Enthaltungen', vals[idx]]); //, `Enthaltungen\r\n ${vals[idx]}/${totalNumVotes}`]);
            } else {
                const answer = findAnswer(keys[idx], question.answerOptions);
                seriesData.push([answer.answer, vals[idx]]); //, `${answer.answer}\r\n ${vals[idx]}/${totalNumVotes}`]);
            }
        }

        seriesData.sort(function(a, b) { return b[1] - a[1]; });

        var dataTable = new google.visualization.DataTable();
        dataTable.addColumn('string', 'Option');
        dataTable.addColumn('number', 'Stimmen');
        //dataTable.addColumn({ type: 'string', role: 'tooltip' });
        dataTable.addRows(seriesData);

        var options = {
            colors: ['#657f8d', '#232f19', '#8a6476', '#7e3237', '#224e7f', '#798233', '#443848', '#c47a58'],
            fontName: 'Roboto',
            fontSize: 14,
            chartArea: { width: '85%', height: '85%' }
        };


        var chart = new google.visualization.ChartWrapper({
            containerId: `fw-question-result-${question.index}`,
            dataTable: dataTable,
            options: options,
            chartType: 'PieChart'
        });

        chart.draw();
        addChart(question.index, chart);
    }

    var renderOrderingQuestionResults = function (question) {
        const keys = [];
        const vals = [];
        let totalNumVotes = 0;
        let maxNumVotes = 0;
        question.votes.forEach(function (vote) {
            if (vote.length > maxNumVotes) {
                maxNumVotes = vote.length;
            }
        });

        question.votes.forEach(function (vote) {
            if (vote.length === 0) {
                return;
            }

            vote.forEach(function (key, index) {
                totalNumVotes += 1;
                let keyIndex = keys.indexOf(key);
                if (keyIndex === -1) {
                    keyIndex = keys.length;
                    keys.push(key);
                    const newVal = [];
                    let idx = 0;
                    newVal.push(findAnswer(key, question.answerOptions).answer);
                    for (; idx < maxNumVotes; idx++) {
                        newVal.push(0);
                    }
                    vals.push(newVal);
                }

                vals[keyIndex][index + 1] = vals[keyIndex][index + 1] + 1;
            });
        });

        vals.sort(function (a, b) { return b[1] - a[1]; });

        var dataTable = new google.visualization.DataTable();
        dataTable.addColumn('string', 'Option');
        let keyIndex = 0;
        for (; keyIndex < maxNumVotes; keyIndex++) {
            dataTable.addColumn('number', `Reihung #${keyIndex + 1}`);
        }
        dataTable.addRows(vals);

        var options = {
            colors: ['#657f8d', '#232f19', '#8a6476', '#7e3237', '#224e7f', '#798233', '#443848', '#c47a58'],
            fontName: 'Roboto',
            fontSize: 14,
            bars: 'horizontal',
            chartArea: { 'width': '90%', 'height': '90%' },
        };

        var chart = new google.visualization.ChartWrapper({
            containerId: `fw-question-result-${question.index}`,
            dataTable: dataTable,
            options: options,
            chartType: 'Bar'
        });

        chart.draw();
        addChart(question.index, chart);
    }


    var showResults = function () {

        if (!mChartsReady) {
            return;
        }

        const questionsWithResults = mQuestions.filter(function (question) {
            return question.status === 2;
        });

        questionsWithResults.forEach(function (question) {
            if (question.type === 1) {
                renderDecisionQuestionResults(question);
            }
            else if (question.type === 2) {
                renderMultipleChoiceQuestionResults(question);
            }
            else if (question.type === 3) {
                renderOrderingQuestionResults(question);
            }

        });

    }

    var getProgressBar = function (index) {
        const bar = mProgressBars.find(function (element) {
            return element.index === index;
        });

        if (typeof bar !== 'undefined' && bar) {
            return bar.bar;
        }

        const newBar =
            new ProgressBar.SemiCircle(document.getElementById(`progress-container-${index}`), mProgressBarConfig);
        mProgressBars.push({ index: index, bar: newBar });
        $(`#progress-container-${index}`).removeClass('d-none');
        $(`#progress-container-${index}`).addClass('d-flex');
        return newBar;
    }

    var showResultCounts = function (data) {
        data = (typeof data !== 'undefined') ? data : mCurrentVotingData;
        if (typeof data === 'undefined' || typeof mGrantedRegistrationCount === 'undefined') {
            return;
        }

        $.each(data,
            function (index, value) {
                const progressBar = getProgressBar(value.index);
                const percentage = (1.0 * value.count) / mGrantedRegistrationCount;
                const container = $(`#progress-container-${value.index}`);
                if (container.hasClass('d-none')) {
                    container.removeClass('d-none');
                    container.addClass('d-flex');
                }
                progressBar.animate(percentage);
                $(`#progress-value-${value.index}`).text(`${value.count}/${mGrantedRegistrationCount}`);
            });
    }

    var updateQuestions = function () {
        $('#questionList').load(`QuestionList?id=${mVotingId}`);
        showResultCounts();
        showResults();
    }

    var verifyResults = function (questionIndex) {
        $(`#fw-button-verify-results-${questionIndex}`).addClass('disabled');
        $.ajax({
            url: '../Voting/VerifyVotes',
            data: { "votingId": mVotingId, "questionIndex": questionIndex },
            type: 'POST',
            datatype: 'json',
            success: function (data) {
                $(`#fw-button-verify-results-${questionIndex}`).removeClass('btn-secondary');
                $(`#fw-button-verify-results-${questionIndex}`).addClass('btn-success');
                $(`#fw-button-verify-results-${questionIndex}`)
                    .text(`Die Integrität von ${data} abgegebenen Stimmen wurde erfolgreich überprüft!`);
            },
            error: function(data) {
                $(`#fw-button-verify-results-${questionIndex}`).removeClass('btn-secondary');
                $(`#fw-button-verify-results-${questionIndex}`).addClass('btn-danger');
                $(`#fw-button-verify-results-${questionIndex}`)
                    .text('Die Integrität der Ergebnisse konnte nicht verifiziert werden!');
            }
        });
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


    var editQuestion = function (questionIndex) {
        const question = mQuestions.find(function (question) {
            return question.index === questionIndex;
        });

        $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(questionIndex); });
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

    var updateResultCounts = function () {
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
                mCurrentVotingData = data;
                showResultCounts(data);
            } // TODO error
        });
    }


    var createQuestion = function () {
        resetNewQuestionModal();
        $('#shareLinkText').popover('hide');
        $('#fwBtnShareLink').popover('hide');
        $('#shareLinkText').popover('dispose');
        $('#fwBtnShareLink').popover('dispose');
        $('#newQuestionModal').modal();
        $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(0); });
    }

    var setGrantedRegistrationCount = function (count) {
        mGrantedRegistrationCount = count;
        showResultCounts();
    }

    var unlockQuestion = function (questionIndex) {
        $.post({
            url: 'UnlockQuestion',
            data: { "votingId": mVotingId, "questionIndex": questionIndex },
            success: function (data) {
                updateQuestions();
            }
            // error: todo
        });
    }

    var chartsReady = function() {
        mChartsReady = true;
        showResults();
    }

    var init = function (votingId, questions) {
        mVotingId = votingId;
        mQuestions = questions;

        $('#questionList').on('click', ".fwBtnEditQuestion", function () {
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

        mProgressBarConfig = {
            strokeWidth: 15,
            color: '#3583A9',
            trailColor: '#eee',
            trailWidth: 1,
            easing: 'easeInOut',
            duration: 1000
        };

        mProgressBars = [];
        mCharts = [];

        updateResultCounts();
        window.setInterval(updateResultCounts, 60000);

        mChartsReady = false;
        google.charts.load('current', { 'packages': ['corechart', 'table'] });
        google.charts.setOnLoadCallback(chartsReady);
    }

    return {
        init: init,
        setGrantedRegistrationCount: setGrantedRegistrationCount,
        setQuestions: setQuestions,
        setChartType: setChartType,
        verifyResults: verifyResults
    }
})();


//-----------------------------------
// Module Registrations
//-----------------------------------
const Registration = (function () {
    let mVotingId;
    let grantRegistration;
    let denyRegistration;
    let currentUpdateRequest;

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
        if (currentUpdateRequest) {
            currentUpdateRequest.abort();
        }
        currentUpdateRequest = $.ajax({
            url: '../Registration/GetRegistrations',
            data: { "votingId": mVotingId },
            type: 'POST',
            datatype: 'json',
            success: function (data) {
                showRegistrations(data);
                currentUpdateRequest = null;
                window.setTimeout(updateRegistrations, 120000);
            }, // TODO error
            error: function (err) {
                currentUpdateRequest = null;
            }
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

    var init = function (votingId, questions) {
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
        $('#openRegistrationsList').on('click', '.fwBtnDenyRegistration', function () {
            denyRegistration([$(this).attr('data-registration-id')]);
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
function setupEditScreen(votingId, questions) {
    Registration.init(votingId);
    Edit.init(votingId, questions);

    $(document).on('click',
        '.dropdown-menu',
        function (e) {
            e.stopPropagation();
        });

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
        $clone.find('br').remove();
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
