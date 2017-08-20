# Simple Signature Demo

The purpose of this demo is to show in a very minimal example how to use the API provided
by a-trust for digital signatures using a mobile phone as your Bürgerkarte. Some information
is also provided by A-Trust in their A-Trust labs at 
https://labs.a-trust.at/developer/Default.aspx .

If you want to run the demo yourself, all you need is a web server accessible from the internet
(local web server is not sufficient!) and PHP (5 or newer). Just open the index.php of this demo in any web browser.

## A-Trust API

In order to sign data, you need to send a HTTP GET- or POST-request to 
https://www.a-trust.at/mobile/https-security-layer-request/default.aspx
In addition to the data you want to sign, you also need to enter your mobile
phone number, password and TAN (or scan the QR-code generated after entering
the mobile phone number and password). 

It is mandatory to use a web site provided by a-trust for entering the mobile
phone number and password. This is mandatory so the mobile phone number and password
are not visible for a third party (only the user and a-trust).

In the demo application, the a-trust web site was
included using an iframe (with id "signatureFrame"). The data that is supposed
to be signed is sent using HTTP GET, it is simply a hard-coded URL-parameter.

## Signature process

After entering your mobile phone number and your credentials, the credentials are
verified by a-trust and a-trust will sign your data. The a-trust application will then send 
the signed data to the URL that was provided to a-trust in the signature request as the "DataURL"
parameter. In the case of this demo, the signed data will be sent to signaturereceiver.php on your
web server. When running the demo, this should simply lead to your web browser downloading the 
signed data.

Just have a look at the code, it should be pretty self-explanatory. If you have any questions,
do no hesitate to contact me [michivo@gmail.com](mailto:michivo@gmail.com).