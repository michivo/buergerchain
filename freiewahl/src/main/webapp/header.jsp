<%@ page language="java" contentType="text/html; charset=UTF-8"
    pageEncoding="UTF-8"%>

	<script src="https://www.gstatic.com/firebasejs/4.9.1/firebase.js"></script>
	<script src="https://www.gstatic.com/firebasejs/4.9.1/firebase-auth.js"></script>
	<script src="https://cdn.firebase.com/libs/firebaseui/2.6.0/firebaseui.js"></script>
	<link type="text/css" rel="stylesheet" href="https://cdn.firebase.com/libs/firebaseui/2.6.0/firebaseui.css" />
	<script>
  	// Initialize Firebase
  	var config = {
	    apiKey: "AIzaSyDM8e5m6ToGxu5MZPpUeEYOkypSY1_j0PY",
    	authDomain: "stunning-lambda-162919.firebaseapp.com",
    	databaseURL: "https://stunning-lambda-162919.firebaseio.com",
    	storageBucket: "stunning-lambda-162919.appspot.com",
    	messagingSenderId: "576087239560",
  	};
  	firebase.initializeApp(config);
  	
 // Firebase log-in widget
  	function configureFirebaseLoginWidget() {
  	  var uiConfig = {
  	    'signInOptions': [
  	    	{
  	      		provider: firebase.auth.EmailAuthProvider.PROVIDER_ID,
  	      		requireDisplayName: true
  	    	}
  	    ],
  	  	'credentialHelper': firebaseui.auth.CredentialHelper.NONE,
  	    // Terms of service url
  	    'tosUrl': 'http://www.orf.at',
  	  };

  	  var ui = new firebaseui.auth.AuthUI(firebase.auth());
  	  ui.start('#firebaseui-auth-container', uiConfig);
  	}  	
</script>    