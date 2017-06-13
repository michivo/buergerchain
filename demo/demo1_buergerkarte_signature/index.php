<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>

<head>
  <title>B&uuml;rgerkarte Signature Test</title>
  <script src="jquery.js"></script>
  <script src="helpers.js"></script>
  <script>
    var eventMethod = window.addEventListener ? "addEventListener" : "attachEvent";
    var eventer = window[eventMethod];
    var messageEvent = eventMethod == "attachEvent" ? "onmessage" : "message";

    // Listen to message from child window
    eventer(messageEvent, function (e) {
      var key = e.message ? "message" : "data";
      var data = e[key];
      var decodedData = window.atob(data);
      var xml = $(decodedData).find('sl\\:CMSSignature').text();
      
      document.getElementById("content").innerHTML = "A download with your CMS-signed file should have started. You can verify the signature \<a href='https://www.signatur.rtr.at/en/vd/Pruefung.html'>here\</a>";
      var blob = b64toBlob(xml, 'application/octet-binary');
      download(blob, "message.cms");
    }, false);
  </script>
</head>

<body>
  <div id="content">  
    <?php
      $actual_link = (isset($_SERVER['HTTPS']) ? "https" : "http") . "://$_SERVER[HTTP_HOST]$_SERVER[REQUEST_URI]";
      $slash_idx = strrpos($actual_link, '/');
      $actual_link = substr($actual_link, 0, $slash_idx) . '/signaturereceiver.php';
      $actual_link = urlencode($actual_link);
      echo '<iframe id="signatureFrame" width="250px" height="250px" src="https://www.a-trust.at/mobile/https-security-layer-request/default.aspx?XMLRequest=%3C%3Fxml%20version%3D\'1.0\'%20encoding%3D\'UTF-8\'%3F%3E%0A%3Csl%3ACreateCMSSignatureRequest%20%20%20xmlns%3Asl%3D\'http%3A%2F%2Fwww.buergerkarte.at%2Fnamespaces%2Fsecuritylayer%2F1.2%23\'%20Structure%3D\'enveloping\'%3E%0A%3Csl%3AKeyboxIdentifier%3ESecureSignatureKeypair%3C%2Fsl%3AKeyboxIdentifier%3E%0A%3Csl%3ADataObject%3E%0A%3Csl%3AMetaInfo%3E%0A%3Csl%3AMimeType%3Etext%2Fplain%3C%2Fsl%3AMimeType%3E%0A%3C%2Fsl%3AMetaInfo%3E%0A%3Csl%3AContent%3E%0A%3Csl%3ABase64Content%3ESWNoIGJpbiBlaW4gZWluZmFjaGVyIFRleHQu%3C%2Fsl%3ABase64Content%3E%0A%3C%2Fsl%3AContent%3E%0A%3C%2Fsl%3ADataObject%3E%0A%3C%2Fsl%3ACreateCMSSignatureRequest%3E&amp;appletwidth=250&amp;appletheight=250&amp;backgroundcolor=white&amp;DataURL=' . $actual_link .'" scrolling="no"></iframe>';
   ?>
  </div>
</body>