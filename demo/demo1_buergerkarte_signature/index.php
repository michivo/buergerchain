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
      $urlParams = array(
        'XMLRequest' => "<?xml version='1.0' encoding='UTF-8'?>\n<sl:CreateCMSSignatureRequest xmlns:sl='http://www.buergerkarte.at/namespaces/securitylayer/1.2#' Structure='enveloping'>\n<sl:KeyboxIdentifier>SecureSignatureKeypair</sl:KeyboxIdentifier>\n<sl:DataObject>\n<sl:MetaInfo>\n<sl:MimeType>text/plain</sl:MimeType>\n</sl:MetaInfo>\n<sl:Content>\n<sl:Base64Content>SWNoIGJpbiBlaW4gZWluZmFjaGVyIFRleHQu</sl:Base64Content>\n</sl:Content>\n</sl:DataObject>\n</sl:CreateCMSSignatureRequest>",
        'appletwidth' => '250',
        'appletheight' => '250',
        'backgroundcolor' => 'white',
        'DataURL' => 'http://www.google.at'
      );
      
      echo '<iframe id="signatureFrame" width="250px" height="250px" src="https://www.a-trust.at/mobile/https-security-layer-request/default.aspx?' . http_build_query($urlParams,'', '&amp;') . '" scrolling="no"></iframe>';
   ?>
  </div>
</body>