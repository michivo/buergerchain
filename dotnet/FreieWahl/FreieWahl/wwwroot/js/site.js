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

function getCurrentUser() {
    return currentUser;
}

function initApp() {
    firebase.auth().onAuthStateChanged(function (user) {
        if (user) {

            // User is signed in.
            var displayName = user.displayName;
            user.getIdToken().then(function (accessToken) {
                //document.getElementById('sign-in-status').textContent = 'Signed in as ' + displayName;
            });
            currentUser = user;
        } else {
            // User is signed out.
            //document.getElementById('sign-in-status').textContent = 'Signed out';
            currentUser = null;
        }
    }, function (error) {
        currentUser = null;
        console.log(error);
    });
};
