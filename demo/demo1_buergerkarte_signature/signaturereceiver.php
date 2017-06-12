<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
  <head>
    <script>
<?php
  $response = base64_encode("---");
  if(isset($_POST["XMLResponse"])) {
    $response = base64_encode($_POST["XMLResponse"]);
  }

  echo "    var encodedData = '" . $response . "';\r\n";
?>      
      function postResponse() {
        <?php 
        $host = explode(":", $_SERVER['HTTP_HOST']);
        echo "var domain = 'http://" . $_SERVER['HTTP_HOST'] . "';\r\n";
        ?>
        parent.postMessage(encodedData, domain);
      }
    </script>
  </head>
  <body onLoad="postResponse()">
  &nbsp;
  </body>
</body>
</html>
