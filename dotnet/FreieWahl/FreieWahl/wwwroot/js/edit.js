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

        $(`#fw-chart-export-csv-${questionIndex}`).click(function () {
            const dataTable = chart.getDataTable();
            var csvFormattedDataTable = google.visualization.dataTableToCsv(dataTable);
            var jsonArray = Papa.parse(csvFormattedDataTable, { encoding: "UTF-8" });
            var labels = [];
            let colCount = 0;
            var numColumns = dataTable.getNumberOfColumns();
            for (; colCount < numColumns; colCount++) {
                labels.push(dataTable.getColumnLabel(colCount));
            }
            jsonArray.data.splice(0, 0, labels);
            var formattedCsv = Papa.unparse(jsonArray, { delimiter: ";", encoding: "UTF-8" });
            var encodedUri = 'data:application/csv;charset=utf-8,' + encodeURIComponent(formattedCsv);
            this.href = encodedUri;
            this.download = 'tableData.csv';
            this.target = '_blank';
        });
        $(`#fw-chart-export-png-${questionIndex}`).click(function () {
            if ($(`#fw-chart-export-png-${questionIndex}`).attr("data-usecanvas") === "true") {
                const innerChart = chart.getChart();
                const imageData = innerChart.getImageURI();
                this.href = imageData;
                this.download = 'resultGraph.png';
                this.target = '_blank';
            }
        });
    }

    var setChartType = function (type, questionIndex) {
        const chart = mCharts.find(function (chart) {
            return chart.index === questionIndex;
        }).chart;

        if (type === 'Table') {
            $(`#fw-chart-export-png-${questionIndex}`).hide();
        } else {
            $(`#fw-chart-export-png-${questionIndex}`).show();
        }

        chart.setChartType(type);
        chart.draw();
        $(`.fw-chart-buttons[data-questionid="${questionIndex}"] > i[data-charttype="${type}"]`)
            .removeClass("fw-chart-button-inactive").addClass('fw-chart-button-active');
        $(`.fw-chart-buttons[data-questionid="${questionIndex}"] > i`).not(`[data-charttype="${type}"]`)
            .removeClass("fw-chart-button-active").addClass('fw-chart-button-inactive');
    }

    var renderDecisionQuestionResults = function (question) {

        const keys = [];
        const vals = [];
        let totalNumVotes = 0;
        let maxNumVotes = 0;
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
                if (vals[keyIndex] > maxNumVotes) {
                    maxNumVotes = vals[keyIndex];
                }
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

        const hAxis = {};
        if (maxNumVotes > 0 && maxNumVotes < 10) {
            const ticks = [];
            let tickIdx = 0;
            for (; tickIdx <= maxNumVotes; tickIdx++) {
                ticks.push(tickIdx);
            }
            hAxis.ticks = ticks;
        }

        var options = {
            colors: ['#657f8d', '#232f19', '#8a6476', '#7e3237', '#224e7f', '#798233', '#443848', '#c47a58'],
            fontName: 'Roboto',
            fontSize: 14,
            hAxis: hAxis
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
        let maxNumVotes = 0;

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
                    if (vals[keyIndex] > maxNumVotes) {
                        maxNumVotes = vals[keyIndex];
                    }
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

        seriesData.sort(function (a, b) { return b[1] - a[1]; });

        var dataTable = new google.visualization.DataTable();
        dataTable.addColumn('string', 'Option');
        dataTable.addColumn('number', 'Stimmen');
        //dataTable.addColumn({ type: 'string', role: 'tooltip' });
        dataTable.addRows(seriesData);

        const hAxis = {};
        if (maxNumVotes > 0 && maxNumVotes < 10) {
            const ticks = [];
            let tickIdx = 0;
            for (; tickIdx <= maxNumVotes; tickIdx++) {
                ticks.push(tickIdx);
            }
            hAxis.ticks = ticks;
        }

        var options = {
            colors: ['#657f8d', '#232f19', '#8a6476', '#7e3237', '#224e7f', '#798233', '#443848', '#c47a58'],
            fontName: 'Roboto',
            fontSize: 14,
            hAxis: hAxis
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

        const hAxis = {};
        if (maxNumVotes < 10) {
            const ticks = [];
            let tickIdx = 0;
            for (; tickIdx < maxNumVotes; tickIdx++) {
                ticks.push(tickIdx);
            }
            hAxis.ticks = ticks;
        }

        var options = {
            colors: ['#657f8d', '#232f19', '#8a6476', '#7e3237', '#224e7f', '#798233', '#443848', '#c47a58'],
            fontName: 'Roboto',
            fontSize: 14,
            bars: 'horizontal',
            hAxis: hAxis
        };

        console.log(options);

        var chart = new google.visualization.ChartWrapper({
            containerId: `fw-question-result-${question.index}`,
            dataTable: dataTable,
            options: options,
            chartType: 'BarChart'
        });

        chart.draw();
        addChart(question.index, chart);
    }

    var verifyResults = function (questionIndex) {
        $.ajax({
            url: '../Voting/VerifyVotes',
            data: { "votingId": mVotingId, "questionIndex": questionIndex },
            type: 'POST',
            datatype: 'json',
            success: function (data) {
                $(`#fw-results-verified-ok-${questionIndex}`).removeClass('d-none');
                $(`#fw-results-verified-ok-${questionIndex} > i`).text('verified_user');
                $(`#fw-results-verified-ok-${questionIndex} > i`).css('color', '#568233');
            },
            error: function (jqXhr, exception) {
                $(`#fw-results-verified-ok-${questionIndex}`).removeClass('d-none');
                $(`#fw-results-verified-ok-${questionIndex} > i`).text('error');
                $(`#fw-results-verified-ok-${questionIndex} > i`).css('color', '#7E3237');
                $(`#fw-results-verified-ok-${questionIndex}`).attr('title', 'Die Integritätsprüfung der Ergebnisse ist fehlgeschlagen!');
                $(`#fw-results-verified-ok-${questionIndex}`).attr('data-original-title', 'Die Integritätsprüfung der Ergebnisse ist fehlgeschlagen!');
            }
        });
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
            verifyResults(question.index);
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
        $('#questionList').load(`QuestionList?id=${mVotingId}`, function () {
            showResultCounts();
            showResults();
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
            },
            error: function (data) {
                showErrorMessage("Beim Löschen der Frage ist ein Fehler aufgetreten!", data);
            }
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


        $.ajax({
            url: 'UpdateVotingQuestion',
            type: 'POST',
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
            success: function (data) {
                $('#newQuestionModal').modal('hide');
                resetNewQuestionModal();
                updateQuestions();
            },
            error: function (data) {
                showErrorMessage("Beim Speichern der Frage ist ein Fehler aufgetreten!", data);
            }
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
            },
            error: function (data) {
                showErrorMessage("Beim Sperren der Frage ist ein Fehler aufgetreten!", data);
            }
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
            },
            error: function (data) {
                showErrorMessage("Beim Aktualisieren der Ergebnisse ist ein Fehler aufgetreten!", data);
            }
        });
    }

    var clearPopovers = function () {
        $('#shareLinkText').popover('hide');
        $('#fwBtnShareLink').popover('hide');
        $('#shareLinkText').popover('dispose');
        $('#fwBtnShareLink').popover('dispose');
        $('#shareLinkText').removeAttr('data-toggle');
        $('#fwBtnShareLink').removeAttr('data-toggle');
    };

    var createQuestion = function () {
        resetNewQuestionModal();
        clearPopovers();
        $('#newQuestionModal').modal();
        $('#modalQuestionOk').off('click').on('click', function () { saveQuestion(0); });
    };

    var setGrantedRegistrationCount = function (count) {
        mGrantedRegistrationCount = count;
        showResultCounts();
    }

    var unlockQuestion = function (questionIndex) {
        $.ajax({
            url: 'UnlockQuestion',
            data: { "votingId": mVotingId, "questionIndex": questionIndex },
            type: 'POST',
            success: function (data) {
                updateQuestions();
            },
            error: function (data) {
                showErrorMessage("Beim Freischalten der Frage ist ein Fehler aufgetreten!", data);
            }
        });
    }

    var chartsReady = function () {
        mChartsReady = true;
        showResults();
    }

    var editVoting = function () {
        Edit.clearPopovers();
        $("#newVotingModal").modal();
    }

    var updateVoting = function () {
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

        $.ajax({
            url: 'UpdateVoting',
            data: {
                "title": title,
                "desc": desc,
                "id": mVotingId,
                "imageData": imageData,
                "startDate": startDate.toISOString(),
                "endDate": endDate.toISOString(),
            },
            type: 'POST',
            success: function (data) {
                $('#newVotingModal').modal('hide');
                window.location.replace(`Edit?id=${mVotingId}`);
            },
            error: function (data) {
                showErrorMessage("Beim Speichern der Abstimmung ist ein Fehler aufgetreten!", data);
            }
        });
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

        $(function () {
            $('[data-toggle="tooltip"]').tooltip();
            $('[data-toggle="dropdown"]').dropdown();
        });

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

        $('#fw-btn-edit-voting').click(editVoting);

        $('#fwBtnCreateVoting').click(updateVoting);

        $('.form-group.date').datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "de",
            orientation: "top left"
        });

        $('#fw-voting-img-input').change(function () { previewAndUploadFile('voting'); });
        $('#fw-voting-img-content').hover(
            function () { highlightImgSelector('voting', 'rgba(101, 127, 140, 0.5)'); },
            function () { resetImgSelector('voting', 'rgba(101, 127, 140, 0.5)'); }
        );
    }

    return {
        init: init,
        setGrantedRegistrationCount: setGrantedRegistrationCount,
        setQuestions: setQuestions,
        setChartType: setChartType,
        clearPopovers: clearPopovers
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
    let mRegistrations;

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

        mRegistrations = registrations;
        for (let i = 0; i < registrations.length; i++) {
            const registration = registrations[i];
            const item =
                `<div class="d-flex mx-3 my-0 border-bottom openRegistrationItem" id="openreg-${registration.registrationId}"><div style="flex:1;margin-right:1rem">${registration.voterName}</div>\n` +
                `<button tabindex="${3 * i}" type="button" class="btn fw-registration-item-button fwBtnGrantRegistration" data-registration-id="${registration.registrationId}"><i class="material-icons">check_circle_outline</i></button>\n` +
                `<button tabindex="${1 + 3 * i}" type="button" class="btn fw-registration-item-button fwBtnDenyRegistration" data-registration-id="${registration.registrationId}"><i class="material-icons mx-1">block</i></button>\n` +
                `<a tabindex="${2 + 3 * i}" class="btn fw-registration-item-button fwBtnShowRegistrationDetails" data-toggle="popover" data-trigger="focus" data-registration-id="${registration.registrationId}" title="${registration.voterName}" data-content="<strong>Bürgerkarten-Id: </strong>${registration.voterIdentity}<br><strong>Registrierungsdatum: </strong>${formatDateTimeSeconds(registration.date)}"><i class="material-icons">info</i></a></div>`;

            $(item).insertBefore('#openRegistrationsDivider');
        }

        $('#openRegistrationsBadge').text(registrations.length);
        $('.fwBtnShowRegistrationDetails').popover({ html: true });
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
            },
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
            }
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

        $.ajax({
            url: "../Registration/GrantRegistration",
            data: { 'registrationIds': regIds, 'utcOffsetMinutes': new Date().getTimezoneOffset(), 'timezoneName': Intl.DateTimeFormat().resolvedOptions().timeZone },
            type: "POST",
            success: function (data) {
                updateRegistrations();
            },
            error: function (data) {
                showErrorMessage("Beim Bearbeiten der Regisitrierungen ist ein Fehler aufgetreten!", data);
            }
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
            type: 'POST',
            data: { "registrationIds": regIds },
            success: function (data) {
                updateRegistrations();
            },
            error: function (data) {
                showErrorMessage("Beim Bearbeiten der Regisitrierungen ist ein Fehler aufgetreten!", data);
            }
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
            type: 'POST',
            data: { "votingId": mVotingId, "addresses": recipients },
            success: function (data) {
                $('#inviteVotersModal').modal('hide');
            },
            error: function (data) {
                showErrorMessage("Beim Verschicken der Einladungen ist ein Fehler aufgetreten!", data);
            }
        });
    }

    var showInviteModal = function () {
        Edit.clearPopovers();
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
