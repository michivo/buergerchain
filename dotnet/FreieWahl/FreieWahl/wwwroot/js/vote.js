var Vote = (function () {
    // private variables
    let mVoterId;
    let mVotingId;
    let mGetSignedTokenUrl;
    let mGetTokensUrl;
    let mQuestionIndices;
    let mPassword;
    let mTokenData;


    // private methods
    var updateSorting = function(selector) {
        $(selector + ' .option-numbering').detach();
        $(selector + ' .option-spacer').detach();
        $(selector + ' div.fw-answer-title').each(function(index, val) {
            $(val).prepend(
                `<div class="bg-primary text-white text-center my-2 mr-2 option-numbering rounded-circle float-left" style="height:2rem;width:2rem;font-size:1.25rem">${
                index + 1}</div>`);
            $(val).append('<div class="option-spacer" style="clear: both;"></div>');
        });
    };

    var setupSelectableOptionsList = function(questionIdx) {
        var optionsGroup = document.getElementById(`options-${questionIdx}`);
        Sortable.create(optionsGroup,
            {
                group: `options-group-${questionIdx}`,
                onStart: function(evt) {
                    $(`#info-options-${questionIdx}`).hide();
                },
                onAdd: function(evt) {
                    $(`#options-${questionIdx} div.option-numbering`).detach();
                    $(`#options-${questionIdx} .option-spacer`).detach();
                }
            });
    };

    var setupSelectedOptionsList = function(questionIdx) {
        var selectedOptions = document.getElementById(`selected-options-${questionIdx}`);
        Sortable.create(selectedOptions,
            {
                group: `options-group-${questionIdx}`,
                onRemove: function() {
                    if ($(`#selected-options-${questionIdx} div.fw-answer-title`).length === 0) {
                        $(`#info-options-${questionIdx}`).show();
                    }
                    if ($(`#selected-options-${questionIdx} div.fw-answer-title`).length <= 3) {
                        $(`#selected-options-${questionIdx}`).parent().css("border", "2px solid #C0C0C0");
                        $(`#alert-question-${questionIdx}`).addClass("d-none");
                        $(`#submit-question-${questionIdx}`).removeClass("d-none");
                    }
                },
                onAdd: function() {
                    if ($(`#selected-options-${questionIdx} div.fw-answer-title`).length > 3) {
                        $(`#selected-options-${questionIdx}`).parent().css("border", "3px solid red");
                        $(`#alert-question-${questionIdx}`).removeClass("d-none");
                        $(`#submit-question-${questionIdx}`).addClass("d-none");
                    }
                },
                onSort: function() {
                    updateSorting(`#selected-options-${questionIdx}`);
                }
            });
    };

    var setupOrderingQuestion = function(questionIdx) {
        setupSelectedOptionsList(questionIdx);
        setupSelectableOptionsList(questionIdx);
    };

    var setupQuestions = function() {
        $('.ordering-question-list').each(function(index, elem) {
            const id = $(elem).attr('id');
            const startIdx = id.indexOf('-');
            const endIdx = id.lastIndexOf('-');
            const questionidx = id.substr(startIdx + 1, endIdx - startIdx - 1);
            setupOrderingQuestion(questionidx);
        });
    };

    var loadQuestions = function(data) {
        const tokens = data.tokens.map(x => x.token);
        $('#passwordEntry').hide();
        $('#votingQuestions').load("GetQuestions",
            {
                'tokens': tokens,
                'votingId': mVotingId,
                'voterId': mVoterId
            },
            function() {
                setupQuestions();
            });
    };

    var updateQuestion = function (index) {
        $('#answered-questions-header').removeClass('d-none');
        const token = mTokenData.filter(x => x.tokenIndex === index);

        $.ajax({
            url: "GetQuestion",
            data: {
                'voterId': mVoterId,
                'votingId': mVotingId,
                'questionIndex': index,
                'token': token[0].token
            },
            type: 'POST',
            datatype: 'json',
            success: function (data) {
                $(`#question-card-${index}`).replaceWith(data);
            },
            error: function (x) {
                alert(JSON.stringify(x));
                // error: todo
            }
        });
    };

    var showErrorForVote = function (message, index) {
        const elementId = `#question-error-${index}`;
        $(elementId + ' > span').text(message.responseText);
        $(elementId).removeClass('d-none');
        $(elementId).removeClass('hide');
        $(elementId).addClass('show');
        $(`#submit-question-${index}`).hide();
        $(`#abstain-question-${index}`).hide();
        $(elementId).on('closed.bs.alert',
            function() {
                $(`#submit-question-${index}`).removeClass("disabled");
                $(`#abstain-question-${index}`).removeClass("disabled");
                $(`#submit-question-${index}`).text("Stimme abgeben");
                $(`#abstain-question-${index}`).text("Enthalten");
                $(`#submit-question-${index}`).show();
                $(`#abstain-question-${index}`).show();
                $(elementId).addClass('d-none');
            });
    }

    var submitVoteWithToken = function (index, answerIds, token, signedToken) {
        $.ajax({
            url: "SubmitVote",
            data: {
                'voterId': mVoterId,
                'votingId': mVotingId,
                'questionIndex': index,
                'answerIds': answerIds,
                'token': token,
                'signedToken': signedToken
            },
            type: "POST",
            datatype: "json",
            success: function () {
                updateQuestion(index);
            },
            error: function (x) {
                showErrorForVote(x, index);
            }
        });
    };

    var submitVote = function(index, answerIds) {
        $.ajax({
            url: mGetSignedTokenUrl,
            data: { 'voterId': mVoterId, 'password': mPassword, 'questionIndex': index },
            type: "POST",
            datatype: "json",
            success: function(data) {
                submitVoteWithToken(index, answerIds, data.token, data.unblindedToken);
            },
            error: function(x) {
                showErrorForVote(x, index);
            }
        });
    };

    var getAnswerIdFromOptionId = function(id) {
        const startIdx = id.indexOf('-', 8);
        return id.substr(startIdx + 1);
    };


    // public methods
    var getTokens = function() {
        const pwd = $('#voterPassword').val();
        mPassword = pwd;
        
        $.ajax({
            url: mGetTokensUrl,
            data: { "voterId": mVoterId, "password": pwd, "questionIndices": mQuestionIndices },
            type: 'POST',
            datatype: 'json',
            success: function(data) {
                mTokenData = data.tokens;
                loadQuestions(data);
            },
            error: function (x) {
                if (x.status === 401) {
                    $('#alert-wrong-password > span').text('Falsches Passwort!');
                } else {
                    $('#alert-wrong-password > span').text('Unerwarteter Fehler: ' + x.responseText);
                }
                $('#alert-wrong-password').removeClass('d-none');
                $('#alert-wrong-password').removeClass('hide');
                $('#alert-wrong-password').addClass('show');
            }
        });
    };

    function submitDecisionQuestionVote(index) {
        $(`#submit-question-${index}`).addClass("disabled");
        $(`#abstain-question-${index}`).addClass("disabled");
        $(`#submit-question-${index}`).text("Übertrage...");
        const answerIds = [];
        $(`#answer-list-${index} :checked`).each(function () {
            answerIds.push($(this).val());
        });

        const indexVal = parseInt(index);
        submitVote(indexVal, answerIds);
    };

    function submitMultipleChoiceQuestionVote(index) {
        $(`#submit-question-${index}`).addClass("disabled");
        $(`#abstain-question-${index}`).addClass("disabled");
        $(`#submit-question-${index}`).text("Übertrage...");
        const answerIds = [];
        $(`#answer-list-${index} :checked`).each(function () {
            answerIds.push($(this).val());
        });

        const indexVal = parseInt(index);
        submitVote(indexVal, answerIds);
    };

    var submitOrderingQuestionVote = function(index) {
        $(`#submit-question-${index}`).addClass("disabled");
        $(`#abstain-question-${index}`).addClass("disabled");
        $(`#submit-question-${index}`).text("Übertrage...");
        const selector = `#selected-options-${index} div.fw-answer-title`;
        const selectedItems = $(selector);
        const answerIds = [];
        $(selectedItems).each(function(idx, item) {
            answerIds.push(getAnswerIdFromOptionId(item.id));
        });

        const indexVal = parseInt(index);
        submitVote(indexVal, answerIds);
    };

    var abstainQuestionVote = function(index) {
        $(`#submit-question-${index}`).addClass("disabled");
        $(`#abstain-question-${index}`).addClass("disabled");
        $(`#abstain-question-${index}`).text("Übertrage...");
        const indexVal = parseInt(index);
        const answerIds = [];

        submitVote(indexVal, answerIds);
    };

    var init = function(votingId, voterId, getSignedTokenUrl, getAllTokensUrl, questionIndices) {
        mVoterId = voterId;
        mVotingId = votingId;
        mGetTokensUrl = getAllTokensUrl;
        mGetSignedTokenUrl = getSignedTokenUrl;
        mQuestionIndices = questionIndices;

        $('.alert button.close').on('click', function () {
            $(this).parent().addClass('d-none');
            $(this).parent().removeClass('show');
        });
    };

    return {
        init: init,
        getTokens: getTokens,
        submitDecisionQuestionVote: submitDecisionQuestionVote,
        submitOrderingQuestionVote: submitOrderingQuestionVote,
        submitMultipleChoiceQuestionVote: submitMultipleChoiceQuestionVote,
        abstainQuestionVote: abstainQuestionVote
    };
})();
