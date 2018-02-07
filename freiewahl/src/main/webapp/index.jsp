<%@ page language="java" contentType="text/html; charset=UTF-8"
    pageEncoding="UTF-8"%>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
  <head>
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <title>Hello App Engine</title>
    <%@ include file="header.jsp"  %>
  </head>

  <body onload=configureFirebaseLoginWidget()>
    <h1>Hello App Engine!</h1>
      <div id="firebaseui-auth-container">&nbsp;</div>
  </body>
</html>