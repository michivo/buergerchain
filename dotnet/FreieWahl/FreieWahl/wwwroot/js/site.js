﻿// Initialize Firebase
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

function logout() {
    firebase.auth().signOut().then(function () {
        onLogout();
    }, function (error) {
        onLogout();
    });
}

function onLogout() {
    document.cookie = 'token=;path=/;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    window.location.href = window.location.origin;
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

function editQuestion(votingId, questionNumber) {
    alert('not implemented yet');
}