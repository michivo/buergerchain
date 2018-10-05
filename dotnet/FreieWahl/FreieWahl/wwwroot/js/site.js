// Initialize Firebase
var config = {
    apiKey: "AIzaSyDM8e5m6ToGxu5MZPpUeEYOkypSY1_j0PY",
    authDomain: "stunning-lambda-162919.firebaseapp.com",
    databaseURL: "https://stunning-lambda-162919.firebaseio.com",
    storageBucket: "stunning-lambda-162919.appspot.com",
    messagingSenderId: "576087239560",
    projectId: "stunning-lambda-162919"
};

var app = firebase.apps.length === 0 ? firebase.initializeApp(config) : firebase.app();
var currentUser = null;
var idToken = null;
var isInitialized = false;

function getCurrentUser() {
    return currentUser;
}

function initApp() {
    if (isInitialized)
        return;

    firebase.auth().onAuthStateChanged(function (user) {
        if (user) {
            // User is signed in.
            user.getIdToken().then(function (accessToken) {
                idToken = accessToken;
            });
            currentUser = user;
        } else {
            logout();
            idToken = null;
            currentUser = null;
        }
        isInitialized = true;

    }, function (error) {
        currentUser = null;
        console.log(error);
    });
};


function isEmpty(str) {
    return (!str || 0 === str.length || str === '0');
}

function logout() {
    firebase.auth().signOut().then(function () {
        onLogout();
    }, function (error) {
        onLogout();
    });
}

function onLogout() {
    document.cookie = 'token=;path=/;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    var currentLocation = trimSlashes(window.location.href.split(/[?#]/)[0]);
    var homeLocation = trimSlashes(window.location.origin);
    if (currentLocation === homeLocation)
        return;
    window.location.href = homeLocation;
}

function trimSlashes(x) {
    return x.replace(/\/+$/g, '');
}

function deleteQuestion(votingId, questionNumber) {
    $.ajax({
        url: 'DeleteVotingQuestion',
        data: { "id": votingId, "qid": questionNumber },
        type: 'POST',
        datatype: 'json',
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", idToken);
        },
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

    $("#answersTable .answerOption").not('.hide').each(function() {
        $(this).detach();
    });


    $.each(question.answerOptions,
        function(idx, answer) {
            var $clone = $('#answersTable').find('tr.hide').clone(true).removeClass('hide');
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
    var description = $('#newQuestionDescription').val();
    var title = $('#newQuestionTitle').val();
    var type = parseInt($('#questionType').val());
    var currentUser = firebase.auth().currentUser;
    var answers = [];
    var answerDescriptions = [];
    $("#answersTable .answerOption").not('.hide').each(function () {
        answers.push($(this).find('td.answerOptionText').text());
        answerDescriptions.push($(this).find('td.answerOptionDescription').text());
    });
    var minNumAnswers = parseInt($('#minNoAnswers').val());
    var maxNumAnswers = parseInt($('#maxNoAnswers').val());

    if (currentUser) {
        currentUser.getIdToken(true).then(function (idToken) {
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
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", idToken);
                },
                success: function (data) { // todo
                    $('#newQuestionModal').modal('hide');
                }
                // error: todo
            });
        });
    }
}

function grantRegistration(regId, votingId) {
    var currentUser = firebase.auth().currentUser;
    if (currentUser) {
        currentUser.getIdToken(true).then(function (idToken) {
            $.post({
                url: '../Registration/GrantRegistration',
                data: { "rid": regId },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", idToken);
                },
                success: function (data) { // todo
                    updateRegistrations(votingId);
                }
                // error: todo
            });
        });
    }
}

function denyRegistration(regId, votingId) {
    var currentUser = firebase.auth().currentUser;
    if (currentUser) {
        currentUser.getIdToken(true).then(function (idToken) {
            $.post({
                url: '../Registration/DenyRegistration',
                data: { "rid": regId },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", idToken);
                },
                success: function (data) { // todo
                    updateRegistrations(votingId);
                }
                // error: todo
            });
        });
    }
}

function showCompletedRegistrations(registrations) {
    var grantedList = $("#grantedRegistrationsList");
    grantedList.empty();
    var deniedList = $("#deniedRegistrationsList");
    deniedList.empty();
    var grantedCount = 0;
    var deniedCount = 0;
    for (var i = 0; i < registrations.length; i++) {
        var registration = registrations[i];
        var item = '<div style="margin:0 1rem;border-bottom:1px solid #DCE4E7">' +
            registration.voterName +
            '</div>';
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
        type: 'GET',
        datatype: 'json',
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", idToken);
        },
        success: function (data) {
            showRegistrations(data, votingId);
        } // TODO error
    });
    $.ajax({
        url: '../Registration/GetCompletedRegistrations',
        data: { "votingId": votingId },
        type: 'GET',
        datatype: 'json',
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", idToken);
        },
        success: function (data) {
            showCompletedRegistrations(data);
        } // TODO error
    });
}

function showRegistrations(registrations, votingId) {
    $("#openRegistrationsList").removeClass('openRegistrationItem');
    for (var i = 0; i < registrations.length; i++) {
        var registration = registrations[i];
        var item =
            '<div style="display: flex; margin: 0 1rem;border-bottom: 1px solid #DCE4E7;" class="openRegistrationItem"><div style="flex:1;margin-right:1rem">' +
            registration.voterName +
            '</div>';
        item += '<div style="align-self: flex-end;cursor:pointer;color:#657f8C" onclick="grantRegistration(\'' +
            registration.registrationId +
            '\', \'' + votingId + '\')"><i class="material-icons">add_circle_outline</i></div>';
        item += '<div style="align-self: flex-end;cursor:pointer;color:#657f8C" onclick="denyRegistration(\'' +
            registration.registrationId +
            '\', \'' + votingId + '\')"><i class="material-icons mx-1">remove_circle_outline</i></div>';
        item +=
            '<div style="align-self: flex-end;cursor:pointer;color:#657f8C"><i class="material-icons">info</i></div></div>'; // TODO
        $(item).insertBefore('#openRegistrationsDivider');
    }

    $('#openRegistrationsBadge').text(registrations.length);
}


function invite(votingId) {
    var currentUser = firebase.auth().currentUser;
    if (currentUser) {
        var mails = ["michfasch@gmx.at"]
        currentUser.getIdToken(true).then(function (idToken) {
            $.post({
                url: 'SendInvitationMail',
                data: { "votingId": votingId, "addresses": mails },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", idToken);
                },
                success: function (data) { }
                // error: todo
            });
        });
    }
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
        var $clone = $('#answersTable').find('tr.hide').clone(true).removeClass('hide table-line');
        $clone.addClass('answer-line');
        $('#answersTable').find('table').append($clone);
    });

    $('.table-remove').click(function () {
        $(this).parents('tr.answer-line').detach();
    });

    $('.table-up').click(function () {
        var $row = $(this).parents('tr');
        if ($row.index() === 1) return; // Don't go above the header
        $row.prev().before($row.get(0));
    });

    $('.table-down').click(function () {
        var $row = $(this).parents('tr');
        $row.next().after($row.get(0));
    });
}


function blinkOnScroll() {
    var winHeightPadded = $(window).height() * 1.1;
    var scrolled = $(window).scrollTop();
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
        var $this = $(this),
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