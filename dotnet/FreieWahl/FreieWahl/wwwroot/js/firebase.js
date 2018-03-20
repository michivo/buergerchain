// Initialize Firebase
var config = {
	apiKey: "AIzaSyDM8e5m6ToGxu5MZPpUeEYOkypSY1_j0PY",
	authDomain: "stunning-lambda-162919.firebaseapp.com",
	databaseURL: "https://stunning-lambda-162919.firebaseio.com",
	storageBucket: "stunning-lambda-162919.appspot.com",
    messagingSenderId: "576087239560",
    projectId: "stunning-lambda-162919"
};
var app = firebase.initializeApp(config);

// Firebase log-in widget
function configureFirebaseLoginWidget() {
	var user = firebase.auth().currentUser;
	if (user) {
		firebase.auth().signOut();
		return;
	}

	var uiConfig = {
		'provider': firebase.auth.EmailAuthProvider.PROVIDER_ID,
		'signInOptions': [
			{
				provider: firebase.auth.EmailAuthProvider.PROVIDER_ID,
				requireDisplayName: true
			}
		],
		'callbacks': {
			signInSuccess: function (currentUser, credential, redirectUrl) {
				document.getElementById('firebaseui-auth-container').style.display = 'none';
				return false;
			}
		},
		'credentialHelper': firebaseui.auth.CredentialHelper.NONE,
		'tosUrl': 'http://www.orf.at',
	};

	var ui = new firebaseui.auth.AuthUI(firebase.auth());
	ui.start('#firebaseui-auth-container', uiConfig);
}


function initApp() {
	firebase.auth().onAuthStateChanged(function (user) {
        if (user) {

			// User is signed in.
			var displayName = user.displayName;
			user.getIdToken().then(function (accessToken) {
				document.getElementById('sign-in-status').textContent = 'Signed in as ' + displayName;
				document.getElementById('sign-in').textContent = 'Sign out';

			});
		} else {
			// User is signed out.
			document.getElementById('sign-in-status').textContent = 'Signed out';
			document.getElementById('sign-in').textContent = 'Sign in';
		}
	}, function (error) {
		console.log(error);
	});
};

function getStuff() {
    var currentUser = firebase.auth().currentUser;

    if (currentUser) {
        currentUser.getIdToken(true).then(function(idToken) {
            var http = new XMLHttpRequest();
            http.onreadystatechange = function() {
                if (http.readyState == XMLHttpRequest.DONE) {
                    document.getElementById('my-stuff').textContent = http.responseText;
                }
            }

            http.open("GET", "GetStuff");
            http.setRequestHeader("Authorization", idToken);
            http.send();
        });
    }
}
