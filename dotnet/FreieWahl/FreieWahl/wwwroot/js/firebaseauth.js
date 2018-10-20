﻿function setupLoginUi() {
    initApp();

    var uiConfig = {
        signInOptions: [
            // Leave the lines as is for the providers you want to offer your users.
            firebase.auth.EmailAuthProvider.PROVIDER_ID
        ],
        credentialHelper: firebaseui.auth.CredentialHelper.NONE,
        // Terms of service url.
        tosUrl: 'TermsOfUse',
        // Privacy policy url.
        // privacyPolicyUrl: 'PrivacyPolicy',
        callbacks: {
            signInSuccessWithAuthResult: function (authResult, redirectUrl) {
                // User successfully signed in.
                // Return type determines whether we continue the redirect automatically
                // or whether we leave that to developer to handle.
                const loggedInUser = authResult.user;
                loggedInUser.getIdToken(true).then(function (idToken) {
                    $.ajax({
                        url: 'Home/SessionLogin',
                        data: {
                            "idToken": idToken,
                            "csrfToken": 'TODO'
                        },
                        type: 'POST',
                        datatype: 'json',
                        error: function (err) { // TODO
                        },
                        success: function (data) {
                            window.location.replace("VotingAdministration/Overview");
                        }
                    });
                });
                return false;
            }
        }
    };

    // Initialize the FirebaseUI Widget using Firebase.
    var ui = new firebaseui.auth.AuthUI(firebase.auth());
    // The start method will wait until the DOM is loaded.
    ui.start('#fw-firebase-auth-container', uiConfig);
}

// Initialize Firebase
function initApp() {
    if (firebase.apps.length !== 0)
        return;

    const config = {
        apiKey: "AIzaSyDM8e5m6ToGxu5MZPpUeEYOkypSY1_j0PY",
        authDomain: "stunning-lambda-162919.firebaseapp.com",
        databaseURL: "https://stunning-lambda-162919.firebaseio.com",
        storageBucket: "stunning-lambda-162919.appspot.com",
        messagingSenderId: "576087239560",
        projectId: "stunning-lambda-162919"
    };
    firebase.initializeApp(config);
    firebase.auth().setPersistence(firebase.auth.Auth.Persistence.NONE);
};

function logout() {
    firebase.auth().signOut().then(function () {
        onLogout();
    }, function (error) {
        onLogout();
    });
}

function onLogout() {
    document.cookie = 'session=;path=/;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    let currentLocation = trimSlashes(window.location.href.split(/[?#]/)[0]);
    let homeLocation = trimSlashes(window.location.origin);
    if (currentLocation === homeLocation)
        return;
    window.location.href = homeLocation;
}

function trimSlashes(x) {
    return x.replace(/\/+$/g, '');
}



function setupIndex() {
    $(document).ready(function () {
        $('body').addClass('pt-0');
        $('#functions-slick').slick({
            dots: true,
            autoplay: true,
            infinite: false,
            speed: 300,
            slidesToShow: 3,
            slidesToScroll: 2,
            arrows: true,
            responsive: [
                {
                    breakpoint: 1024,
                    settings: {
                        slidesToShow: 2,
                        slidesToScroll: 2,
                        infinite: true,
                        dots: true
                    }
                },
                {
                    breakpoint: 480,
                    settings: {
                        slidesToShow: 1,
                        slidesToScroll: 1
                    }
                }

            ]
        });
    });
}